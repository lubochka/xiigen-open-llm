# N16-01 · маленький причинный эксперимент «KNN учит предметные предикаты или их подавляет длина текста?»
**2026-10-16 | SPEC + исполняемый арифметический пилот; native .NET, реальное обучение Student и blind оценка не проведены.**

## Исходная гипотеза, без запроса ещё 500 DPO-пар

В `KnnLogicalUnit.Vectorize` [строки 265–300](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L265-L300) частоты BPE-токенов и метки законов до косинуса нормализуются одной общей L2-нормой. При увеличении длины текста semantic-predicate получает всё меньшую относительную энергию и близость к **wrong-law** соседу может превысить близость к **correct-law**. Этот пробел иной, чем уже найденный metadata-`string[]` дефект, коллизия хешей, top1 или отсутствие KNN в isolated RP01 Student.

**Одно различие между плечами A/B:** нормализация тех же frozen координат.

| Плечо | Признаки / поиск | Ожидаемое различение |
| --- | --- | --- |
| **A — baseline** | Current 64D: token counts+1; predicate hashed +2; **global L2**; exact cosine | Long-text near-miss может выбрать wrong law |
| **B — enhanced** | Те же 64D+counts+predicates; **отдельный L2 каждого tier**; конкатенация с фиксированной энергией 1/2+1/2; exact cosine | Correct-law сосед не теряет силу при росте длины |
| **C — falsification** | B, но перепутаны только метки output обучающих записей; всё остальное одинаково | Улучшение должно исчезнуть |
| **D — scope control** | A на коротких запросах той же предметной структуры | Выявить зависимость от длины, не просто выигрыш на одном наборе |

Важно: **две реализации точного индекса нужны потому, что изменяются векторы**, но они строятся из *одних и тех же* ID, token sequence, predicate IDs и labels; никакой HNSW randomness, другого BPE, другой формулы расстояния, top-k voting, нового guard или нового LLM одновременно. Для отсутствующего predicate tier заранее определить однотипное поведение (zero tier; не делить на нуль) и отдельно измерять coverage.

## Этап 0 — уже возможный дешёвый арифметический контрпример, не scientific Student run

В каталоге опубликован [`16-KNN-TIER-BALANCE-PROBE.mjs`](16-KNN-TIER-BALANCE-PROBE.mjs) — Node 18+ без npm, файловых входов, веб-доступа и сторонних зависимостей.

```bash
node "llm mvp/design to code session/replanning/possibleDifs/16-KNN-TIER-BALANCE-PROBE.mjs" > knn-tier-probe-result.json
```

Данные генерируются воспроизводимо: **8 TRAIN** (четыре группы × две правовые метки `SAFE/UNSAFE`, одна из меток при одинаковых текстовых векторах намеренно distractor) и **16 HELDOUT numeric cases** (группа × две метки × два ID-смещения, сохраняющие конкретные BPE-bucket counts). Кандидат правильного закона имеет всего четыре переставленных token slots. Exact cosine, фиксированная tie-breaking последовательность. Скрипт содержит `assert` по размерам, коллизиям SAFE/UNSAFE и ожидаемому контролю; сам формирует JSON с каждым winner/score.

**Фактически выполнен опубликованный Node v22.16.0 скрипт** (SHA-256 `9750aed446cd605faff1f098fe2e4520481b8201ed2a870a8f3263826976e19f`); [сохранённый отчёт результатов](16-KNN-TIER-BALANCE-PROXY-RESULT.json): A 8/16, B 16/16, C 0/16. Это только **сконструированный witness существования риска**, где BPE IDs подаются уже готовыми. Повторение ID со сдвигом на 32 намеренно сохраняет hash bucket: эти 16 записей **НЕ independent unseen semantic families**. Поэтому даже полностью подтверждённый Node output **НЕ** доказывает, что native KNN или RP01 Student улучшились.

**Если Step0 не повторяется:** STOP, исправить перенос формулы в pilot или пересмотреть гипотезу. Не утверждать эффект по одной «зелёной» цифре без per-case winners.

## Этап 1 — настоящая небольшая native проверка качества по незнакомым входам

**Входные материалы:** CODEX с моделью Astra 6 UltraCode выбирает учебные примеры и учебную цель (роль человека) по новому доменному закону, не подгоняя заранее неизвестный экзамен. Для KNN typed training case генерация `DPO chosen/rejected` не нужна; если в следующем эксперименте создаются именно DPO-пары, **обе стороны создаёт разрешённая внешняя открытая китайская модель**, после чего CODEX с **Astra 6 UltraCode** отдельно проверяет пригодность exact пары, и ещё отдельно оценивается эффект обучения Student. Никаких имитаций внешней модели.

**Материал:** из свежих разрешённых реальных задач C# и EN↔RU сформировать, например, **12 TRAIN, 4 DEV, 16 SEALED HELDOUT**, по несколько случаев на 4 непересекающихся семейства law (негация и её область, actor/recipient, порядок условий и typed result). Выбрать распределение **до** расчёта outcomes; желательно включить SQL NULL / Angular обработку в расширенный pilot без изменения критерия текущего. **Группы HELDOUT не должны совпадать по исходному документу, `CodeContext`, шаблону, source id или семейству буквальной фразы с TRAIN**; заранее записать provenance. Не переиспользовать private RP01 exam и consumed DPO pairs.

**Один общий обученный BPE320** из TRAIN-only; freeze state SHA, tokenizer IDs, admissible DOMAIN, normalized predicate metadata **перед** строением индекса. Не обучать tokenizer отдельно для arm B. Закрепить одинаковый `VectorDimensions=64`, exact cosine, k=1 for primary, same judge rubric, одинаковые candidate labels / source IDs. На DEV можно обоснованно выбрать один заранее заявленный вес между tier, но **не подбирать его по SEALED HELDOUT**; дефолт 0.5/0.5. Изменять только нормализацию до cosine.

**Нативная проверка:** сначала локальный маленький diagnostic harness на настоящем `Vectorize`/BPE-типа коде с optional normalization strategy **без переписывания global KNN Unit**. A и B читают одинаковые raw train/test IDs; каждый создаёт свой exact-search массив. После paired результата — отдельно HNSW shadow (новый independent randomness/source of variance), затем ordinary consumer. Замерять реально возвращённый typed `Output`, а не одну близость.

### Основная таблица результата (заполнить при native run)

| Метрика | A | B | C shuffled | Комментарий |
| --- | --- | --- | --- | --- |
| `valid_correct/16` | NOT_RUN | NOT_RUN | NOT_RUN | CODEX/Astra проверяет смысл, blinded |
| `wrong_law/16` | NOT_RUN | NOT_RUN | NOT_RUN | Critical false SAFE separately |
| `abstain/16` / coverage | NOT_RUN | NOT_RUN | NOT_RUN | Нельзя приписывать рост precision только отказам |
| `paired_B_better/A_better/tie` | NOT_RUN | NOT_RUN | NOT_RUN | Каждый case, не только среднее |
| `long_text_B_gain` vs short | NOT_RUN | NOT_RUN | NOT_RUN | Проверка исходного механизма length-dilution |
| `latency_ms` / memory, source/sha | NOT_RUN | NOT_RUN | NOT_RUN | Сравнение при равных ресурсах |
| `ordinary_consumer_invoked` | UNKNOWN | UNKNOWN | UNKNOWN | Это **отдельный** арбитражный gate |

### Порог выводов и остановка

- **MECHANISM_REPRODUCED:** аналитический и native vector-score counterexample одинакового направления; ещё не learning gain.
- **HELDOUT_QUALITY_SIGNAL:** `B.correct>A.correct` на source-disjoint SEALED HELDOUT, нет новых критических wrong-law нарушений; отрицательный C не выигрывает; значения каждого случая опубликованы. 16 примеров — только pilot, статистической устойчивости на весь домен не доказывает.
- **NOT_REPRODUCED:** на сравнимом native source-disjoint наборе разницы нет или B хуже; не интегрировать/не менять старые рубрики.
- **CANNOT_ATTRIBUTE_TO_RP01:** если `RP01 CodeDocumentationSimulationDpoExperiment` не вызывает KNN в test/infer, даже положительный B — не причина free 0/3 или 0/4; положительная находка переносится в RP02/actual Unit consumer.
- **STOP при одном из нарушений:** A/B получили разный tokenizer, разные labels, hidden source leakage, outputs сформированы вместо сохранённых результатов, evaluator увидел имя arm, HNSW рандомизируется между плечами или недопущенная пара попала в Student.

## Отчёт, который обязан сохранить исполнитель

В `results/n16-01/<run-id>/`: `manifest.json` (core/planning SHA, frozen BPE SHA, IDs, law families, exact code diff, arm, tokenizer/version); `per-case.jsonl` (source hashed ID, arm, prediction, target/rubric, nearest score, law verdict, length, latency); `summary.json` (quality, coverage, paired differences, cost, confidence in claim); `review.json` (CODEX/Astra exact verdict where necessary, **не фальсифицировать model call**); `consumer-trace.json` (Unit invoked или NONE). НЕ публиковать private heldout тексты и скрытые ответы в общедоступные артефакты без разрешения.

**Owner:** RP02 representation / kNN learner, RP01 REC04 — только link на consumer и small causal proof. Не создавать новую очередь review-pass, не запускать Windows matrix; короткий runtime probe допускается без product release pipeline.

**Source relation:** [основной новый отчет 16](16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md); [существующие 44 DIF](01-PRIORITIZED-DIFFERENCES.md); [матрица→KNN ошибочные интерпретации](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md).
