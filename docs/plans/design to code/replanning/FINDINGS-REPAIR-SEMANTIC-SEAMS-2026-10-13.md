# Исправление findings: смысловые связи, обучаемое состояние и выход к пользователю

**13.10.2026. Документные решения и новые адресные чтения кода; не выполненные R01–R25 и не результаты обучения.**

Мы хотим научить существующую систему лучше решать задачу, а не только менять файл весов. Для этого нужно сохранить смысл урока, изменить именно тот механизм, который должен учиться, и получить результат через обычный пользовательский путь. Исправление схемы данных, наблюдаемый optimizer step и полезное обучение — три разных результата.

Этот документ дополняет, а не заменяет `COMPLETION-REPAIR-ALGORITHMS-2026-10-13.md`, семь заполненных паспортов в `COMPLETION-REPAIR-DOMAINS-2026-10-13.md` и два DAG в `COMPLETION-REPAIR-INTEGRATION-2026-10-13.md`. Повторно создавать эти материалы не требуется. Dispatcher передаёт исполнителю только назначенный ниже раздел с нужным primary/code excerpt. Это не ещё одна книга, обязательная всем целиком, и не R26.

## 1. Новое evidence и точные пределы

P0 = `lubochka/xiigen-planning@36230b000d8237743038d5764178e48bcd78deb1`.
K0 = `lubochka/llm_mvp_core@3c7f5f8c7809936e25579603c96ea6df8598fc40`.
Последующие shared-file reads для R17/START относятся к их фактическим blob, а не задним числом к P0. Чужие results не переписаны.

| ID | Прямо прочитанный источник / ограничение | Вывод, который разрешён |
|---|---|---|
| FS-01 | K0 `21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs`, весь файл, blob `a61871dc6f1f8f6c579eecccc9fa82383342d8df` | `BuildSegmentDocumentsAsync` соединяет `compiled.Segments.Zip(attributedResult.Value!.Output, ...)`; learned router может выбираться через registry. В этом месте сопоставление позиционное. Равенство длины/семантических IDs тут не установлено. Фактическая частота неверных ассоциаций не измерена. |
| FS-02 | K0 `21.AgentInfrastructure/Logical/LogicalUnitDynamicGraphNode.cs` 1–240, blob `781ba5afe61bd3dfa34049133fd4f334a710daa0` | `ExecuteAsync` вызывает typed Unit, выполняет projection, один раз материализует candidates и возвращает `candidates.AsQueryable()`. Это реальный inference bridge, но не доказательство разрешения OutputRefs в typed payload и не lazy remote query engine. Остаток класса не объявлен прочитанным. |
| FS-03 | K0 `21.AgentInfrastructure/Logical/DynamicGraphNodeOutputSourceContracts.cs`, весь файл, blob `1526e4220da863db24b6f5a62ca023536bd9d853` | Snapshot копирует candidate metadata и `OutputRefs`. Это сохраняет carriers, но сам этот класс не является resolver содержимого. Отсутствие resolver во всём repo не заявляется. |
| FS-04 | K0 `10.LoRA/LoraApplicator.cs` 1–165, 400–740, 746–835, 900–EOF, blob `5eeaf38c5fc12fa2f88dc6c247a3fb2fc250716d` | Canonical target vocabulary: q_proj/k_proj/v_proj/o_proj; фактически выбирается подмножество из config. `ApplyCore` выключает `RequiresGrad` у `lease.BaseParameters`, затем вызывает `EnableAdapterGradients`; fresh seeded pair/triple отдельно выполняют freeze/install/enable. Непрочитанные spans и prepared-continuation caller остаются отдельным ограничением. |
| FS-05 | K0 `18.CapabilityTraining/LlmIngredientLogicalUnits.cs` 1120–1360, 1480–1715, 1780–2110, blob `cb8744fcbbcff99821852e789dc92163976c0ad7` | Видны fresh/apply и parent-loading пути, snapshot параметров, `PrepareExecution`, `CreateDpoTrainer`, отдельный `CreateForPreparedContinuation`, проверка independent policy/reference storage и ключей. Это не фактический перечень trainable keys конкретного materialized pilot. |
| FS-06 | K0 `11.DPO/DpoTrainer.cs` 1–333, 478–640, blob `9c358978075db8c22fe0125eaea9e89e831d5635` | `StepVector` вызывает `_optimizer.Step(_policy.Parameters().Where(parameter => parameter.Value.RequiresGrad), lr: _learningRate)`. Конструкторы в прочитанном диапазоне не включают все policy gradients заново. Export связывает portable optimizer state с checkpoint keys; из этого не следует наличие optimizer buffers для каждого frozen parameter. |
| FS-07 | K0 `25.ModelServer/Program.cs` 1–200, отдельно 80–91, blob `47ae015f8b5402364128f4f75f1c92e45f3d3811` | Owner-pinned bootstrap использует `RepositoryOwnedTrainingTrustedRepositoryRoot = @"C:\Projects\llm_mvp_core"`. Ненативная DPO-математика не доказывает Linux-ready bootstrap; local host и GitHub Windows campaign — разные вопросы. |
| FS-08 | P0 `llm mvp/visual studio & visual studio code continue/RAG-06-llm-dynamic-flow.md` 1–100 → P0 `llm mvp/llm dynamic flow/DECISIONS-LOCKED.md`, весь primary | Действительно выполнен переход RAG→первичный смысл D-01…D-15. Canonical intent labels, StackDetector order, TensorCore, BM25+kNN, arbiter authority сохранены. Исторические 2 MB, 70/30, percentages и CODE-ABSENT не становятся текущим runtime evidence или разрешением replay. Это не обход всех 12 RAG roots. |
| FS-09 | P0 `Kiro-Xiigen-Original-Slice-Queue.md`, весь документ; current `PLAN-25-RUNS.md`, относящиеся разделы | S00–S10, DPO/DynamicGraph, DTC, IDE и дополнительные product repositories остаются в программе. Queue — кандидат зависимостей, не доказательство отсутствующего кода и не разрешение всех effects. |
| FS-10 | Current `R17-EVIDENCE-POLICY-2026-10-13.md` до исправления, blob `a9b7353f03eb78447f55f6183d12efdacc8d8378`; current R09 FINDINGS §2 | R17 повторно принял агрегаты 30 runs без восстановленных исходных строк. Исправлено в действующей policy: ATTRIBUTED_AGGREGATES, без использования 3.2×/5.5× как подтверждённой экономии. Исходные carriers R09 не изменены. |

Обычный routing path и `CodeDocumentationRepositoryOwnedModelCheckpointStartService` в R07 остаются **ATTRIBUTED**: полезный trace прежнего исполнителя, но не новое полное чтение здесь. Так же исходные CR-A01/02 про Compact/480 и response mean берутся из code-traced baseline с его locators; настоящий документ не приписывает себе повторный полный аудит этих методов.

## 2. Решённый контракт ассоциаций: не позиционный Zip

**Проблема:** если input содержит два сегмента, а attributed router вернул результаты в другом порядке, одинаковая длина массивов не доказывает правильного соответствия. Если результатов меньше, позиционный Zip может скрыть неприсоединённый input. Это статически локализованный риск, а не сообщение о выполненном дефектном запуске.

**Выбранное проектное решение FS-D01:** identity исходного сегмента создаётся до маршрутизации и сохраняется через typed contracts. Выход указывает ID исходного входа, а при настоящем many→many — непустое множество соответствующих input IDs плюс собственный output ID. Соединять по этим связям, не по позиции, текстовому сходству или порядку завершения async. Требование не создаёт новый `ILearningUnit`: расширяется/используется действующий контракт.

При одном результате на сегмент: уникальные source IDs, отсутствие неизвестных IDs, полное ожидаемое множество и однозначный join. При many→many: отдельная relation table `output_id → input_ids`; fan-out/fan-in и допустимая кратность определены domain/output contract. Нельзя заставить many→many искусственно стать 1:1. Потерянный обязательный input — явный неполный результат/отказ до применения, а не успешная короткая выдача. Допустимый пустой результат определяется доменным контрактом, не универсальным исключением.

**Пример, не учебная пара:** input A — «проверь A», input B — «отправь B». Router вернул [B, A]. Correct: результат B присоединён к B, A к A. Wrong: первый output присоединён к A из-за индекса. Edge: один output относится к A+B; repeated identical text с разными IDs; cancellation только B; неизвестный ID; результат предыдущей topology revision.

**Адресная передача:** R04 закрывает contract/producer/join, R07 — связи Unit/Flow/async, R12 принимает миграцию реальных типов, R18 включает только затронутый production seam. Критерий документа — конкретные producers/consumers и решение каждого edge. Критерий будущего опыта — association fidelity на новых композициях плюс downstream качество/стоимость при фиксированных Unit, не новый coverage проект.

## 3. OutputRefs: закрыть дизайн, не объявлять найденным неизвестный resolver

**Выбранное решение FS-D02:** `GraphNodeCandidate.OutputRefs` — ссылки на результаты, не сами результаты. Для каждого output, который должен потребить downstream Unit/Flow, обязательны: invocation/scope, output ID, output schema/type/version, reference identity, источники input associations и владелец срока жизни. До actual consumption reference разрешается в payload и проверяется semantic/schema compatibility. Shape equality не compatibility.

Плановый алгоритм: `selected candidate → each required output reference → scope/version/rights check → typed payload resolution → reconstruct declared input relations → call downstream consumer → preserve output provenance`. Если существующий resolver найден, использовать его и показать путь. Если нет, R12 выбирает минимальную реализацию на действующем output source/store; не создавать второй graph/trainer. Нельзя молча подставлять первый candidate, пустой объект или hash вместо payload.

Для invocation-local output сохранить snapshot на срок всех разрешённых asynchronous consumers; завершение первого child не удаляет данные fan-in. Для persisted output явно задать version/ownership/reopen; для missing/cancelled/expired ref вернуть конкретный исход без повторного побочного эффекта. Digest candidate set подтверждает identity набора, но не содержание каждого результата.

**Пример:** Flow «verify(A)→send(A)» получает результат проверки A даже если проверка B завершилась раньше. Неверно «send(A) после любой успешной проверки». Edge: несколько outputs разных requested types, fan-in, stale revision, duplicate ref, cross-request reference, payload доступен только в памяти завершившегося процесса.

R04/R07 должны вернуть **либо найденный producer→resolver→typed consumer, либо точный предложенный patch seam с доказанной границей поиска**. Формулировки «resolver существует по названию» и «во всём repo его нет» без такого evidence недопустимы. Полный graph resolver не prerequisite CodeDocumentation E1, если его ordinary path этим graph не пользуется.

## 4. LoRA, semantic projection и правильный получатель

FS-04…06 уточняют незакрытое требование: механизм freeze/install/optimizer filtering уже существует. Не заказывать его заново. Остаток — привязать **реальный выбранный pilot** к этому пути, особенно `SeededTopologyPreparedByRepositoryStart`, а не вывести все trainable keys из одного класса LoraLinear.

**Выбранный контракт FS-D03:** перед разрешённым update получить из фактического policy экземпляра `GetCheckpointParameterEntries()` и текущие flags; для каждого ключа зафиксировать роль base/adapter/иной явно разрешённый параметр, shape, storage/dtype, RequiresGrad, membership в optimizer, checkpoint identity и источник разрешения. Сверить с реально установленными adapter slots и config.TargetModules. Reference имеет самостоятельное хранилище и не входит в update set. После reload/continue эти сведения не заменяются конфигурацией свежей модели.

Policy для E1 остаётся existing CodeDocumentation Student, не routing profile и не winning solver. Изменение одной документной модели не приписывать Flow/Orchestrator/Graph. Изменение готового route manifest само по себе не доказывает DPO над path policy.

**LoRA не определяет смысл вектора.** Для требуемой проекции отдельно задать `P_d: source representation → target representation`, coordinate meanings, допустимые outputs, objective, trainable owner и реального downstream consumer. Если target — последовательность vector-token/code IDs, задать codebook/version и conditional token likelihood. Если target — непрерывный вектор, назвать supervision/density или contrastive objective; произвольное расстояние не выдавать за log probability стандартного DPO. LoRA может параметризовать P_d, но attention q/k/v/o adapters нельзя без evidence переименовать в готовый output projector.

**Good:** actual разрешённые adapter keys участвуют в objective выбранного Student, source/target смыслы и ordinary consumer показаны. **Bad:** «A/B есть, значит база точно frozen в любом runtime и все domains уже отображаются». **Edge:** warm-start flags, пустой target set, чужой adapter, несовместимый codebook, изменение topology, сохранённый optimizer другого key set, reference/policy shared storage.

Первичные математические источники: [DPO, Rafailov et al.](https://arxiv.org/abs/2305.18290), [LoRA, Hu et al.](https://arxiv.org/abs/2106.09685). Они поясняют objective/параметризацию, но не доказывают корректность XIIGEN wiring или fit на пользовательском ПК.

## 5. Dynamic kNN и hybrid: что именно обучать и сравнивать

Сохранить предметные features/гипотезы семи заполненных паспортов. Не объявлять найденной «лучшую функцию для всех domains» без опытов.

Разделить пять независимых механизмов: (1) lawful/domain eligibility; (2) vector/token representation; (3) score/ranking; (4) выбор k/radius; (5) использование выбранных соседей downstream. Функция `s_d(q,n;θ)=Σ_j w_j(d,q;θ) φ_j(q,n)` — кандидат reranker, не автоматически metric: query-dependent weights могут нарушать симметрию/triangle inequality. Поэтому approximate candidate generator и exact reranker оцениваются отдельно; recall lawful neighbors измеряется относительно exact reference pool. Для backend с metric assumptions либо доказать необходимые свойства функции, либо оставить её post-retrieval score.

**Выбранный bounded experimental порядок FS-D04:** сначала фиксированный eligible pool и representation, сравнить existing ranking с domain-aware ranking; затем на фиксированном ranking отдельно сравнить fixed-k и query-dependent k; только затем включить настоящую multi-neighbor composition. k выбирается из заранее объявленного конечного множества в пределах budget по параметризованной политике. Её features, training signal, mutable parameters и stop condition назвать до опыта. Сходство/уверенность не становится вероятностью без соответствующей модели/калибровки.

Если consumer возвращает `bestExample.Output`, выигрыш при k>1 можно отнести к поиску/ранжированию, но не к объединению знаний нескольких соседей. Negative/warning memory исключает/понижает неподходящие случаи в согласии с arbiter authority; warning, приложенный к неверному answer, не является abstention. Законы и права не понижаются ради высокой similarity.

Для learned ranking с preference pairs назвать фактический ranking loss; не маркировать его стандартным DPO без нормированной policy, условного пространства результатов и reference likelihood. Для graph-path DPO specify допустимые actions/contexts и факторизацию log probability пути, сохраняющую OrderedPatternIds и input associations; update принадлежит policy выбора, не solver победившего пути.

| Роль | Что фиксировать / что менять | Различающий опыт |
|---|---|---|
| Unit | Один domain/output contract; менять Unit representation или разрешённые параметры по отдельности | Новые task families, качество many→many output и его связи с input, full cost |
| Flow | Заморозить Units; менять разрешённую relation/correction policy | Переставленные входы, новый fan-in, разные lawful intermediate results; выигрыша Unit недостаточно |
| Orchestrator | Заморозить solver/candidate pool; менять policy lawful async combination | Порядок зависимостей, cancellations, качество и стоимость **всех** ветвей, regret к лучшей рассмотренной допустимой альтернативе |
| DynamicGraph | Заморозить lower-role behavior; менять higher-order relations/selector | Новые сочетания знакомых primitives с независимыми families; memorized whole path не transfer |
| Builder | Сначала качество/стоимость создаваемых пар, затем отдельный downstream Student effect | Preference correctness ≠ pair usability ≠ пользу получил Student; judge-pass rate не конечная цель |

## 6. DPO-пара: три разных причины малого сигнала

Сохраняются CR-D01/02: exact assessed pair без скрытого Compact/обрезки; overflow — явный отказ или новый отдельно оценённый короткий урок; response-only summed conditional objective со своим backward, без незаметного изменения public average inference scorer. Prompt остаётся conditioning context, attention mask и loss mask различны; token boundary определить по фактической сериализации, не предполагать prefix при раздельном BPE encoding.

**FS-D05, LOW_CONTRAST:** (a) одинаковые/смыслово эквивалентные final sides — TIE/непригодная preference; (b) изначально разные стороны стали одинаковыми после подготовки — INVALID_PREPARATION, не weak lesson; (c) meaningful допустимая пара, но policy=reference даёт начальный margin 0 — это не основание отклонить пару как LOW_CONTRAST. При нулевом actual gradient отдельно исследовать saturation/disconnection/masking; не придумывать новый reject по одному margin.

LOW_CONTRAST не имеет права незаметно запустить teacher повторно. Предложенный consumer — существующая lesson disposition/selection boundary: сохранить reason, exact pair identity и diagnosis; optimizer admission не выдавать для (a)/(b); новый author request возможен только в оставшемся single-use/attempt budget. Фактический handler и его ветки пока должны быть привязаны R05/R12/R18; это выбранная семантика, не утверждение найденного runtime handler.

## 7. Исполнимая схема экзамена: не подменять её coverage

**Обязательный результат R13 — не ещё одна форма:** конкретный заполненный E1 recipe плюс причинные E2/E3/E4 packets по §5. Семь паспортов — baseline. Неподтверждённый universal threshold не подставлять ради READY.

Для E1 заранее замораживаются: primary target/domain, accepted parent и независимый reference, actual tokenizer/config/parameter set, ordinary entry, разрешённый новый lesson, teaching/evaluation separation, rubric и budget. Порядок: ordinary parent baseline → допустимая новая exact pair → один разрешённый update из full snapshot → isolated candidate examination → keep/promote/recover по исходу → cold ordinary use. Не переставлять promotion перед examination.

**Rubric для C# null-примера, выбранная на уровне дизайна:** правильный результат для null и zero; правильное объяснение выполненных/пропущенных ветвей; отсутствие существенного утверждения, противоречащего source; достаточное объяснение для исходного вопроса. Ошибка закона не компенсируется стилем. Документный пример не попадает в untouched exam как «невидимый».

**Family unit:** источник/шаблон вместе со всеми paraphrases и вариантами имён. Делить на teach, operational-development и untouched confirmation по family до authoring. Для mapping отдельно true-domain/predicted-domain/unknown/mixed; oracle-domain — diagnostic arm, не скрытая подсказка production arm.

**Сравнение:** внутри bulk parent и candidate получают одинаковые допустимые inputs/context/memory и fixed scoring protocol. No-update control не получает иной prompt или меньше разрешённых ресурсов. Для task i и одинакового inference protocol сохранить paired difference Δ_i; сначала агрегировать внутри family, затем по families/domain. Повторные seeds одной family не увеличивают N независимых families. R13 выбирает конкретный family-level uncertainty estimator и фиксирует его вместе с n/числом разрешённых looks до результата. Не выбирать самый выгодный estimator постфактум.

**Между bulk:** общий versioned anchor + отдельно новые более сложные families. Сравнение средних на разных по сложности заданиях не learning curve. При многократном использовании operational panel его роль — адаптивное решение, не независимое подтверждение; final confirmation не поступает в DPO selection/teacher context.

**До запуска нужны конкретные числовые bindings:** practically meaningful effect; worst-domain/retention tolerance; uncertainty level/method; n independent families/repetitions; finite teacher/assessor/evaluator calls; memory/time budget и число уникальных уроков. Источник — действующее решение owner либо явно предложенное и принятое design decision до опыта. Отсутствие этих значений не скрывать в implementation gate: этот recipe имеет `DESIGN_BINDING_REQUIRED`, остальные независимые R13 contrasts продолжаются. Runtime IDs не выдумывать.

**Исходы:** INVALID_EVIDENCE при нарушении identity/split/объективности/mutation disposition; REGRESS при вреде; INCONCLUSIVE при недостаточной точности или эффекте; IMPROVE только при заранее объявленных effect/retention/cost условиях. Completed negative experiment — результат исследования, но не выполненное требование улучшения. У candidate с неопределённым/плохим результатом не отбирать только удачные seeds.

**Стоимость:** wall time parent/teacher/pair assessment/update/evaluation/adoption отдельно; cost всех attempts и branches; peak RAM/VRAM с base/reference/adapters/gradients/optimizer/activations/index/graph/cache/snapshots; disk growth/reindex/restart. Не складывать независимые component peaks как одновременно измеренный peak. Сравнивать Pareto quality–latency–memory и time-to-declared-quality, а не только маленький adapter. Без фактического железа и measurements PC-fit — NOT_MEASURED.

**User feedback:** сначала сохранить ответ до обучения и feedback/provenance; проверить конфликт, novelty и разрешение; построить новый допустимый lesson; затем обновить в разрешённом idle/resource window. Не включать feedback target в before-evaluation задним числом. Retention exam не даёт разрешения replay старых пар; D-06 historical 70/30 не отменяет current no-repeat.

## 8. Runtime binding: найти реальные значения, не придумать команду

| Binding | Откуда получить и что сравнить | Граница остановки |
|---|---|---|
| Parent/checkpoint/config/tokenizer | Действующий repository-owned start/materialized accepted model; exact saved artifacts/provenance + K0 parent validation path | Нет реального materialization — RUNTIME_BINDING_OPEN; не random model под старым именем |
| Trainable state | Реальные checkpoint entries/adapter slots/flags + optimizer membership, §4 | Неоднозначный mutable owner — запрет update, не запрет остальных исследований |
| Pair/teacher/assessment | Current authorized selection и immutable saved pair, actual authors/profiles/request IDs/quality decision | Примеры этого документа — не actual Chinese-authored DPO |
| Ordinary entry | Actual end-user CodeDocumentation caller → accepted version selection → tokenizer/config/model invocation → returned answer | Router receipt или trial evaluator недостаточны; новый endpoint не выдумывать |
| OutputRefs/segment linkage | Реальный producer/resolver/typed downstream, §§2–3 | Блокирует тот graph/Flow опыт, который зависит от связи, не автоматически E1 |
| Thresholds/budget | Current domain policy/owner decision + preregistered R13 recipe | Незавершённый design не маскировать execution-only gate |
| Local host | Actual supported bootstrap, protected root, permissions; FS-07 | Не ослаблять root/permit ради Linux и не запускать GitHub Windows без задания |

## 9. Адресные остатки: все 25 назначений, без перезапуска

| Run | Что принять из этого исправления | Один конкретный следующий результат, не повтор исследования |
|---|---|---|
| R01 | FS-08/09 и §8 | Закончить discovered-root→family→source-depth→owner matrix, не перестраивать существующие LF→U links; отдельно generated/archive/current roots. |
| R02 | §§2–6, FS-08 | Привязать новые semantic obligations к существующим U IDs и authority; сохранить конфликты D-06/no-repeat и roles как решённые по current contract, не возродить старые цифры. |
| R03 | FS-08 | Использовать реально пройденный RAG-06→DECISIONS как attributed receipt и пройти свой оставшийся primary frontier; directory count не CONTENT_READ. |
| R04 | FS-01…03, §§2–3 | Точная identity relation сегментов и typed OutputRefs resolver/patch seam; перечислить caller/consumer и lawful cardinalities. |
| R05 | FS-04…07, §§4/6/8 | Привязать реальный prepared pilot key set, LOW_CONTRAST disposition и ordinary model invocation; не заказывать новый freeze/optimizer/trainer. |
| R06 | §5 | Один domain-specific score/k/consumer contrast с exact/ANN boundary и negative/warning memory; не выдать k>1 за fusion. |
| R07 | §§2–6 | Полный four-role causal graph с associations/credit/state/adoption плюс собственные обязательные primary; этот документ не закрывает A–J. |
| R08 | §§5–7 | Проверить уже заполненные паспорта на соответствие consumer; добавить только отсутствующие закон/контрпример/семейство, не семь новых пустых форм. |
| R09 | FS-10 и исправленная R17 policy §1 | Восстановить исходные 30 raw rows с формулой либо оставить агрегаты unverified; не генерировать новую выборку под старой датой. |
| R10 | §10 | Partial semantic join девяти research snapshots; новые R11–R17 receipts учитываются по зависимым решениям, не превращают R10 в повтор семнадцати исследований. |
| R11 | §§2–8 | В двух существующих DAG отделить обязательные E1 bindings от неиспользуемых graph/native/remote веток; не убрать full delivery. |
| R12 | §§2–6/8 | Дать dispositions выбранным FS-D01…05 и реальным code-type migrations; unresolved binding не выдавать за архитектурный PASS. |
| R13 | §§5–8 | Заполненный causal experiment packet с actual bindings/предобъявленными decision rules; отдельно non-ready значения, без подмены full hybrid одним E1. |
| R14 | FS-09, §8 | Сохранить M0–M7/исходные IDs и capability queue, перенести зависимости адресно, а не удалить S05–S10. |
| R15 | exact candidate, §§2–8 | Независимое опровержение конкретной версии: association inversion, неверный mutable recipient, leakage, false PC claim. Без вызова reviewer — NOT_PERFORMED. |
| R16 | принятые §§2–6 | Перенести resolved clauses в actual requirements, не повторять уже принятую Chinese both-side policy. |
| R17 | исправленная evidence policy + §10 | Применённая risk matrix сохраняется; поправить только противоречащие clauses/потребителей, без повторного полного R17. |
| R18 | §§2–8 | Конкретные bounded implementation packets для exact pair/objective/recipient/adoption; graph seams лишь при действительной зависимости. |
| R19 | §10 | Единый текущий старт R01–R17 в START/COMMON/JOIN, scoped repair injection и безопасное владение shared files. |
| R20 | §§8–10 | В каждом effective prompt фактические context/skills/refs, meaningful good/bad/edge и finite child boundary; не список книг для пользователя. |
| R21 | §§5–7 | Следующий structurally different domain после working loop; actual Builder pair validity отдельно от downstream Student gain. |
| R22 | §§4–7 | Ограниченные causal contrasts и заранее выбранные взаимодействия; null result не повод full factorial или скрытого replay. |
| R23 | §§7–8 | Полный PC/feedback lifecycle с observed pre-update answer, unique lessons, restart/no-repeat и total-cost accounting. |
| R24 | FS-09, §§3/8 | DTC/IDE ordinary user journeys и active model adoption; remote/native obligations сохранены на собственных triggers. |
| R25 | exact published candidate | Независимая финальная передача и конкретный следующий packet; не объявлять документную правку выполненным обучением. |

## 10. Параллельная работа и окончательная приёмка

Последнее прямое поручение: R01–R17 могут начать одновременно. R01–R09 собирают direct evidence; R10 делает incremental join; R11–R14 анализируют sufficient snapshots; R15 рассматривает exact candidate; R16/R17 готовят и применяют достаточные source-bound deltas. Start ≠ готовый окончательный вывод ≠ право перезаписать общий файл.

Для общего файла R16/R17 действуют как один последовательный интеграционный поток: exact base blob + section-bound diff + current reread + semantic merge + remote readback. Already applied R17 decisions не передаются заново как TODO. Research workers пишут только собственные пять carriers; children return-only. Достаточные receipts принимаются по clause; неизвестный сосед не останавливает независимую часть. Изменение чужого RUN-LOG не причина общего rerun.

В effective packet входят содержанием: CURRENT TASK/NOT THE TASK; роль; фактически нужные skills; точные source/RAG/primary/code spans и их глубина; own checkpoint; current→target behavior; подробный алгоритм и causal question; allowed/forbidden paths/actions; материальные outputs; good/bad/edge; documentary acceptance отдельно от будущего experiment; finite budget/stop; return schema и consumer. Для child все эти поля заполняются конкретным содержанием родителем до вызова. R08 CP4 exception и уникальные R07/R11/R12/R13/R14 repair clauses сохраняются.

**Текущий вердикт:** решения и code-read уточнения опубликованы; применяемые плановые правки отделены от hypothetical implementation. Полное завершение R01/R03/R07/R12/R13, реальные runtime IDs/числовые thresholds, независимая Astra-экспертиза и эмпирическое улучшение не заявляются. Это не новые вопросы Лубе вместо работы: каждый конкретный остаток имеет existing owner, source resolver, output и ограниченную зависимость; готовое не исследуется повторно.
