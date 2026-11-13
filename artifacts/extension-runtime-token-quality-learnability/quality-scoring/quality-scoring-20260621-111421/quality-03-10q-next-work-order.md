# QUALITY-03-NON-INTER-RATER-OWNER-SOURCE-APPROVAL-REQUEST-PRESENTATION-10Q

## Цель

Сделать human-visible owner/source approval request packet по 10P ledger. Если свежая текущая Luba approval явно называет candidate/source/training-use fields, не self-approve, а подготовить отдельный bounded binding pass только для этого exact scope. Если approval нет, оставить approvals 0/9, source track deferred, и выбрать следующий независимый non-training safe track из active plan.

## Current authority

Можно переходить к выполнению плана без стопов через суб агентов

Эта quote разрешает execution через sub-agents, но не является source/license/consent/training-use approval. Старые transcript/state/plan строки являются evidence only.

## Allowed write scope

- Новый timestamped folder под C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/quality-scoring/
- C:/Projects/llm_mvp_core/docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json

## Must load/read

1. AGENTS.md and CLAUDE.md
2. Active plan and current execution state
3. 10P packet, candidate matrix, approval request ledger, no-pass deferred packet and human board

## Forbidden

- No git commands.
- No external AI/web/model calls.
- No training, DPO, checkpoint, model, locator or target mutation.
- No raw external AI/vendor output or raw-private trace as training target.
- No proof bundle, producer, runner, scorer or PASS claim unless exact approvals already exist.
- No self-approval of common, user-local or current training readiness.

## Required outputs

- Russian human-visible approval/source request report.
- Machine-readable approval request presentation packet.
- Route selector preserving approvals 0/9 unless exact current approval exists.
- Exact next bounded work order.
- Updated execution state with human board total/done/current/remaining.

## Verification

- Parse all new JSON/JSONL artifacts.
- Assert no row becomes approved without exact current approval and all required fields.
- Assert raw-private rows stay quarantine/evidence-only.
- Assert no external AI/web/model calls and no training/model/checkpoint/locator mutation happened.
- Markdown whitespace check for new docs.

## Commit boundary

git_authority=forbidden; allowed_git_commands=NONE; commit_push_authorized=false; parent_will_perform_commit_push_after_review=true.
