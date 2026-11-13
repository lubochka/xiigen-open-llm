# E18: обучаемая память доменов и нынешние получатели маршрута

Дата: 2026-10-16. Фаза — READ_ONLY_RESEARCH. Этот документ продолжает поиск; здесь нет новых запусков классификатора, обучения или проверки качества. Текущий снимок исходников: `259c61769c3c66c9353f70d3dd569da19682067b`. Старые результаты ниже прочитаны из сохранённых JSON, а не получены в этой работе.

## ДР-01. Положительный E18 обучал память размеченных признаков английских запросов

**Статус:** положительный исторический результат и механизм подтверждены источниками; причинное сравнение с нынешним маршрутизатором ещё не выполнено. Это обучение предметной памяти, даже без изменения нейронных весов.

Первичный результат — [run-2026-05-01-002.json](../../../35.ExperimentsV2/results/E18-DOMAIN-CLASSIFIER/results/run-2026-05-01-002.json:5), также прочитанный через `git show 530c1e0581fa854574bd8608a7576a80236f2ebd:35.ExperimentsV2/results/E18-DOMAIN-CLASSIFIER/results/run-2026-05-01-002.json`: строки 5–17 содержат `primary_metric=top1_accuracy`, `baseline_value=0.825`, `enhanced_value=0.95`, `improvement_pct=15.15`, `passed=true`. Это 33/40 → 38/40 правильных меток, +12,5 процентного пункта. Из 200 simulation-запросов на английском 160 использовались для памяти, 40 — для проверки. По классам сохранено: CODE_QUESTION 5/7, BUG_FIX 6/6, CODE_CHANGE 8/8, ORCHESTRATION 11/11, ARCH 8/8. `enhanced_time_ms=14.9` — поле старого результата, измеряющее enhanced-классификацию validation, не обучение Student и не замер нынешней работы.

Получатель метрики — `E18DomainClassifier.Run`, а не генерация ответа: в историческом `35.ExperimentsV2/Experiments/E18DomainClassifier.cs@530c1e0:17–44` формируются split 80/20 с seed 42, keyword baseline и kNN k=5, затем для **одних и тех же** validation-примеров сравниваются метки. Исходник также сопоставлен с `d4fc9fca8834cfe42240b78d9ffefff464777110`: diff этого runner и `Infrastructure/DomainClassifier.cs` пустой. Сохранённый JSON не содержит SHA исполнявшейся сборки, поэтому это связь артефакта с сохранённым снимком исходников, не доказанная аттестация бинарника.

В `35.ExperimentsV2/Infrastructure/DomainClassifier.cs@530c1e0:25–45` mutable state — список `(int[] Predicates, int Domain)`. `Train` очищает список и принимает обучающие примеры; `Predict` выбирает ближайшие по Jaccard и большинство меток. `JaccardDist:61–67` сравнивает множества предикатов. Детерминированы извлечение признаков, метрика и голосование; обучаемая часть — содержимое памяти и привязанные метки. Сохранение/восстановление этой памяти данный класс не реализует.

Исторический `TextToPredicates@530c1e0:70–95` выделял семь групп признаков непосредственно из английских слов (`null`, `fail/error/fix`, `add/create/implement`, `what/how/get`, `check/test`, `flow/plan`, `design/architect`). Прочитан не только комментарий: diff `4b5321a0c3a0108b592390cdec15beb952ac2f6c` действительно заменяет `SemanticNormalizer.Normalize → FamilyCollapser.Collapse` на эти detectors. Commit объясняет, что прежнее представление C# давало пустые признаки английских prompts. Дата результата сама по себе не устанавливает точный commit каждого раннего неудачного запуска; отрицательные апрельские JSON нельзя переносить на исправленный extractor.

`GenerateExamples@530c1e0:102–162` задаёт 25 шаблонов, по пять на класс, с **явными метками шаблонов** и случайными подстановками service/method/object/property. Train и validation отделяются по позиции в общей последовательности; отдельного разделения по семейству шаблона нет. Сохранённые JSON не содержат per-case prompts/IDs; точное пересечение строк между train/validation здесь не вычислялось. Историческая шапка «300 prompts, auto-labelled by keyword rules» не заменяет прочитанный код tier×100 и явные labels. Поле `ambiguity_ratio=0.565` проверяет отсутствие только пяти очевидных слов (`:97–100`), а не экспертно установленную семантическую неоднозначность.

**Граница эффекта:** проверялась метка одного из пяти разработческих намерений. Это не способность написать C# документацию, перевести EN→RU или сгенерировать правильное начало ответа. Повторные сохранённые tier-2 JSON с теми же 200/40, seed и итогами не считаются новыми независимыми разбиениями.

**Контраргумент к широкому переносу:** в нынешнем runner изменились одновременно представление и baseline; старый positive не доказывает превосходство любого класса с тем же именем. Самый узкий будущий контраст — два extractor на одном закреплённом корпусе и одном k=5; метрики отдельно по шаблонам и классам. Это только проект проверки, корпус сейчас не генерировался.

## ДР-02. E18 был перенесён через другую систему признаков; нынешняя цифра не является чистым повтором старой

**Статус:** различие реализации подтверждено; его вклад в качество экспериментально не установлен.

В commit `a98afb7ccf065bf626dcf619c57c1d30863df85b` изменён `E18DomainClassifier.TextToPredicates`: вместо непосредственных семи английских detectors используется `SemanticTeachingPromptCompiler`. Нынешний [runner](../../../35.ExperimentsV2/Experiments/E18DomainClassifier.cs:75) берёт development/pipeline predicates и дополнительно преобразует `compiled.Domain` в экспериментальные признаки (`:88–148`). Следовательно, получаемый вектор содержит результат другого маршрутизатора; это не независимое восстановление старого набора признаков.

Нынешний [Compatibility/DomainClassifier.cs](../../../35.ExperimentsV2/Compatibility/DomainClassifier.cs:32) обучает `DomainClassifierUnit`, а baseline `:51–54` создаёт новый **необученный** `DomainClassifierUnit`. Старый baseline был отдельным простым списком ключевых слов. Также старый CODE_CHANGE преобразуется в REFACTOR, ORCHESTRATION — в PLAN (`:59–69`). В старых CODE_CHANGE шаблонах есть `Add null check…` и `Create … that validates …`, тогда как нынешняя keyword-карта относит `implement/build/create` к PLAN. Совпадение имён семейств не устраняет это различие контракта.

Сохранённый поздний `35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E18-DOMAIN-CLASSIFIER/results/run-2026-05-07-002.json` сообщает 0,825 → 0,85, а не 0,95. Одновременно изменено несколько факторов, поэтому это **не доказательство регресса самого Jaccard learner**. Точная привязка сохранённого запуска к исполнявшемуся бинарнику не установлена.

Возможный человеческий эффект различия — запрос «добавь/реализуй» может обучаться под одной меткой, а признаки и fallback получать из другого разбиения намерений. Альтернативное объяснение позднего результата — изменённый baseline/компилятор, а не потерянная способность памяти. Будущий однопараметрический контраст: оставить corpus, labels, split, k и fallback фиксированными, поменять только extractor; отдельно вывести матрицу CODE_CHANGE/PLAN/REFACTOR. Никаких новых измерений для этого здесь нет.

## ДР-03. Старое «классификатор никогда не обучается» к нынешнему пути неприменимо

**Статус:** REUSE_NO_CHANGE для наличия действующего Train→публикация→получатель. Граница restart/load выделена в ДР-06; качество опубликованного классификатора этим чтением не измерено.

Исторический [PLAN-KNN-MATRIX-INTEGRATION-v3.md](<C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator implementation/PLAN-KNN-MATRIX-INTEGRATION-v3.md:128>) в строках 71, 128–185 описывает `_trained=false` и предлагает seeded classifier. Этот статус нельзя переносить на нынешний HEAD: diff `5fa9a2fca59eaacb5a6c94b402c0bde590048aa9` заменил статический `new()` на `BuildSeededClassifier`, добавил накопление examples на диске и wrapper/registry-получателя. `git branch --all --contains 5fa9a2f…` включает нынешнюю `codex/dpo-implementation`; это не исправление, оставшееся только в соседней ветке.

В [DefaultLogicalCapabilityTrainer.cs](../../../18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs:3510) `PrepareDomainClassifierPublication` получает predicate profile из скомпилированного prompt. Затем читает прежние examples, добавляет `(request.RunId, request.Domain, predicateProfile, time)`, устраняет дубликаты по паре domain+profile (`:3578–3597`) и выполняет:

```csharp
var classifier = new DomainClassifierUnit();
classifier.Train(examples.Select(
    static example => (example.Predicates, example.Domain)).ToArray());
```

Это точные строки `:3607–3609`. Сам Train находится в [DomainClassifierUnit.cs](../../../23.SemanticCompilerAdapter/DomainClassifierUnit.cs:64): передаёт примеры внутрь и устанавливает `_trained = examples.Count > 0`. Wrapper регистрируется как learned/Probationary в trainer `:1234–1244`. Сохранение JSON — не только подготовка: `PublishClassifierState` вызывается в `:1291`, а `:5478–5499` переносит staged файл в конечный путь с проверкой identity/hash.

В [AdaptiveLogicalOrchestrator.cs](../../../21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs:693) при `UseLearnedUnitWhenAvailable` ищется опубликованный unit роли domain classifier, начиная с tier Probationary (`:704–717`), а затем вызывается его `ExecuteAsync` с compiled profile (`:746–766`). При отсутствии подходящего unit создаётся новый `CompiledDomainClassifierUnit` — этот fallback сам по себе не доказательство загрузки обученной памяти.

В [InternalLlmGateway.cs](../../../21.AgentInfrastructure/Loop/InternalLlmGateway.cs:120) также есть отдельный lookup registry: используются `CompiledDomainClassifierUnit` или `DomainClassifierUnit`, иначе локальный default (`:139–150`). Confidence ниже 0,30 переводит запрос в Generic (`:134`). Это реальный получатель решения о домене, а не только тестовый caller.

**Контраргумент к «достаточно найти Train»:** наличие публикации не доказывает, что именно такой unit установлен и выбран в конкретном запросе RP01. У оркестратора явный scenario может пройти раньше compiler/classifier (`AdaptiveLogicalOrchestrator:355–357`). У adaptive-запроса цепочка видна: compile → classify → domain в metadata (`:359–373`) → route/branches (`:391–409`). У gateway domain определяет выбор адаптера (`InternalLlmGateway:80–90`). Прямой Student evaluation из [12](12-student-update-to-generation-path.md) этим фактом не превращается в вызов E18.

Наиболее дешёвый будущий контраст для самого наличия интеграции — два одинаковых вызова существующего adaptive consumer с одним compiled profile, registry без/с заранее обученным classifier, при неизменных flags/остальных units. Проверять выбранный unit ID, domain, evidence `trained_predicate_knn` и получателя маршрута; это не доказательство качества без независимой ожидаемой метки. Контраст здесь не выполнялся.

## ДР-04. Обученная ветвь пока не имеет собственного отказа по расстоянию

**Статус:** контракт исходника подтверждён; частота ошибочных маршрутов на реальных запросах не измерена.

[29.SemanticCompiler/Routing/RouteClassifier.cs](../../../29.SemanticCompiler/Routing/RouteClassifier.cs:70) хранит labelled examples. `Classify(int[]):77–89` берёт k ближайших и большинство меток: порога Jaccard distance, ограничения на отсутствие пересечения и отдельного механизма UNKNOWN/mixed-отказа в этой ветви нет. UNKNOWN может оказаться обучающей меткой, но не выводится из самого слабого соседства. `DomainClassifierUnit.ClassifyBase:158–182` при `_trained && request.Predicates.Length > 0` сначала принимает этот результат. Затем `Classify:118–119` до поправки по контексту поднимает confidence как минимум до 0,90 при `UsedKnn` — независимо от расстояния или отрыва голосов.

Для разработческого запроса вне области памяти это может означать уверенную метку по слабому соседству; gateway-порог 0,30 такой результат пропустит. Альтернатива: compiler profile и реально загруженный corpus достаточно хорошо покрывают запросы, поэтому отсутствие отказа может не вредить на текущем домене. E18 38/40 на своих validation-примерах не решает вопрос OOD или смешанных намерений.

Будущая узкая проверка: на одном неизменном published corpus сравнить текущий forced label с одним правилом отказа по заранее выбранному Jaccard-порогу; отдельно известные profiles, непересекающиеся допустимые profiles и mixed prompts. Метрики — правильный конечный маршрут, ложная уверенная маршрутизация и доля отказов; не свободная генерация Student. Подбор порога после просмотра контрольных ответов не допускается. Здесь эта проверка не запускалась.

## ДР-05. Прежняя независимая метка и частота примеров заменены накоплением собственных решений

**Статус:** подтверждённое различие обучающего контракта в `RunWithCapabilityGenerationAsync`; влияние на качество остаётся гипотезой. Домен — разработческие намерения, consumer — classifier после публикации.

В старом E18 метка закреплена автором simulation-шаблона; learner сохраняет все 160 обучающих строк, включая повторяющиеся profiles. Нынешний [AdaptiveLogicalOrchestrator.cs](../../../21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs:484) выбирает `trainingDomain` из первого результата, а при отсутствии метки — из собственного `PromptDomainClassifier`. Затем `:495–504` передаёт `Domain = trainingDomain` в `TrainAndRegisterAsync`. `TryReadDomainFromResult:4494–4509` читает classifier label из result/branch metrics. Это не внешний исправленный ответ на вопрос о домене. Git blame связывает выбор результата с `57600fe7c637ef68958def4639ca68ac2b797248`; fallback через metadata изменён `a98afb7…`.

Trainer `:3587–3596` добавляет этот label к profile и сворачивает **каждую одинаковую пару domain+profile до одного примера**. Следствия различны: ошибочное исходное решение может повторяться в памяти; частоты старого корпуса больше не дают прежних голосов; одинаковый profile с двумя разными labels сохраняется дважды, потому что domain входит в ключ. Повторная подача той же пары увеличивает дату последней записи, но не её число голосов. Это следует из кода; конкретная частота ошибок/коллизий в реальной persisted памяти не измерена.

**Контраргументы:** исходный classifier может правильно размечать нужный домен; сохранение его знаний полезно. Прямые callers `Train` или `TrainAndRegisterAsync` могут передать независимую правильную метку. Дедупликация может быть сознательной защитой от многократного повторения одного шаблона. Поэтому тезис относится к прочитанному автоматическому caller, а не ко всем способам обучения.

**Будущие раздельные контрасты:** сначала одна переменная — происхождение labels: тот же небольшой фиксированный набор profiles и k, метки firstRun против независимых предметных меток; измерить heldout route accuracy и ошибки, исправленные/закреплённые памятью. Отдельно, если нужно проверить частоты, меняется только сохранение кратности при полностью одинаковых labels. Объединять эти изменения в один «новый E18» нельзя. Если метки/профили дают одинаковые решения, гипотеза влияния на этот корпус не подтверждается; это не исключает другой домен.

## ДР-06. Файл памяти есть, но изученный cold session loader не восстанавливает domain classifier

**Статус:** установлен разрыв в конкретной цепочке capture/load. Не утверждается утрата файла, невозможность ручной загрузки или ошибка восстановления Student/LoRA.

Цепочка сохранения описана в ДР-03. Точный путь — `_checkpointRoot/domain-classifier-training.json` (`DefaultLogicalCapabilityTrainer:3984–3986`). Парсер вызывается только из подготовки следующей публикации (`:3577–3578`); конструктор trainer `:232–244` задаёт зависимости и root, не вызывает `Train/Load`. По поиску production `.cs` отдельного reader этого файла не найдено.

Изучен и реальный session-loader, поэтому вывод не основан только на grep. [TeachingSessionBuilder.cs](../../../15.OrchestratorModel/Logical/TeachingSessionBuilder.cs:302) читает snapshot и вызывает `TeachingRegistryPersistence.LoadIntoRegistries` (`:316–330`). [CaptureUnitEntries](../../../18.CapabilityTraining/TeachingRegistryPersistence.cs:1126) для unit без generated manifest записывает descriptor с `UnitKind=DeterministicUnit`, сохраняя его `IsLearned` (`:1148–1153`). Domain classifier имеет типы DomainClassifierRequest/DomainClassification и не является generated string→string retrieval unit. Его обучающие examples в descriptor не входят: [TeachingUnitRegistryEntry](../../../18.CapabilityTraining/TeachingRegistrySnapshot.cs:29) содержит только identity/type/version/trust/order/kind.

При load `TeachingRegistryPersistence:834–845` пропускает entries без manifest и generated kind; `:930–958` создаёт только точный `TransformerEmbeddingKnnLogicalUnit<string>`. Таким образом, descriptor domain classifier не превращается обратно в обученный classifier. Сохранившийся отдельный JSON снова используется при следующем `PrepareDomainClassifierPublication`, но это **продолжение обучения**, а не восстановление перед первым запросом. Current filter прочитан также в истории `61d25ab5c75e939b44dcd1cb4325ff550dce02c1`; этот commit не объявляется началом всего пробела: более ранний lifecycle полностью не расследован.

**Возможный эффект:** сразу после перезапуска adaptive route использует default/другой доступный classifier вместо прежней накопленной памяти; после нового training память может вернуться. **Контраргументы:** живой registry сохраняет опубликованный unit; приложение может внедрить свой classifier; static helper имеет семь seed-примеров; ещё не прочитанный внешний host может восстановить unit отдельно. Ни один cold-запрос здесь не запускался.

**Будущий контраст:** сохранить один и тот же обученный classifier и registry snapshot, затем сравнить live session с новой сессией через существующий `TeachingSessionBuilder`, без дополнительного Train. Не менять JSON, labels, compiler или profile. Зафиксировать наличие exact unit ID, источник domain/evidence и результат на заранее выбранном profile, где learned и fallback различаются. Отрицание гипотезы — штатная новая сессия сама восстановит тот же learned unit и память; совпадение одного label без unit/state proof недостаточно. Такой restore-design не требует Student/DPO.

## ДР-07. Одноимённые классы и сегментная маршрутизация читают разные состояния

**Статус:** различие получателей подтверждено. ДР-01 нельзя переносить на каждую строку «RouteClassifier» в коде или отчёте.

| Получатель при HEAD `259c617…` | Что реально вызывается | Обучаемая память E18 |
|---|---|---|
| `PromptDomainClassifier.Classify:22–36` | Static classifier, если explicit instance не передан | Семь фиксированных seed-примеров из `:76–88`; это не 160 исторических примеров и не чтение persisted памяти |
| `PromptDomainClassifierUnit:149–168` | По умолчанию новый `DomainClassifierUnit`, передаваемый явно | Этот default не пользуется seeded static instance; production construction такого wrapper в выполненном поиске не найдено |
| `DomainClassifierUnit:64–67,160–164` → [project12 wrapper](../../../12.SemanticCompiler/Routing/RouteClassifier.cs:23) → project29 `Classify(int[])` | Jaccard kNN по примерам, если Train был вызван | Да; actual publication/consumers — ДР-03 |
| [SemanticCompilerPipeline.cs:68–70](../../../29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs:68) → project29 `Classify(string[])` | Fixed predicate votes, затем keyword fallback (`RouteClassifier:52–66`) | Эта перегрузка `_train` не читает, даже если тот же объект ранее обучался |
| [project22 RouteClassifier:21–34](../../../22.AgentAlgorithms/Routing/RouteClassifier.cs:21) | `ClassifyByKeywords`, confidence 0,95 для непустого распознанного label | Нет Train; найденные callers — Inspector sample (`InspectorSession:65,268`), LibraryComparison (`Program:20,75–80`) и optional comparison harness (`AgentComparisonHarness:84–87`) |

Для составного prompt есть ещё более узкая граница. [PromptRoutingInputCompilerUnit:119–169](../../../21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs:119) при нескольких сегментах разрешает **отдельную** роль `AttributedSegmentRouterUnit` через registry либо создаёт default. Обученный domain classifier другой роли автоматически в него не внедряется. [AttributedSegmentRouter:85–98](../../../23.SemanticCompilerAdapter/AttributedSegmentRouter.cs:85) вычисляет `classified`, но выбирает уже известный `segment.ExistingDomain` вместо `classified.Label`. При этом confidence и evidence берутся от `classified`. Этот приоритет существует с создания участка в `1917945c5793940d1ca8c488656f080c3c82ce4d`, а не обнаружен только в комментарии.

**Возможный эффект:** исправление learned label верхнего prompt не обязательно исправляет домен каждого сегмента; confidence/evidence могут относиться к другому решению, чем возвращённая метка. **Контраргумент:** compiled domain может быть намеренно авторитетным и верным, а segment label и classified label могут совпадать. E18 проверял один label на целый prompt, поэтому не измерял эту границу вообще.

**Будущий контраст:** неизменный двухсегментный compilation и один injected trained classifier; меняется только правило приоритета ExistingDomain/learned label в segment consumer. Нужны заранее независимые labels каждого сегмента, итоговые subphase routes и согласованность label/confidence/evidence. Совпадение результатов опровергнет влияние на выбранные случаи; само изменение label без улучшения маршрута не является выигрышем. Для перегрузки string[] отдельная дешёвая будущая проверка — label-flip одной и той же памяти и чтение обоих overload на одном фиксированном profile; это проверка зависимости от state, не тест смыслового качества.

## ДР-08. Язык, версия признаков и UNKNOWN: точные границы

В [DomainClassifierRequest:12–20](../../../23.SemanticCompilerAdapter/DomainClassifierUnit.cs:12) нет типизированных language/encoder-version полей; произвольная Metadata возможна, но в прочитанных Classify/Train эти два значения не формируют отдельные памяти. На диске `DomainClassifierTrainingExample:7274–7278` хранит RunId, Domain, Predicates, CreatedAtUtc. Парсер `:3733–3743` принимает ровно эти имена. Hash/identity защищают байты публикации; это не версия смысла числового predicate slot. Проверяется числовой диапазон/уникальность (`:3910–3928`), но сама таблица `TrainingVectorBuilder.PredicateSlotFor` не сохранена рядом с примером.

Это **риск совместимости**, а не найденный случай смены значения конкретного slot. Будущий однопараметрический design — два extractor-version для одного сырого корпуса при неизменных labels, с проверкой профилей и конечных routes; сначала нужна конкретная историческая смена mapping, которой этот пакет не устанавливает. Не требуется безусловно переписывать mapping или вводить миграцию без такого evidence.

Old positive покрывает английские разработческие prompts. Английские keyword maps и отсутствие поля language не доказывают неработоспособность русского: compiler может привести разные языки к одним predicate IDs. Но результат 38/40 не распространяется на русский/иврит, ENRU translation или смешанные намерения. Узкий будущий языковой контраст — параллельные смысловые prompts, одинаковые ожидаемые domain labels, один corpus/consumer; менять только язык входа и отдельно смотреть profile, label, confidence, маршрут. Это предложение, не новая оценка.

UNKNOWN в source имеет несколько смыслов: пустой запрос возвращает UNKNOWN/0,20 (`DomainClassifierUnit:102–103`); непустой keyword fallback без совпадений возвращает CODE_Q (`project29:93–99`), хотя adapter confidence может оставить его низким; trained int[] ветвь возвращает label соседей без собственного OOD-отказа (ДР-04). Это не единый контракт «непонятный запрос → UNKNOWN». Наличие domain enum не означает проверенную смешанную/неизвестную классификацию.

## Что этот пакет установил и что остаётся искать

- Полезный historical donor E18 установлен: именно labelled predicate memory с положительным 38/40 против 33/40. Подтверждена конкретная поправка несовместимого с английским представления (`4b5321a0…`). Этот donor **не объявляется** тем самым матричным механизмом, который пользователь помнит как перенесённый в функцию kNN: прямой связи с такой матрицей здесь нет.
- Исправление «никогда не Train» действительно вошло в текущую ветку (`5fa9a2f…`). Повторять его как новый ремонт не нужно. Остаются другие контрасты: labels/кратность примеров, restore, OOD-confidence, отдельные consumers/сегменты и версия/язык признаков.
- Прочитаны 16 сохранённых E18 JSON выбранных April30/May1/May7 серий; это не 16 новых проверок. Подробно сопоставлены положительный May1-002 и поздний May7-002. Прочитаны тела названных classifier classes, адресные участки trainer/orchestrator/session persistence, первичный исторический plan и адресные git diff/blame. Ветки не обходились заново: проверено contains конкретного repair commit. Старые JSON в исследованных путях — данные, не LFS pointers.
- Не прочитана фактическая пользовательская `domain-classifier-training.json`; нет inventory реальных его labels/коллизий, результата cold restart или языкового heldout. SHA исполнявшихся старых бинарников не установлен. Текущие тесты, сборки, inference, training и probes не выполнялись; исходники/данные не менялись.
- Следующие первичные адреса по этой оси: конкретный сохранённый classifier JSON выбранного прежнего product-run и его registry snapshot; вызывающий host `TeachingSessionBuilder` с установкой optional logical registry в `InternalLlmGateway`; `TrainingVectorBuilder.PredicateSlotFor` в историческом encoder commit, если появится свидетельство несовместимых profiles. Это дальнейший поиск источников, не назначение запуска.

Изменён только этот Markdown. Общий исследовательский поиск и соседние документы этим пакетом не закрываются.
