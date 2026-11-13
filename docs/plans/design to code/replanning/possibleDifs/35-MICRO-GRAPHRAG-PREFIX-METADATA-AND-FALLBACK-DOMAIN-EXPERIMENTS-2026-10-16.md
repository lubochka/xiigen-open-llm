
# MICRO-N35 — три короткие причинные проверки GraphRAG

**16.10.2026. PLAN ONLY; native .NET NOT_RUN; quality NOT_MEASURED.** Основной audit: [N35](35-NEW-GRAPHRAG-EMBEDDING-PREFIX-METADATA-AND-HIDDEN-FALLBACK-2026-10-16.md). Править только planning/possibleDifs; core/docs/plans/replanning-possible-diffs READ ONLY.

## Gate 0 — существует ли целевой consumer?

До эксперимента исполнитель находит фактический конструктор EmbeddingModel: model checkpoint/model config, tokenizer hash и real maxLen; GraphRag.Indexer, LocalSearch, GlobalSearch, HybridRetriever; кто принимает RetrievalContext и формирует ordinary response. RP01 CodeDocumentationSimulationDpoExperiment может GraphRAG вообще не вызывать. Если этот вызов не найден, поставить NOT_APPLICABLE_TO_RP01; можно изучать отдельный GraphRAG, но нельзя объявить причину отрицательной RP01 neural accuracy.

Только immutable source-family split TRAIN/DEV/SEALED; frozen domains, actual law oracles, same weights/tokenizer/random state/graph/metadata; не менять несколько факторов между A и B.

## MICRO-N35-G1 — prefix-only против tail-aware при том же budget

**ExperimentId:** N35-G1. **Tier1:** 4–8 индексируемых документов, 8–12 новых запросов на один домен. **Baseline A:** текущая token selection, первые N BPE IDs, N=actual maxLen. **Enhanced B:** только selection изменяется на первые floor(N/2) и последние ceil(N/2) IDs; БЕЗ увеличения N, изменения model/LoRA/BPE/graph. Пересчитать ВСЕ document vectors и query vectors для каждой arm, иначе non-version-matched index обесценит результат.

L0: создать 4 пары запросов/документов с реально идентичными первыми N BPE IDs и противоположным законным правилом строго после cutoff. Сравнить токенные ID, graph embedding vectors, norm и факт ненулевой ветки. Если использовался fallback, такой case НЕ доказывает learned-prefix equivalence; вывести отдельно. Boundary negative control: перенести контраст внутрь первых N IDs. Поведение A: одинаковые learned vectors для same prefix и ненулевой нормы; B может обнаружить противоположный tail, но качество НЕ гарантировано.

L1: один exact BruteForceIndex перед HNSW; одинаковые node IDs, graph edges и PageRank, retrieval topK и law-oracle. Снять matched paired retrieval candidate IDs, law-correct node recall@k, source ID, actual ContextText и (только при consumer) конечный correct/wrong/abstained/critical answer.

**Фальсификатор:** финальный consumer исправляет смысл за счет PageRank/другого retrieval, либо B не улучшает независимое law correctness. Тогда source-level truncation остается, но loss of product learning не доказана.

## MICRO-N35-G2 — RawText против Name+Signature+RawText

**ExperimentId:** N35-G2. **Tier1:** 4–8 нод, 8–12 source-disjoint query families. **Baseline A:** real Indexer.BuildNodeText — RawText if nonempty, иначе Signature, иначе Name. **Enhanced B:** только один меняемый фактор: embedding material explicit named text fields Name + Signature + RawText. Все строки в обеих arms намеренно короче N BPE IDs, чтобы исключить G1; прочие score, index, graph и query одинаковы. Ревекторизовать обе arms без повторного обучения.

L0: как минимум две пары нод с одинаковым непустым RawText и разной **релевантной** Signature/Name. A передает равные vector-input строки; B — разные. Содержательная независимая экспертиза должна подтвердить, что разница в Signature представляет law, а не искусственно придуманную метку.

L1: сравнить retrieval@k и конечное качество на C# overload/nullability, SQL schema/dialect и DTC Angular-version; контролем служат узлы, где RawText уже полностью содержит смысл. Новые ложные совпадения и удорожание тоже записать. Повышение recall без исправления пользовательских ответов = RETRIEVAL_DELTA_ONLY.

**Фальсификатор:** в настоящем corpus Signature не содержит уникального закона, а RawText самодостаточен, либо финальный consumer не потребляет GraphRAG.

## MICRO-N35-G3 — zero-vector fallback и ложная видимость обучения

**ExperimentId:** N35-G3. **Tier1:** 2–4 не пустых строки. **A:** обычный неизмененный ненулевой encoder с norm>1e-6. **B:** исключительно disposable test-only LlmModel с явно нулевыми hidden activations; пользоваться реальным EmbeddingModel.Embed(nonempty text), ничего не публиковать и не изменять production checkpoint или обычное право мутации DPO. Если для тестового нулевого model имеются безопасные construct helpers — использовать их.

L0: зарегистрировать, достигается ли branch norm<=1e-6; сверить B с independent normalized vector, который подсчитывает каждый token ID ПОЛНОГО исходного текста в modulo(hiddenDim) slot. Для long suffix проверить source property, что fallback читает текст после cutoff. Нельзя автоматически заявлять, что хэш дает различные вектора или что empty-text ветка достижима.

L1 выполнять **только при настоящем наблюдаемом fallback** в живом GraphRAG: 4–8 frozen requests, условное сравнение branch B против явного abstention/no-retrieval на том же input, независимая semantic law проверка. Primary measure — incorrect accepted final outputs; fallback percentage вторично. Нет zero branch в production → INACTIVE_IN_OBSERVED_RUNTIME, а не корень RP01.

## Минимальный доменный пилот и метрики

| Домен | Семантический contrast | Проверка закона | Первая проверка |
| --- | --- | --- | --- |
| C# | null fallback 7/0, nullable call, overload/scope | Roslyn/типизация плюс CODEX | G1 и G2 |
| SQL | LEFT JOIN ON-vs-WHERE, UNKNOWN, dialect и schema | сравнить row cardinality и SQL law | G1 и G2 |
| EN→RU | must/must not, actor/recipient, referents | независимая смысловая оценка допустимых переводов | G1; POS/order feature не глобальный |
| DTC/IDE | Figma restriction после длинного текста, Angular version | разрешенные DOM/CSS/SDK ограничения | G1 и G2 |

Одинаковые A/B source-disjoint cases, 2 positive controls и 2 negative controls в выбранном домене. 8–12 случаев — только exploratory; не менять gold после outputs. **Primary** paired law-correct per family при реальном consumer. **Secondary** recall@k, norm, fallback branch, candidate IDs, latency; wins и losses и критические нарушения считать отдельно. Предпочитать один targeted локальный .NET runner, не запускать Windows CI или 500 DPO ради диагностики.

Записать per-case: experimentId/arm, domain, sourceFamily, promptHash, tokenizerSHA, encoderCheckpointSHA, actualMaxLen, BPE-length, firstNIDsHash, norm, fallbackUsed, indexVersion, candidateIds, output, lawCorrect, critical, latency, judgeSession. Если fallbackUsed не наблюдаем, статус NOT_OBSERVABLE, добавить только bounded test probe.

**Вердикты:** CODE_SEAM_REPRODUCED / NATIVE_RETRIEVAL_DELTA / OBSERVED_SMALL_GAIN / NO_QUALITY_GAIN_OBSERVED / NOT_TRIGGERED / NOT_APPLICABLE_TO_RP01 / INVALID_EVIDENCE. Результат всего лишь вычисления BPE/hash или JS proxy никогда не обозначать как actual Student training gain.

## Исполнительский CODEX work order и независимая проверка

CURRENT TASK: три отдельные N35 native one-factor pilots, не смешивать GraphRAG и neural DPO. NOT THE TASK: main500, Windows CI, исправление core protected audit, перепись RP01 frozen results. EVIDENCE ONLY: [N35 source](35-NEW-GRAPHRAG-EMBEDDING-PREFIX-METADATA-AND-HIDDEN-FALLBACK-2026-10-16.md), [N23 64-token KNN](23-TRANSFORMER-KNN-64TOKEN-PREFIX-ALIAS-2026-10-16.md), [N28 GraphRAG context](28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md) and actual source commit SHA.

required_skills_and_gates: planning-session-startup-SKILL.md, intent-to-plan-SKILL.md, phase-planning-SKILL.md, trainable-architecture-decision-SKILL.md, code-examination-SKILL.md, experiment-design-SKILL.md, plan-review-SKILL.md, generated-code-review-SKILL.md, claim-verification-SKILL.md, human-readable-architecture-audit-SKILL.md, prompt-patch-authoring-SKILL.md and human-communication skill. Map callers and consumers BEFORE editing tests; make source hypothesis BEFORE opening old tests. Independent reviewer checks final exact diff and actual runtime.

context_files_to_read: complete EmbeddingModel.cs, MeanPooling.cs, GraphRag Indexer/LocalSearch/GlobalSearch/HybridRetriever, Tokenizer/BpeTokenizer.cs, LlmModel.ForwardHidden, tests and consumer constructors. good_examples: real same-case A/B, final law flips and no new errors. bad_examples: same SHA/vector equality advertised as better learned output. allowed_files: bounded test-only probe plus authorized planning/possibleDifs receipts. forbidden_files: core/docs/plans/replanning-possible-diffs, frozen model/score files. allowed_commands: one targeted local dotnet test/run and exact readback. forbidden_commands: Windows CI campaigns, main500, unapproved Chinese/Astra model calls, other file edits. verification_required: full-version input hashes, evidence of actual route, token alignment, A/B quality before/after cold restore, reviewer of exact diff. return_packet_schema: source and commit SHA, observed status and counts, executable command, individual raw case receipts, paired actual scores, unverified limitations, next falsifier, changed files, independent reviewer decision; claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed.

**Роли DPO, если по результату потребуется новое обучение:** выбор (человеческая роль) делает CODEX c Astra 6 UltraCode; обе DPO sides chosen/rejected создает допущенная внешняя открытая китайская модель; независимую проверку DPO и реальных outputs выполняет CODEX с Astra 6 UltraCode. Данная карточка не означает запуск этих моделей.
