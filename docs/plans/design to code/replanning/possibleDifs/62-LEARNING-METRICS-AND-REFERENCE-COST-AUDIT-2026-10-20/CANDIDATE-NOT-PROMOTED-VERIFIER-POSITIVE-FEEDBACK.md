# Условный кандидат: положительная поправка обученного verifier

**Статус: SOURCE_CONFIRMED_LOCAL_NOOP / CALLER_NOT_ESTABLISHED / NOT_ACTIVE_EXPERIMENT.** Это не доказанная причина ухудшения RP01. Новый learning run не предлагается до установления настоящего producer/admission path. Никакая protected карточка не менялась.

## Прочитанный код

1. [QualityVerifierUnit.TrainOnExample, L1095–1146](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1095-L1146): десериализует input/chosen, но state writes выполняются только внутри `if (!chosen.Pass && chosen.Issues is not null)`. Положительный `chosen.Pass=true` сам по себе не записывает правило, исключение или отзыв ранее выученного требования.
2. [TranslationMachine.TrainStructuredBatchAsync, L107–121](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationMachine.cs#L107-L121): после void TrainOnExample unit и identity добавляются в учёт. [L133–142](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationMachine.cs#L133-L142) возвращает RowsTrained/PromotedUnitIds независимо от того, менял ли конкретный тренер state.
3. [QualityVerifierUnit.Verify, L1052–1092](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1052-L1092): ранее выученные requiredTerms/markers/conceptRules продолжают применяться к кандидатам. Следовательно, при условии допуска такого positive correction существующий unit не сможет выучить её через эту ветку.
4. Но [BuildGeneralBatch, L128–141](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L128-L141) строит input на **ошибочном rejectedCandidate** и правильно задаёт chosen.Pass=false/rejected.Pass=true. Это не positive correction producer. Переворачивать chosen/rejected было бы ошибкой.

## Почему не активная карточка

Существование публичного TrainStructuredBatchAsync и возможность вручную передать произвольный JSON не доказывают, что именно такой урок поступает в текущий продуктовый процесс. Найденный builder генерирует обратную polarity. Связанный feedback writer, который превращает человеческое подтверждение lawful answer в admitted chosen.Pass=true verifier lesson, не установлен.

Поэтому прямой запуск hand-authored positive JSON создал бы новый сценарий; он не установил бы утраченную деталь уже существовавшего RP01 процесса. Такой сценарий можно исследовать позже при явном выборе нового функционального требования, но не подменять им нынешнюю задачу пользователя.

## Prior art и узкая граница

- N23 уже описывает накопление/конфликт verifier требований и scope. Его нельзя повторно выдать за новую находку.
- N27/N28 уже описывают negative-only heldout и различие unit metric/live quality.
- N59-L02/N60 уже исследуют возможный цикл phrase override → ошибочный positive feedback и прямо оставляют writer неустановленным.
- Здесь проверен более узкий локальный факт о **невозможности положительным уроком отозвать старый veto** и о неспецифичности RowsTrained. Полная новизна семантического сценария не заявляется до установления caller и повторного сопоставления с соответствующими protected P56/P58 owners.

## Что должно появиться, чтобы вопрос стал проверяемым

Нужен source reference на реальный producer → admission → unit-specific serializer → TrainStructuredBatchAsync, и минимум один уже допустимый serialized positive correction. Затем надо проверить, что пользовательская поправка действительно supersedes конкретное устаревшее правило, а не разрешает любые плохие ответы.

Только после этого имеет смысл небольшой родной C# A/B: обе руки получают одни и те же неизменённые admitted lessons, A сохраняет текущее накопление, B реализует provenance/version-scoped correction/exception, без глобального отключения verifier. Снять before learning, после начального отрицательного урока, после подтверждённой положительной поправки; frozen panel содержит исправляемый класс, ранее правильные другие классы и плохие кандидаты. Метрики: false rejection, false acceptance, corrections learned, useful acceptance/latency и downstream result после настоящего consumer. Любое полезное улучшение — успех по своей метрике/домену, с ухудшениями рядом. Это условное описание, **не готовая к запуску карточка**.

## Результат проверки

Подтверждён локальный no-op в current source. Не подтверждены существующий positive producer, поступление такого урока в RP01, величина вреда, причинность или предлагаемая реализация исправления. Эксперимент не запускался.

