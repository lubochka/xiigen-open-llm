# P5B OrchestrationReasoning revised source-disjoint split draft

Статус: `draft_only_pending_parent_review`.

Вердикт по candidate families: `READY_FOR_DPO_PROMOTION_REVIEW`.

Вердикт по реальной готовности строк: `INSUFFICIENT_TRAINING_READY_ROWS`.

У `OrchestrationReasoning` теперь достаточно независимых candidate families для parent DPO promotion review, но training-ready rows все еще нет. Это нужно сказать прямо: `OrchestrationReasoning` lacks enough training-ready rows, потому что пять новых families являются `named_reference` с observed `todo`, а исходная P4 family остается draft-only.

## Что рассмотрено

| Family | Source state | Training ready | DPO status | Почему независима или ограничена |
|---|---|---:|---|---|
| `sf-orchestration-phase-transition-8CD45F23` | accepted draft source | false | draft not training-ready | Исходная P4 family; две строки являются same-source variants, поэтому нужен один representative row. |
| `sf-p5a-skills-inventory-universal-adapter-905F88B4` | named reference, analysis-state todo | false | accepted candidate for split draft only | Отдельная universal-core-vs-adapter family; не phase-transition protocol. |
| `sf-p5a-orchestration-doc07956-universal-flow-FE25B535` | named reference, analysis-state todo | false | accepted candidate for split draft only | Отдельный universal orchestrator package про branches, review before formatting and targeted repair. |
| `sf-p5a-orchestration-universal-dependencies-7DB3DAB0` | named reference, analysis-state todo | false | accepted candidate for split draft only | Отдельная dependency/relation family; duplicate cache copy не считается второй family. |
| `sf-p5a-orchestration-nine-node-flow-16C38AC6` | named reference, analysis-state todo | false | accepted candidate for split draft only | Отдельная station-chain family; высок риск exact-nine/node/class shortcut. |
| `sf-p5a-orchestration-flow-isolation-F85D7BAA` | named reference, analysis-state todo | false | accepted candidate for split draft only | Отдельная isolation/dedup/no-cross-contamination family; высок риск test-name/framework shortcut. |

## Черновой split

Train candidate families для будущего review:

- phase-transition family как один representative seed;
- doc07956 universal-flow family после named-reference acceptance;
- universal-dependencies family после named-reference acceptance.

Eval candidate families для будущего review:

- skills inventory для universal-core-vs-adapter reasoning;
- nine-node flow для station-chain reasoning;
- flow-isolation tests для no-cross-contamination evidence.

Excluded or blocked:

- `P4-ORCH-DPO-DRAFT-002` не добавляет новую source family;
- все `named_reference` families blocked from training-ready promotion until parent acceptance.

## Почему нельзя тренировать

Все rows сейчас `training_ready=false`. P5B не выполнял DPO promotion review, source acceptance, ablation, eval, training or checkpoint work. Черновой split пригоден только как input для parent review: parent может сказать, что candidate coverage достаточно для следующего review round, но не может из этого вывести real training/eval permission.

## Что должен проверить parent

Parent review должен проверить:

- какие named-reference candidates можно принимать только как split-draft evidence, а какие могут перейти в DPO promotion review;
- не перекрываются ли doc07956 and nine-node flow как near-duplicate station-chain families;
- не является ли universal-dependencies row слишком code-source-heavy для common orchestration;
- не является ли flow-isolation row test-framework shortcut;
- покрывает ли revised ablation plan path/title, code filename/class/method, test framework, analysis-state-status and source-state shortcuts.
