# Next Batch Decision

Decision: PROCEED_TO_CALLGRAPH_BATCH_009
Current batch: dotnet-csharp-callgraph-extraction/batch-008
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-008 verified negative cross-project version flattening: incorrect runtime/language labels fail version gate while routing still chooses documentation unit.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- CPU ms=343.75
- peak RAM MB=35.82

Next batch focus:

1. Add source-root negatives for vendor/build/generated files across projects.
2. Keep valid project/framework labels separate from source-root trust.
