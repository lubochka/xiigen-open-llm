# P41 Evidence Card

Scope: source-disjoint pre-eval guard for P36 fixture rows only.

Files:

- `43.SmartSkillsDomain.Dashboard/ApiDashboardSourceDisjointGuard.cs`
- `Tests/ApiDashboardSourceDisjointGuardTests.cs`
- `tools/api-dashboard-qa.ps1`
- `docs/projects/43-smart-skills-domain-dashboard.md`
- `docs/plans/SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1-STATE.json`

Evidence:

- Current P36 manifest and fixture rows pass the precheck.
- Current P36 fixture count is 36, with 12 rows per machine.
- Duplicate fixture ids are detected with temp fixture rows.
- Missing and unexpected manifest-family mismatches are detected with temp files.
- Train/eval source-family overlap is detected with temp rows.
- Proof flags stay false, and `CanProceedToEvalRunner` stays false.
- Dashboard QA checks source, tests, docs, and false proof boundaries.
- JSON/JSONL parse checks and `git diff --check` passed.
- Verification side effect: the allowed fixture QA script refreshed the existing P38 fixture QA report timestamp/check field; no new P38 implementation work was done.

Boundary: no model eval, no training, no checkpoint mutation, no semantic-quality proof, no host runtime proof, no external AI, no eval authorization, no git write/ref commands, no codex commands, no sub-agent commands, and no external-AI commands. The only git command run was the explicitly allowed read-only `git diff --check` verification.
