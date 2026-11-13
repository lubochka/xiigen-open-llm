# Next Batch Decision

Decision: PROCEED_TO_BATCH_014
Current batch: dotnet-csharp-runner-adapter/batch-013
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- Batch-013 verified mixed client/server documentation routing across AngularJS, Breeze, WebApi2, and net46 metadata.
- routing_accuracy=1.000
- source_gate_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- bottom_up_order_accuracy=1.000
- callee_summary_transit_accuracy=1.000
- local_vs_delegated_accuracy=1.000
- CPU ms=328.125
- peak RAM MB=34.758

Next batch focus:

1. Add ASP.NET Core dynamic SQL/data-access documentation edges.
2. Keep exact target frameworks and C# language versions explicit.
3. Preserve 50 train + 50 held-out runner verification.
