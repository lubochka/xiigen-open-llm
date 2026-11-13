# Next Batch Decision

Decision: PROCEED_TO_CALLGRAPH_BATCH_005
Current batch: dotnet-csharp-callgraph-extraction/batch-004
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-004 verified source and framework-version rejection while preserving correct documentation-unit routing.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- CPU ms=359.375
- peak RAM MB=36.348

Next batch focus:

1. Increase bottom-up depth by using callers with multiple extracted callees.
2. Mix complete caller summaries, missing summaries, and wrong-version rows in one run.
