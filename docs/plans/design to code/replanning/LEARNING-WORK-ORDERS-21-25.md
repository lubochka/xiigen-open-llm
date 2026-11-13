# R21–R25 — домены, реальные опыты, ПК и передача в обучение

**Редакция 2026-10-13 / CP5. Последние пять задач baseline 10+5+5+5, не ещё пять сверх 25.** Это подготовленные поручения дополнения. Реальные training runs, corpus generation и независимые экспертизы здесь не заявляются.

## Общая часть полного промпта

Выполни один назначенный Rxx. Planning: `lubochka/xiigen-planning`, `llm_plan3`, D = `llm mvp/design to code session`; core/skills: `lubochka/llm_mvp_core`, `codex/dpo-implementation`. Зафиксируй actual refs и прочитанные spans. Используй подтверждённые handoffs, не выдавай их за собственное полное чтение. Собери общий контекст + свой Rxx ниже + только свою специализацию `HYBRID-ALGORITHMS-AND-RUNS.md`/необходимые H. `COMMON-CONTRACT.md`, `SOURCE-ROUTES.md`, `COVERAGE-AND-WORKLOAD-GUARD.md`, LF01–LF12 и `LEARNING-EVALUATION-CONTRACT.md` подключаются адресно. Уникальные рецепты `DOMAIN-EXPERIMENT-RECIPES.md`/`DOMAIN-EXPERIMENT-PLAYBOOK.md` переиспользуются, не перечитываются целиком каждым исполнителем.

Сохраняются весь original task/WorkLeft/RAG scope, S00–S10 и найденные дополнительные этапы, DPO/DynamicGraph, DTC и IDE, семь baseline доменов и подтверждённые дополнительные языки/задачи. Четыре роли и ILearningMachine ownership не заменяются одним CodeDocumentation pipeline. Подготовка первого опыта R13/R18 не ждёт всего curriculum; эти пять задач расширяют его.

**Skills:** общие `planning-session-startup-SKILL.md`, `architect-behavior-classifier-SKILL.md`, `output-readability-gate-SKILL.md`; P = `phase-planning-SKILL.md` + `plan-review-SKILL.md`; A = `trainable-architecture-decision-SKILL.md` + `human-readable-architecture-audit-SKILL.md`; M = `experiment-design-SKILL.md` + `improvement-measurement-SKILL.md`; E = `prompt-patch-authoring-SKILL.md`; C = `code-examination-SKILL.md` + `generated-code-review-SKILL.md`; S = `reference-inventory-and-source-map-SKILL.md`. Файлы читать в core/docs/skills по применимости и зависимостям, фиксировать глубину. Исторические 30/50-review требования не добавляют новые раунды автоматически.

**Разрешено:** чтение, аналитика, документная проверка, назначенные live planning edits внутри D и собственные `replanning/results/Rxx/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. Не переписывать чужие results или исходные исторические слова. Fresh SHA/reconcile/readback обязательны для публикации. GitHub writes разрешены; локальный C: не синхронизируется автоматически.

**Не задача:** product tests/coverage/Windows/Actions/build/training, платные product-generation campaigns, code/weights/datasets/core skills/workflow edits, установки/cleanup/force-push. Назначенная отдельная planning-expertise R25 не запрещается этой границей, но её actual invocation должна быть настоящей. Созданный плановый пример не является train-ready pair или результатом эксперимента.

**Возврат:** русский смысл результата; sources/refs; конкретное дополнение; текущая документная проверка; будущая functional examination; good/bad/edges; applied diff/readback; ограничения и bounded handoff. NOT_MEASURED отличается от нуля; self-review отличается от independent Astra. Реальный child получает полностью собранный context/skills/source subset/задачу/шаги/output/examples/edges/проверку/return-only полномочия. Дополнительные calls считаются явно, внутренние части одной модели не называются независимыми раундами.

## R21 — учебная программа с доменными законами и meaningful negatives

**Роль/skills:** исследователь curriculum; M, A, S. **Входы:** WorkLeft `RESEARCH-01-training-quality-speed-accuracy-program` и его review как baseline; R08 passports/data gaps, R12 representations, R13 method, R16 requirements, R18 first packet/R19 roadmap. Старые 20 research passes не становятся новыми Rxx.

### Промпт

Подготовь семь содержательных passports плюс подтверждённые исходные дополнительные направления. Для каждого укажи реальные источники задач и действительности ограничений, domain/intent/level, input/output meaning, features/token/vector schema, kNN candidate, learned target, task families, сложность, positive/hard-negative/warning классы, независимую оценку и стоимость. Различай закон/спецификацию, convention и субъективное предпочтение. Красивый/короткий ответ не автоматически правильнее.

Используй следующую матрицу как конкретные **спецификации будущих примеров**, не готовый корпус:

| Домен | Что обязано сохраняться / предметные признаки | Различающий негатив | Наблюдение пользы |
|---|---|---|---|
| EN↔RU | Смысл, область отрицания, числа/единицы, время, контекст неоднозначности; language direction отдельно от domain. | Перевод «не все» как «никто»; потерянное число при гладкой фразе. | Семантическая точность по независимым семьям; critical meaning errors отдельно от fluency. |
| Оркестрация | Preconditions, dependencies, доступные действия и ресурсы, ветви/fallback; полезность итогового результата. | Более короткий маршрут нарушает обязательную предпосылку или выдумывает доступный инструмент. | Valid-path rate и качество, затем полная цена всех ветвей; learned выбор отдельно от scheduler. |
| Figma→HTML+CSS | Структура дерева/layout, размеры и ограничения нескольких viewports, видимое содержание/поведение. | Правильный desktop screenshot при overflow/потере блока на другом разрешении. | Визуальная и семантическая оценка по разным исходным дизайнам, не только одинаковым рендерам одного макета. |
| Документация C# | Реальные types/control/data flow, side effects, exceptions и ограничения конкретного кода. | Правдоподобно выдуманный побочный эффект или обещание поведения, которого нет. | Точность и полезность объяснения относительно actual code facts, не совпадение одной фразы. |
| Документация Angular | Связь component/template/state/dataflow в конкретной версии/исходниках. | Перепутана причина обновления UI или приписан отсутствующий binding. | Корректная причинная модель на новых компонентах, отдельно от знания терминов. |
| Документация SQL | NULL semantics, JOIN preservation, cardinality, constraints и заданный dialect. | INNER JOIN вместо требуемого сохранения всех левых строк; ложный вывод о NULL. | Верное объяснение результата/ограничений, hard logical errors отдельно от стиля. |
| DPO Builder для шести | Один conditioning, допустимый chosen, содержательный rejected, причина предпочтения, provenance и target unit. | Оба ответа плохи; разные inputs; очевидная бессмыслица вместо hard negative; закрытый экзамен скопирован в пару. | Pair validity/admission отдельно от полезного изменения следующего поведения Student. |

Сначала простые независимые случаи, затем неоднозначность/ошибки/реальные ограничения, композиции, новые семейства и user feedback. Начальный объём выбирается по разнообразию и ресурсам, не по круглому числу пар. Первый домен — по готовности R18, следующий качественно иной; C# и Angular documentation или два направления одного перевода не доказывают всю разнодоменность программы.

Split train/dev/certification делай по исходным семьям до производных paraphrases/рендеров/версий. Проверь teacher-context и index leakage. Число строк не равно числу независимых семейств. TIE/BOTH_BAD/unknown идут на пересмотр/карантин, не в forced preference. Полезные отрицательные/warning соседи не превращаются в положительные из-за близости.

Реальные пары создаёт разрешённый teacher/Builder с provenance и exact-pair оценкой по текущему role contract; текст закрытого планирующего автора не переносится в train как будто его написал teacher. Graph/path quality можно оценивать, но update target/payload/ILearningMachine согласуются с R12; самостоятельный Train в Unit/Flow не добавляется.

**Выход/приёмка:** curriculum, семь заполненных passports, дополнительные retained domains, negative classes, source/split/ordering и привязка к R18 packets. У каждого есть simple/hard/novel сценарий, meaningful falsifier и metric; отсутствие данных честно обозначено. **Хорошо:** hard negative учит различию допустимого JOIN. **Плохо:** 100 вариантов одного шаблона = широкий корпус. **Края:** новый dialect/language, неоднозначное задание, конфликт feedback с предметным правилом, неизвестное происхождение. Передача R22/R24; actual domain gain остаётся NOT_MEASURED.

## R22 — довести первый recipe и задать условные causal experiments

**Роль/skills:** исследователь опытов; M, A, P. **Входы:** первый protocol/recipe R13, actual implementation packets R18, R19 order, R21 families, existing measurement APIs/artifacts. Не строить новый benchmark framework до первого опыта.

### Промпт

Сначала доведи существующий первый recipe до точных inputs/actions/outputs на выбранном code owner. Не откладывай это до нового исследования всех вариантов. Подготовка recipe не запускает обучение. Если отсутствует API, укажи точный planned seam из R18, не выдуманный CLI.

**А — preflight и исходное наблюдение.** Возьми из R13/R18 exact baseline/checkpoint/consumer, first domain family, rubric и resource limits. Определи версии tokenizer/encoder/index/graph/adapter/reference и допустимую новую pair identity. Зафиксируй panel до candidate update, проверку source lineage/доступа teacher и состояние восстановления. Выход: заполненный experiment manifest и исходный comparison plan. Good: both states будут отвечать на одинаковый input. Bad: train example используется как certification. Edge: нет baseline identity, leaked family, одна independent family, incompatible index. Приёмка: нет незаданных критических входов или выдуманного measurement entrypoint.

**Б — один новый урок и решение.** По действующей one-pair policy: получить текущий ответ → разрешённая диагностика дефицита → teacher candidate pair → независимая exact-pair оценка → admission → полный snapshot → update через настоящий owner → common-panel evaluation → decision/restore → ordinary consumer observation. Сохрани отдельно pair validity, mutation и Student utility. Выход: schema raw outcomes и journal; шаги соответствуют реальным API из R18. Good: valid pair без gain остаётся отрицательным результатом. Bad: pair saved или loss down объявлены learning success. Edge: wrong-sign/TIE/BOTH_BAD, OOM, обрыв после mutation, unknown operation. Приёмка: у каждого состояния понятен следующий допустимый шаг без повторного gradient поверх неизвестного state.

**В — выбрать следующий различающий опыт по результату, не выполнить все сразу.** Сохрани условия сравнения; заранее назови factor/control, falsifier, primary metric, защитные домены, source/state versions, budgets и stopping rule для каждого нужного семейства:

| Опыт | Что изменяется / что удерживается | Что различает результат |
|---|---|---|
| Domain mapping | Predicted domain против отдельно помеченного oracle-domain diagnostic; solver и доступные источники одинаковы. | Ошибка mapping отдельно от ошибочного решения при правильном domain; oracle не production достижение. |
| Representation | Generic representation против выбранных domain features/проекции, одинаковые семьи/решатель; index пересчитывается совместимо. | Смысл/units/структура и downstream quality, а не только cosine/shape; reindex входит в цену. |
| Dynamic kNN | Fixed metric/k против одного выбранного адаптивного варианта при одинаковой admitted memory и budgets. | Retrieval/solver quality, empty-pool behavior, negative/warning handling, latency и полный индекс; рост memory не выдаётся за обучение metric. |
| DPO/LoRA | Frozen/no-update baseline против новой допустимой update; adapter off/on как отдельное диагностическое вмешательство. | Правильный objective/target, реальный consumer и held-out utility; inference ablation не требует заново учить старую пару. |
| Hybrid selection | Один admissible candidate set: deterministic baseline, learned selector, выбранная комбинация. | Качество и valid-path cost; deterministic constraints не отключаются ради выигрыша; глобальный optimum не обещается. |
| Связи четырёх ролей | Disable correction edge, flat/higher-order contrast, фиксированные branches с изменённым completion order, ограниченный 2×2 при доказанной interaction-гипотезе. | Many→many outputs, ценность correction/composition, async выбор не равен first-finish, вклад joint mechanism; не полный factorial всех факторов. |
| Продолжение обучения | Prequential user input до обучения, затем admitted feedback, следующий новый family и retention panel. | Полезная коррекция/forgetting/drift и growth state; ответ после обучения тому же input не out-of-sample успех. |

Каждый реально делегированный А/Б/В получает общую часть, свой exact source subset, steps/output/examples/edges/acceptance целиком. Это внутреннее разделение R22, не автоматические новые replanning sessions. Реальные experiment invocations учитываются позднее в authorizing execution work order.

Для bulk b обе сравниваемые версии отвечают на одну F_b, включая при необходимости новую независимую панель. Отдельно оцени anchor trend и fresh-family transfer. Candidate labels скрыты/порядок сбалансирован для judge; rubric и thresholds зафиксированы до данных. Повторные inference нужны для соответствующего noise, но не доказывают training variance. Closed certification не превращается в adaptive selection panel.

Отчёт содержит: experiment/candidate/reference IDs; input families и splits; chosen/rejected provenance/admission/target; before/after state identities; per-case/per-domain raw outcomes; quality/law violations/mapping/path cost; время pair generation/update/evaluation/reindex/restore; RAM/VRAM/state bytes; failures/cancelled branches; operational decision; bulk-level conclusion; uncertainty и ограничения. Сведения о закрытых targets не отправляются teacher. Отсутствующие measurements — NOT_MEASURED, не 0.

**Выход/приёмка:** один готовый первый work order и условные следующие recipes. Outcomes IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE имеют разные действия. Null result не заставляет менять эталон. Все inputs доступны или точно отнесены к отсутствующему R18 seam; первая реализация не ждёт полного curricula/Windows release. **Хорошо:** kNN не полезен — сохранить простой reference и проверить другое объяснение. **Плохо:** сложный алгоритм обязан победить. **Края:** несопоставимые data budgets, compensated fallback, cache confounder, multiple comparisons, costly expert loop. Передача R23 и будущему отдельно авторизованному исполнителю.

## R23 — полный PC budget, рост состояния и user-feedback lifecycle

**Роль/skills:** архитектор ресурсов и состояния; A, C, M. **Входы:** R04–R07 state maps, R09 actual environment/cost evidence, R12 architecture, R22 recipes, доступные hardware/model manifests. Ранее упомянутый ПК не объявлять подтверждённой текущей конфигурацией без источника.

### Промпт

Составь ресурсную модель именно выбранного локального пути. Раздели measured и estimated: CPU/GPU, доступные RAM/VRAM, storage, ограничения runtime. Учитывай base weights, adapters/projections, optimizer/activations, embeddings/index, graph, admitted examples, caches, snapshots, temporary/reindex copies и concurrency. Дисковый размер не равен resident memory, суммирование отдельных пиков не равно измеренному одновременному peak. Маленький LoRA-файл не доказывает компактность системы.

Выведи bounded first-experiment envelope: длины inputs, число новых пар, candidate/neighbor limits, concurrency, adapter target/rank, index rebuild/copy-on-write costs, reserve для restore. Числа берутся из actual evidence/обоснованной оценки и уточняются опытом, не выдаются за measured fit. При нехватке ресурса выбирай допустимый меньший scope/candidate, а не скрытую внешнюю модель. Отдельно учитывай paid/external calls и стоимость ошибочных/отклонённых попыток.

Сравни compression/pruning/caching как гипотезы: они принимаются по quality/retention и полной цене, а не потому, что index стал меньше. Domain growth должен иметь лимит, вытеснение с сохранением provenance и проверяемый effect; старые accepted pairs не возвращаются в gradient под новым ID.

Опиши versioned state machine: current answer и prequential observation → feedback pending → admission/конфликт/карантин → prepared snapshot → updating → evaluation pending → accept либо restore → published compatible state → consumer ack. Раздели immediate memory/context adaptation и изменение параметров. Коррекция пользователя не истина по умолчанию; конфликт с domain constraints не становится chosen автоматически.

Inference использует согласованный snapshot router/encoder/index/graph/adapter; обучение не мутирует одновременно читаемый набор без определённого протокола. После сбоя выясни реальное состояние операции до retry/rollback; нельзя одновременно откатывать и оставлять неизвестный trainer работающим. Предусмотри exact-state/RNG/optimizer/index recovery, journal и applied-pair identity. Допустимость повторной попытки после полного restore определяется current policy, не самовольной переинтерпретацией no-repeat. Расходы/уже раскрытые данные не откатываются вместе с весами.

**Выход/приёмка:** PC resource contract, growth model и adaptation stateflow с actual owners/versions, measurable unknowns и связью с первым recipe. Пройти OOM до/после update, crash до evaluation, incompatible index, шумный feedback, новый домен, consumer со старым snapshot. **Хорошо:** adapter уменьшился, total memory выросла — compactness не доказана. **Плохо:** graph/index растут без предела, отчёт считает лишь weights.

**Будущая оценка:** simultaneous RAM/VRAM peaks, persistent/temporary bytes, end-to-end latency, train/reindex/restore times, prequential accuracy, forgetting, время до полезной коррекции и внешние вызовы. Передача R24/R25; утверждения PC-fit остаются NOT_MEASURED до реального опыта.

## R24 — связать алгоритмы с реальными пользовательскими сценариями

**Роль/skills:** архитектор предметной приёмки и consumers; A, P, M, C. **Входы:** original requirements R02, все slice maps, R16/R19, R21–R23, актуальные external consumer contracts по source map.

### Промпт

Опиши end-to-end наблюдения, не новое e2e coverage. Для каждой линии покажи request→domain/intent→representation→four-role path→answer→feedback→update→следующий пользовательский результат. У каждого перехода реальный owner/type/state identity и способ наблюдения; недоступный consumer не объявляется проверенным.

DTC сохраняет исходную цепь: дерево/HTML/CSS/images→API→learning processing→несколько разрешений→visual/semantic/cost evaluation→БД и допустимые источники→DPO→обучение→публикация состояния→повторный consumer, с user-tier budget. IDE сохраняет реальные подтверждённые VS/VS Code/Continue и дополнительные исходные integrations: выбранный local model/path, видимый ответ, отсутствие скрытого expensive fallback, осмысленный feedback. DPO/DynamicGraph линия показывает не только пары на диске, но и изменение нужного authority и использование состояния получателем.

Составь `original expectation → domain/level → algorithm/connection → source/code owner → future observation → metric → readiness class`. Семь baseline доменов и дополнительные цели остаются; Unit many→many, Flow correction, async Orchestrator, higher-order Graph наблюдаются отдельно. Core gain не закрывает native Windows/IDE delivery; наличие canonical bytes не закрывает подключение UI. Отложенные обязанности имеют trigger/owner.

**Выход/приёмка:** применённые user scenarios и capability examination matrix. Для каждой исходной цели есть конкретное наблюдение, не только task ID. Один сквозной happy path, один wrong-domain/invalid-path и один correction→next-input сценарий показывают expected и NOT_MEASURED без придуманных actual результатов.

**Хорошо:** core обновился, app ещё использует старый checkpoint — integration gap остаётся. **Плохо:** «файлы опубликованы, продукт готов». **Края:** budget меняет допустимые branches, offline mode, mismatch user/tenant data, noisy correction, nondeterministic but correct output. Будущая оценка: качество реального ответа, full time/cost, mapping/retention и snapshot identity у consumer; передача R25.

<!-- R24-CURRENT-CAUSAL-REPAIR-2026-10-13 -->
### R24 after repaired contract was run (documentary only)

The earlier sentence “R24_REEXECUTION_REQUIRED until the repaired prompt is run” records **historical pre-run status**. The repaired prompt has now been executed as a source-backed **documentary R24**, with its full record and A–F bounded child prompts in [R24-CAUSAL-REPAIR-2026-10-13.md](R24-CAUSAL-REPAIR-2026-10-13.md) and current five [R24 carriers](results/R24/). R01–R20 semantic FINDINGS snapshot/refresh was documented; R23 results were found and included, not assumed missing. All original algorithm/product requirements remain; no runtime learning, model invocation, independent Astra review or consumer adoption is asserted. Future R24 rereview should take these applied bytes as baseline and only reopen specific semantic deltas.

## R25 — отдельная итоговая экспертиза и конкретное следующее действие

**Роль/skills:** независимый Astra Ultra reviewer, не автор проверяемых решений; P, A, M, C, E. Requested configuration — Astra Ultra согласно текущему поручению; historical endpoint string не доказательство доступности. Запиши actual model/configuration, invocation, reviewed SHA и scope. Отсутствие отдельного reviewer = NOT_PERFORMED, не авторский independent PASS.

**Входы:** actual Rxx RUN-LOG/results; whole-program source/requirement map; R15 findings; применённые R16–R20; R21–R24; live README/AGENTS/START/roadmap; первый R18 implementation packet и R13/R22 recipe; original/WorkLeft/RAG→primary/code anchors. История и независимые результаты не переписываются.

### Промпт проверяющему

Проверь план следующего действия, не готовность уже обученного продукта. Различи реально выполненные Rxx, partial и not started; 25 prompts не означают 25 runs. Сопоставь исходные требования со всеми roles/domains/slices/three directions и реальными следующими пакетами. Уникальные ценные решения из R01–R03 не должны исчезнуть в итоговом summary.

На текущем candidate проверь причины R15 findings, не только изменённые формулировки. Пройди первый учебный цикл, второй иной домен и user correction. Найди hidden Windows/coverage prerequisite, несуществующий API, незаполненный task packet, неверное DPO conditioning/reference/target, неопределённую attribution, LoRA-as-tokenizer, невалидную metric/index комбинацию, переопределение арбитра similarity, утечку через retrieval, fast-wrong branch, unbounded correction loop, незаметный replay и неполный PC footprint.

Различай design blocker и empirical hypothesis. Missing owner/schema/permission/state protocol требует точного исправления; неизвестное преимущество алгоритма при готовом безопасном сравнении не требует бесконечного replanning. Проверь, что operational single-pair admission/promotion не смешаны со статистическим bulk conclusion. Порог 95% остаётся goal, не выдуманным achieved score. Экспертное мнение дополняет actual measurements, не подменяет их.

У итогового handoff должен быть один ближайший implementation/experiment packet с разрешёнными действиями и data/state/metric/resource inputs; не «сначала разработать ещё план». Если нужен дополнительный адресный раунд, требуй обоснованную полностью заполненную extension-карточку по PLAN и учёт вызовов. Нет автоматического R26/30/50 и нет запрета необходимой работы ради красивого счётчика. Изменение candidate после review требует проверки затронутой дельты; автор не выдаёт самопроверку за отдельный вызов.

**Выход/приёмка:** candidate-bound verdict, закрытие/остаток R15, конкретные remaining blockers и affected requirements, первый точный implementation prompt + первый recipe, available permissions/resource envelope и честный перечень NOT_MEASURED. Remote readback относится к reviewed bytes. Отдельный reviewer действительно существует либо явно NOT_PERFORMED.

**Хорошо:** следующий опыт определён; выигрыш hybrid/PC-fit предстоит измерить. **Плохо:** «25 файлов готовы — модель обучена, Windows закрыт». **Края:** отсутствует requested endpoint, одновременно изменившийся source blob, partial R03 без влияния на first seam, provider/data admission gap. Конечная передача сохраняет историю и позволяет начать уже определённую ограниченную работу по соответствующему поручению, а не обнуляет программу.

## R24 repair override — parallel-input causal acceptance

This override supersedes weaker R24.A/B wording where there is a conflict. R24 must not start from a fixed R21–R23 summary only. At invocation it snapshots the observable R01–R20 carriers under `PARALLEL-R01-R20-CONTRACT.md`, consumes claim-relevant semantic receipts without waiting for global DONE, and refreshes once before publication. Missing sibling evidence limits only the dependent claim.

R24 output is incomplete unless it contains: (1) whole-program/original/WorkLeft/RAG receipt preventing S04-only scope; (2) causal rows for Unit/Flow/Orchestrator/DynamicGraph with deterministic law, learned state, update owner, ordinary consumer and falsifier; (3) domain-law→mapper→token/vector/projection/index→dynamic-kNN→consumer rows for the used curriculum families; (4) DPO positive/negative pair construction and exact-pair judgement separated from Student effect; (5) LoRA→representation→consumer crosswalk; (6) deterministic-only vs learned-only vs hybrid comparisons; (7) bulk-to-bulk accuracy/utility/time/full-PC/retention examination; (8) DTC, IDE and DPO/DynamicGraph ordinary-user journeys ending in a cold selected-state request. Coverage, green CI, Windows Actions, lower loss or a stored checkpoint cannot close a learning row.

Use the complete model-ready prompt in `run-prompts/R24.md` and the detailed acceptance contract in `R24-DTC-IDE-ORDINARY-ACCEPTANCE.md §8`. The pre-repair R24 results are historical bounded evidence; execution status is `R24_REEXECUTION_REQUIRED` until the repaired prompt is run.