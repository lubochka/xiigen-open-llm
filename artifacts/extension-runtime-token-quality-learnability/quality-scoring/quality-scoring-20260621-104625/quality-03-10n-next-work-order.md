# QUALITY-03-NON-INTER-RATER-CANDIDATE-ACCEPTANCE-SOURCE-BINDING-10O

## Цель

Разрешить acceptance request из 10N без self-approval. Если для кандидата уже есть точные локальные `source_license_ref`, `license_hash`, `provenance`, `consent`, `training_use_approval_ref` и `independent_acceptance_ref`, выпустить accepted source-binding packet. Если их нет, выпустить no-pass source-acquisition или owner-approval request и оставить approvals `0/9`.

## Текущая authority

`Mожно переходить к выполнению плана без стопов через суб агентов`

Старые transcript/state/plan строки являются evidence only и не расширяют этот work order.

## Allowed write scope

- Новый timestamped folder под `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\quality-scoring\`
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`

## Must load/read

1. `AGENTS.md` and `CLAUDE.md`
2. `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
3. `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
4. `artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260621-104625/quality-03-10n-review-packet.json`
5. `artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260621-104625/quality-03-10n-reviewed-candidate-decision-matrix.jsonl`
6. `artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260621-104625/quality-03-10n-row-route-matrix.jsonl`
7. `artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260621-104625/quality-03-10n-acceptance-request-packet.json`
8. 10M/10L/10K refs named by 10N, only as needed for provenance.

## Forbidden

- No git commands.
- No external AI/web/model calls unless a future current Luba instruction explicitly authorizes source acquisition.
- No training, DPO, checkpoint, model, locator, target mutation.
- No raw external AI/vendor output or raw-private trace as training target.
- No proof bundle, producer, runner, scorer, or PASS claim unless exact approvals already exist.
- No self-approval of common, user-local, or current training readiness.

## Required outputs

- Russian human-readable 10O source-binding/acceptance report.
- Machine-readable 10O source-binding review packet.
- Resolved candidate acceptance matrix.
- Per-row route matrix with `common_training_allowed=false` and `current_training_allowed=false` unless full fields and independent acceptance exist.
- Accepted source packet only if all required fields exist; otherwise no-pass source-acquisition/owner-approval packet.
- Exact next bounded work order.
- Updated execution state with human board total/done/current/remaining.

## Verification

- Parse all new JSON/JSONL artifacts.
- Assert accepted candidates have `source_license_ref`, `license_hash`, `provenance`, `consent`, `training_use_approval_ref`, and `independent_acceptance_ref`.
- Assert no row becomes `common_training_allowed` or `current_training_allowed` without full fields and independent acceptance.
- Assert raw-private rows stay quarantine/evidence-only.
- Assert no external AI/web/model calls and no training/model/checkpoint/locator mutation happened.
- Markdown whitespace check for new docs.

## Commit boundary

`git_authority=forbidden`; `allowed_git_commands=NONE`; `commit_push_authorized=false`; `parent_will_perform_commit_push_after_review=true`.
