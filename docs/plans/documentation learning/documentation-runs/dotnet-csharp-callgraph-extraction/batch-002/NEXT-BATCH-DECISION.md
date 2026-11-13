# Next Batch Decision

Decision: PROCEED_TO_CALLGRAPH_BATCH_003
Current batch: dotnet-csharp-callgraph-extraction/batch-002
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-002 verified exact missing-callee detection on real extracted call graph edges.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- bottom_up_order_accuracy=0.000
- callee_summary_transit_accuracy=0.000
- local_vs_delegated_accuracy=0.000
- CPU ms=359.375
- peak RAM MB=36.305

Next batch focus:

1. Mix extracted-callgraph positive and negative rows in one held-out batch.
2. Confirm the gate can distinguish complete vs partial callee coverage in the same run.
