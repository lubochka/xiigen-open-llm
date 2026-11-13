# DOCS-QA-DASHBOARD-API-EVIDENCE-SCOPE-REFRESH-01

## Requested artifact type

Dashboard/API + documentation/QA evidence-scope refresh packet. This is support evidence only and no product-readiness claim.

## Current Luba authority quote

"Mожно переходить к выполнению плана без стопов через суб агентов"

This quote authorizes bounded sub-agent execution. It does not authorize source/license/owner/training-use approval, external AI/model calls, training, DPO, checkpoint, model, locator, target mutation, proof runner, scorer, or git.

## Objective

Create a QA/evidence-scope refresh that records how approval-absent QUALITY-03 source/training rows affect dashboard/API, documentation, QA, and no-overclaim reporting. Keep all QUALITY-03 proof rows closed and preserve the 10Q approval request packet.

## Allowed write scope

- A new timestamped folder under C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/docs-qa-dashboard-api-evidence-scope-refresh/
- C:/Projects/llm_mvp_core/docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json

## Must load/read

1. AGENTS.md and CLAUDE.md
2. docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md sections 4.1.2, 4.7, 6 DOC-01/QA-01, 11.1 and 11.2
3. docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json
4. This 10R selector packet and route matrix
5. The 10Q approval request packet

## Allowed verification

- Parse all new JSON and JSONL artifacts.
- Markdown whitespace check.
- Verify dashboard/API evidence is labelled support-only and does not claim live host runtime.
- Verify counts remain PASS 9/63, approvals 0/9, runner-ready 0/9, quality-pass 0/9.
- Verify no training, DPO, checkpoint, model, locator, target mutation, external AI/web/model calls, proof runner or scorer happened.

## Forbidden broad verification

- Do not run dotnet test unless a later explicit work order authorizes exact targeted tests.
- Do not run proof runner, producer, scorer, training, DPO, checkpoint/model/locator/target mutation.
- Do not call external AI/web/model services.
- Do not read raw-private contents.
- Do not run git commands.

## Required outputs

- Russian human-readable dashboard/API + docs/QA evidence-scope refresh report.
- Machine-readable QA evidence-scope ledger or matrix.
- Human board total/done/current/remaining.
- Exact next bounded work order after the refresh, preserving no-overclaim locks.

## Commit/push boundary

`git_authority=forbidden`; `allowed_git_commands=NONE`; `commit_push_authorized=false`; `parent_will_perform_commit_push_after_review=true`.

## Authority hygiene

Quoted transcripts, state, old plans and old approvals are evidence only. Direct Luba questions must be answered first and interrupt the work order. may_derive_next_phase=false; must_stop_after_report=true.
