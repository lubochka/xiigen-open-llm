
# MICRO-N34 — дешёвые различающие эксперименты: подготовка corpus → pretraining → проверка полезности, по доменам

**Дата:** 2026-10-16. **STATE: DESIGN + SOURCE-ONLY PROXY EXECUTED; NATIVE_NOT_RUN; ACTUAL LEARNING NOT_RUN.** Ожидаемый масштаб: десятки документов / единицы–десятки проб, не 500 DPO и не Windows CI. Рабочий core READ ONLY, этот файл — work order, а не разрешение переписывать существующий пакет.

## Цель и независимые переменные

Отличить данные/обучение от исправления метрики. Опыт A/B меняет **строго один** параметр за запуск: `EOF/EOD`, `window_stride`, `uint16 admission` либо `shard_boundary`. Фиксируются код модели, seed, tokenizer SHA и vocab, порядок файлов, optimizer state, число **реально обработанных уникальных токенов** и число update steps. Не обучать сразу всеми четырьмя изменениями: причинность тогда не установить.

## 0. PRE-GATE — несколько минут, без gradient steps

1. Пиновать commit `216a1f3d6af3f40ceef081dc168b07e4ce3934f9`. Прочитать `DatasetBuilder`, `DataLoader`, `BpeTokenizer`, `Trainer` и их actual callers; сохранить сборочную версию и SHA datasets.
2. Выполнить **узкий native C# probe**: vocab629 ⇒ expected EOF ID=1, EOS ID=628; DataLoader seq4, batch3 ⇒ offsets 0/1/2; `t=70000` ⇒ stored65535; два шарда с неполным хвостом ⇒ проверить отсутствие cross-shard окна. Assertions сначала на *текущем* коде. Затем маленький контраст или простой фикс; не запускать полный `dotnet test`/GitHub Windows.
3. Сохранить canonical `inputIds`, `targetIds`, source document IDs, `start/end/span`, sha shard и точные hash train/validation. Исходные исторические E09/E26/E43 и RP01 500 не менять.
4. Отдельно проверить [N33 Embedding gradient](33-MICRO-AUTOGRAD-WARMUP-AND-LEARNING-IMPACT-2026-10-16.md) и supervised LR=0. Если через выбранный native SFT/pretrain learner градиент не проходит, **STOP: DATA_PROBE_ONLY / CANNOT_CLAIM_LEARNING_RESULT**. Не записывать 0 quality gain как опровержение N34 — модель фактически не учится.

**Предварительный source-only JS probe:** `node 34-SOURCE-ONLY-DATA-PIPELINE-PROXY.mjs > 34-SOURCE-ONLY-DATA-PIPELINE-PROXY-RESULT.json`. Возвращает при seq4/batch3 offsets 0/1/2 вместо 0/4/8, 6 уникальных input IDs вместо 12 в одной маленькой панели, EOF1 != EOS628, saturated IDs 65536/70000→65535. Это **не native и не генерация модели**.

## 1. C# / code documentation — EOF/EOD и целостность файла

- **Датасет:** 12–24 маленьких исходных файла `.cs`, 3–4 синтаксические семьи: `if (x == null) throw` vs `if (x != null)`, `await`/`Task`, позиция аргументов, `Dictionary<TKey,TValue>`. Новые heldout файлы из иных source families, не изменённые только именами переменных; 6–12 задач короткого completion/explain-error.
- **A:** фактическая конкатенация file tokens с `EOF=1`.
- **B:** согласованный документированный EOD token + отдельная policy attention/loss-mask, если decoder умеет token. Если не умеет, сперва сравнивать только exact train material/target boundary; не подменять на произвольный ID.
- **Controls:** no-learning B0; shuffled file boundaries; документ с исходным `0x01` как adversarial negative. До и после — **обычный** generation consumer (не только train loss).
- **Outcome:** C# compilation/semantic law correctness и boundary errors, не exact string только; токены, loss на исходном файле и heldout, cost; число отказов/срывов. Если B выигрывает только на synthetic boundary metric, но не на кодовом heldout — `MECHANISM_ONLY`.

## 2. EN↔RU / синтаксис, отрицание, роль — stride и sample diversity

- **Dataset:** 16–32 короткие парные RU/EN предложения с independent source-family split. Семьи: отрицание (`can/cannot`), роли (`X gave Y to Z`), порядок `before/after`, род/согласование; 8–12 blind heldout примеров. Предложения оформлены отдельными текстовыми документами только если pretraining pipeline действительно их использует.
- **A:** stride=1 в `DataLoader`.
- **B:** stride=seqLen; опционально третий локальный контроль stride=floor(seqLen/2). Тот же tokenizer, corpus, seed, capacity, optimizer, schedule.
- **Fairness:** записать два результата: equal update steps (стоимость) И, отдельным прогоном, equal unique-token exposures (учебная информация). Нельзя выбрать только удобную нормировку.
- **Outcome:** правильное отрицание, direction/subject, ролевые перестановки на новых семьях; retained старые словоформы, exact free answer, coverage/abstention/latency. Если stride1 лучше на коротком corpus, **не** навязывать stride=seqLen глобально.

## 3. SQL/DSL/schema и большой vocab — token-id collision (условно)

- **Gate:** только если vocab или ingress реально содержит ID > 65535. На текущем RP01 vocab629 — `NOT_APPLICABLE`; нельзя создавать искусственный «исправленный RP01».
- **A:** native `uint16` reader/writer; **B:** reject invalid IDs или корректный `uint32` transport с явной schema/version, одинаковым для writer и reader.
- **Test law:** два разных протокольных идентификатора или AST/operator токена должны оставаться различными после write→cold read; counterexample ID65535 vs ID65536. Проверить downstream tokenizer mapping; только если meaningful coverage существует — tiny learning on heldout operator/order.

## 4. Файлы-шарды, воспроизводимость и cold restore

- **A:** native file enumeration and shard policy.
- **B:** явно упорядоченный source manifest (path, SHA, split-group), deterministic sharding + последовательная window policy **без пересечения train/heldout family**.
- Пары случаев: split одного C# файла посреди функции; два независимых файла в одном шарде; repeated identical files с разным абсолютным OS path.
- Наблюдать ordered `input/target/window_ids` и checkpoint student/corpus hash до/после cold restore, а затем heldout law correctness. **Не приписывать дрейф случайной инициализации методу выборки.**

## Оценка результатов — матрица вердиктов

| Приёмка | Условие |
|---|---|
| `CONFIRMED_DATA_SEAM` | exact native bytes/window IDs совпали с source finding |
| `QUALITY_GAIN_NATIVE` | единственное A/B различие, независимые gold и устойчивый прирост на обычном learned consumer, проверка после cold restore |
| `QUALITY_NEUTRAL` | механизм различается, но paired quality не меняется |
| `NEGATIVE_TRANSFER` | B улучшает один домен, ухудшает другой → domain gate, не universal fix |
| `NOT_APPLICABLE_TO_RP01` | frozen RP01 только DPO Student без `DatasetBuilder` |
| `BLOCKED_BY_NEURAL_LEARNABILITY` | embedding/LR/gradient gate N33 не пройден — прекращаем costly learner A/B |

**Обязательный отчёт:** source sha, compiler/runtime version, seed, exact token ids, sampled window ids, before/after observable outputs, независимый scorer rubric без after-the-fact изменения, numerator/denominator (никаких обещанных 95%), resource/time, negative controls. Внешняя/внутренняя authority и лицензирование новых учебных DPO-пар остаются по действующим правилам: CODEX/Astra выбирает/независимо оценивает cases, разрешённая внешняя китайская open model создаёт **обе** стороны `chosen/rejected`. В MICRO-N34 новых DPO пар не создано и никакой judge не вызывался.

**См.** [source evidence N34](34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md), [source-only proxy](34-SOURCE-ONLY-DATA-PIPELINE-PROXY.mjs), [raw result](34-SOURCE-ONLY-DATA-PIPELINE-PROXY-RESULT.json).
