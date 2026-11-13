# KN-ROUTER-01 — доступен ли выученный маршрут при обычном написании C#/.NET

**Статус: протокол с точными записями, NOT_RUN_NATIVE_RUNTIME_UNAVAILABLE.** Никакие результаты ниже не измерены. Исходник и исправление: [KN-ROUTER](KN-ROUTER-LEARNED-KEY-CANONICALIZATION-2026-10-20.md). Core pin: `bf9b543e149ef197da85a767821d98c9b67916d2`.

**Правило успеха:** любое фактически измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Рядом показать изменения остальных показателей, включая ухудшения. Увеличение внутреннего confidence само по себе не объявлять полезным улучшением: оно может сделать ошибку более уверенной.

## 1. Проверяемая гипотеза и участие в обучении

После настоящего обучения профилей чтение с той же нормализацией, что использовал trainer, повышает доступность выученного маршрута на C#/.NET написаниях. Возможный результат — меньше лишних отказов, без роста неправильных принятых маршрутов. Это однофакторный опыт **доступности обученной памяти**; он не измеряет качество генерации документации и не демонстрирует обучение весов.

Цепочка: `RoutingProfileTrainer.Train(TRAIN) → UnitScoringProfile.Keywords → BaselineKeywordCandidateGenerator(profiles) → TrainableRoutingFlow.RouteAsync(EXAM)`. В core она уже соединена [bootstrap:22–38](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/26.TrainableRouter/Training/DistributedRoutingBootstrap.cs#L22-L38) и [учебным runner:30–61](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/26.TrainableRouter/Training/RoutingTrainingRunner.cs#L30-L61). Не создавать параллельный словарь «правильных ответов». `ChosenUnitId` EXAM известен только оценщику и не передаётся в RoutingRequest.

Получатель: словарная память признаков маршрутизатора; нейронные слои, LoRA, optimizer, GPU, внешний учитель и DPO loss — неприменимы. Один Flow читает два обученных профиля. Новые профили должны возникнуть из фактических вызовов trainer.

## 2. Точные данные

Полная схема и значения — [KN-ROUTER-FIXTURE-2026-10-20.json](KN-ROUTER-FIXTURE-2026-10-20.json). Это вручную составленные короткие учебные/экзаменационные задания для данного механизма, а не архивные пользовательские ответы.

Два TRAIN:

- `Explain C# .NET` → `unit-dotnet-legacy-doc-l1`.
- `Translate Russian English` → `unit-ru-en-general`.

Оба используют пустой TaskSlice, null direction/hint и одинаковые два CandidateUnitIds. Метаданные — настоящий `CreateDefaultUnitMetadataRegistry()` без изменения prior0.05/floor0.20: [исходник:16–43](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/26.TrainableRouter/Training/OrchestrationRoutingCurriculum.cs#L16-L43).

| EXAM | Запрос | Ожидаемый ответ оценщика | Назначение |
|---|---|---|---|
| R01 | Describe C# .NET | documentation Unit | Оба исходных написания |
| R02 | Describe C# dotnet | documentation Unit | Одно исходное написание |
| R03 | Describe csharp .NET | documentation Unit | Второе исходное написание |
| R04 | Describe csharp dotnet | documentation Unit | Уже нормализованный контроль |
| R05 | Translate Russian notes into English | translation Unit | Другой домен |
| R06 | Translate a C# .NET guide from Russian into English | translation Unit | Не спутать предмет перевода с документированием |
| R07 | Plan a deployment | knowledge edge | Неподдержанный маршрут |
| R08 | Describe C# .NET | documentation Unit | С TaskSlice `legacy dotnet code documentation` и DomainHint `documentation`; контроль маскирующей подсказки |

R01–R04 — варианты одного смыслового семейства, не четыре независимых закона. R08 воспроизводит **форму подсказки** реального documentation caller, но не утверждает вызова полного `CodeDocumentationRoutingFlow`.

## 3. Сравнение без подмены объекта

1. Зафиксировать pin, исходники, JSON, default metadata и `RoutingFlowThresholds.Default` (confidence0.40, gap0.05). Проверить наличие нативного SDK и штатных зависимостей; отсутствие → BLOCKED_ENVIRONMENT, без установки и без JS/Python имитации learner.
2. Подготовить две изолированные копии того же core. A — исходный reader. В B применить только указанные две нормализации в оригинальном `BaselineKeywordCandidateGenerator.cs`. Не менять обучающий trainer. Запускать runner с нормальной ссылкой на `TrainableRouter.csproj`, не с переписанными классами.
3. A0/B0: создать `BaselineKeywordCandidateGenerator` с пустым набором профилей, передать реальному `TrainableRoutingFlow`, выполнить все восемь `RouteAsync`. Это законное состояние до записи уроков; `Train(empty)` вызывать нельзя — API его отвергает.
4. A1/B1: в каждом плече вызвать настоящий `new RoutingProfileTrainer().Train(TRAIN)` ровно один раз. Сохранить возвращённые profiles; передать **именно их** новому reader/Flow; выполнить тот же EXAM без изменения входов/порядка. До EXAM зафиксировать совпадение обученного содержимого A/B; не подправлять keywords после trainer.
5. Все четыре состояния используют одинаковые routing thresholds, candidate order и gold. Преобразование нормализации находится в reader B, не в fixtures. Файл EXAM не подаётся trainer. `RoutingTrainingRunner` не нужен: он создаёт дополнительные DPO-pairs после evaluation; здесь строить/возвращать их в обучение незачем.
6. Сохранить оригинальные native результаты и ошибки, затем вычислить изменения A1−A0, B1−B0 и разность этих изменений. Не делать вывод «обучение улучшилось», сравнив только две заранее заполненные карты или два scorer без уроков.

**Предел первого опыта:** 2 вызова Train всего, 4 обработанные учебные строки всего, 32 RouteAsync (8×4), 0 optimizer updates, 0 model API, 0 cold restore, 0 повторов. A0/B0 считать отдельно в этом протоколе; не молча вычитать вызовы из бюджета. Встроенные `ScoreAsync` внутри Route — вложенная работа, не ещё32 внешних эксперимента. Новый source-level unit-test suite или полный CI для этой карточки не требуются.

Этот документ задаёт нативные вызовы и патч, но не содержит уже скомпилированного harness. Исполнитель должен реализовать небольшой runner; до появления фактического бинарника/команды нельзя писать READY_TO_RUN/PASSED.

## 4. Что сохранять и как трактовать

На каждую строку/состояние: сырой запрос, chosen Unit или knowledge edge, все ranked candidates, score/confidence/gap, authority role, native error; отдельный gold, не присутствующий во входе. Для профилей: ключи и содержимое до/после, фактический счёт Train/строк; для запуска — команда, SDK, commit/diff, wall/CPU/peak RAM, training-time, routing-time и размер сериализованной памяти. У небольшого опыта timing может быть ниже разрешения измерителя; тогда написать «различие не установлено», без автоматического расширения.

Показывать отдельно:

- правильные принятые маршруты /7 поддержанных заданий;
- неправильные принятые маршруты /8 всех заданий;
- лишние отказы /7 поддержанных и правильный отказ /1 неподдержанного;
- каждый из R01–R08, а не только общий процент;
- затраты и изменения памяти; confidence/keyword hits — объясняющие сведения.

Полезным эффектом может быть уменьшение лишних отказов, снижение ошибочных направлений, времени либо ресурса. Рост одного полезного показателя остаётся успехом в нём даже при ухудшении другого; ухудшение обязательно публикуется и может ограничить решение о внедрении. При неизменном trainer нельзя приписывать reader ускорение обучения. Если меняются только keywords/hits без полезного наблюдаемого результата или затрат, гипотеза качества на этой панели не подтверждена. Если A уже решает все строки из-за подсказки, это результат контроля, а не основание убрать неудобную строку.

## 5. Промпт исполнителю

> Проверь KN-ROUTER-01 нативным C# на закреплённом core и точном fixture: два настоящих урока профилей, восемь одинаковых EXAM до/после в A/B. Единственный фактор — две нормализации C#/.NET в original BaselineKeywordCandidateGenerator.cs перед lowercase. Получатель — RoutingProfileTrainer, записанное состояние — UnitScoringProfile.Keywords, читатель результата — тот же TrainableRoutingFlow.RouteAsync. Сохрани фактические profiles, raw decisions и ресурсные показатели; не вручную задавай keywords, не передавай gold в request, не запускай learner-mirror, DPO, полный CI или внешний model call. Всего2 Train,4 учебные строки,32 Route. Любое измеренное улучшение полезного показателя — успех в этом показателе и домене; рост точности не обязателен, все ухудшения рядом. Не называй confidence качеством и не распространяй локальный эффект на нейронный RP01 или документацию целиком. Если учебная цепочка не исполняется, не подставляй декоративное обучение и не запускай несвязанные проверки. При отсутствии SDK/зависимостей запиши BLOCKED_ENVIRONMENT и фактический препятствующий шаг. docs/plans/replanning-possible-diffs не менять.

