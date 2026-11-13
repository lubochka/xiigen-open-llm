# Next Batch Decision

Decision: PROCEED_TO_CALLGRAPH_BATCH_004
Current batch: dotnet-csharp-callgraph-extraction/batch-003
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-003 verified mixed complete/partial callee coverage in one held-out batch.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- bottom_up_order_accuracy=0.500
- callee_summary_transit_accuracy=0.500
- local_vs_delegated_accuracy=0.500
- CPU ms=281.25
- peak RAM MB=35.668

Next batch focus:

1. Stress source/version gates using real extracted callgraph contexts.
2. Keep documentation task routing correct while rejecting wrong project-kind and framework-version claims.
