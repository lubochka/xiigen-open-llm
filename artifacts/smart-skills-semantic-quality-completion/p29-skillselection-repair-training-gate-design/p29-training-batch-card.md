# P29 training batch card

P29 batch card описывает только будущий gate. Он не превращает P27 rows в training set и не разрешает P30.

## Статус batch

- Источник: p27-repair-dataset-candidate.jsonl.
- Строк: 8.
- Тип: local-private candidate-only.
- commonTrainingAllowed=false.
- trainingAuthorizedNow=false.
- evalAuthorizedNow=false.
- checkpointMutationAuthorizedNow=false.
- Common reuse: запрещен без отдельного решения Luba, redaction review и source/license review.

## Достаточность

8 строк достаточно для gate/design package. 8 строк не достаточно для настоящего candidate-only training/eval slice, если от него ждут качества или generalized skill selection. Причины: мало source families, мало label coverage, по одной family на skill, и слишком сильный риск shortcut по adapter/order/source lineage.

## Что должно появиться перед P30

| Gate | Что требуется |
|---|---|
| Fresh approval | Точная свежая цитата Luba после P29: принять пакет и разрешить P30 candidate-only training/eval сейчас. |
| P30 work order | Отдельный bounded work order с allowed write scope, writer id, checkpoint path и max time. |
| State flags | training_allowed=true, eval_allowed=true только для P30; commonTrainingAllowed=false. |
| Split acceptance | Parent review принимает source-disjoint split и batch card. |
| Leakage gates | Exact prompt, source id, path, filename, pair id, row ordinal, candidate order, selected-skill position, target adapter shortcut. |
| Lifecycle gates | fresh-load, continue-training from existing checkpoint, export/import, ablation, dashboard/runtime evidence. |
| No promotion | Никакой promotion, common model replacement или semantic-quality claim в P30. |

## Нельзя заявлять

P29 не обучает, не запускает eval, не мутирует checkpoint, не доказывает semantic quality, не делает SkillSelection ready и не заменяет common model.
