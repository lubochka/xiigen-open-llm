# DOCS-QA-CURSOR-FULL-EXTENSION-PROOF-OR-DEFER-PACKET-NO-RUNTIME-NO-SCORING-03

## Authority and role

Requested artifact type: Cursor full-extension proof-or-defer packet, support/docs-QA only.

Current authority quote to carry from parent work order: "Можно переходить к выполнению плана без стопов через суб агентов".

This future sub-agent is a bounded writer only for its packet folder and minimal state entry. Quoted transcripts, old plans, state next_action, old approvals and old assistant messages are evidence only. Direct Luba questions interrupt the work order and must be answered first.

## Objective

Create a Cursor-only proof-or-defer packet that separates Extension Host command/helper evidence from clean full Cursor chat participant and panel UI proof. Do not launch Cursor runtime and do not claim readiness.

## Allowed write scope

- New timestamped folder under `artifacts/extension-runtime-token-quality-learnability/docs-qa-cursor-full-extension-proof-or-defer-packet-no-runtime-no-scoring/`.
- Minimal state entry in `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.

## Must load before writing

- `AGENTS.md`
- `CLAUDE.md`
- `docs/skills/conversation-intent-and-self-reflection-SKILL.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- This bundle folder.
- Safe Cursor summary/report/matrix artifacts only:
  - `artifacts/extension-runtime-token-quality-learnability/host-impl-cursor-01/host-impl-cursor-01-report-ru.md`
  - `artifacts/extension-runtime-token-quality-learnability/runtime-03-cursor-live-runtime/runtime-03-20260618-012559/runtime-03-cursor-live-runtime-evidence.json` as summary pointer only.
  - `artifacts/extension-runtime-token-quality-learnability/cursor-safe-launch-network-process-isolation-repair/cursor-safe-launch-network-process-isolation-repair-20260619-042113/cursor-isolation-repair-report-ru.md`.

Raw request/response/trace contents remain forbidden. Raw-like refs may be recorded only as not-read evidence pointers.

## Required outputs

- Russian human-readable Cursor proof-or-defer report.
- Cursor full-extension requirement matrix.
- Cursor evidence-classification JSON.
- Cursor defer decision JSON when clean full UI/chat proof is not proven.
- Human board JSON preserving counts: 63 total, PASS 9, MISSING 45, DEFERRED 9, approvals 0/9, runner-ready 0/9, quality-pass 0/9.
- Checks report JSON.
- Selected next work order proposal.

## Required Cursor rows

- Real host entrypoint: clean Cursor chat participant/panel UI path, or explicit defer.
- Network/process isolation: no external AI/model call path in the proof packet, or explicit defer.
- Settings/debug surface through the full host path.
- Core API path into `llm_mvp_core` tied to same-run host-surface proof.
- Prompt-to-task through host.
- Task-to-orchestration through host.
- Task/plan-to-skills through host with AvailableSkills and RequestedSkills binding.
- QA with live host evidence or explicitly scoped handler evidence.

## Allowed evidence classes

- Safe summary/report/matrix artifacts.
- Not-read raw evidence pointers copied from safe summaries.
- Existing safe-launch isolation contract as prep evidence.
- Explicit defer packet with reason and required future runtime proof.
- State references and plan references.

## Forbidden substitutions

- Extension Host command/helper evidence labelled as clean full Cursor UI/chat proof.
- Handler-level chat adapter labelled as user-visible chat participant proof.
- Safe-launch contract prep labelled as launch proof.
- Token savings, quality improvement, semantic quality, learnability or readiness claim.

## Forbidden actions

Git commands, dotnet test, external AI/web/model calls, Cursor runtime launch, proof/eval/scoring runners, raw-private content reads, training, DPO, checkpoint/model/locator/target mutation, code changes, product readiness or final readiness claims.

## Verification

- Parse new JSON/JSONL artifacts.
- Markdown whitespace check.
- CR bytes = 0 and trailing horizontal whitespace lines = 0.
- Claim-lock scan: no token savings, quality improvement, semantic quality, learnability, live host runtime readiness, product readiness or final readiness opened.
- State consistency check: current_phase equals this work order id; next_action equals selected follow-up id; checks report records the result.

## Commit boundary

Git authority forbidden. Allowed git commands: NONE. Parent will perform commit/push after review.
