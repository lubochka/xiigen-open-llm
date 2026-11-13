# KN-ROUTER — урок сохраняет «csharp/dotnet», читатель ищет исходные «C#/.NET»

**20.10.2026 · новое адресное расхождение кода · влияние на обучение ещё не измерено.** Проверен core `bf9b543e149ef197da85a767821d98c9b67916d2`. Это гипотеза о доступности результата обучения словарной памяти маршрутизатора. Она не доказывает причину ухудшения нейронного RP01 и не относится к градиентам DPO/LoRA.

## Что теряется и почему это касается обучения

Получатель двух уроков — настоящий `RoutingProfileTrainer`. Он создаёт для выбранного Unit набор слов и возвращает `UnitScoringProfile`; до уроков этих записей нет. При обучении `C#` заменяется на `csharp`, а `.NET` — на `dotnet`. Читатель полученного профиля выполняет только lowercase исходного запроса. Поэтому сохранённое слово может не совпасть с тем же названием технологии при последующем обращении.

| Звено | Проверенное действие | Точный исходник |
|---|---|---|
| Запись урока | `Train` собирает слова по `ChosenUnitId` в новый набор, строит профили и возвращает их | [RoutingProfileTrainer:32–72](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/26.TrainableRouter/Training/RoutingProfileTrainer.cs#L32-L72) |
| Представление при обучении | Две замены выполняются **до** разбиения на слова | [RoutingProfileTrainer:93–109](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/26.TrainableRouter/Training/RoutingProfileTrainer.cs#L93-L109) |
| Чтение обученного состояния | Конструктор хранит переданный набор профилей; текст запроса только приводится к lowercase; hits считаются через `Contains` | [BaselineKeywordCandidateGenerator:18–20,36–63](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/26.TrainableRouter/Training/BaselineKeywordCandidateGenerator.cs#L18-L63) |
| Наблюдаемый результат | Количество совпадений меняет score/confidence; при недостаточной уверенности или разрыве Flow возвращает knowledge edge вместо выбранного Unit | [TrainableRoutingFlow:53–82,147–162](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/26.TrainableRouter/Training/TrainableRoutingFlow.cs#L53-L82), [пороговое решение](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/26.TrainableRouter/Training/TrainableRoutingFlow.cs#L147-L162) |
| Реальный соединённый caller | Bootstrap вызывает `Train`, передаёт **его** профили scorer и возвращает Flow; `RouteAndLocateAsync` далее читает решение | [DistributedRoutingBootstrap:22–38,49–57](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/26.TrainableRouter/Training/DistributedRoutingBootstrap.cs#L22-L57) |
| Нативный учебный runner | Сначала `Train(trainingExamples)`, затем `Flow.RouteAsync` для evaluation | [RoutingTrainingRunner:30–61](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/26.TrainableRouter/Training/RoutingTrainingRunner.cs#L30-L61) |

Контрпример для будущего запуска: урок «Explain C# .NET» создаёт слова `explain,csharp,dotnet`; запрос «Describe C# .NET» не содержит ни одного из них в представлении нынешнего читателя. Это вывод из строк кода, **не выполненный C# опыт и не измеренная точность**. В этом механизме реально обучается память признаков, но score и правило выбора остаются детерминированными. Код помечает scorer ролью `BaselineCandidateGeneratorRole`; исправление не превращает его в нейронный роутер.

## Где эффект может отсутствовать

В `CodeDocumentationRoutingFlow` стандартный caller заранее задаёт `DomainHint="documentation"` и включает `legacy dotnet code documentation` в TaskSlice. Это одновременно даёт доменную подсказку и уже нормализованное слово `dotnet`. Они способны скрыть расхождение. Нельзя переносить выигрыш на запросах без подсказки на этот caller без отдельного наблюдения: [проекция обычного запроса:173–199](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/26.TrainableRouter/Training/CodeDocumentationRoutingFlow.cs#L173-L199), [domainBoost:57–63](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/26.TrainableRouter/Training/BaselineKeywordCandidateGenerator.cs#L57-L63).

Основной домен — выбор .NET/C# Unit по тексту запроса. Перевод служит контролем: упоминание C# в просьбе перевести текст не должно автоматически превращать её в задачу документирования. Для AST, порядка слов, POS, семантических токенов перевода и LoRA эта карточка ничего не утверждает.

## Что исправлять в оригинале

В `26.TrainableRouter/Training/BaselineKeywordCandidateGenerator.cs`, строка36, применить те же две замены к объединённому тексту перед lowercase. Использовать уже существующие internal-константы `RoutingProfileTrainer`, чтобы не завести второй набор написаний:

```csharp
var text = $"{request.OriginalPrompt} {request.TaskSlice} {request.RequiredDirection} {request.DomainHint}"
    .Replace(RoutingProfileTrainer.CSharpFrom, RoutingProfileTrainer.CSharpTo, StringComparison.OrdinalIgnoreCase)
    .Replace(RoutingProfileTrainer.DotNetFrom, RoutingProfileTrainer.DotNetTo, StringComparison.OrdinalIgnoreCase)
    .ToLowerInvariant();
```

Для причинного опыта менять только эту семантику читателя; не менять уроки, stopwords, `Contains`, доменные бонусы, пороги, classifier, ranking или профили вручную. Переход к общей функции нормализации — возможная последующая уборка кода, не дополнительный фактор опыта. Перед производственным внедрением отдельно согласовать описание этой семантики с владельцем version/cache policy; текущая работа код core не меняет.

## История и новизна

Расхождение есть и в соседней ветке `orchestrator-extensions@b293045bd89b1eb96d3e32fedd341f1a4170adbc`: [trainer:65–80](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/26.TrainableRouter/Training/RoutingProfileTrainer.cs#L65-L80), [старый KeywordUnitScorer:27–48](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/26.TrainableRouter/Training/KeywordUnitScorer.cs#L27-L48). Это **унаследованное расхождение**, а не доказанный регресс при последнем переносе.

Проверен локально полученный корпус 555 Markdown `possibleDifs` на planning SHA `1ad622fe1f6d1e8b0860265d8c208aeada3fefb0`: точные имена `RoutingProfileTrainer`, `BaselineKeywordCandidateGenerator`, `KeywordUnitScorer` не встретились; расширенный поиск сочетаний routing/маршрутизация/нормализация и C#/.NET не выявил этой цепочки. Новизна ограничена этим корпусом, не всеми когда-либо существовавшими материалами.

В прочитанном защищённом каталоге `docs/plans/replanning-possible-diffs/experimets` точной карточки нет. Активный `05-orchestration.md` проверяет другой получатель — `PredicateDatasetRouter` и признак `test_failed`; он не покрывает данное расхождение. Поэтому защищённый набор нельзя назвать полностью покрывающим эту находку. Дополнение подготовлено рядом: [маленький опыт и промпт](KN-ROUTER-SMALL-LEARNING-EXPERIMENT-2026-10-20.md), [точные записи](KN-ROUTER-FIXTURE-2026-10-20.json). Защищённая папка осталась без изменений.

**Приоритет:** после разблокирования прямого обучения и обязательных пользовательских обновлений; затем как малый опыт чтения его результата по правилу4 пользователя. Не запускать отдельную проверку строк без `Train → profiles → Route`. В текущей среде `dotnet/csc/mcs/mono` не обнаружены; native запуск и новое обучение не состоялись.

