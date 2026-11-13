# Document Use Log

Run: dotnet-csharp-runner-adapter/batch-001
Status: RUNNER_VERIFIED_ACCEPTED

Documents used before building or running this batch:

1. README.md
2. CODE-DOC-CALL-TREE-ORCHESTRATION-PLAN-v1.md
3. CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md
4. CODE-DOC-DPO-REGISTRY-GUIDE-v1.md
5. CODE-DOC-EXAMINATION-METHOD-v1.md
6. CODE-DOC-FOUR-LEVEL-TRAINING-PLAN-v1.md
7. CODE-DOC-SOURCE-REGISTRY-v1.md
8. EXECUTOR-PROMPT-CODE-DOC-50-PAIR-LEARNING-v1.md
9. External CODE-DOC-EXTERNAL-SOURCE-REGISTRY-v1.md
10. Existing dotnet-csharp generated review-extract JSONL files
11. Local legacy validation projects: wizGameserver, GenieDynamicServer, geniesoftGameMVPServer, genietasks

Important correction discovered during the run:

- runner-001 produced version_gate_accuracy=0.0 because the batch field `csharp_language_version` was not accepted by the C# record property `CSharpLanguageVersion` under SnakeCaseLower. The runner was fixed with an explicit JsonPropertyName mapping and runner-002 passed.
