# Next Batch Decision

Decision: PROCEED_TO_CALLGRAPH_BATCH_011
Current batch: dotnet-csharp-callgraph-extraction/batch-010
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-010 completed the first 500-row callgraph checkpoint with all gate outcomes mixed in one held-out run.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- CPU ms=390.625
- peak RAM MB=36.852

Next batch focus:

1. Begin deeper semantic documentation rows: controller/action/service/helper roles from extracted symbols.
2. Preserve the same 50 train + 50 held-out runner-verified cycle.
