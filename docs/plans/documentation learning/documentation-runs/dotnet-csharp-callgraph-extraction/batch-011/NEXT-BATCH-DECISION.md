# Next Batch Decision

Decision: PROCEED_TO_CALLGRAPH_BATCH_012
Current batch: dotnet-csharp-callgraph-extraction/batch-011
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-011 added semantic role preservation over extracted caller/callee contexts.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- CPU ms=390.625
- peak RAM MB=36.434

Next batch focus:

1. Role-mismatch rejected examples: controller documented as helper, startup as model, data helper as UI action.
2. Keep structural gates passing while strengthening rejected-side semantic contrast.
