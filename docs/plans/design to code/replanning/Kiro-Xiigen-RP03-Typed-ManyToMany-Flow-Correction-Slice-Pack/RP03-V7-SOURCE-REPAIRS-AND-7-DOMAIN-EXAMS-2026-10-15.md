# RP03 V7 · Дополнительные code-backed repairs N26–N31 и семь доменов
Дата: 15 октября 2026. Тип: PLANNING / TASK REPAIR ONLY. Не запускались Student/Flow training, Windows CI, domain exams или Astra review. Код K, веса и результаты измерений не менялись. Кодовый источник на момент чтения: lubochka/llm_mvp_core, ветка codex/dpo-implementation, HEAD 025c42c74b1d30ffa4660f9824a52cf8fcaf7c88. Перед исполнением переопределить актуальные P_SHA/K_SHA и blob SHA.

**Одна действующая версия:** [V7 dispatch](RP03-V7-LEARNING-FIRST-DISPATCH-2026-10-15.md), 15 parent cards tasks/v7, пять исходных children и четыре новых независимых экзамена. Старые V6 не подмешивать. Старт: 00B || 02A || 00C-A; фоново 00A/00C-B; 02B строит scorer немедленно, применяет W1 только по реальному signal; 02D cold load; независимо 01A→01B→01C; 02C join → 03B/03C. **Все семь доменов НЕ precondition первого C# урока.**

## Нормативная последовательность F0/F1 и что считается результатом

Сохранить раздельно: Unit = typed many-input↔many-output ассоциации и результаты; Flow = законный F0 запрет + F1 обучаемая коррекция законных путей; Orchestrator = асинхронное исполнение (RP04); DynamicGraph = higher-order relationships (RP04). RP01 Student DPO с отдельной китайской моделью-автором обеих final sides и независимым Astra-экзаменатором НЕ есть Flow W. RP02 отвечает за domain law/vector/adaptive kNN; RP18 — за LoRA и семантическую проекцию; RP05 — за последующий feedback.

**Первое настоящее обучение:** получить от независимого и разрешённого источника один feedback_id и сравнимое предпочтение B>A на одном input x, оба маршрута F0-legal, версии domain/law/coordinate заморожены; phi вычисляется до результата. Минимальный pairwise Flow update: g = phi(x,B) - phi(x,A), p = sigmoid(W0·g), W1 = clip(W0 + eta*((1-p)*g - lambda*W0)). Пример иллюстративный: phi(A)=(1,0), phi(B)=(0,1), W0=(0,0), eta=.2, lambda=0, B>A → W1=(-.1,+.1). Никаких fake W1 при NO_SIGNAL/g=0, NaN, stale version, repeat feedback, одного допустимого маршрута. При real W1: checkpoint/journal once, другой PID cold read, then actual serving typed next Unit→Host, затем слепые P/C0/T по независимым семействам.

## N26 — Serving trust gate vs first experimental lesson (CODE VERIFIED)

K: 21.AgentInfrastructure/Logical/RegistryDynamicGraphEdgeAuthorityResolver.cs, Resolve (blob fef6479a71dc0bb042bcdfb4132e14a1a8d5e077). Сначала ResolveLatestById(AuthorityUnitId), потом exact role/domain/types, IsLearned и TrustState >= Probationary. Candidate/untrained scorer этим serving route недопустим; это факт исходника, но не наблюдённый runtime bug.

**Repair owner 02A→02B:** назвать реальный unit registrar, current served tuple и trial scope. Строить scorer/учить W на отдельном экспериментальном recipient, который сохраняет F0 и не обходит live guard. После фактического lesson и проверенного state/cold causal evidence использовать существующий разрешённый publish/promote путь. Не менять IsLearned/TrustState просто для запуска урока и не снижать порог resolver.
GOOD: опыт на Candidate отдельно, Trusted Host продолжает штатную службу.
BAD: выставить IsLearned=true/Trusted до обучающей мутации, тем самым подменить готовность метаданными.
EDGE: нет допустимой живой authority — NO_LIVE_AUTHORITY и отдельный trial, не fake Host adoption.
FALSIFIER: найден другой реальный serving path, который уже законно принимает untrained Candidate; процитировать exact caller→reader.
Verification: READ_ONLY=0 product tests; при изменении publish — 1 targeted allowed/denied trust/role/type case, не Windows blanket.

## N27 — Latest exact-id registry shadowing (CODE VERIFIED, effect hypothesis)

K: 21.AgentInfrastructure/Logical/InMemoryLogicalUnitRegistry.cs, ResolveLatestById, строки около 357–404, latest RegistrationOrder по exact UnitId выбирается до type/trust/learned фильтра. Новый непроверенный tuple с прежним UnitId МОЖЕТ заслонить действующий Trusted. Гипотеза отказа serving при такой конкретной публикации, НЕ факт его наступления.

**Repair owner 02A/02D/02C:** найти actual publisher и rollback interface; держать Candidate на отдельном id/слоте или staging, только принятый checkpoint/descriptor/registry pointer публиковать атомарно, сохранять served parent. Прочитать ещё один раз registry после рестарта и после rollback.
GOOD: отказанный trial не заменил lawful served tuple.
BAD: newest Candidate становится serving и доверять resolver, что он выберет более старый Trusted.
EDGE: concurrent promotion, тип/домен несовместимы, crash между W и pointer, restart.
FALSIFIER: в реальном registrar trial и served всегда имеют разный id до publish, shadow исключён контрактом.
Minimum: changed-risk old-serving survival + rollback/read proof, не stochastic coverage.

## N28 — Куда теряется typed output (CODE VERIFIED boundary)

K: 21.AgentInfrastructure/Logical/DeterministicDynamicGraphNodeOutputProjector.cs, Project, blob eced7aa0b4b917d603f31905fab2b5f09ef5971e. L2→L3 adapter.UpstreamCandidateRefs получает GraphNodeCandidate.CandidateId и hash множества этих ID. GraphNodeCandidate.OutputRefs (00.Core/DynamicGraphContracts.cs) — отдельное поле; projector НЕ переносит real TOutput. Ранее N23 отметил scalar AsQueryable; новый факт — точное место identity-only handoff.

**Repair owner 00B/01A/B/C/02C:** q1→{o1,o2}, q2→{o2,o3} хранит 4 explicit association refs, shared o2 но request-scoped authorization. CandidateId→store→OutputRef→real typed TOutput→explicit typed next port→real ILogicalUnit.ExecuteAsync→ordinary Host. Не ломать текущий ABI и canonical candidate-ID hash.
GOOD: o2 может быть общим, но право q1 не даёт право q2; именно real output получен вторым Unit.
BAD: считать CandidateId содержимым, ZIP arrays или считать L3 Flow result выполненным L2 target (TargetsExecuted=false).
EDGE: expired ref, wrong tenant, schema/coordinate drift between selection/effect — DENIED/ABSTAIN; F0 recheck.
FALSIFIER: есть уже exact approved resolver и serving next Unit; зафиксировать method, не писать новый адаптер.
Minimum: touched typed allow + denied/cancel probe + actual consumer trace.

## N29 — Semantic version owner in edge authority input (RECORD VERIFIED, GAP NOT PROVEN)

K: 21.AgentInfrastructure/Logical/DynamicGraphEdgeAuthorityContracts.cs, blob 51e04cdb350aeaefdcbe8cf88c9105918f3ccac2, EdgeAuthorityInput содержит EdgeId, SemanticTokens, PredicateIds, ContextSignals, Candidates, ExcludedLeakageSignals; здесь нет отдельных полей law_version/tokenizer_id/vector_coordinate_version/tenant. Возможно, версии живут во внешнем Context/snapshot; отсутствие глобальной проверки НЕ доказано.

**Repair owner 02A/02B + RP02 liaison:** установить настоящие producer→state/checkpoint→reader и версии domain/schema/law/coordinates. Два равных vector dimension не доказывают одинаковую семантику. Дополнять только минимальный внутренний envelope, если доказан конкретный пропуск.
GOOD: SQL NULL и RU negation vector coordinate никогда не читаются через общий W только потому, что dim=64.
BAD: принять same dimension как совместимые domain/law feature meanings.
EDGE: изменение law/tokenizer в ходе запроса → pin snapshot/abstain/migration owner RP14/18.
Verification: zero product tests для trace, changed-risk version-accept/deny при внедрении.

## N30 — Кто обучает Flow и почему это не ExecuteAsync side effect (DOMAIN-SPECIFIC SKILL FACT)

K docs/skills/design-conversion-ilearningmachine-architecture-GUIDE.md, blob 3db7cca0233c81488af27e2cce61949ad128d756: A-LM-01/02 требуют DesignConversionMachine : ILearningMachine<TDomain> owner Train*, promotion/checkpoint, а Units/Flows/Orchestrator исполняют inference/QA. Это правило специализировано для design-to-code; перенос на все RP03 домены — отдельный architecture decision, не молчаливый глобальный рефакторинг.

**Repair owner 02B/03A:** назвать фактический training owner C# Flow, физический W journal/writer отдельно от L2 scorer. Все 1+5+3 guard-probes только читают тот же immutable W snapshot. Для DesignConversion использовать Machine-owner и отдельно проверить владельца их state.
GOOD: trainer пишет W1 один раз, scorer purely reads, probes не потребляют feedback.
BAD: Flow/guard ExecuteAsync случайно делает gradient step 9 раз, или новый helper без checkpoint объявлен обучением.
EDGE: DesignConversionMachine ещё не готов → OWNER_GAP для domain, без задержки первого C# lesson.

## N31 — Семь WorkLeft доменов, но только один честный первый урок (PLAN GAP)

WorkLeft семь треков: EN↔RU, Orchestration, Figma→HTML/CSS, C# docs, Angular docs, SQL docs, lawful DPO construction. Уже есть child exams CSHARP, ENRU, SQL. Добавлены четыре самостоятельных НЕ blocking: ORCH, DESIGN, ANGULAR, DPO. В 00C-B фиксировать независимые source-family TRAIN/OP_DEV/EXAM_SEALED; hidden CERT cannot drive feedback.

GOOD: три пилота получают первые сравнительные цифры, четыре следующих экзамена проверяют ширину, собственный domain feedback только после законного наблюдения.
BAD: C# W1 на Angular оценивается как «Angular обучен». Это лишь OOD transfer, пока отсутствует Angular-specific lawful pair/recipient state.
EDGE: данных мало, laws спорные или пользовательский Host недоступен — INCONCLUSIVE/UNKNOWN/NOT_ADOPTED, не искусственный 95%.
FALSIFIER: exam families overlap, порог подгонялся на CERT, C0/T различались одновременно tokenizer/k/graph/code — learning effect not identified.

## Эксперимент и бюджет проверки без тестового хвоста

P = legacy; C0 = corrected code + W0, no lesson; T = тот же corrected code + W1. T−C0 — кандидат причинного эффекта обучения, C0−P — интеграция. Непересекающиеся источники и неизменные код/фичи/судья/бюджет/rights, явное law HARD/CONDITIONAL/UNKNOWN, отдельные scores законности, mapping, пути, Host результата, времени и полной PC памяти/диска. Считать paired correct k/N, ошибочные hard laws, abstain/uncertainty, ресурсы p50/p95, CPU/GPU RAM/VRAM, tokenizer/index/LoRA/Flow journal/checkpoint/rollback, retention на каждом новом bulk. Эксперименты по взаимодействию (W0/W1)×(k_old/k_new) только ПОСЛЕ первого урока. KNN применяет domain logical constraints к соседям, а не слепой hits[0].

| Задача | Ровно необходимое evidence | Не запрашивать |
|---|---|---|
| Source/WorkLeft/RAG audit | file:method→caller→consumer, opened PRIMARY, zero new product tests | Windows actions, %coverage |
| Typed integration/F0 | actual type/rights allow + denial/cancel changed branches | случайные exact-word fixtures |
| Flow writer + cold state | numeric finite/sign/no-op, idempotency, atomic save/load/rollback, serving registry | broad test totals |
| Learning | независимые blinded P/C0/T domain outputs, paired law quality and cost | source-level GREEN as model improvement |
| Host/PC | real next L2 + user output, fresh PID, per-bulk full budget | handoff flag as Host success |

Return schema each task: exact P/K SHAs, primary/skill sections actually read, code producer→caller→writer→reader→ordinary Host, lawful feedback provenance or NO_SIGNAL, numeric W0/W1 or NOT_RUN, cold PID and serving registry version, typed next L2, raw P/C0/T or NOT_MEASURED, resource ledger, GOOD/BAD/EDGE/falsifier, owner of narrow next repair; separate SOURCE_TRACED/CODE_CHANGED/PARAMETER_CHANGED/COLD_CONSUMED/NEXT_TYPED_EXECUTED/CAUSAL_MEASURED/USER_ADOPTED/INDEPENDENT_ASTRA each TRUE/FALSE/NOT_RUN.

**Статус этого документа: source review + планирование. Runtime code/weights, independent Astra and benchmark NOT_RUN/NOT_MEASURED.**
