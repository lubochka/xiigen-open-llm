# Next Batch Decision

Decision: PROCEED_TO_CALLGRAPH_BATCH_002
Current batch: dotnet-csharp-callgraph-extraction/batch-001
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Actual extracted callgraph positives passed with exact callee coverage.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- bottom_up_order_accuracy=1.000
- callee_summary_transit_accuracy=1.000
- local_vs_delegated_accuracy=1.000
- CPU ms=390.625
- peak RAM MB=36.621

Next batch focus:

1. Generate extracted-callgraph negatives by omitting one real callee summary.
2. Verify the exact transit gate fails only those rows and not source/version gates.
3. Keep local projects validation-only.
