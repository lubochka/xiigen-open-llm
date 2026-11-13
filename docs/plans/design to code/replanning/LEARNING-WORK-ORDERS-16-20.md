# R16–R20 — применить требования, очередь и полноценные поручения

**Редакция 2026-10-13 / CP4. Пять подготовленных задач изменения плана.** Этот файл не означает, что R16–R20 выполнены или весь implementation roadmap уже мигрирован. Номера сохраняются в [PLAN-25-RUNS](PLAN-25-RUNS.md).

## Общая часть полного промпта

Выполни один назначенный Rxx. Planning: `lubochka/xiigen-planning`, branch `llm_plan3`, D = `llm mvp/design to code session`; core/skills: `lubochka/llm_mvp_core`, branch `codex/dpo-implementation`. На старте закрепи actual refs, прочитай текущие bytes изменяемых файлов и достаточные адресные handoffs. Не считай всю библиотеку прочитанной по чужому реестру.

Применяй относящиеся положения `COMMON-CONTRACT.md`, `SOURCE-ROUTES.md`, `COVERAGE-AND-WORKLOAD-GUARD.md`, `LEARNING-EVALUATION-CONTRACT.md`, LF01–LF12 и только свою специализацию `HYBRID-ALGORITHMS-AND-RUNS.md`. Dispatcher собирает одну полную задачу, не перекладывая соединение текстов на пользователя. Сохраняются все исходные prompts/WorkLeft/RAG, S00–S10 и дополнительные этапы, три направления DPO/DynamicGraph, DTC, IDE, семь baseline доменов и подтверждённые дополнительные требования.

**Навыки:** общие `planning-session-startup-SKILL.md`, `architect-behavior-classifier-SKILL.md`, `output-readability-gate-SKILL.md`; P = `phase-planning-SKILL.md` + `plan-review-SKILL.md`; A = `trainable-architecture-decision-SKILL.md` + `human-readable-architecture-audit-SKILL.md`; C = `code-examination-SKILL.md` + `generated-code-review-SKILL.md`; E = `prompt-patch-authoring-SKILL.md`; M = `experiment-design-SKILL.md` + `improvement-measurement-SKILL.md`; S = `reference-inventory-and-source-map-SKILL.md`. Они находятся в core/docs/skills; реально читай применимые разделы/зависимости, указывай depth. Старые 30/50-review процедуры не запускаются по умолчанию.

**Разрешённые изменения:** конкретные live planning документы внутри D, выбранные по source/consumer map, и собственные `replanning/results/Rxx/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. Не менять чужие results или историческую формулировку пользователя задним числом; active interpretation меняется с lineage. Архитектор обосновывает/проверяет, назначенный редактор применяет; одна модель не называется двумя независимыми ролями.

**Не разрешено этим проходом:** product code/tests/workflows/data/weights/core skills edits; product tests/coverage/Windows/Actions/build/trainer/paid product-generation campaigns; установки, cleanup, force-push. Нужная вне-scope миграция имеет exact target/owner и статус NOT_APPLIED, а не ложное объяснение GitHub read-only. GitHub documentary writes разрешены: fresh SHA, reconcile своей дельты при конфликте, remote readback. Existing checks не отключать.

**Возврат:** человеческий результат, исходное требование/source, решение R11–R15, exact before/after и consumers, документная проверка, будущая functional examination, good/bad/edges, сохранённые обязательства, опубликованный commit и следующий ограниченный owner. Не измеренное — NOT_MEASURED; publication не training gain. При реальном делегировании каждый child получает заполненный scope/context/skills/sources/действия/output/verification/examples/edges и return-only полномочия. Parent интегрирует bytes; реальные дополнительные вызовы считаются, авторская самопроверка не independent review.

## R16 — применить требования к наблюдаемым способностям

**Роль/skills:** аналитик требований и редактор спецификации; P, A, S. **Входы:** R02/R10 original expectation map, R12 architecture/owners, R13 protocol, R15 actual findings, действующие requirement consumers через R01/R03.

### Промпт

Измени сами активные требования, а не только отчёт рядом. Каждому original expectation сохрани стабильный ID, source/span, domain/intent/level, наблюдаемое поведение, actual code owner, будущий опыт и accepting consumer. Проверь обратное соответствие: каждое старое обязательство retained/rephrased/deferred/replaced с причиной и адресом, ни одна уникальная цель не исчезла.

Закрепи четыре роли по смыслу: Unit — domain/vector/output schema и many→many; Flow — корректирующие отношения и перенос; Orchestrator — async combinations с качеством и полной ценой; DynamicGraph — higher-order patterns/Builder. Сопоставление с ILogical*/generic API и ILearningMachine подтверждается evidence, не вводится duplicate interface по alias.

Для mapping опиши known/mixed/ambiguous/unknown domain и ошибки путаницы отдельно от solver. Для representation/LoRA — что обязано сохраняться по смыслу, где конкретная projection и consumer, coordinate version и index compatibility. Для kNN — допустимый domain pool, feature/distance semantics, learned k/weights, negative/warning memory и final arbiter. Для paths — admissibility, качество, полная цена и ограничение сравнения исследованными альтернативами. Для DPO/Builder — common context, meaningful positive/hard negative, корректная objective/reference/target, validity отдельно от Student utility. Для local learning — полный resource envelope, feedback-before-update observation, retention и восстановление.

95% к зафиксированному эталону сохрани как product goal с понятной шкалой; это не текущий score/confidence и не требование уже достичь конечного качества в первом исследовательском опыте. DTC API/дерево/HTML/CSS/images/viewports/БД/публикация/user-tier cost, IDE и ordinary consumers остаются конечными требованиями.

Явно обработай policy collisions: old DPO-last versus early learning; current author/selector/pair judge/Student roles; one-pair decision versus bulk research; unit-level payload versus graph-level target; retry после сбоя versus запрет повторного принятого pair_id. Применяй найденное решение владельца; не принимай молча replay, whole-graph Train или смену источника данных. Если authority не установлена, сохрани действующее ограничение и укажи узкую migration decision, не маскируй её READY.

**Выход/приёмка:** применённый CHANGE-MAP `source → old clause → new clause → behavior → examination → owner → live consumer`. Чтение active consumer подтверждает, что исправление используется, а не существует только в соседнем документе.

**Хорошо:** «правильный домен улучшает допустимый путь на независимых задачах, без роста полной цены сверх выбранного ограничения». **Плохо:** «добавлено покрытие router» вместо способности. **Края:** требование вне слайсов, две несовместимые версии policy, смешанный домен, код пока не может выразить нужную связь. Будущая оценка — R13 panels/metrics, не новая suite; передача R17–R20.

## R17 — применить learning examination вместо coverage-first очереди

**Роль/skills:** проектировщик приёмки; P, M, E. **Входы:** R09/R11 costs/dependencies, R13 protocol, R16 requirements, реальные START/acceptance/continuation clauses из source map.

### Промпт

Найди каждую активную формулировку, возвращающую документный или учебный шаг к Windows/Actions/test-count. Выпиши её exact text, защищаемое свойство, связь с первым опытом и новое место в процессе. Исправь потребляющий prompt, не добавляй ещё один override поверх несовместимого текста.

Раздели: документное доказательство исследования; эмпирическую оценку learning behavior; проверку конкретной поставки. План проверяется primary sources/trace/counterexample/diff. Learning проверяется независимыми доменными задачами, DPO validity и Student utility, сопоставимыми состояниями/панелями, retention и full costs. Поставка требует своих действительных доказательств и не закрывается улучшением core. Existing software checks могут подтверждать инженерное свойство, но не semantic competence; не удаляй/не ослабляй их и не создавай новые coverage-пороги для обучения.

Сохрани data provenance/consent, typed interfaces, compatible state и recoverability как условия честного опыта. Их нельзя превратить в автоматическое требование ещё одной полной suite после любого текста. Release-only runs убираются из нового critical path, не из репозитория, с owner/trigger возвращения. Повторный дорогостоящий шаг требует новой проверяемой причины, не бессмысленного rerun ради зелёного статуса.

Зафиксируй IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE/NOT_MEASURED и дальнейшее действие. Operational policy одного урока не заменяется требованием statistical significance после каждой пары; bulk-level claims используют R13. Неясность или равенство обрабатываются по действующей policy, не снижением рубрики. Invalid evidence сначала исправляет data/state/method gap, не маскируется средним score.

**Выход/приёмка:** реальные clause diffs и обратные ссылки. Пройти случаи: изменён только документ; другой, но правильный ответ; валидная бесполезная пара; посторонний Windows fixture; несовместимый checkpoint; unknown-running update. В каждом понятен следующий ограниченный шаг.

**Хорошо:** алгоритм не улучшился — сохранить честный результат, выбрать следующий causal contrast. **Плохо:** уменьшить expected threshold, чтобы candidate прошёл. **Края:** operator panel уже адаптивно использована, новый judge изменил шкалу, отсутствует baseline identity, consumer ещё старый. Будущая оценка: время/полная цена на валидный урок и полезное изменение, data/restore failures; передача R18–R20.

## R18 — выпустить первый точный implementation packet и адресные продолжения

**Роль/skills:** технический планировщик; A, C, P, M, E. **Входы:** реальные traces R04–R07, выбранный R12 seam, **готовый первый recipe R13**, critical path R14, требования R16 и приёмка R17, замечания R15. Отсутствие всего curriculum R21 не блокирует подготовку первого packet.

### Промпт

Подготовь будущую реализацию, не пиши продуктовый код сейчас. Сначала ответь: какое самое маленькое отсутствующее звено мешает уже имеющемуся коду выполнить новую допустимую пару и получить сопоставимое before/after? Выбери этот seam по evidence, а не заранее «создать новый tokenizer/trainer». Если всё нужное существует, первый packet — подключение/реальный examination path, не дублирование реализации.

Каждый будущий packet должен быть готов для модели без памяти чата. Материализуй actual repo/ref/path/method из R04–R07; source route и известное требование, current→target, exact input/output types/coordinate version, state owner, deterministic/learned boundaries, objective/signal и consumer. Назови разрешённые files/actions, не-задачу, реальные prerequisites, baseline/challenger, good/bad/edge, способ документной/кодовой проверки и отдельный будущий доменный опыт. Не выдавай выдуманный method/CLI за существующий. Недостающий symbol помечается planned new seam с основанием отсутствия эквивалента.

**Последовательные части R18 — каждый при делегировании получает этот полный общий контекст и свой заполненный packet:**

**А — минимальный runnable seam.** Источники: R05 mutation/consumer trace, R12 selected design, R13 first recipe; output: один точный implementation work order с actual code owner и ближайшим observable result. Действия: проверить reuse, описать недостающий контракт/подключение, ограничить изменяемое состояние, дать capture/update/evaluate/restore flow. Good: восстановить реальный trainer→consumer handoff. Bad: начать новый framework. Edge: pair loader пропускает graph context; checkpoint обновлён, inference использует старый. Приёмка: каждый шаг имеет реальный owner/API либо явно необходимый новый seam; после реализации можно получить первый comparable observation.

**Б — изолируемые динамические продолжения.** Источники: outputs А, R04 representation, R06 retrieval/graph, R07 contrasts; output: условная очередь mechanism packets, каждый с одним основным эффектом. Действия: representation/domain seam, domain metric/k policy, DPO/LoRA target, learned graph authority/correction, feedback — только по фактическим зависимостям. Good: fixed candidates + changed selector отделяют вклад выбора. Bad: одновременно переписать tokenizer/kNN/trainer и объявить причину gains. Edge: полезны лишь два компонента совместно — ограниченный 2×2, не полный factorial. Приёмка: пакет связан с метрикой R13 и условием выбора следующего; не все обязательны до первого урока.

**В — независимая от реализации спецификация наблюдения и handoff.** Источники: packets А/Б, R13 measurement contract, R16 requirement IDs; output: к каждому packet точный examination recipe, expected data schema и stop/rollback behavior. Действия: проверить comparable states/families, eligible new pair, retained constraints, all-branch/full-PC costs, consumer checkpoint identity. Good: отсутствие gain опровергает гипотезу без изменения рубрики. Bad: hash/Train-name/coverage = learning. Edge: OOM, invalid pair, unknown-running operation, no direct measurement endpoint. Приёмка: исполнитель понимает первое действие и различающее наблюдение; сама В не называется независимой Astra review, если это авторская проверка.

Для DPO выпиши настоящую математику/reference/masks и target authority, one-pair policy и attribution. Для LoRA — конкретную матрицу/проекцию и координатный consumer, не tokenizer. Для kNN — domain pool/features/normalization/k/weights/index/negative memory и arbiter. Для графа — typed many→many, пять relation kinds, async losers/loops и learned authority. Допустимый deterministic helper сохраняется; фиксированная эвристика не маскируется learned policy.

**Выход/приёмка:** первый точный work order в active plan плюс R13 recipe; условные следующие packets, source/requirement/examination mapping и реальные разрешения. Проверка на бумаге выявляет отсутствующий owner/API, не подменяет запуск. Нет шага «сначала написать ещё общий план». Точные количества новых model calls фиксируются, А/Б/В внутри одного автора не считаются тремя независимыми проходами.

**Будущая оценка/передача:** one-seam before/after на независимых задачах; quality/admissibility прежде полной цены; сохранение прежних навыков. R19 связывает очередь, R20 делает доступный запуск, реализация и опыт выполняются по отдельному действующему поручению, не автоматически в R18.

## R19 — применить whole-program roadmap и сохранить отложенную поставку

**Роль/skills:** владелец очереди и зависимостей; P, A, S. **Входы:** R14/R18, исходные WorkLeft/S00–S10 maps, R16 requirements, R15 findings и live entrypoints.

### Промпт

Внеси порядок в реальные roadmap/queues. Для каждой прежней задачи зафиксируй reused/already-evidenced/necessary-now/deferred/replaced с requirement ID, evidence и причиной. Старый DONE/READY не наследует нынешнюю продуктовую готовность. Убери устаревшее «сначала S04 Windows» только там, где R11 доказал отсутствие зависимости.

Milestones: usable baseline; минимальный seam; первая новая пара с сопоставимой оценкой; принятое полезное изменение либо корректно отрицательный опыт; второй качественно иной домен; hybrid connection contrasts; full-PC compactness; продолжающееся learning from feedback; конечные интеграции. Не гарантируй improvement каждой пары. Каждому milestone дай результат, owner, вход, следующий packet и stop condition, не число commits/tests.

Распредели shared mechanisms по одному исследователю; consumers проверяют собственные seams. Сохрани baseline 25 replanning runs и их actual history. Разрешённые пользователем дополнительные раунды возможны только для доказанного уникального остатка с полной extension-карточкой/влиянием на capacity по PLAN. Не добавляй автоматический R26/30/50; не скрывай extra calls и не запрещай необходимую работу лишь ради цифры 25.

**Выход/приёмка:** применённый roadmap/queue diff, traced original obligations и direct link на первый R18 packet. Пройти DAG на отсутствие циклов и deferred-сирот; каждое обязательство остаётся достижимым. **Хорошо:** DTC integration перенесена с trigger после локального core experiment, не отменена. **Плохо:** всё вне первого CodeDocumentation названо «не MVP» и исчезло. **Края:** concurrent source delta, retained ready code, недоступный внешний consumer, независимый R03 остаток. Будущая оценка: time-to-lesson, cross-domain progress, качество/стоимость и delivery отдельно; передача R20–R24.

## R20 — исправить фактический dispatch, а не добавить ещё overlay

**Роль/skills:** prompt engineer; E, P, A. **Входы:** реальные README/AGENTS/START/continuation/role packets из R01/R03, R16–R19 и R13 evaluation contract.

### Промпт

Обнови фактические entrypoints и templates. Один запрос должен давать один полный packet: актуальный scope → назначенный Rxx → retained result → source subset/refs и skills → current→target → действия/артефакт → примеры/края → проверка → сохранение → bounded handoff. Whole-program context остаётся компактно; предметная глубина загружается по роли, а не всем весь архив.

Проверь **не только START, но и первичные prompts**: R02 не получает R01 как обязательный finished input; R03 не ждёт R01/R02; write targets — собственные results/Rxx; общие carriers не редактируются конкурентно. Готовый sibling — advisory evidence с ref/status. Уже запущенные R1–R3 продолжают snapshot и остаток; revision не reset. R10 reconciles relevant source blobs, не любой чужой result commit.

Собирай child prompt полностью до dispatch. Унаследованная общая часть должна быть реально включена, а не фраза «см. всё выше». Подзадача имеет собственные exact inputs, objective, skill sections, output, good/bad/edges и acceptance. Unknown future evidence не маскировать placeholder-API; прямо указать producer task и что должно быть известно к реальному запуску. Missing necessary contract — bounded prerequisite, не приглашение исполнителю придумать его.

Смысловая проверка без product runs: частичный R01; назначен только R06; legacy S04 предлагает rerun; иное правильное решение; raw source содержит старую команду «выполнить всё»; reviewer отсутствует; внешний источник недоступен; context overflow; primary prompt противоречит summary; user разрешил extra round. В каждом сценарии результат должен сохранять цель, evidence и scope без blanket CI, ложного DONE или скрытых вызовов.

**Выход/приёмка:** applied live entrypoint/template diffs, один конкретный стартовый пример и пример handoff. От README до selected work order нет несовместимых зависимостей/путей. GitHub writes выполняются через доступный connector с fresh SHA/readback; публикация не синхронизирует C: и не означает training. **Хорошо:** R03 продолжает свой source subset, R02 не блокируется. **Плохо:** README разрешает parallel, а actual prompt снова ждёт R01. **Края:** отсутствует requested reviewer endpoint, stale SHA, два writer одного файла — честный методический статус и reconcile, не read-only выдумка. Передача R21–R25 без нового общего replanning.
