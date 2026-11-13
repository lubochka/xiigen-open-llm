# RP01 / possibleDifs — независимое дополнение N26, приоритет гипотез и контроль причинности

**16.10.2026 | Действительный снимок источника:** `llm_mvp_core@dfd13067ecebdf7b3c3da5000ddbe8604b94ce27`; исторический E30/E32/E39 архив 2026-05-01. **Статус:** `SOURCE_AUDIT_PUBLISHED / NEW_LEARNING_GAIN_NOT_VERIFIED / INDEPENDENT_ASTRA_REVIEW_NOT_RUN`. Не считать старые документы самостоятельными верификациями друг друга. Примечание: на планировании `llm_plan3` могут работать другие агенты.

## Важнейшее разграничение: какую именно «обучающуюся» систему проверяем?

**Система исторических domain functions:** structural AST, FamilyCollapser/semantic features, BM25, retrieval kNN, correction memory, TranslationTrainedUnits, Flow, verifier, episodic routing, частично tiny neural/bigram experiments. Эти функции в части положительных исторических запусков улучшали **свои** задачи, но в отдельных матричных экспериментах были ошибки дизайна или метрик. Нельзя выдавать все результаты за доказанную neural token generation.

**RP01 scientific Student:** exact-material DPO on random/limited B0, q/v LoRA, 500 DPO records C# и 500 ENRU, по 50 optimizer steps на домен; historical free generation C# 0/3→0/3, ENRU 0/4→0/4. Обновлены разрешённые LoRA; это факт optimizer, не доказательство usable skill. Источник базовой сверки: [01-PRIORITIZED-DIFFERENCES.md](01-PRIORITIZED-DIFFERENCES.md), [00-MASTER-SYNTHESIS-AND-CONFLICTS.md](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [RP01 recovery code audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).

Текущие результаты недостаточны, чтобы однозначно распределить деградацию по этим двум системам: сначала нужен **same input, same split, same consumer** сравнение. Нельзя приписывать невызванному Unit причиной провала отдельно стоящей Student policy.

## Дополнение N26: ТРИ ДЕТАЛЬНЫХ ПРОБЕЛА, не сводимых к ещё одной общей гипотезе KNN

| ID / приоритет | Что именно новое доказано | Почему важно для гипотезы деградации | Точное досье |
|---|---|---|---|
| N26-E39 / P1 validity | E39 помечен `passed=true` даже при обязательном `recall .911 < .95`; общий `Passed` берёт только primary improvement. Production `QualityOrchestrator.Check` проверяет только 2 поверхностных условия, несмотря на объявленные check lists. | **Promotion критерий мог не защищать качество**, но DPO judge CODEX не подменяется этим классом. Не доказан consumer RP01. | [N26-A E39](26-NOVEL-E39-GATE-PROMOTION-AND-CHECK-EXECUTION-2026-10-16.md) |
| N26-E32 / P1 validity+transfer | 10 canonical × 9 phrasings, split по строкам, 0.50→0.5556 на 18 val; source-group holdout отсутствует. Исторический KNN на FamilyCollapser заменён константами domain count в product decomposer. | **Положительный архив мог завышать перенос на новые задачи; trainable-to-hardcoded transfer точно неэквивалентен**. Имеется в соседних ветках до RP01 — не новая regression RP01. | [N26-B E32](26-NOVEL-E32-GROUP-LEAKAGE-AND-KNN-TO-CONSTANT-2026-10-16.md) |
| N26-E30 / P1 retrieval provenance | E30 original −48.31%, E30v2 word Jaccard −33.33% и всего 1/16 hits; baseline/labels/fallback между версиями не сопоставимы. In v2 forced fallback label=0 при 8 классах×2, без majority. | Ошибка — **непригодность простого cosine/cache + неудавшийся Jaccard rescue**, а не «вся память/KNN бесполезна». Проверить hybrid similarity и explicit abstain как гипотезу, не объявлять заранее выигрыш. | [N26-C E30](26-NOVEL-E30-FAILED-V2-AND-FALLBACK-COLLAPSE-2026-10-16.md) |

## Все обязательные семейства RP01 gaps — **куда обращаться в существующем каталоге**

Здесь **реестр покрываемых направлений**, не ложный claim «найдены абсолютно все баги» и не пересказ более ста соседних markdown:

| Область | Где доказательства / проверки | Сначала проверять |
|---|---|---|
| DPO authorship, chosen/rejected quality, objective, mask, exact-v2 vs legacy | [02-DPO-DATA-OBJECTIVE-GRADIENT](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [03-DPO-PAIR-AND-GRADIENT](03-DPO-PAIR-AND-GRADIENT.md), [10-INDEPENDENT-CODE-PATH](10-INDEPENDENT-CODE-PATH-RECONCILIATION-2026-10-16.md) | Source-disjoint pair quality, token/response mask, tiny overfit; **не** приписывать legacy Compact активному exact-v2 |
| Random B0 / LoRA q/v rank, training budget / finite difference | [04-BASE-LORA-CHECKPOINT](04-BASE-LORA-CHECKPOINT.md), [03-LORA-TOKENIZATION-VECTOR-SPACE](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [11-MICRO-COLD-START](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) | Одна переменная: tiny SFT→DPO против DPO-only, real free generation |
| BPE, tokenizer/root vocab, semantic coordinates | [03-LORA-TOKENIZATION-VECTOR-SPACE](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [21-INDEPENDENT-BPE-RETRAIN](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [10-STOP-TOKENS](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md) | Trainer vs inference token/stop identity, local-vs-global axes; не заявлять «кириллица удалена» |
| KNN, late fusion, structure/order/semantic law, matrix→KNN | [04-KNN-HYBRID-RETRIEVAL](04-KNN-HYBRID-RETRIEVAL.md), [12-MATRIX-TO-KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [14-KNN-IDENTITY](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md), [N26-C](26-NOVEL-E30-FAILED-V2-AND-FALLBACK-COLLAPSE-2026-10-16.md) | Same query pool, top-k vs first-hit, hard-negatives, confidence/coverage |
| One DPO → named Units; translation morphology; learned corrections | [06-UNITS-FLOWS-GRAPH](06-UNITS-FLOWS-GRAPH.md), [10-TRANSLATION-UNIT](10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md), [23-LANGUAGE-ROUTING-POS](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md) | Trace `DPOId → UnitId → state update → actual consumer`, not class existence alone |
| Flow/Graph/async roles, orchestration, selection | [05-UNITS-FLOW-GRAPH-DETERMINISM](05-UNITS-FLOW-GRAPH-DETERMINISM.md), [N26-B](26-NOVEL-E32-GROUP-LEAKAGE-AND-KNN-TO-CONSTANT-2026-10-16.md) | Evidence attribution, context retention across branches, real recipient |
| Checkpoint state vs adoption/serving; restore interface mismatch | [06-CHECKPOINT-RESTORE-ADOPTION](06-CHECKPOINT-RESTORE-ADOPTION.md), [10-TRANSLATION-UNIT](10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md) | Full reopen already reported PASS in RP01; test actual identity-bound ordinary consumer and translation metadata-only interface separately |
| Continual/replay/cross-domain, historical successful trains and sibling branches | [11-CROSS-BRANCH](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md), [12-MATRIX-KNN-PROVENANCE](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md), [24-E36-FEEDBACK](24-NEW-E36-FEEDBACK-KNN-SPLIT-AND-ROUTING-2026-10-16.md) | Donor checkpoint+recipient+heldout, no train accuracy as semantic transfer; don't erase negative examples reinterpreted through KNN |

**Важная оговорка:** подпапка core `docs/plans/replanning-possible-diffs` прочитана **только** как источниковая ветка; там ничего не изменялось. На момент этой публикации не выполнено исчерпывающее чтение всех ссылочных RAG/родительских репозиториев и всех многочисленных sibling feature branches. Это открытый audit coverage debt, а не скрытый PASS.

## Самый короткий доказательный путь — три последовательные независимые «воронки»

**0. Non-neural validation integrity (очень дешёво).** E39 gate replay: архивный `recall=.911`, expected `BLOCKED_RECALL` vs старый `PASSED`; negative control `recall=.97` должен изменить судьбу. Доказать discrepancy без запуска модели. Ни одна старая метрика не переписывается.

**1. KNN functional positive proof — [MICRO-E32-01](26-MICRO-E32-GROUP-DISJOINT-2026-10-16.md).** А/В на тех же 90, feature/k=3/label фиксированы, train/val 72/18. A=random row (есть пересечение по canonical source), B=все 45 двух-групповых holdout по 2 canonical families (нет пересечения). Измерять distinct source families и `KNN – median`, а не просто верхнюю цифру +11.11%. Не интерпретировать per-phrase points как 810 независимых примеров.

**2. Actual RP01 learned-answer proof — минимальный Student micro-training.** На заранее выбранных **8–32 действительно корректных source-disjoint hard pairs**, проверенных CODEX/Astra6 UltraCode, четыре matched arms:
- (A) frozen B0 Student no updates;
- (B) same B0 + exact DPO tiny overfit;
- (C) warm/SFT-initialized B0 + matched DPO;
- (D) same actual consumer + trained, law-aware hybrid Unit/KNN (без ложного приписывания памяти LoRA).
Измерить training-set overfit первым, затем blinded new-family free answers / semantic correctness, changed logprobs vs decoded token, confidence, coverage, false-accepted hard laws и consumer trace. Каждое arm требует **отдельного one-factor контраста** (B vs A, C vs B и D в отдельном frozen-student test) — это не одновременное изменение всех причин. Нельзя считать это выполненным при публикации плана.

**Приоритет:** нейронный RP01 cold start / gradient/no-free-gain — P0; E39/E32/E30 помогают исключить ложный historical verdict и восстановить иные функциональные learning recipients, но **не** заменяют P0 real Student проверку.

## Governing quality and responsibilities

- **DPO author:** внешняя открытая китайская модель генерирует chosen/rejected; не принимается за судью.
- **Human-in-loop:** **CODEX, работающий на модели Astra 6 UltraCode, выполняет человеческий выбор**; **тот же CODEX/Astra 6 UltraCode выполняет отдельную оценку качества DPO**, отклоняет invalid/TIE/BOTH_BAD/UNJUDGEABLE; **после** optimizer update отдельно оценивается качество Student/consumer. Это три раздельных свидетельства.
- Каждый документ помечен `DRAFT` до **независимого** CODEX/Astra 6 UltraCode review, привязанного к точному planning commit и source SHA. Ни один такой model review не заявлен проведённым в этой сессии. Использованы skills: архитектурный аудит, reference/source inventory, phase planning, plan-review, generated-code-review, experiment-design, prompt patch и learning-path-audit. 
- Вспомогательные/Windows CI compile PASS не заменяют domain-generalization. Commit+readback документов — evidence публикации, **не** evidence повышения качества модели.
