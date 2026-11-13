# Next Batch Decision

Decision: PROCEED_TO_CALLGRAPH_BATCH_008
Current batch: dotnet-csharp-callgraph-extraction/batch-007
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-007 verified cross-project stack/version preservation across four validation projects.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- CPU ms=375
- peak RAM MB=35.746

Next batch focus:

1. Negative cross-project flattening: wrong runtime/language labels should fail per project.
2. Keep documentation route selected while gate verdict moves to review.
