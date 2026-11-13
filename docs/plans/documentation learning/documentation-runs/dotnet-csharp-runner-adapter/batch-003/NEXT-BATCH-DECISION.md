# Next Batch Decision

Decision: PROCEED_TO_BATCH_004
Current batch: dotnet-csharp-runner-adapter/batch-003
Status: RUNNER_VERIFIED_ACCEPTED

Why:

- D4 workflow-level held-out passed.
- routing_accuracy=1.000
- version_gate_accuracy=1.000
- transit_gate_accuracy=1.000

Next batch focus:

1. Add WebForms/Page_Load and ASMX service documentation routing.
2. Add old ASP.NET MVC/WebApi2 controller route cases where System.Web evidence matters.
3. Increase false-positive negatives: project name suggests domain, but source code proves generic server/controller utility.
