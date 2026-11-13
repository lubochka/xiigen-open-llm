# R11–R25: решения, изменения и передача к реальному обучению

**13.10.2026 · CP-C, третий checkpoint подготовки поправки. Пятнадцать дополнений к существующим задачам, не пятнадцать выполненных запусков.**

## Общая часть для dispatcher и каждого ребёнка

Очередь остаётся [PLAN-25-RUNS](PLAN-25-RUNS.md): R11–R15 анализируют, R16–R20 меняют требования/план, R21–R25 дополняют программу и проверяют передачу. Взять общую часть и полный назначенный Rxx из [11–15](LEARNING-WORK-ORDERS-11-15.md), [16–20](LEARNING-WORK-ORDERS-16-20.md) или [21–25](LEARNING-WORK-ORDERS-21-25.md); добавить его уникальные HYBRID/RECOVERED требования и один раздел ниже. Повторы свернуть, уникальные требования сохранить. Не загружать все пятнадцать задач или весь архив в каждый worker.

D = `llm mvp/design to code session` в `lubochka/xiigen-planning:llm_plan3`. K = `docs/skills` в `lubochka/llm_mvp_core:codex/dpo-implementation`. E = [LEARNING-ACCEPTANCE-DELTA](LEARNING-ACCEPTANCE-DELTA.md). Входы materialize по actual repo/ref/path/blob/span; чужое verified finding не выдавать за собственный обход всей библиотеки. Достаточный тематический checkpoint можно использовать до завершения независимых частей. Неразрешённый вопрос блокирует только зависящий от него вывод/опыт.

Общие skills: `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `output-readability-gate-SKILL.md`. В карточках: S = `reference-inventory-and-source-map-SKILL.md`; P = `phase-planning-SKILL.md`; A = `trainable-architecture-decision-SKILL.md`; C = `code-examination-SKILL.md` + `generated-code-review-SKILL.md`; M = `experiment-design-SKILL.md` + `improvement-measurement-SKILL.md`; Q = `prompt-patch-authoring-SKILL.md`. Прочитать реальные применимые разделы и записать depth, не просто названия. Старые review-count/coverage процедуры не создают здесь дополнительные 30/50 запусков.

Общие ограничения: только документная работа/анализ существующего evidence и разрешённая публикация. Не запускать/создавать product tests/coverage/Windows/Actions/build/trainer/paid generation; не менять code/weights/datasets/core skills/workflows. R11–R15 пишут свои пять carriers в `results/Rxx/`. R16–R25 дополнительно изменяют только назначенные live planning документы внутри D, прочитав их целиком и актуальные consumers. Не изменять чужие results/историческую формулировку пользователя. Fresh SHA→own delta→remote readback; без force push. NOT_MEASURED, planned, source-traced и actual invocation различать.

Подзадачи A/B по умолчанию выполняются последовательно одной моделью в рамках одного Rxx; это не независимые reviewers. При реальном делегировании parent выдаёт ребёнку эту общую часть, контекст/refs/skills Rxx и полностью соответствующий A/B prompt с output, example, edge и acceptance. Child return-only; parent интегрирует в existing carriers. Вход B — именно принятый output A и указанные дополнительные источники. Действительные дополнительные calls учитываются. Непоместившийся материал делится по конкретным решениям/связям и ёмкости, не переносится безгранично в R25.

## R11 — причинно объяснить задержки, не заменить одну подгонку другой

**Входы/refs:** R09 episodes/cost; R04–R07 reuse/missing seams; R08 evaluation gaps; R02/R10 requirements; базовый R11. **Skills:** S/P/M. **E:** §8–9.

**A — prompt исследования причин.**
> Для каждого существенного bottleneck первого полезного учебного результата запиши факт за, факт против, альтернативное объяснение, missing evidence и discriminating observation. Раздели ожидание Windows/CI, реальную работу реализации, повторные reviews/чтение контекста, неправильные пары, отсутствие state consumer и несовместимые representations. Обвинение в подгонке не считать доказанным без relevant diff/истории. Выход A — причинная таблица с фактическими границами измерения.

**B — prompt решений процесса.**
> По A выбери do-now/reuse/defer/remove-from-critical-path. У каждого решения назови защищаемое свойство, связь с первым уроком, источник, риск, owner и trigger возврата отложенного release шага. Различай время до валидного опыта и до принятого улучшения. Не отменяй provenance/state recovery и не меняй rubric ради положительного score. Выход B — ранжированные process changes для R14/R17, а не workflow edits.

**Пример:** существующий Windows fixture не участвует в первом trainer→consumer пути, его release-проверка сохраняется с отдельным trigger. **Антипример:** все проверки отменены или объявлено точное число сэкономленных дней без логов. **Края A:** queue дольше compute, missing logs. **Края B:** дешёвый baseline даёт отрицательный результат; это всё равно валидное исследование. **Приёмка A:** каждый causal claim имеет опору и counter-explanation. **Приёмка B:** изменение приближает конкретный observed user outcome; unknown не ноль. **Будущая оценка:** полная цена/время на валидный урок и useful retained improvement, не количество зелёных jobs.

## R12 — выбрать минимальный гибрид, сохранив четыре роли и владельца обучения

**Входы/refs:** R04 representations, R05 objective/state, R06 connections/kNN/graph, R07 seam trace, R08 laws/data; original task, WorkLeft RESEARCH-02/03 и actual core symbols из этих traces. **Skills:** A/C/M/P. **E:** §2–7,9.

**A — prompt выбора схемы.**
> Выбери source-faithful baseline и один минимальный challenger, используя существующий код прежде нового pipeline. Для каждой участвующей роли и связи укажи deterministic operation, learned authority, input/output meaning, schema/coordinate version, mutable state, owner и objective. Сохрани typed many→many, correction, async alternatives, higher-order graph/Builder. Выход A — выбранная схема и rejected alternative с причиной; отсутствие пользы пока hypothesis, отсутствие owner — настоящий design gap.

**B — prompt методов и причинной различимости.**
> Преврати выбор A в конкретные comparisons: oracle/predicted domain; frozen/updated unit; fixed/learned Flow or route; source kNN/fixed domain-aware/adaptive candidate; static/LoRA projection; frozen/learned graph relations. Укажи что сравнение держит неизменным и чему обучает pair. Не назови arbitrary score DPO; не запиши query-dependent score как metric без проверки применимости index. Разреши только source-backed policy; unresolved unit-level credit versus graph objective отдаётся на явную миграцию, не исчезает. Выход B — decision и smallest implementation/examination handoff.

**Пример:** low-rank adaptation конкретной найденной projection с тем же lawful consumer. **Антипример:** LoRA автоматически считается tokenizer; отдельный Train придуман для каждого уровня. **Края A:** неизвестный domain, несовместимые ports. **Края B:** два пути отличаются несколькими причинами; нет идентифицируемого credit. **Приёмка A:** схема не дублирует существующий owner. **Приёмка B:** real methods или planned gap названы честно, experiment может опровергнуть пользу. **Передача:** R16/R18 получают решения и конкретный первый seam, не указание ещё раз изучать весь проект.

## R13 — первый исполнимый recipe и bulk-to-bulk измерение

**Входы/refs:** R05 update policy, R08 families, R09 PC/cost, R12 architecture, existing measurement entrypoints через actual code map. **Skills:** M/A/P. **E:** §4,6–10.

**A — prompt baseline и независимости.**
> Сначала подготовь минимальный первый recipe, не откладывая его до R22. Назови actual baseline/state/consumer, первый source-supported domain, отдельные source families, lawful outcome rubric, oracle/predicted-domain diagnostic, hardware/resource envelope и dataset/state version manifest. Выбери один challenger и основной вопрос. Определи meaningful change/uncertainty до оценки candidate; missing baseline не заменяй выдуманным threshold. Выход A — заполненный первый examination packet либо точные отсутствующие prerequisites с owner.

**B — prompt нового урока и серии наблюдений.**
> По current one-pair/no-repeat policy опиши fresh input observation→admissible pair→snapshot→один update→operational decision/restore→real consumer. Bulk — группа новых уроков и отчёт, не replay/minibatch. Для research сравни before/after на matched panel по независимым families; отдели многократно использованный operational panel от reserved confirmation. Укажи per-domain routing/quality/path validity/latency/memory/retention, accepted/rejected attempts и полную стоимость. Выход B — raw-result schema, IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE rule и минимальный следующий causal contrast.

**Пример:** улучшилась source-family generalization, но один домен деградировал — оба факта сохраняются. **Антипример:** больше сохранённых пар или loss ниже считается learning gain; каждую пару заставляют ждать нового significance gate. **Края A:** один независимый источник, нет oracle domain. **Края B:** zero accepted improvements, noisy timing, no-op update, interrupted mutation. **Приёмка A:** entrypoint настоящий или exact planned seam, не invented CLI. **Приёмка B:** метод различает утечку/шум/регрессию и не обещает monotonic gains. **Передача:** R18 может готовить первую реализацию сейчас; R21/R22 расширяют, не блокируют.

## R14 — критический путь без зависимости от всех 25 отчётов

**Входы/refs:** R11 causal priorities, R12 smallest design, R13 first recipe, R04–R09 capacity and state prerequisites. **Skills:** P/A/M. **E:** §9.

**A — prompt dependency graph.**
> Для каждого шага первого lesson перечисли реальные prerequisites отдельно от advisory evidence, последующих domain extensions и release-only работ. Сохрани source/code owner и ожидаемое observable output на каждом ребре. R01–R03 допускаются параллельно; тематический reader использует достаточный checkpoint, не ждёт общий DONE. Выход A — DAG с first-learning lane и whole-program continuation без циклов.

**B — prompt нагрузки и передачи.**
> Сопоставь DAG с объёмом чтения/решений/новых code seams и ресурсами PC/reviewer. Параллельность разрешай по независимости входов/выходов, не по одинаковым номерам. На first-learning lane помести baseline→missing seam→fresh pair/update→comparison→state decision→иной domain. Не ставь полную матрицу всех domain×rank×k×graph до первого опыта. Выход B — посильные work units, стоп-условия scope expansion и явное описание когда требуется additional round.

**Пример:** R21 curriculum продолжает расширение, пока отдельно авторизованный узкий learner seam уже реализуется. **Антипример:** R25 DONE — prerequisite любого обучения. **Края A:** state owner не выбран, но другой маршрут готов. **Края B:** GPU contention, общий writer, late source. **Приёмка A:** каждое blocking ребро имеет причину. **Приёмка B:** нет скрытых calls и бесконечного резерва R10/R25; unknown duration остаётся оценкой. **Передача:** R18/R19 получают настоящую очередь реализации, не новый годовой обзор.

## R15 — независимая экспертиза направления

**Входы/refs:** bounded R11–R14 packets, original expectation map R02/R10, actual dissent/evidence. **Skills:** A/M/P/C. **E:** §10.

**A — prompt независимого критика.**
> Выполни экспертизу запрошенной Astra Ultra, если именно она реально доступна назначенному reviewer; зафиксируй requested/actual identity и invocation. Не считай автора второй независимой моделью. Исследуй выбранный гибрид: где потеряно исходное требование; где fixed helper выдан за learned choice; где domain law/teacher provenance/credit assignment/total PC cost делает вывод недействительным. Найди сильнейший содержательный контрпример. Выход A — evidence-bound findings; при отсутствии отдельного reviewer отмечается NOT_EXECUTED, а не вымышленный PASS.

**B — prompt предметного решения.**
> Для каждого finding из A различи противоречие решения, отсутствующую code seam, недостаточные данные и честную эмпирическую неопределённость. Дай smallest correction или discriminating experiment, точный owner и объяснение, блокирует ли это первый урок. Не превращай review в новое обследование всех файлов. Выход B — actionable recommendation для R16–R20 с границей independent/self-review.

**Пример:** графовая pair не локализует blame; нужен controlled path contrast, не наказание всех units. **Антипример:** уверенный reviewer пишет «модель точно компактнее» без памяти/времени. **Края A:** позиционный bias, разные rubric versions. **Края B:** независимый reviewer недоступен, несколько lawful вариантов. **Приёмка A:** реальность экспертизы и ссылки проверяемы. **Приёмка B:** empirical hypothesis с готовым опытом не становится общим embargo. **Будущая оценка:** экспертиза дополняет raw outcomes, не заменяет их.

## R16 — применить требования и не стереть восстановленные решения

**Входы/refs:** R02/R10 preservation map, R12 decisions, R13 method, R15 реальные findings, active requirement consumers. **Skills:** P/A/S. **E:** §2–9.

**A — prompt изменений требований.**
> Прочитай actual bytes requirements и consumers. Для каждого исходного expectation сохрани stable ID/source и укажи retained/rephrased/deferred/replaced с причиной. Примени four-role semantics, domain/intent axes, law-aware representation, dynamic kNN, actual DPO target, LoRA projection, full PC envelope и feedback/retention. Выход A — реальные before/after clauses, не соседний рекомендательный отчёт.

**B — prompt обратной трассы.**
> Пройди каждую исходную роль, baseline/дополнительный domain, весь slice/extra scope и DTC/IDE/consumer обязательства обратно к новым clauses и experiments. Сохрани conflict lineage для policy; source-backed design нельзя заменить удобной реализацией без явной migration decision. Выход B — CHANGE-MAP с actual consumer readback и unresolved-only list.

**Пример:** required higher-order relation сохраняется, пока существующий trainer требует migration. **Антипример:** непомещающееся graph learning удалено из требований ради раннего DONE. **Края A:** исторический source несёт единственное решение. **Края B:** два consumers читают разные clauses. **Приёмка A:** файлы реально изменены в разрешённом scope. **Приёмка B:** ни один unique source expectation не потерян; count строк сам по себе не доказательство. **Передача:** R17–R20 получают exact applied requirements и будущие observations.

## R17 — изменить саму приёмку, а не добавить противоречащий override

**Входы/refs:** R09/R11 source-backed process changes, R13 examination, R16 requirements, actual startup/continuation/acceptance text. **Skills:** P/M/Q. **E:** §8–9.

**A — prompt поиска и переноса clauses.**
> Найди активные требования, ведущие learning/docs работу к Windows/coverage/test-count. Для каждого сохрани exact clause, защищаемое свойство и real consumer. Отдели документную проверку, learning examination и release compatibility. Исправь активный текст, сохранив release owner/trigger; code/workflow не менять. Выход A — применённые clause diffs.

**B — prompt прохода по решениям.**
> На A проследи document-only change; lawful alternative answer; valid but useless pair; invalid provenance; incompatible state; unrelated Windows failure; inconclusive bulk. Для каждого опиши следующий ограниченный шаг и разрешённый вывод. Нельзя менять panel/rubric под результат; нельзя объявить data/state integrity необязательной. Выход B — walkthrough того, как новый executor принимает решения без test treadmill.

**Пример:** valid experience без gain ведёт к следующему причинному сравнению, а не ослаблению критерия. **Антипример:** coverage gate просто переименован в «Astra PASS» на каждую пару. **Края A:** stale startup mirror. **Края B:** несовместимый judge scale, unknown-running update. **Приёмка A:** исправлен consuming prompt. **Приёмка B:** NOT_MEASURED/INCONCLUSIVE/INVALID_EVIDENCE не равны PASS. **Будущая оценка:** цена и время до валидного наблюдения без потери безопасности состояния.

## R18 — сначала один настоящий implementation/examination packet

**Входы/refs:** actual R04–R07 traces, R12 selected seam, R13 first recipe, R14 dependencies, R16/R17 clauses. **Skills:** A/C/P/M/Q. **E:** §2,6–9.

**A — prompt ближайшего missing seam.**
> Первым результатом выдай самый маленький достаточный implementation work order: actual repository/ref/files/symbols, source requirement, current→target, existing reuse, producer/consumer, lawful typed input/output, mutable state/owner, objective/data admission, snapshot/recovery, one baseline/challenger recipe, resources и acceptance. Если нужный код уже есть, выбери wiring/examination, не новый framework. Missing API обозначается planned seam с основанием отсутствия эквивалента. Выход A — один исполнимый после отдельной авторизации code packet.

**B — prompt динамических продолжений.**
> После A сформируй адресные packets для действительно отсутствующих representation/kNN/flow/orchestration/graph решений. Каждый сам содержит весь context/skills/refs/prompt/output/good/bad/edge/examination; не делегируй «исправь граф» по ссылке на родителя. Раздели алгоритмическую пользу, техническую связность и релизную поставку. Выход B — bounded successors, запускаемые по результату первого опыта, а не обязательный полный набор до него.

**Пример:** после реализации consumer действительно читает вновь принятую версию adapter. **Антипример:** снова написать trainer рядом с существующим или дать invented CLI. **Края A:** pair context теряется в loader; state version не доходит до consumer. **Края B:** общая base mutation, новый domain требует projection migration. **Приёмка A:** у каждого будущего действия есть actual owner и измеримый итог. **Приёмка B:** каждая child задача самостоятельна и не притворяется выполненной. **Передача:** обучение начинается по узкому проверенному пакету, не после завершения всей документации.

## R19 — перестроить roadmap вокруг обучения, сохранив продуктовую поставку

**Входы/refs:** R14 DAG, R18 work orders, R16 requirements, all-slice/consumer map R02/R10. **Skills:** P/A/S. **E:** §9.

**A — prompt нового порядка.**
> Примени в active roadmap порядок baseline→минимальный dynamic seam→fresh lesson/update→same-panel examination→decision→иной domain→целевая проверка связей→compactness/feedback→поставки. Для существующих задач укажи keep/reuse/move/merge/defer с источником и причиной; completed доказанное не заказывай заново. Выход A — реальные roadmap diffs.

**B — prompt цены и неутраченных обязанностей.**
> Сопоставь каждый старый slice/этап и DTC/IDE/ordinary consumer с новым местом и observable outcome. Release-only Windows/task не исчезает, но не тормозит unrelated learning. Отдельно покажи critical dependencies, advisory knowledge, resource conflicts и future triggers. Выход B — old→new transition map и ближайшая очередь посильных исполнений.

**Пример:** второй structurally different domain появляется после первого содержательного наблюдения, platform polish имеет свой trigger. **Антипример:** весь продукт сокращён до CodeDocumentation demo. **Края A:** задача была названа по старому slice, но механизм общий. **Края B:** hardware bottleneck делает два параллельных training jobs хуже одного. **Приёмка A:** изменён используемый roadmap. **Приёмка B:** нет orphan requirement или общего R25 embargo. **Будущая оценка:** пользовательская функциональность и time-to-valid-learning, не commit count.

## R20 — проверить, какой prompt действительно получит модель

**Входы/refs:** R16–R19 applied text, START/AGENTS и фактические consumers, базовые и HYBRID/RECOVERED prompts, настоящие R01–R03 checkpoints. **Skills:** Q/P/A. **E:** §2,9–10.

**A — prompt сборки.**
> Для каждого R01–R25 укажи единственный authoritative base section, общие правила, уникальные source-preserved additions и необходимые E spans. Собери effective packet одного назначаемого Rxx до передачи. Нельзя требовать читать все 25, повторять целые версии или пересылать пользователю список документов вместо поручения. Реальная child часть получает заполненный пакет, не placeholder. Выход A — dispatcher changes и проверяемая mapping R-ID→input→output→consumer.

**B — prompt cold-start проверки.**
> Пройди с нулевой памятью чата независимый R02, R05 update investigation, R13 recipe, R18 implementation handoff и R25 closure. Проверь, что prompt сообщает что делать, где искать, что уже известно, что нельзя менять, example/counterexample, edge, нужное evidence и stop/handoff. Коллизии нумерации старых папок связывай по смыслу и actual invocation, не перенумеровывай результаты. Выход B — найденные и исправленные prompt conflicts плюс явные неподтверждённые места.

**Пример:** executor получает только относящийся R05 и actual state/projection sources. **Антипример:** надпись «один prompt», но внизу требование перечитать 500 KB общих документов. **Края A:** missing skill/renamed source. **Края B:** parent/child role confusion, model limit, conflicting baselines. **Приёмка A:** 25 distinct IDs и именованные dependencies без phantom runs. **Приёмка B:** walkthrough не называется independent model experiment. **Передача:** новые старты используют актуальные bytes; старые workers получают дельту без restart.

## R21 — расширить curriculum по законам доменов

**Входы/refs:** WorkLeft RESEARCH-01 и прежние research results как baseline; R08 passports/families, R12 representations, R13 method, R16 obligations, R18/R19 rollout. **Skills:** M/A/S. **E:** §4–8.

**A — prompt domain passports.**
> Сохрани семь baseline: EN↔RU; orchestration; Figma→HTML+CSS; C# documentation; Angular documentation; SQL documentation; DPO Builder для шести. Добавь все source-confirmed дополнительные области/направления, не смешивая domain и intent. Для каждого напиши реальные laws/условия, task families, input/output meaning, representation discriminants, kNN candidate, learned target и cost. Выход A — заполненные passports, не семь названий.

**B — prompt meaningful curriculum.**
> Для каждого passport дай simple, hard, novel composition, boundary/OOD и retained-skill scenario; meaningful positive/hard negative/warning; permission/provenance; split по первичным families. Пример перевода: область «не все» не тождественна «никто»; SQL: сохранение левых строк отличается от INNER JOIN; layout: красивый desktop не оправдывает потерю блока на другом viewport. Выход B — progression и owner каждого опыта, без live corpus generation.

**Пример:** hard negative меняет ровно существенную предпосылку пути. **Антипример:** 100 paraphrases одного шаблона объявлены широтой домена. **Края A:** непроверенная law applicability/dialect. **Края B:** две равнохорошие альтернативы, adversarial feedback. **Приёмка A:** каждый retained domain имеет содержательную спецификацию. **Приёмка B:** negative действительно другой outcome той же задачи; данные не утекли в confirmation. **Передача:** R22 запускает только необходимые после наблюдения comparisons; первый урок не ждёт полного curriculum.

## R22 — реальные experiment recipes вместо общей фразы «проверить алгоритм»

**Входы/refs:** R13 first recipe, actual R18 code/measurement owners, R19 order, R21 families, existing experiment methods. **Skills:** M/A/P. **E:** §3–9.

**A — prompt первого recipe.**
> Заполни manifest и последовательность будущего первого опыта по реальным entrypoints: baseline/state/version, lawful fresh pair, full snapshot, permitted update once, matched evaluation, state decision и ordinary consumer. Дай raw outcomes/schema, OOM/crash/invalid-data handling и измеряемые boundaries. Не выдумывай команды, missing API укажи как exact planned seam R18. Выход A — operator-ready recipe, не запуск.

**B — prompt условной лестницы опытов.**
> Подготовь bounded cards для domain oracle contrast; representation; fixed/adaptive kNN при той же памяти; memory-only versus policy update; frozen/LoRA projection; Flow correction при frozen units; Orchestrator route при fixed candidate universe; graph relations при unchanged primitives; combined versus component interventions. У каждой — factor/control, independent families, budget, law-sensitive negative, metric, confound, falsifier, stopping rule и decision about next experiment. Не запускай полный Cartesian product. Выход B — conditional recipe ladder, каждый следующий опыт выбран по конкретному наблюдаемому bottleneck.

**Пример:** рост quality после новой памяти проверяется отдельно от learned k policy. **Антипример:** одновременно поменяли tokenizer/k/rank/graph и приписали gain kNN. **Края A:** stale index, interrupted state commit. **Края B:** mechanism невозможно изолировать; честно описать joint intervention/confounding. **Приёмка A:** протокол исполним после авторизованного code handoff. **Приёмка B:** исход меняет решение; no-op и regress допустимы. **Будущая оценка:** measured per-domain utility/price/retention и attribution, не coverage.

## R23 — компактность на ПК и продолжение по вводу пользователя

**Входы/refs:** R09 actual/missing resources, R05 state/recovery, R12/R13 policies, R18/R22 recipes, feedback consumer. **Skills:** M/A/C/P. **E:** §8–9.

**A — prompt полного resource recipe.**
> Подготовь измерение фактического PC profile и peak/resident footprint: base, adapters, optimizer/activations, embedding/index, graph, caches, snapshots, runtime/host, reindex и неудачные попытки. Отдельно train/infer warm/cold и async contention. Определи допустимый envelope и measured-versus-estimated fields. Не заявляй local feasibility по одному adapter size. Выход A — resource examination + affordable options, без изменения hardware/software.

**B — prompt continual input/recovery.**
> Проследи input before learning, permission/authenticity/law validity, duplicate family, admissible correction/pair, isolated update, accept/restore и consumer consistent state. Retention old-domain cases не превращай в hidden replay. Укажи поведение при zero gains, OOM, concurrent request, crash и contaminated feedback. Выход B — end-to-end continuous-learning recipe с полным cost и state lifetime.

**Пример:** полезная коррекция сохраняется только после source-backed admission; inference не читает half-updated state. **Антипример:** любой user correction — truth; hash restore откатывает только веса, но не индекс. **Края A:** snapshot удваивает peak memory, GPU недоступен. **Края B:** повтор пары под новым ID, unknown update completion. **Приёмка A:** все существенные cost owners учтены. **Приёмка B:** policy не меняется молча; не утверждается, что PC уже выдержал опыт. **Передача:** R24/R25 знают реальную feasibility uncertainty и smallest remaining measurement.

## R24 — проверить пользовательскую способность, а не внутренние названия

**Входы/refs:** R16 requirements, R18 actual handoff, R21 curriculum, R22 recipes, R23 envelope; DTC/IDE/ordinary consumer sources. **Skills:** M/A/Q/P. **E:** §3–4,8,10.

**A — prompt сценария результата.**
> Опиши по-русски пользовательские сценарии request→correct domain→valid path→useful output→correction→next better/retained behavior. Включи DTC source/HTML/CSS/images/viewports, documentation и orchestration/Builder outcomes по source scope. Объясни multiple valid answers, unknown domain и law-sensitive failure. Выход A — user-visible examination rubric с trace к настоящим consumers.

**B — prompt контрэкспертизы.**
> Передай actual evidence отдельно назначенному Astra Ultra reviewer при доступности; иначе явно NOT_EXECUTED. Используй blinded candidates, disputed-order reversal и E §10 questions; сохранение domain law важнее убедительного стиля. Для каждого concern дай smallest correction/experiment и влияние на first lesson. Выход B — реальная review recommendation или честный pending invocation, не фиктивный model PASS.

**Пример:** перевод стал гладким, но теряет число — meaningful failure независимо от fluency. **Антипример:** internal class names и charts вместо ответа что умеет пользователь. **Края A:** second viewport fails, ambiguous intent. **Края B:** expert disagrees с rubric, слишком мало families. **Приёмка A:** пользователь понимает expected output и успех/провал. **Приёмка B:** independent assessment не имитируется self-review и не заменяет измерения. **Передача:** R25 получает пользовательский смысл и actual open points.

## R25 — завершить передачу без новой общей серии планирования

**Входы/refs:** verified R01–R24 outputs по применимости; R10 union; R16–R20 actual diffs/consumers; R21–R24 additions; concurrent worker checkpoints и current GitHub state. **Skills:** P/A/S/Q/M. **E:** §1–10.

**A — prompt non-erasure и пригодности.**
> Сверь исходные требования с actual live plan: четыре роли, все domains/slices/extra scope, source routes/WorkLeft, lawful DPO objective/target, representations/LoRA/index, dynamic retrieval/connections, PC/feedback и ordinary consumers. Unread source не превращай в resolved; у каждого действительного gap есть exact owner/посильное следующее действие. Проверь только relevant deltas и настоящие contradictions, а не заново весь архив. Выход A — source-to-implementation-to-examination handoff map с explicit scope of verification.

**B — prompt публикации и next action.**
> Проверь actual published bytes/links/refs, отсутствие коллизий 25 IDs/параллельных writers, независимость заявленных reviews и отличие prepared tasks от executed learning. Первый готовый implementation/examination packet должен быть виден без чтения всего архива. Сохрани чужие results и поздние source deltas. Выход B — точный human summary: что опубликовано, что осталось неизвестным, какой ближайший уже подготовленный шаг возможен после нужной авторизации. Не добавляй автоматический R26 или новый full-plan review.

**Пример:** весь план передан, первый narrow packet пригоден; broad-domain efficiency ещё NOT_MEASURED. **Антипример:** 25 файлов объявлены 25 исследованиями; self-review — Astra Ultra; план считается реализованным. **Края A:** critical contract unresolved versus merely empirical unknown. **Края B:** source SHA конфликтует, contributor дописал результат, отдельный J03 уже выполнен. **Приёмка A:** нет молчаливо потерянного решения. **Приёмка B:** remote publication и claimed scope совпадают; дополнительный действительно необходимый round оформлен явно, а не скрыт. **Будущая оценка:** начинается с подготовленного реального опыта, а не с очередной абстрактной проверки плана.

## Граница готовности этой поправки

Этот файл даёт пятнадцать адресных дополняющих prompts с входами, skills, A/B outputs, good/bad/edge и приёмкой. Основные work orders и уникальные ранее восстановленные решения сохраняются. Ни independent Astra invocation, ни code change, ни training, ни улучшение accuracy/скорости этим авторством не заявляются.
