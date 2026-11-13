# Next Batch Decision

Decision: PROCEED_TO_CALLGRAPH_BATCH_010
Current batch: dotnet-csharp-callgraph-extraction/batch-009
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-009 verified source-root exclusion for generated/build/package/vendor paths while preserving version/transit correctness.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- CPU ms=312.5
- peak RAM MB=35.586

Next batch focus:

1. Balanced checkpoint combining pass, source-fail, version-fail, and transit-fail rows.
2. End the first 500-row callgraph stream with mixed gates in a single run.
