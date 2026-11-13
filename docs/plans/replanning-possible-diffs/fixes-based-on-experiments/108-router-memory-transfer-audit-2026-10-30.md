# 108. Сохранность обученной памяти двух маршрутизаторов и приёмка публикации

Дата сверки: 30.10.2026. Фаза: READ_ONLY_RESEARCH и Markdown. Прочитаны текущие исходники, история целевого файла и сохранённые журналы. В этой работе тесты, обучение, сборки и дочерние процессы не запускались. Это дополнение к [102](102-late-transfer-repair-plan-2026-10-29.md), а не новый план повторного внедрения уже исправленных строк.

## Что остаётся открытым

1. Полный путь обычного промпта через обученную память в выбор домена — R9 плана102. Исправление владения массивами само по себе не подключает этот путь.
2. Сверка01.11: прежний отказ публикации ниже — исторический baseline. Найден `C03-PUB-REPAIR-attempt-0001`: green3/3, два фактических вызова trainer, один publisher и один reservation loser; GO_TIMEOUT теперь вызывает Assert.Fail. [Аудит110](110-transfer-state-and-new-evidence-audit-2026-11-01.md) связывает исходник и TSV. Повторный ремонт C03 не назначать; эта узкая приёмка не закрывает весь общий набор и R9.
3. Граница пустой партии различается у двух владельцев. У `RouteClassifier` она отвергается; `PredicateDatasetRouter` публикует пустую память. Это различие действующих контрактов, а не доказательство автоматически разрешённой общей правки.

## Два владельца, два комплекта доказательств

| Владелец | Фактически перенесённый инвариант | Проверенное основание | Чего это не доказывает |
|---|---|---|---|
| `22.AgentAlgorithms/Context/PredicateDatasetRouter.cs:28–55` | Копии признаков **и bool-меток**, новая таблица публикуется после полного чтения; неуспешное чтение не стирает принятую память | История файла: `a218264d666`; два отдельных комплекта R102-R3/R4 ниже | Не новый алгоритм голосования; не отказ на пустой партии; не сохранение на диск |
| `29.SemanticCompiler/Routing/RouteClassifier.cs:77–112` | Копия признаков, проверка null/пустой партии и публикация после копирования; валидный Train заменяет память | `e093aa83d02`; C03 red81/90 → green90/90 | Не подключение `Classify(string[])` к обученной таблице; не зелёная расширенная приёмка |

У первого исходная пара `(int[] Predicates, bool[] Datasets)`, у второго `(int[] Predicates, Domain Domain)`. Enum Domain не требует копирования массива меток. Общий ярлык «router» не позволяет закрыть тестами одного владельца дефект другого.

### Dataset router: R3 и R4 реализованы в своих границах

Исходник [PredicateDatasetRouter](../../../../22.AgentAlgorithms/Context/PredicateDatasetRouter.cs:28) сначала создаёт `ownedRows`, копирует оба массива и лишь затем присваивает `_train = ownedRows`. Отказ null сейчас возникает при чтении отсутствующего объекта; этот аудит не приписывает коду новый типизированный протокол ошибок. Для сохранности памяти существенен момент публикации.

[DatasetRouterMemoryOwnershipTests](../../../../Tests/ExperimentFixes/DatasetRouterMemoryOwnershipTests.cs) проверяет внешнюю мутацию признаков и меток, независимость возвращённого ответа, допустимые короткие/длинные маски и отказ при равных противоположных соседях. [DatasetRouterTrainingAtomicityTests](../../../../Tests/ExperimentFixes/DatasetRouterTrainingAtomicityTests.cs) проверяет настоящую замену: два прежних урока за первую корзину заменяются одним за вторую; ожидаемый ответ меняется с `[true,false]` на `[false,true]`. При append это условие не проходит. Null партии, null признаков/меток и ошибка перечисления сохраняют заранее заданный правильный ответ. Успешное перечисление и последующая валидная партия — положительные контроли.

| Сохранённый журнал | Итог |
|---|---|
| [R3 контроль без копий](../../../../experiments-v2/_runners/R102-R3-DATASET-ROUTER-MEMORY-attempt-0001/Run-DatasetRouterMemoryOwnershipTests-CONTROL-input-array-copy-removed.log) | 45/47, два отказа |
| [R3 финальный](../../../../experiments-v2/_runners/R102-R3-DATASET-ROUTER-MEMORY-attempt-0001/Run-DatasetRouterMemoryOwnershipTests-FINAL-after-restore.log) | 47/47 |
| [R4 контроль Clear-before-read](../../../../experiments-v2/_runners/R102-R4-DATASET-ROUTER-ATOMICITY-attempt-0001/Run-DatasetRouterTrainingAtomicityTests-CONTROL-clear-before-read-restored.log) | 52/56, четыре отказа |
| [R4 финальный](../../../../experiments-v2/_runners/R102-R4-DATASET-ROUTER-ATOMICITY-attempt-0001/Run-DatasetRouterTrainingAtomicityTests-FINAL-after-restore.log) | 56/56 |

47 и56 — размеры соответствующих наборов тестов, не количество новых опытов или независимых циклов. Здесь есть реальный `Train` памяти и чтение выученного ответа; это не градиентное обучение и не доказательство роста общей точности модели. R3/R4 не ставятся заново в очередь реализации.

### Domain classifier: узкая приёмка успешна, расширенная — нет

[RouteClassifierMemoryOwnershipAndAtomicityTests](../../../../Tests/ExperimentFixes/RouteClassifierMemoryOwnershipAndAtomicityTests.cs:91) принимает два урока PLAN/CODE_Q. После внешней подмены признаков первого урока ожидается прежний PLAN. Для пустой партии сначала принимается настоящий урок PLAN; отказ обязан сохранить его. Отдельно покрыты null, ошибка чтения, валидная замена вместо append, восстановление приёма после отказа, compatibility wrapper и consumer unit.

| Сохранённый журнал C03 | Итог и допустимый вывод |
|---|---|
| [До исправления](../../../../experiments-v2/_runners/C03-ROUTE-MEMORY-OWNERSHIP-attempt-0001/Run-RouteClassifierMemoryOwnershipTests-attempt-01-BEFORE-FIX-red.log) | 81/90, девять отказов |
| [После исправления](../../../../experiments-v2/_runners/C03-ROUTE-MEMORY-OWNERSHIP-attempt-0001/Run-RouteClassifierMemoryOwnershipTests-attempt-02-AFTER-FIX-green.log) | 90/90 в выбранном наборе |
| [Расширенный набор](../../../../experiments-v2/_runners/C03-ROUTE-MEMORY-OWNERSHIP-attempt-0001/Run-RouteClassifierMemoryOwnershipTests-attempt-03-FINAL-all-consumers.log) | 225/226; не завершённая общая приёмка |
| [Отдельный повтор падающего теста](../../../../experiments-v2/_runners/C03-ROUTE-MEMORY-OWNERSHIP-attempt-0001/Run-RouteClassifierMemoryOwnershipTests-attempt-04-isolated-child-process-test.log) | Родительский тест0/1; вложенные строки Passed1/1 относятся к дочерним процессам |

Падающий тест: `DefaultLogicalCapabilityTrainerTests.Publication_TwoChildProcessesOneReservationOnePublisher`. Факт его падения не доказывает регрессию алгоритма маршрутизации, но запрещает скрывать незакрытый gate. Не измерены сравнительные память/скорость новых копий. Изоляция исправляет явную логическую ошибку сохранности правильного выученного ответа; это отдельное пользовательское основание допуска, не выдуманный ресурсный выигрыш.

## Исторический план C03-PUB — ремонт подтверждён01.11, не исполнять повторно

Ниже сохранён первоначальный рецепт30.10. Он описывает код **до ремонта**. Актуальное состояние: `Assert.Fail` на GO_TIMEOUT, трассы этапов и сохранённый green3/3; точные ограничения — в110. Команды этого раздела не являются новым заданием на повтор.

**Основание:** успешные переносы сохранности памяти выше нуждаются в честной приёмке; дополнительно найден прямой дефект диагностики. В [RunPublicationChildAsync](../../../../Tests/DefaultLogicalCapabilityTrainerTests.cs:4906) отсутствие `go` записывает `GO_TIMEOUT` и делает обычный `return`. Дочерний xUnit-тест потому может стать зелёным, не дойдя до `TrainAndRegisterAsync`. Это видно из кода, а не доказывается одной строкой Passed. Сохранённые журналы не содержат временной трассы, достаточной для установления причины неготовности дочерних процессов.

**Изменять в будущей реализации:** только протокол теста и его helpers в `Tests/DefaultLogicalCapabilityTrainerTests.cs`: `Publication_TwoChildProcessesOneReservationOnePublisher` (4627), `StartPublicationChild` (4823), `RunPublicationChildAsync` (4906), `WaitForFilesAsync` (4936). Продуктовые reservation/обучение/публикация не меняются по этим журналам.

1. `GO_TIMEOUT` остаётся сохранённым диагностическим результатом, но завершает дочерний тест отказом, а не успешным return. Не считать timeout выигравшим или проигравшим reservation.
2. Разделить измеряемые этапы запуска процесса, входа в child test, ready, go, вызова trainer и terminal. Записывать монотонные длительности и ID процесса/попытки; не выводить истинный exit=-1, если процесс ещё жив — обозначить `RUNNING`.
3. Ожидать готовность/выход по одному дескриптору процесса с ограниченным deadline. Сейчас диагностическая ветвь после30с ожидает `ReadToEndAsync` без собственного ограничения; это может задерживать отказ до окончания child. Сначала ограниченно завершить/дождаться child, потом собрать доступный вывод. В `finally` не удалять общий каталог, пока дочерние процессы ещё могут писать. Не добавлять повторные запуски как средство скрыть первый отказ.
4. Бюджет будущего ремонта: один адресный запуск родительского теста, два child. Сохраняются исходные `BuildTrainingRequest`, `BuildTopologyFor44`, `RecordingLogicalCapabilityTrainingAdmissionAuthority`, Domain.BUG и native `TrainAndRegisterAsync`; максимум два вызова trainer, ожидаются один PUBLISHED и один из уже разрешённых reservation-отказов. Это диагностика существующей учебной публикации, не новый сравнительный опыт качества и не разрешение заменить обучение mock-успехом. Два отрицательных контроля протокола (нет go; ранний выход child) не вызывают trainer; они дополняют, но не заменяют основной учебный сценарий.
5. Нельзя чинить тест удалением child-assertions, поиском слова Passed, исключением из фильтра, принятием GO_TIMEOUT или увеличением задержек без измерения этапов. Если измерения покажут медленный старт testhost, выбрать отдельный startup deadline по наблюдению; не менять бюджет собственно обучения. Причина сейчас **OPEN**, а не «среда тормозит».

Будущие адресные команды из `C:/Projects/llm_mvp_core` (сборка включает восстановление зависимостей; затем один родительский тест):

```powershell
dotnet build Tests/LlmFromScratch.Tests.csproj --configuration Debug
dotnet test Tests/LlmFromScratch.Tests.csproj --configuration Debug --no-build --no-restore --filter "FullyQualifiedName~Publication_TwoChildProcessesOneReservationOnePublisher" --logger "trx;LogFileName=parent-publication.trx" --results-directory experiments-v2/_runners/C03-PUB-REPAIR-attempt-0001
```

Это **не исполненная команда**. Если каталог уже существует, использовать следующий свободный `attempt-NNNN`, не стирать прежние результаты. Предварительная сборка должна включать изменённый test owner; старый `--no-build` бинарник не доказывает ремонт. Отчёт — по-русски: время каждого этапа, родительский terminal, оба child terminal/result, число фактических trainer-вызовов, сохранённый publisher и reservation loser, оставшаяся граница. Навигация по skills, ожиданию и правилу немедленного исполнения прямого «стоп/следующий» — [03](03-test-and-execution-contract.md). Исполнитель использует свою модель по умолчанию, не выбирает дополнительную дорогую модель.

## Конкретный путь e2e для проверки переноса памяти

Текущий consumer уже можно назвать точно: [InternalLlmGateway.InspectRouting](../../../../21.AgentInfrastructure/Loop/InternalLlmGateway.cs:80) → `SemanticTeachingPromptCompiler.Compile` → [DomainClassifierUnit.BuildRequestFromCompilation](../../../../23.SemanticCompilerAdapter/DomainClassifierUnit.cs:80) → `EncodePredicateProfile` → зарегистрированный либо переданный `DomainClassifierUnit` → trained `Classify(int[])` → `MapDomain` → `ResolveAdapterWithFallback`. При confidence<0.30 выбирается Generic; доступность адаптера проверяется отдельно от выбранного домена. Нельзя молча заменить эту цепочку старым `SemanticCompilerPipeline.Compile` с другим overload.

Существующие [DpcDomainClassifierLogicalUnitTests](../../../../Tests/DpcDomainClassifierLogicalUnitTests.cs:132) уже описывают обычный prompt→Arch/arch.lora и registry→Plan/plan.lora. Здесь прочитан их код; новый запуск не выполнен. Их метки намеренно переопределяют keyword fallback, поэтому это доказательство назначения **проверки связи с памятью**, а не эмпирическая точность семантической классификации.

**Будущее расширение этих же тестов для C03, без новой архитектуры:** один исходный текст из существующего fixture — `Fix the auth crash in the authorization endpoint`. Сначала компилировать его действительным `SemanticTeachingPromptCompiler`, требовать непустой `EncodePredicateProfile`. Создать независимые A без обучения, B с одним уроком `(profileCopyB, ARCH)` и C с одним уроком `(profileCopyC, PLAN)`, используя k=1. Profile для C копируется до любой мутации B. Вызвать **InspectRouting с исходной строкой**, не передавать в gateway ручные slots. У B ожидается Arch/arch.lora до события, после мутации caller-массива и после отвергнутого `Train([])`; у C — Plan/plan.lora. A фиксирует обычный fallback, не назначается заранее как «ошибка обучения». Для B/C модельный registry содержит соответствующие реальные fixture paths из существующего теста; изменение адаптера должно следовать метке, а не недоступности файла. Сохранить проверку `UsedFallback` по контракту registry. Повторить тот же сценарий для разрешения через `InMemoryLogicalUnitRegistry`, не создавая третью оболочку.

Бюджет двух поверхностей: **4 успешных Train, 4 предъявления урока, 2 отвергнутых пустых Train, 10 InspectRouting** (по5: A, B до события, B после мутации, B после отказа, C). Две подготовительные компиляции плюс10 внутри gateway =12 Compile. Не добавлять незаметно отдельные Classify ради журнала; для диагностики достаточно сохранённых compiled IDs/profile, назначенных меток и actual domain/adapter/fallback. Сохранение/восстановление этого unit и генерация текста LLM в данный бюджет не входят; native checkpoint API здесь не показан. Это адресное учебное регрессионное покрытие успешного C03-переноса, а не новый результат качества и не разрешение переобучать общий маршрутизатор.

Owner будущих assertions — `Tests/DpcDomainClassifierLogicalUnitTests.cs`, например `GatewayKeepsLearnedRouteAfterCallerMutationAndRejectedBatch` и такой же случай с registry. Команда после сборки: `dotnet test Tests/LlmFromScratch.Tests.csproj --configuration Debug --no-build --no-restore --filter FullyQualifiedName~DpcDomainClassifierLogicalUnitTests`. Тесты/команда в этой работе не запускались. Сохранить существующие проверки fallback и выбора registry, не менять expected домены ради зелёного результата. Общая точность на нескольких доменах, обучение feature producer и fresh-load остаются отдельными осями R9; данным узким маршрутом они не закрываются.

## Усиление других карточек без подмены результата

- Приёмка владения/атомарности обязана называть полный тип owner, Train и читающий consumer; одинаковое имя метода у соседнего проекта не считается переносом.
- Успех дочернего процесса не заменяет terminal родительского теста. Итог широкого фильтра приводится рядом с узким, включая падения.
- Для lifecycle-ремонта сравнивается заранее известный правильный ответ после настоящего Train до/после мутации или отказа; исходный untrained ответ недостаточен.
- Успешная новая партия проверяет replacement отдельно от append; ошибка партии не даёт права без согласования изменить контракт пустой памяти, масок или ошибок.
- Любое полезное измеренное улучшение отмечается по своему показателю и домену; прочие изменения показываются рядом. Динамическая переработка кода дополнительно требует отсутствия ухудшения качества и скорости обучения по последнему правилу пользователя.

Эти требования внесены в действующий общий договор03 и адресные статусы102. Новые эксперименты, тесты и продуктовые изменения этим аудитом не исполнены.
