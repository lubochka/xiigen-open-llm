# N32 — добавление к реестру расхождений RP01: две новые проверяемые границы и порядок принятия решений

**16 октября 2026. Research DRAFT.** Изучены XIIGEN planning `llm_plan3`, core `codex/dpo-implementation@79082340f01b744cdf08185616f2bd818dbd06b8`, существенные тела E09 в `language_translate` и `DesignToCode`; прочитаны выборочные RAG и уже подготовленные агентами исследования. **Не заявляется полного чтения 200+ возможных различий, всех parent RAG или всех веток.** Independent CODEX/Astra review и native full-product learning NOT_RUN. Единственное место новых файлов — этот planning/possibleDifs; core/docs/plans/replanning-possible-diffs оставлен без изменений.

## 1. Переиспользуем существующие доказательства, а не повторяем их как открытие

| Проверяемое отличие | Ранее установленная evidence owner | Что делать дальше / домен |
| --- | --- | --- |
| Случайная B0, LoRA q/v обновилась, free-answer не вырос; C# 0/3→0/3 и ENRU 0/4→0/4 | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [N29 master](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md) | Сначала 4–8 genuine lawful pairs + tiny overfit + blind free output; **neural**, не ошибаться с KNN |
| DPO/teacher: legal provenance ≠ независимая качество пары; simulation-контраст не external teacher | [DPO data](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [N29](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md) | Точно измерить content/recipients, исключить contamination; все teacher-dependent задачи |
| Exact DPO BPE response mask ≠ legacy hashed/truncated translation bridge | [Bridge erasure](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md), [DPO gradient](03-DPO-PAIR-AND-GRADIENT.md) | По actual caller, не переносить legacy-error на RP01 |
| LoRA/tokenizer/checkpoint и serving EOS/argmax | [LoRA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [stop](26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md), [KV](16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md), [restore](06-CHECKPOINT-RESTORE-ADOPTION.md) | Same-token full Forward vs KV cache, cold reload; до новых больших DPO runs |
| Матрица → структурная kNN: E07 неверный alignment, E09 positive but unmatched | [Matrix→KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [historical history](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md) | C# AST/code domain; source-family-matched re-evaluation |
| KNN loss of order, negation, roles/operators, weak metadata, HNSW | [N29](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md), [N21 operator admission](21-NEW-GAPS-SYMBOL-BLIND-LESSON-ADMISSION-AND-DIRECTION-MISROUTING-2026-10-16.md), [N27 negation](27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) | **НЕ для всех доменов**: AST важно C#, POS/order/tense/negation — отдельные natural-language families |
| Structured Units/Flow/Graph routing, wrong recipient and hidden deterministic fallback | [Unit/consumer](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [product bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), [N29](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md) | Только actual ordinary consumer trace отличит learned state от static lookup |
| Memory KNN score changes when new lesson added, cache alias, restore state identity | [Nonlocal rerank](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md), [State identity](22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md) | Inference quality + regression after **new** lesson; no replacement of existing results |
| Misleading training metrics & product PASS | [N23 scorer](23-INDEPENDENT-TATOEBA-QUALITY-SCORER-LEXICON-CACHE.md), [N29](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md) | Blind law correctness, negative controls, paired denominator; all domains |

## 2. Дополнительные N32 находки — не смешивать два получателя

| Новое адресное отличие | Статус | Не относится к | Быстрая первая проверка |
| --- | --- | --- | --- |
| **N32-A: один и тот же prompt с двумя разными rejected запрещён Inspect** [отчёт](32-NEW-RP01-SAME-PROMPT-CONTRAST-ADMISSION-2026-10-16.md) | **Confirmed code** для simulation import; влияние на качество **H** | Не ошибка Trainer.StepVector и не объяснение уже успешно принятых RP01 500 строк; не все production DPO | 3 native Inspect fixtures; затем tiny lawful same-prompt two-negatives vs duplicate-one-negative A/B |
| **N32-B1: E09 учит искусственные AST переходы между C# файлами** [отчёт](32-NEW-E09-CROSS-FILE-TRANSITIONS-AND-IGNORED-SOURCE-2026-10-16.md) | **Confirmed source mechanism**, synthetic proxy shows 2/8; native quality **H** | Не RP01 standalone Transformer; не ранее известные E09 local kindMap/unmatched validation | Same-files/same-split A existing transition vs B exclude across-file; matched within-file heldout |
| **N32-B2: E09 csharpSource проверяется на nonempty, но actual text берётся только files[i].Text** [отчёт](32-NEW-E09-CROSS-FILE-TRANSITIONS-AND-IGNORED-SOURCE-2026-10-16.md) | **Confirmed code**; defect only if caller promises different source owner | Не автоматическая ошибка, если files canonical | Two different nonempty csharpSource; files fixed; compare exact output metrics |

[Один исполнимый bounded протокол M32-A/M32-B](32-MICRO-ONE-FACTOR-LEARNING-CONTRAST-AND-E09-2026-10-16.md). [Выполненный proxy script](32-MICRO-ADMISSION-E09-SOURCE-PROXY.mjs) и [receipt](32-MICRO-ADMISSION-E09-SOURCE-PROXY-RESULT.json). Он подтверждает **только механизм**, ни одна модель не обучалась и целевая семантическая точность не мерилась.

## 3. Исторические успешные эксперименты — ценность и область доказательства

- **E09 Structural KNN:** одна архивная публикация выше baseline, другая ниже заранее установленного threshold, но положительна по направлению. В Tier2 опубликовано baseline 0.2413 vs kNN 0.3507; **не matched-panel causal comparison**. Сохранить как положительную гипотезу о структурных AST-features, не присваивать Student/LoRA.
- **E26 ContextualDictionary:** POS left/right context дал archived 0.40→1.00 на 15 фиксированных контекстах, строился `BuildDefault()`; это не evidence новых unseen переводов с реальным обученным unit. Релевантно natural-language disambiguation, **не universal KNN-feature**.
- **E13B:** обучение exact expression completion + осознанный abstain на unseen expressions — узкий рабочий retrieval механизм, а не генерализация. **Не стирать это как «детерминизм»**: проверять, создана ли память через train и изменяет ли ответ.
- **E14:** малый позитивный Tier2 и большой отрицательный Tier3 требуют source-family matched re-evaluation; «failed на большом» не отрицает, что определённое scoring/feature полезно в отдельном kNN consumer.
- **E51A/E51B и E52:** полезная системная архитектура и маршрутизация, но часть результативности связана с repeated context, oracle fields или deterministic pipeline. Независимый blind law judge обязателен.

Первичные источники: [historical experiment archive](02-HISTORICAL-EXPERIMENTS.md), [matrix→KNN source proof](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [N29](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md). **Найдены сохраняющиеся тела E09/E26/E13B в соседних ветках**, однако конкретная исходная миграция «failed matrix formula → successful KNN formula» по одному названию и архивным коэффициентам не установлена.

## 4. Decision order для минимального бюджета

1. **STOP large runs** до одного functional chain: prompt + chosen/rejected → admitted material → actual recipient → changed state → saved/cold restored → ordinary answer → blind law judge.
2. **Две разные диагностики:** Student DPO small causal exam (проверить B0/SFT prerequisite и learnability) отдельно от KNN/E09 microprobe (проверить structural effects, source boundaries); не переносить metrics между ними.
3. **Микропроверка N32-A Inspect**, затем 2 updates × 2 arms **только при получении разрешённых validated разных negatives**; иначе оставить в статусе mechanism-only.
4. **E09 same-panel file-boundary correction** на 6–10 файлах. Если выигрыш на top1 не переносится в consumer-law correctness, не тратить время на product integration.
5. **Human quality review** от независимого CODEX/Astra и authorised teacher provenance; ни отчёт, ни successful Node asserts не заменяют оценку. Отсутствие measured gain означает `NO_CAUSAL_LEARNING_GAIN_YET`, а не автоматически «сломанная модель».

## 5. Граница готовности

Новые hypotheses **не включены в source training implementation**, не объявлены релизными изменениями. Уже опубликованные results/Rxx, historical E09/LoRA checkpoints не переписаны. Core repo, особенно docs/plans/replanning-possible-diffs, **read-only**. Скилы архитектора, кода, планирования, проверки, prompt engineering и человеческой коммуникации учтены как требования к evidence/roles; независимые агентские tool-executions не имитируются и не считаются сделанными.
