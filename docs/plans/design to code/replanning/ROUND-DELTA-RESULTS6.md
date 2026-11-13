# RESULTS6 — адресные поручения всем 25 запускам

**13.10.2026.** Это уточнение уже существующих R01–R25 по опубликованным R01–R06, не новая очередь и не выполненные исследования. Структура **10+5+5+5** сохранена. R01–R06 работают одновременно; поздние результаты принимаются без reset. R15/R25 — отдельные предусмотренные reviewer sessions, не выдуманные проверки автора.

**Как пользоваться:** dispatcher берёт common и полную карточку назначенного Rxx из текущих MODEL-RUN-PACKETS/самостоятельных R05/R06, затем встраивает **только одноимённую дельту ниже**. Содержательные обязанности старой карточки сохраняются, новые конкретные inputs заменяют уже устаревшие общие вопросы. Пользователь и child получают один полный effective prompt, а не список книг для склейки. Весь этот файл каждому worker не загружать.

## Общие инструкции, обязательные для effective prompt

**CURRENT TASK:** выполнить только назначенное исследование, анализ или изменение плана. **NOT THE TASK:** выполнять весь проект, запускать trainer/Windows/Actions, создавать product tests или считать подготовленные prompts готовым обучением.

P=`lubochka/xiigen-planning:llm_plan3`; D=`llm mvp/design to code session`; G=`llm mvp/dpo legal build`; V=`llm mvp/visual studio & visual studio code continue`; F=`llm mvp/llm dynamic flow`; K=`lubochka/llm_mvp_core:codex/dpo-implementation`. Source-route aliases не заменяют exact path/ref/blob/section. Фактические адреса кодовых seams — в worker SOURCES, особенно [R06/SOURCES](results/R06/SOURCES.csv). Нельзя считать поиск по default branch доказательством нужной ветки.

**Сохранить:** все исходные prompts/WorkLeft/LeftToWork/RAG, S00–S10/extras, DPO/DynamicGraph + DTC + IDE, seven baseline domains и дополнительные требования. Unit typed many→many, Flow relationships/correction, Orchestrator async combinations, Graph higher-order relations; Train у Machine, Graph=L5 specialization. Сохранить TensorCore/shared base/project adapters, semantic order, domain/intent/stack distinctions, BM25+kNN/positive-negative-warning memory/arbiter. Это не четыре новых trainers и не новый API из-за aliases.

**DPO policy:** final chosen/rejected пишет допущенная китайская модель; CODEX/Astra выбирает разрешённый урок и отдельно оценивает exact pair; Student response — возможный context, не обязательный final rejected. Correct deterministic answer не negative по происхождению. Material rights/semantic preference/optimizer eligibility/actual update/learning utility разделяются. One-pair/no-repeat/full-state/accept-or-rollback сохраняются; bulk — наблюдения новых разных уроков. Ноль standard margin при policy=reference не плохая пара. LoRA не tokenizer; similarity не DPO.

**Навыки:** A=`trainable-architecture-decision-SKILL.md`; P=`phase-planning-SKILL.md`; Vp=`plan-review-SKILL.md`; C=`code-examination-SKILL.md` и применимый `generated-code-review-SKILL.md`; E=`prompt-patch-authoring-SKILL.md`/`context-package-design-SKILL.md`; H=`human-readable-architecture-audit-SKILL.md`; S=`reference-inventory-and-source-map-SKILL.md`/`authority-requirement-binding-SKILL.md`; M=`experiment-design-SKILL.md`/`improvement-measurement-SKILL.md`. Пути относительно K/docs/skills. Загружать нужные actual sections, записывать что прочитано; имена не доказательство load. Текущие 25 и шесть parallel workers не заменяются историческими 30/50 последовательными routines.

**Права:** read/search, анализ existing sources/code/logs/diffs и documentary writes. R01–R15 — только собственные `results/Rxx/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. R16–R25 также назначенные active planning files внутри D; outside-D/core изменения только явно неприменённые proposals. Не переписывать sibling results или historical source bodies. Не менять code/tests/workflows/weights/datasets/core skills; не запускать product tests/coverage/build/trainer/paid generation/Windows/Actions; existing checks не отключать. Fresh bytes/SHA, non-destructive reconciliation, remote readback; no force-push.

**Output каждого Rxx:** конкретное сохранённое ожидание → source/глубина чтения → actual/proposed механизм → наблюдаемый разрыв/неизвестное → meaningful counterexample → способ будущей examination → next owner и semantic receipt. Documentary self-check не independent review и не runtime gain. Факт кода в sibling report указывать как атрибутированный, пока не прочитан primary. Для эксперимента использовать [ALGORITHM-EXAMINATION-RESULTS6](ALGORITHM-EXAMINATION-RESULTS6.md), только назначенные разделы.

**Нагрузка:** A/B — две части одного назначения с полезным checkpoint после A. Сначала закончить один центральный вопрос/маршрут, не читать все большие тела ради количества. Реальный child получает полный common, именно свою A/B, source subset, outputs/examples/edge/verification и return-only права; parent интегрирует. Дополнительные calls считать явно. Независимый reviewer — фактический отдельный вызов; requested Astra Ultra и actual invocation различаются. Отсутствующий критический источник ограничивает конкретный вывод; unrelated unread tail не блокирует весь проект.

---

## R01 — закончить карту, не начинать её заново

**Context/skills:** собственные F01–F13/visited set; SOURCE-ROUTES; actual P/K trees; D/WorkLeft INDEX/REDATE/MASTER и original/task/goal; R03/R04/R06 новые roots. S/P/E/H. Examination document §§1,10.

**Prompt A:** продолжи незакрытые source families по сохранённым refs. Различи human design documents, generated teaching corpora, source-examination-cache и live core; огромный generated tree не повод перечитывать каждый файл как отдельное design decision. Для каждой current RAG-ветви и WorkLeft family сохрани root/primary entry, глубину чтения и owner. Новые branches/extra slices/outside-D obligations не исключай по отсутствию в первой таблице.

**Prompt B:** перераспредели только реальный остаток: expectations R02, primary lineage R03, representation R04, trainer/state R05, routing R06, connections R07, domain laws R08, process R09. Не перегружай R07/R10 содержательным чтением всех мегабайтных WorkLeft.

**Выход/приёмка:** existing source IDs + конкретный unread span/question/owner/capacity; каждое original requirement имеет маршрут. Метаданные не READ. **Good:** cached core помечен snapshot и заменён live code только для актуального claim. **Bad:** все generated MD=прочитано; S04=весь проект. **Edge:** cyclic RAG, одинаковые basename, moved root, sole historical primary. **Далее:** R03/R10; будущая эффективность не измеряется количеством mapped files.

## R02 — сохранить решения и уточнить настоящие конфликты

**Context/skills:** existing 52 IDs/CHANGE-MAP, own review; D/original task/task prompt/Алгоритм создания ДПО; current RUN §§2.1–2.3; WorkLeft owner decisions; R03 F17–F23. S/A/P/Vp/H. Examination §§2,4.

**Prompt A:** проверь только неоднозначные/изменённые требования. Сохрани исходные ID, scope и authority. Explicit Chinese authorship обеих final sides и CODEX assessment не отправляй обратно в выбор R12. Не применяй нерелевантное правило из другого продукта только по слову DPO.

**Prompt B:** подготовь отдельные пары конфликтующих норм: pre-update margin; unit recipient versus solver/edge label; one-pair versus replay; helper ban versus hybrid; semantic order versus presentation. Для каждого — две exact source стороны, affected consumer и что именно должен решить R12; resolved не переносить в OPEN.

**Выход/приёмка:** source-bound existing requirements и адресные decisions; каждый перенос сохраняет user capability. **Good:** авторство resolved, admission остаётся отдельным вопросом. **Bad:** новый набор52ID или вынужденный rejected=Student. **Edge:** valid deterministic answer, two equally good answers, misleading historical heading. **Далее:** R05/R08/R12/R16; examination — semantic pair/recipient contract, не coverage.

## R03 — пройти RAG к смыслу, не к ещё одному перечню

**Context/skills:** own CP08/visited set; D/WorkLeft RAG MD/NDJSON/MASTER/usage; V/RAG-06→F/DECISIONS-LOCKED и ARBITER-KNN-AUTHORITY-AMENDMENT; G/plan07; R04/R06 exact receipts. S/A/C/H. Examination §§1–5.

**Prompt A:** сохрани F01–F23 и продолжи unread consequential routes. Минимум один содержательный primary transition на каждую current RAG-ветвь и все transitions, от которых зависит критическое решение. Не ставь общий адресный аудит перед чтением полезного смысла. Различай navigation RAG и runtime retrieval.

**Prompt B:** передай конкретное meaning: raw-word/TaskSignal seam; semantic order; recipient/fan-out; память positive/negative/warning; helper/authority; one-pair conflict. Старые численные claims/ready labels не повышай до fresh measurements. Новую находку отправь одному механическому owner с exact question.

**Выход/приёмка:** source graph + semantic receipt/остаток; F20/F21 не теряются при merge. **Good:** RAG→primary D10/D15→current consumer question. **Bad:** parse NDJSON=восстановлена архитектура. **Edge:** broken locator, overwritten merged source, generated corpus, CP07 finding absent from shorter summary. **Далее:** R04–R08/R10; source completeness и learning gain раздельны.

## R04 — сохранить смысл, координаты и many→many payload

**Context/skills:** own F01–F10/G01; current `KnnLogicalUnit`, compilation/normalization/vector builders по SOURCES; direct S00–S03; R05-F03; R06-F07. A/C/S/M/H. Examination §§2,3,5,8.

**Prompt A:** не повторяй уже доказанный BPE/kNN и не приписывай ему локальный словарь другого TeachingVector. Для changed representation проследи конкретный consumer: raw, canonical, vector или лишь scalar weighting. Подготовь контрасты negation/quantity/script/binding/order с unchanged downstream и покажи, какие признаки реально доступны.

**Prompt B:** передай R07 только G01 dereference/associations и Zip/empty boundary. Для evaluator передай R13 фактическое различие numeric gate, character/exact score и semantic rubric. Существующий queryable/materialization сохранить; новый generic interface без actual gap не проектировать.

**Выход/приёмка:** representation identity/meaning/consumer и один заполненный contrast packet; unresolved consumer не объявлять отсутствующим. **Good:** loss одного raw distinction прослежен до конкретного scorer. **Bad:** 60 dimensions=общая семантика всех pipelines. **Edge:** Cyrillic/Hebrew, equal-length incompatible codebooks, legitimate 1→0, 2 inputs/3 outputs. **Далее:** R05/R06 compatibility, R07 payload, R13 semantic examination.

## R05 — закончить update→ordinary consumer, не переписывать DPO

**Context/skills:** own CP1/F01–F08; `run-prompts/R05-PARALLEL6.md`; current S04–S06/RUN; G/plan07 §§3.1–3.2a; WorkLeft teaching/INTEGRATION-01; `DpoTrainer`, admission/projector/endpoint/coordinator/LoRA actual paths. A/C/S/M/H. Examination §§3,4,6.

**Prompt A:** продолжи от уже прочитанного Step/backward/AdamW. Проведи assessed pair→recipient envelope→named parameter group→snapshot/admission/reference/masks→update. Отличи semantic assessment от pre-update margin. Найди actual LOW_CONTRAST consumer либо зафиксируй plan-only норму; существующий Step не объявлять отказавшим по margin, которого в нём нет.

**Prompt B:** проследи accepted candidate/manifest→publication/load→ordinary consumer и freeze/injection LoRA. Установи первый missing seam, либо TRACE_NOT_FINISHED с точным следующим caller; не превращай отсутствие доказательства в заказ реализации. Pending applied lesson сначала заканчивает evaluation, не повторный gradient.

**Выход/приёмка:** source-to-state и state-to-consumer traces; один bounded next packet R18 с оставшимися dependencies. **Good:** named adapter действительно читается ordinary endpoint. **Bad:** новая DPO библиотека, loss=domain accuracy, changed hash=полезность. **Edge:** identical policy/reference, both sides equivalent, empty mask, unpublished child, unknown effects. **Далее:** R07 только host bridge, R12 decisions, R13/E1, R18.

## R06 — dynamic kNN только вместе с его потребителем

**Context/skills:** own checkpoint B/F01–F14; `run-prompts/R06-PARALLEL6.md`; R06/SOURCES K01–K25; direct S07–S10/WorkLeft P0–P6/PT; V RAG→F primary; R04 representations. A/C/S/M/H. Examination §§2,3,5,7.

**Prompt A:** сохрани existing exact/ANN/BM25/registry/pipeline/L3 executor. Для выбранного domain route разложи pool, features, score, search approximation, k/radius, consumption и arbiter. Покажи, что top-k в текущем KnnLogicalUnit возвращает top-1 output; не обещай обучаемую агрегацию от изменения k. BruteForceIndex cosine не custom-score backend.

**Prompt B:** передай R07 только checkpoint-to-preload и ordinary L2→L3 coupling, R12 — actual L5 activation/helper-ban decision. NOT_ACTIVATED у конкретного Machine не означает отсутствие всего graph runtime. Предложи один same-pool contrast и отдели mapping error от retrieval/solver error.

**Выход/приёмка:** algorithm/consumer card и exact residuals; bounds all branches/index migration. **Good:** gain first-hit recall отдельно от learned selector. **Bad:** keyword profile=DPO authority; флаг IsLearned=обучение. **Edge:** unseen/mixed domain, empty pool, stale index, same-sized coordinates, approximate hit changes. **Далее:** R07/R12/R13/E3/R22.

## R07 — два ограниченных исследования связей

**Context/skills:** R04 G01/F06–F08; R05 F05/F08 и свежий остаток; R06 F08/F09; K Logical/DynamicGraph contracts; R06/SOURCES K11–K16; WorkLeft MECH/RESEARCH-03 относящиеся interfaces. A/C/P/M/H. Examination §§2–3,7 E4.

**Prompt A — payload/association:** начни с `LogicalUnitDynamicGraphNode` projection, продолжи OutputRefs storage/resolution до requested typed payload. Проверь input→output association на 2 inputs/3 outputs, Zip cardinality, semantic order, версию и legitimate empty/abstention. Верни chain с exact symbols и первый неподтверждённый hop; не изучай заново весь vectorizer.

**Prompt B — state/authority/flow:** используй established runtime→resolver→pipeline и L3 executor. Найди ordinary host caller, который связывает trained checkpoint/preload, L2 decision и фактически исполняемый L3 flow. Назови кто выбирает, кто исполняет, кому адресуется DPO и чьи параметры изменяются. Deep optimizer остаётся R05. Передай R12 одну несовместимость или доказанный reusable bridge.

**Выход/приёмка:** два конкретных trace/packet в своих carriers, не пять новых общих аудитов. **Good:** fixed Units + changed relation имеет observable downstream comparator. **Bad:** рядом существуют ExecuteAsync — значит они соединены. **Edge:** lost OutputRef, equal type/wrong version, changed completion order, detached selected target. **Future examination:** E4 при одинаковых Units/budget и отдельный reverse intervention; эффект всех изменений одновременно не attribution. **Далее:** R12/R13/R18.

## R08 — законы предметов и реальные families

**Context/skills:** WorkLeft RESEARCH-01/REVIEW/RESEARCH-03 baseline; R02 requirement matrix; R03 source routes; R04 domain table/F10; существующие DOMAIN-EXPERIMENT-PLAYBOOK/RECIPES. S/A/M/E/H. Examination §§4,5,8.

**Prompt A:** для seven baseline subjects и обнаруженных extras восстанови текущие functional expectations и lawful/invalid near misses. На каждый закон укажи primary source/version/applicability; generic LLM confidence не источник. Для программных/языковых спецификаций используй подходящий первичный источник, а не выдуманный universal law. Не путай кодовые intents с учебными предметами.

**Prompt B:** распредели actual material по source families/rights/teaching/dev/evaluation/certification; variants одной задачи оставить одной family. В первую очередь полностью подготовь family для первого R13 опыта, остальные паспорта дополняются R21. Для Builder отдельны pair validity и последующее Student benefit.

**Выход/приёмка:** law→feature→contrast→metric→source-family карта, включая conflicting labels/unknown. **Good:** SQL row-preservation с dialect/schema; translation negation с допустимым парафразом. **Bad:** seven одинаковых text fixtures или split по UUID. **Edge:** ambiguous task, multiple correct answers, mixed domain, version-dependent law. **Далее:** R13 first recipe, R21 curriculum; data count не proof breadth.

## R09 — доказать стоимость процесса и настоящие blockers

**Context/skills:** WorkLeft INTEGRATION-02/TEST-BATCHES/DEC01/02/04/MATRIX/REPORT; existing workflow records/commit diffs; R05/R06 reuse seams. S/C/P/H. Examination §§1,9.

**Prompt A:** по ограниченной явно описанной выборке existing records раздели queue/build/environment/check/evaluation/repair/document work. Назови selection rule и missing telemetry. Не выводи человеческие часы из commit spacing или CI duration; не создавай новый Windows run для проверки гипотезы, что runs дороги.

**Prompt B:** проследи representative requirement→implementation→failed expectation→diff. Классифицируй legitimate fixture repair, changed requirement, weakened expectation и unresolved. «Все тесты подгоняются» не исходный вердикт. Для каждого переносимого gate покажи отсутствие причинной зависимости от выбранного first lesson и retained delivery owner.

**Выход/приёмка:** cost evidence/sample limits и dependency recommendations R11/R17, без invented totals. **Good:** native Windows seam отделён от unrelated packaging job. **Bad:** просто удалить workflows или назвать каждый fail бесполезным. **Edge:** mixed-purpose run, missing logs, flaky environment, shared runner queue. **Далее:** R11/R17/R23; успех — устранённая бесполезная зависимость, не меньше safeguards любой ценой.

## R10 — join шести snapshots, не шесть фиктивных DONE

**Context/skills:** PARALLEL-R01-R03-JOIN; current five carriers R01–R09, включая actual R04 result/R05 CP1/R06 B; SOURCE-ROUTES; only contested primary spans. S/P/Vp/A/H. Examination §§1–3,7.

**Prompt A:** зафиксируй каждый input как run/checkpoint/ref/blob/read_scope/status. Сохрани исходные IDs, unresolved remainder и attribution. Выпиши semantic receipt каждой consequential находке: что именно она меняет в R11–R25. Повтор одной source цитаты в трёх reports не три независимых доказательства.

**Prompt B:** собери один first-lesson chain по конкретным R05/R07 ports с R04 coordinates и R06 consumer semantics. Отдельно дай whole-program scope gaps. R05 IN_PROGRESS не мешает принять его доказанный Step trace, но не разрешает додумать ordinary consumer. Missing critical hop → один owner; unrelated unread archive → continued inventory, не общий запрет анализа.

**Выход/приёмка:** `FIRST_LESSON_SUBSET=SUFFICIENT/INSUFFICIENT` с условиями отдельно от whole-program completeness; readiness продукта не заявлять. **Good:** R04 character metric меняет R13 rubric и R17 criteria, receipt указан. **Bad:** copied findings без downstream edits или join only R01–R03. **Edge:** concurrent source change, missing carrier alias, new requirement, stale proposed schema. **Далее:** R11–R15, без автоматического J06/R26.

## R11 — два причинных пути и ранний выход к реализации

**Context/skills:** R05/R07 actual first seam, R09 costs, R10 sufficient subset, all requirement IDs; current slice queue. P/A/M/H. Examination §§3,7,9.

**Prompt A:** построить first-lesson DAG и full-delivery DAG. Для каждой дуги назвать required input/state/right/consumer, а не «так раньше шёл slice». Особо проверить, не поставлены ли полные R01/R03 traversal, все семь curricula, весь Graph lifecycle или Windows package перед независимым CodeDocumentation lesson без причины.

**Prompt B:** оставь у раннего пути compatible checkpoint, правильную пару/recipient/objective, state recovery, ordinary consumer и независимую оценку. Перенесённая обязанность сохраняет old ID, owner/trigger и milestone. Разреши R12/R13 работать по sufficient subset до закрытия unrelated tails; эта возможность не authorization code/training.

**Выход/приёмка:** minimum causal blockers и retained delivery queue. **Good:** UI остаётся в DTC/IDE delivery, отсутствующий consumer — реальный blocker. **Bad:** speed achieved by removing evaluation/state identity. **Edge:** actual native dependency, no accepted baseline, pending mutation, unavailable material. **Future examination:** E0/E1, не очередной full stack qualification. **Далее:** R12/R14/R17/R18.

## R12 — выбрать один минимальный hybrid вариант

**Context/skills:** R04 representation/evaluator, R05 update/state, R06 search/authority, R07 bridges, R08 laws, R11 DAG; G plan07/current RUN; F D10/D15 и R03 helper-ban primary. A/C/P/M/H. Examination §§2–7.

**Prompt A:** сравни reusable CodeDocumentation learner и discovered graph route как кандидаты первого результата. Выбери по фактическому missing seam/ordinary consumer/PC cost, не по престижу архитектуры. Дай выбранный owner, input/output/version, parameter group/loss/reference/masks, deterministic helpers, final authority и required migration. Существующий код переиспользовать; TRACE_NOT_FINISHED сначала дочитывается, не переписывается.

**Prompt B:** отдельно разреши cold-start admission, helper scope, recipient bridge, coordinate compatibility и k-consumer. Для каждой альтернативы объясни failure/benefit comparator. Unit/Flow/Orchestrator/Graph не схлопывать; неиспользуемые первым уроком роли получают последующий milestone, а не удаляются. Empirical best metric остаётся вопросом опыта, но target owner/ports/objective должны быть выбраны.

**Выход/приёмка:** один выбранный executable design и отклонённые alternatives, exact patch recipients R16/R18. **Good:** existing trainer + один missing loader bridge. **Bad:** «новая dynamic kNN+LoRA система» без изменяемой функции. **Edge:** non-differentiable selector, equal preferences, actual graph NOT_ACTIVATED, index migration. **Future examination:** E1 и один диагностический E2/E3/E4 по bottleneck; не весь sweep.

## R13 — полностью заполненный первый learning experiment

**Context/skills:** R12 selected seam, R08 first lawful family, R04 F09/F10, R05 ordinary consumer, WorkLeft research, existing evaluation contracts. M/A/E/Vp/H. Examination §§4,7–9.

**Prompt A:** заполни actual first recipe: domain/family/material IDs, parent/reference/representation/pool identities, existing endpoint или exact R18 missing hop, один challenger после одного разрешённого нового урока, matching-memory no-update control, evaluator role и blind/presentation-balanced rubric. Если pair уже применена, recipe продолжает evaluation, не повторяет update. Неподтверждённый actual field не заменяется правдоподобной командой.

**Prompt B:** до ответов выбери sample-family plan, primary semantic metric, retained domains, time/memory measures, practical thresholds/uncertainty/analysis points/budget. Proposed thresholds обозначь exploratory до утверждения; без них нет auto-promotion. Parent/candidate оцениваются на same cases. Нужен semantic rubric сверх character match и pre-update DpoMetrics.Accuracy.

**Выход/приёмка:** runnable-after-authorization recipe с no-gain/regression/invalid branches, не шаблон. **Good:** одинаково хорошие переводы признаются допустимыми; отрицание проверяется по смыслу. **Bad:** average bulk1 vs harder bulk2 или teacher видит certification answers. **Edge:** tiny sample, disagreement, cost increase, no-op update, dependent families. **Далее:** R15/R18; E0/E1 first, expansion R22.

## R14 — перестроить всю очередь по способностям

**Context/skills:** R01/02 original scope/IDs; R10/11 DAG; R12 design; R13 recipe; S00–S10/extras и WorkLeft current responsibilities. P/A/E/H. Examination §§1–3,7.

**Prompt A:** milestones: ordinary baseline/first valid lesson → structurally different domain → domain representation/retrieval → Flow/async/higher-order transfer → PC/feedback → remaining DTC/IDE delivery. Перестановка допустима только по причинным dependencies. Для каждого milestone measurable capability, reused state/code, missing seam и consumer.

**Prompt B:** old task→KEEP/REUSE/SPLIT/DEFER/REMOVE-DUPLICATE→new owner/trigger. REMOVE-DUPLICATE требует эквивалентной функциональной обязанности и evidence; «не в первом уроке» не значит delete. Сохрани вне-slice original requirements и running R01–R06.

**Выход/приёмка:** every old ID traceable, every new task justified; отдельные scientific/implementation/delivery outcomes. **Good:** one existing projection reused by two capabilities. **Bad:** rename S04 as all-program completion. **Edge:** one source task contains host+algorithm+data, missing external repo, historical sole requirement. **Далее:** R15/R19; будущая приёмка — способность, а не закрытая строка.

## R15 — независимая экспертиза направления и первого recipe

**Context/skills:** immutable R11–R14 candidate + sources и конкретные counterexamples R04–R08; requested Astra Ultra, actual model/effort/invocation фиксируются. A/Vp/M/C/H. Examination §§2–9.

**Prompt A:** как отдельный reviewer попробуй опровергнуть выбранный design: consumer не читает weights; gain memory-only; correct fixed answer wrongly rejected; semantic order/association потеряны; global kNN metric assumption неверно; LoRA работает в другом space; first margin gate блокирует cold start. Используй source/code/recipe, а не репутацию модели.

**Prompt B:** отдельно оцени sufficient narrow first-lesson packet и whole-program design gaps. При проблеме дай минимальный exact repair с owner и affected candidate identity; unrelated inventory tail не blanket embargo. Повторная проверка касается semantic changed candidate, не автоматически всех25.

**Выход/приёмка:** независимый attributable verdict и reproducible reasoning summary, не self-review автора. **Good:** конкретный leakage path и исправление panel boundary. **Bad:** APPROVED потому что текст длинный. **Edge:** model alias unavailable, stale candidate, no actual call, inconclusive evidence. При отсутствии invocation — NOT_PERFORMED. **Далее:** R16–R20 адресные repairs; efficacy остаётся NOT_MEASURED.

## R16 — применить исправленные требования в действующих потребителях

**Context/skills:** R02 existing IDs, R12 selected decisions, R15 review, R04 F09/F10 и R05/R06 specific contradictions; exact current active D planning files. P/S/A/Vp/E/H. Examination §§2–6,8.

**Prompt A:** для каждой принятой нормы составь source→old active text→replacement→downstream consumers. Начни с реально блокирующих first lesson: margin-vs-semantic-quality, recipient/state/ordinary consumer и evaluator meaning. Не меняй уже resolved authorship и не отменяй no-repeat.

**Prompt B:** примени разрешённые изменения active requirements внутри D и проверь readback/backlinks. Original/historical bodies сохраняются; outside-D locked source или core runtime не объявляй исправленными sidecar. Для них exact proposed patch/owner/impact, без ложного APPLIED.

**Выход/приёмка:** реальный diff и updated consuming prompts; неизвестное не спрятано в READY. **Good:** character score становится auxiliary в active acceptance, semantic rubric имеет owner. **Bad:** только новый advisory файл при прежнем current gate. **Edge:** concurrent edits, source outside scope, same norm in multiple entrypoints. **Далее:** R17/R18/R20; документная проверка, не запуск продукта.

## R17 — заменить доказательство обучения, не отключить защиты

**Context/skills:** R09 gate taxonomy, R11 two DAGs, R13 first recipe, R16 requirements, current S03/S04 acceptance/replanning evaluation docs. P/M/Vp/C/H. Examination §§7–9.

**Prompt A:** выпиши для каждого current criterion, что он доказывает: mechanism integrity, semantic learning, publication или host delivery. Перенеси unrelated Windows qualification с first-learning critical path по причинному основанию. Не создавай новые coverage targets или требование exact learned answer.

**Prompt B:** внеси active learning acceptance: same-panel semantic change, mapping/retrieval/path diagnostics, retention/full cost, uncertainty и IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE. Сохранённые deterministic protections не называть learning evidence; existing tests не удалять и skipped не PASS. Реальные schema/state/rights violations нельзя компенсировать average quality.

**Выход/приёмка:** every criterion связан с функциональным ожиданием и подходящим evidence. **Good:** numeric meter проверяет identity, экспертная рубрика оценивает смысл. **Bad:** «нет tests — нет проверки вообще» или retry-until-green. **Edge:** stochastic correct outputs, runtime native dependency, invalid family split, evaluator drift. **Далее:** R18/R22/R25.

## R18 — готовое поручение на первый недостающий участок реализации

**Context/skills:** R05 actual learner, R07 exact bridge, R12 choice, R13 filled recipe, R15 repairs, R16/17 active rules; exact current code files/callers/consumers. A/C/P/E/M/H. Examination §§2–7.

**Prompt A — implementation work order:** подготовь не code changes, а полный пакет будущему исполнителю одного выбранного seam: current task/non-task; exact repo/branch/path/symbols и whole-file read targets; existing producer/consumer; input/output/state versions; algorithm steps; named parameter group/objective; safeguards/recovery; allowed code files; forbidden redesigns; examples/edge; observables/stop. Если seam уже реализован, замени поручение на ближайший реально отсутствующий bridge или E0/E1 — не дублируй код.

**Prompt B — examination work order:** включи R13 recipe содержанием, ordinary entrypoint, persisted baseline/candidate identities, matching-memory control, permitted single update/recovery branch, domain rubric, full cost/retention, no-gain and invalid-evidence actions. Дай exact next handoff, а не «потом протестировать». Implementation/examination children при реальном делегировании каждый получают самостоятельный полный packet и отдельный счёт calls.

**Выход/приёмка:** один complete first-seam packet, который не заставляет исполнителя выбирать architecture заново; implementation authorization по текущему назначению отсутствует. **Good:** reused trainer + named load boundary + обычный ответ после candidate. **Bad:** создать второй trainer и full Windows campaign. **Edge:** stale SHA, missing caller, applied-once pending lesson, unknown permission. **Далее:** отдельно разрешённый исполнитель; не ждать R21 curriculum без причины.

## R19 — применить новую очередь без исчезнувших обязанностей

**Context/skills:** R14 old→new map, R16/17 changes, R18 first packet, actual active queue/README/next-slice planning consumers. P/S/E/Vp/H. Examination §§1–3,7.

**Prompt A:** измени действующую очередь внутри разрешённого D scope: dependencies по capability, REUSE отмечает existing code/evidence, DEFER имеет owner/trigger. Сохрани S00–S10/extras и все три delivery линии, включая browser/render/images/API/DB/user-tier cost/publication/IDE consumer.

**Prompt B:** проверь next-task entrypoints и ссылки: новый порядок действительно выдаёт first-seam packet, а не старый checklist. Не переименовывай running Rxx и не переписывай их evidence. Не превращай исходную условную очередь в preauthorization всех будущих effects.

**Выход/приёмка:** applied queue + zero orphaned requirement IDs в назначенном scope; непроверенные external surfaces перечислены. **Good:** preserved graph milestone follows an early code-doc lesson. **Bad:** новые названия без изменённых dependencies. **Edge:** duplicate queue versions, embedded old prompts, mixed tasks, external root. **Далее:** R20/R24/R25.

## R20 — выдать модели одно непротиворечивое поручение

**Context/skills:** AGENTS/START/PLAN/JOIN, MODEL packets, selected old ROUND-DELTA и RESULTS6, R16–R19 changes; current own results. E/P/Vp/H/S. Examination §10.

**Prompt A:** для R01–R06 выполни документный cold-start walkthrough: каждому доступны direct inputs без sibling DONE; для R07/R12/R13/R18 — result-bound route без повторного полного audit. Скомпилируй representative effective packets, сохраняя actual source/skill instructions, task/non-task, complete A/B, output, good/bad/edge, verification и права. Нельзя оставить «четыре workers» рядом с «шесть».

**Prompt B:** проверь late delta: new tokenizer→only dependent metric/consumer; new checkpoint→only its loader/evaluation; same source quoted twice→one evidence basis. Parent формирует полный child packet; пользователь не собирает книги. Exact current constraints выигрывают у исторических 30/50-pass routines, но уникальные requirements старых документов не исчезают.

**Выход/приёмка:** actual applied dispatcher/prompt fixes и примеры full effective packets, не только routing links. **Good:** R05 сохраняет CP1, продолжает ordinary consumer. **Bad:** новое общее исследование всех skills перед каждым seam. **Edge:** unavailable required model, missing source, context overflow, concurrent siblings. **Далее:** R21–R25/исполнитель по назначению; documentary walkthrough не independent model call.

## R21 — дополнение 1/5: полный domain curriculum

**Context/skills:** R08 laws/families, R02 IDs/extras, R04 meaning table, R12 representation/score candidates, WorkLeft research/DOMAIN recipes. M/A/E/S/H. Examination §§4–5,8.

**Prompt A:** заполни seven паспорта: EN↔RU; orchestration; Figma→HTML/CSS; C# documentation; Angular documentation; SQL documentation; Builder для этих шести. Добавь подтверждённые extras без смешения с intent/stack. В каждом: actual source/version, task families, input/output schema, lawful transformations, features/tokenization, candidate domain score/k policy, positive/negative/tie/unknown, independent rubric.

**Prompt B:** лестница new families→new structure→mixed domain→feedback/retention. Для каждого предмета минимум содержательный correct example, plausible one-law near-miss, equivalent answer и edge case; illustrative content не выдавать за approved training rows. Сложные композиции не заменить переименованием переменных.

**Выход/приёмка:** предметные различия влияют и на representation, и на examination. **Good:** SQL cardinality и translation negation оцениваются разными законами. **Bad:** одна cosine threshold для всех. **Edge:** multiple acceptable outputs, Hebrew script, responsive layout, versioned semantics. **Далее:** R22/R24; first E1 не ждёт всей книги.

## R22 — дополнение 2/5: опыты по алгоритмам и связям

**Context/skills:** R13 first recipe, R21 curricula, R04/05/06/07 intervention cards, R12 design, active acceptance. M/A/C/E/H. Examination §§5–8 E2–E6.

**Prompt A:** подготовь отдельные filled recipes: representation semantics; predicted/oracle domain; fixed/proposed distance и exact/ANN; k versus consumer aggregation; LoRA with frozen encoder; frozen Units/changed Flow и reverse; async branches; unseen higher-order composition. У каждого same panel, one intervention, preserved guards, actual state/entrypoint, falsifier и cost. Не весь Cartesian sweep до первого урока.

**Prompt B:** ограниченные interaction recipes только для выявленной зависимости, например projection×index или selector×solver; явно назвать невозможность single-factor attribution. Не повторять gradient старой пары в ablation branches под новыми IDs. Snapshot inference comparisons и legitimate новые lessons различать.

**Выход/приёмка:** каждый experiment может дать информативный negative/inconclusive, а не «должен улучшиться». **Good:** top-k evidence gain не объявлен output gain. **Bad:** tokenizer+k+weights+pool changed together. **Edge:** nonmetric score, stale codebook, budget asymmetry, shared families, no available control state. **Далее:** R23/R25/будущие допущенные experiments.

## R23 — дополнение 3/5: PC budget и безопасное продолжение обучения

**Context/skills:** R09 measured/unknown process costs, R05 state lifecycle, R12 architecture, R13/22 recipes, actual hardware/config/artifacts and user-approved constraints. M/A/P/C/H. Examination §§4.3,9.

**Prompt A:** установи фактическую целевую PC-конфигурацию из доступных sources; старые hardware notes пометь неподтверждёнными, не обещай fit. Раздели steady inference, peak training, checkpoint/reindex/adapter switch и branch overhead. Считай base/runtime/encoder/adapters/optimizer/activations/index/graph/cache/snapshots, а не только adapter bytes или current-thread allocation.

**Prompt B:** сформируй feedback loop: before-update output→meaning/rights/ambiguity review→new-family admission→permitted idle lesson→same-panel/retention→accept/recovery. User correction не truth по умолчанию; personal/global scopes и no-repeat сохраняются. Budget exhaustion не разрешает silent lower-quality model или повторные paid calls.

**Выход/приёмка:** actual/proposed resource budget, full measurement plan и complete feedback/resume packet. **Good:** рост index и стоимость неуспешных branches учтены. **Bad:** 2MB LoRA=всё помещается. **Edge:** long context, no free VRAM, concurrent foreground, pending update, malicious/conflicting feedback. **Далее:** R24/R25; PC-fit NOT_MEASURED до реального профиля.

## R24 — дополнение 4/5: полезность в обычных пользовательских маршрутах

**Context/skills:** R02 original DTC/IDE requirements, R14/19 queue, R18 ordinary seam, R21 domain passports, R23 scopes/cost. D/task prompt/goal; actual named external repo contracts only after read. A/P/C/E/H. Examination §§2–3,7,9.

**Prompt A:** подготовь конкретные DTC и IDE user-journey packets: input/tree/assets→processing→output/multiple resolutions or code documentation→assessment→stored weakness→allowed pair/update→ordinary next use. В каждой границе указать actual owner/type/version и observable user improvement, не только internal successful call.

**Prompt B:** personal/global/portability, local/offline constraint, user-tier cost, publication/load и wrong-version cases. External code не считать исследованным по ссылке в плане; missing access → exact boundary/owner. Полная доставка сохраняется, но не становится задним числом prerequisite независимого E1.

**Выход/приёмка:** два user-visible packet с concrete scope и referenced implementation seams. **Good:** next ordinary request действительно использует принятую версию. **Bad:** demo endpoint или screenshot=полный продукт. **Edge:** missing assets, different resolution, unsupported domain, stale model, another project scope. **Далее:** R25/будущая delivery реализация.

## R25 — дополнение 5/5: независимая экспертиза и точная передача

**Context/skills:** immutable final candidate всех изменённых active plan consumers; R15 verdict/repairs; R18 complete next packet; R21–R24; unresolved scope map/receipts. A/Vp/M/C/E/H. Examination §§1–10 по конкретным claims.

**Prompt A:** отдельным actual Astra Ultra reviewer проверь сохранение user expectations/four roles/all slices/WorkLeft, научную различимость опытов, model-ready completeness и causal first-lesson dependencies. Не требуй measured gain от ещё не исполненного recipe, но не принимай неизвестный owner/contract за готовый design. Review привязан к exact candidate; изменения не наследуют PASS.

**Prompt B:** выдай один готовый следующий implementation/examination packet и точный remaining work. Сверь R15 fixes, first-seam prerequisites, conditional extra work и все deferred delivery owners. Нужная небольшая правка получает bounded correction, не автоматический новый круг25. Если actual reviewer не был вызван, NOT_PERFORMED и pending independent review, не fictitious acceptance.

**Выход/приёмка:** attributable review, final transfer и explicit limits. **Good:** sufficient authorized narrow packet отделён от ещё неполного full-program audit. **Bad:** сам автор подписал independent PASS; prepared25=executed25. **Edge:** stale review SHA, partial source traversal, changed task scope, empirical inconclusive. **Далее:** конкретное отдельно разрешённое исполнение, без auto-R26.

---

## Дополнительная сессия — только по точному остатку

25 — базовый бюджет. Если после reuse/адресной передачи остаётся обязательный независимый вопрос, назначить `EXT-Rxx-<question>` с причиной, exact owner/capacity и тем же полным common/task/non-task/context/skills/source spans/algorithm steps/output/good/bad/edge/documentary acceptance/future examination. Указать which original requirement would be lost without it, какие существующие calls она заменяет/не заменяет и реальный дополнительный счёт. Не добавлять автоматически ещё один join или 30 review calls. Не скрывать extra model invocation под A/B/self-check.

## Контрольная матрица передачи, без нового ledger

- R04 semantics/OutputRefs/evaluator → R07 payload, R13 semantic metric, R17 active acceptance, R21 domain contrasts.
- R05 existing objective/update/LoRA/state → R07 host bridge, R12 selected owner, R13 E1, R18 minimal work order.
- R06 top-1/metric/authority/L2/L3 → R07 connection, R12 k-consumer/activation choice, R22 controlled retrieval/flow experiments.
- R01/R02/R03 source scope/original decisions → R10 receipts, R14/19 full queue, R16 active requirements, R21/24 preserved domains/delivery.

Эти передачи должны появиться смыслом в existing FINDINGS/CHANGE-MAP получателя. Ссылка на этот файл без изменённого вопроса/выхода не receipt. Результаты R01–R06 этим документом не редактируются и не повышаются в статусе.
