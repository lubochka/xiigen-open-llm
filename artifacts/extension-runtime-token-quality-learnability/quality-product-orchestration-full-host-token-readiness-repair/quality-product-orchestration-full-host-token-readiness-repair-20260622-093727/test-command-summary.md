# Test Command Summary

| Name | Exit | Passed | External AI/model call | Command |
| --- | --- | --- | --- | --- |
| focused_orchestration_reasoning_p9b_eval_ablation_test | 0 | true | false | dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~OrchestrationReasoningP9BEvalAblationRerunTests --no-restore --logger console;verbosity=minimal |
| codex_installed_skill_adapter_smoke | 0 | true | false | node C:\Projects\codex-xiigen-smart-skills\src\codex-entrypoint.js --method prompt-to-task ... |
| generated_artifact_validation | 0 | true | false | node tools/quality-product-orchestration-full-host-token-readiness-repair.js --validate-only --leaf artifacts/extension-runtime-token-quality-learnability/quality-product-orchestration-full-host-token-readiness-repair/quality-product-orchestration-full-host-token-readiness-repair-20260622-093727 |
| review_findings_repair_validate_only | 0 | true | false | node tools/quality-product-orchestration-full-host-token-readiness-repair.js --validate-only --leaf artifacts/extension-runtime-token-quality-learnability/quality-product-orchestration-full-host-token-readiness-repair/quality-product-orchestration-full-host-token-readiness-repair-20260622-093727 |

JSON/JSONL/state parse verified: true
LF/no trailing whitespace verified: true

Full dotnet test was not run.
