# Next Batch Decision

Decision: PROCEED_TO_BATCH_002
Current batch: dotnet-csharp-runner-adapter/batch-001
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- 50 train rows passed hard pair gates.
- 50 local legacy held-out rows passed routing/version/transit gates.
- runner-002 metrics: routing_accuracy=1.000, version_gate_accuracy=1.000, transit_gate_accuracy=1.000.
- CPU/RAM were measured by the runner.

Next batch focus:

1. Add harder negative DPO around wrong .NET version routing: net46 vs netcoreapp3.1 vs net8.0.
2. Add explicit caller-before-callee rejected examples for D3/D4.
3. Keep local projects as held-out validation only unless source authority is explicitly registered.
4. Continue harvesting only clean external rows; mojibake and weak rows stay in repair-required.
