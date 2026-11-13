# 26. Обучение профиля маршрута, DPO-пара и успешный Flow — разные результаты

16.10.2026. READ_ONLY_RESEARCH. Исходники core `259c61769c3c66c9353f70d3dd569da19682067b`, без запуска. Этот участок продолжает вопрос о Unit/Flow и скрытой детерминированной подмене; не объявляет всякий детерминированный компонент ошибкой.

## Что действительно обучает RoutingTrainingRunner

[RoutingTrainingRunner.cs](../../../26.TrainableRouter/Training/RoutingTrainingRunner.cs):19–35 передаёт trainingExamples в RoutingProfileTrainer и создаёт Flow с BaselineKeywordCandidateGenerator. [RoutingProfileTrainer.cs](../../../26.TrainableRouter/Training/RoutingProfileTrainer.cs):32–72 собирает для каждого ChosenUnitId множество слов из Prompt/TaskSlice/RequiredDirection/DomainHint. Повторения удаляются, частоты не сохраняются. Порог, prior и другие metadata берутся из registry (112–124); обучение их не подбирает. Это настоящее накопление признаков по labelled examples, но не neural gradient и не обучение отрицательному контрасту.

В [BaselineKeywordCandidateGenerator.cs](../../../26.TrainableRouter/Training/BaselineKeywordCandidateGenerator.cs):36–63 score равен prior + число совпавших keyword substrings + фиксированные domain/direction boosts. Flow проверяет authority role именно baseline candidate generator ([TrainableRoutingFlow.cs](../../../26.TrainableRouter/Training/TrainableRoutingFlow.cs):38–47); имя класса не скрывает эту роль. Route также имеет confidence/score-gap gate (65–67). Нельзя описывать его как безусловный выбор любого первого слова.

### Конкретная несовместимость train/query текста

ProfileTrainer95–108 преобразует C#→csharp и .NET→dotnet перед извлечением keyword. CandidateGenerator36 только приводит весь query к lowercase; аналогичного преобразования нет. Поэтому keyword `csharp` не обязан совпасть с исходным текстом `C#`, хотя это то же обозначение домена. Остальные prompt/metadata могут содержать нужное слово, а domain boost компенсировать различие. Частота и влияние на route не измерены. Это узкая будущая проверка одинаковых обозначений, не повод переписывать scorer целиком.

Фильтр StopWords удаляет before/after и ряд глаголов. В одном домене это может снизить шум, в другом — удалить закон порядка действий. Поскольку score использует substring, а не token-boundary совпадение, совпадение части другого слова тоже может добавить hit. Эффект определяется конкретными profiles и конкурирующими units; общая accuracy из этого чтения не следует.

## Откуда появляются routing DPO-пары

Runner39–84 сначала оценивает evaluationExamples. Счётчик correctness сравнивает действительный выбранный Unit с ожидаемым. Но отдельный DecisionRecord63–78 записывает ожидаемый ChosenUnitId для принятого маршрута, а остальные кандидаты формирует относительно этой ожидаемой метки. Это может быть полезным материалом исправления, а не точным журналом того, какой Unit реально выбрал Flow. Эти два поля нельзя использовать взаимозаменяемо при разборе ошибок.

Затем [RoutingDpoPairBuilder.cs](../../../26.TrainableRouter/Training/RoutingDpoPairBuilder.cs):19–36 выбирает наиболее высоко оценённого альтернативного кандидата и создаёт тексты chosen/rejected. Rejected содержит явное `[WRONG: ...]`. **Сам этот runner не передаёт полученные пары обратно в optimizer:** они возвращаются в результате вместе с profiles/cases. Не найденная здесь обратная связь не доказывает отсутствия всех внешних consumers.

Поскольку пары образуются из evaluationExamples, будущий downstream, который их учит, должен перестать считать эти же случаи независимым экзаменом. Настоящее такое повторное обучение в данном исследовании не установлено. Важный уже существующий контраргумент к смешению: [CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient.cs](../../../26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient.cs):35–39 требует пустые routing Cases/DpoPairs и ровно одну RepositoryOwnedDpoPair. Это отдельные контракты; routing builder не объявляется источником текущих simulation500.

## Что означает PASS у GeneratedCapabilityFlow

[DefaultLogicalCapabilityTrainer.cs](../../../18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs):7282–7335 содержит factory: при разрешённом UseLearnedUnitWhenAvailable выбирается Probationary-or-better retrieval unit; при его отсутствии используется UniversalNineNodeDevelopmentFlow. В7373–7396 вход компилируется, CanonicalText передаётся найденному Unit. Это реальный вызов памяти, не возврат только учебной квитанции.

После непустого успешного output Flow7440–7470 устанавливает summary 0→1 и Verdict=PASS, переводит stored output через CapabilityResultTranslator и возвращает его. Здесь этот PASS означает состоявшееся исполнение и непустой результат. Метод не проводит независимый предметный экзамен или сравнение качества до/после. Сам по себе успешный Flow не доказывает, что learned ranking стал точнее, что Unit выразил новое знание своим текстом или что Flow научился коррекции промежуточного состояния.

В этом Flow один вызванный retrieval Unit. Нельзя переносить на него полную заявленную обучаемую many-to-many композицию только по интерфейсу ILogicalFlow. Другие topology/flow implementations существуют и требуют отдельного анализа. Прямой RP01 TextGenerator, напротив, не вызывает этот Flow — [03](03-hybrid-units-and-domain-representation.md), [12](12-student-update-to-generation-path.md).

**Проверенное контрдоказательство подмене текста:** затем прочитан [CapabilityResultTranslator.cs](../../../18.CapabilityTraining/CapabilityResultTranslator.cs):56–87,118–137. Этот вызов сохраняет `knnOutput` и при необходимости дописывает annotations; при непустом output он не заменяет его фиксированным ответом. Поэтому гипотеза «в данном текущем Flow полезный stored output целиком выбрасывается translator» не соответствует этому коду. Наличие добавленных focus/prompt lines всё ещё не является приобретённым знанием Unit.

## Малые будущие контрасты

1. На одном сохранённом profile задать C#/csharp и .NET/dotnet варианты одинакового запроса. Записать hits, domain boost, score-gap, отказ или выбранный Unit. Сначала проверить фактическую потерю признака, потом качество маршрута на заранее размеченных случаях. Другие слова/metadata не менять.
2. Для влияния отрицательных примеров сохранить те же положительные profiles и заранее определить независимый набор competing-unit cases. В отдельном будущем варианте включить только обучаемую коррекцию по exact routing pairs, не менять extractor и evaluator. Пара не должна одновременно оставаться heldout. Наличие пары без вызова её consumer не считается выполненным обучением.
3. Для Flow сравнить два сохранённых Unit-состояния при том же compiler/factory/input и оценить содержательную правильность фактического output. PASS/0→1 отдельно от качества. Stored output и добавленные translator annotations оценивать раздельно: утверждение в annotation не доказывает наличие этой способности в выученном ответе. Прочитана ветка сохранения output, но весь parser metadata translator отдельно не аудирован.

Опытов нет. Этот файл устанавливает границы трёх конкретных механизмов и потенциальных искажений; он не объявляет причиной провала neural RP01 любой из них без наблюдаемого consumer.
