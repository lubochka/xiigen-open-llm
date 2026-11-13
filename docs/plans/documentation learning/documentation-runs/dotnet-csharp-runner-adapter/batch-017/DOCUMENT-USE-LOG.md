# Document Use Log

Batch: dotnet-csharp-runner-adapter/batch-017
Status: RUNNER_VERIFIED_ACCEPTED

Documents and contracts used before this batch was accepted:

- CODE-DOC-ACTIVE-GOAL-2026-05-13.md
- CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md
- CODE-DOC-CALL-TREE-ORCHESTRATION-PLAN-v1.md
- CODE-DOC-FOUR-LEVEL-TRAINING-PLAN-v1.md
- CODE-DOC-DPO-REGISTRY-GUIDE-v1.md
- EXECUTOR-PROMPT-CODE-DOC-50-PAIR-LEARNING-v1.md
- CodeDocumentationRoutingFlow.cs
- CodeDocumentationTrainingRunner.cs
- CodeDocumentationBatchFileRunner.cs

Acceptance basis:

- 50 train rows read from TRAIN-PAIRS.jsonl.
- 50 held-out rows read from HELDOUT-ROWS.jsonl.
- Held-out rows test net46 WebApi2/Breeze vs netcoreapp3.1 ASP.NET Core controller contrast.
- CPU/RAM/timing metrics are real runner observations, not projections.
