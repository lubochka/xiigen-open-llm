# Next Batch Decision

Decision: FIRST_1000_ROW_STREAM_COMPLETE
Current batch: dotnet-csharp-runner-adapter/batch-020
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-020 completed 1000 train rows + 1000 held-out runner-verified rows across batches 001-020.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- bottom_up_order_accuracy=1.000
- callee_summary_transit_accuracy=1.000
- local_vs_delegated_accuracy=1.000
- CPU ms=265.625
- peak RAM MB=34.086

Next stream recommendation:

1. Start a second documentation stream focused on actual extracted call graph ordering instead of hand-authored held-out chains.
2. Keep local projects validation-only unless their source authority is explicitly registered.
3. Keep the 50 train + 50 held-out runner gate unchanged.
