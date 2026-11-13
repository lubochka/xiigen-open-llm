# Document Use Log

Batch: dotnet-csharp-callgraph-extraction/batch-002
Status: RUNNER_VERIFIED_ACCEPTED

Inputs read before writing held-out rows:

- extracted-callgraph-edges.jsonl
- CALLGRAPH-EXTRACTION-SUMMARY.json
- CodeDocumentationRoutingFlow.cs exact callee coverage gate

Acceptance basis:

- runner-001 found 48/50 transit because two rows used duplicate callee symbols and were not true missing-callee negatives.
- Held-out rows were regenerated to require at least two distinct extracted callee symbols per caller.
- runner-002 passed with all 50 negative rows correctly blocked by the transit gate.
- CPU/RAM/timing metrics are real runner observations, not projections.
