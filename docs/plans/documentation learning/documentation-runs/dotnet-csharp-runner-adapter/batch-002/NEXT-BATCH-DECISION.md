# Next Batch Decision

Decision: PROCEED_TO_BATCH_003
Current batch: dotnet-csharp-runner-adapter/batch-002
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- runner-003 passed 50/50 after JSON array normalization.
- routing_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000
- Batch includes harder negative gates for wrong runtime/language version and missing callee summaries.

Next batch focus:

1. Add D4 workflow-level rows where multiple D3 callers depend on shared D1/D2 leaves.
2. Add WebForms/ASMX-specific held-out cases from genietasks/quantaWiz style legacy projects.
3. Add rejected examples where documentation starts at controller workflow before leaf utilities.
