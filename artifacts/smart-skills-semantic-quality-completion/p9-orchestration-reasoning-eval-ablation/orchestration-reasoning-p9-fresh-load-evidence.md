# P9: fresh-load evidence

P8B checkpoint был загружен через проектный код: `OrchestrationReasoningMachine.LoadCheckpointAsync`.

- Loaded patterns: 3.
- Accepted train rows: `P5E-DPO-ORCH-001`, `P5E-DPO-ORCH-002`, `P5E-DPO-ORCH-004`.
- Forbidden rows are absent from checkpoint training input.
- `semantic_quality_claimed=false`.
- No checkpoint rewrite was performed.

Focused command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~OrchestrationReasoningP9EvalAblationTests" --logger "console;verbosity=minimal"
```

Result: PASS, 1 focused test.
