# Q8-01 Code Review/Fix Cycle 1

Verdict: PASS after one narrow QA-script fix.

Scope reviewed: PromptToTask dashboard semantic-quality automation scenario only. Q8-02 and Q8-03 were not started. No production model, training, host, plugin, or external AI files were changed. No staging, commit, or push was run.

## Authority And Verification

- Branch: `codex/chat-gpt-5.5`.
- Allowed verification: changed-file review for Q8-01, focused WPF dashboard tests, focused dashboard QA script, JSON parse, and `git diff --check`.
- Exact problem reviewed: ensure Q8-01 automation cannot pass with fake, smoke-only, or overclaimed evidence for semantic quality, trained-model readiness, or host runtime.

## Finding And Fix

Finding: the WPF test did drive both the positive PromptToTask scenario and the missing/untrained negative scenario through real controls, but `tools/api-dashboard-qa.ps1` only required positive control-path snippets and negative-case text. That meant a future edit could keep the not-ready strings in the file while accidentally removing the negative WPF input/run/assert path.

Fix applied: strengthened `tools/api-dashboard-qa.ps1` so the Q8-01 scenario check requires these negative-path snippets too:

- `SetText(inputBox, negativeInput);`
- `Assert.Equal(negativeInput, gateway.LastRequest.UserPromptOrTaskText);`
- `AssertPromptToTaskScenarioNotReadyOutput(`

## Checklist

| Check | Result | Evidence |
|---|---|---|
| Scenario drives real WPF controls/automation path, not a direct view-model shortcut. | PASS | `ApiDashboardWindow_PromptToTaskSemanticQualityScenario_DrivesControlsAndShowsHonestDashboardEvidence` uses `FindRequired` for WPF controls, `SelectListBoxItem`, `SetText`, toggles, and `InvokeButton(runButton)` through automation peers. |
| It selects the PromptToTask human API row by contract/human row and uses a real example/input. | PASS | Selects `XiigenExtensionApiContract.PromptToTaskMethod`, asserts the visible human name `Turn a user prompt into a structured task`, and sends `Write a plan for improving skill selection; do not implement.` |
| It asserts visible structured task output, constraints, forbidden actions, evidence, and `implementation_authorized=false`. | PASS | `AssertPromptToTaskScenarioPositiveOutput` checks task type, goal, constraints, forbidden actions, required evidence, structured task ref, and `implementation_authorized=false`. |
| It asserts raw/debug boundary: redacted refs/output only, no raw private prompt copied as training evidence. | PASS | Positive and negative helpers assert raw prompt absence from result, raw JSON, debug trace, and export text; raw/debug areas use `redacted_input_ref` and `output_fields`. |
| It includes negative/untrained/missing-state honesty branch and verifies no semantic-quality/trained-model/host-runtime overclaim. | PASS | Negative input is run through `SetText` and `InvokeButton`; `AssertPromptToTaskScenarioNotReadyOutput` checks not-ready/degraded text and false boundary flags. |
| `tools/api-dashboard-qa.ps1` checks the scenario by name and key assertions, not just file presence. | PASS_WITH_FIX | The script now requires scenario name, example, not-ready fields, positive control snippets, and negative control/assert snippets. |
| Docs explain the scenario in human language with API name, example input, expected result, negative case, raw/debug boundary, evidence scope. | PASS | `docs/projects/43-smart-skills-domain-dashboard.md` has the PromptToTask scenario table and QA mapping row. |
| No Q8-02/Q8-03 scope accidentally implemented; no external AI; no model/training changes; no host runtime claims. | PASS | Diff is limited to Q8-01 dashboard test/script/docs/state and this review artifact; source/QA output keeps semantic quality, trained-model proof, host runtime, and external AI claims false. |
| `git diff --check` passes. | PASS | Run after review/state edits. |

## Verification

- `dotnet test Tests.ApiDashboardWpf/Tests.ApiDashboardWpf.csproj --nologo`: PASS, failed 0, passed 6, skipped 0, total 6.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\Projects\llm_mvp_core\tools\api-dashboard-qa.ps1`: PASS, `q8_01_prompt_to_task_semantic_quality_wpf_scenario_exists` passed with no missing text and no missing control snippets.
- `powershell -NoProfile -Command "$json = Get-Content -Raw -LiteralPath 'C:\Projects\llm_mvp_core\docs\plans\SMART-SKILLS-SEMANTIC-QUALITY-UPLIFT-STATE.json'; $null = $json | ConvertFrom-Json"`: PASS.
- `git diff --check`: PASS.

## Result

Cycle 1 status: PASS.

Fixes applied: strengthened `tools/api-dashboard-qa.ps1` negative-path guard for Q8-01.

Next action: Q8-01 code review/fix cycle 2 of 4.
