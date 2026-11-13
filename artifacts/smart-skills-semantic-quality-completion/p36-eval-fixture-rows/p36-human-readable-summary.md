# P36 - eval fixture rows

P36 создал проверочные строки для будущего runner. Это не запуск оценки и не доказательство качества модели.

## Что создано

- PromptToTask: 12 строк. Они проверяют, что запрос пользователя превращается в универсальную задачу.
- OrchestrationReasoning: 12 строк. Они проверяют ветки, review, repair, lifecycle и границы доказательств.
- SkillSelection: 12 строк. Они проверяют, что скилы выбираются по смыслу задачи, а не по порядку кандидатов.
- Dashboard/API: 36 сценариев, по одному на каждую fixture строку.

## Примеры

1. Если запрос требует ответа до работы, PromptToTask должен выделить authority question.
2. Если нужен план, OrchestrationReasoning должен сохранить ветки и review.
3. Если нужен skill injection, SkillSelection должен идти по смыслу, а не по индексу.

## Что это позволит проверить позже

Будущий runner сможет загрузить эти строки, запустить source-disjoint eval, проверить negative controls и показать результат в dashboard/API.

## Чего это не доказывает

- Модель не обучалась.
- Eval не запускался.
- Checkpoint не менялся.
- Fresh-load и continue-training не проверялись.
- Host runtime Claude/Cursor/Codex не проверялся.
- Качество модели не заявлено.
