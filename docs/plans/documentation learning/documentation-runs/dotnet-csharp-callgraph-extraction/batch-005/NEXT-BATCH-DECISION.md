# Next Batch Decision

Decision: PROCEED_TO_CALLGRAPH_BATCH_006
Current batch: dotnet-csharp-callgraph-extraction/batch-005
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-005 verified high-degree caller handling with 3+ distinct extracted callees.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- bottom_up_order_accuracy=0.480
- callee_summary_transit_accuracy=0.480
- local_vs_delegated_accuracy=0.480
- CPU ms=359.375
- peak RAM MB=37.906

Next batch focus:

1. Add leaf-vs-caller contrast: D1 leaf rows should not require callees, D4 caller rows should.
2. Verify documentation level changes the transit requirement using the same project corpus.
