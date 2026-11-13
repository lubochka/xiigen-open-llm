# Q8-03 Dashboard QA Scope Review

Verdict: PASS after one narrow QA-script fix.

Scope reviewed: the full Q8 dashboard QA automation scope after Q8-01 and Q8-02. No production model, training, host, plugin, external AI, staging, commit, or push was run.

## Authority And Verification

- Branch: `codex/chat-gpt-5.5`.
- Allowed verification: focused dashboard WPF tests, focused dashboard QA script, state JSON parse, and `git diff --check`.
- Exact problem reviewed: final Q8-03 scope review for dashboard QA automation scenarios and evidence-boundary honesty.

## Finding And Fix

Finding: the Q8 WPF scenarios and dashboard documentation covered all three required scenario ids, but `tools/api-dashboard-qa.ps1` had an asymmetry. Q8-02 checks inspected comment-stripped executable C# test text, while the Q8-01 PromptToTask guard still inspected raw test-file text. That meant a future edit could leave PromptToTask snippets in comments and still satisfy the Q8-01 QA guard.

Fix applied: changed the Q8-01 PromptToTask QA guard to inspect `interactiveTestExecutableText` for both required scenario text and control-path snippets. This keeps Q8-01 and Q8-02 on the same executable-test-text standard.

## Checklist

| Check | Result | Evidence |
|---|---|---|
| All required Q8 scenario ids exist and are automated. | PASS | PromptToTask, OrchestrationReasoning, and SkillSelection scenarios exist in `Tests.ApiDashboardWpf/ApiDashboardWindowInteractiveTests.cs`. |
| Tests use real WPF control automation paths, not direct view-model shortcuts. | PASS | Scenario tests construct `ApiDashboardWindow`, find WPF controls, select API rows, set input text, toggle raw/debug, and invoke the WPF Run button before asserting gateway requests and visible output. |
| Each scenario has positive and negative/untrained/malformed branch assertions. | PASS | PromptToTask has missing-checkpoint negative branch; OrchestrationReasoning has malformed/untrained branch; SkillSelection has missing-inventory/untrained branch. |
| Each scenario has human API name, example input, run action, and visible result assertions. | PASS | Tests assert human API row names, send the plan/task examples, invoke Run, and check human-readable result fields. |
| Raw/debug/history/export/privacy boundaries are asserted. | PASS | Raw/debug helpers assert redacted refs and no raw input leakage; history/export assertions cover prompt hashes, dashboard evidence scope, false proof flags, and raw-input omission. |
| QA script has guards for Q8-01 and Q8-02 executable test text and docs. | PASS_WITH_FIX | Q8-01 now uses comment-stripped executable text; Q8-02 already did. Docs checks cover all three scenarios. |
| Docs are human-readable and cover all three scenarios. | PASS | `docs/projects/43-smart-skills-domain-dashboard.md` names each API, example input, expected visible result, negative case, raw/debug boundary, history/export boundary, and evidence scope. |
| Proof flags remain false where required. | PASS | `semantic_quality_claimed`, `trained_model_proof_claimed`, `host_runtime_claimed`, `runtime_extension_claimed`, and `external_ai` remain false or explicitly out of scope. |
| Dashboard evidence is not used to claim model quality or host runtime. | PASS | The docs, QA script report, and tests label the scenarios as dashboard/API fixture evidence only. |
| No staged files. | PASS | `git diff --cached --name-only` returned no staged files after state update. |

## Verification

- `dotnet test Tests.ApiDashboardWpf/Tests.ApiDashboardWpf.csproj --nologo`: PASS, failed 0, passed 8, skipped 0, total 8.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\Projects\llm_mvp_core\tools\api-dashboard-qa.ps1`: PASS, `passed: true`.
- State JSON parse: PASS.
- `git diff --check`: PASS.
- No staged files: PASS.

## Result

Q8-03 status: PASS.

Fixes applied: one narrow QA-script guard fix in `tools/api-dashboard-qa.ps1`.

Q8 accepted: yes, as dashboard/API automation evidence only, not model semantic quality, not trained-model proof, and not Claude/Cursor/Codex host runtime.

Next action if continuing the Q8 plan: Q8 pre-commit review/fix cycle 1 of 4.
