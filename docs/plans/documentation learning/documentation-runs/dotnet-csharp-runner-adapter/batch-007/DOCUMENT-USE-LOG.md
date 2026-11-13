# Document Use Log

Run: dotnet-csharp-runner-adapter/batch-007
Status: RUNNER_VERIFIED_ACCEPTED

Documents used:

1. README.md
2. CODE-DOC-CALL-TREE-ORCHESTRATION-PLAN-v1.md
3. CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md
4. CODE-DOC-DPO-REGISTRY-GUIDE-v1.md
5. CODE-DOC-EXAMINATION-METHOD-v1.md
6. CODE-DOC-FOUR-LEVEL-TRAINING-PLAN-v1.md
7. CODE-DOC-SOURCE-REGISTRY-v1.md
8. EXECUTOR-PROMPT-CODE-DOC-50-PAIR-LEARNING-v1.md
9. Batch-006 NEXT-BATCH-DECISION.md

Batch-007 adds source-root selection gate:

- valid leaf utility source
- valid Breeze WebApi2 controller source
- excluded EF designer/generated source
- excluded vendor/node_modules/Breeze JavaScript source

Runner observation:

- runner-001 showed source_gate_accuracy=1.000 but version_gate_accuracy=0.760 because vendor JavaScript rows expected version pass incorrectly.
- runner-002 corrected those rows to expect_version_gate_pass=false and passed all gates.
