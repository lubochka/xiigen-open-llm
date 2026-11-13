# P5B revised leakage and ablation plan

Статус: `draft_only_pending_parent_review`.

Ablation сейчас не исполняется. Причина простая: нет training-ready rows, нет принятого revised split, нет authorized model artifact, и training/eval прямо запрещены.

## Базовые controls из P5

| Control | Машина | Что должен сломать |
|---|---|---|
| exact prompt lookup | PromptToTask | Запоминание точной prompt-формулировки вместо извлечения task contract. |
| filename/path lookup | PromptToTask, OrchestrationReasoning | Решение по local path или filename. |
| keyword-only classifier | PromptToTask | Поверхностные keywords вместо permission, validation and no-claim boundaries. |
| source-title shortcut | PromptToTask, OrchestrationReasoning | Решение по title/provenance. |
| WordPress/plugin title shortcut | OrchestrationReasoning | Узнавание plugin provenance вместо phase-boundary reasoning. |
| static phase keyword shortcut | OrchestrationReasoning | Реакция на phase/review/commit words без проверки порядка gates. |
| exact protocol text lookup | OrchestrationReasoning | Memorization of exact protocol wording. |

## Новые controls из P5A

| Control | Машина | Что должен сломать |
|---|---|---|
| named-reference shortcut | PromptToTask, OrchestrationReasoning | Принятие source только потому, что он named by Luba/current plan. |
| analysis-state-status shortcut | PromptToTask, OrchestrationReasoning | Использование `done` или `todo` как semantic answer вместо lifecycle gate. |
| source_state shortcut | PromptToTask, OrchestrationReasoning | Использование поля `source_state` как скрытого label. |
| source path/title shortcut | PromptToTask, OrchestrationReasoning | Path segments, document titles, branded labels and directory names as answer. |
| code filename/class/method shortcut | OrchestrationReasoning | `.cs`, class names, method names, namespaces, node ids and implementation symbols. |
| test-framework shortcut | OrchestrationReasoning | `xUnit`, `Assert`, test method names, run ids and temp paths. |

## Как читать этот план

Controls пока являются обязательным будущим проверочным contract, а не результатом. Parent review должен убедиться, что каждый candidate split row имеет хотя бы один релевантный shortcut control. Если row может пройти только по named reference, path/title, code identifier or test framework marker, row остается evidence-only или переписывается до DPO promotion.

## Условия будущего запуска

Будущий ablation/eval можно запускать только если:

- parent принял revised split как input to DPO promotion review;
- parent отдельно повысил candidate rows до DPO-ready;
- named-reference/todo rows не выданы за training-ready без явного acceptance;
- train/eval families source-disjoint and not near-duplicates;
- later work order явно разрешил training/eval.

До этого `training_allowed=false`, `eval_allowed=false`, `semantic_quality_claimed=false`.
