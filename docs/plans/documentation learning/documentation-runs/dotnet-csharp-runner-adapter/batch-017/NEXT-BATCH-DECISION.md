# Next Batch Decision

Decision: PROCEED_TO_BATCH_018
Current batch: dotnet-csharp-runner-adapter/batch-017
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-017 verified that similar controller shapes stay separated by .NET generation and framework version.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- bottom_up_order_accuracy=1.000
- callee_summary_transit_accuracy=1.000
- local_vs_delegated_accuracy=1.000
- CPU ms=312.5
- peak RAM MB=35.289

Next batch focus:

1. Add generated/vendor/source-root mixed negatives with framework-contrast positives.
2. Ensure no source-root exclusions are mistaken for version failures.
3. Continue exact 50 train + 50 held-out runner verification.
