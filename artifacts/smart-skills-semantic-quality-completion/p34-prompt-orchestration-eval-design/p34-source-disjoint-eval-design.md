# P34 source-disjoint eval design - PromptToTask and OrchestrationReasoning

Вердикт: P34 вернул план к двум главным слабым машинам: PromptToTask и OrchestrationReasoning. Это design-only slice: eval не запускался, training не запускался, checkpoint не менялся.

## Что проектируется

P34 описывает, как будущий source-disjoint eval должен проверять две вещи:

- PromptToTask: пользовательский prompt превращается в универсальную задачу, без привязки к конкретному стеку или файлам проекта.
- OrchestrationReasoning: структурированная задача превращается в универсальный orchestration plan: ветки, gates, review, repair, evidence, но без C#/.NET/WPF/Cursor/Claude mechanics как обучающего сигнала.

## Какие source families используются

- Для train-candidate: prompt examples, authority rules, universal orchestrator plan.
- Для dev-candidate: Claude-rule boundary и universal-vs-project-specific inventory.
- Для eval-holdout-candidate: universal nine-node flow concept и universal dependency concept.

## Почему это важно

Если eval использует те же source families, что train, модель может узнать источник, путь, заголовок или node id вместо смысла задачи. P34 поэтому заранее разделяет семьи и требует ablations: path-only, quoted-approval-only, node-id-only, stack-mechanics-only.

## Что еще не закрыто

P34 не делает executable eval. Следующая безопасная фаза должна подготовить fixture rows с ожидаемыми task slots и orchestration branches, либо checkpoint rewrite manifest design. Training/eval/checkpoint mutation все еще запрещены.
