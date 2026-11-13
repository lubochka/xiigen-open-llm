# Test Command Summary

| Name | Exit | Passed | Command |
| --- | --- | --- | --- |
| runtime_runner_dotnet_run | 0 | true | dotnet run --project <temp>/r.csproj -- . artifacts/extension-runtime-token-quality-learnability/quality-product-orchestration-expanded-v1-training-authorization-eval/quality-product-orchestration-expanded-v1-training-authorization-eval-20260622-102225 |
| artifact_validation | 0 | true | node tools/quality-product-orchestration-expanded-v1-training-authorization-eval.js --validate-only --leaf artifacts/extension-runtime-token-quality-learnability/quality-product-orchestration-expanded-v1-training-authorization-eval/quality-product-orchestration-expanded-v1-training-authorization-eval-20260622-102225 |

JSON/JSONL parse verified: true
LF/no trailing whitespace verified: true
State next work order coherent: true
No overclaim verified: true

Targeted dotnet test was not run because no `Tests/` files were changed. Full dotnet test was not run.
