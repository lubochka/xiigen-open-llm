# N33 — RP01: доказательная карта причин ухудшения обучения и двух новых проверяемых гипотез

**16.10.2026 | AUTHOR AUDIT / DRAFT / NOT NATIVE LEARNING / NOT INDEPENDENT CODEX REVIEW**

Целевая ветка планирования: lubochka/xiigen-planning, llm_plan3. Исходный core: lubochka/llm_mvp_core, codex/dpo-implementation @ **32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7**. Историческая ветка DesignToCode @ 8f20ceb8a9bfbadab2ad309226c47dd76e714548; проверены имена веток language_translate и external_domains, а также выборочные первичные архивные JSON. Полный построчный обход всех parent RAG, всех 234 соседних possibleDifs и всех исторических commit-деревьев не заявляется. Это дополнительная самостоятельная сверка существующих доказательств плюс N33-01/N33-02, а не признание всех соседних гипотез доказанными.

**READ ONLY:** llm_mvp_core/docs/plans/replanning-possible-diffs. В этом исследовании НЕ меняется ни этот каталог, ни core product code, ни модели, ни historical run receipts.

## 1. Главный диагноз

В RP01 не воспроизведён полный исторический learner. Сравниваются различные получатели учебного сигнала: (A) LoRA Student, (B) обучаемая дискретная Unit/KNN/phrase/verification memory, (C) условные domain-law/AST/POS/sequence алгоритмы, (D) Flow/Orchestrator и ordinary consumer, который должен реально читать сохранённый опыт.

Frozen RP01 C#500: 25 параметризованных семей ×20, 500 records, 50 optimizer updates, 16 изменённых LoRA tensors при 39 frozen base; margin Δ +0.0012275314, chosen ranking 16/25 без улучшения; **semantic free 0/3→0/3**. EN→RU500: 500 records, ещё 50 steps, margin Δ +0.0001100063, ranking 12/20 без улучшения; **semantic free 0/4→0/4**. Эти числа взяты из [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md); новой репликации не было.

Точная причинная граница: LoRA changed ≠ useful quality changed; KNN successful ≠ LoRA generalization; deterministic law may be полезным learned-hybrid selector, а не автоматически дефектом. Задача — установить, что именно училось, где хранится, как восстанавливается, достигает ли обычного user-output и улучшает ли качество на новых независимых случаях.

## 2. Первичные исторические положительные и отрицательные результаты

| Источник | Зафиксировано в run JSON | Что допустимо заключить |
| --- | --- | --- |
| [E07 Structural Context Token 01.05](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-002.json) | Matrix-like structural feature: 0.1502→0.1310, negative | Этот способ кодирования AST-родителя не помог именно в E07 |
| [E09 Structural KNN 01.05](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) | Roslyn structural KNN: 0.2413→0.3507, positive, 5617 tokens | Структурный сигнал может помогать KNN; это НЕ matched-data replay E07 |
| [E14 Tier2 30.04](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-04-30-001.json) | context-action KNN 0.75→1.00, 4 validation files | Малый положительный сигнал |
| [E14 Tier3 01.05](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-002.json) | 0.8824→0.8676, 68 val files; сильный label imbalance | Не универсальная победа KNN, требуется domain- и class-aware gate |
| [E26 Contextual Dictionary](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json) | 0.4→1.0 на 15 примерах, hard=6 | Переводческая POS/контекстная дизамбигуация, ограниченный стенд |
| [E43 Replay](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) | 0.3226→0.3978 retention | Replay заслуживает отдельно измеренного retention при неизменной панели |
| [E29V2 DPO Pair Value](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/35.ExperimentsV2/results/E29V2-DPO-PAIR-VALUE/results/run-2026-05-01-001.json) | 0.2308→0.2308, negative | Обучение оценке пар не гарантирует выигрыш |
| [Соседние DesignToCode V191–V210](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md) | Реальные DPO StepMany, optimizer, сохранённые checkpoints, training margins до 0.833 на отдельных runs | Реальное обновление весов, НЕ гарантированный blind free-answer gain |

Нельзя ретроспективно переписать отрицательный E07 как успех: позитивный E09 — другая реализация и другая панель. Нельзя также стереть E14 Tier3 потому, что Tier2 был положительным. Provenance matrix→KNN: [12](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md), [12 misread](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [32 structural](32-NEW-E09-CROSS-FILE-TRANSITIONS-AND-IGNORED-SOURCE-2026-10-16.md).

## 3. Сквозной каталог различий, с владельцем предшествующего доказательства

Здесь каждая строка — отдельная гипотеза/класс, не автоматически установленная root cause RP01. Метки: C = исходный код проверен; R = сохранённый historical result; H = качество причинно не проверено; P = analytic proxy; P0/P1/P2 = порядок исследования.

| ID | Этап source→learner→consumer; возможная потеря | Предыдущее доказательство/владелец | Диагностика и область |
| --- | --- | --- | --- |
| A01 P0 | Кейс выбирается, но DPO chosen/rejected не различают конкретный нарушенный law | [02 DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [32 same-prompt](32-NEW-RP01-SAME-PROMPT-CONTRAST-ADMISSION-2026-10-16.md) | Raw exact pair quality judge; все домены |
| A02 P0 | Реальный внешний Chinese author заменён programmatic/synthetic pair, обучение по ошибочным меткам | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [B500](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-BATCH500-REFERENCE-RECONCILIATION.md) | Authorship + hash + source law; без ложной маркировки |
| A03 P0 | Random B0 629 токенов, 4 слоя без pretrained language competence | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [11 cold start](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) | Tiny SFT warm-up vs cold DPO; Student only |
| A04 P0 | DPO answer-mask/gradient/reference/sign, sum-vs-mean, token offsets; нет доказательства численного finite-difference | [03 gradient](03-DPO-PAIR-AND-GRADIENT.md) | 8–32 pair overfit + tiny finite difference; actual recipient |
| A05 P1 | LoRA q/v rank8 alpha16, не доказан достаточный capacity/target coverage; dropout conditional | [03 LoRA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [04 checkpoint](04-BASE-LORA-CHECKPOINT.md) | Frozen base, adapter gradients, target-module A/B; RP01 dropout=0 |
| A06 P1 | TeachingVector превращён в scalar nonzero-slot count, смысл координат не обучается | [03 LoRA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [26 AdamW](26-NEW-ADAMW-SCALE-CANCELLATION-2026-10-16.md) | Vector semantic permutation and gradient/effect; DPO vector path |
| A07 P0 | Byte BPE/tokenizer train coverage, frozen hash, truncation, split segments and common response prefix | [21 BPE](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [27 prefix](27-RP01-DPO-COMMON-PREFIX-GENERATION-GAP-2026-10-16.md), [16 bridge](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md) | Same exact token IDs; legacy bridge НЕ RP01 exact |
| A08 P1 | Неправильный EOS/stop/response length; decoder и cached/full logits расходятся | [10 stop](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md), [26 stop](26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md), [16 KV](16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md) | Consumer trace; ordinary Student |
| A09 P0 | Saved checkpoint не переходит в restore/adopted Flow; потери optimizer/RNG/parent/consumed IDs | [06 restore](06-CHECKPOINT-RESTORE-ADOPTION.md), [27 mutable cache](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) | Fresh process same behavior before/after |
| A10 P0 | DPO→typed sub-Units: выбранная сторона не переводится в закон, POS/AST, memory, verifier/Executor | [05 Units](05-UNITS-FLOW-GRAPH-DETERMINISM.md), [17 dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md), [21 operator](21-NEW-GAPS-SYMBOL-BLIND-LESSON-ADMISSION-AND-DIRECTION-MISROUTING-2026-10-16.md) | Проверять мутацию каждого реального Unit и ordinary route |
| A11 P0 | KNN: key version, normalize/collision, direction namespace, candidate tiers и corpus growth | [N28 tokenizer key](28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md), [N30 direction](30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md), [N20 fusion](20-NEW-FINDINGS-FUSION-MEMORY-GROWTH-2026-10-16.md) | Pair index/query hashes, exact source-disjoint recall; Translation |
| A12 P1 | HNSW stale/alias/NaN/dimension, partial Add, reverse prune, lost lesson reachability | [N23 shape](23-INDEPENDENT-AUDIT-SHAPE-DECODER-AND-LEARNING-2026-10-16.md), [N30 Add](30-NEW-HNSW-FAILED-ADD-PARTIAL-MEMORY-2026-10-16.md), [N28 prune](28-NEW-HNSW-REVERSE-PRUNE-AND-LOST-LESSON-REACHABILITY-2026-10-16.md) | ANN recall@k against guarded brute-force; KNN only |
| A13 P1 | Positive & negative retrieval tiers; order/AST/POS/negation/syntactic roles, adaptive k | [E26](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md), [E30](26-NEW-E30-PREDICATE-CACHE-STATE-ALIAS-AND-MISREAD-2026-10-16.md), [N31 replay](31-NEW-REPLAY-COUNT-CLIFF-AND-PSEUDOSTRUCTURE-2026-10-16.md) | Domain-specific order/negation counterfactual; не включать глобально |
| A14 P1 | Replay data balance, admission window, domain retention, feedback projection | [07 replay](07-CONTINUAL-REPLAY.md), [27 replay aperture](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md) | Same old-law panel after new lesson; PC continual |
| A15 P1 | Graph/Flow/Orchestrator shortcut/retrieval bypass и ложное closure readiness | [18 bypass](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), [27 graph](27-NEW-E50-PREMATURE-GRAPH-CLOSURE-FALSE-PASS-2026-10-16.md) | Trace final consumer actual learned ID, not mere storage |
| A16 P0 | Historical metric leakage, train/test subset changes, same-source leakage, hard-negative mislabelling, evaluator comparing wrong outputs | [26 history](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md), [E13 split](17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md), [N28 heldout](28-NEW-N28-UNIT-HELDOUT-OBJECTIVE-NOT-LIVE-QUALITY-2026-10-16.md) | Family-disjoint blind CODEX judge, paired before/after |
| **N33-01 P1** | HNSW random level generator static/global, не сохраняется в индекс: cold continuation может изменить retrieval | [новая узкая карточка](33-NEW-HNSW-RNG-CONTINUATION-AND-COUPLING-2026-10-16.md) | P1 KNN only; code confirmed, causal quality not tested |
| **N33-02 P2** | Byte-level truncation at max tokens may end inside UTF-8 sequence; Decode inserts replacement symbol | [новая узкая карточка](33-NEW-UTF8-DECODER-CUTOFF-2026-10-16.md) | RU/HE/ES and non-ASCII; source + 4-case Python proxy, native not run |

Эти классы покрывают вопрос шире, чем только DPO/LoRA: векторную геометрию, learned units, KNN, semantic tree/порядок слов, state, replay, recovery и реальную оценку. Это не 18 вновь открытых ошибок. Большинство строк — указатели на подробные авторские первичные отчёты соседних агентов.

## 4. Доменная политика вместо одной глобальной функции

| Признак/механизм | Где оправдан | Где ограничить/отключить до самостоятельного сравнения |
| --- | --- | --- |
| AST parent/operator/callsite в KNN | C#/Angular/SQL/code structure | Свободный EN→RU текст без AST; случайное AST кодирование E07 уже проваливалось |
| POS + grammatical role, agreement, negation scope | EN↔RU, HE, ES, запросы с субъектом/объектом | Regex/exact symbolic tokens, если лингвистическая эвристика стирает код |
| Соседство / word-order / sequence | Перевод, генерация текста, DSL token sequence | Bag-of-words предпочтительный, если порядок не несёт права/смысла; делать A/B |
| Source exact map и project-local correction | Исправление специфических правил и версий | Cross-domain/cross-direction global namespace без права/версии |
| Structural vectors + KNN | Code routing, qualified patterns | Не подменять матрицей, где успешный KNN узкий и иной |
| Replay и law guard | Continual learning с retention и safety | Hard-coded selector без фактического learned update/quality evidence |
| UTF-8 boundary completion | Не-ASCII генерация RU/HE/ES | ASCII-only C# RP01 — отрицательный контроль |
| HNSW RNG state | Любая KNN память с cold incremental insertion | Прямой RP01 DPO/TextGenerator без KNN — не обвинять |

## 5. Порядок кратких причинных проверок — быстрый путь к пользе

1. **L0**: exact 8–32 новых case-пар, source-family split до обучения, CODEX/Astra case choice, китайская модель создаёт обе стороны, отдельный CODEX/Astra проверяет bytes/domain law, checksum. Без fake Astra author.
2. **L1**: frozen Student and reference; один актуальный DPO training route, numeric gradient/sign + response-mask, 8–32 overfit and tiny warmup-vs-cold counterfactual. Проверять actual free output, а не weight diff.
3. **L2**: для Unit/KNN при неизменном Student сравнить no-memory vs trained-memory vs trained-memory+domain POS/AST/order feature; отдельные панели C#, SQL, ENRU, HE/ES. Source-family-disjoint, correct and wrong counts.
4. **L3**: preserved model/index/checkpoint → **fresh process** → actual ordinary consumer output; сравнить exact IDs, route, evidence. Для HNSW дополнительно проверить N33-01.
5. **L4**: новый feedback bulk/replay → тот же старый law panel + новые unseen prompts. Приоритет дальнейшего масштабирования даёт **правильный ответ на новом вопросе** и отсутствие новых критических ошибок, не число зелёных тестов.

Один малый независимый опыт вместо повторного 500-pair run описан в [N33 executor protocol](33-MICRO-CAUSAL-EXECUTOR-AND-EXPERIMENT-2026-10-16.md). Ниже в той карточке указано, как отличить риск качества KNN от отдельного провала RP01 Student.

## 6. Полномочия, provenance, review и стоп-условия

**Непереопределяемые роли**: (a) **CODEX с Astra 6 UltraCode выполняет роль человека и ВЫБИРАЕТ case**; (b) **открытая внешняя китайская модель создаёт обе финальные стороны chosen И rejected**, с честным exact author/model ID; (c) **отдельный CODEX с Astra 6 UltraCode оценивает качество обеих сторон** и provenance; (d) ещё один независимый CODEX/Astra 6 UltraCode оценивает blind free answers до/после, если назначен. Никто здесь не имитировал фактический вызов этих моделей.

Не смешивать: actual source evidence; historical result; source-equivalent proxy; native user-quality causal A/B; независимый reviewer PASS; full production adoption. Любое отсутствие критического звена значит UNKNOWN / NOT_RUN, а не PASS. Skill routes прочитаны из core/docs/skills: human-readable architecture audit, phase-planning, plan-review, code-examination, generated-code-review, prompt-patch-authoring, claim verification, reference inventory, training-data-gap-audit, root-cause tracing и investigation handoff; review-вердикт этого конкретного нового candidate: **DRAFT_PENDING_INDEPENDENT_REVIEW**.

Источники контракта: [Recovered Design Contract](../RECOVERED-DESIGN-CONTRACT.md), [Domain Experiment Playbook](../DOMAIN-EXPERIMENT-PLAYBOOK.md), [WorkLeft RAG master](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md), [Алгоритм создания ДПО](../../Алгоритм%20создания%20ДПО.md). RAG — навигация и требования, не доказательство native runtime.
