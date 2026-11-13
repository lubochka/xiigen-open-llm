# Checks report

Targeted checks for code acceptance score-range fix:

- `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionEvalOnlyQualityRunner"`: passed, 20 tests, 0 failed.
- `dotnet build 43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Quality02EvalOnlyRunner.csproj`: build succeeded, 0 warnings, 0 errors.
- Synthetic runner invocation: passed, regenerated JSON, JSONL and Markdown output in this folder with `evidence_scope=local_private_eval_only`, `synthetic_fixture_only=true`, and `actual_approved_eval_executed=false`.
- Score-range validation coverage: passed for `baseline_score` below 0, `baseline_score` above 5, `adaptive_score` below 0, `adaptive_score` above 5, no generated reports on invalid scores, and valid boundary values 0 and 5.
- Excluded-route privacy probes: still covered by the targeted runner test suite.
- JSON parse checks: passed for evidence JSON files after the score-range fix report write.
- JSONL parse checks: passed for 2 files and 4 rows: synthetic input rows and generated score rows.
- No forbidden product claims opened: passed.
- Touched-output raw-marker scan: passed; configured marker values are absent from the regenerated runner JSON, JSONL, Markdown, and score-range fix report files.
- `git diff --check`: passed, exited 0.

No broad full `dotnet test` run was performed; the bounded work order allowed the runner-specific filter only.
