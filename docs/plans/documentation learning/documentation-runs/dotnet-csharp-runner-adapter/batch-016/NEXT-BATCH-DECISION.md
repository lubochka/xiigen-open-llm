# Next Batch Decision

Decision: PROCEED_TO_BATCH_017
Current batch: dotnet-csharp-runner-adapter/batch-016
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-016 verified Excel/Elastic pipeline documentation transit.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- bottom_up_order_accuracy=1.000
- callee_summary_transit_accuracy=1.000
- local_vs_delegated_accuracy=1.000
- CPU ms=437.5
- peak RAM MB=35.711

Next batch focus:

1. Add mixed framework contrast: net46 WebApi2 vs netcoreapp3.1 API that look similar at controller level.
2. Ensure version-aware documentation does not collapse them into one generic C# web stack.
3. Continue exact 50 train + 50 held-out runner verification.
