# Next Batch Decision

Decision: PROCEED_TO_BATCH_012
Current batch: dotnet-csharp-runner-adapter/batch-011
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-011 corrected the weak batch-010 submetrics by requiring accepted callee summaries on D4 workflow rows.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- bottom_up_order_accuracy=1.000
- callee_summary_transit_accuracy=1.000
- local_vs_delegated_accuracy=1.000
- CPU ms=437.5
- peak RAM MB=35.219

Next batch focus:

1. Mix positive D4 transit with controlled source/version negatives.
2. Keep all negatives explicit in train pairs with functional [WRONG:] consequences.
3. Preserve local legacy projects as validation-only evidence, not training authority.
