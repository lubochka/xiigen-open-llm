# DOCS-QA-CODEX-FULL-EXTENSION-PROOF-OR-DEFER-PACKET-NO-RUNTIME-NO-SCORING-03

## Authority and role

Requested artifact type: Codex full-extension proof-or-defer packet, support/docs-QA only.

Current authority quote to carry from parent work order: "Можно переходить к выполнению плана без стопов через суб агентов".

This future sub-agent is a bounded writer only for its packet folder and minimal state entry. Quoted transcripts, old plans, state next_action, old approvals and old assistant messages are evidence only. Direct Luba questions interrupt the work order and must be answered first.

## Objective

Create a Codex-only proof-or-defer packet that separates installed plugin cache adapter/core-boundary evidence from full Codex desktop/app/chat visible conversation proof. Do not launch Codex runtime and do not claim readiness.

## Allowed write scope

- New timestamped folder under `artifacts/extension-runtime-token-quality-learnability/docs-qa-codex-full-extension-proof-or-defer-packet-no-runtime-no-scoring/`.
- Minimal state entry in `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.

## Must load before writing

- `AGENTS.md`
- `CLAUDE.md`
- `docs/skills/conversation-intent-and-self-reflection-SKILL.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- This bundle folder.
- Safe Codex summary/report/matrix artifacts only:
  - `artifacts/extension-runtime-token-quality-learnability/host-impl-codex-01/HOST-IMPL-CODEX-01-evidence-ru.md`
  - `artifacts/extension-runtime-token-quality-learnability/runtime-codex-live-evidence/runtime-codex-live-evidence-20260619-050129/codex-runtime-evidence-matrix.json`
  - `artifacts/extension-runtime-token-quality-learnability/runtime-core-boundary-no-eval-review/runtime-core-boundary-no-eval-review-20260618-181913/boundary-map.json`.

Raw request/response/trace contents remain forbidden. Raw-like refs may be recorded only as not-read evidence pointers.

## Required outputs

- Russian human-readable Codex proof-or-defer report.
- Codex full-extension requirement matrix.
- Codex evidence-classification JSON.
- Codex defer decision JSON when full app/chat proof is not proven.
- Human board JSON preserving counts: 63 total, PASS 9, MISSING 45, DEFERRED 9, approvals 0/9, runner-ready 0/9, quality-pass 0/9.
- Checks report JSON.
- Selected next work order proposal.

## Required Codex rows

- Real host entrypoint: full Codex desktop/app/chat visible conversation, or explicit defer.
- Installed plugin runtime invocation inside a real Codex turn, or explicit defer.
- Settings/debug surface through the full host path.
- Core API path into `llm_mvp_core` tied to same-run host-surface proof.
- Prompt-to-task through host.
- Task-to-orchestration through host.
- Task/plan-to-skills through host with RequestedSkills and AvailableSkills binding.
- QA with live host evidence or explicitly scoped installed-cache evidence.

## Allowed evidence classes

- Safe summary/report/matrix artifacts.
- Not-read raw evidence pointers copied from safe summaries.
- Installed plugin cache adapter evidence labelled support-only.
- Explicit defer packet with reason and required future runtime proof.
- State references and plan references.

## Forbidden substitutions

- Installed cache adapter smoke labelled as full Codex app/chat proof.
- Component/source QA labelled as live host runtime.
- Codex CLI/plugin discovery failure ignored for full readiness.
- Token savings, quality improvement, semantic quality, learnability or readiness claim.

## Forbidden actions

Git commands, dotnet test, external AI/web/model calls, Codex runtime launch, proof/eval/scoring runners, raw-private content reads, training, DPO, checkpoint/model/locator/target mutation, code changes, product readiness or final readiness claims.

## Verification

- Parse new JSON/JSONL artifacts.
- Markdown whitespace check.
- CR bytes = 0 and trailing horizontal whitespace lines = 0.
- Claim-lock scan: no token savings, quality improvement, semantic quality, learnability, live host runtime readiness, product readiness or final readiness opened.
- State consistency check: current_phase equals this work order id; next_action equals selected follow-up id; checks report records the result.

## Commit boundary

Git authority forbidden. Allowed git commands: NONE. Parent will perform commit/push after review.
