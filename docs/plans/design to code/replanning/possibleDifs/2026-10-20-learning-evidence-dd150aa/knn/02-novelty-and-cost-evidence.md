# Новая узкая гипотеза: стоимость журнала уже обученных примеров

20.10.2026. **Статус: гипотеза, измерений нет.** Имя опыта `IDENTITY-TRAIN-INDEX-01`. Получатель — настоящая переводческая память `TranslationMachine`; изменение neural LoRA/Student этим опытом не исследуется.

## Доказанная исходником работа внутри обучения

В [`TranslationMachine.cs`, строки 105–122](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/27.TranslationDomain/TranslationMachine.cs#L105-L122) сначала вызвано `trainer.TrainOnExample(example)`, затем для каждой строки пакетного обучения выполнен поиск:

```csharp
if (!_structuredTrainingIdentities.Any(identity =>
    string.Equals(identity.ExampleId, example.ExampleId, StringComparison.Ordinal) &&
    string.Equals(identity.UnitId, example.UnitId, StringComparison.Ordinal)))
    _structuredTrainingIdentities.Add(...);
```

Это работа того же вызова обучения до его успешного возврата, а не посторонний evaluator. Урок меняет exact/phrase/KNN state через [`ExecutorUnit.TrainOnExample`, строки 404–457](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L457); обычный перевод затем читает эту память. Восстановление журнала — [`ImportCheckpoint`, строки 375–381](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/27.TranslationDomain/TranslationMachine.cs#L375-L381).

Если в журнале H записей и в новом пакете B ранее не встречавшихся ключей, исходная ветка проверяет H×B+B×(B−1)/2 записей. Для H=512, B=4 это **2054 проверки предиката**, выведенные из кода. Это не измеренный расход времени или байтов.

**Проверяемая гипотеза:** дополнительный `HashSet<(ExampleId,UnitId)>` уменьшит выделения либо время завершённого перехода обучения, оставляя list для прежнего порядка/checkpoint. Возможен противоположный результат: создание индекса при restore стоит больше, чем экономия на четырёх строках. Поэтому отдельно измеряются restore, Train и их сумма; увеличение выделений/памяти нельзя скрыть в setup.

## Три точных будущих изменения только экспериментальной копии

1. Рядом с полем list (строка 21) добавить приватный `HashSet<(string ExampleId, string UnitId)>`. Строковые элементы стандартного value tuple сравниваются ordinal; сохранить тот же ключ, не вводить SourceId/BatchId.
2. Заменить `.Any(...)` на `if (_structuredTrainingIdentityIndex.Add((example.ExampleId, example.UnitId)))` **на прежнем месте после `trainer.TrainOnExample`**. Не пропускать повторное обучение и не переносить проверку перед mutation: это был бы другой фактор P67.
3. В `ImportCheckpoint` очистить set и заполнить его теми же парами одновременно с прежним заполнением list. Сам list импортировать без новой дедупликации. Не менять контракт экспортируемого checkpoint.

Ожидание сложностной экономии не является доказательством полезного результата. Кандидат становится успешным по конкретной оси только после реального измерения; при равенстве/ухудшении всех осей фиксируется отсутствие полезного выигрыша. После измерения решение о полном переносе принимает Люба.

## Что уже найдено раньше и не присваивается повторно

| Связанный факт | Ранее описан | Отличие этой гипотезы |
|---|---|---|
| Same identity не предотвращает повторную mutation | [16-INDEPENDENT-TRANSLATION…, строки 35–39](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md#L35-L39) | Здесь не исправляется admission/replay; измеряется стоимость поиска журнала после mutation |
| Unit casing и неатомарное пакетное обучение | [23-NEW-UNIT…](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md#L15-L29) | Регистр, валидатор, rollback, результат failed batch не меняются |
| Отрицание/вопросы, direction/topic collisions, verifier reader, phrase composition greedy/offset | Корпус possibleDifs: P54/P55, N30/N33; [дополнение к P54](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-results-and-new-gaps/06-P54-learned-reader-addendum.md) | Повторно проверены как возможные semantic gaps, новой находкой не объявлены |
| E26 — BuildDefault, не новое обучение | [51-GOLD-BRANCH…](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/51-GOLD-BRANCH-EXPERIMENT-CODE-PARITY-AND-E26-DETERMINISTIC-LOOKUP-2026-10-19.md) | Уточнение силы доказательства, не новая гипотеза |

Проведён текстовый поиск по материализованному `possibleDifs` (886 Markdown на данном снимке) и доступным защищённым карточкам/отчётам по `_structuredTrainingIdentities`, `HashSet`, identity/index/ledger, линейному поиску, allocation/cost; затем прочитаны совпадающие адресные finding. Отдельного опыта стоимости индекса именно этого журнала не найдено. Это ограниченное утверждение о проверенном корпусе, не гарантия мировой или полной исторической новизны. Family identity уже известна: кандидат — новое адресное уточнение стоимости, а не «открытие identity».

## Порядок и готовность

Новая карточка и промпт — [identity-train-index-01/CARD.md](identity-train-index-01/CARD.md), [EXECUTOR-PROMPT.md](identity-train-index-01/EXECUTOR-PROMPT.md). Blueprint — текст проекта будущего host; он не реализован и не запускался. Статус `NEEDS_NATIVE_HOST / NOT_RUN`. Кандидат не подменяет очередь сводного `REGISTRY.md` текущего additive пакета и его audit reconciliation. По сверке с сохранёнными результатами №08 `N63-INGRESS-LENGTH` уже завершён, хотя прежний protected index ещё указывает его NEXT. Следующий — №09 `N63-AR-CAP` (host N63R), `PREPARATION_REQUIRED`: host snapshot отсутствует. Не повторять №08 по устаревшей надписи. Устранение блокировок actual learning и конфликты текущих карточек выше этой оптимизации стоимости. Защищённые документы остаются без изменения; дополнение публикуется только в planning.
