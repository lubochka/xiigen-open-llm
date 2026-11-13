# P2 Material Quality Classifier Report

## Текущий статус

Post-commit parent review нашел один дефект P2 перед P3: в `seed-candidate-table.jsonl` у двух seed rows не было заполненного `universal_boundary`. Repair заполнил `universal_boundary` для обеих seed rows из accepted candidate boundaries и добавил `path_hash` из candidate rows.

P3 не начинался. Semantic quality не заявлена. Code, tests, build, training, DPO, checkpoint/model, commit, push и branch switch не выполнялись.

## Counts

- candidate rows: `2951`
- `gold_candidate`: `1`
- `silver_candidate`: `1`
- `evidence_only`: `2766`
- `reject_or_quarantine`: `183`
- seed rows: `2`
- decomposition rows: `14`
- accepted rows: `2`

## Seed Boundary Repair

- `P2-REPAIRED-SEED-001`: Prompt превращается в проверяемый контракт задачи: цель, фазы, запреты, артефакты, validation, примеры и recovery/state должны быть выделены до исполнения.
- `P2-REPAIRED-SEED-002`: Phase-boundary orchestration: review/fix/commit/next-phase нельзя схлопывать; следующая фаза стартует только после принятого evidence и явной границы.

Verification target: `seed_universal_boundary_null_count=0`, `seed_schema_complete=true`, forbidden true flags `0`.

## Accepted Candidate Support

1. `basic prompt.txt` as `gold_candidate`: В содержании basic prompt вручную проверены требования no-code explanation first, phase split, plan validation, positive/negative examples и state/recovery; локальные stack-примеры исключены.
2. `PATCH-phase-transition-protocol-SKILL.md` as `silver_candidate`: В содержании phase-transition protocol вручную проверены gate-before-stop, state/docs update, next-phase plan-before-execution и explicit approval boundary; WordPress/plugin mechanics исключены.

## Parent Repair Context

Previous P2 over-accepted `1663` rows by filename/path/family. Repair demoted `1661` rows and kept only content-supported accepted candidates. Demoted examples: `docker-compose.md`, `phase.test.cjs`, `.clinerules`, `.cursorrules`, `PROMPTS.md`, `XIIGEN_NESTJS_REACT_TRANSLATION_PLAN.md`.

## No-Claim Boundary

All seed rows remain `training_ready=false`, `dpo_ready=false`, `raw_private_content_copied=false`, and `semantic_quality_claimed=false`. P3 is next only after commit/push of the P2 seed-boundary repair artifacts.
