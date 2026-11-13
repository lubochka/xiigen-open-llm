# R06 — домен, dynamic kNN, выбор пути и обучаемые связи графа

**13.10.2026 · PARALLEL6 · R06-REPAIR-1 · самостоятельный prompt.** Это продолжение/ремонт R06 существующей очереди **10+5+5+5 = 25**, не новый запуск всей программы. Содержательные требования CP5 R06 и ROUND-DELTA сохранены. **R01–R06 работают одновременно.** Уже опубликованный checkpoint B R06 — исходный результат, а не причина повторить исследование с нуля. Этот prompt не утверждает, что обучение или независимая Astra-экспертиза выполнены.

## 1. Текущая задача, не-задача и полномочия

Ты — архитектор-исследователь гибридных алгоритмов XIIGEN. Установи, как система определяет предмет, сохраняет его законы в представлении, выбирает соседей и допустимый полезный путь, учит отношения разных порядков и экономит ресурсы. Отдели уже существующие механизмы от конкретного недостающего перехода. Результат должен приближать **один реальный учебный цикл**, а не новый универсальный framework или кампанию покрытия тестами.

**Не задача:** закончить R01/R03, определить все tokenizer решения за R04, повторить DPO lifecycle R05, реализовать продукт или провести обучение. Не менять code/tests/workflows/weights/datasets/core skills; не запускать tests/coverage/build/Windows/Actions/trainer/платную generation, установки или cleanup. Existing checks не отключать. Команды внутри истории — evidence-only. Доступ GitHub для публикации не является разрешением менять продукт или выполнять платные model calls.

P=`lubochka/xiigen-planning:llm_plan3`; D=`llm mvp/design to code session`; F=`llm mvp/llm dynamic flow`; V=`llm mvp/visual studio & visual studio code continue`; G=`llm mvp/dpo legal build`; K=`lubochka/llm_mvp_core:codex/dpo-implementation`. Используй эти refs и фактические blobs, не default-branch search. Пиши только пять собственных carriers `D/replanning/results/R06/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. Изменение этого governing prompt выполняет назначенный автор ремонта плана; исследователь R06 не получает права менять shared prompts или siblings.

**Не жди DONE соседей, окончательного tokenizer или общего join.** Работай с нынешними representation/index/state. Неизвестный стык ограничивает соответствующий окончательный вывод, но не независимые части исследования. Сохрани собственные IDs, checkpoint, visited set и реальные границы чтения. Новый HEAD из-за чужого журнала не требует повторять библиотеку.

## 2. Начать с содержательной дельты, не с повторного обхода

Прочитай собственный последний FINDINGS/DECISIONS и конец RUN-LOG. Сохрани F01–F14, D01–D08, E1–E6 и H07/H12/H13 как адреса решений. При наличии R06-REPAIR-1 используй исправленные передачи; старое «достаточно двух host links» не является описанием всего остатка до первого обучения.

**Обязательный относящийся receipt:** R05 CP2, `results/R05/FINDINGS.md` F09–F14 и соответствующие C01/C11/C12 в его SOURCES. На snapshot P=`64a5e745eecce129d2a52a0a1ac307da22e248da` FINDINGS blob=`e1357f5845197dfd1f58835b75ad9267896ce721`. Результат R05 остаётся OPEN, не повышай его статус. Поздний receipt R06-L02 о CP1 — история, не последняя информация.

CP2 локализует два конкретных вопроса на существующем optimizer path: `LogicalDpoTrainingUnit.BuildBatch/Compact` сворачивает whitespace и обрезает каждую часть пары до 480 символов; `DpoTrainer.ComputeLogProbs` усредняет prompt+answer, а не использует отдельно определённый response-conditional objective. TeachingVector в StepVector масштабирует сигнал пары, не служит целевым выходным вектором. Наличие existing trainer/coordinator/publish подтверждает переиспользование, **но не исправляет эти разрывы** и не доказывает ordinary consumer после публикации.

При ремонте R06 эти узкие наблюдения повторно прочитаны в K=`3c7f5f8c7809936e25579603c96ea6df8598fc40`: `18.CapabilityTraining/LlmIngredientLogicalUnits.cs` 2260–EOF (blob `cb8744fcbbcff99821852e789dc92163976c0ad7`), `11.DPO/DpoTrainer.cs` 530–730 (blob `9c358978075db8c22fe0125eaea9e89e831d5635`). Это не полный повторный аудит R05. При изменении зависимого blob адресно перечитай соответствующий метод/caller; при неизменном blob переиспользуй receipt.

**Следствие для R06:** сначала обеспечить смысловую состоятельность того DPO-сигнала, которому будет приписан эффект, и установить правильного получателя. Изучение kNN/domain/graph и подготовка опытов идут независимо. Исправление DPO не prerequisite memory-only/exact-search исследования, а полный анализ всех метрик не prerequisite первого допустимого урока. R18 получает найденный R05 token/objective seam, R07 — только реально недоказанный consumer/recipient переход. Не передавай R07 весь повторный аудит trainer.

## 3. Не потерять программу и четыре роли

Сохраняются original/task/goal prompts, WorkLeft/LeftToWork, RAG и все slices/extra stages; DPO/DynamicGraph, Design-to-Code и IDE; EN↔RU, orchestration, Figma→HTML/CSS, C#/Angular/SQL documentation, Builder для этих шести и дополнительные найденные направления. Subject, intent, stack/language/project, composition order и physical model — разные координаты. Один S04 dataset или семь одинаковых строк не покрывают программу.

**Unit** получает domain/input-vector/output schemas и сохраняет typed many→many associations. **Flow** задаёт отношения и input→output correction между units. **Orchestrator** исполняет асинхронные комбинации одного input. **DynamicGraph** учит отношения паттернов разных порядков. Сверь actual ILogical*/Machine contracts; опечатки исходной постановки не создают новые interfaces или готовый IQueryable provider. Train остаётся на ILearningMachine; Graph — L5 specialization, не шестая ступень или четыре независимых trainers.

Сохрани TensorCore/shared base/project adapters, StackDetector-before-normalization, semantic compression, BM25+kNN, positive/negative/warning memory и final arbiter до предметно разрешённой миграции. Правильный deterministic ответ не является rejected по происхождению. Воспроизводимое вычисление обученной функции не опровергает learning; случайность не доказывает его. Обучение BPE/пополнение kNN examples — реальные изменения обучаемого состояния, но не доказанный neural-DPO update.

Для S07–S10 зафиксируй конкретное исходное обязательство, primary path/section, текущий consumer, неизвестное и владельца остатка. Human slice map — маршрут к task body, не доказательство полного task/scenario audit. Сохрани S07.04A/B/C и Builder utility, S08 design→HTML/CSS/images→browser на нескольких разрешениях→judge/user-tier cost→БД/история→урок→публикация→следующий ответ, S09 private semantic index/editor journeys и S10 whole-program outcome. Нужный primary body читай по вопросу сразу, не после общего обхода всех адресов. Не превращай отложенную поставку в удалённое требование.

## 4. Skills, контекст и RAG routes

Прочитай относящиеся sections K/docs/skills: `planning-session-startup-SKILL.md`, `architect-behavior-classifier-SKILL.md`, `trainable-architecture-decision-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `experiment-design-SKILL.md`, `improvement-measurement-SKILL.md`. Запиши actual path/ref/span и применённый смысл; ссылка не означает loaded. Dependencies — адресно, не вся библиотека. Исторические 30/50-pass/product-test routines не меняют текущие 25, parallel и documentary scope.

| Источник | Вопрос и граница чтения |
|---|---|
| Свои results/R06; START-NEXT-RUN; PARALLEL-R01-R03-JOIN; SOURCE-ROUTES | Собственный остаток и права; START уже направляет R06 в этот самостоятельный packet |
| R03 CP08, R04 representation findings, R05 CP2; их source rows | Какие уникальные решения уже получены; принять смысл в конкретный вывод, не присвоить чужое исследование |
| D/original task.md; D/Алгоритм создания ДПО.md; WorkLeft DESIGN-ACTIONS-01/TASK-PROMPTS-P0-P1/P2-P3/P4-P6-PT | Четыре роли, many→many, async, higher-order, S07–S10 и ранние seams; только именованные sections, остальное с owner |
| D/09.09.26WorkLeft-RAG-MASTER-INDEX.md → RAG-04 MD/NDJSON → V/RAG-06-llm-dynamic-flow.md → F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md и F/DECISIONS-LOCKED.md | Domain pool, memory/arbiter, helper scope и исторические замещения; дойти до primary body, не остановиться на индексе |
| K actual tree: 06.VectorSearch, 08.GraphRag, 15.OrchestratorModel, 17.DynamicDomains, 26.TrainableRouter, 00.Core/LogicalContracts.cs, DynamicGraph/edge-authority contracts и callers | Реальные inputs, state, owner, score/k/index, path authority и consumers; directory name не доказательство реализации |
| K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs | Saved BPE + example memory + cosine HNSW + copy-first output; не выдумывать голосование k соседей |
| P-root extensions reasearch/PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md → current K/43.SmartSkillsDomain | Исторический helper-ban, learned skill authority, mandatory skill outside top-k, unknown tasks |
| LEARNING-ACCEPTANCE-DELTA §§2–5/8–9; свой ROUND-DELTA; относящиеся HYBRID/RECOVERED | Уникальные ограничения и способы измерения; не повторно загружать все книги |

Primary/code claim имеет repo/ref/path/blob/section/span. Различай source-read, code-traced, proposed, observed. Большой WorkLeft делится по вопросу и consumer; передача владельцу не считается прочитанным источником. Недоступный конкретный root не заменяй публичным описанием kNN.

## 5. Часть A — фактический алгоритм и сохранность сигнала

### A1. От входа до domain-conditioned retrieval

**Prompt:** «На одном существующем normal request проследи detection(subject/intent/stack/project) → raw cues до normalization → domain-specific representation/version → eligible pool → features → score/metric → k/radius/query policy → exact/ANN → positive/negative/warning memory → final arbiter. Для consequential code claim дай class/method/короткий call и source locator. Где цепь не подтверждена, поставь NOT_TRACED, не дорисовывай связь по названию».

Для существенно разных механизмов составь карточку: `input meaning; lawful output; deterministic operation; learned decision; mutable state/owner; signal/objective; producer/consumer; baseline; intervention; falsifier; runtime/maintenance cost`. DomainHint boost не равен eligibility filter, confidence literal не равен calibrated probability. Raw/canonical/TeachingVector/saved-kNN encoding — разные каналы; потеря одного не доказывает потерю всех.

**R05 CP2 receipt в этой части:** покажи, какой канал реально видит выбранный optimizer/selector. Если смысл chosen/rejected исчезает в Compact/Encode, это input-fidelity проблема, а не доказанный провал kNN, LoRA или законов предмета. Не оценивай качество фактически изменённого материала старым CODEX verdict по исходным bytes. Token budget ограничивай явным admission/разбиением на новые самостоятельно оценённые задачи, не молчаливым усечением.

**Входы/skills:** свои F01–F07/F11/F12; R04 channels; R05 F10–F12; targeted core bodies; code examination + architecture + prompt engineering. **Выход:** один trace, feature/coordinate/pool matrix и точный непрослеженный hop.

**Хорошо:** близкий SQL кандидат недопустим по данному dialect/schema; отдельно локализована ошибка pool или metric. **Плохо:** similarity 0.9 объявлена правильным domain; разные raw hashes объявлены доказательством разных training tensors. **Edges:** mixed/unknown domain, empty/tiny pool, tied scores, stale index, colliding encodings, contradictory memories, missing mandatory skill, одинаковые числа с разными единицами, значимое отрицание после 480 символов, значимый whitespace/order.

**Документная приёмка:** вывод поддержан фактическим consumer либо честно ограничен. **Будущая examination:** reference versus predicted domain, различимость law-sensitive near misses и lawful downstream output, не shape/coverage. Рецепты E1/E2; R04 получает coordinate implications, R08 — признаки/законы, R18 — уже локализованный token seam без повторного исследования R05.

### A2. От retrieval к отношениям и обновляемому получателю

**Prompt:** «Продолжи запрос: Unit typed outputs/associations → Flow correction → async combinations → higher-order authority → ordinary result. Не требуй линейного прохождения всех четырёх ролей для каждого input. Для выбранного учебного сигнала проследи отдельно lesson recipient → actual policy/state group → objective → update → published state → обычный consumer. Сопоставь смысл и типы, а не только одинаковые IDs».

Раздели candidate generator, selector, winning solver, training capability и semantic recipient. Переданный PolicyModel не становится state kNN или edge-authority из-за общего слова learning. `KnnLogicalUnit.Train` не является автоматически target существующего LlmModel DPO optimizer. Если bridge отсутствует, назови один необходимый adapter/loader/semantic contract как **предложение**, не создавай второй training stack. При первом CodeDocumentation уроке не требуй неиспользуемый graph/L3 path; при опыте graph policy не подставляй обучение document solver вместо selector.

Сохрани learned reasoning внутри одной L5 machine, не hollow adapter или вторую независимую машину. Для relations покажи source/admission/reuse/pruning и предел ресурсов. Graph size не gain. Не сортируй semantic OrderedPatternIds ради presentation. Не раздавай final reward всем edges без causal credit основания. Укажи связь с R07 и будущим решением R12, не выбирай глобальную credit objective здесь без evidence.

**Входы/skills:** original roles; свои F06/F08–F10; R05 F05/F09–F13; actual runtime/resolver/pipeline/L3 calls; architecture + code/plan review. **Выход:** producer/port/meaning/version/consumer/authority/owner matrix, один ordinary result trace и ограниченный H07. Раздели уже связанные вызовы, неактивированный lifecycle, несовместимый signal и непрослеженный ordinary host.

**Хорошо:** Unit outputs допустимы, Flow перепутал input associations — исправляется именно relation; опубликованный child реально выбран обычным caller. **Плохо:** подключить любые LoRA weights к memory-only selector; evaluator trial reload выдать за обычное приложение; первую закончившуюся ветвь назвать лучшей. **Edges:** cycle, lawful alternative paths, partial/late branch, missing dependency, budget exhaustion, mixed versions, unseen composition, неправильный recipient при верной паре.

**Документная приёмка:** четыре роли сохранены, API не выдуманы, state и смысл получателя совпадают либо exact gap передан. **Будущая examination:** frozen Units/changed relations и при необходимости обратное; fixed primitives/new higher-order combination; полная цена всех branches. Старый H07 про два host links — только graph-specific subset, не весь путь до первого обучения.

После A опубликуй содержательный checkpoint и продолжи B в рамках назначенного R06. Повторно читать уже подтверждённые A части при возобновлении не нужно.

## 6. Часть B — конкретная гипотеза и измеримый эффект

### B1. Предметная функция, потребитель соседей и обучаемость

**Prompt:** «Для одной source-supported weak family зафиксируй существующий baseline и ОДИН domain-aware challenger. Для остальных предметов перечисли нужное отличие законов/признаков, не строй все реализации. Опиши domain/intent/stack, coordinate version, lawful pool, features, score/metric, k/radius, aggregation/reranking, memory policy, arbiter, learned owner и maintenance cost. Назови, что именно учится, от какого signal и какой consumer меняет результат».

Фактический kNN baseline: saved BPE/примерная память → cosine HNSW → `bestExample.Output`; прочие hits добавляют evidence IDs. Сначала установи семантику дополнительного соседа. Выбери один вариант: evidence-aware reranker, compatible candidates для существующего selector или предметно допустимая агрегация. Не внедряй три одновременно. Изменение k может изменить ANN первый hit; это отдельный retrieval effect, не доказательство learned aggregation.

Как исследовательскую гипотезу допустимо задать `s_d(q,z)=sum_j w_dj(q)*phi_dj(q,z)` и ограниченную `k_d(q,state,budget)`/radius/abstention policy. Укажи actual либо proposed parameter group, update owner и обучающий сигнал. Query-dependent score может не быть metric и не обслуживаться старым ANN. Для малого frozen pool используй exact reference; существующий BruteForceIndex поддерживает cosine, не произвольную новую функцию. Альтернатива — fixed-metric proposals + один reranker с отдельной оценкой потерянных кандидатов.

Если challenger учится через DPO, нужен реальный conditional distribution и frozen reference над совместимыми typed alternatives/контекстом, а не scalar similarity или judge confidence вместо log-probability. При отсутствии такого owner сначала зафиксируй semantic bridge для R12/R18 либо честно назови иной objective. Не выдавай memory training за neural DPO и не требуй заменять полезную память нейросетью.

Для law features используй primary domain evidence и область применимости: перевод — смысл/отрицание/направление; SQL — заданный dialect/schema/NULL context; layout — hierarchy/geometry/viewport; orchestration — prerequisites/typed ports/budget. C#/Angular docs привязаны к фактическим symbols/version. R08/R21 владеют полными паспортами семи subjects/extras; R06 передаёт конкретные feature/near-miss требования. LoRA adaptation не tokenizer/codebook: назови вход/выход/семантику adapter и consumer; обновление координат учитывает reindex/cache/rollback.

**Входы/skills:** A1/A2, свои E2/E4, primary ARBITER/DECISIONS, actual KnnLogicalUnit; architecture + experiment design. **Выход:** одна заполненная algorithm card с baseline, challenger, frozen/changed components, signal/owner, near misses, budget и отвергнутой альтернативой. Лучший k/metric/rank не назначается до данных.

**Хорошо:** одинаковый pool и encoder; меняется один score/consumer; несовместимый закон не проходит за высокую близость. **Плохо:** новый tokenizer+больше памяти+больше compute объявлены эффектом dynamic k; оба правильных решения принудительно размечены по происхождению. **Edges:** sparse pool, no reliable relevance labels, density shift, duplicates, changed projection, cheap path skips prerequisite, low-confidence warning без фактического abstention.

**Приёмка:** гипотеза имеет falsifier и реального/явно предложенного владельца. **Будущая examination:** сначала metric при fixed k или k при fixed metric, с matched downstream consumer; только выбранный contrast. Выбор и implementation owner — R12, recipe — R13/R22. Не новый обязательный run.

### B2. Первый урок и examination комбинации

**Prompt:** «Собери один ранний learning-examination packet на ближайшем существующем маршруте. Прими R05 CP2 token/objective delta; выбери first missing seam по цепи, не по модному названию алгоритма. Подготовь наблюдения до/после одной новой пары и matched no-update control. Остальные algorithm/interaction contrasts передай R22 с владельцем/входами/falsifier; не запускай Cartesian sweep и не делай все опыты prerequisite первого урока».

| Вопрос | Различающий контроль | Недопустимый вывод |
|---|---|---|
| Domain mapping | Predicted versus independently established domain; solver fixed | Маленький разрыв двух плохих solver объявлен хорошим router |
| Representation | Один transform при fixed consumer/pool; различимость предметного смысла | Same shape/cosine или raw passport доказывают сохранность training meaning |
| Neighborhood | Metric или k/consumer по отдельности; exact и ANN разделены | Recall/добавленные данные названы parameter learning |
| Hybrid retrieval | BM25-only/kNN-only/один fusion при сопоставимом universe/budget | Local/global GraphRag автоматически назван BM25+kNN |
| Memory versus parameters | Memory fixed/update versus no-update; при необходимости weights fixed/memory change | Контроль получил меньше знаний или прошлый ответ из exam попал в память |
| Flow/Orchestrator/Graph | Frozen Units/changed relations; same primitives/unseen combinations; все branches учтены | ID lookup, число edges или latency победителя доказывают перенос/экономию |
| DPO/LoRA | Exact reviewed pair → meaning-preserving encoding → declared response objective → correct recipient → ordinary output | Gradient/hash/training accuracy либо форма LoRA доказывают Student utility |
| Full hybrid | Source-faithful baseline и один challenger; один обоснованный interaction contrast | Больше ресурсов скрыто за меньшим adapter file |

**CODEX с моделью astra 6 ultracode выполняет человеческий педагогический выбор разрешённого урока. Внешняя открытая китайская модель создаёт ОБЕ окончательные стороны chosen/rejected. CODEX с моделью astra 6 ultracode отдельно оценивает exact pair и exact permitted context.** Project alias `GPT-6-Astra`/`ultra` не является доказательством фактического вызова. Записывай requested и actual invocation отдельно. Автор пары не назначает произвольно training recipient; он получает его из задания и возвращает подтверждение. Судья не переписывает стороны под чужим provenance. TIE/BOTH_BAD/UNJUDGEABLE допустимы; две правильные стороны можно ранжировать только по заранее объявленной существенной utility, а не forced labels.

Pair semantic validity, permissions/admission, optimizer entry, actual update, accepted child и ordinary consumer use — разные наблюдения. Контрольный контрпример для fidelity: стороны различаются только существенным условием за первыми 480 нормализованными символами; исходный verdict нельзя автоматически переносить на совпавшие tensors. Это иллюстрация для разбора, не сгенерированная китайской моделью train-ready пара. У score/objective явно определить response boundary/EOS/length policy и forward/backward соответствие; публичный average inference scorer не менять молча вместе с training objective. Не создавать ещё один trainer.

One-pair/no-repeat/full-state accept-or-rollback сохраняются. Bulk — окно последовательных новых уроков и наблюдений; не повтор пары ради улучшения score. Пользовательскую поправку сначала наблюдать на текущем state, проверить смысл/источник/полномочия, затем допускать как новый урок. Не считать feedback автоматически истинным. Не выполнять второй update при неопределённом результате первого. Откат weights не возвращает потраченные деньги или раскрытые данные.

**Входы:** одна weak family, immutable parent, actual machine/recipient/objective, разрешённая exact pair/assessment route, hardware/config evidence, compatible encoder/index versions, frozen rubric, independent source-family split, matched no-update state и budget. Неизвестные values передать named owner; не выдумывать dataset, model call, пороги или запуск. Design-ready отличается от real-run-admitted.

**Выход:** заполненный first-recipe с producer→optimizer→ordinary consumer, retained-domain panel, наблюдениями до/после, parent/child/pair/representation identities, ценой попыток и допустимыми исходами. R13 заранее задаёт meaningful-effect threshold, precision/family sample rationale и stopping rule. Нет обязательного улучшения каждой пары. Negative result не исправляется сменой rubric.

Измерять per-domain mapping, lawful success/accuracy, abstention и его coverage, path quality/regret относительно bounded comparison set, latency p50/p95, update/evaluation time, all-branches cost, retention, cold/warm load и полный PC footprint: base/runtime/encoder/adapters/reference/optimizer/index/graph/cache/snapshots/buffers плюс reindex и teacher/reviewer cost. Не исключать неудачные/медленные попытки. При меньшем количестве выданных ответов accuracy без abstention coverage недостаточна. Более простой bulk не доказывает обучение: shared operational panel/rubric и separate unseen families, untouched final panel отдельно. Учитель не видит закрытые exam answers или их перефразы.

**Хорошо:** hybrid сохраняет допустимое качество и прежние навыки при меньшей общей цене; объяснено, что менялось. **Плохо:** training accuracy, changed checksum, размер LoRA или confidence судьи объявлены gain; неопределённый результат принудительно PASS. **Edges:** null gain, noisy feedback, identical encodings, changed evaluator/rubric, small sample, domain drift, critical rare relation lost by compression, graph growth beyond PC budget.

**Документная приёмка сейчас:** source→ожидание→механизм/gap→контрпример→метрика/контроль→owner; empirical статус **NOT_MEASURED**. **Будущий вердикт:** IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE с uncertainty и per-domain regressions. Наличие recipe не считается его исполнением или независимой Astra-оценкой.

## 7. Передача, нагрузка и публикация

R04 получает law/features/coordinate-index и output semantics; R05 — зависимый signal/recipient вопрос, не повторный аудит его trainer; R07 — конкретный semantic/ordinary consumer seam, а L2/L3 только для выбранного graph path; R08 — source-grounded law/near-miss families; R12 — один owner/objective/metric/authority choice и helper-scope reconciliation; R13 — ближайший достаточный recipe; R18 — уже локализованный token/objective implementation packet; R22 — остальные необязательные contrasts; R23 — PC maintenance/resource risks.

Сохрани namespace origin=(R06,local_id,checkpoint) + primary repo/ref/path/blob/span + meaning/status + recipient action + limits. Sibling receipt должен изменить конкретное поручение или явно остаться непринятым с причиной. Не подписывай R03/R05 DONE. J03 внутри R10 объединяет шесть отдельных snapshots, не ждёт фиктивного общего завершения и не создаёт новый 26-й run.

A1/A2/B1/B2 — bounded части R06, не четыре новые сессии. Возобновление выполняет только доказанный остаток. Реально делегируемая часть получает этот контекст/права, свои exact sources/skills, полный prompt, output, good/bad/edge, documentary и future empirical criteria, return-only полномочия. Parent интегрирует; фактический дополнительный model call учитывается честно. Авторский walkthrough не independent Astra Ultra review; R15/R25 сохраняют отдельную реальную экспертизу.

Перед записью перечитай fresh bytes/blob только своих изменяемых файлов; при конфликте reconcile, не overwrite. Сохрани старые evidence/IDs; исправленный действующий вывод отдели от истории. После commit выполни remote readback и сообщи реальные commit/blob. No force-push, sibling/shared overwrites, новые пустые carriers или выдуманная публикация. Не добавляй новый общий review barrier вместо ближайшего предметного действия.
