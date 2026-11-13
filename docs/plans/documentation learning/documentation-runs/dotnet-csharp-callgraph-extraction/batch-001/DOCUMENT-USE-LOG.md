# Document Use Log

Batch: dotnet-csharp-callgraph-extraction/batch-001
Status: RUNNER_VERIFIED_ACCEPTED

Inputs read before writing held-out rows:

- extracted-method-inventory.jsonl
- extracted-callgraph-edges.jsonl
- CALLGRAPH-EXTRACTION-SUMMARY.json
- CodeDocumentationRoutingFlow.cs with exact callee coverage gate
- CodeDocumentationBatchFileRunner.cs

Acceptance basis:

- 284 methods extracted from local projects.
- 277 cleaned direct-name callgraph edges extracted after excluding common .NET/Object/LINQ/API calls.
- 50 train rows read from TRAIN-PAIRS.jsonl.
- 50 held-out rows generated from actual extracted caller/callee edges.
- Every held-out call callee_symbol_id has a matching known_callees.symbol_id.
- CPU/RAM/timing metrics are real runner observations, not projections.
