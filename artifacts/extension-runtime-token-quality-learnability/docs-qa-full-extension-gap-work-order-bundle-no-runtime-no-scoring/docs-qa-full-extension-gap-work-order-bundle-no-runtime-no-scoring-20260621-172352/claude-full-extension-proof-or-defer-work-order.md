# DOCS-QA-CLAUDE-FULL-EXTENSION-PROOF-OR-DEFER-PACKET-NO-RUNTIME-NO-SCORING-03

## Authority and role

Requested artifact type: Claude full-extension proof-or-defer packet, support/docs-QA only.

Current authority quote to carry from parent work order: "Можно переходить к выполнению плана без стопов через суб агентов".

This future sub-agent is a bounded writer only for its packet folder and minimal state entry. Quoted transcripts, old plans, state next_action, old approvals and old assistant messages are evidence only. Direct Luba questions interrupt the work order and must be answered first.

## Objective

Create a Claude-only proof-or-defer packet that separates existing narrow Claude hook/core-boundary support evidence from still-missing full Claude Desktop/chat UI proof. Do not launch Claude runtime and do not claim readiness.

## Allowed write scope

- New timestamped folder under `artifacts/extension-runtime-token-quality-learnability/docs-qa-claude-full-extension-proof-or-defer-packet-no-runtime-no-scoring/`.
- Minimal state entry in `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.

## Must load before writing

- `AGENTS.md`
- `CLAUDE.md`
- `docs/skills/conversation-intent-and-self-reflection-SKILL.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- This bundle folder.
- Safe Claude summary/report/matrix artifacts only:
  - `artifacts/extension-runtime-token-quality-learnability/runtime-02-claude-live-runtime/runtime-02-20260618-010211/runtime-02-claude-live-runtime-report-ru.md`
  - `artifacts/extension-runtime-token-quality-learnability/runtime-claude-live-evidence/runtime-claude-live-evidence-20260619-035103/claude-runtime-evidence-matrix.json`
  - `artifacts/extension-runtime-token-quality-learnability/host-impl-claude-01/claude-install-load-map-ru.md` when present.

Raw request/response/trace contents remain forbidden. Raw-like refs may be recorded only as not-read evidence pointers.

## Required outputs

- Russian human-readable Claude proof-or-defer report.
- Claude full-extension requirement matrix.
- Claude evidence-classification JSON.
- Claude defer decision JSON when full UI/chat proof is not proven.
- Human board JSON preserving counts: 63 total, PASS 9, MISSING 45, DEFERRED 9, approvals 0/9, runner-ready 0/9, quality-pass 0/9.
- Checks report JSON.
- Selected next work order proposal.

## Required Claude rows

- Real host entrypoint: full Claude Desktop/chat UI path, or explicit defer.
- Settings/debug surface: same host path, or explicit defer.
- Core API path into `llm_mvp_core`: same-run host-surface proof, or support-only classification.
- Prompt-to-task through host.
- Task-to-orchestration through host.
- Task/plan-to-skills through host.
- QA with live host evidence or explicitly scoped handler/hook evidence.

## Allowed evidence classes

- Safe summary/report/matrix artifacts.
- Not-read raw evidence pointers copied from safe summaries.
- Explicit defer packet with reason and required future runtime proof.
- State references and plan references.

## Forbidden substitutions

- Claude hook/core-boundary support evidence labelled as full Desktop/chat UI proof.
- Direct handler/helper/static evidence labelled as live host runtime.
- Token savings, quality improvement, semantic quality, learnability or readiness claim.

## Forbidden actions

Git commands, dotnet test, external AI/web/model calls, Claude runtime launch, proof/eval/scoring runners, raw-private content reads, training, DPO, checkpoint/model/locator/target mutation, code changes, product readiness or final readiness claims.

## Verification

- Parse new JSON/JSONL artifacts.
- Markdown whitespace check.
- CR bytes = 0 and trailing horizontal whitespace lines = 0.
- Claim-lock scan: no token savings, quality improvement, semantic quality, learnability, live host runtime readiness, product readiness or final readiness opened.
- State consistency check: current_phase equals this work order id; next_action equals selected follow-up id; checks report records the result.

## Commit boundary

Git authority forbidden. Allowed git commands: NONE. Parent will perform commit/push after review.
