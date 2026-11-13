# Q8-01 Code Review/Fix Cycle 2

Verdict: PASS after narrow assertion, QA-script, and documentation-scope fixes.

Scope reviewed: PromptToTask dashboard semantic-quality automation scenario only. Q8-02 and Q8-03 were not started. No production model, training, host, plugin, external AI, staging, commit, or push was run.

## Authority And Verification

- Branch: `codex/chat-gpt-5.5`.
- Allowed verification: changed-file integrity, focused WPF dashboard tests, focused dashboard QA script, JSON parse, and `git diff --check`.
- Exact problem reviewed: make sure Q8-01 cannot pass with generic response assertions, indistinguishable not-ready behavior, raw prompt leakage, or dashboard/API evidence overclaimed as model quality or host runtime.

## Findings And Fixes

Finding 1: the WPF scenario already drove the PromptToTask row through real WPF controls, but raw JSON checks only required some key names. A future change could keep the keys while dropping the exact `false` values or PromptToTask-specific check-set fields.

Fix 1: strengthened `Tests.ApiDashboardWpf/ApiDashboardWindowInteractiveTests.cs` with `AssertPromptToTaskCheckSetValue`, exact positive and negative check-set value assertions, and explicit history assertions proving the not-ready branch records `Success=false` with the degraded status.

Finding 2: `tools/api-dashboard-qa.ps1` guarded the Q8-01 scenario name and broad strings, but not the new exact check-set assertions or the not-ready history assertions.

Fix 2: strengthened `tools/api-dashboard-qa.ps1` so Q8-01 requires the exact PromptToTask check-set assertion calls, `host_runtime_claimed=false`, negative history failure evidence, and documentation wording that the automated scenario uses a `RecordingGateway` fixture.

Finding 3: the dashboard documentation described the target technical path too broadly and could be read as proof that the automated WPF scenario invoked live `PromptToTaskMachine.Translate`.

Fix 3: updated `docs/projects/43-smart-skills-domain-dashboard.md` to state that the automated scenario drives `ApiDashboardWindow -> ApiDashboardRunner -> RecordingGateway`, not live model inference, while preserving the product contract target and dashboard/API evidence boundary.

## Checklist

| Check | Result | Evidence |
|---|---|---|
| Assertion robustness is PromptToTask-specific, not generic response text only. | PASS_WITH_FIX | Positive helper now asserts task fields plus exact raw JSON check-set values for scenario id, implementation authorization, constraints, forbidden actions, required evidence, raw prompt evidence false, semantic quality false, trained proof false, and host runtime false. |
| Negative/untrained branch is distinguishable from normal success. | PASS_WITH_FIX | Negative run goes through `SetText(inputBox, negativeInput)` and `InvokeButton(runButton)`, asserts not-ready status/output, and verifies latest history entry has `Success=false`. |
| Raw/debug assertions verify redacted refs/fields and no raw prompt evidence. | PASS | Positive and negative helpers require redacted input refs, output field lists, exact false raw-prompt-training evidence, and absence of raw input in result, raw JSON, debug trace, and export text. |
| QA script requires scenario method/name and false proof keys. | PASS_WITH_FIX | Script now requires the scenario method/name, exact check-set assertion snippets, `implementation_authorized=false`, `semantic_quality_claimed=false`, `trained_model_proof_claimed=false`, and `host_runtime_claimed=false`. |
| Documentation mirrors the automated scenario scope. | PASS_WITH_FIX | Documentation names the `RecordingGateway` fixture path and explicitly says the WPF automation does not call live `PromptToTaskMachine.Translate`. |
| No Q8-02/Q8-03 or model/training/host/plugin scope included. | PASS | Diff scope remains Q8-01 WPF test, dashboard QA script, dashboard docs, Q8 review evidence, and state only. |
| No external AI, model semantic quality, trained-model proof, or host runtime claim. | PASS | Source, QA script, docs, and review/state keep those claims false or explicitly outside scope. |

## Verification

- `dotnet test Tests.ApiDashboardWpf/Tests.ApiDashboardWpf.csproj --nologo`: PASS, failed 0, passed 6, skipped 0, total 6.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\Projects\llm_mvp_core\tools\api-dashboard-qa.ps1`: PASS, `passed: true`.
- `STATE.json` parse with `ConvertFrom-Json`: PASS.
- `git diff --check`: PASS after review/state finalization.

## Result

Cycle 2 status: PASS.

Fixes applied: exact PromptToTask raw/check-set assertions, negative history distinction, QA-script guard strengthening, and documentation scope correction.

Next action: Q8-01 code review/fix cycle 3 of 4.
