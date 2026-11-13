# Next Batch Decision

Decision: PROCEED_TO_BATCH_007
Current batch: dotnet-csharp-runner-adapter/batch-006
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Cross-file startup/config transit passed.
- routing_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000

Next batch focus:

1. Add repository-level file classification: leaf utility vs controller vs config vs workflow root.
2. Add source-root selection negatives where documentation starts from generated/vendor files incorrectly.
3. Keep 50 train + 50 held-out per batch.
