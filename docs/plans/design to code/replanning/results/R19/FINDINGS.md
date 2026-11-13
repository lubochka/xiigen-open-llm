# R19 — проверенный ремонт входов и точная граница доказательств

## R19-R2 · 13 октября 2026 · текущий результат

**Что изменилось для пользователя:** действующее R19 теперь задаёт четыре подробные, ограниченные части работы — authority, actual packet, сохранение алгоритмов, безопасная публикация. В JOIN устранены остаточные активные ограничения девятью/четырнадцатью starters. Двадцать R01–R20 могут начинать независимо; R10 по-прежнему объединяет девять исследований. Не требуется девять ненужных правок ради формальной приёмки.

Это **документный ремонт R19 и его собственных результатов**, а не исполнение всех R01–R20, реализация compiler/trainer, обучение или независимая Astra-экспертиза. Новых product tests/Windows/Actions/model calls в этой работе нет. Исходная научная цель сохранена: быстрее получить корректную implemented learning chain и затем измерять полезность новых уроков по доменам, времени, точности и полной стоимости PC.

**Снимки чтения:** P=`lubochka/xiigen-planning` @ `171a5485a32fb84e6c0c0e97a27e5a556b84d2c3`; K=`lubochka/llm_mvp_core` @ `3c7f5f8c7809936e25579603c96ea6df8598fc40`. Перед каждой записью проверяется current blob. Поздние sibling commits не заменяются нашим старым snapshot.

**История сохранена адресно:** [полный прежний FINDINGS со всеми C19/CE19 и подробными примерами](https://github.com/lubochka/xiigen-planning/blob/171a5485a32fb84e6c0c0e97a27e5a556b84d2c3/llm%20mvp/design%20to%20code%20session/replanning/results/R19/FINDINGS.md), blob `faf41fc013108c64799cb55ad590e5c7f85f8c61`. Ниже сохранены все прежние IDs, предметные решения и сценарии; исторический полный текст не считается новой работой. R2 исправляет переоценённую completeness, а не отменяет полезные архитектурные выводы.

## 1. Найденные дефекты и исправления — не гипотезы о качестве модели

| ID | Наблюдаемое основание на P snapshot | Исправление / точная граница |
|---|---|---|
| R19-R2-F01 | JOIN §1 ещё нормализовал старые указания в «девять независимых входов»; нижний current override называл latest instruction R01–R14, хотя шапка и PARALLEL20 разрешали R01–R20 | Исправлено в самом JOIN: двадцать starters, девять research inputs; R07-J refresh и direct-primary scope сохранены. Это исправление dispatch, не новый research. |
| R19-R2-F02 | Прежний FINDINGS называл CE paragraphs «проверкой полноты всех двадцати отдельных compiled packets», но сами paragraphs поручали dispatcher дополнительно загрузить COMMON/полные cards/skills | **Этот completeness claim superseded.** CE19 — полезные DOCUMENT_EXAMPLE, не materialized full-prompt receipts. PATH_RESOLVED, PACKET_CONTENT_VERIFIED и MODEL_DISPATCH_OBSERVED теперь различаются в R19. |
| R19-R2-F03 | CE19-20 принимал «9 live patches + five own carriers» как критерий; прежняя карточка не давала полноценной отдельной схемы каждой A–D части | Девять live targets — разрешённые владельцы, не минимальное число edits. Новая карточка содержит четыре подробных prompts с входами, навыками, действиями, output, good/bad/edge, правами и falsifier. Already-correct text получает NO_CHANGE, не косметическую перезапись. |
| R19-R2-F04 | WorkLeft-02 R16-insert содержит R12 REEXECUTION_REQUIRED; прямой R12 RUN-LOG уже фиксирует ARCH-2 A–F INTEGRATED DESIGN CANDIDATE / exact implementation NOT READY | В R19 закреплён snapshot-bound status resolution. Повтор всего A–F не выводится из старой source summary; runtime readiness не повышается. Изменение WorkLeft — **OWNER_PROPOSAL R16**, не будто уже принятая правка R16. |
| R19-R2-F05 | Прежний SOURCES имел единую R19-K01 строку с несколькими сокращёнными skill names и без отдельных blobs/spans | Прочитаны адресные excerpts шести реальных `*-SKILL.md`; новые source records раздельны. Старые source records сохранены как prior-run evidence; отсутствующие поля не выдуманы. Truncated return не FULL_READ. |
| R19-R2-F06 | Прежние CE охватывали R01–R20 и R25; явных отдельных R21–R24 сценариев не было | Actual nonrecursive run-prompts tree подтверждает все R01.md–R25.md; добавлены четыре continuation scenarios ниже. Это path/meaning routing, не 25 выполненных calls и не полное прочтение всех карточек. |
| R19-R2-F07 | Прежний RUN-LOG описывал planned one-commit publication, не содержал собственного проверяемого итогового receipt | R2 использует реально доступные sequential expected-blob writes, отдельный readback и фактические commits в RUN-LOG/финальном ответе. Историческое обещание одного commit не приписано этой сессии. |

### Ключевой научный смысл, который ремонт НЕ меняет

Детерминированное вычисление не противоположно обучению: обучаемое состояние может использоваться детерминированным алгоритмом. Случайность сама не доказывает обучение. Предмет исследования — **какое состояние изменилось, каким сигналом, кому принадлежит решение и улучшился ли обычный результат**, а не нестабильность отдельных ответов. Это сохранённое правило JOIN §4, не утверждение о проведённом эксперименте.

## 2. Прямое чтение целей и один проверенный RAG→primary маршрут

В этой сессии прямо прочитаны `original task.md` relevant body, `task prompt.md` и `goal of the round.md`, а не только S04. Original фиксирует Unit many→many, Flow correction, async Orchestrator и higher-order DynamicGraph. Task prompt сохраняет DTC: исходное дерево/HTML/CSS/скрины→server API→модель→рендеры разных размеров→оценка→данные урока→обновлённая модель. Goal сохраняет canonical DPO/weights, загрузку с диска, продолжение без повторного выполнения и необходимость обходить RAG. Их исторические команды реализации/удаления worktrees не исполнялись.

**Прямо пройденный маршрут R2:** D/`09.09.26WorkLeft-RAG-00-usage-guide.md` current introduction → явно указанная там D/`09.09.26WorkLeft-02-teaching-chinese-model.md` current R16/R17/02.10 sections → D/`replanning/results/R12/RUN-LOG.md` и `results/R13/RUN-LOG.md` для конкретного status conflict. Это реальное чтение первичного требования и владельца статуса; не полный обход всех RAG. Отдельно прочитаны SOURCE-ROUTES relevant sections, PARALLEL20, JOIN целиком, R19 old carriers, R20 protocol/manifests excerpt, own matrix #r19 и related clauses, previous-round beginning и строки 1600–1660 с recovered algorithm decisions.

**Не заявляется:** повтор всего R01/R03, полное чтение каждого WorkLeft/NDJSON/каждого slice, новая трассировка core production code, эмпирическая проверка алгоритмов. Старые code traces R12/R18 остаются ATTRIBUTED и не превращаются в CODE_READ этой сессии.

## 3. Защищённые требования → содержательная проверка → downstream owner

| Требование | Что сохранено в действующем R19 §6 | Чем исследовать, не заменяя learning coverage | Владелец |
|---|---|---|---|
| ILearningUnit | Domain + vector/input/output schemas, typed many→many, identity associations | При перестановке/cancellation outputs не присоединяются по ошибочному position; lawful mapping оценивается отдельно | R04/R07/R12/R22 |
| ILearningFlow | Отношения Units, input→output correction, состояние и credit | Frozen Units / changed Flow и обратный contrast разделяют вклад коррекции и solver | R07/R12/R13/R22 |
| ILearningOrchestrator | Async combinations, dependencies, admissible paths, cancellation и полный branch budget | Quality/time выбранного feasible path против bounded reference; не обещание global optimum | R06/R07/R12/R22 |
| ILearningDynamicGraph | Higher-order relations и перенос на новые compositions | Primitive success отдельно от unseen composition/edge-policy learning | R07/R12/R21/R22 |
| Общая машина | Existing ILearningMachine training owner; Graph L5; не четыре новых trainers | Фактический owner/changed state/ordinary caller; восстановленный design не code proof | R07/R12/R18 |
| Domain mapping | Domain/intent/stack/project/role, unknown/mixed/abstain, реальные законы | Predicted-domain outcome плюс oracle diagnostic; macro/worst-domain и ошибки маршрутизации | R08/R13/R21/R22 |
| Tokens/vectors | Meaning/schema/version, отрицание/порядок/структура/единицы, lawful projection | Equal dimensions не semantic compatibility; representation/index contrast с reindex cost | R04/R08/R12/R22 |
| Dynamic kNN | Domain eligibility/features/metric или score/k/radius/index/neighbor use/arbiter; positive/negative/warning memory | Fixed/exact vs dynamic/learned на одинаковых данных; k=8 при first-hit consumer не доказательство multi-neighbor эффекта | R06/R12/R22 |
| DPO author/judge | Разрешённая open Chinese model пишет обе final sides; CODEX/Astra выбирает lesson и отдельно оценивает exact pair; Student и outcome evaluator раздельны | usable/preferred/admitted различаются; TIE/BOTH_BAD/UNJUDGEABLE не forced preference; правильный fixed ответ не negative по происхождению | R05/R08/R16/R18 |
| DPO objective/recipient | Exact assessed contrast→representation→conditional policy/reference objective→правильный state→ordinary Student | Пример A/B solvers и selector S: урок выбора адресован S, не автоматически A; response/prompt masking и gradient recipient имеют собственные доказательства | R05/R07/R12/R18 |
| LoRA и projection | Названные trainable keys, смысл входных/выходных координат, objective/consumer | Адаптер может обучать выбранное отображение; наличие LoRA не доказывает tokenizer/codebook или semantic projection | R04/R05/R12/R18/R22 |
| Compact PC и feedback | Base, adapter, index, graph, optimizer, snapshots, admission, applied-once, rollback и cold adoption | Полный RAM/VRAM/time budget всех попыток; retention; user feedback не автоматически истина; unknown update не повторяется | R18/R23/R24 |
| Вся программа | DPO/DynamicGraph + DTC + IDE, S00–S10/extras, семь baseline subjects и extras | First local lesson отдельно от full delivery DAG; deferred capability сохраняет owner и trigger | R01–R03/R11/R14/R21–R25 |

Сохранены TensorCore/shared base/project LoRA, StackDetector-before-normalization, semantic compression, BM25+kNN, arbiter и semantic order. Historical helper-ban не отменён молча: exact source conflict принадлежит R06/R12/R16. Existing R18 A–D, R20 manifests, domain passports и R14 DAG переиспользуются.

### Bulk examination, передаваемая дальше, а не выполненная здесь

L — legacy behavior, когда сравнение разрешено и совместимо. N — исправленный pipeline с frozen parent без update. U — тот же pipeline после допустимого нового урока. **L→N не выдаётся за DPO gain; N→U оценивает learning.** Память/retrieval/code/compute в N/U должны быть сопоставимы.

R13 задаёт до outcomes frozen family-disjoint panel, rubric, практические пороги и uncertainty; operational reuse panel отделяется от untouched confirmation. R21/R22/R23 измеряют per-domain quality/accuracy, domain mapping, lawful path, latency/time, full PC cost и retention от bulk новых pair IDs к bulk. Плохие/отклонённые попытки входят в cost denominator. Changed hash, lower loss и green Windows не достаточны; existing deterministic/native/security checks имеют отдельную область по R17, не отключаются этим ремонтом.

## 4. Что реально подтверждено о двадцати пяти входах

Actual tree `637c3ef58c939b51d9d45f457ab75804e3c68c23` по P snapshot returned `truncated:false`. Все numbered paths присутствуют. Ниже **snapshot blobs до R2**, не утверждение, что все их тела прочитаны или compiled. Изменённый R19 имеет новый blob в publication receipt.

| Card | Observed blob |
|---|---|
| R01.md | 778f3836da81aeee0f9b34aa7d415ad46c5a4e0f |
| R02.md | 1aa8d9e89ce50ad13f699a782c4e4ad8f77e1763 |
| R03.md | fbbbef2bd9c8f12c86dd358ddbab812b8ad1beb1 |
| R04.md | 5c65ad05d9a3bc0226fee239f4a5e36a08e5d79e |
| R05.md | 4e18371437397ec48fbcccc7373018b274121da9 |
| R06.md | d06f3d2ab11d8efff38d6764a13ec551d42be16d |
| R07.md | bce1707a57640181e3d83352c2d133bfcc3dbe9b |
| R08.md | b28d0f1eda9ae40311aa4c941146b090073a1ed8 |
| R09.md | 79553ecbf0a565dbb08a6a1b7c3f8108976e15d0 |
| R10.md | 0332b53d1722263e9715a041d33e6dfc74f23eba |
| R11.md | f3968547d5dafeeef40138591d8e71dd38ab325e |
| R12.md | 9f244e68220719e6c554abdedcc84d1cd2688458 |
| R13.md | 66c2c4d5eb7bcc0740e9ace55a2444884c4b92ca |
| R14.md | d80fec469cc72ec9032ff5c5207e5fba855bcae3 |
| R15.md | 1ff23270c8553a62a1678b95fd29702362130975 |
| R16.md | 0859f1f7ef6031b4cbf437511706f5c43db23044 |
| R17.md | e2462b74c1816a535883ef3cfcaa1852452a23cd |
| R18.md | 51cd7f16e6e0156307263d8f3b551361d27f11a0 |
| R19.md | 0d9caf7f2211b7280a30c65c1a7d871ae5100a83 |
| R20.md | de5e4ece7dfaa5304e6d490f6608849dc0f40772 |
| R21.md | f93fcb55c60366d46464ae3d1caf959e9d3a30af |
| R22.md | f80c9d3b0d67984ad5699c02d1a887e326e34894 |
| R23.md | 048ecddc6502c592af04b19ecfab1221b3950b5d |
| R24.md | 11a9d7589c5d74336d7f87072f1090a3bd2ef341 |
| R25.md | 21743e08a97b948134aeba9c69d83875c1f0d1e5 |

R08 combined `R08-PARALLEL9.md` также присутствует, blob `eb2bacab68926a14f6a6bf4caf802d19a37a17a7`; его body не перечитан целиком в R2. Alias resolution rule взято из прямо прочитанных START/PARALLEL20/R20. COMMON tree blob `ac7bbe6fe7c4de2575f717bf7541d70e94bf7b6a` не означает FULL_READ его 32 KB.

**Приёмка уровней:** 25 numbered paths — RESOLVED на snapshot; отдельные navigation conflicts — REPAIRED; полнота двадцати actual effective prompts — **NOT_ESTABLISHED**; actual model dispatch — **NOT_OBSERVED**; training/PC gain — **NOT_MEASURED**; separate Astra review — **NOT_PERFORMED**. Нельзя поднимать последние статусы по числу строк этой таблицы.

## 5. Сохранённые authority IDs C19-01…C19-12

| ID | Сохраняемый смысл / R2 уточнение |
|---|---|
| C19-01 | README routing не доказательство DONE. Existing R20 route сохраняется; R2 не переписывает already-correct intro. |
| C19-02 | R14 capability DAG вместо обязательного линейного M0→M7; E1 bounded, E2–E4 отдельно. Это attributed R14 baseline, не переисполненный анализ R14. |
| C19-03 | Двадцать independent starts; shared writes сериализованы; no worker restart. |
| C19-04 | R12 current owner status отдельно от старого CP1/REEXECUTION_REQUIRED и runtime binding; прямой R12 RUN-LOG проверен в R2. |
| C19-05 | R13 — полный causal compiler, не только first recipe; прямой RUN-LOG всё ещё REPAIR_REQUIRED на snapshot. |
| C19-06 | Пять собственных `results/Rxx/*`, не общий results/SOURCES.csv; RCOR исключение только явно назначенному integrator. |
| C19-07 | Initial preparation history не сегодняшний status; ни один новый current snapshot не означает все Rxx DONE. |
| C19-08 | Девять research inputs versus двадцать starters; R2 исправил остаток в теле и footer JOIN. |
| C19-09 | COMMON once + full assigned card/repair/checkpoint/substantive excerpts; R08 combined exception; actual content receipt отдельно. |
| C19-10 | Windows/coverage/loss/hash не learning evidence; прежние 403/migration claims — prior-run attributed, не новый API inspection R2. |
| C19-11 | R15 independent review и R18 runtime не выполняются R19; R2 не повышает их statuses. |
| C19-12 | Versioned delta на nearest substantive checkpoint; IDs/visited/evidence/remaining сохраняются. |

## 6. Сохранённые CE19-01…CE19-21 — концептуальные сценарии, не compiled packets

Полные прежние descriptions доступны по immutable history link в начале. Таблица сохраняет IDs, core question, good/bad/edge и meaning; **никаких новых twenty-call counts**. Для actual dispatch R20 обязан материализовать полный контекст по R19 §5. Current exact owner status нужно прочитать при исполнении, а не наследовать из исторического CE.

| ID / run | Context / действие | Good versus bad; edge / результат |
|---|---|---|
| CE19-01 / R01 | Original/WorkLeft/RAG/source roots, own PARTIAL inventory | Concrete primary/root/owner вместо S04-only; alias unresolved не code absent. Own remaining сохраняется. |
| CE19-02 / R07 | Full A–J, original DPO/MECH/RESEARCH-03 и direct code | Full four-role causal map вместо двух seams; late contradiction переоткрывает dependent claim, не весь run. |
| CE19-03 / R09 | WorkLeft process + bounded workflow evidence/R17 | Cost-to-learning-signal вместо CI dogma; старые 13/17 samples не доказанная экономия; 403 не pretend PASS. |
| CE19-04 / R11 | Full repair + sufficient partial receipts | First-lesson minimal DAG и отдельный full delivery DAG; не удалить DTC/IDE ради E1; не ждать whole R13. |
| CE19-05 / R12 | Full A–F + current own candidate | Response objective/recipient/vector-kNN/LoRA actual consumer; не второй trainer; неизвестные keys остаются binding gap. |
| CE19-06 / R18 | Existing materialized A–D, R13/R17, own five carriers | Lossless pair/conditional objective/LoRA keys/ordinary adoption; отсутствие checkpoint блокирует соответствующий child, не всё planning. |
| CE19-07 / R25 | Exact candidate и distinct reviewer | Actual independent receipt либо NOT_PERFORMED; R19 self-readback не Astra PASS; changed semantics не наследуют старый review. |
| CE19-08 / R02 | Original/task/goal + decisions/current role contract | Scoped authority resolution вместо S04-only summary; duplicate ID конфликт помечен и адресован владельцу. |
| CE19-09 / R03 | RAG MASTER/usage + D/G/F/V roots/visited | Pointer→правильный root→primary→caller if claimed; index не primary-read; cycle/alias bounded. |
| CE19-10 / R04 | Original typed contracts + representation/consumer | Many→many associations переживают reorder; position Zip не semantic join; deferred payload lifetime — named seam. |
| CE19-11 / R05 | WorkLeft-02 + R18 + actual trainer chain | Exact contrast→conditional loss→keys→ordinary adopter; collapse/truncation не equivalent pair; keys не invented. |
| CE19-12 / R06 | Domain laws + retrieval/index/neighbor consumer | Learned rerank/k сравнивается с fixed/exact; k=8 с first-hit не gain; nonmetric pruning требует exact fallback. |
| CE19-13 / R08 | One combined CP4 + own checkpoint/domain passports | SQL NULL semantics, valid contrast и independent families; **правильный** deterministic solution не rejected по происхождению; protected source не leak. |
| CE19-14 / R10 | Nine own snapshots + JOIN | Partial semantic receipt не sibling DONE; двадцать statuses не двадцать research inputs; missing exact fact только claim-bound. |
| CE19-15 / R13 | Full compiler + R12/R17/R18 + domain passports | E1–E4 plus first bound recipe; no invented panel/threshold/recipient; historical protocol не full readiness. |
| CE19-16 / R14 | Original IDs + all slices/extras + two DAG | Reuse/change/defer с owner/trigger; independent E2 while E1 missing binding; no forced M0→M7. |
| CE19-17 / R15 | Exact candidate + own objections | Distinct reviewer actual invocation; author self-check не independent; affected new semantics require scoped re-review. |
| CE19-18 / R16 | Current WorkLeft owners + accepted source clauses | Chinese both sides and separate judgment live; history-only edits не active repair; concurrent source patch fresh-blob. |
| CE19-19 / R17 | Risk matrix + actual protected property | Native/security evidence там, где относится; learning gain через panel, не Windows; 403 proposal not applied. |
| CE19-20 / R19 | Current entrypoints + own carriers + R20 | Только нужные live repairs/NO_CHANGE и readback; **не квота девяти patches**; preserve concurrent R20. |
| CE19-21 / R20 | Own published protocol/manifests + actual packet components | Included source/skill content, не link-only; external compiler NOT_OBSERVED без receipt; late R19 только delta. |

### Новые continuation scenarios CE19-22…CE19-25 — тоже DOCUMENT_EXAMPLE

**CE19-22 / R21 — curriculum.** Context: existing R08 passports, WorkLeft teaching и R18 E1. Prompt: сохранить семь baseline subjects/extras, выбрать bounded последовательность новых lessons и второй структурно иной domain; не пересоздавать готовые паспорта. Good: отрицание в EN↔RU и dependency law orchestration имеют разные task families и holdout. Bad: одна улучшенная C# documentation пара объявлена всем-domain competence. Edge: новый mixed domain/rights restriction. Output: domain→law→lesson family→independent examination→next owner R22/R23. No actual teaching in R19.

**CE19-23 / R22 — causal arms.** Context: R04–R08 mechanism maps, R13 N/U protocol, R18 ready/design bounds. Prompt: отделить representation, domain mapping, dynamic-kNN neighbor use, LoRA, Flow/Orchestrator и Graph interventions; frozen factors и causal recipient явны. Good: equal retrieval/code/memory N/U; changed Flow при frozen Units. Bad: одновременно заменить tokenizer, index и trainer, затем приписать всё DPO. Edge: query-dependent distance invalidates ANN; L legacy NOT_COMPARABLE не отменяет N/U. Output: bound experiment recipes с uncertainty/stop либо exact missing binding, не готовый gain.

**CE19-24 / R23 — PC and continual input.** Context: R17 policy, R18 lifecycle, WorkLeft applied-once/rollback и actual hardware evidence при исполнении. Prompt: учесть base/adapter/index/graph/optimizer/snapshots, все attempts и failed/cancelled branches; feedback через admission, fresh pair IDs и cold adopted state. Good: timeout с неизвестным optimizer outcome идёт в reconciliation/quarantine. Bad: повторить update и назвать новым bulk; компактность только по LoRA file. Edge: memory limit, retained-domain regression, poisoned feedback. Output: measured-resource protocol и promotion/keep-parent/rollback criteria; PC fit в R19 NOT_MEASURED.

**CE19-25 / R24 — ordinary DTC/IDE journeys.** Context: прямо прочитанный D/task prompt/goal, RAG F/V routes и R18 ordinary bridge. Prompt: сохранить дерево/HTML/CSS/скрины→API→Student→render/evaluate→lesson→accepted state, отдельный IDE consumer и versioned deployment. Good: ordinary request после cold load использует accepted model. Bad: console-only demonstration либо retrieved route stage объявлены DTC delivery. Edge: cancellation, incompatible model/schema, stale server state, render differences. Output: original requirement→actual consumer→evidence/remaining owner. Не делать весь DTC/native delivery обязательным до первого локального урока.

## 7. Передачи и остаток — конкретные, не скрытые DONE

**R20 proposal R19-R2→PACKET:** принять исправленную R19 карточку и JOIN count/authority delta; при сборке отличать conceptual CE, path presence, materialized content и actual invocation. Включать named skill bodies по real paths/spans; full R07 A–J/R08 CP4/R13 compiler/R18 A–D не обрезать. В этой сессии R20-owned runtime compiler и manifests не изменены; receipt R20 **NOT_OBSERVED**.

**R16 proposal R19-R2→STATUS:** сверить WorkLeft-02 R16-insert R12 REEXECUTION_REQUIRED с прямо прочитанным `results/R12/RUN-LOG.md` ARCH-2 candidate. Менять только obsolete status summary, сохранив role policy и exact runtime NOT READY. WorkLeft здесь не переписан; acceptance R16 **NOT_OBSERVED**.

**Остаток полного R19 acceptance:** содержательно материализовать/проверить actual effective packets по addressed Rxx там, где имеется право и входы; исправить обнаруженные remaining consumers. R2 не переаудировал целиком все девять live target bodies и все двадцать cards, поэтому не объявляет глобальный ALL_ROUTES_SEMANTIC_PASS. Уже исправленные конкретные дефекты и новая полноценная карточка могут быть опубликованы без выдуманного whole-program PASS.

**Текущий итог:** R19-R2 SCOPED DOCUMENTARY REPAIR; remote publication/readback фиксируется в RUN-LOG. Full model-packet completeness NOT_ESTABLISHED; external dispatch NOT_OBSERVED; actual learning and PC-fit NOT_MEASURED; separate Astra reviewer NOT_PERFORMED. Sibling results, core code, tests, workflows, datasets и weights не изменены.
