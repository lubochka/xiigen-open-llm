# Next Batch Decision

Decision: PROCEED_TO_BATCH_013
Current batch: dotnet-csharp-runner-adapter/batch-012
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-012 verified controlled negative gates without degrading documentation-unit routing.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- bottom_up_order_accuracy=1.000
- callee_summary_transit_accuracy=1.000
- local_vs_delegated_accuracy=1.000
- CPU ms=250
- peak RAM MB=33.996

Next batch focus:

1. Add more local server/client mixed edges: AngularJS controller -> Breeze/WebApi2 endpoint -> EF save/query.
2. Keep source and version negatives balanced but not dominant.
3. Continue 50-pair train plus 50-row held-out runner verification.
