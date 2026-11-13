# Next Batch Decision

Decision: PROCEED_TO_CALLGRAPH_BATCH_007
Current batch: dotnet-csharp-callgraph-extraction/batch-006
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-006 verified level-aware transit: D1 local/leaf rows pass without known callees; D3/D4 caller rows require them.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- CPU ms=343.75
- peak RAM MB=36.094

Next batch focus:

1. Cross-project validation: distribute held-out rows evenly over the four local legacy projects.
2. Keep stack/version labels project-aware while using the same documentation unit.
