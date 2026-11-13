# MICRO-KNN-ORDER-01 — короткий эксперимент: теряется ли качество обучения KNN из-за порядка действий?

**16.10.2026 · DRAFT_SPEC / READY_TO_IMPLEMENT / NOT_RUN / CODEX-ASTRA_REVIEW_NOT_PERFORMED.**

**Что проверяем:** в текущем `KnnLogicalUnit` частотный BPE-вектор не представляет порядок слов и ролей как независимую координату. Поэтому два запроса с одинаковым набором токенов, но противоположным порядком действий могут стать неразличимыми и получить один ответ, хотя обучались на двух разных метках. Хотим проверить не только равенство векторов, но и **изменение качества learned nearest-neighbor output после предъявления тренировочных примеров**.

**Исключено:** не проверяем DPO/LoRA, качество китайского teacher, EOS, RP01 checkpoint и достижение целевых 95%. Фактическая связь с обычным RP01 consumer пока не доказана. Это один лабораторный тест **памяти обучаемого Unit**, не переписывание текущей реализации.

## 1. Почему выбран именно этот разрыв

- [KnnLogicalUnit.Train:102–153](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L102-L153): BPE vocab320 **обучается на текстах textSelector**, HNSW индекс строится заново.
- [KnnLogicalUnit.Vectorize:265–299](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L265-L299): токены суммируются по `tokenId % tokenTierWidth`, предикаты отдельно по hash; явной координаты порядка нет.
- [KnnLogicalUnit.ExecuteAsync:216–254](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L216-L254): берёт `hits[0]` как ответ.
- [BpeTokenizer.Train/Encode](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/01.Tokenizer/BpeTokenizer.cs#L42-L82) и [RegexSplitter](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/01.Tokenizer/RegexSplitter.cs): формирование BPE зависит от входной строки и начального пробела.

### Исправление экспериментального дизайна — важно

Прежний вариант сравнивал `KnnLogicalUnit.Train(rawSelector)` с `KnnLogicalUnit.Train(augmentedSelector)`. Это **не чистая проверка признака порядка**: `Train` каждый раз вызывает `tokenizer.Train` с разными строками, значит обучаются разные merge-таблицы и затем случайные HNSW-графы. Полученное улучшение нельзя было бы уверенно приписать positional feature.

**Авторитетный первичный A/B ниже исправляет проблему:** один раз фиксирует один и тот же BPE tokenizer, один и тот же train memory и exact `BruteForceIndex` без стохастики; различается только **дополнительный универсальный позиционный BPE-признак**, не проверка правила backup/delete. Отдельный shadow-прогон настоящего `KnnLogicalUnit` нужен для инженерного переноса, но **не** участвует в главном причинном сравнении.

## 2. Фиксированные данные и область вывода

[11A-MICRO-KNN-ORDER-FIXTURE.json](11A-MICRO-KNN-ORDER-FIXTURE.json) — уже опубликованный *синтетический диагностический fixture*, **не DPO** и ещё не оценённый независимо CODEX/Astra. В нём:

- **TRAIN:** 8 зеркальных пар (16 строк) на объектах `asset101`…`asset108`.
- **HELDOUT:** 8 зеркальных пар (16 строк) на других объектах `asset201`…`asset208`. Нет exact-text совпадений с train; шаблон и закон прежние.
- Контракт лаборатории: **SAFE** = backup до delete для того же asset; **UNSAFE** = delete до backup. Никаких реальных удалений или файловых операций нет.

Пример:

```text
plan backup asset201 before delete asset201 → SAFE
plan delete asset201 before backup asset201 → UNSAFE
```

**Предельно честная граница:** это near-transfer на новый объект при известном синтетическом законе, не unseen-domain, не новая обученная нейронная способность и не доказанная причина RP01. Сначала CODEX с Astra 6 UltraCode выбирает/проверяет корректность и отсутствие утечки label в `input`, затем fixture SHA фиксируется **до** любой оценочной генерации. Пока это не сделано, статус gold = `UNREVIEWED`.

## 3. Два плеча: меняется ровно один информативный сигнал

| Инвариант | A — текущий вектор | B — тот же вектор + порядок |
| --- | --- | --- |
| train/heldout и порядок строк | те же 16/16 | те же 16/16 |
| BPE tokenizer | **один объект**, `Train(rawTrainingText,320)`, сохранённый SHA | **тот же самый объект и SHA**, НЕ отдельный BPE fit |
| Базовые координаты | точная формула нынешнего `Vectorize` в 64D, пустые predicates | **те же 64 значения** |
| Дополнительные координаты | 128 нулей | 128 bucketed **(BPE ID, relative position)** признаков |
| Общая размерность | 192 | 192 |
| KNN поисковик | `BruteForceIndex` exact cosine, k=1 | **тот же** тип, cosine, k=1 |
| Решение | метка top1 обученного example | та же метка top1; **без if-блоков по SAFE/UNSAFE** |
| Голд, оценка, threshold | идентичны | идентичны |

Для B: каждой позиции токена `i` задаётся корзина `positionBin = min(3, floor(4*i/max(1, nTokens)))`. Использовать устойчивый заранее фиксированный hash пары `(tokenId, positionBin)` в 128 слотах, складывать частоты и L2-нормировать positional block. Базовый 64D блок **не менять**; в B объединить L2-блоки с **фиксированным равным весом 1:1** и нормировать общий вектор; в A дописать 128 нулей. Не менять веса после получения результата. Никаких доменно-специфических `if (first==backup)`, `if (law==SAFE)`, `firstactionbackup` или целевых labels внутри encoder.

```text
                  ОДИН frozen tokenizer, ОДНИ train examples
                                  │
                    ┌─────────────┴─────────────┐
                    ▼                           ▼
              A: old hashed 64D          B: old 64D + ordered 128D
                 + zeros 128D             generic token-position
                    │                           │
                    ▼                           ▼
            exact KNN + learned labels    exact KNN + learned labels
                    └─────────────┬─────────────┘
                                  ▼
               ОДИН frozen blind HELDOUT H01…H16
                                  ▼
                 paired correct / wrong / false SAFE
```

**Почему это обучение, а не подставленный ответ:** encoder B фиксирует только порядок токенов. Выход SAFE/UNSAFE появляется **исключительно из записанных при обучении меток соседей**. В контроле с переставленными метками B должен менять ответы. В нулевом состоянии памяти оба индекса обязаны abstain (`NO_NEIGHBOURS`), без вшитого правила.

## 4. Минимальная реализация: один экспериментальный runner, без production edits

Предлагаемый **новый, пока не существующий путь**: `tools/Rp01MechanismProbe/` с консольным `Program.cs` и ссылками на уже существующие `01.Tokenizer`, `06.VectorSearch` и общие типы. **Не создавать второй product KNN и не менять `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`.**

1. Прочитать fixture; проверить ровно train16/test16, 8 зеркальных пар, уникальные IDs, отсутствующий `asset20x` в train, отсутствие `SAFE/UNSAFE` в текстовых полях, правильность gold. Зафиксировать SHA всех input/case IDs до прогнозов; если непроверенная метка — `INVALID_FIXTURE`.
2. Обучить `new BpeTokenizer()` **один раз только на 16 raw TRAIN**, `vocabSize=320`. Записать `ComputeStateSha256()`; никаких HELDOUT строк при fit.
3. Для A скопировать **точную исходную формулу** `KnnLogicalUnit.Vectorize` в experiment-only `LegacyVector64` (включая token hash, normalization, predicate tier) и получить неизменные 64 координаты. Для B использовать этот же output без изменений + дополнительные 128 координат как выше. Логи `legacy64_sha`, `order128_sha`.
4. **Векторный preflight до оценки:** для каждой зеркальной пары предъявить A и B векторы; у A пары должны совпадать в пределах 1e-6, у B — различаться. Если нет, это `INVALID_EXPERIMENT` / `POSITION_SIGNAL_NOT_ENCODED`; не изменять fixture или hash после ответа.
5. Создать два `new BruteForceIndex()`, добавить одни и те же **16 TRAIN** по возрастанию `ExampleId` и их метки в отдельный словарь ID→label. Для каждого HELDOUT вызвать `Search(vector,1)`; отсутствие кандидатов = abstain, одинаковый ID tie-break обеспечивается одинаковым порядком вставки. A/B используют один и тот же `top1` output rule.
6. За один проход получить все **16 heldout** прогнозов (не отбрасывать неудобные случаи), сохранить nearest `ExampleId`, cosine, raw predicted label, gold, correctness, timeout/error, wall-time, memory. Учитывать false SAFE на UNSAFE отдельно.
7. Повторить B с **теми же TRAIN input vectors**, но только с инвертированными train labels (negative control); не перестраивать embeddings. Если выход не зависит от train labels — проверить hidden rule/leak. Дополнительно пустая память должна abstain. Для exact BruteForce при фиксированной последовательности достаточно одного run.
8. **Только диагностический product-shadow:** A с настоящим `KnnLogicalUnit<string,string>.Train`/`ExecuteAsync` на этих же 16/16, зафиксировать соответствие решений его HNSW пути к A exact и различия. Поскольку HNSW RNG не фиксирован, провести до 3 одинаковых build (все результаты записать) **только если** первый shadow существенно расходится. **Не использовать B с изменённым textSelector как чистое подтверждение эффекта** — оно переобучает BPE на других текстах. Для B product интеграция станет отдельной RP02 задачей после лабораторного сигнала.

Проектируемая команда, **не существует до создания runner**:

```powershell
dotnet run --project tools/Rp01MechanismProbe -c Release -- --fixture "PATH/11A-MICRO-KNN-ORDER-FIXTURE.json" --out "PATH/result.json"
```

Достаточно локального `dotnet build` только нового runner и одного bounded run. Не запускать GitHub Windows CI/все Tests, не проводить 500 пар DPO/обучение Student.

## 5. Что измерять и заранее установленный фальсификатор

**Основной KPI:** `heldout_correct_A`, `heldout_correct_B` из **ровно 16**; `paired_A_wrong_B_correct` и `paired_A_correct_B_wrong`. Дополнительно `false_SAFE_on_UNSAFE`, `both_mirror_correct/8`, `abstain/16`, `p50/p95 latency`, managed allocation, tokenizer/vector/index hashes.

**Позитивный узкий результат `LOCAL_KNN_ORDER_LEARNING_EFFECT`** (исследовательский predeclared pilot threshold, не 95%-ное статистическое доказательство): A-вектор равен для **всех восьми** зеркальных пар, B-вектор различается для всех восьми; B ≥ 12/16 правильных и B−A ≥ 4/16; false SAFE у B не выше A; B с инвертированными train labels меняет предсказания не менее чем в 8/16 случаях; не было gold/input leakage и не было постфактум перенастройки. Назвать качество только **heldout новой entity в одном toy-law**.

**Альтернативные исходы:**
- `MECHANISM_CONFIRMED_NO_GAIN`: B различает порядок, но качество не улучшилось; это опровергает достаточность данного feature/budget на этой панели.
- `REGRESS`: B даёт меньше правильных или больше критических false SAFE; гипотеза о полезности добавления признака для этих данных не подтверждена.
- `INCONCLUSIVE`: меньше predeclared δ, нестабильные результаты или недостаточно информации.
- `INVALID_EXPERIMENT`: не совпадает tokenizer, изменился train set, gold утёк в input/encoder, A-пары неколлизионны, нет независимого подтверждения фикстуры.
- `NOT_RUN`: опубликована лишь постановка, без native evidence.

**Строгое ограничение:** положительный toy-law результат доказывает функциональный эффект информации о порядке для обученной памяти KNN в **экспериментальном exact consumer**. Он НЕ доказывает эффект на нынешнем HNSW Unit, LoRA optimizer или ordinary RP01 free answers без отдельного actual product trace и RP02 implementation/admission. Текущий RP01 может вовсе не вызывать этот Unit на своих запросах.

## 6. Evidence, review, публикация и роли

Выход в **новую** (не frozen) папку эксперимента: `intent.json` (source SHA, fixture/tokenizer hashes, fixed criteria), `vector-preflight.json` (8 зеркальных сравнений), `case-results.jsonl` (16×2 + flipped-label controls), `comparison.json` (paired numbers, critical errors, cost, status), `review.md` (independent reviewer or `NOT_PERFORMED`). **Не заменять** архивные E07/E09, RP01 C#500/ENRU500, gold и checkpoints.

Роли для любой последующей DPO работы: **CODEX с Astra 6 UltraCode выполняет роль человека по выбору уроков и отдельно проверяет качество точной DPO пары**, **внешняя разрешённая открытая китайская модель создаёт обе стороны `chosen` и `rejected`**. Здесь мы не генерируем DPO и не создаём teacher receipt. CODEX/Astra отдельно проверяет закон toy fixture и после результата делает независимый evidence-bound review; **его вызов в этой документарной сессии НЕ выполнялся**.

**Граница публикации:** experiment design + существующий fixture. Новый C# runner, реальные локальные измерения, blinded Astra check, изменение production KNN, linked ordinary RP01 consumer = `NOT_PERFORMED`. Вердикт review до выполнения — `DRAFT`. Исполнителю запрещено объявлять научный PASS по этому файлу.

## 7. Готовый наряд исполнителю (одна задача)

> **Текущая задача:** реализовать только `MICRO-KNN-ORDER-01` по этой карточке и уже опубликованному fixture, одним новым console runner; общее состояние tokenizer320, A=legacy64+zeros128, B=legacy64+generic positional128, exact cosine k=1, без label logic. **Не задача:** изменение продукта, оптимизатора DPO/LoRA, критерия RP01, большой CI. Прочитать [архитектор](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-readable-architecture-audit-SKILL.md), [планирование](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/phase-planning-SKILL.md), [проверка плана](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-review-SKILL.md), [код](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/code-examination-SKILL.md), [эксперименты](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/experiment-design-SKILL.md) и human-readability. Сначала проверь данные и frozen gold через CODEX/Astra 6 UltraCode (если недоступно — `GOLD_UNREVIEWED`, не scientific PASS), затем preflight всех 8 пары, потом один 16-case A/B и flipped-label. Сохрани с SHA inputs, trace, outputs и costs. Отдельно проверь один настоящий KnnLogicalUnit A-shadow; если он расходится с exact A, не подменяй kernel. Верни короткое русское заключение: повлияло / не повлияло / неясно; ограничь его ровно областью реально проверенного обучения KNN.
