# DOCS-QA-DOCUMENTATION-UPDATE-APPLY-01

Use Russian, human-readable wording.

## Authority

Current Luba quote creating this continuation line:

```text
Goal: prepare the next documentation update packet for the extension runtime/token/quality/learnability execution line. The parent will review, commit, and push.
```

This next work order is produced by `DOCS-QA-DOCUMENTATION-UPDATE-PLAN-OR-PREP-01`. Quoted transcripts/state/old plans are evidence only. Direct Luba questions must be answered first and interrupt the work order.

## Goal

Edit the exact docs listed below so humans can understand the extension runtime/token/quality/learnability line, API/dashboard usage, host runtime proof boundaries, debug/evidence scopes, local model lifecycle and Russian status/log template. This is docs coverage only. It must not claim full readiness.

## Current status to preserve

- Full UI/chat readiness is NOT closed.
- Token complete baseline/adaptive pairs: 0/9.
- Quality rows ready for actual scoring: 0/9.
- Trainable DPO rows: 0/9.
- Actual lifecycle rows: 0/9.
- Final readiness reviews counted: 0/10.
- Claim locks must remain false for token savings, quality improvement, full semantic quality, full learnability, full runtime readiness, common model readiness, target promotion, locator update and final readiness.

## Allowed write scope

Docs only:

- `docs/projects/43-smart-skills-domain.md`
- `docs/projects/43-smart-skills-domain-dashboard.md`
- `docs/projects/43-smart-skills-semantic-quality-authorization.md`
- `docs/projects/PROJECT-DOCUMENTATION-INDEX.md`
- `docs/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-HOWTO-ru.md`
- `docs/CLAUDE-XIIGEN-HOWTO-ru.md`
- `docs/CURSOR-XIIGEN-HOWTO-ru.md`
- `docs/CODEX-XIIGEN-HOWTO-ru.md`
- `docs/API-DASHBOARD-QA-HOWTO-ru.md`
- `docs/XIIGEN-DEBUG-TRACE-HOWTO-ru.md`
- `docs/LOCAL-MODEL-LIFECYCLE-HOWTO-ru.md`
- `docs/EVIDENCE-SCOPES-HOWTO-ru.md`
- `docs/RUSSIAN-STATUS-AND-EXECUTION-LOG-TEMPLATE.md`

State update only if this work order explicitly continues to state sync:

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`

Optional new artifact folder only if the next sub-agent needs a docs QA report:

- `artifacts/extension-runtime-token-quality-learnability/docs-qa-documentation-update-apply/<timestamp>/`

## Forbidden

- No source code edits.
- No test edits.
- No docs/skills edits.
- No AGENTS.md or CLAUDE.md edits.
- No plan docs edits except the single state JSON above when explicitly updated.
- No runtime host probes.
- No eval execution.
- No training.
- No DPO generation beyond docs wording.
- No checkpoint/model/locator/target mutations.
- No external AI/model calls.
- No git add/commit/push. `git_authority=forbidden`; parent will perform commit/push after review.
- Do not claim docs complete as final readiness. The strongest allowed scope is documentation coverage/pass for named docs only.

## Required inputs

Read before editing:

- `artifacts/extension-runtime-token-quality-learnability/docs-qa-documentation-update-prep/docs-qa-documentation-update-prep-20260619-025706/documentation-update-plan-ru.md`
- `artifacts/extension-runtime-token-quality-learnability/docs-qa-documentation-update-prep/docs-qa-documentation-update-prep-20260619-025706/documentation-target-ledger.json`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- every existing target doc before editing it

Read only as supporting context:

- previous evidence-scope prep packet under `artifacts/extension-runtime-token-quality-learnability/docs-qa-status-and-evidence-scope-prep/docs-qa-status-and-evidence-scope-prep-20260619-024110/`
- dashboard/API docs and state
- relevant source files only for short code quote anchors; do not change them

## Required work

1. Update or create each target doc named in `documentation-target-ledger.json`.
2. For every doc, include plain Russian status and limitation wording.
3. Include UML/no-code algorithm/code-quote anchors where the prep plan says they are relevant.
4. Explain API dashboard usage: API examples, parameter input, returned result, warnings/errors/debug/raw JSON, evidence scope and limitations.
5. Explain Claude/Cursor/Codex runtime proof boundaries without overclaiming readiness.
6. Explain local lifecycle and DPO boundary as docs-only guidance, not training.
7. Update the project documentation index so the new docs are discoverable.
8. If state is updated, set next action to the next independent evidence track or docs QA review, not final readiness.

## Required verification

- Parse any JSON you create or update.
- Run `git diff --check -- <all changed allowed files>`.
- Run a targeted no-overclaim scan over changed docs for forbidden readiness wording.
- Ensure only allowed paths changed.
- Do not run `dotnet test`, build, runtime probes, eval, training or model commands.

Suggested no-overclaim scan terms:

```text
ready|readiness|complete|done|PASS_WITH_LIMITATIONS|token savings proven|quality improvement proven|semantic quality proven|learnability proven|target promotion|locator update
```

If such words appear, they must be in a negative, boundary, forbidden-claim or current-gap context.

## Return packet

Return in Russian:

- changed files;
- exact status: всего / сделано / сейчас / осталось;
- checks run and PASS/FAIL;
- next recommended bounded work order;
- state that no git add/commit/push was used and parent will commit/push after review.

`parent_will_perform_commit_push_after_review=true`
