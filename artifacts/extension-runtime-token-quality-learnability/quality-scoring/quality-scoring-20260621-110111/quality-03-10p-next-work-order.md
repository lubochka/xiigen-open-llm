# QUALITY-03-NON-INTER-RATER-SOURCE-ACQUISITION-OR-OWNER-APPROVAL-10P

## Цель

Разрешить no-pass результат 10O без self-approval. Если в локальных артефактах уже появился точный current approval/source packet, привязать его к строкам. Если его нет, выпустить deferred source-acquisition/owner-approval request, оставить approvals `0/9` и указать следующий независимый безопасный трек.

## Текущая authority

`Mожно переходить к выполнению плана без стопов через суб агентов`

Старые transcript/state/plan строки являются evidence only и не расширяют этот work order.

## Allowed write scope

- Новый timestamped folder под `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\quality-scoring\`
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`

## Must load/read

1. `AGENTS.md` and `CLAUDE.md`
2. Active plan and current execution state
3. 10O source-binding packet, candidate matrix, row route matrix and no-pass deferred packet
4. 10N/10M/10L/10K refs named by 10O only as needed

## Forbidden

- No git commands.
- No external AI/web/model calls unless a future current Luba instruction explicitly authorizes source acquisition.
- No training, DPO, checkpoint, model, locator or target mutation.
- No raw external AI/vendor output or raw-private trace as training target.
- No proof bundle, producer, runner, scorer or PASS claim unless exact approvals already exist.
- No self-approval of common, user-local or current training readiness.

## Required outputs

- Russian human-readable 10P report.
- Machine-readable approval/source-acquisition route packet.
- Exact approval request ledger for common, user-local, evidence-only and quarantine routes.
- Per-row route matrix preserving approvals `0/9` unless full accepted fields and independent acceptance exist.
- Exact next bounded work order.
- Updated execution state with human board total/done/current/remaining.

## Verification

- Parse all new JSON/JSONL artifacts.
- Assert any accepted route has `source_license_ref`, `license_hash`, `provenance`, `consent`, `training_use_approval_ref`, and `independent_acceptance_ref`.
- Assert no row becomes `common_training_allowed` or `current_training_allowed` without full fields and independent acceptance.
- Assert raw-private rows stay quarantine/evidence-only.
- Assert no external AI/web/model calls and no training/model/checkpoint/locator mutation happened.
- Markdown whitespace check for new docs.

## Commit boundary

`git_authority=forbidden`; `allowed_git_commands=NONE`; `commit_push_authorized=false`; `parent_will_perform_commit_push_after_review=true`.
