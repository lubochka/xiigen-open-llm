# P7D evidence card

## Результат
Создан кандидат checkpoint PromptToTask для родительской проверки. Он расширяет P6 baseline с 2 до 5 learned pattern за счет трех P7C parent-approved train-only строк.

## Границы безопасности
Не использованы P7 eval/holdout строки, сырой приватный prompt, пути, имена файлов, source family, row id или hash как модельная власть. Эти поля присутствуют только в отчетах происхождения.

## Что еще не доказано
P7D не заявляет semantic quality и не заявляет P7 pass. Кандидат должен пройти родительскую проверку и затем отдельный P7 rerun на зарезервированных строках и ablation-контролях.
