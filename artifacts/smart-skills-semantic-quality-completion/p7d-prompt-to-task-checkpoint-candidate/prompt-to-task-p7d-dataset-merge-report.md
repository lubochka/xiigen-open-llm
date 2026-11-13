# P7D: кандидат checkpoint для PromptToTask

## Вердикт
Создан кандидат checkpoint P7D для родительской проверки. Он объединяет два уже существующих P6 train-паттерна и три parent-approved P7C train-only строки. Оценочные и holdout строки P7 не использовались.

## Что изменилось по сравнению с P6
P6 содержал 2 learned pattern. P7D содержит 5 learned pattern: 2 из P6 и 3 новых универсальных PromptToTask-паттерна из P7C. Новые паттерны описывают не проектный стек, а преобразование просьбы в контракт задачи: цель, входы, ограничения, артефакты, запреты и проверку.

## Что исключено из модельной власти
Поля происхождения строки сохранены только для аудита: row id, source family, safe ref и hash/pointer. Они не входят в learnedSignals и не должны служить основанием выбора ответа. Также не копировался сырой приватный prompt, не использовались имена файлов, пути, классы, тесты или точные фразы источника.

## Ограничения
Это только кандидат checkpoint под P7D. Он не доказывает P7 pass, semantic quality, API readiness или готовность финальной модели. Следующий честный шаг — родительская проверка, затем отдельный P7 rerun на source-disjoint eval/holdout и ablation-контролях.

## Использованные строки
- P6 train-паттерны: 2.
- P7C parent-approved train-only строки: 3.
- P7 eval/holdout строк в обучении: 0.

## Артефакты
- checkpoint: `artifacts/smart-skills-semantic-quality-completion/p7d-prompt-to-task-checkpoint-candidate/prompt-to-task-p7d-candidate-checkpoint.json`
- manifest: `artifacts/smart-skills-semantic-quality-completion/p7d-prompt-to-task-checkpoint-candidate/prompt-to-task-p7d-candidate-manifest.json`
- source manifest: `artifacts/smart-skills-semantic-quality-completion/p7d-prompt-to-task-checkpoint-candidate/prompt-to-task-p7d-source-manifest.json`
- lifecycle export: `artifacts/smart-skills-semantic-quality-completion/p7d-prompt-to-task-checkpoint-candidate/prompt-to-task-p7d-lifecycle-export.json`
