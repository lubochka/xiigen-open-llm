# Row10 Split/Harness Prep Review

```text
claim_scope: bounded sub-agent review of Row10 Cursor split/harness prep
evidence_scope: read target Markdown/JSON; ran JSON parse, MD/JSON consistency, targeted text checks, whitespace scan, and git diff --check
count_claimed: 8 check records
independent_evidence_count: 8 command outputs
subagent_id_or_NONE: bounded-subagent-reviewer-row10
downgrade_needed: yes; non-execution is verified only from packet contents and this reviewer's own actions, not from external runtime/process logs
```

## Verdict

NEEDS_FIX

The prep is useful and mostly execution-ready as a planning artifact, but it does not strictly satisfy the requested "dual-path same-scenario testing for each scenario" gate for all 7 sub-scenarios.

## Evidence Reviewed

- `row10-split-harness-prep.md`
- `row10-split-harness-prep.json`

## Checks Performed

- JSON parse: PASS.
- JSON has 7 sub-scenarios: PASS (`Row10-S1` through `Row10-S7`).
- Markdown has 7 unique sub-scenario IDs: PASS (`Row10-S1` through `Row10-S7`).
- Markdown/JSON agreement on IDs and human-meaning rows: PASS.
- Each JSON sub-scenario has non-empty `path_a` and `path_b`: PASS.
- All JSON `max_active_minutes` values are <= 10: PASS.
- Claim locks are present and false: PASS (12/12 false).
- Whitespace scan: PASS, no trailing whitespace in either target file.
- `git diff --check` on both target files: PASS, no output.

## Passing Coverage

- Auth/session preflight is present.
- Browser login recovery is present for anonymous/lost/wrong-account Cursor state.
- Secret-safety wording is present, including `no secrets captured`.
- Future UI steps name the program/window target, panel, textarea `xiigen-smart-skills-prompt`, submit button `xiigen-smart-skills-submit`, expected before/after screenshots, visible result/error evidence, and forbidden generic Cursor chat/New Agent substitutions.
- Claim locks cover product readiness, joined pass, host readiness, visible answer, token savings, semantic quality, training/learnability, lifecycle readiness, checkpoint promotion, locator update, runtime execution, and test execution.
- The packet states it did not execute product/runtime/UI/tests/training and contains no product/runtime/UI/test/training output evidence. This review did not independently inspect external process logs, so the verified claim is limited to packet contents plus this reviewer's own non-execution.

## Blocking Findings

### F1 - Strict dual-path coverage is not explicit for all 7 sub-scenarios

Requirement checked: each sub-scenario should cover same-scenario dual-path testing with Path A automated/boundary/API up to host boundary and Path B real local Cursor UI/program screenshots/evidence.

Observed:

- Markdown line 19 makes Path A global and qualified with "where possible", not exact per slice.
- `Row10-S1` Markdown line 56 and JSON `path_b` explicitly say no UI execution / scripted future UI only. That can be valid as a prep-only contract slice, but then S1 must be explicitly exempted from strict executable dual-path coverage or not counted as one of the executable dual-path scenarios.
- `Row10-S4` Path A says approved source-disjoint/heldout guard harness, but does not explicitly anchor the check to an llm_mvp_core boundary/API/host-boundary artifact.
- `Row10-S5` Path A says dry-run/read-only lifecycle boundary checks, but does not explicitly name the automated/API/host-boundary interface or required evidence.
- `Row10-S6` Path A says read-only/staging-only checkpoint/locator refs, but does not explicitly name the automated/API/host-boundary interface or required evidence.

Required fix:

1. In both Markdown and JSON, decide and state one exact rule for `Row10-S1`: either `Row10-S1` is prep-only and excluded from executable dual-path coverage, or it must include a real Path B Cursor UI proof requirement with screenshot/evidence and BLOCKED criteria.
2. For `Row10-S4`, `Row10-S5`, and `Row10-S6`, rewrite Path A in both Markdown and JSON to name the exact llm_mvp_core automated/boundary/API/host-boundary interface or artifact expected for the future slice. If the exact interface is intentionally unknown, add a BLOCKED criterion: missing Path A boundary/API interface means the slice cannot execute.
3. Keep the same marker and prompt hash requirements across both paths for every executable slice.

### F2 - Some evidence language is still optional where PASS/BLOCKED should be deterministic

Observed:

- `Row10-S5` Path B says "capture screenshots if available". For a UI proof slice, screenshot/evidence should be required or the slice should BLOCK.
- `Row10-S2` Path A and JSON mention trace refs "if available"; this is acceptable only if request/response refs are mandatory and trace ref absence is recorded as non-blocking.
- The global UI steps are exact enough, but per-slice rows should point to those mandatory before/after screenshot filenames or explicitly BLOCK when visible evidence is unavailable.

Required fix:

1. Replace `if available` / `if surfaced` wording for required UI evidence with deterministic language: capture the screenshot/ref, or mark BLOCKED with the missing visible state.
2. In both Markdown and JSON, make mandatory evidence filenames explicit per executable slice or reference the global filename pattern as required evidence.

## Non-Execution Statement

I did not execute Cursor, product runtime, UI automation, tests, builds, training, checkpoint/locator operations, lifecycle operations, web access, external AI, nested subagents, git stage, commit, push, reset, checkout, or cleanup. I only read the target prep files, ran parse/text checks, created this review folder, and wrote this review packet.

## Required Next Work

Next bounded sub-agent task: repair the Row10 prep packet only.

Allowed write scope for that repair should be limited to:

- `row10-split-harness-prep.md`
- `row10-split-harness-prep.json`
- one repair evidence packet folder if the parent work order allows it

The repair sub-agent should implement F1 and F2, then rerun JSON parse, MD/JSON agreement checks, strict 7 sub-scenario dual-path coverage checks, and whitespace/diff checks. Product execution, Cursor UI execution, tests/builds, board/status edits, commit/push, web, external AI, and nested subagents should remain forbidden.
