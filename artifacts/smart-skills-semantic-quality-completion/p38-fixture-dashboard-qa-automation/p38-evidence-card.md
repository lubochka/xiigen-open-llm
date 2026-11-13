# P38 fixture/dashboard QA automation evidence card

Verdict: `PASS_AS_NON_TRAINING_FIXTURE_DASHBOARD_QA_AUTOMATION_PENDING_PARENT_REVIEW`

What was added:
- `tools/semantic-quality-fixture-qa.js`: standalone Node script using only built-in modules.
- `p38-fixture-dashboard-qa-report.json` and `p38-fixture-dashboard-qa-report.md`: script output reports.
- `p38-review-cycles.jsonl`: 8 review/fix cycles.
- `p38-self-check.json`: boundary and evidence summary.

What the script checks:
- P36 fixture rows and dashboard scenarios parse as JSONL.
- P37 dashboard labels and test-plan rows parse as JSONL.
- There are exactly 12 fixtures for PromptToTask, 12 for OrchestrationReasoning, and 12 for SkillSelection.
- Every fixture has a matching dashboard/API scenario and a matching P37 human label.
- Every scenario has human-readable Russian labels, examples, expected visible result text, and debug fields.
- Fixture and label rows keep safe non-training flags: universal only, no raw private text copied, training disabled, eval execution disabled, and not executed yet.
- Held-out source families are unique inside each machine.
- P36/P37 artifacts do not claim semantic quality, live runtime, checkpoint mutation, promotion, or extension host runtime.

Honest boundary:
- No model was called.
- No checkpoint was loaded, rewritten, or promoted.
- No training was run.
- No eval was run against checkpoints.
- No dashboard runtime, Claude runtime, Cursor runtime, or Codex runtime was called.
- This is artifact integrity and dashboard QA readiness evidence only.
