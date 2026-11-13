# R01–R10 — источники, сохранённые решения и реальные алгоритмы

**Редакция 2026-10-13 / CP2. Это десять подготовленных поручений, не десять выполненных исследований.** Исправлены сами исходные prompts: независимые входы R01–R03, собственные results/Rxx, ограниченные передачи и предметная проверка алгоритмов. Подтверждённые результаты уже запущенных R01–R03 сохраняются; перезапуск не требуется.

## Общая часть каждого полного промпта

**Задача:** выполнить только назначенный Rxx. **Не задача:** обучать модель, создавать coverage, чинить Windows или реализовать весь продукт. Planning: `lubochka/xiigen-planning`, `llm_plan3`; D = `llm mvp/design to code session`. Core: `lubochka/llm_mvp_core`, `codex/dpo-implementation`; K = `docs/skills`. На старте закрепи actual planning/core refs и revision поручения. Восстанови свои опубликованные findings и непрочитанный остаток; новый общий HEAD не аннулирует прежнее чтение.

**Контекст:** original/task prompts и WorkLeft — источники требований; RAG — маршруты, а не доказательство реализации; код — реализация, но не измерение качества. Сохраняются три направления DPO/DynamicGraph, DTC, IDE; S00–S10 и дополнительные найденные этапы; семь baseline доменов и исходные дополнительные языки/задачи. Четыре роли: typed many→many Unit, отношения/коррекция Flow, асинхронные комбинации Orchestrator, higher-order DynamicGraph. Не смешивай роль, домен, intent, слайс, physical model и training owner.

**Сборка prompt:** настоящая общая часть + один назначенный раздел ниже + только его специализация из `HYBRID-ALGORITHMS-AND-RUNS.md` и необходимые H/source spans. Dispatcher делает это сам; пользователь не склеивает документы. `COMMON-CONTRACT.md` задаёт evidence schema; `SOURCE-ROUTES.md` — адреса; `COVERAGE-AND-WORKLOAD-GUARD.md` — нагрузку; `LEARNING-EVALUATION-CONTRACT.md` — метод. Эти документы читаются по относящимся разделам, не целиком каждым исполнителем. Уникальные прежние требования сохраняются, но повторные RUNS/PLAYBOOK не становятся вторым обязательным заданием.

**Skills:** общие — `planning-session-startup-SKILL.md`, `architect-behavior-classifier-SKILL.md`, `output-readability-gate-SKILL.md`. S = `reference-inventory-and-source-map-SKILL.md`; P = `phase-planning-SKILL.md` + `plan-review-SKILL.md`; A = `trainable-architecture-decision-SKILL.md` + `human-readable-architecture-audit-SKILL.md`; C = `code-examination-SKILL.md` + `generated-code-review-SKILL.md`; E = `prompt-patch-authoring-SKILL.md`; M = `experiment-design-SKILL.md` + `improvement-measurement-SKILL.md`. Загружай действительные применимые разделы и зависимости; фиксируй путь/ref/диапазон. Название skill не равно чтению. Исторические 30/50 reviews не добавляются к текущему поручению.

**Полномочия и результат:** чтение/search, анализ существующей истории/артефактов, документная проверка и публикация собственных `D/replanning/results/Rxx/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. Shared `results/SOURCES.csv` не является write target этих workers. Не менять чужие results, live план, code/tests/workflows/weights/datasets/core skills; не запускать trainer/build/product tests/coverage/Windows/Actions/платные генерации, установки или cleanup. Перед записью fresh bytes/SHA, при конфликте reconcile только своей дельты, затем remote readback. Existing checks не отключать, skipped не PASS.

**Возврат:** русский ответ о пользовательском результате; refs/spans и глубина чтения; факты отдельно от гипотез; concrete artifact; meaningful counterexample; нынешняя документная проверка; будущая функциональная оценка/её владелец; ограничения; точная передача. Learning gain = NOT_MEASURED без реального опыта. Requested и actual model/reviewer не смешивать. Промежуточный результат можно потреблять по доказанному subset, не называя весь проход DONE.

**Нагрузка:** общий механизм имеет одного основного исследователя. Передача содержит точный вопрос, paths/spans, уже доказанное, оставшийся объём и принимающего владельца. R07/R10 не резерв для всего непрочитанного. А/Б ниже — последовательные части одного Rxx, не фиктивные независимые запуски. При реальном делегировании ребёнок получает эту общую часть, свою цель/источники/границы/выход/примеры/проверку и return-only полномочия; parent интегрирует. Дополнительный вызов учитывается явно. Непомещающийся обязательный материал нельзя объявить прочитанным: сначала перераспределение, затем обоснованная отдельная extension-карточка по PLAN, не бесконечное повторение.

---

## R01 — карта всего проекта и адресное распределение чтения

**Роль/skills:** исследователь источников; S, P. **Независимые входы:** реальные деревья D/core, `SOURCE-ROUTES.md`, WorkLeft `INDEX-01-file-index-and-archive-plan`, `REDATE-01-one-version-per-kind-plan`, MASTER-INDEX/usage-guide; собственный `results/R01/`; `previous rounds thinking results.md`. Готовые R02/R03 не нужны. `START-R01-COVERAGE-REPAIR.md` используется только для установленного остатка, не автоматического restart.

**Промпт.** Построй карту всей программы, не выбирая архитектуру. Сохрани проверяемый S04-обзор как локальный evidence. Рекурсивно обнаружь WorkLeft/LeftToWork, RAG/local-RAG, MD/NDJSON, общие prompts, S00–S10, дополнительные этапы и библиотеки из исходных задач. Проверь корни `dpo legal build`, `Prepare to merge`, `visual studio & visual studio code continue` и реально найденные дополнительные корни; название папки не доказывает относимость.

Для каждого источника укажи repo/ref/path/blob, размер, роль/версию, требование или предмет, read depth, нужные spans и владельца R02–R10. Мегабайтное смешанное тело дели по вопросам и уникальным решениям, а не по случайным страницам. В реестре отдели текущую часть, единственный исторический первоисточник и дублирующий пересказ. Найди старые правила количества reviews/публикации/coverage, но не запускай их. Сопоставь карту с реальным деревом и покажи ещё не классифицированные поверхности.

**Выход/закрытие:** конкретный SOURCES и распределение вопросов; все обнаруженные обязательные семейства имеют владельца либо точно названный capacity gap. Полнота каталога не выдаётся за содержательное чтение всех файлов.

**Хорошо:** representation одного WorkLeft передан R04, update — R05, graph — R06, общая история прочитана один раз. **Плохо:** R07 поручено «изучить всё». **Края:** переименованный файл, циклический RAG, одинаковый basename в разных корнях, код вне D, новый язык HE↔EN. Отсутствие в одном каталоге не доказывает отсутствие проекта.

**Связь с обучением/передача:** каждому domain mapping/vector/kNN/DPO/LoRA/graph/feedback ожиданию есть маршрут исследования; метрики принадлежат R04–R08/R13. R02/R03 получают адресные advisory deltas и не ждут окончания всей карты.

## R02 — восстановить исходные ожидания и authority решений

**Роль/skills:** аналитик требований; S, P, A. **Независимые входы:** напрямую `D/original task.md`, `task prompt.md`, `task prompt.txt`, `goal of the round.md`, `Алгоритм создания ДПО.md`, `Изменения в плане работы - цель.md`, `planning of testing.md`, `Claude session decisions.md`; WorkLeft `00-ИТОГ`, `PLAN-HUMAN-01`, `03-luba-decisions`, `03-decisions-register`, `COVERAGE-01-three-projects-requirements`, `RESEARCH-02-owner-decisions-resolution`; собственные results/R02. R01/R03 — необязательные versioned handoffs, не prerequisites.

**Промпт.** Прочитай содержательные постановки, а не только баннеры. Для каждой уникальной обязанности восстанови точную исходную формулировку, source/span, решение владельца, нынешнюю действительность/конфликт, domain/intent/level, наблюдаемый пользовательский результат и code/slice owner. Отдели конечную цель от прежней очереди «весь код, DPO последним».

Пройди request→domain→representation→путь→результат→feedback→пара→update→выбор состояния→ordinary consumer. Сохрани typed many→many, async, correction loops и Builder. DTC включает API, дерево/HTML/CSS/images, несколько разрешений, оценку и user-tier cost, БД, обучение и публикацию; IDE и дополнительные языки не исчезают за семью baseline доменами. Различай 95% к эталону, confidence и статистический критерий опыта.

Отдельно сверь teacher/generator/pair judge/Student/evaluator, one-pair/no-repeat, full snapshot и unit-level payload против graph-level цели. Зафиксируй действующую policy с lineage. Архитектурный gap не отменяет исходное ожидание; старый replay 70/30 не активируется молча. Не переноси правила другого продукта из старого общего документа без доказательства применимости.

**Выход/закрытие:** CHANGE-MAP: исходное требование→поведение→источник→owner→способ наблюдения; DECISIONS конфликтов. Обратная трассировка показывает, что ни одна исходная линия не потеряна.

**Хорошо:** «оркестратор сохраняет качество и уменьшает полную цену допустимого пути». **Плохо:** «20 тестов доказали обучение». **Края:** SQL+BUG, новый домен, разные языки, несколько допустимых ответов; роль называется Learning*, а код — ILogical*. Имена не заменяют проверку смысла.

**Передача:** R04–R08 получают конкретные expectations; R12/R16 — решения/миграции. Не объявляй качество измеренным и R01/R03 завершёнными.

## R03 — пройти все найденные RAG-ветви до первичных решений

**Роль/skills:** исследователь источников и связей; S, P. **Независимые входы:** напрямую WorkLeft `RAG-MASTER-INDEX`, `RAG-00-usage-guide`, `RAG-01-dpo-legal-build`, `RAG-02-design-to-code`, `RAG-03-merge-and-branches`, `RAG-04-vs-vscode-continue`, `RAG-DELTAS`, соответствующие MD/NDJSON, `TASK-PROMPTS-CHRONICLE`, `TASK-PROMPTS-COVERAGE`; собственные results/R03. R01/R02 не нужны для старта.

**Промпт.** Построй маршрут вопрос→запись RAG→объявленный source root→первичный документ→преемник→owner. Разбери строки назначенных текущих NDJSON для адресуемости и отдельно прочитай критические primary spans. Используй visited-set repo/ref/path/blob; traversal cycle не требует перечитывания. Для каждой найденной ветви нужен содержательный маршрут, а для каждого важного вывода — все использованные первичные переходы. Один удачный пример не закрывает остальной обязательный подграф.

В первую очередь восстанови четыре уровня, сравнение комбинаций одного input, domain-scoped положительную/отрицательную/предупреждающую память, final arbiter, source separation и update policy. Конкретный вход из R03 handoff: RAG-04→`visual studio & visual studio code continue/RAG-06-llm-dynamic-flow.md`→`llm dynamic flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md` и `DECISIONS-LOCKED.md`; проверяй актуальный root/ref, не доверяй старому номеру строки.

Пройди также DTC, ветки/merge и IDE. Старый READY/CODE-ABSENT/read-only не доказывает нынешнюю реализацию/полномочия. Исторические метрики и пороги не становятся текущими результатами. Сохрани конфликт replay и различие domain/intent taxonomies.

**Выход/закрытие:** SOURCES с resolved/unavailable/cyclic/superseded и глубиной; FINDINGS решений; точные непроверенные переходы с владельцами. Невозможность открыть цель ограничивает вывод, а не доказывает отсутствие кода.

**Хорошо:** близкий пример запрещённой операции используется как warning, а не chosen. **Плохо:** similarity отменяет арбитра. **Края:** одинаковые пути в разных roots, архив с единственным решением, битая ссылка, новый nested index. Передавай подтверждённые тематические subsets R04–R08 сразу; общее сведение принадлежит R10, не требует ожидания всего R01/R02.

## R04 — S00–S03, typed many→many и смысл представлений

**Роль/skills:** исследователь representation/контрактов; S, A, C, M. **Входы:** актуальные S00–S03; WorkLeft `01-code-changes`, `05-branch-reconciliation-codex-dpo-implementation`, `SOLUTIONS-01-four-code-findings`, `VERIFY-01-status-vs-reality`, `VERIFY-02-counters-and-parallel-worktree`; тематические handoffs. Первичные core anchors: `00.Core/LogicalContracts.cs`, `00.Core/DynamicGraphContracts.cs`; реальные tokenizer/parser/encoder consumers найти по свежему дереву. Незакрытая несвязанная часть R01/R03 не препятствие.

**Промпт.** Сначала по каждому S00–S03 восстанови obligation→active plan→code→existing evidence→gap. Проследи resume/snapshot, before/after identity и split. Затем проследи raw input→parser/token IDs→domain features→vector/projection→typed payload consumer. Одинаковая размерность не доказывает одинаковый смысл. Сохрани units, отрицание, порядок, числовую точность, связи AST/layout и версию координат.

Проверь существующий `IDynamicGraphNode<TInput,TOutput>.ExecuteAsync` и `IQueryable<GraphNodeCandidate>`: где материализуются OutputRefs, как обеспечен requested type, как один/многие inputs порождают несколько действительно разных outputs. IQueryable не означает автоматически remote query provider или бесконечную ленивую выборку. Укажи стоимость materialization и принадлежность результатов snapshot.

**Разделение А/Б:** А — representation producer/shape/version и контраст потери смысла; Б — requested-output consumer/multiplicity и совместимость. Б использует manifest А, не перечитывает всю библиотеку. Выход А — портовая схема, Б — trace до реального consumer; проверки: отрицание не исчезло, два допустимых outputs не схлопнуты. При делегировании применить общий полный child contract.

**Выход/закрытие:** четыре slice строки и представление со source/method/consumer; reusable/partial/unknown отдельно. Нет реализации из одного имени класса.

**Будущий опыт:** общий encoder против domain features на тех же семьях; semantic retention, routing confusion, downstream quality и полная стоимость reindex. **Хорошо:** SQL LEFT JOIN и INNER JOIN различимы по смыслу. **Плохо:** high cosine = корректный SQL. **Края:** OOV, новый stack/version, несовместимый старый индекс, many→zero. Передача R05 — state/coordinate boundary, R06 — distance inputs, R13 — различающий опыт.

## R05 — S04–S06, настоящая DPO-мутация и LoRA

**Роль/skills:** исследователь learning/state; A, C, M. **Входы:** все текущие S04–S06; WorkLeft `02-teaching-chinese-model`, `INTEGRATION-01-training-into-work-program`, `TASK-PROMPTS-TRAINING`, `RESEARCH-03-dpo-algorithm-conformance`; R04 representation subset. Core anchors: `00.Core/LearningMachineContracts.cs`, `10.LoRA/LoraLinear.cs`; actual trainer/pair loader/checkpoint consumers проследить напрямую.

**Промпт.** Не своди три слайса к Windows-дефекту. Проследи pair storage→admission→trainable payload→objective/reference→mutation→independent evaluation→accept/restore→ordinary inference. Для каждой стрелки нужны producer, method, mutated state, consumer и trace либо конкретный gap. Сохранённая пара, увеличенный counter, изменённая kNN memory и gradient update — разные события.

Для стандартного DPO найди common conditioning, chosen/rejected log-probabilities, masks, reference policy, beta, trainable parameters и знак preference. Для собственного vector/path learner выпиши реальную вероятность/правило и objective; arbitrary distance не объявляй DPO. Rich graph trace должен доходить до допустимого training owner/payload, а не теряться в loader. Проверь unit-level GraphDpoPair по первичному источнику; graph learning gap не устраняется удалением требования.

LoRA: назови W/A/B, rank/scaling, forward consumer и фактическую границу optimizer; различи text tokenization, vector encoding и output projection. Нужна совместимость vector-token/schema/index и полный snapshot, не только adapter file. One-pair/no-repeat действует также после сбоя; успешность checkpoint hash не learning gain.

**Разделение А/Б:** А — pair/objective→muтация→promotion, выход executable trace и диагностический wrong-sign пример; Б — LoRA projection→consumer→state lifecycle, выход shape/coordinate map и несовместимый-index пример. Общие sources читаются один раз, пересечение фиксируется портом.

**Выход/закрытие:** три slice карты, точный training owner, parameter/state map и smallest missing seam. **Будущий опыт:** frozen/no-update versus one admitted update, отдельно adapter off/on, per-domain quality/time/VRAM/retention. **Хорошо:** loss снизился, held-out качество нет — utility не подтверждена. **Плохо:** переименовать старую pair_id для нового gradient. **Края:** TIE/BOTH_BAD, усечённый conditioning, неподдерживаемый payload, unknown-running update, rollback только A/B без optimizer/index. Передача R07/R12/R18; provenance/Chinese author/exact-pair expert остаются обязательными.

## R06 — S07–S10, dynamic kNN и композиции

**Роль/skills:** исследователь routing/graph; S, A, C, M. **Входы:** актуальные S07–S10 и `S05-S10-learning-role-audit-2026-10-08.md`; WorkLeft `DESIGN-ACTIONS-01`, `TASK-PROMPTS-P0-P1`, `TASK-PROMPTS-P2-P3`, `TASK-PROMPTS-P4-P6-PT`; R03 primary routes, R04 port manifest, R05 state subset. Core: `00.Core/DynamicGraphContracts.cs`, `21.AgentInfrastructure/Logical/DynamicGraphEdgeAuthorityContracts.cs`; router/index/arbiter implementations найти по consumers.

**Промпт.** Для каждого из четырёх слайсов восстанови исходный сценарий, реализацию, ordinary DTC/IDE consumer и пробел. Отдельно проследи domain/intent mapping, unknown/mixed domains, admissible candidate pool, distance/normalization, k/weights/radius, memory labels, learned selector и final arbiter. Изменяемый вручную k не доказывает learned adaptation. Неподходящая metric/ANN combination требует exact rerank или другого индекса, не молчаливого объявления стандартного cosine достаточным.

Исследуй FanOut, FanIn, ManyToMany, MultiOutput, ReturnLoop; parallel eligibility — режим, не шестой вид ребра. Отдели deterministic enumeration/merging от learned semantic authority и от scheduler. Проверь bounded loops, fan-in meaning, checkpoint-consistent branches, cancellation и полную цену проигравших/незавершённых ветвей. Не называй впервые финишировавшую ветвь лучшей. Higher-order relation должна влиять на решение, не только храниться в topology JSON.

**Разделение А/Б:** А — domain→memory→metric/k→retrieval candidates; выход candidate/label manifest и wrong-domain negative. Б — candidates→learned authority→typed combination→consumer; выход five-edge-kind trace и fast-wrong-branch контрпример. Б потребляет интерфейс А, не дублирует весь retrieval audit.

**Выход/закрытие:** четыре slice строки, authority/connection map и реальные reusable/missing seams. Train остаётся у подтверждённого ILearningMachine, не переносится в каждый Unit/Flow. **Будущие сравнения:** predicted/oracle domain diagnostic; static/dynamic k; flat/higher-order route; deterministic/learned/hybrid при одинаковой доступной памяти. Метрики: mapping отдельно от solver, valid-path rate, quality, cost/regret относительно исследованных допустимых кандидатов. **Края:** пустой доменный pool, near-negative, unsupported metric index, цикл, memory budget, async straggler. Передача R07/R08/R12/R13.

## R07 — REPAIR REQUIRED: полный гибридный learning-механизм, не bounded seam audit

**Статус:** прежний `results/R07` сохраняется как исторический partial evidence, но больше не закрывает R07. Причина: он не выполнил обязательное direct reading назначенных R07 primary sources (`09.09.26WorkLeft-MECH-01-mechanism-validation.md`, `09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md`, `Алгоритм создания ДПО.md`, relevant RAG primary lineage) и сузил задачу до нескольких consumer seams.

**Полный work order:** `replanning/R07-REPAIR-WORK-ORDER.md` — это обязательный prompt, context, skills, RAG, references, subtasks, good/bad examples, edge cases, verification и return schema. Sections 14–18 contain **ten model-ready packets R07-A…J**, the R01–R06 semantic join gate, functional-expectation coverage matrix and anti-narrowing rules. Короткий текст ниже не заменяет его.

**Роль/skills:** архитектор обучающейся системы + code/plan reviewer + prompt engineer + human-readable architecture communicator. Обязательны SK-PLAN, SK-REVIEW, SK-ARCH, SK-PROMPT и SK-EXPERIMENT из repair work order.

**Primary inputs:** original task; `Алгоритм создания ДПО.md`; оба assigned WorkLeft R07; R01–R03 source/RAG maps; exact R04–R06 handoffs; current core tree/code owners для Unit/Flow/Orchestrator/DynamicGraph, domain representation/tokenization, BM25+kNN, DPO/LoRA/checkpoints, feedback и ordinary consumers.

**Промпт.** Восстанови полный causal chain request→domain→domain-sensitive representation→deterministic admissibility→dynamic retrieval/kNN→learned Unit/Flow/selector→async Orchestrator→DynamicGraph higher-order composition→typed result→feedback/weakness→CODEX/Astra 6 ultracode lesson selection→open Chinese model chosen/rejected authoring→CODEX/Astra exact-pair judgment→lossless tokens→DPO/LoRA update of same semantic recipient→accept/rollback→ordinary adoption→next request→bulk evaluation.

Human role выполняет **CODEX/Astra 6 ultracode**; внешняя открытая китайская модель только создаёт обе стороны DPO; exact pair также оценивает CODEX/Astra. Не возвращать ручного человека в operational loop и не использовать proprietary judge output как training target.

Для каждого домена определить laws/invariants, необходимые tokens/features/vector semantics и гипотезу dynamic kNN function. Для каждого learned механизма назвать signal/objective/state/consumer. Для каждой deterministic части доказать, почему это hard constraint, helper или resolver, а не скрытая learned authority.

**Проверка результата:** никакого acceptance через coverage/Windows runs. Learning evidence = causal ML-style contrasts и bulk-to-bulk quality/domain-mapping/time/RAM/VRAM/storage/retention; ordinary tests только для deterministic integrity. Обязательны falsifiers и WRONG-DIRECTION criteria.

**Выход:** обновить те же пять R07 carriers. До выполнения repair текущий статус только `R07_REPAIR_REQUIRED`. R12/R13/R18 получают решения только после repaired R07 evidence, но независимые соседние исследования не обязаны ждать общего DONE.

## R08 — доменные законы, качественные пары и независимые семейства

**Роль/skills:** исследователь данных/обучения; S, A, M. **Входы:** WorkLeft `RESEARCH-01-training-quality-speed-accuracy-program`, `RESEARCH-01-REVIEW-20-passes`; R02 requirements, R05 pair contract, R06 domain taxonomy; existing manifests/measurement artifacts. Это reuse исследований, не новый 20-pass цикл.

**Промпт.** Для семи baseline доменов и подтверждённых дополнительных возможностей создай passports: задачи/поддомены, реальные ограничения и источник их действительности, representation features, positive/hard-negative/warning examples, ambiguous/unknown cases, metric и стоимость. Реальный закон, software specification, domain convention и preference — разные основания. «Короткий ответ лучше» не закон. Builder оценивается по пригодности пары и последующей пользе Student, не по числу строк.

Проверь lineage/split по исходным семействам: paraphrases, рендеры одного дизайна, версии файла и производные пары не пересекают train/dev/certification. Отдельно проверь retrieval-index и teacher-context leakage. Chosen/rejected должны отвечать на один input и различаться по обоснованному предметному критерию; TIE/BOTH_BAD/unknown идут на пересмотр/неприёмку, не принудительно в DPO. Provenance/permissions ролей сохраняются.

Для существующего claim о росте запроси обе версии, общий набор, rubric/judge и raw outcomes. Отсутствие measurement не заменяй нулём. Раскрой distribution gaps: mixed-domain, новые семейства, редкие случаи и forgetting без запрещённого replay.

**Выход/закрытие:** domain-law-feature-negative matrix с источниками и доступностью; confounder inventory; retained WorkLeft research. **Хорошо:** «верни все строки левой таблицы» отличает LEFT JOIN chosen от INNER JOIN hard negative. **Плохо:** произвольный бессмысленный rejected делает pair лёгкой, но не учит границе. **Края:** неоднозначный source, два правильных ответа, противоречивое user feedback, устаревшая норма, неизвестная лицензия. **Передача:** R13 — независимые families/metrics; R21 — curriculum; показатели NOT_MEASURED до опыта.

## R09 — фактические затраты, Windows зависимости и гипотеза подгонки

**Роль/skills:** аналитик процесса; S, C, M. **Входы:** существующие commits/jobs/logs; WorkLeft `INTEGRATION-02-test-batching`, `TASK-PROMPTS-TEST-BATCHES`, `DEC-01-smart-app-control-runtime-path`, `DEC-02-architect-adjudication`, `DEC-04-runtime-blocked-executor-playbook`, `MATRIX-01-progress-by-dates`, `REPORT-01-progress`. Новые job не запускать.

**Промпт.** Обоснуй ограниченное окно истории и восстанови queue time, execution, retries и фактический output каждого этапа. Раздели алгоритм, данные, обучение, оценку, среду и документацию. CI duration не равна человеческим часам. Неизвестная цена не ноль. Подозрение test-fitting проверяй цепью requirement→code→failure→assertion/spec diff→независимое основание. Ни изменённый assert, ни зелёный rerun сами по себе не решают вопрос.

Для каждого blocker проведи зависимость до выбранного будущего локального учебного пути. Различи data/state integrity prerequisite, runtime dependency, release-only gate и посторонний fixture. Предложи retain/reuse/defer/remove-from-critical-path с защищаемым свойством и владельцем; не удаляй workflows/tests. Проверь текущую hardware/runtime конфигурацию по доступному evidence: прежние параметры ПК — не автоматическая гарантия нынешнего fit.

**Выход/закрытие:** causal cost table, evidence за/против подгонки, минимальные предпосылки первого опыта. **Хорошо:** job не на train path, но нужен будущей Windows-поставке — перенести с owner/условием возвращения. **Плохо:** «все раны бесполезны» или «нет лога — всё готово». **Края:** очередь длиннее compute, отменённый job, cache effect, скрытый внешний вызов, разные версии среды.

**Будущая оценка/передача:** time-to-first-valid-lesson отдельно от time-to-first-accepted-improvement; стоимость пары/update/evaluation, failed attempts и стоимость release. R11 получает причинные факты, R17 — точные live planning gates; не выдавай снижение CI time за learning gain.

## R10 — адресное сведение и передача анализу без нового полного аудита

**Роль/skills:** координатор evidence; S, P, A. **Входы:** опубликованные R01–R09 с exact refs/status; назначенные WorkLeft `DOCDEBT-01-documentation-debt-map`, `TASK-PROMPTS-DOCDEBT`, `VERIFY-03-session-documentation-consistency`, `VERIFY-04-integrity-and-readability`, `VERIFY-05-summary-and-verdict`. Чужие files не переписывать.

**Промпт.** Сведи требования и факты: все три направления, S00–S10, baseline/additional domains, four roles и algorithm/state/feedback/PC ожидания. Для каждого вывода укажи source depth и unresolved dependency. Разные refs R01–R03 согласуй по реально изменившимся source blobs/spans, не по самому факту нового HEAD. Различай противоречия фактов, старых policies, aliases и независимые гипотезы. Не принимай IN_PROGRESS за DONE; проверенный subset может поддерживать отдельный аналитический вывод.

Дочитай только точно назначенные gaps. Сформируй competing explanations: missing wiring, слабые пары, утечка/оценщик, representation mismatch, unnecessary serial gate, дублирование, resource bottleneck. Каждому дай evidence за/против, минимальное различающее наблюдение и owner. Критический недоступный источник ограничивает зависимую часть, не всю независимую работу.

Если обязательный остаток не помещается, приложи заполненную extension-карточку: source spans, уникальное требование, почему прежние 25 не поглощают объём, узкий prompt, skills, examples/edges, output/acceptance, affected dependencies и число дополнительных вызовов. Сначала проверь адресное перераспределение. Дополнительная сессия возможна по разрешённому пользователем расширению, но не маскируется выполненным R10 и не запускается автоматически при подготовке плана.

**Выход/закрытие:** компактный evidence bundle для пяти разных аналитиков, conflict register и ограниченные gaps/owners. **Хорошо:** registry/encoder seam подтверждён, историческая несвязанная библиотека открыта — анализ seam продолжается с границей. **Плохо:** «всё исследовано» по количеству файлов. **Края:** concurrent partial update, спорный primary, новый domain, поздний источник, изменённый code blob. **Передача:** R11 причины; R12 architecture; R13 measurement; R14 schedule; R15 independent challenge. Learning measurements и 10 фактически выполненных runs заявляются только по реальным evidence.
