# P6: кандидат checkpoint для PromptToTask

## Вердикт

Создан baseline/checkpoint candidate для PromptToTask из двух строк P5L с ролью `train_candidate`. Четыре строки с ролями `eval_candidate` и `holdout_candidate` сохранены отдельно и не участвовали в построении кандидата checkpoint.

Это не semantic quality, не source-disjoint eval и не ablation PASS. Использованный механизм является прозрачным baseline: строки превращены в `PromptToTaskLearnedPattern`, затем сохранены через `PromptToTaskMachine.SaveCheckpointAsync`.

## Использованный путь кода

- `PromptToTaskMachine.SaveCheckpointAsync`
- `PromptToTaskMachine.LoadCheckpointAsync`
- `PromptToTaskMachine.ImportAsync`
- `PromptToTaskMachine.Translate`

## Счетчики

- Строки, использованные для обучения P6: 2
- Зарезервированные eval/holdout строки: 4
- Learned patterns в candidate checkpoint: 2

## Статус checkpoint

Путь candidate checkpoint: `artifacts/smart-skills-semantic-quality-completion/p6-prompt-to-task-training-checkpoint-candidate/prompt-to-task-p6-candidate-checkpoint.json`

Путь candidate manifest: `artifacts/smart-skills-semantic-quality-completion/p6-prompt-to-task-training-checkpoint-candidate/prompt-to-task-p6-candidate-manifest.json`

SHA-256 candidate checkpoint: `5483f58c3e3dcaf4938b651cad8aa7faf9ad027142d000cb4f482a96e454cd36`

Fresh-load/import probe прошел внутри focused P6 test. Это доказывает только загрузку кандидата существующим PromptToTask lifecycle; это не доказывает semantic quality.

## Ограничения

P6 не обучался на eval или holdout строках. P6 не перезаписывал существующую финальную модель в `models/prompt-to-task/prompt-to-task-v1`. P7 все еще должен выполнить source-disjoint оценку и ablation controls до любых заявлений о качестве.
