# Do the agents in LlmFromScratch check their own output?

Yes, but through a deterministic quality gate — 2 of 16 named checks, no retry — not a trained self-correction model. In LlmFromScratch, after the orchestrator merges sub-agent results it passes the output to `QualityOrchestrator`, which applies a fixed pass/fail check; only output that clears the gate is stored and returned.

LlmFromScratch runs every merged result through a deterministic quality gate before it is returned. After the orchestrator merges the sub-agent results, it hands the output to `QualityOrchestrator`, defined at `15.OrchestratorModel/QualityOrchestrator.cs:13`; its `Check` method (lines 34-51) is invoked as step 7 of the pipeline from `15.OrchestratorModel/OrchestratorFacade.cs:109`. This self-check is the `quality` step of the orchestration chain, near the end, just before storage. Only output that clears the gate is stored and returned. Honesty boundary: this is a deterministic quality gate that runs two real checks — output length and presence of a `return` statement — with no retry loop, not a trained self-correction model. In the code only 2 of the 16 named checks actually run, and on a failed check the facade immediately returns `Fail("QUALITY_FAILED")` with no retry. It does not learn from its mistakes; it re-applies the same fixed checks every time an agent result comes back.

### Where is this in the code?

The pipeline is an 8-step chain in `OrchestratorFacade.ProcessAsync` (`15.OrchestratorModel/OrchestratorFacade.cs:48`): classify -> decompose -> stop-filter -> sort -> launch -> merge -> quality -> store. The self-check is the `quality` step near the end.

### What is the industry trend?

A recurring 2025-2026 theme is agents that check their own output before returning it, so that a mistake in one step does not compound across the whole chain.

### Honesty boundary

This is a deterministic quality gate that runs two real checks (output length, and presence of a `return` statement) with no retry loop, not a trained self-correction model; the broader 16-check-with-retry design in older docs is not what the runtime executes today. In the code only 2 of the 16 named checks actually run, and on a failed check the facade immediately returns `Fail("QUALITY_FAILED")` (no retry). It does not learn from its mistakes — it re-applies the same fixed checks.

Last updated: 2026-07-24
