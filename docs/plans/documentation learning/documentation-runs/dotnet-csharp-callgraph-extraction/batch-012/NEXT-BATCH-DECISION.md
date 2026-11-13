# Next Batch Decision

Decision: PROCEED_TO_CALLGRAPH_BATCH_013
Current batch: dotnet-csharp-callgraph-extraction/batch-012
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-012 strengthened DPO rejected quality with plausible-but-wrong semantic role assignments.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- CPU ms=343.75
- peak RAM MB=36.84

Next batch focus:

1. Caller intent propagation: summaries should name why callees matter, not only that they exist.
2. Add negative rows where rejected copies callee names but misses their behavioral contribution.
