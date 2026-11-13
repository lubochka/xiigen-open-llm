# Q8-01 Code Review/Fix Cycle 3

Verdict: PASS after narrow privacy/history guard fixes.

Scope reviewed: PromptToTask dashboard semantic-quality automation scenario only. Q8-02 and Q8-03 were not started. No production model, training, host, plugin, external AI, staging, commit, or push was run.

## Authority And Verification

- Branch: `codex/chat-gpt-5.5`.
- Allowed verification: changed-file integrity, focused WPF dashboard tests, focused dashboard QA script, state JSON parse, and `git diff --check`.
- Exact problem reviewed: cycle 3 privacy/debug/history/no-overclaim review for Q8-01, especially ensuring raw user prompts are not copied into training evidence or exported history and that debug/raw evidence stays redacted and dashboard-scoped.

## Finding And Fix

Finding: cycle 2 already asserted redacted raw/debug output and false proof flags, and export text did not contain the positive or negative raw prompts. However, the PromptToTask scenario did not explicitly assert that each history entry stores the prompt hash and dashboard evidence scope, or that the static QA guard requires those history/export privacy assertions.

Fix applied: strengthened `Tests.ApiDashboardWpf/ApiDashboardWindowInteractiveTests.cs` so the positive and negative PromptToTask history entries assert exact SHA-256 prompt hashes, dashboard evidence scope, false host/runtime and semantic/trained proof flags, and no raw prompt text in history record text or exported history. Strengthened `tools/api-dashboard-qa.ps1` so Q8-01 fails if those privacy/history/export assertions or the hash helper disappear. Updated dashboard docs with the human-visible history/export boundary.

## Checklist

| Check | Result | Evidence |
|---|---|---|
| Privacy: raw prompt is not copied into training evidence, history, or export text. | PASS_WITH_FIX | WPF scenario asserts `raw_private_prompt_copied_as_training_evidence=false`, redacted raw/debug refs, exact prompt hashes in history/export, and no positive or negative raw prompt in history record text or `LastExportText`. |
| Debug/raw: trace shows safe redacted xiigen method/input/output boundary only. | PASS | Positive and negative helpers require `xiigen prompt-to-task`, `redacted_input_ref:*`, and safe `output_fields:*`; raw prompts are absent from result, raw JSON, and debug trace. |
| History/export: prompt hash and evidence scope are asserted for both positive and negative runs. | PASS_WITH_FIX | Positive and negative `ApiDashboardHistoryEntry` objects assert `PromptSha256`, `EvidenceScope`, and false proof flags; exported history asserts both hashes and dashboard evidence scope. |
| Negative/untrained branch: no semantic quality or trained proof is claimed. | PASS | Negative response is `Success=false` with `prompt_to_task_not_ready_degraded_dashboard_scenario`, visible degraded/not-ready text, and false semantic/trained/host proof flags. |
| Documentation tells Luba what appears in the dashboard and the exact boundary. | PASS_WITH_FIX | PromptToTask docs now include expected visible result, negative case, raw/debug boundary, history/export boundary, and `dashboard_api_only_not_model_quality_not_host_runtime`. |
| QA script fails if Q8-01 lacks privacy/history/debug assertions. | PASS_WITH_FIX | `tools/api-dashboard-qa.ps1` requires the history hash/scope/proof-flag snippets, raw/debug prompt absence snippets, and `ComputeSha256` helper. |
| No Q8-02/Q8-03 or model/training/host/plugin scope included. | PASS | Diff scope remains Q8-01 WPF test, dashboard QA script, dashboard docs, review evidence, and state only. |

## Verification

- `dotnet test Tests.ApiDashboardWpf/Tests.ApiDashboardWpf.csproj --nologo`: PASS, failed 0, passed 6, skipped 0, total 6.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\Projects\llm_mvp_core\tools\api-dashboard-qa.ps1`: PASS, `passed: true`; Q8-01 scenario check has no missing text or control snippets.
- `STATE.json` parse with `ConvertFrom-Json`: PASS after state update.
- `git diff --check`: PASS after review/state finalization.

## Result

Cycle 3 status: PASS.

Fixes applied: PromptToTask history/export hash and evidence-scope assertions, QA-script privacy/history guard strengthening, and dashboard documentation history/export boundary wording.

Next action: Q8-01 code review/fix cycle 4 of 4.
