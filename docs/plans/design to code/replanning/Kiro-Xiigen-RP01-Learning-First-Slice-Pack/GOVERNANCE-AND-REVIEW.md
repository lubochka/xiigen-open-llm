# RP01 — роли, права и минимальная проверка BATCH500

Действующая редакция15.10.2026. Один dispatcher: [RP01-EXECUTE-NOW](RP01-EXECUTE-NOW.md). Новые исполняемые work orders: [tasks/batch500](tasks/batch500/README.md), основной [BATCH500 contract](RP01-OFFLINE-TEACHER-BATCH500.md). Старые execution/children/T0–T3/V3–V6/405a/979256 тексты — references для прежнего опыта/конкретного proven defect, не параллельная очередь и не default Chinese-only restriction для нового режима.

## Кто выполняет какую роль

| Роль | Новая research campaign | Сохраняемая граница |
|---|---|---|
| S | CODEX `astra 6 ultracode` выполняет человеческую роль выбора TRAIN | Только разрешённый source, не private gold |
| A | Отдельная Astra `astra 6 ultracode` симулирует роль внешнего Chinese Teacher и пишет обе стороны DPO | `ASTRA_SIMULATED_TEACHER`; не выдавать за реальный Qwen/Chinese output |
| J | Отдельный CODEX `astra 6 ultracode` судит exact immutable pair | Не автор пары, не её редактор, не final examiner |
| Student | Existing local trainer реально обновляет разрешённое learned state | Не симуляция; frozen base/reference и state ownership по protocol |
| E | Независимое назначение CODEX `astra 6 ultracode` экзаменует сохранённые реальные ответы | Blind identity до фиксации scores; не TRAIN author/J |
| Research continuation owner | Сохраняет/продолжает isolated candidate+optimizer/RNG/cursor | Не объявляет каждый bulk production promotion |
| Canonical publisher / cold consumer | Только отдельное реальное serving/release admission и проверка принятых weights | Нельзя подставить small research gain как native PASS |

Requested model label не установленный API ID. Actual platform/model/session/authority записываются из настоящего receipt. Нет вызова — NOT_PERFORMED, не выдуманное назначение. Разные sessions одной модели не гарантируют независимости ошибок. Текущий development exception не отменяет целевую роль настоящей внешней открытой китайской модели в последующей production интеграции; provenance providers не смешивается.

## Права пропорциональны действию

Публикация planning разрешена запросом пользователя и не требует повторного согласования каждого markdown. Документ не создаёт fake runtime grants, доступ к PC, external model calls или право опубликовать private data. Existing genuine поручение переиспользуется в точном разрешённом scope; не требовать того же согласия для каждой пары/файла. Missing operational authority — конкретный факт, не повод построить новый approval waterfall.

S/A/J видят TRAIN/source/provenance; custodian/E получают отдельный evaluation-only доступ к gold/rubric/raw answers. J verdict, training eligibility, optimizer permission и promotion — разные решения. Новый source kind требует честного versioned admission, не изменения `LOCAL_CHINESE_BOTH...` поля задним числом. Старые labels/даты сами по себе не доказывают сегодняшнюю policy; actual source rights/privacy/retention проверяются отдельно.

## Минимальные проверки вместо coverage-ворот

READ_ONLY/REUSE_NO_CHANGE/документы →0 новых product tests. При изменённом exact material/mask/objective/LoRA/journal/loader/type/async contract — ближайшая проверка затронутого invariant и существенного refusal/edge. Не весь список на каждом шаге. Сборка только affected dependency closure. Existing точное evidence переиспользуется; affected change делает соответствующий verdict stale.

Не добавлять blanket Windows Actions, проценты coverage, test-count quotas,30/50 sequential review quotas или полный library preload перед новым уроком. Relevant architecture/planning/plan-review/code/prompt/human-communication skills применяются через actual sources, owner, good/bad/edge и bounded evidence. Generic skill procedure не возвращает отменённый пользовательским поручением waterfall. Это scope-specific правило RP01, не отмена необходимых permissions/custody/branch/serving safeguards.

Полезность обучения — [LEARNING-EXAMINATION](LEARNING-EXAMINATION.md), а не число assertions. Узкая deterministic проверка DPO sign/checkpoint не претендует покрыть все будущие outputs модели. Случайность и обучаемость различаются: часть learning system имеет проверяемые инварианты даже при nondeterministic behavior.

## Убрать два локальных тормоза, не подделывая результат

В BATCH500 отсутствуют local Qwen author И local Qwen evaluator calls. Аuthor/J заранее создают и проверяют immutable material; E работает после capture. В Student optimizer loop нет внешнего model call и mandatory legacy calibration60. Perf probe1–4 TRAIN до shard50 допустим; цена и его effect считаются отдельно.

Не требуется обновлённый final generator baseline до первого step: immutable B0 и frozen independent exam design допускают отложенный read-only baseline inference. Необходимо, чтобы B0 не мутировал. Нельзя подставить нынешний Student вместо B0 или dummy evaluator verdict.

Small paired Q_law gain — exploratory signal, не statistical significance. Положительный point estimate/частично исправленная clause не получает автоматически production-ready/native PASS. Negative/inconclusive observation допускает следующий обоснованный research bulk в рамках budget; не требует дорогих повторов до победы.

## State ownership и история

Current executor уже работает — получить handoff/diff до overlapping code/state действий. Один writer на learned state и один writer на path. Data/assessment preparation может быть параллельной в раздельных scopes. BATCH500 — новый prospective protocol/experiment, не replay старого consumed PairId и не удаление old intent.

Historical one-pair run/12×5/+2/12/scorer/verdict сохраняются. Если он complete negative, не повторять. Genuine unknown in-flight state требует journal-owner recovery для конфликтующего effect. Changed source/rubric/settings не маскируется под продолжение старого run. Старый root-only API не применяется к descendant через reset; новый research continuation сохраняет соответствующее optimizer/RNG/cursor состояние и изоляцию от serving.

## Git-публикация и evidence

Planning: `lubochka/xiigen-planning:llm_plan3`; product: `lubochka/llm_mvp_core:codex/dpo-implementation` только в scope реального product task. Current blob→SHA-bound scoped update→commit result→remote readback. Concurrent conflict→refresh/reconcile, не force push. Чужую работу/weights/private exam не трогать. Intermediate planning publication допустима, но не выдуманный independent PASS.

Local save, Git/LFS delivery и actual cold-serving identity различаются. Большие/ограниченные веса, user inputs и sealed gold не публикуются автоматически. Проверка pointers сама не доказывает наличие всех backing bytes. Public examples не превращаются в private held-out exam.

## Review и отчёт

Review имеет явный PLAN_ONLY/CODE_DELTA/LEARNING_RESULT/ADOPTION scope. Собственная редактура не независимое Astra review; документный PASS не обучение. Проверить exact named candidate и функциональные expectations, не только наличие headings/файлов. Локальная schema/digest проверка не считается независимым domain judgment.

Return: actual task/P/K/skills/sources; changed files или NO_CHANGE; actual actor receipts; effect/state/learning/serving раздельно; unique pairs/families/tokens/steps; время по стадиям/new/reused/unknown calls; meaningful failure+resolution; commit/readback; один next bounded task. Пользователь получает milestone/вывод/проблему, не поток “ещё один из120 slots”. Новые data/training/metrics не выдаются за выполненные без artifacts.

[Предыдущая governance редакция, historical only](https://github.com/lubochka/xiigen-planning/blob/822ff1b5e811f1da261919834bfeedbef6e0d41e/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/GOVERNANCE-AND-REVIEW.md).
