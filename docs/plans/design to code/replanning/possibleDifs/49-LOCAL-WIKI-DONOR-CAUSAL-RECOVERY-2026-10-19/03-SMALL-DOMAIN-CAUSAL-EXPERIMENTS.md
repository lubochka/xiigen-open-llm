# 49/03. Маленькие причинные эксперименты: сначала вернуть донор, затем проверить реальное качество

**19.10.2026 · Протоколы, НЕ результаты нативных запусков.** Нельзя называть source-only разницу победой на реальном обучении. Рабочее ограничение: никакого большого Windows/GitHub CI ради semantic-модели, никакой циклической подгонки тестовых ожиданий к ответам. Все будущие опыты — изолированные локальные scoped runs, лишь требуемая компиляция изменённого куска и обязательные охранные проверки.

Для каждого будущего опыта: входы и разрешения pinned (SHA), раздельные семейства TRAIN/DEV/EXAM, fixed seed/инициализация/токенизатор/число предъявлений/optimizer/reference/CPU budget, не подбирать модель на EXAM; сохранять реальные предсказания, ошибки, warnings, latency и identity checkpoints. Если меняется более одного фактора, опыт не является причинным.

## EXP49-CT-PAIR — P0: вернуть source-content в CodeTrace DPO (C# / документация)

**Источник:** late donor [CodeTraceBuilder batch064](01-DONOR-RECEIPTS-AND-PAIR-CARDINALITY.md), 100 разных троек, архивная DPO heldout=.92. **Функция:** CodeTraceBuilderTrainingPipeline.ToDpoMetricPair. **Получатель:** именно ProjectWikiDpoMetricRunner policy, не самостоятельный regex CodeTraceBuilderUnit и не RP01 Student.

**Гипотеза:** DPO на фактической C# трассе позволяет выбирать правильный caller/callee/value flow лучше, чем DPO на одном RejectedType.

Постановка:
1. Возьми 12–20 небольших C# методов **из разных исходных файлов/семейств**, отдельно 5 разных rejectedType. На каждый метод два контраста: отсутствующий callee vs правильный; wrong caller; ложная edge; потерянный аргумент/return; неполный terminal path. Gold задаётся чтением исходного C# / независимой ручной либо компиляторной проверкой **до** model run.
2. До сравнения извлеки подготовленные model-visible тройки старой и нынешней функции. Для одинакового RejectedType, но разных source, подсчитай количество уникальных prompt/chosen/rejected и collisions. Это дешёвый **unit/source preflight**. При 100 реально различных архивных парах текущая функция, принимающая только RejectedType, логически может различить максимум число типов ошибки, а не 100 input cases.
3. **A контроль:** неизменённая нынешняя type-only подготовка. **B воздействие:** донорский raw `pair.A/Chosen/Rejected` в тот же DPO scorer при неизменных модели, tokenizer и 20–50 обучающих шагах. Сохранить positive/negative из авторизованных данных, не использовать закрытые экзаменационные строки для обучения. В обоих плечах одинаковые defensive legal admission checks.
4. **EXAM:** heldout 12 C# случаев из новых файлов/семейств с неоднозначным источником; сравнить правильность предпочтения и, **отдельно**, output настоящего trace consumer. Требовать действительные `callee`, `caller`, `argument`, `terminal return`, reject invented edges. Для DPO scorer, который не производит трассу, quality result пометить SCORER_ONLY, пока нет consumer.
5. **Negative controls:** поменять labels местами; источник заменить другим кодом с тем же error type; withheld source→abstain; пустой/обрезанный source; запустить без optimizer updates.

**Причинное подтверждение:** фиксированный ранее неверный контраст теперь исправляется именно у B при том же source family EXAM и правильном обычном ответе, нет аналогичного выигрыша shuffled-label. **Опровержение:** raw input одинаков по фактически видимым токенам; одинаковые score/answers; только DPO margin улучшается, но обычная трасса нет; или raw ухудшает качество. Не переписывать результаты на PASS.

## EXP49-ROUTE-IDENTITY — P0: маршрутизация должна видеть prompt и target (оркестрация)

**Источник:** ProjectWikiRequestRoutingTrainingPipeline.ToDpoMetricPair, который в current использует CODE_TRACE_DPO и rejected type, а old сохранял executor prompt и true chosen/rejected. **Получатель:** DPO metric scorer router. Отдельный lexical trainable model оценивается как независимый baseline.

- TRAIN: 12 коротких prompt/route задач (по 2–3 на вид), DEV 6, EXAM 12 **неповторяющихся намерений/перефразировок**. Все rejectedType частично совпадают между противоположными route destinations.
- Контрпример: A «Построй документацию текущего проекта» → DocumentationBuild, B «Обучи модуль маршрутизации документации» → ProjectWikiRequestRouterTraining. Если выбран один тип «wrong_training_unit», исходная нынешняя проекция становится тождественной и не сообщает, какой маршрут нужен.
- **Плечо A:** current template marker. **Плечо B:** true pair.Prompt/Chosen/Rejected. **Контроль C:** старый trained lexical model на тех же входах. **Контроль D:** shuffled route labels для выявления marker leakage.
- Метрика: exact target Unit ID и правильный requiredInputs/status на новых prompt; confusion matrix по intent, примеры неправильных transitions; отдельно neural preference score, отдельно статический fallback. Никакого synthetic high-confidence вместо true gate.
- **Подтверждение:** B исправляет конкретную ошибку route выбора при равном RejectedType, C показывает, был ли этот навык у донора; shuffled labels его ломают. **Неудача:** на потребительском пути осталась статическая Route: значит пока SCORER_ONLY, а не PRODUCT_RECOVERED.

## EXP49-QP-CONDITIONAL — P0: не принять 100% на постоянных ответах за обученный план (Wiki/Design-to-Code)

**Источник:** QueryPlanning batch134, 50+50 rows, 8+8 разных prompts, 1 chosen/1 rejected на все 100. **Получатель:** query-planning DPO metric learner, затем реальный QueryPlanningFlow если подтвержден checkpoint-consumer contract.

- Сделать 12 пар **контрфактических вопросов**, в каждой меняется одно требование: Angular→server chain vs server-only; read-only vs write with persistence; missing evidence vs present; wrong project/version; DTO mapping present vs absent; C# vs SQL stored procedure. Expected plan отличается конкретной ступенью, owner или abstention policy.
- **A baseline:** donor constant-label choice. **B intervention:** 12–24 разных содержательных chosen/rejected с указанными от автора правильными маршрутами и критичными различиями; исключить side labels вида GOOD/WRONG из семантического gold, но если маркеры требуются настоящим contract — сбалансировать их в обоих плечах.
- **EXAM:** 12 неиспользованных новых project/family questions, в том числе 3 missing-evidence, заранее записанное «правильное следующее действие». Meter: plan-step F1/recall, order consistency, hallucinated steps, abstentions, source evidence; DPO margin дополнительный, не primary.
- **Negative:** один и тот же chosen для всех случаев, переставленный prompt, shuffled labels, fixed no-training template. **Причинный критерий:** B действительно меняет разные планы согласно вопросу и превосходит template на независимом EXAM без роста unsafe accepted answers. **Ноль эффекта:** если endpoint продолжает deterministic output либо пары/tokenizer сливаются.

## EXP49-GATE-ADOPTION — P0: обученное состояние доходит до продукта законным образом

**Источник:** ProjectWikiRequestRoutingTrainingRunner.semanticQualityClaimed=false, current TrainableModel.Route/RouteForEvaluation и WikiTrainingOrchestrator._router.Route.

- Вызвать отдельно **Train() → RouteForEvaluation(prompt)** и **Route(prompt)** с тем же snapshot, включая замкнутое состояние после save + fresh process. Зафиксировать refused status и причины.
- Затем выполнить ordinary ProjectWikiTrainingOrchestrator.RunAsync, а не предполагать, что он вызывает trainable Route. Записать имя resolved Unit, если известен, and whether read checkpoint, плюс semantic correctness.
- **Плечо A:** native approved gate, которое сейчас может отказывать. **Плечо B:** только полноценный независимый semantic review + подтверждённая связка authority в публикационный контракт; запрет установить semanticQualityClaimed=true только ради эксперимента. При недоступной санкционированной B ветке вернуть BLOCKED_BY_AUTHORITY — это ожидаемая безопасная граница.
- Метрика: доля правильных обычных маршрутов среди 12 новых cases, количество корректных безопасных отказов, реальное состояние после restore/route, latency. **Отдельный контроль:** никакое обращение к trained route не должно случайно менять irreversible effects.

## EXP49-CROSS-RECIPIENT — P1: проверить перенос в RP01, не смешивая учеников

После доказанного P0-CT эксперимента: один и тот же набор правил о трассе C# документирования, 16 train / 8 dev / 12 EXAM по группам. Четыре **обозначенных** варианта: D=старый обученный donor Unit, U=нынешний специализированный Unit, S=RP01 q/v-LoRA Student, SU=Student+включённый проверенный Unit. Никакой скрытой «подмены» ответа извлечением allowed labels. Измерять три результата: DPO preferred margin, содержательный code/documentation answer, обычный user-facing consumer после cold restore. Для D/U SFT/LoRA update count неприменим — честно фиксировать различие метода/ресурса, сравнивать качество с учётом техники, а не выдуманной одинаковой арифметики бюджета. Второй эксперимент **внутри** S по warm-head/SFT→DPO только после отдельного genuine failure-to-overfit и при отдельной ресурсной бухгалтерии.

### Темы, для которых другие признаки могут быть полезны (но это НЕ один эксперимент сразу)

| Предметная тема | Самая короткая следующая проверка | Отрицательный контроль |
|---|---|---|
| **Перевод RU→EN и EN→RU** | 12 неоднозначных фраз: same vocabulary, left/right POS context, отрицание, изменение порядка слов; baseline wordmap vs POS-aware kNN | Тот же смысл без контекста либо переставленные context labels; не объявлять en→ru тем же самым результатом |
| **Программирование C#** | 12 методов: AST sibling structure, ordered statements, lambda/uninvoked guard и разные caller families; сравнить kNN без/с одним AST feature | Не относящийся к семантике узел либо перестановка комментариев |
| **SQL / .NET documentation** | QueryPlan 12 схем: сохранение через transaction, порядок call→persist→return, evidence coverage | SQL query с другим owner/таблицей, rejected без придуманного persistence |
| **Оркестрация** | 12 двухшаговых намерений: typed outputs many→many, fallback/abstention, index replay | Explicit wrong unit, reversed dependency order, stale index |
| **Непрерывное обучение памяти** | E43-style 12 old + 12 new: одно изменение replay fraction; cold restore | Shuffled replay labels, no-update, previous checkpoint; отдельно большие домены |

Такие кNN/AST/POS/morph признаки — **условно предметные**. Предметный успех на морфологии RU не делает её полезной для C# RPC. Не переносить старый matrix learner в kNN, пока source lineage не проверена; в прежних 47/48 и Core уже есть контрпримеры ошибочной интерпретации matrix result.

## Шаблон результата будущего исполнителя (обязателен)

~~~text
experiment_id; source_commit; donor_artifact_sha; subject/domain; profile;
training_recipient; changed_one_factor; actual_consumer;
train/dev/exam case IDs and source-family split;
A/B actual train steps and CPU/wall/RAM/GPU; tokenizer/LoRA/reference IDs;
A/B generated answers; preference score; domain correctness;
negative controls; expected harms; cold restore parity;
conclusion = POSITIVE / NO_GAIN / NEGATIVE / INCONCLUSIVE / REFUSED;
proof/source paths; review_status; ordinary_user_quality_claim_allowed.
~~~

Не менять EXAM после просмотра результата, не учитывать дважды повторённый prompt. Если научная метрика выросла, но teacher/unit consumer не вызывался — **NEEDS_CONSUMER**, не успешное восстановление всего алгоритма.
