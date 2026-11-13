# N29 — сводный реестр причин плохого обучения RP01, успешных старых механизмов и действительно новых проверок

**16.10.2026 | PLANNING-only evidence register.** Сверены текущие репозитории: `xiigen-planning/llm_plan3`, `llm_mvp_core/codex/dpo-implementation@d5c2409b24292d361593b8262b0c54997b15f539`, часть исходников `DesignToCode`; прочитаны входы RAG. **Это навигационная синтеза существующих отчётов + два уточнённых исследовательских вопроса, НЕ заявление о полном чтении всех RAG/веток, НЕ новый ML run.** Существующие отчёты других агентов сохранены. `llm_mvp_core/docs/plans/replanning-possible-diffs` **НЕ изменялся**.

## Главный вывод для решения о разработке

Нельзя складывать в одну «точность обучения» три разные машины:

1. **RP01 neural:** случайная базовая B0, DPO exact-v2 → LoRA q/v → `TextGenerator`, без доказанного переноса знаний в свободной генерации. Сохранённые итоги: C# 0/3→0/3; EN→RU 0/4→0/4; 500 пар каждого домена, 50 optimizer updates каждого этапа. Это **не общий тест исторического kNN/Flow**. [Recovery audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).
2. **Историческая память/KNN/структурные Units:** правильные пары, POS, syntax, AST, словари, адаптеры, отдельный arbiter и learned KNN могли давать функциональный результат **без** генеративного нейронного улучшения. Например E09/E34 записали прирост hit/top1, но сравнения **несопоставимы по validation subsets**; E26/E13B содержали реальные context/memory effects. [Первичные эксперименты](02-HISTORICAL-EXPERIMENTS.md), [Matrix → KNN mistaken failures](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [происхождение transfer](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md).
3. **Проверяющий и readiness gate:** тренировочная preference, `UnitAccuracy`, negative-only Verifier и продуктовый `ProductMatch` проверяют не обязательно смысл ordinary answer; ложные PASS или FAIL нельзя принимать за regression/gain. [Unit metric](28-NEW-N28-UNIT-HELDOUT-OBJECTIVE-NOT-LIVE-QUALITY-2026-10-16.md), [product meaning gate](28-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-DIVERGENCE-2026-10-16.md).

## 15 осей, где можно потерять качество: владелец доказательств и класс применимости

| Ось/механизм | Первичный отчёт или кодовый owner | Условие проверки; домен |
| --- | --- | --- |
| 01. B0 без начальной способности | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) | Проверить small chosen-only warmup vs DPO-only; C#/перевод, **P0** |
| 02. Слабые DPO contrasts, pairs/family split, provenance, roles | [DPO objective](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [historical learning](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md), [legal RAG](../../09.09.26WorkLeft-RAG-01-dpo-legal-build.md) | Raw pair ≠ usable learning signal; **P0 neural** |
| 03. LoRA q/v recipient, rank, optimizer state | [LoRA/tokenizer](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [restore](04-BASE-LORA-CHECKPOINT.md) | Получил ли целевой параметр градиент и читает ли его user inference; **P0/P1 neural** |
| 04. Exact DPO mask/token/EOS vs иной legacy word-hash bridge | [bridge token erasure](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md), [pair collapse](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md), [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) | **Не смешивать call paths**; только фактически вызванный код |
| 05. Vector/BPE/index identity; BOS, EOS и семантическая позиция | [BPE drift](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [stop tokens](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md) | Один train/serve encoder и тот же frozen index; **нейронный и retrieval**, но разными способами |
| 06. DPO→Unit conversion и ложный chosen/rejected внутри Unit | [structured](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [learning recipient](05-UNITS-FLOW-GRAPH-DETERMINISM.md) | Действительный typed Unit получил правильную метку, а не bootstrap self-label; **structured only** |
| 07. KNN identity/duplicate IDs/HNSW и смысл predicate | [identity](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md), [hybrid](04-KNN-HYBRID-RETRIEVAL.md) | Сначала найти правильных кандидатов, затем законный выбор; **kNN** |
| 08. BM25, feature fusion, top-k order и positive/negative/warning | [fusion/memory growth](20-NEW-FINDINGS-FUSION-MEMORY-GROWTH-2026-10-16.md), [misreads](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) | Сравнить matched memory и arbiter; AST/синтаксис только relevant domain |
| 09. POS/word order/grammar, negation, sentence boundary | [E26](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md), [language routing](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md), [polarity](27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) | Разделить word-order features и syntax tree; естественный язык/AST, **не всем доменам** |
| 10. Неправильный topic/domain/scope у памяти | [Unit + KNN](16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md), [positive role domain](23-MASTER-GAP-TRIAGE-AND-EXPERIMENT-PRIORITY.md) | Обучение может сменить значение одного слова в разных domain; проверять отдельно |
| 11. Сохранение/повторное открытие model, Unit, KNN, optimizer, cache | [checkpoint](06-CHECKPOINT-RESTORE-ADOPTION.md), [cache alias](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) | Cold fresh process, identical outputs + state hashes; **все обучаемые recipients** |
| 12. Реальный ordinary consumer / Flow / Graph vs построение урока | [consumer bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), [N28 feature drift](28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md) | Learned state must be called by real request, not just exported; **P0 архитектурно** |
| 13. Тестовая иллюзия: Unit heldout, quality judge, недостоверные gold | [N28 verifier](28-NEW-N28-UNIT-HELDOUT-OBJECTIVE-NOT-LIVE-QUALITY-2026-10-16.md), [N28 product](28-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-DIVERGENCE-2026-10-16.md) | Семантика, удержание, независимые семьи, отрицание/roles, расходы; **все домены** |
| **14. N29-A: rejected annotation → positive memory key** | **[новое исследование](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md)** | Уточнить отдельный источник non-reproducible feature; **Translation Unit с [RULE:]**, не direct RP01 |
| **15. N29-B: first-position pseudoalignment → lexical collision** | **[новое проверяемое следствие ранее замеченной эвристики](29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md)** | Проверить inversion + противоположный порядок уроков, source-disjoint; **RU→EN / языки со свободным word order**, не прямой LoRA |

Словарь «доказано»:
- **SOURCE-CONFIRMED** = условие действительно присутствует в опубликованном коде.
- **FROZEN-RESULT** = опубликовано измерение, оно не обязательно причинно сопоставимо.
- **CAUSAL-HYPOTHESIS** = эффект на обычный ответ не проверен.
- **NOT_RUN** = реальный Model/Unit experiment и отдельная экспертная оценка в этом раунде отсутствуют.

## Старые успешные ветки — что сопоставить до отказа от механизма

**`language_translate`**: [branch archaeology](11-BRANCH-ARCHAEOLOGY-AND-LEARNING-PROOFS.md) приводит сохранённые позитивные functional cycles, включая морфологическую память, но не приравнивает их к generalization of neural RP01. **`DesignToCode`**: [real DPO run receipts](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md) содержат настоящие `DpoTrainer.StepMany`/checkpoint и train accuracy (в одном repeat10 повторены 50 исходных уроков), однако иной tokenizer и наличие `Chosen:`/`Rejected:` markers исключают безусловный перенос метрик. В текущем и старом `DesignToCode` builder `BuildRuleTags(pair.Rejected)` присутствует — **N29-A является унаследованным риском**, не доказанной регрессией именно этого переноса.

**E09/E34**: опубликованные цифры без matched validation не являются доказательством причинного улучшения. **E14/E29V2/E31V2/E44V2/E45V2**: отрицательные результаты относятся к определённому algorithm/recipient/panel, а не к запрету любой сходной функции в другой KNN representation. [Evidence owner](02-HISTORICAL-EXPERIMENTS.md). **E26**: реально полезное contextual/POS поведение может исчезнуть при переносе в word-only Unit даже когда старый matrix experiment был записан как unsuccessful; [E26 POS audit](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md). Не объявлять success на основании одного заголовка или «100%» без leakage audit.

## Малый порядок решения задачи, не новый многонедельный цикл тестов

1. **Немедленный neural P0 (уже прописан в RP01):** 4–8 fresh lawful пары → actual exact-v2 model IDs/masks → analytic-vs-finite-difference gradient → tiny train-pair overfit/conditional decoding → *new-family* semantic comparison vs frozen B0; без `Windows GitHub Actions` и массовой 500-row CI matrix. Результат определяет, имеет ли смысл продолжать DPO-only при B0 random.
2. **Новый bounded Unit pilot:** [N29-MICRO — два изолированных A/B](29-MICRO-REJECTED-KEY-AND-WORD-ORDER-CAUSAL-EXPERIMENT-2026-10-16.md). Сначала `BuildGeneralBatch/WordMappings` и actual keys; только если L0 подтверждает reachability — 8–12 source-family-disjoint normal Flow outputs + cold checkpoint. Один factor за раз.
3. **Решение о переносе старой функции:** на одинаковой памяти и *той же* панели перенести *только* соответствующую feature/ranking/arbiter функцию AST или языкового порядка; пометить `DOMAIN_SPECIFIC` или `NOT_APPLICABLE`. Решать по semantic gains и retention, а не числу тестов или изменённых LoRA матриц.
4. **Не тратить бюджет на повторные old pair IDs.** Круг source→authorised teacher→distinct pair→approved recipient→new state→fresh consumer→independent blind judge и complete rollback обязателен; historical one-pair rules в [DPO conformance RAG](../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md).

**Источниковая карта RAG:** [RAG master](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md), [DPO legal RAG](../../09.09.26WorkLeft-RAG-01-dpo-legal-build.md), [Design-to-Code RAG](../../09.09.26WorkLeft-RAG-02-design-to-code.md), [MECH-01](../../09.09.26WorkLeft-MECH-01-mechanism-validation.md), [historical design contract](../../Алгоритм%20создания%20ДПО.md). RAG index даёт ссылки на историю — не автоматически evidence исполнения. Пользовательские unit и neural пути разделены намеренно.

## Протокол дедупликации / роли

Существующие `possibleDifs/README.md`, [N27 master crosswalk](27-MASTER-CROSSWALK-RP01-ALL-AXES-AND-NOVEL-SEAMS-2026-10-16.md), N28 papers и core READ_ONLY research не переписывались. **N29-A является новым локальным causal seam; N29-B — новая отличающаяся проверка ранее описанного source heuristic**, а не дублирующая претензия «нашли впервые». Скиллы: architect, phase planning, planning review, code-examination, prompt authoring, human-readable communication, experiment-design и claim-verification; отдельные model/expert tools в ходе написания не вызваны.
