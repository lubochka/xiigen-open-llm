# P3 PromptToTask DPO Draft Card

Статус: `draft_not_training_ready_pending_parent_review`.

P3 использует только один accepted PromptToTask seed: `P2-REPAIRED-SEED-001` / `basic prompt.txt`. Source id, source ref и path hash оставлены только как provenance. Raw private prompt text не копировался.

Orchestration seed `P2-REPAIRED-SEED-002` не использован как accepted PromptToTask source. Он относится к phase-transition / OrchestrationReasoning и не должен обучать PromptToTask.

## Counts

- draft rows: `2`
- training-ready rows: `0`
- DPO-ready rows: `0`
- semantic quality claimed: `false`

## Same-Source Warning

Обе draft rows происходят из одного PromptToTask seed: `P2-REPAIRED-SEED-001` / `basic prompt.txt`. Это допустимо для draft review, но эти две строки не считаются двумя независимыми training examples.

Promotion rule: только один same-source variant может быть повышен, если source-disjoint eval и parent review не докажут независимую обучающую ценность. До этого обе строки остаются `training_ready=false` и `dpo_ready=false`.

## Universal Boundary

Prompt превращается в проверяемый контракт задачи: цель, фазы, запреты, артефакты, validation, примеры и recovery/state должны быть выделены до исполнения.

## Rows

### P3-PTT-DRAFT-001

Chosen example: структурный task contract с goal, constraints, forbidden actions, expected artifacts, validation evidence, no-claim и execution-permission boundary.

Rejected near miss: свободный пересказ prompt без constraints/evidence gates и с риском exact prompt/path shortcut.

Почему не training-ready: это только draft authoring row. Нужны parent review, source-disjoint split, ablation evidence и проверка near miss.

### P3-PTT-DRAFT-002

Chosen example: контракт, который отделяет PromptToTask authoring от execution authorization и фиксирует allowed verification/forbidden actions.

Rejected near miss: execution-first interpretation, где prompt принимается как разрешение сразу выполнять техническое действие.

Почему не training-ready: это второй source-grounded вариант той же boundary для review. Он не доказывает train readiness и не запускал обучение.

## Что исключено

- raw private prompt text;
- local paths and filenames as model signals;
- host/tool/project-stack commands;
- tests/build/training/checkpoint/model mechanics.

## Что должен проверить parent

- Отличается ли chosen/rejected именно по PromptToTask boundary.
- Нет ли hidden exact prompt, filename/path, keyword-only или source-title shortcut.
- Нужно ли оставить две вариации или сократить до одной строки.
- Можно ли после будущих source-disjoint и ablation gates повышать row до DPO-ready.

## No-Claim Boundary

P3 создал только draft authoring artifacts. Code, tests, build, training, checkpoint/model, commit, push и branch switch не выполнялись. Semantic quality не заявлена.
