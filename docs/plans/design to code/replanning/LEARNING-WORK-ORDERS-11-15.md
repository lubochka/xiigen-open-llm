# R11–R15 — решения об алгоритмах, измерениях и первом учебном пути

**Редакция 2026-10-13 / CP3. Пять подготовленных аналитических поручений, не выполненные исследования или экспертизы.** Базовая очередь — [PLAN-25-RUNS](PLAN-25-RUNS.md). Авторство этого файла не завершает R11–R15.

## Общая часть полного промпта

Выполни только назначенный Rxx. Planning: `lubochka/xiigen-planning`, branch `llm_plan3`, D = `llm mvp/design to code session`. Core: `lubochka/llm_mvp_core`, branch `codex/dpo-implementation`. Зафиксируй фактические refs и прочитанные диапазоны. Источники R01–R10 — проверяемые handoffs, не доказательство, что ты сам прочитал весь архив. Потребляй достаточный подтверждённый subset; незакрытая независимая тема не блокирует анализ другой темы. Спорный источник ограничивает именно зависимое решение.

Собери один prompt: эта общая часть + свой раздел ниже + одноимённый Rxx из `HYBRID-ALGORITHMS-AND-RUNS.md` и необходимые H/source spans. `SOURCE-ROUTES.md` определяет маршруты, `COMMON-CONTRACT.md` — evidence schema, `COVERAGE-AND-WORKLOAD-GUARD.md` — распределение нагрузки, `LEARNING-EVALUATION-CONTRACT.md` и LF01–LF12 из `LEARNING-EXAMINATION-CONTRACT.md` — сохранённую функциональную оценку. Не требуй пользователю соединять тексты; не загружай все Rxx и повторные PLAYBOOK/RECIPES каждому worker.

**Смысл задачи:** приблизить измеримое обучение всей системы, не новый цикл обслуживания проверок. Сохраняются исходные prompts/WorkLeft/RAG, S00–S10 и дополнительные этапы, DPO/DynamicGraph, DTC и IDE, семь baseline доменов и подтверждённые дополнительные направления. Unit/Flow/Orchestrator/DynamicGraph — разные функциональные роли; реальные ILogical*/generic contracts и ILearningMachine training owner определяются по коду. Many→many, коррекция, async и higher-order patterns не заменяются одним линейным Unit.

**Навыки:** общие `planning-session-startup-SKILL.md`, `architect-behavior-classifier-SKILL.md`, `output-readability-gate-SKILL.md`; S = `reference-inventory-and-source-map-SKILL.md`; P = `phase-planning-SKILL.md` + `plan-review-SKILL.md`; A = `trainable-architecture-decision-SKILL.md` + `human-readable-architecture-audit-SKILL.md`; C = `code-examination-SKILL.md` + `generated-code-review-SKILL.md`; M = `experiment-design-SKILL.md` + `improvement-measurement-SKILL.md`; E = `prompt-patch-authoring-SKILL.md`. Файлы — в core/docs/skills. Реально прочитай применимые разделы/зависимости и укажи глубину. Исторические 30/50 reviews не переносятся в эту программу.

**Разрешено:** чтение, анализ существующего evidence, документная проверка, публикация только своих `D/replanning/results/Rxx/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. Архитектор выбирает/объясняет и проверяет; живые требования/roadmap применяют R16–R20. Не менять чужие results, code/tests/workflows/weights/datasets/core skills; не запускать product tests/coverage/Windows/Actions/build/training, product-generation campaigns, установки/cleanup/force-push. Назначенная отдельная planning-expertise R15 не означает разрешение этих продуктовых запусков. Fresh SHA перед записью, non-destructive reconcile при конфликте, remote readback после.

**Возврат:** человеческая цель; sources/refs; альтернативы и контраргумент; принятое решение с границей доказанного; exact artifact; сегодняшняя проверка; будущий опыт/owner; good/bad/edges; ограниченный handoff. NOT_MEASURED не заменяется нулём или выдуманным score. Requested reviewer/model не равен actual invocation. А/Б — последовательные части одного Rxx; реальный child получает полностью собранную общую часть, свою цель, источники, выход, примеры/края, проверку и return-only полномочия. Дополнительные вызовы учитываются, независимость не имитируется.

## R11 — выбрать изменения процесса по причинным зависимостям

**Роль/skills:** аналитик причин и приоритетов; S, P, M. **Входы:** R09 cost/diff evidence, R04–R07 reusable/missing seams, R08 measurement inventory, R02 requirements, относящийся R10 evidence brief. R11 может разбирать доказанную process-часть, пока независимая алгоритмическая гипотеза уточняется.

### Промпт

Построй причинную схему: что именно задерживает первый достоверный учебный результат? Рассмотри missing trainer wiring, неподходящие пары, недостоверную оценку, representation mismatch, реальные ресурсы, лишние Windows-gates, повторное планирование и дублирование реализации. Для каждого объяснения запиши факт за, факт против, неизвестное и наблюдение, которое изменит рекомендацию. Не превращай подозрение пользователя о подгонке tests в установленный факт; не оправдывай старую очередь количеством зелёных проверок.

Для каждого предлагаемого stop/defer/reuse/do-now укажи защищаемое свойство, зависимость до первого опыта, уже имеющееся evidence, стоимость изменения и риск. Различай time-to-first-valid-lesson и time-to-first-accepted-improvement: корректный опыт без прироста — полезный результат, но не accepted learning gain. CI duration не равна времени человека; отсутствующая стоимость не ноль. Когда чисел нет, используй обоснованный качественный порядок, не точные выдуманные часы.

Проверь, не заменяется ли test-fitting новой подгонкой judge/rubric/панели. Отложенный release gate получает owner и trigger возвращения. Сохранение состояния, происхождения данных и корректного update нельзя убрать просто потому, что они не повышают accuracy сами по себе.

**Выход и документная приёмка:** ранжированная causal decision table в DECISIONS; у каждого верхнего пункта есть source→защищаемое свойство→решение→falsifier→owner. FINDINGS сохраняет альтернативные объяснения. Не менять workflows.

**Хорошо:** убрать fixture из критического пути, потому что подтверждённый training path его не использует, сохранив Windows-поставку отдельно. **Плохо:** отменить все проверки или написать ещё guard вместо устранения зависимости. **Края:** длительная очередь при коротком compute; скрытый paid fallback; неучтённые reruns; ускорение job без приближения update.

**Будущая оценка/передача:** время до пригодного baseline/урока/улучшения, полная цена попыток и оценки, ошибки данных/restore. R14 получает причинные зависимости, R17 — точные live clauses для изменения.

## R12 — выбрать минимальную согласованную гибридную архитектуру

**Роль/skills:** архитектор обучаемых механизмов; A, C, M, P. **Входы:** R04 representation/ports, R05 pair/objective/state, R06 domain/kNN/graph, R07 connections, R08 laws/data; WorkLeft RESEARCH-02/03; исходные четыре роли и DPO-family через R03. Core anchors: `00.Core/LogicalContracts.cs`, `00.Core/DynamicGraphContracts.cs`, `00.Core/LearningMachineContracts.cs`, `21.AgentInfrastructure/Logical/DynamicGraphEdgeAuthorityContracts.cs`, `10.LoRA/LoraLinear.cs`, а также найденные реальными traces trainer/index/consumer methods.

### Промпт

Выбери один первый baseline и один минимальный challenger по текущему коду. Решение должно сохранять цепь: domain/intent → представление → допустимые кандидаты → learned выбор → typed Unit/Flow/async Orchestrator/DynamicGraph → ответ → диагностика → допустимая пара → ILearningMachine → оценка/выбор состояния → ordinary consumer. Не создавай второй pipeline рядом с существующим. Для каждого порта нужны schema/coordinate version, multiplicity, state owner и последствия ошибки. Для каждого алгоритма — deterministic часть, learned authority, objective, изменяемое состояние, control и falsifier.

**А. Представление и соседство.** Раздели домен, intent, язык, тип входа/выхода. Неизвестный/смешанный домен не заставляй насильно попадать в один enum. Выбери предметные признаки по законам R08: единицы/числа/отрицание; AST/side effects; NULL/cardinality; layout/tree/viewports; зависимости действий. Различай tokenizer/token IDs, semantic vectors, структурные features и learned output projection.

Рассмотри как проверяемого кандидата `d_d(x,z)=Σ_b w_{d,b}(x)·δ_{d,b}(φ_b(x),φ_b(z))/s_{d,b}`: допустимость домена/типа/версии/происхождения проверяется до ранжирования; scales получают только из допустимого training/dev материала. Выбери реальные feature blocks, не оставляй формулу без семантики. Query-dependent weights обычно не гарантируют metric properties: нельзя автоматически обещать совместимость с любым ANN index. Выбери совместимый index либо surrogate retrieval + exact rerank и включи потери recall/стоимость. Альтернатива для сравнения — фиксированная metric, либо обучаемая PSD Mahalanobis `M=LᵀL` при подходящих признаках. Это варианты исследования, не утверждение, что такой метод уже есть или лучше.

Определи bounded k/radius и поведение при малом/пустом pool; положительные, отрицательные и warning соседи сохраняют разные роли. Learned k/weight policy должна иметь сигнал, состояние и update owner. Смена конфигурации вручную не обученная адаптация. Similarity и стоимость не отменяют final arbiter.

**Б. DPO, LoRA и связи.** Для стандартного DPO используй точное соответствие реализации цели:

`L = -E log σ(β[(log πθ(y+|x)-log πref(y+|x))-(log πθ(y-|x)-log πref(y-|x))])`.

Назови общее conditioning x, masked output tokens, reference policy/version и её жизненный цикл, β и фактические trainable parameters. Для vector/path policy нужна собственная корректная вероятность кандидата/плотность и objective; similarity score сам по себе не log-probability и не DPO. Source [P1] объясняет стандартную цель, не доказывает соответствие текущего trainer.

Покажи, как preference между путями обучает нужный authority unit через существующего ILearningMachine. Если два пути отличаются несколькими причинами, нельзя автоматически наказать каждое ребро проигравшего пути: раздели контекст/кандидаты, подготовь более различающую пару или явно отметь неидентифицируемую attribution. Rich GraphDpoPair/trace должен пережить loader/serialization и достигнуть target; четыре строковых поля LearningTrainingItem сами по себе не доказывают потерю — контекст может быть сериализован в Prompt. Это требуется проследить.

Current unit-level training boundary сверяй по первоисточнику; graph-level measurement не разрешает отдельный Train на каждом Unit/Flow. Недостаточная выразительность boundary — migration gap с решением/owner, не причина убрать graph learning. One-pair/no-repeat и полное состояние сохраняются. После сбоя нельзя выполнять второй gradient поверх неизвестного/уже обновлённого состояния; допустимость retry после точного restore сверяется по действующему протоколу и lineage, не выдумывается.

LoRA — конкретная projection: для текущего LoraLinear `base(x)+Scaling·((dropout(x)Aᵀ)Bᵀ)`. Назови shapes, target matrices, actual optimizer ownership, inference consumer и coordinate/index migration. Не объявляй LoRA tokenizer или доказанным смысловым преобразованием только по форме вектора. Сохрани full-state/RNG/optimizer/index recovery.

**Выход:** DECISIONS: выбранная архитектура, альтернативы/почему отклонены, реальные owners/ports, current→target, минимальный seam, критерий полезности, fallback и восстановление. А возвращает representation/retrieval contract и empty-pool counterexample; Б — objective/authority/consumer contract и wrong-attribution counterexample. При делегировании это два полностью собранных return-only packets, а не «исследуй половину архитектуры».

**Приёмка:** пройти известный, mixed/unknown domain, many→zero/many outputs, wrong-sign pair, несовместимый index, correction loop, fast-wrong async branch и OOM. Для каждого определены действие и state owner. Эмпирическое «какой k лучше» получает опыт, а не выдуманный ответ; невыбранный API/owner/контракт нельзя спрятать в future gate.

**Будущая оценка:** deterministic / learned / hybrid на одинаковой доступной информации; admissibility и quality прежде cost; regret только относительно исследованных допустимых вариантов, не глобального optimum. Отдельные contrasts adapter off/on, fixed/adaptive neighbors, flat/higher-order graph. Отсутствие выигрыша допустимо. Передача R13 и R18.

## R13 — causal experiment compiler (REPAIR REQUIRED)

**Governing prompt:** [R13-REPAIR-WORK-ORDER.md](R13-REPAIR-WORK-ORDER.md). It supersedes the older R13 wording below wherever they conflict.

R13 is no longer accepted as an architecture-neutral protocol only. It MUST semantically consume the latest R01–R06 carriers, reopen applicable primary WorkLeft/RAG/original DPO/dynamic-flow sources for consequential choices, populate one real first-experiment card, and emit the next bounded interaction queue.

The first experiment is chosen by causal identifiability and information value / full implementation+teacher+judge+evaluation+reindex+rollback cost. It must name the actual learned owner/state/signal, deterministic guards, ordinary consumer, same-panel baseline/challenger/no-update control, lawful domain families, falsifier, retention and full PC cost.

Required algorithm coverage across the first card + queued cards: DPO pair integrity/objective/adoption; domain mapping; domain-sensitive token/vector representation; dynamic-kNN/reranking; LoRA/projection where relevant; ILearningUnit; ILearningFlow; ILearningOrchestrator; ILearningDynamicGraph; feedback/rollback.

No coverage, CI, Windows run, changed hash, nonzero gradient or lower loss may close a learning-efficacy claim. Deterministic mechanism checks remain bounded evidence only.

Current status: **R13_REPAIR_REQUIRED / LEARNING_NOT_MEASURED** until the governing work order stop conditions are satisfied.

## R14 — построить короткий critical path без потери всей программы

**Роль/skills:** планировщик зависимостей; P, A, M. **Входы:** R02 obligations, R11 causal costs, R12 selected seam, R13 first recipe, R04–R06 slice maps.

### Промпт

Раздели непосредственно необходимое первому локальному уроку, расширение алгоритмов/доменов и конечную DTC/IDE/Windows-поставку. Для каждой старой зависимости укажи причинное основание сохранить сейчас либо перенести с owner/trigger. Не делай все S00–S10 обязательным предисловием к одному update; не вычёркивай конечные API/рендеры/БД/публикацию и consumer budget.

Построй путь: usable baseline → минимальный отсутствующий dynamic seam → новая пара/update/comparable observation → принятие или честное опровержение гипотезы → второй отличающийся домен → точечные connection contrasts → full compactness/feedback → необходимые поставки. Не требуй идеальных tokenizer/kNN/LoRA сразу во всех доменах. При достаточном узком implementation packet будущая отдельно разрешённая работа не ждёт всего curriculum/platform polish.

У каждого пакета один основной эффект, реальный owner, inputs/outputs, cost limit и stop condition. Независимые reads могут идти одновременно; изменения одного checkpoint/index/общего файла сериализуются. R01–R03 продолжают свои snapshots/results без reset. R10 общий join не означает повторить чтение всех источников.

**Выход/приёмка:** causal DAG и ближайший измеримый packet для R18, retained/deferred obligations, capacity и отсутствующие предпосылки. Каждая обязательная dependency должна влиять на безопасность/смысл именно этого опыта. Неизвестный empirical parameter с готовым опытом не бесконечный design blocker.

**Хорошо:** baseline уже есть — первая работа подключает отсутствующий consumer, не пишет duplicate trainer. **Плохо:** закончить Windows suite, потом придумать урок. **Края:** первый домен готов, другой независимый ещё нет; частичный RAG handoff; reviewer finding влияет лишь на один seam; реальный объём требует extension. Последнее оформляется заполненной карточкой по PLAN, не скрытым дополнительным циклом.

**Будущая оценка/передача:** время до урока/полезного улучшения отдельно, domain breadth, external calls и full resource envelope. R15 проверяет направление, R18 готовит точные implementation prompts, R19 применяет очередь.

## R15 — независимая предметная экспертиза направления

**Роль/skills:** отдельный архитектор-ревьюер, не автор R11–R14; A, C, P, M. Запрошена **Astra Ultra**. Строка `gpt-6-astra / ultra` в историческом role contract не является подтверждением доступного endpoint. В реальном запуске запиши requested и actual model/configuration, независимый invocation и exact candidate SHA. Нет отдельного вызова — NOT_PERFORMED, не self-review PASS.

**Входы:** оригинальные требования и WorkLeft RESEARCH; R03 primary routes; R10–R14; критические current code seams и не только авторский summary. Используй бюджет предметной проверки, не повторный аудит всего репозитория.

### Промпт проверяющему

Попытайся опровергнуть пригодность выбранного первого опыта и сохранение всей архитектуры. Проверь смысл четырёх ролей, deterministic candidates versus learned authority, domain mapping и подходящую metric, negative/warning memory/final arbiter, реальную DPO вероятность/reference/направление/target, graph→unit credit attribution, LoRA projection→consumer и полный state restore. Проверь, что обучение связи не заменено её красивым именем, а alias не породил duplicate API.

Рассмотри mixed/unknown domain, loss of negation/units, empty pool, wrong-sign/TIE/BOTH_BAD пары, многопричинные проигравшие пути, быстрый недопустимый async result, correction loop, incompatible vector space/index, teacher/retrieval leakage, user poisoning/noisy feedback и OOM. Оспорь обещание compactness: включены ли все расходы, проигравшие ветви и неудачные уроки? Оспорь обязательный выигрыш каждого компонента: null result должен оставаться допустимым выводом.

Проверь operational one-pair decision отдельно от bulk evidence; нельзя замаскировать replay или новый статистический embargo. Проверь сохранение 95% product goal без выдачи его за achieved score, всех доменов и конечных consumers. Требуй минимальную исправляющую правку и различающий опыт, не новое framework там, где хватает существующего measurement path.

**Выход:** candidate-bound DECISIONS с verdict «направление пригодно для опыта / конкретные изменения / недостаточно источников»; source, affected expectation, falsifier и owner R16–R20 на каждое замечание. Reviewer не применяет собственные изменения и не принимает их как независимый автор. Исправления проверяются по затронутому candidate; дополнительный вызов считается явно, не 30/50 по умолчанию.

**Приёмка:** существует настоящий отдельный review packet и trace к первичным источникам. **Хорошо:** architecture coherent; superiority dynamic kNN пока гипотеза с определённым contrast. **Плохо:** «Astra одобрила — обучение эффективно». **Края:** requested endpoint недоступен, candidate изменился после review, автор выдал А/Б за двух reviewers. Фиксируй методический пробел, не выдумывай invocation. R25 проверяет устранение R15 findings и итоговую передачу, а не отменяет неизвестные measurements.

## Первичные методические ссылки для R12/R13

Эти работы задают сравниваемые методы, не подтверждают реализацию или выигрыш XIIGEN:

- [P1 — Direct Preference Optimization, Rafailov et al.](https://arxiv.org/html/2305.18290): §4, objective и policy/reference log-ratios. Сопоставить с actual trainer, не переносить название на arbitrary scorer.
- [P2 — LoRA, Hu et al.](https://arxiv.org/abs/2106.09685): low-rank parameter adaptation. Project-specific матрицы и ownership подтверждать по `10.LoRA/LoraLinear.cs` и consumers.
- [P3 — Distance Metric Learning for Large Margin Nearest Neighbor Classification, Weinberger & Saul](https://jmlr.org/papers/v10/weinberger09a.html): learned Mahalanobis как кандидат сравнения, не универсальная лучшая metric для всех доменов.
