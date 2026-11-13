# 15 · RP01 — опубликованные исправления Main500 и их перенос в атлас

**16 октября 2026 · ATLAS_RP01_MAIN500_SYNC_20261016 · документная интеграция.** Поручение: внести в атлас изменения RP01, опубликованные в предыдущем ответе. Это не новое обучение, не повтор R01–R25 и не второй набор продуктовых заданий. Обновляется существующий раздел15; исходные номера AT-B500-01…12 сохраняются.

## 1. Что уже опубликовано и что переносится сейчас

Действующая редакция RP01 — **B500-LIVE-RUN-REPAIR-20261015**, package version4.0. Её три коммита:

| Коммит планирования | Реально изменённая область |
|---|---|
| [60daca28](https://github.com/lubochka/xiigen-planning/commit/60daca28bd6ed268665dae60f76a1b9beae42af8) | Диспетчер и карта кода: сначала состояние уже выданного Main500, не повторный запуск |
| [1bbb8141](https://github.com/lubochka/xiigen-planning/commit/1bbb8141a5bccfba54e93bf73cff046c3a72b77c) | Пять полных B500 карточек, README, основной контракт и общий контекст |
| [0082b7aa](https://github.com/lubochka/xiigen-planning/commit/0082b7aad136539754e00d91433d1cc1b0f7cea3) | PACKAGE-MANIFEST, AGENTS и execution board; согласование пяти primary tasks и14 inline children |

Всего **14 различных существующих файлов RP01**, а не14 реализованных алгоритмов: `RP01-EXECUTE-NOW.md`, `RP01-CURRENT-CODE-MAP.md`, `README.md`, `RP01-OFFLINE-TEACHER-BATCH500.md`, `RP01-BATCH500-EXECUTOR-REPAIR-2026-10-15.md`, `EXECUTION-BOARD.md`, `PACKAGE-MANIFEST.json`, `tasks/AGENTS.md`, `tasks/batch500/README.md` и пять файлов `RP01.B500.01-DATA.md`…`RP01.B500.05-DOMAINS.md` в `tasks/batch500/`.

При нынешнем переносе прочитан Atlas snapshot P=`5f11281d50463535050bef641a46cb87c978ff3c`. Сравнение с0082b7aa показало только последующие RP06/RP03 изменения, без изменения этих RP01 файлов. Это повторная сверка опубликованных изменений, **не повторный полный аудит core, всех WorkLeft/RAG или всех слайсов**. Новые правки атласа не присваивают себе прежнюю работу и чужие findings.

## 2. Один вход и правильное ближайшее действие

Исполнитель открывает [RP01-EXECUTE-NOW](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-EXECUTE-NOW.md), [current code map](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md), затем **одну** карточку из [пяти B500 tasks](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/batch500/README.md) вместе с [общим контекстом](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-BATCH500-EXECUTOR-REPAIR-2026-10-15.md).

Актуальный машинный вход — **[PACKAGE-MANIFEST.json](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/PACKAGE-MANIFEST.json)**, а не прежняя ссылка атласа на `BATCH500-MANIFEST.json`. Старый manifest не является альтернативной инструкцией. Основной контракт — [BATCH500](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-OFFLINE-TEACHER-BATCH500.md).

**Сначала B500.03/03T: read-only reconciliation у исходного владельца уже выданного `simulation-csharp-main500-20261015-01`.** В прочитанном ранее K58 уже были offline runner, данные, C0-артефакты и несколько завершённых step windows. Это не live-PC наблюдение и не доказательство завершения всего Main500. Нельзя повторять calibration/Main, удалять issued marker, менять run ID для обхода защиты или создавать второй writer. Если Main завершён — использовать его настоящий checkpoint/readback и выполнить только недостающие исходные T scoring/free generation и blind grading. Если эффект неизвестен — восстановление через фактического владельца, не угадывание состояния по отсутствующему GitHub файлу.

## 3. Сохранённые ID исправлений: причина, действие, свидетельство

| ID | Уточнённое действующее решение | Почему и как проверить | Владелец |
|---|---|---|---|
| AT-B500-01 | Переиспользовать существующие C#500 и их настоящее `simulation` происхождение; новые прямые Astra outputs оформлять отдельно | Не пересоздавать данные и не выдавать программные шаблоны за500 модельных вызовов; exact bytes/source/receipt binding | B500.01/02 |
| AT-B500-02 | Ни local Qwen author, ни local Qwen evaluator не входят в нынешний inner loop | Offline author при оставшемся синхронном evaluator не устраняет тормоз; source path и actual calls, не записанный вручную0 | B500.02/03 |
| AT-B500-03 | Reuse существующей calibration и step observations; новый probe только для новой выбранной гипотезы после reconciliation | Прежний mandatory probe1–4 больше не ближайший шаг; time-to-learning не увеличивается повторными gates | B500.03/03P |
| AT-B500-04 | Current main принимает exact500, batch10, max50 updates,1epoch; first50 streaming не утверждается реализованным |25 templates×20 не500 независимых families; count/max не actual completed; данные/токены/updates раздельно | B500.01/03 |
| AT-B500-05 | Reuse existing trainer; continuous optimizer/reference внутри запуска не равен generic resume/descendant API | Нельзя обещать отсутствующую команду или500 legacy train-and-hold; следующий parent/domain seam ограничен прежним owner | B500.02/03/05 |
| AT-B500-06 | Не повторять сохранённый совместимый C0 и не добавлять legacy60-slot prefix | Reuse по реальным code/state/input/decoder/protocol bindings; кодовый эффект отделён от изменения весов | B500.03/04 |
| AT-B500-07 | Сохранить точный frozen25+3 критерий текущего опыта; маленький prospective semantic gain допустим для следующего | Положительная margin не равна правильному свободному ответу; старые пороги не меняются после outcomes | B500.04 |
| AT-B500-08 | Не добавлять DEV10/4,40+20 или промежуточные behavioral точки как обязательства current Main500 | Отсутствующий промежуточный замер — NOT_MEASURED, не основание обучить500 заново; новый дизайн только NEXT_CAMPAIGN | B500.03/04 |
| AT-B500-09 | Подготовить настоящий EN→RU consumer и500 новых пар; C# retention от immediate parent при поддержанной continuation | Нынешняя CLI жёстко привязана кC#/B0; label rename не multidomain; третья500-парная кампания не обязательна | B500.05/03 |
| AT-B500-10 | Сохранить law-aware representation/mapping/kNN и четыре архитектурные роли | Не k>1/majority vote вместо предметного смысла; один matched causal contrast, actual consumer и стоимость | B500.05/RP02–RP04 |
| AT-B500-11 | Docs/read/reuse=0 новых product tests; changed deterministic risk проверяется ближайшим имеющимся средством | Нет Windows/coverage/review-count waterfall; source/masks/tensors/journal/reload/serving safeguards не удаляются | Все B500 |
| AT-B500-12 | Сохранить old0/12 verdict, уникальные legacy решения и согласовать все активные входы | README/dispatcher/board/AGENTS/PACKAGE-MANIFEST указывают5 cards/14 children; исторические16 tasks не default | B500.04 и planning owner |

Это12 сохранённых пунктов документного ремонта, не12 новых находок, дополнительных раундов или измеренных улучшений. Публикация атласа не запускает ни один из перечисленных runtime эффектов.

## 4. Текущий frozen опыт и будущая кампания — разные режимы

Источники ранее прочитанного K=`58d12ecfe7af05e3221f984bf6c4c67d82fe3b96`:
[runner README](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/tools/Rp01SimulationDpo/README.md), [Program.cs](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/tools/Rp01SimulationDpo/Program.cs), [dataset manifest](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/.kiro/xiigen-slices/work/results/RP01-astra-synthetic-dev-csharp-500-20261015/dataset-manifest.json), [frozen runtime manifest](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/frozen-runtime-manifest.json), [frozen protocol](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/.kiro/xiigen-slices/work/results/RP01-simulation-exact-review-and-heldout-20261015/frozen-protocol.json). Они закреплены на snapshot, не объявляются текущим состоянием PC.

| CURRENT_FROZEN | Не изменять переносом атласа |
|---|---|
| Main/data | `simulation-csharp-main500-20261015-01`; exact TRAIN SHA256 `c78a620a01cd2ab1d35962eb3521a9e87ee51deeedc325dfc809112b0e035935` |
| Обучение |500 distinct rows,25 parameterized templates; batch10, max50 updates,1epoch, fixed B0/reference; continuous optimizer внутри run |
| Время |wall `none`, конечные пары/updates и cooperative cancellation; не новый обязательный deadline |
| Оценка |25 preference cases +3 free cases как подмножество этих25,1repeat/arm;112 planned explicit scorer forwards и6 planned generations |
| Primary |delta среднего token-average chosen-minus-rejected log-probability margin, исходный tolerance1e-6; только narrow near-transfer preference signal |
| Free behavior |Исходный критерий net correct gain и хотя бы одного wrong→right; ошибки/потери отдельно; частичное улучшение descriptive |
| Evidence |Реальное completed/effect/readback и E receipts нужны отдельно; issued/max/manifest не доказательство completion/gain |

**NEXT_CAMPAIGN** выбирается только для нового ещё не начатого исследования. Здесь заранее допустимы family-balanced Q_law/partial semantic credit, небольшая DEV-панель, иной осмысленный бюджет, новый domain/parent или один алгоритмический contrast. Нет универсальной квоты DEV10/40+20 и нет обязательного+2/12/95% перед следующим информативным уроком.500пар — начальный бюджет, не гарантия освоения домена. TRAIN500 не урезать ради переименования части TRAIN в независимый экзамен.

Q_law будущего опыта: заранее заданные смысловые clauses внутри case, затем среднее внутри family и между независимыми families; critical definition и допустимые альтернативы фиксируются до ответов. Показывать positive/negative/unchanged cases и uncertainty. Повторные или переименованные шаблоны не создают независимость. Раскрытый panel в дальнейшем diagnostic/retention, не новый blind confirmation. Ни один future criterion не пересчитывает current frozen или old one-pair verdict.

## 5. Пять карточек и14 именованных дочерних промптов

| Полная карточка у owner | Children | Что действительно делать теперь |
|---|---|---|
| [01 DATA](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/batch500/RP01.B500.01-DATA.md) |01S/01A/01J |Reuse current500; отдельно новые материалы следующего домена и exact judgments |
| [02 ADMISSION](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/batch500/RP01.B500.02-ADMISSION.md) |02A/02B |Reuse готового route; один unsupported source/domain seam при наличии реального требования |
| [03 TRAIN](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/batch500/RP01.B500.03-TRAIN.md) |03P/03T/03R |Сначала existing Main reconciliation; только недостающий effect/readback; новый hotspot отдельно |
| [04 EXAM](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/batch500/RP01.B500.04-EXAM.md) |04F/04D/04E |Reuse frozen protocol и outputs; missing T/independent grading; current/future методы раздельно |
| [05 DOMAINS](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/batch500/RP01.B500.05-DOMAINS.md) |05A/05B/05C |ENRU consumer/500+retention, один law-aware hybrid contrast; подготовка параллельна C# |

**5 primary tasks,14 inline children — не14 обязательных отдельных запусков или ревью.** Исполнитель получает полный выбранный child, общий контекст, точные входы/refs/owner, применимые skills, good/bad/edge, причины ошибок, recovery, допустимые изменения и return schema. Таблица не заменяет промпт. Подробные карточки не копируются в атлас: иначе появились бы два расходящихся набора.

Подготовка05A и данных следующего домена не ждёт C# gain; один mutable Student остаётся у одного writer. Тяжёлые PC процессы не запускаются одновременно только из-за логической независимости задач. Не требуется общий DONE всех RP02–RP05, всех семи доменов или новый полный обход WorkLeft/RAG.

## 6. Роли и происхождение: не смешивать симуляцию с реальным автором

**CODEX с `astra 6 ultracode` выполняет человеческую роль выбора учебного случая S. Отдельный CODEX с `astra 6 ultracode` оценивает точную DPO-пару J.** Отдельное назначение той же requested модели E оценивает настоящие Student outputs вслепую; score/hash сохраняется до unblind. Ни requested name, ни имя папки не доказательство model invocation.

Целевой внешний допущенный открытый китайский author создаёт **ОБЕ** стороны chosen/rejected. Для новых development материалов отдельная Astra может симулировать эту роль с честным actual authorship и поддержанным admission. Историческое `ASTRA_SIMULATED_TEACHER` совместимо с прямым `ASTRA_SYNTHETIC_DEV` только при реально соответствующем происхождении и сохранённых bytes/receipts.

**Current500: фактический source `simulation`, программный materializer семантических шаблонов. Это не generic alias прямого Astra output и не500 доказанных model calls.** Не переименовывать dataset/J/hashes, не маркировать его Qwen. Для новых direct Astra outputs нужен настоящий source contract; сериализация текстов программой не меняет их авторство автоматически. Возврат Chinese provider — отдельный последующий источник, не текущий author/evaluator bottleneck.

Student всегда настоящий локальный. A/J/E — разные назначения; S/A/J не получают closed exam/gold/rubric. Разделение сессий одной модели не устраняет все общие ошибки. Реальные source/data/effect rights, privacy и release safeguards сохраняются; повторного согласования уже разрешённого действия не требуется, но фиктивные права/receipts запрещены.

## 7. Что сохраняется в алгоритмах, измерениях и функциональности

**DPO/LoRA.** Existing trainer, exact shared input и response-only chosen/rejected; shifted masks, BOS/EOS/padding/truncation и различающий токен сохраняются. Standard sequence objective и token-average evaluation margin не взаимозаменяемы. Полная формула и code-trace work order остаются в B500.03; перенос атласа не меняет objective текущего binary. Reference frozen; reference/tokenization cache зависит от exact bytes/framing/tokenizer/template/mask/context/reference/numerical version; current Student likelihood после update не берётся из stale cache. Новый autoregressive ответ и внешний grader на каждую TRAIN пару не нужны. Saved hash, numeric tensor delta, readback и free-answer gain — разные наблюдения.

**Одинаковые ответы.** Проверить actual loaded checkpoint/adapter/cache/logits и argmax boundary по необходимости. Изменённые logits при прежнем greedy тексте — не автоматический defect и не semantic gain. B0 bootstrap не даёт обещания обучения языку с нуля за одну пару или500 preferences. Warm-up/SFT — отдельная обоснованная гипотеза, не скрытая подмена DPO.

**Стоимость.** [Ранее опубликованное наблюдение](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/main-existing-cost-observation.json): четыре training-step windows101.6866939/110.2845364/111.2888932/106.7415123s. Это полные шаги, не optimizer-only. Nested checkpoint/optimizer-RNG save не складывать с outer total второй раз. Process CPU/RAM не whole-PC, внешние S/A/J/E учитываются отдельно. Новый замер — одна подтверждённая гипотеза, не instrument-everything gate перед готовым обучением. UNKNOWN не0; при0/negative gain cost-per-gain не выдаётся за выгодную конечную величину. Wall none сохраняется согласно frozen run, не отменяет finite work.

**Domain/kNN.** Mapping→смысловые роли/порядок/type/referent→применимые законы→positive/negative/warning memory→learned rerank/metric/k/radius/skip→real consumer/final arbiter. Не majority vote как замена AST/scope/nullability, отрицания, согласования конкретного referent или SQLNULL. Неуверенный POS/coreference — soft evidence, не общий hard veto. Empty/all-illegal pool — честный fallback/abstention. При retrieval miss различать pool/representation/ANN/scorer; exact/ANN сравнивать на сопоставимом материале. «Лучший путь» — лучший среди рассмотренных допустимых при данном budget, не global optimum.

**Causal contrast.** B500.05C сохраняет A=(B0,fixed), B=(C500,fixed), C=(B0,adaptive), D=(C500,adaptive): четыре inference arms над двумя Student snapshots, не четыре retraining. Student effect B−A, policy effect C−A, interaction D−B−C+A — отдельные оценки одного объявленного опыта, не обещание synergy. Нет всех входов — меньший выбранный contrast и явный NOT_MEASURED для остальных. Не менять representation/index вместе сpolicy и не называть это чистым k-эффектом; reindex/cache/arbiter costs учитываются.

**Четыре роли и семь доменов.** Unit domain/input/output schemas и typed many→many associations; Flow learned input-output correction — RP03. Async Orchestrator и higher-order DynamicGraph — RP04. Tokenizer, semantic projection и LoRA имеют разные contracts/state/consumers. Feedback/continual/retention/PC, DTC/IDE, SQL/Angular/Figma/orchestration/Builder сохраняют существующих owners; C#500→ENRU500 не закрывает остальные домены. Пользовательские ILearning* aliases сопоставляются с actual ILogical*/ILearningMachine по поведению, без дублирования trainer.

[Legacy reference reconciliation](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-BATCH500-REFERENCE-RECONCILIATION.md) сохраняет исторические решения; canonical Git path исходного `PLAN-DOTNET-LEGACY-DPO-v1.md` этим переносом не установлен. Family diversity/dedup переносим только при совместимости; старые V246/.60/.70/.010, cap3/lexical discard/[WRONG:] не новые gates. Ошибка rejected объясняется sidecar, не служебным маркером в Student target.

## 8. Буквальные примеры для проверки переноса

| GOOD | BAD и причина | EDGE / recovery |
|---|---|---|
| Передать03T actual Main handoff; completed→missing T only | Начать ещё один Main по новому ID: возможен повтор эффекта | GitHub не содержит completed: состояние PC UNKNOWN, читать owner journal |
| Current frozen25+3 используется без изменений | Требовать DEV10/40+20/Q_law или новый C0: подмена протокола/лишняя нагрузка | Более удачный будущий rubric — NEXT_CAMPAIGN, old verdict неизменен |
| `simulation` bytes и настоящие receipts сохранены | Переименовать500 в Astra/Qwen под прежним J: ложное происхождение | Direct Astra source несовместим с parser: один настоящий adapter02, не bypass |
| Stage total и вложенные save intervals раздельны | Все102–111s названы optimizer time: отсутствующая детализация выдана за измерение | Один hotspot измерить после reconciliation, не менять running frozen binary |
| ENRU consumer и поддержанный parent path перед500 | C# domainId заменён наENRU: нет настоящей multidomain функции | Нет continuation API: bounded existing-owner03/05 seam |
| «Книга…стол…он» анализируется по referent | Masculine запрещён после любого feminine: неправильный закон | Неоднозначная ссылка/неполный prefix не отвергаются как однозначно неверные |

Для docs/read/reuse — **0 новых product tests**. Для настоящего code change — nearest existing check изменённого deterministic риска, не весь suite. Нет новых Windows Actions, model calls, optimizer или публикации private данных из этого атласного поручения.

## 9. Применение к разделам атласа и история

README: текущие RP01 summary/route/roles/manifest references должны указывать на existing Main500, а не обязательный probe/first50.02: operative A/B/C/D/E читают CURRENT_FROZEN и NEXT_CAMPAIGN раздельно; не достаточно одной новой шапки над старым приказом.01/03 сохраняют общую историю/предметные ожидания; для конкретного RP01 schedule/threshold действуют этот раздел и actual owner protocol, не исторический generic budget.12/13 — история old one-pair source/recovery, не default нынешнего запуска. Sibling RP02–RP08 entries и их статусы не переписываются.

[Предыдущая полная редакция раздела15](https://github.com/lubochka/xiigen-planning/blob/5f11281d50463535050bef641a46cb87c978ff3c/llm%20mvp/design%20to%20code%20session/replanning/RP01-RP05-REPAIR-ATLAS-2026-10-15/15-RP01-BATCH500-ATLAS-INTEGRATION.md) сохраняет12 исходных sync IDs, прежние probes/Q_law/DEV предложения и детализацию cache/LoRA/hybrid. Их уникальные пригодные положения перенесены выше; конфликтующие current next actions заменены, не добавлены поверх. История первого atlas transfer:2595ec93 иc725100d; прежний read scope и авторские receipts не становятся новой независимой экспертизой.

Применены профильные подходы из уже прочитанных в этой беседе skills: архитектура/owner, phase-planning, plan-review, generated-code-review, prompt-patch-authoring и human-readable-architecture-audit. Нынешнее прямое чтение: Atlas README/02/15, replanning/AGENTS, delta0082b7aa→5f11281d; прежние source/model результаты не заявляются повторно выполненными. Публикация и remote readback проверяются по фактическим new commit/blob, не по обещанию.

**Граница:** это перенос опубликованного14-file RP01 ремонта в действующий атлас. Код core, training data, Student weights, private exam, old/current protocol и Windows workflows не изменяются. Новых product tests/model calls/обучения/PC benchmark или независимой Astra-экспертизы нет. Main completion/gain/adoption требуют настоящих свежих runtime receipts, не этого текста.

`ATLAS_SYNC_DOCUMENT_ONLY; SOURCE_REPAIR_REUSED; PRODUCT_CODE_UNCHANGED; DATA_WEIGHTS_UNCHANGED; NO_NEW_PRODUCT_TESTS; TRAINING_NOT_RUN_BY_THIS_SYNC; INDEPENDENT_ASTRA_NOT_PERFORMED`.
