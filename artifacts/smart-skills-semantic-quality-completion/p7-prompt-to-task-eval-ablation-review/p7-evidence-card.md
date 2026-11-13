# P7 evidence card

## Итог

`P7_FAIL_BASELINE_NO_RESERVED_GENERALIZATION`. P6 baseline checkpoint candidate был загружен через `PromptToTaskMachine.LoadCheckpointAsync` и проверен через `PromptToTaskMachine.Translate`.

## Что проверено

- 4 зарезервированные строки: 2 eval и 2 holdout.
- Training split не использовался как eval-вход.
- Отрицательные контроли проверяли точный запрос, имя файла или путь, ключевые слова, source family и nearest-only поведение.
- Положительных reserved ответов: 0.
- Проходов по коротким маркерам: 0.

## Граница утверждения

Это runtime evidence для родительского review. Семантическое качество не заявлено, обучение не запускалось, чекпойнт и финальная модель не переписывались.
