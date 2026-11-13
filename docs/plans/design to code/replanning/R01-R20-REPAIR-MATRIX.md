# R01–R20 — адресные исправления, входы и приёмка

**13.10.2026 · PARALLEL20.** Это дельта действующих двадцати назначений, не двадцать новых запусков и не выполненные исследования. Сохраняются R21–R25 и схема **10+5+5+5**. Старт всех R01–R20 регулирует [PARALLEL20](PARALLEL-R01-R20-CONTRACT.md). Работавший агент принимает только свою дельту на checkpoint; уже подтверждённое не исследуется заново.

## 0. Что действительно проверено и как читать этот документ

Прямо прочитаны действующие AGENTS/START/PLAN/JOIN, COMMON и карточки R18/R19/R20; SOURCE-ROUTES, начало original task, MASTER RAG, начало previous rounds thinking results; relevant FINDINGS-REPAIR sections, начала results/R01, results/R07, results/R13 и results/R18. Последние содержат разные source pins и ограничения. Это **не полный обход всех RAG/WorkLeft, не полный аудит каждого results/Rxx и не новое чтение всех упомянутых core methods**. Остальные адресные обязанности ниже опираются также на действующий PLAN и FINDINGS §9; это assignments, не новые verdicts об исполнителях.

Наблюдаемые проблемы: START/AGENTS/COMMON ещё ограничивали старт R17, JOIN/PLAN/R19/R20 сохраняли nine-start формулировки; R07 и R13 прямо отмечены REPAIR_REQUIRED; R01 inventory остаётся IN_PROGRESS. R18 уже материализовал четыре A–D implementation packets и нашёл TextGenerator kernel, но ordinary endpoint предложен, а actual bindings и learning gain открыты. Нельзя поручить снова написать эти четыре packets или объявить генератор отсутствующим во всём repo.

**Источники этой дельты:** [FINDINGS §§1–10](FINDINGS-REPAIR-SEMANTIC-SEAMS-2026-10-13.md), [ALGORITHMS](COMPLETION-REPAIR-ALGORITHMS-2026-10-13.md), [DOMAINS](COMPLETION-REPAIR-DOMAINS-2026-10-13.md), [INTEGRATION](COMPLETION-REPAIR-INTEGRATION-2026-10-13.md), [R18 findings](results/R18/FINDINGS.md), [R18 work order](R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER.md), [source routing](SOURCE-ROUTES.md), [предыдущие результаты](<previous rounds thinking results.md>). Читать только относящиеся sections, не весь список каждому.

Каждая нижняя карточка — **подробный repair prompt, добавляемый содержанием к existing full Rxx prompt**, а не заменяющий его. COMMON, текущий PARALLEL20 contract, нужные source/skill excerpts, собственный checkpoint и эта одна карточка составляют один effective packet. Для R08 сохраняется CP4 full packet без повторного COMMON. Dispatcher собирает пакет до вызова, не поручает пользователю склеивать книги. При уже выполненной дельте вернуть точный receipt и NO_CHANGE, а не делать её повторно.

## Общие права, разделение и return contract

CURRENT TASK: собственная адресная repair/verification работа. NOT THE TASK: вся программа за один проход, новый trainer, code implementation, training, teacher calls, product tests/coverage, Windows/Actions или изменение чужого результата. Прямое пользовательское разрешение на GitHub document writes действует; доступ не объявлять read-only по 404 одного пути.

R01–R15 пишут только свои SOURCES.csv/FINDINGS.md/DECISIONS.md/CHANGE-MAP.csv/RUN-LOG.md; R16–R20 также assigned planning files. Shared-file owner/интегратор — по PARALLEL20 §3. Allowed: read/search/source inspection/documentary walkthrough и назначенные planning writes. Forbidden: code/tests/workflows/weights/datasets/core-skills edits, training/build/install/cleanup, force-push, чужие carriers. Child return-only.

Для каждого child parent до вызова раскрывает: CURRENT TASK/NOT THE TASK/EVIDENCE-ONLY; роль и actual skills; exact sources/ref/blob/span + почему; own checkpoint; current→target mechanism; concrete inputs/outputs; allowed/forbidden files/commands; подробные bounded действия; good/bad/edge; documentary acceptance; future learning falsifier; budget/stop; return schema и consumer. Секции A/B не независимые вызовы. При overflow продолжать от exact remaining question/span, не урезать scope и не добавлять автоматически R26.

Профили skills, путь `K/docs/skills/`, где K=`lubochka/llm_mvp_core:codex/dpo-implementation`: **P** phase-planning + plan-review; **A** trainable-architecture-decision; **C** code-examination; **M** prompt-patch-authoring; **H** output-readability-gate. Имена имеют суффикс `-SKILL.md`. Применённые при этой редакции excerpts: phase-planning 1–80; plan-review 1–110; trainable-architecture-decision 1–100; code-examination 1–85; prompt-patch-authoring 1–110; output-readability-gate 40–125. Worker читает актуальные нужные sections; чужая ссылка не собственный skill load. Исторические 30/50 последовательных проходов и blanket product QA из skills не импортируются поверх текущего запроса.

Return каждого назначения: человеческий результат; preserved IDs; own source/skill load; accepted/rejected incoming receipts; current→target mechanism; concrete artifact/patch; success/failure/edge walkthrough; documentary verdict; future experiment отдельно; exact remaining dependency/owner; actual calls/reviewer identity. Claim/evidence scope явно ограничить. NOT_MEASURED и NOT_PERFORMED не повышать.

---

<a id="r01"></a>
## R01 — закончить карту всей программы, не повторить S04

**Context/skills:** собственные F01–F13, SOURCE-ROUTES §§2–5, actual P/K trees, WorkLeft INDEX/REDATE/MASTER и обнаруженные соседние roots. P+A+H. Findings FS-08/09 — дополнительные receipts, не выполненный whole-tree audit.

**Prompt:** A. Продолжи existing visited/inventory state. Для каждого source family укажи реальный root/ref, current/history/generated/archive, discover/read/primary depth и владельца смыслового диапазона. B. Сохрани LF→U и все S00–S10/extras; классифицируй обнаруженные language/documentation/wiki/orchestrator/design-conversion/IDE roots, не выбирай по имени. C. Отдельно назначь чтение непрочитанного остатка previous rounds и первичных решений: это не общий долг R07/R10. Новые источники включай адресно, без полного reread всех workers.

**Good/Bad/Edge:** правильный результат — RAG IDE ведёт к F-root и конкретному R03/R06 вопросу; неправильный — «12 roots найдены, всё прочитано». Edge: одно имя в двух roots, merge of historical bodies со сдвигом строк, circular RAG, недоступный locator, новый source после snapshot.

**Приёмка/передача:** каждый обнаруженный required family имеет depth/owner/next question; непокрытое не исчезло. Future examination enabling: для любого заявленного domain capability можно восстановить требование, learned mechanism и независимый оценочный сюжет. Output — existing R01 carriers → R02/R03/R10/R14; карта не runtime результат.

<a id="r02"></a>
## R02 — восстановить смысл требований и authority

**Context/skills:** original/task/goal, Алгоритм создания ДПО, WorkLeft owner decisions/RESEARCH-02/03, current S04 RUN §§2.1–2.3, собственные U IDs, R18-F01…08 как attributed proposed deltas. P+A+H.

**Prompt:** A. Для Unit/Flow/Orchestrator/Graph, domain laws, DPO, tokens/kNN/LoRA, PC feedback, DTC и IDE свяжи original intent с существующим requirement ID и current authority. B. Различи preserved owner choice, superseded historical parameter, unresolved conflict и реализационную гипотезу. C. Добавь R18 findings только как addressed design/evidence receipt: v2 material/objective/stop contract не уже выполненная миграция и не owner-approved runtime факт.

**Good/Bad/Edge:** many→many возвращает typed payload и associations, а не просто список IDs. Bad: опечатка IlearnyngFlow создаёт новый интерфейс или старое правило разных authors отменяет current Chinese-both-sides. Edge: tie, ambiguous domain, historical replay percentage, product requirement без номера, прежняя рекомендация без owner approval.

**Приёмка:** нет потерянного original obligation; для каждой отмены есть exact authority и affected consumers. Future examination: каждое expectation имеет наблюдаемую доменную способность и falsifier, а не test-count target. Выход → R12/R16/R19, не полное переписывание WorkLeft.

<a id="r03"></a>
## R03 — RAG должен приводить к первичному решению

**Context/skills:** собственный visited frontier, MASTER/usage, current/nested MD+NDJSON, G/F/V routes из SOURCE-ROUTES; original decision lineage. P+A+H; C только для consequential code claims.

**Prompt:** A. Иди по remaining semantic frontier: указатель → actual source root → primary section → смысл/authority. Проверяй каждую критическую ссылку, а для прочих веток показывай границу глубины. B. Сохрани TensorCore, StackDetector-before-normalization, semantic compression, BM25+kNN, memory signs, arbiter, ordered patterns и mandatory skills. C. Отдельно разреши current scope helper-ban/SmartSkills и historical replay; не переносить старые цифры в новый experimental budget.

**Good/Bad/Edge:** найденное primary решение изменяет конкретную R06/R12 clause. Bad: NDJSON parses, поэтому архитектура verified. Edge: alias/cycle, filename case, архивный первоисточник единственного решения, недоступный документ, новые findings с другим pin.

**Приёмка:** source→meaning→recipient receipt для consequential decisions; непройденные branches названы. Future falsifier: downstream план не может выбрать противоположную runtime authority, не указав разрешённую миграцию. Выход → R04–R09/R10/R12; RAG не доказывает эффективность.

<a id="r04"></a>
## R04 — доменные представления и many→many без потери связей

**Context/skills:** S00–S03, original roles, LogicalContracts/LearningMachineContracts и actual representation consumers, FS-01…03/FS-D01…02, R18-F01/F03/F07. A+C+P+H.

**Prompt:** A. Отдельно опиши raw, canonical text, domain features, TeachingVector, codebook/vector-token IDs, trainer BPE и output type; укажи meaning/version и consumer каждого канала. B. Для перестановки/асинхронного many→many сохрани input IDs и output→input associations; positional Zip не semantic join. C. Проследи OutputRefs producer→scope/version/lifetime→payload resolver→typed consumer; найденный resolver переиспользовать, отсутствие заявлять лишь в границах реально выполненного поиска. D. Прими R18 v2 transport/stop question как отдельный token contract, не universal domain encoder.

**Good/Bad/Edge:** NULL и 0 или отрицание сохраняют различие; A/B reordered output присоединяется по identity. Bad: одинаковый float[] shape доказывает семантику, последний vocab ID автоматически EOS. Edge: fan-in, empty lawful output, cancelled branch, expired payload, unseen schema, token overflow.

**Приёмка:** контракт и exact producer/consumer либо bounded proposed seam на каждый loss/association gap. Future examination: смысловая сохранность и downstream качество на новых domain families; E01 encoder не prerequisite несвязанного E02. Выход → R05–R08/R12/R13/R18.

<a id="r05"></a>
## R05 — exact DPO должен дойти до правильных параметров

**Context/skills:** WorkLeft-02/INTEGRATION-01, S04–S06/current RUN, собственный CP2, FS-04…07, R18-F01…08 и work order A/B. A+C+P+H.

**Prompt:** A. На current dependent blobs сверь known Compact/480/whitespace, prefix-response token boundary, response mask/reduction и backward. B. Проверь отдельность public average inference scorer от proposed response-only DPO objective. C. Свяжи actual prepared Student checkpoint, installed adapter keys, RequiresGrad, optimizer membership и frozen independent reference; найденные freeze/install/filter не писать заново. D. Прими новый R18 kernel receipt и передай ordinary binding, но не объявляй assessment endpoint обычным endpoint. EOS/stop ID определяется подтверждённым tokenizer contract, не VocabCount−1 по предположению.

**Good/Bad/Edge:** distinction in long suffix доходит до response objective того же Student. Bad: seal на raw pair доказывает сохранённый training contrast; initial margin zero отклоняет корректную пару. Edge: equivalent pair, BOTH_BAD, contrast lost in preparation, prompt truncation, prepared-continuation flags, unknown update outcome.

**Приёмка:** input→objective→mutable keys→state→ordinary consumer карта с exact code spans и незакрытыми bindings. Future examination: repaired no-update против repaired update на одинаковом контракте; loss decrease отдельно от utility. Выход → R07/R12/R13/R18; не новый trainer.

<a id="r06"></a>
## R06 — dynamic kNN и learned путь, а не просто больше соседей

**Context/skills:** S07–S10/WorkLeft DESIGN-ACTIONS/P0–P6/PT, F authority/RAG primaries, actual 06.VectorSearch/08.GraphRag/15.OrchestratorModel/17.DynamicDomains/26.TrainableRouter и callers; FS-D04. A+C+P+H.

**Prompt:** A. Для каждого назначенного домена раздели admissibility, признаки/масштабы, score, k/radius, index, consumer соседей, memory signs и final learned authority. B. Исследуй семейство score s(d,q,n;θ)=Σw_j(d,q;θ)φ_j(q,n) с явно указанными hard exclusions и calibration; это кандидат, не «лучшая» готовая функция. C. Укажи, кто/чем обновляет θ, как feedback связан с выбором пути, и где недифференцируемый top-k отделён от learning objective. D. Для query-dependent score объясни compatible exact search либо shortlist+rerank и отдельную цену/ошибку candidate retrieval; не предполагай поддержку произвольной функции ANN backend.

**Good/Bad/Edge:** SQL eligibility учитывает dialect/schema/NULL, orchestration — dependency order; changed k рассматривается отдельно от actual multi-neighbor fusion. Bad: first hit consumer объявлен dynamic aggregation, solver победил — значит selector обучился. Edge: no valid neighbor, mixed domain, conflicting warning, changed embedding/index, ties, stale graph, rare lawful neighbor.

**Приёмка:** конкретные functions/state owners/consumers плюс controlled contrasts ranking/k/fusion. Future evaluation: oracle versus predicted domain, same candidate budget, law-valid path utility/time и all-branch cost. Выход → R07/R08/R12/R13/R22.

<a id="r07"></a>
## R07 — полный гибрид, не только ближайший adoption seam

**Context/skills:** обязательные direct original/Алгоритм ДПО/WorkLeft MECH-01/RESEARCH-03/RAG primary, full R07-REPAIR-WORK-ORDER и A–J, actual Unit/Flow/Orchestrator/Graph code; FS-D01…05 и R18 deltas. A+C+P+M+H.

**Prompt:** A. Сохрани useful old F01–F03, но восполни отсутствовавшие assigned primary reads. B. Для каждого блока построй цепь law/admissibility → learned state/authority → signal/objective → output/association → ordinary consumer. C. Для связи двух блоков укажи credit recipient, frozen counterpart, async/cancellation/budget и counterfactual, который отличает изменение solver от selector. D. Покрой domain tokens/kNN, DPO positive/negative/tie, LoRA projection, higher-order transfer, PC/feedback и неиспользуемые первым уроком ветки. R18 найденный generator — новый receipt, не полное закрытие гибрида.

**Good/Bad/Edge:** frozen Units + changed Flow меняет корректные associations на новых compositions. Bad: four role names без state/update/consumer или четыре новых trainers. Edge: delayed feedback, ambiguous credit, legitimate deterministic winner, wrong-domain confident path, graph cycle, stale candidate state.

**Приёмка:** весь assigned A–J scope имеет evidence/design/remaining disposition; совместимость ролей объяснена человеку. Future falsifiers: same budget/frozen component contrasts, unseen compositions, all-state retention/cost. Выход → R10/R12/R13/R18/R22; prompt repair не COMPLETE.

<a id="r08"></a>
## R08 — предметные законы и полезные preference пары

**Context/skills:** CP4 full R08 packet, WorkLeft RESEARCH-01/03/MECH, original prompts, existing seven DOMAINS passports, actual consumers, R18 stop/material semantics. A+P+H; C для mapping в код.

**Prompt:** A. Проверяй уже заполненные EN↔RU, orchestration, Figma→HTML/CSS, C#/Angular/SQL documentation, Builder и original extras; не создавать семь пустых паспортов. B. Различи hard law, контекстное правило, heuristic и preference; каждому дай domain applicability и source. C. Создай illustrative chosen/rejected/tie/both-bad/unknown examples одной frozen задачи: chosen удовлетворяет смыслу, rejected имеет конкретный дефект, не просто другой стиль/авторство. D. Для token/kNN предложений укажи смысловую координату, опасную нормализацию и consumer. E. Раздели train/dev/operational/confirmation/retention по source families, включая новые композиции и OOD.

**Good/Bad/Edge:** «нет значения» и «ноль» — различие, two equally lawful outputs — TIE. Bad: любой deterministic ответ rejected; random shuffled negative без смыслового вопроса. Edge: mixed language/domain, ambiguous requirements, near-duplicate sources, both answers wrong, malicious feedback, rare lawful case.

**Приёмка:** каждый реально изменённый passport содержит закон, positive/negative/edge, representation/kNN hypothesis и независимую exam family. Future measurements: predicted versus oracle domain и per-domain quality, не один общий score. Выход → R13/R21/R22, без teacher invocation.

<a id="r09"></a>
## R09 — затраты процесса, не выдуманная экономия

**Context/skills:** WorkLeft INTEGRATION-02/TEST-BATCHES/DEC/MATRIX/REPORT, existing workflow/run records, own evidence, R17 corrected policy/FS-10 и first-learning route. P+C+H.

**Prompt:** A. Восстанови заявленные 30 raw run rows и формулы либо оставь aggregates ATTRIBUTED/RAW_SAMPLE_NOT_RECONSTRUCTED. Новую выборку не выдавать за прежнюю. B. Раздели queue, elapsed job/workflow, runner usage, elapsed human time, external calls и actual invoice. C. Гипотезу test fitting проверяй requirement→code→assertion change→reason, различая repair fixture и weakening expectation. D. Привяжи каждый предлагаемый removed/deferred gate к реальному protected property и cost-to-first-learning-signal, включая domain/DPO/kNN/LoRA/ordinary evidence, не только CI.

**Good/Bad/Edge:** необязательный Windows release gate перенесён на native boundary с owner/trigger. Bad: commits count равен трудозатратам или три jobs подтверждают старое 3.2×. Edge: retries/cancelled runs, overlapping jobs, missing logs, branch policies 403, local Windows bootstrap.

**Приёмка:** reproducible bounded cost ledger и supported/refuted/inconclusive hypotheses. Future evaluation: время полного нового lesson до paired ordinary result, включая failures и judge costs; нет claimed saving без измерения. Выход → R11/R17/R23; никаких новых Actions.

<a id="r10"></a>
## R10 — incremental join без кругового подтверждения

**Context/skills:** actual nine research carrier sets, PARALLEL20, existing R10 join, новые R11–R18 receipts по зависимым вопросам. P+A+H.

**Prompt:** A. Для R01–R09 фиксируй индивидуальный snapshot/status/claim limits, не один общий «все завершены». B. Объедини origin claim→primary span→downstream decision→receipt; copied summaries одного origin не независимые подтверждения. C. Отдельно выдай sufficient first-lesson subset и whole-program uncovered map. D. Новые R18 facts направь R05/R12/R13/R15/R16/R17/R20; R18 не заменяет обязательные primary reads R07. Downstream proposal не доказательство upstream law.

**Good/Bad/Edge:** найденный kernel снимает «kernel неизвестен», но не ordinary adoption. Bad: R18 work order автоматически закрывает R13/R15. Edge: несовместимые pins, частичный sibling, два названия одного result, alias404, changed relevant blob, contradicting authority.

**Приёмка:** consequential conflicts имеют named owner/action, accepted receipts конкретны, нет global DONE barrier. Future examination handoff сохраняет independent panels/state/consumer и full domain remainder. Выход → R11–R25, свои carriers; не повтор девяти исследований.

<a id="r11"></a>
## R11 — два DAG и точный минимальный путь к уроку

**Context/skills:** existing INTEGRATION first-lesson/full-delivery DAG, R11 governing repair, source-bound R05/R07/R12 и R18 G1–G6/ordinary bridge. P+A+C+H.

**Prompt:** A. На existing DAG пометь material/token/objective/state/consumer/rights/measurement dependencies и evidence достаточного разрешения каждого. B. Отдели ready-for-document-design, ready-for-one implementation child, ready-for-authorized experiment и whole-delivery readiness. C. Исключи из первого пути unrelated S05 remote publication, complete curriculum, новый encoder и Graph resolver, если ordinary E1 их не использует; сохрани их delivery owners/triggers. D. Пересчитай минимальный unresolved cut-set после R18: kernel найден, actual parent/mutable keys/stop policy/ordinary binding/review всё ещё отдельны.

**Good/Bad/Edge:** можно реализовать адресный loss contract без ожидания всех domain passports, но нельзя обучать неизвестный checkpoint. Bad: «всё после S04 удалить» либо «25 DONE до первой пары». Edge: native bootstrap, pre-existing equivalent seam, no applicable ordinary caller, low-contrast lesson, limited local resources.

**Приёмка:** для каждого prerequisite есть causal reason; каждое deferred original requirement осталось в full DAG. Future examination: самый ранний valid signal без confounded controls, не доказанный оптимум расписания. Выход → R12/R14/R18/R23.

<a id="r12"></a>
## R12 — решить контракты гибрида и принять конкретные R18 решения

**Context/skills:** full R12 repair A–F/current own decisions, original/RAG/WorkLeft, FS-D01…05, R18-F01…08 и A–D, R04/R06/R07 causal results. A+C+P+H.

**Prompt:** A. Дай accepted/contested/needs-binding disposition для associations, resolver, domain score/k, semantic projection, DPO material/objective/recipient и lifecycle. B. Раздельно прими или предметно оспорь R18 segmented-byte-BPE-v2, response-only sum, preserved average scorer, LoRA-only E1 design и generation bridge. C. Для stop semantics не считать VocabCount−1 доказанным EOS; выбираемый finite-horizon/verified-stop contract должен одинаково применяться всем сравниваемым arms и conditional scoring. D. Уточни actual interface migrations и updating owner; E1 CodeDocumentation не выдавать за выбор оптимального алгоритма всего проекта.

**Good/Bad/Edge:** LoRA адаптирует названную матрицу, а P_d отдельно имеет source/target meaning/objective/consumer. Bad: scalar TeachingVector multiplier назван semantic projection, новый trainer поверх existing. Edge: continuous output без probability model, immutable base, query-dependent nonmetric score, prepared continuation, changed codebook/index, unsupported stopping token.

**Приёмка:** решения исполнимы и имеют source/caller/consumer; runtime-specific неизвестные имеют resolver owner. Future examination: frozen-component causal contrasts, lawful unknown/abstain и resource/retention bounds. Выход → R13/R15/R16/R18; binding gap не architecture PASS.

<a id="r13"></a>
## R13 — causal experiment compiler с отделением engineering fix от learning

**Context/skills:** полный R13-REPAIR-WORK-ORDER, existing E1–E4, R12 decisions, DOMAINS §10, FS §7, R18 A–D/G1–G6. A+C+P+H.

**Prompt:** A. Сохрани полный hybrid experiment scope, не ограничь R13 первым E1. B. Для E1 задай три сравнения: **L** existing legacy behavior (только когда разрешён/совместим), **N** исправленный material/objective/stop/ordinary pipeline с frozen parent без update, **U** тот же исправленный pipeline после одного разрешённого нового урока. N→U — learning effect; L→N — engineering/compatibility effect. Если L не исполним, честно NOT_COMPARABLE; learning estimate всё равно требует N/U. Не обучать одну pair повторно ради arms. C. Привяжи реальные parent/profile/material/token contract/trainable keys/consumer к protocol; неизвестные не заполняй вымышленными IDs. D. Зафиксируй семейства, rubric, operational versus untouched confirmation, practical gain/noninferiority/retention thresholds, uncertainty method, budget и stop до outcomes. Numeric выбор должен быть обоснован baseline noise/utility, не примером из документа.

**Good/Bad/Edge:** оба N/U используют один v2 generation contract и одинаковые retrieval/context/budget. Bad: legacy parent против нового endpoint+updated challenger — всё приписано DPO. Edge: no measurable gain, failed admission, repeated evaluation selection, changed family mix, EOS/length cap, multiple lawful outputs, runtime costs censored after failure.

**Приёмка:** model-ready recipe без скрытого architecture choice; каждое undecided runtime prerequisite помечено. Future results — per-domain mapping/path/quality/time/full PC, all attempted lessons, retention, IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE. Выход → R15/R17/R18/R21–R23; протокол не измерение.

<a id="r14"></a>
## R14 — перераспределить работу, не потерять delivery

**Context/skills:** current M0–M7/old IDs, all slices/extras/source map, R11 two DAG, actual R16–R18 changes. P+A+H.

**Prompt:** A. Для каждого capability зафиксируй reuse/change/defer и точный owner/trigger; новые R18 A–D связать с existing task IDs, не дублировать под другой нумерацией. B. Удержи DPO/DynamicGraph, DTC end-to-end и IDE ordinary journeys; законченный first lesson не закрывает S05–S10. C. Раздели source-reading load, decision work, implementation preparation и future experiment; большой WorkLeft режется по смысловым spans. D. Самостоятельные R18–R20 preparation tasks не ждут whole R14 DONE.

**Good/Bad/Edge:** native release отложен до соответствующего milestone, не удалён. Bad: «после E1 дальше когда-нибудь», same missing seam назначен двум implementations. Edge: already implemented equivalent code, new source family, capability spans multiple slices, budget shortage, conflicting owners.

**Приёмка:** old→new trace без исчезнувших требований, каждый defer исполним по trigger, каждый child имеет finite scope. Future examination: staged domain breadth/transfer и ordinary DTC/IDE outcomes. Выход → R16–R20/R24; план нагрузки не фактическое ускорение.

<a id="r15"></a>
## R15 — независимое опровержение exact candidate

**Context/skills:** конкретные bytes/diff R11–R14 и R18, inherited objections, PARALLEL20, original expectations. P+A+C+M+H.

**Prompt:** A. Начни review доступного candidate scope сразу, не ожидая всей программы. B. Попытайся опровергнуть: lossless contrast, correct response objective/stop, mutable recipient, independent reference, output associations, ordinary adoption, law-valid path, no-update comparability и PC claim. C. Пройди отдельный counterexample «N/U отличаются кодом/контекстом, не только learning». D. Возвращай accepted scope/defects/remaining с candidate identity; unrelated later carrier commit не новая семантика, affected code/design delta требует addressed rereview.

**Good/Bad/Edge:** другой фактически вызванный reviewer указывает exact candidate и конкретный воспроизводимый defect. Bad: автор назвал себя Astra и выдал независимый PASS, review предыдущей версии перенесён автоматически. Edge: hidden actual model identity, partial candidate, change after review, unjudgable output, conflicting reviewer interpretations.

**Приёмка:** actual separate invocation evidence либо NOT_PERFORMED; self-walkthrough явно не независим. Future ML review проверяет причинность и domain failures, а не делает accuracy measurement мнением. Выход → R16–R20; publication DRAFT не поддельное ACCEPT.

<a id="r16"></a>
## R16 — применять решённые требования по clauses

**Context/skills:** actual live WorkLeft/requirements, R02 IDs, accepted R12/FS/R18 decisions, R14 map и R15 scope dispositions. P+A+C+H.

**Prompt:** A. Сохрани уже перенесённую Chinese-both-sides policy; не выполнять её заново. B. Для каждой новой semantic clause запиши current preimage→replacement→authority→requirement→consumer; спорные R18 design choices остаются DRAFT до адресного разрешения. C. Различи exact pair provenance, usable preference, optimizer admission, state change, ordinary utility; no coverage proxy. D. Применяй resolved clauses без ожидания общего sibling DONE, shared intersections передавай владельцу по PARALLEL20.

**Good/Bad/Edge:** actual requirement требует identity association/response contract там, где consumer ими пользуется. Bad: убрать все safeguards ради скорости, выдать proposed endpoint за существующий, удалить original-only expectation. Edge: concurrent edit, late authority, already equivalent clause, conflicting EOS proposal, older R15 review.

**Приёмка:** current blob-based patch, preserved text semantics, remote readback и receipt. Future examination: requirement теперь ведёт к конкретному lawful domain outcome/experiment, не числу тестов. Выход → R17–R20/R25, не чужие results.

<a id="r17"></a>
## R17 — уточнить acceptance, не повторить уже принятую матрицу

**Context/skills:** existing R17 evidence policy/migration proposal, corrected FS-10, R13 three-arm causal contract, R18 A–D/G1–G6 и current source-bound requirements. P+A+C+H.

**Prompt:** A. Принимай применённую risk matrix как baseline. Исправь только противоречащие consumers и добавь различие material integrity / parameter mutation / scientific utility / ordinary adoption. B. Требуй N/U одинакового repaired pipeline и full-cost denominator всех попыток; engineering delta не DPO gain. C. CI/native/required status rules сохраняют отдельную область; Markdown не снимает branch protection. D. Для каждого допустимого retry укажи scope; unknown optimizer outcome не повторный update.

**Good/Bad/Edge:** INCONCLUSIVE удерживает parent и сохраняет evidence, не ослабляет критерий постфактум. Bad: lower loss или successful hash достаточен для acceptance, unverified 30-run aggregates стали saving. Edge: native mismatch, no valid pair, assessment unavailable, noisy tiny panel, async cancelled branch, law violation despite high average score.

**Приёмка:** только адресные live policy patches/readbacks и соответствующие packet receipts. Future verdicts заранее отделяют IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE; thresholds выбираются R13 до outcomes. Выход → R18–R25, workflows неизменны.

<a id="r18"></a>
## R18 — сохранить готовые A–D и закрывать только конкретные G1–G6

**Context/skills:** materialized R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER, own SOURCES/FINDINGS/DECISIONS, R12 dispositions, R13 complete protocol, R17 policy, FS relevant deltas. A+C+P+M+H.

**Prompt:** A. Не создавать новый trainer/ещё четыре packets; текущие A/B/C/D — база. Сверь relevant blob drift и exact ready criterion каждого child отдельно. B. В A/B прими settled material/response-mask/backward/public-average/stop decisions, actual LoRA keys и independent reference. C. В C различи найденный TextGenerator kernel, proposed ordinary service/endpoint и observed adoption; equivalent existing bridge переиспользуй. D. В D встрои R13 N/U same-code comparison; legacy engineering contrast отдельно, без повторного обучения пары. E. По каждому G1–G6 верни bound value с source/authority или exact remaining resolver; документационный work order не запускает code/training.

**Good/Bad/Edge:** changed weights становятся visible через cold ordinary request выбранного accepted state. Bad: assessment-generation kernel объявлен готовым ordinary endpoint или rejected candidate promoted до examination. Edge: unverified EOS ID, same prompt different token segmentation, full sequence overflow, warmed prepared flags, unknown update outcome, missing native root, invalid trial state.

**Приёмка:** addressed A–D delta, no lost detailed child fields, ready/draft per child и невыдуманные runtime prerequisites. Future examination: N→U domain utility/time/resources/retention, separate A/B engineering correctness and D scientific effect. Выход → R05/R12/R13/R15/R16/R17/R20/R22; full-hybrid learning не доказано E1.

<a id="r19"></a>
## R19 — единая политика PARALLEL20 в действующих входах

**Context/skills:** actual README/START/AGENTS/PLAN/COMMON/JOIN/R19/R20, PARALLEL20, эта матрица, late R18 result, unique older repairs. P+M+H+A.

**Prompt:** A. Заменяй живые ограничения числа workers на R01–R20, но сохраняй nine-research-input смысл R10 и 25-task budget. B. Один путь: START→common один раз→full own prompt→own repair card→needed source/skill contents/checkpoint. R08 CP4 exception не терять. C. Назначь non-overlapping read/write sets R16–R20; shared file имеет одного интегратора и section receipts, не пять concurrent whole-file replacements. D. Проведи document-only walkthrough independent start и dependency-limited finalization всех R01–R20, а не только первых девяти. Уже выполненные этой редакцией routing repairs не повторяй; закрывай residual consumers.

**Good/Bad/Edge:** R18 готовит own packet, пока R12 finishes unrelated graph design; disputed recipient не invented ready. Bad: добавлен двадцатый overlay без reachable entrypoint или новый текст сократил R07 A–J. Edge: old standalone prompt уже запущен, concurrent START edits, source same name/different root, previous review candidate, R18 materialized after snapshot.

**Приёмка:** current readbacks, old→current routes и semantic preservation; ни стартового barrier R17, ни нового whole-program audit. Future enablement: executor получает correct mechanism/experiment, не evidence learner gain. Выход → R20/R25, outside-D propagation явно назначен.

<a id="r20"></a>
## R20 — действительно автономные prompts, включая dependent drafts

**Context/skills:** все 25 numbered prompts как assignments, actual relevant checkpoint/source spans, current dispatcher/PARALLEL20, эта матрица, materialized R18 A–D и full unique R07/R11/R12/R13/R14/R08 repairs. P+M+H+A; C для code packets.

**Prompt:** A. Для каждого R01–R20 сформируй один resolved packet: цель/не-задача, состояние, роль/skills, exact context/RAG/primary/code, действия/outputs, meaningful good/bad/edge, documentary acceptance, future causal examination, права/budget/stop/return. B. Встраивай содержанием только относящуюся дельту; full mandatory semantic scope не сокращай ради token бюджета. C. Отдельно маркируй ready-for-independent-preparation и ready-for-final-decision/implementation; missing upstream contract не вынуждает idle и не заполняется вымышленным значением. D. Холодный документный walkthrough всех двадцати starts, plus R21–R25 reachable continuation; A–D R18 materialized reuse и R08 exception обязательны. Не выдавать его за двадцать actual model runs.

**Good/Bad/Edge:** R06 получает score/index/consumer spans и example same candidates different selector; R13 получает actual N/U contract и predefined evidence gaps. Bad: «прочитай все skills» или child только с заголовками без алгоритма. Edge: context overflow, changed blob, unavailable primary, own result newer than packet, unsupported model alias, unresolved numeric budget, child interruption.

**Приёмка:** manifests с source depth, resolved content, explicit exclusions и addressed gaps; child packet столь же полный. Actual context telemetry не выдумывать из размера файлов. Выход → R18–R25/dispatcher через owner; no new queue.

---

## Общий научный критерий для последующих опытов

Цель не «все пары приняты», а доказуемая полезность новых уроков. Сохранять доменные законы, domain assignment, representation fidelity, admissible solution paths и compactness как отдельные оси. Hard constraint — не просто мягкий bonus к accuracy. Positive/negative означают предпочтение в одинаковой задаче, не provenance алгоритма. Оценочный inference на frozen panels не равен запрещённому replay training.

По каждому bulk: одинаковые N/U tasks/rubric/context/budget, family-level uncertainty, per-domain и worst-domain regression, latency distribution/полное lesson time, полный RAM/VRAM/disk/index/graph/reference/optimizer/activations/branches/judges footprint, all attempted lessons и retention. Более лёгкий следующий bulk не доказательство improvement. Operational panels, влиявшие на выбор, не untouched confirmation. Best path — лучший из рассмотренных допустимых при данном budget, не недоказанный global optimum.

LoRA уменьшает trainable update через low-rank параметризацию, но не определяет tokenizer или доменный смысл выходного вектора; projection имеет отдельный contract/objective/consumer. Стандартный DPO опирается на conditional response likelihood policy/reference; arbitrary similarity и TeachingVector scalar weighting нельзя переименовать в DPO/projection без отдельной математической модели. Primary scientific references (не свидетельства эффективности XIIGEN): Rafailov et al., Direct Preference Optimization, https://arxiv.org/abs/2305.18290 ; Hu et al., LoRA, https://arxiv.org/abs/2106.09685 . Конкретные контрасты/распределение нагрузки здесь — предлагаемый дизайн исследования.

## Граница выпуска этой редакции

Опубликована адресная программа ремонта; Rxx statuses/learned results не изменены редактором. Independent Astra examination, runtime binding и эмпирический эффект не заявляются. Исполнитель должен вернуть source-bound disposition каждой применимой дельты, а не ещё одну общую рекомендацию «надо всё исследовать».
