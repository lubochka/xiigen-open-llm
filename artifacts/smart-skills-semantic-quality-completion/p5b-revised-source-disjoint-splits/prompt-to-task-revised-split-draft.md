# P5B PromptToTask revised source-disjoint split draft

Статус: `draft_only_pending_parent_review`.

Вердикт по candidate families: `READY_FOR_DPO_PROMOTION_REVIEW`.

Вердикт по реальной готовности строк: `INSUFFICIENT_TRAINING_READY_ROWS`.

Это не training split и не eval split. P5B только показывает, что после P5A у `PromptToTask` есть достаточно разных candidate families, чтобы parent мог провести отдельный DPO promotion review. Все строки остаются `training_ready=false`, поэтому `training_allowed=false`, `eval_allowed=false`, `semantic_quality_claimed=false`.

## Что рассмотрено

| Family | Источник | Source state | Training ready | DPO status | Почему независима или ограничена |
|---|---|---|---:|---|---|
| `sf-prompt-to-task-basic-prompt-77A175A9` | `basic prompt.txt` | accepted draft source | false | draft not training-ready | Исходная P3 family. Две строки внутри нее являются variants одного source, поэтому максимум один representative row может идти на promotion review. |
| `sf-p5a-prompt-to-task-authority-rules-54DF62A3` | `AGENTS.md` | done | false | accepted candidate | Отдельный rules-документ, не derived от `basic prompt.txt`; учит извлечению task contract из правил и ограничений. |
| `sf-p5a-skills-inventory-universal-adapter-905F88B4` | `skills-inventory-universal-vs-project-specific.md` | named reference, analysis-state todo | false | accepted candidate for split draft only | Отдельная family про границу universal core и project adapter, но пока это named-reference/todo evidence, не training row. |

## Черновой split

Train candidate families для будущего review:

- `basic prompt.txt` как один representative seed, не две независимые строки;
- `AGENTS.md` как независимый done-source candidate после parent promotion review.

Eval candidate family для будущего review:

- `skills-inventory-universal-vs-project-specific.md` как held-out candidate только после named-reference acceptance.

Excluded or blocked:

- `P3-PTT-DRAFT-002` не добавляет новую source family, пока parent не докажет отдельную teaching value.

## Почему нельзя тренировать

В P5B нет ни одной training-ready строки. P5A дал candidate material, но не DPO promotion. Особенно важно: source со state `named_reference` и observed `todo` не может стать training-ready только потому, что он полезен для split draft.

## Что должен проверить parent

Parent review должен решить:

- можно ли принять `AGENTS.md` как candidate для train side после redaction/source acceptance;
- можно ли использовать skills inventory как held-out named-reference candidate или сначала нужен analyzed/done source state;
- какой из двух P3 same-source variants оставить representative row;
- покрывает ли ablation plan exact prompt, filename/path, keyword-only, source-title, named-reference, analysis-state-status and source-state shortcuts.
