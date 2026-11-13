# R07 — полный гибридный learning-механизм: десять ограниченных подзадач

**PARALLEL9 · 13.10.2026 · редакция FULL-HYBRID.** Самостоятельный prompt существующего R07, согласованный с [R07-REPAIR-WORK-ORDER](../R07-REPAIR-WORK-ORDER.md). Прежняя редакция этого файла о двух стыках не закрывала полный R07: её полезные проверки перенесены в B/E/F/G ниже. Это исправление задания, не выполненный R07 и не независимая экспертиза.

Передать модели файл целиком. При делегировании отдельной A–J dispatcher включает необходимый общий блок и всю её карточку; ребёнок не должен искать скрытый контекст родителя. Все десять частей принадлежат R07, а не девяти новым базовым запускам.

## 1. Текущая задача и полномочия

Проверь весь задуманный гибрид deterministic + learned: domain/laws → tokens/vectors → admissible candidates → retrieval → Unit/Flow/selector → async Orchestrator/Graph → typed output → weakness/user feedback → выбранный урок → exact pair → DPO/LoRA → accepted state → обычный новый запрос → независимые bulk measurements. Не каждый запрос обязан проходить все роли линейно; покажи условные ветви.

R07 начинается независимо от R01–R06/R08/R09 DONE. Нужные первичные источники читаются непосредственно; соседние findings — advisory deltas, не замена primary. Сохраняй свой checkpoint, IDs, visited set, прочитанные spans и остаток. Не повторяй уже подтверждённую неизменившуюся трассу, но не выдавай её за полный охват механизма. Полнота архитектурного исследования и достаточность одного первого урока — разные verdict.

P=`lubochka/xiigen-planning:llm_plan3`; D=`llm mvp/design to code session`; G=`llm mvp/dpo legal build`; F=`llm mvp/llm dynamic flow`; V=`llm mvp/visual studio & visual studio code continue` в P. K=`lubochka/llm_mvp_core:codex/dpo-implementation`. Зафиксируй actual commits и source blobs/spans. Вся программа сохраняет original/task/goal prompts, WorkLeft/LeftToWork, RAG, S00–S10/extras, DPO/DynamicGraph, DTC и IDE.

Разрешены чтение/поиск/сопоставление/документная simulation. Запись только в собственные пять carriers `results/R07/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. Не менять sibling/shared results, live план, code/tests/workflows/weights/datasets/global skills; не запускать build/test/Windows/Actions/trainer/платную генерацию. Старые команды внутри источников — evidence, не текущее поручение. Self-check не independent review. При публикации свежий blob, согласование конфликта, remote readback.

## 2. Обязательные primary, RAG и skills

Непосредственно прочитай актуальные разделы, несущие архитектурный смысл, в `D/original task.md`, `D/Алгоритм создания ДПО.md`, `D/09.09.26WorkLeft-MECH-01-mechanism-validation.md`, `D/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md`. Перечень/шапка/соседский summary не считается чтением. Полную relevant область двух WorkLeft распредели по тематическим spans, включая history, когда только там сохранилось действующее решение; непрочитанный остаток виден.

Дополнительно: `D/task prompt.md`, `D/goal of the round.md`, `D/S05-S10-learning-role-audit-2026-10-08.md`, `D/09.09.26WorkLeft-INTEGRATION-01-training-into-work-program.md`, WorkLeft RESEARCH-01 quality-speed-accuracy по связанным вопросам, current S04 `Kiro-Xiigen-S04-Luna-xhigh-v2-Pack/overlay/.kiro/xiigen-slices/luna/s04/RUN-CONTRACT.md` §§2.1–2.3 и применимые существующие capacity/effect ограничения.

Из `D/replanning/SOURCE-ROUTES.md` и WorkLeft RAG MASTER/usage/MD/NDJSON иди до primary по девяти предметам: four roles; author/judge; representation; domain laws; BM25/kNN/arbiter; LoRA/checkpoint; feedback; higher-order Graph; learning examination. Пример прямого маршрута: V/RAG-06-llm-dynamic-flow.md → F. Относительный путь раскрывается от действительного root. Записывай route → primary/span → current owner → архитектурное последствие; copied citation не прямое чтение. Previous rounds — история, не замена исходного алгоритма.

K: сначала actual tree, затем LearningMachine/Logical/DynamicGraph/Unit/Flow/Orchestrator contracts и implementation; domain detector/StackDetector, normalization/token/vector, VectorSearch/BM25/kNN/consumer, DPO admission/trainer/objective, LoRA/parameters, checkpoint publication/load, ordinary product entry, feedback/weakness producer. `00.Core/LogicalContracts.cs`, `00.Core/LearningMachineContracts.cs`, `10.LoRA/LoraLinear.cs`, `11.DPO/DpoTrainer.cs`, `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs` — входы, не исчерпывающая карта. Не приписывай наличие реализации имени файла.

Skills в K/docs/skills, с actual load/ref/span: **S**=`reference-inventory-and-source-map-SKILL.md` + `authority-requirement-binding-SKILL.md`; **P**=`phase-planning-SKILL.md` + `plan-review-SKILL.md`; **A**=`trainable-architecture-decision-SKILL.md` + `human-readable-architecture-audit-SKILL.md`; **C**=`generated-code-review-SKILL.md` + `code-examination-SKILL.md`; **E**=`prompt-patch-authoring-SKILL.md`; **M**=`experiment-design-SKILL.md` + `improvement-measurement-SKILL.md`; **H**=`output-readability-gate-SKILL.md`. Startup/intent — по текущему execution context. Отсутствующий/переименованный файл сверять с tree, не выдумывать чтение. Исторические 30/50 counts не увеличивают текущие 25.

## 3. Сохраняемая архитектура

Unit принимает domain, input-vector/output schemas и выдаёт typed many→many requested output type с associations. Flow корректирует отношения/transport/transformation между Units. Orchestrator выбирает/объединяет async combinations с dependency order и ценой всех ветвей. Graph учит отношения паттернов разных порядков и переносит композиции. Actual ILogical*/Machine сопоставляются поведением; Train остаётся ILearningMachine; Graph=L5 specialization, не шестой уровень/четыре trainers.

Сохранять TensorCore/shared base/project LoRA, StackDetector-before-normalization, semantic compression, BM25+kNN, positive/negative/warning memory, final arbiter и semantic OrderedPatternIds до явной предметной миграции. Winning solver не автоматически learning recipient. Детерминированное вычисление learned weights не опровергает обучение; случайность не доказывает его. Correct deterministic candidate не negative по происхождению.

CODEX/Astra 6 ultracode выполняет operational human role: выбор разрешённого weakness/урока и отдельную оценку exact pair. Допущенная открытая китайская модель пишет обе final candidate texts; CODEX не переписывает их под чужим provenance. Student=XIIGEN; DPO reference, pair judge и independent post-update evaluator — разные роли. Actual invocation фиксируется, alias не доказательство вызова. One-pair/no-repeat/full-state accept-or-rollback сохраняются; bulk — окно новых уроков, не повтор старых пар. S04 one-update-total не вечный лимит S06 campaigns.

## 4. Десять полных карточек A–J

### A — полнота исходного механизма

**Вопрос/skills:** что требуется до выбора узкого implementation seam; S/P/H. **Вход:** original task/DPO algorithm, оба назначенных WorkLeft, RAG subjects и actual trees §2.

**Prompt:** сопоставь требования, primary decisions и текущих owners; открой необходимые sections непосредственно. Раздели DIRECT_READ, HANDOFF_ONLY, NOT_READ, HISTORICAL. Проверь все девять RAG subjects, four-role semantics, DTC/IDE/learning obligations. Сохрани existing IDs и source authority; новое противоречие запиши обеими нормами, не выбери удобную молча. Широкую карту не подменяй чтением S04.

**Выход:** source/decision/owner map с распределением spans B–I и непокрытым остатком. **Хорошо:** назначен и прочитан MECH раздел о Graph, вывод отличается от README. **Плохо:** R04–R06 summaries заменили оба WorkLeft. **Край:** renamed root, cyclic RAG, historical единственный источник, разные refs. **Приёмка:** consequential claim опирается на primary либо явно не принят; нет неизвестной темы без owner. **Передача:** B–I/R10, не новый общий audit.

### B — четыре роли и many→many

**Вопрос/skills:** как сохраняется смысл через связь; A/C/P/H. **Вход:** A, original roles, actual contracts/callers/consumers и соответствующие MECH sections.

**Prompt:** восстанови nodes/edges, typed ports, cardinality/association keys, Flow correction, Orchestrator fan-out/fan-in/order/cancel и higher-order Graph. Для single-domain, mixed-domain и одного input с двумя lawful combinations покажи producer → state/decision → consumer. У каждой стрелки: input/output, schema/version, deterministic constraint, learned owner/signal, fallback, cost, evidence и falsifier. L2 selection→L3 execution проверь параметрами/association, не наличием двух ExecuteAsync. Semantic order не сортируй как IDs.

**Выход:** три walkthrough и connection map. **Хорошо:** 2→3 outputs сохраняют origin input и semantic order. **Плохо:** identical shapes объявлены совместимыми. **Край:** 1→0, partial fan-in, cycle, stale preload, lawful alternatives. **Приёмка:** различимы ошибка Unit и ошибка relation; TRACE_NOT_FINISHED не MISSING_IMPLEMENTATION. **Examination/handoff:** fixed Units/changed Flow и обратный → I/R12/R18/R22.

### C — законы доменов и representation

**Вопрос/skills:** какие различия сохранять в tokens/vectors; S/A/C/M/H. **Вход:** A/MECH/RESEARCH-03, original domain requirements, actual encoder/consumer, первичные законы предмета.

**Prompt:** для шести исходных Student subjects и Builder, плюс найденных extras, заполни law/source/assumptions, subject/intent/stack/version, necessary features/tokens, forbidden semantic loss, raw/normalized/TeachingVector/retrieval/output channels, consumer, coordinate versions, lawful positive/near-miss/unknown. Раздели mathematical law, API contract, пользовательское constraint и statistical heuristic. Oracle-domain и predicted-domain — разные условия. Не обещай единственный лучший tokenizer.

**Выход:** предметные representation passports и migration implications. **Хорошо:** negation/NULL/caller direction не исчезают до consumer. **Плохо:** float[] назван semantic vector без смысла координат. **Край:** RU/HE/mixed language, units, unseen domain, stale codebook, truncation. **Приёмка:** law→feature→consumer→контрпример прослежены. **Examination/handoff:** representation A/B с frozen downstream и compatible index → I/R04/R08/R13.

### D — domain-aware dynamic kNN

**Вопрос/skills:** когда retrieval полезен и что адаптируется; A/C/M/H. **Вход:** C, actual retrieval/BM25/index/consumer/arbiter, F authority primary и R06 источники непосредственно.

**Prompt:** для каждого domain family реши, нужен ли kNN. Раздели eligibility/pool, representation, metric/score, k/radius/confidence, family dedup, aggregation/reranking и final authority. Предложи форму `policy_d(x, evidence_density, ambiguity, budget, state)` с определёнными входами, выходами и trainable/fixed параметрами; не подставляй непроверенные «лучшие» константы. Проверь exact/ANN assumptions, index version/reindex, missing features/zero norm, роль positive/negative/warning memory. Первый hit и другие evidence IDs не равны агрегации лучших решений.

**Выход:** candidate function/owner/update signal, index contract и один дешевый falsifier на family. **Хорошо:** при fixed pool меняется learned reranker, его эффект измеряется отдельно. **Плохо:** больше k автоматически лучше; любой query-dependent score совместим с ANN. **Край:** tiny/empty pool, k>count, near-duplicate density, wrong dialect, incompatible coordinates. **Приёмка:** mapping/retrieval/consumer отделены; optimum не заявлен. **Examination:** same corpus/representation function A/B, exact/ANN отдельно → I/R06/R12/R13.

### E — DPO-пара, objective и semantic recipient

**Вопрос/skills:** чему именно и кого учит exact pair; S/A/C/M/E/H. **Вход:** original DPO algorithm/RESEARCH-03/current RUN, actual admission/trainer/envelope, R05 CP2 primary.

**Prompt:** проследи weakness → CODEX lesson selection → Chinese-authored sides → exact CODEX judgement → tokens/masks → objective → mutable state → accept/rollback → checkpoint → ordinary consumer. Назови response/action space, policy/reference, recipient/parameter group и влияние serialization/length/masks. Проверь Compact/480-char/whitespace findings в текущем blob; не объявляй все каналы повреждёнными по одной трассе. Conditional whole-response likelihood и mean prompt+answer не объединяй без решения objective. При policy=reference нулевой margin сам не делает пару плохой. Другой ranking/vector objective не называй standard DPO по названию метода.

**Выход:** source-to-update и update-to-consumer map, точный семантический contrast, два bounded future fixes при подтверждении pair-loss/objective; existing trainer переиспользуется. **Хорошо:** routing pair меняет selector, а не автоматически winner solver. **Плохо:** паспорт raw pair есть, значит optimizer получил тот же смысл. **Край:** TIE/BOTH_BAD/UNJUDGEABLE, all-masked, identical sides, duplicate family, unknown prior effect. **Приёмка:** pair validity, admission, update и utility разделены. **Examination:** same memory/context no-update/update на unseen families → I/R05/R12/R18.

### F — LoRA, проекция и ordinary adoption

**Вопрос/skills:** какое обучаемое изменение применяет обычный запрос; A/C/M/H. **Вход:** C/E, actual LoraLinear callers/parameter groups, checkpoint/version/load/publication и ordinary entry.

**Prompt:** зафиксируй base/shared/project/adapter topology, freeze masks, trainable parameters, input tokens/masks, reference identity и projection semantics. Различи representation vector, retrieval vector, LoRA parameter delta и graph state. Если output должен перейти в доменные векторные токены, укажи output schema/codebook/target/projection objective и реального consumer; LoRA сама по себе не tokenizer. Проследи accepted child→trusted preload→обычный новый запрос, не только trial evaluator reload. Coordinate update требует совместимости индекса или именованной migration. Сохранение файла не adoption.

**Выход:** parameter/representation/version/ordinary-consumer passport и точный остаток. **Хорошо:** новый adapter загружает тот же product caller на новых задачах. **Плохо:** hash изменился — значит лучшее vectorization. **Край:** first-step одна matrix ещё без delta, stale adapter, concurrent reads, incompatible index. **Приёмка:** learned state назван и потребляется либо gap точно локализован. **Examination:** no-adapter/frozen/adapted при одинаковых inputs и task quality, не только vector similarity → I/R04/R05/R18.

### G — причинное взаимодействие deterministic и learned

**Вопрос/skills:** какой блок отвечает за эффект и не дублирует ли другой; A/C/P/M/H. **Вход:** B–F и actual arbiters/resolvers/feedback consumers.

**Prompt:** классифицируй Stack/domain detection, normalization, BM25, kNN, dynamic k/radius, Unit, Flow, Orchestrator, Graph, DPO/LoRA и final fallback: HARD INVARIANT / LEARNED AUTHORITY / LEARNED SCORE+DETERMINISTIC RESOLVER / EVAL-ONLY / DUPLICATE-DEAD / MISSING CONNECTION. Каждую классификацию докажи owner/signal/state/consumer. Global reward не копируй всем edges без credit objective. Для двух совместно работающих механизмов предложи targeted 2×2 comparison, а не обязательный полный factorial sweep. Pure deterministic, learned-only и hybrid сравниваются честно; лучшая стоимость возможна и у простого baseline.

**Выход:** causal mechanism map, границы полезного helper и learned authority, reuse/removal/migration proposals. **Хорошо:** hard type filter сохраняется, learned selector выбирает среди lawful edges. **Плохо:** deterministic helper назван learner или убран только потому, что не обучается. **Край:** mandatory skill вне top-k, одинаковые outputs разных paths, скрытый memory gain. **Приёмка:** каждый предполагаемый effect имеет изолирующее вмешательство/falsifier. **Передача:** I/R12/R16/R18/R20.

### H — компактность на ПК и continual learning

**Вопрос/skills:** помещается ли вся система и продолжает ли полезно учиться; A/M/P/H. **Вход:** B–G, actual state/memory/feedback/recovery, WorkLeft campaigns и доступный cost evidence.

**Prompt:** задай measurement envelope: base/adapters, gradients/optimizer/activations, indexes/graph/cache, evaluator, checkpoint/temp space, concurrent residency, persistent growth, train/eval/inference/reindex/reload/rollback time. Declared hardware не VERIFIED_HOST. Считай все attempts/branches и remote teacher cost. Для feedback опиши source authority/privacy/project boundary/conflict/quarantine, pre-update prediction, новый разрешённый урок, full-state commit/rollback и следующий ordinary consumer. Нельзя второй update при неизвестном исходе первого. Retention exam не разрешение replay старых пар; S06 campaigns переиспользуются.

**Выход:** PC/cost/continual-learning specification и smallest valid first-lesson route. **Хорошо:** маленький adapter плюс большой index учитываются вместе по actual overlap. **Плохо:** размер LoRA файла доказывает PC fit; user correction всегда истина. **Край:** poisoned/mistaken feedback, domain drift, storage growth, snapshot дороже update, graph не на пути первого CodeDocumentation lesson. **Приёмка:** feasibility пока NOT_MEASURED; ни Graph, ни все domains не искусственный prerequisite первого независимого урока. **Examination/handoff:** equal-quality cost/equal-budget quality, retention/transfer → I/R09/R23/R24.

### I — causal examination и интеграция R07

**Вопрос/skills:** какие наблюдения подтвердят или опровергнут замысел; M/A/P/E/H. **Вход:** результаты A–H, текущие evaluation contracts, primary laws и отдельные source families.

**Prompt:** собери minimum contrasts: same-memory no-update/update; fixed pool/changed selector; fixed selector/changed domain pool; frozen Units/changed Flow и обратный; frozen relations/changed Orchestrator; primitive/higher-order compositions; representation A/B frozen downstream; kNN A/B same corpus; evaluator-only adoption/ordinary adoption; targeted 2×2 для пары взаимодействующих механизмов. Это каталог различающих опытов, не требование выполнить их все до первого урока. Ранжируй по первому uncertainty и цене, сохрани последующих owners.
>
> У каждого опыта укажи hypothesis, fixed/changed, baseline/challenger, domain/family, independent holdout, lawful positive/near-miss/tie, semantic/path/domain metric, time, memory/storage, retention, practically meaningful margin до данных, uncertainty unit и falsifier. Семейства, а не N−1 почти одинаковые пары, определяют независимость. Bulk-to-bulk рост на меняющихся более простых панелях не gain. Запрошенная Astra/ultra judgement дополняет измерения и primary laws, не заменяет их; actual model/invocation записать. Итог IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE, не общий PASS по coverage.

**Выход:** заполненная examination matrix и целостный human-readable architecture summary; source→requirement→mechanism→experiment→consumer. **Хорошо:** широкий интервал даёт INCONCLUSIVE и конкретное следующее наблюдение. **Плохо:** все adapters/память/данные изменены, gain приписан Flow; judge score равен доказанному закону. **Край:** ceiling, disagreement, multiple comparisons, fixed panel leakage, dropped failures, только best branch cost. **Приёмка:** ни одна A–H тема не исчезла; current/proposed/measured раздельны; полный R07 не закрывается только двумя стыками. **Передача:** R10/R12/R13/R18/R21/R22/R23/R24 с точными receipts.

### J — итоговый causal synthesis, первый урок и независимый reviewer

**Роль/skills:** архитектор-интегратор и reviewer-planner; P+A+C+E+M+H из §2, governing `R07-REPAIR-WORK-ORDER.md` §§14(R07-J), 19.7–19.11. **CURRENT TASK:** принять A–I без переписывания их первичных результатов, выбрать наименьший достаточный путь к реальному первому обучению и назвать самостоятельные остатки всей архитектуры. **NOT THE TASK:** запустить обучение, CODEX/Astra или Windows; объявить первую готовую подсхему завершением всех четырёх ролей; изготовить reviewer PASS.

**Контекст и прямые ссылки:** точные source/decision/code/limitation rows A–I; R01–R06 свежие identities только для семантически зависимых решений; исходный DPO алгоритм и WorkLeft MECH/RESEARCH-03; current R12/R13/R18 при наличии; skills и original requirements из §§1–3. Не ждать статуса DONE всех соседей; unresolved question имеет owner, span и влияние на первый урок.

**Подробный prompt:** построй одну причинную диаграмму input→domain/laws→representation→Unit many→many→Flow correction→async Orchestrator→higher-order Graph (где реально вызывается)→typed ordinary output→weakness/feedback→урок CODEX→обе final стороны от разрешённой Chinese open model→отдельная CODEX оценка exact pair→разрешённый Student/ILearningMachine update→полный versioned adopt/rollback→следующий обычный вызов. Для каждой стрелки укажи owner, signal, legal deterministic guard, learned state, actual consumer, evidence scope и falsifier. Сравни не меньше двух первых кандидатов, выбери точный semantic recipient и минимальный gap для R18; укажи домен/family, обучаемые параметры, DPO рецепт, tensor/token route, domain-conditioned retrieval-кандидата только при его присутствии, независимый parent/N/U panel, PC budget с failed branches, retention и условия остановки. Отдели исправление программного пути L→N от эффекта обучения N→U. Опиши двенадцать синтезируемых результатов J и вердикты из governing §19.11.

**Good:** исправленный learner использует ровно одну admitted pair, parent и child сравниваются на замороженном невиданном наборе при том же ordinary code, а графовые способности остаются самостоятельным OPEN, если graph не участвует. **Bad:** успешная документация кода считается доказательством learned graph path; возникло самодельное новое DPO authoring; Astra назван вызванным без trace; всё откладывается до Windows/full coverage. **Edge:** equivalent DPO sides, TIE, two lawful paths, mixed/unknown domain, wrong state owner, async branch timeout, lost associations, incompatible index, near-OOM PC, unknown optimizer outcome, feedback conflict, no-update improves due to code repair.

**Выход и проверка:** causal map со ссылками на actual code/primary или явным NOT_ESTABLISHED; chosen/rejected first-teaching alternatives; минимальный first-blocker; prospective N/U experiment; roll-back/cost plan; unfalsified/contradicted claims; owner handoffs R10/R12/R13/R18/R22/R23. Отдельный independent Astra UltraCode review считается только с реальным invocation/model/candidate identity/output; иначе `INDEPENDENT_REVIEW_NOT_PERFORMED`. Ready-first-teaching не означает whole-system DONE. Child получает полный bounded контекст, skills, приёмку, примеры, права и return-only envelope; не пишет sibling/shared artifacts.

## 5. Нагрузка, подзадачи и результат

Сначала source/decision map A, затем B–H тематическими checkpoints и I как синтез, без выдуманных независимых агентских запусков. Не делить по круглому числу файлов; делить по одному причинному вопросу и связанным spans. Внутри одной сессии части остаются частями R07; при реальном дополнительном call записать actual count/model/worker и полное work order. Если ограничение контекста не позволяет завершить тему, сохранить полезный result и конкретный residual/owner/capacity; не объявлять R07 DONE и не отправлять весь остаток R10/R25.

Каждый child получает тему, not-task, source refs/spans, skills, current→target, actions, output, good/bad/edge, documentary acceptance и будущий experiment из своей карточки, общий four-role/pair-authority anchor и return-only границу. Пустая A.1 карточка не допустимое делегирование. Дочерний отчёт и parent self-check не два независимых подтверждения.

Пять carriers сохраняют source/read-depth, findings, proposals, change-map и log. Origin=(R07,local ID,checkpoint) → primary ref/blob/span → meaning/limits → конкретное изменённое поручение R10/R12/R13/R18/R21–R24. Перечисление ссылок не semantic receipt. Новая sibling delta изменяет только зависимую часть. Сохрани опубликованные полезные traces; old bounded completeness не full-scope completion.

В итоговом ответе по-русски сначала объясни механизм и nearest first-lesson path, затем что реально существует/прослежено/не совместимо, какие comparisons способны это проверить и что остаётся неизвестным. Не заявляй PC fit, улучшение или проведённую Astra экспертизу без наблюдений.
