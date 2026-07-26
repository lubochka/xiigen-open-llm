# How does LlmFromScratch reflect the shift from coding to orchestration?

LlmFromScratch is organized around orchestration rather than hand-written control flow: `15.OrchestratorModel` turns a task string into a quality-checked answer through an 8-step pipeline, and `42.ProjectWikiDomain` routes a documentation request through a logical-orchestrator path. The orchestration structure is real, but the trained wiki model behind it does not exist yet.

LlmFromScratch is organized around orchestration: `15.OrchestratorModel/OrchestratorFacade.cs:48` is an 8-step pipeline (classify -> decompose -> stop-filter -> sort -> launch -> merge -> quality -> store) that turns a task string into a quality-checked answer, and `15.OrchestratorModel/Logical/ProjectWikiOrchestrationFlow.cs:21` routes a documentation request through an `ILogicalOrchestrator` / `ILogicalFlow` path instead of hand-written control flow. This is the developer-role shift made concrete: intent enters as a task string, and the orchestrated pipeline — not per-case hand-written code — classifies, decomposes, runs, merges, quality-checks, and stores the result. The structure is real, but the trained wiki model behind it does not exist yet: `42.ProjectWikiDomain` honestly reports `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING` with `training_status = "runner_missing"` (`42.ProjectWikiDomain/ProjectDocumentationPipelineFlowAdapter.cs:52/73`), and when no orchestrator is configured the flow falls back to a direct `SubmitUserTaskApi` call (`BOOTSTRAP_FALLBACK`). So this is real orchestration structure with honestly-not-yet-trained wiki grounding — not a claim that the documentation model is trained.

### What is the industry trend?

**Trend.** As agents take over line-by-line coding, the developer's role shifts toward orchestrating AI processes and verifying their results rather than hand-writing every line.

### Where is this in the code?

`ProjectWikiOrchestrationFlow` sits at hierarchy level 3 (`ILogicalFlow<string>`) and delegates to a level-4 `ILogicalOrchestrator` (the contract hierarchy is declared in `00.Core/LogicalContracts.cs`); the general task path is the level-4 orchestration chain in `OrchestratorFacade.ProcessAsync` (`15.OrchestratorModel/OrchestratorFacade.cs`).

### Honesty boundary

The orchestration structure is real, but the trained wiki model behind it does not exist yet: `42.ProjectWikiDomain` honestly reports `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING` with `training_status = "runner_missing"` (`42.ProjectWikiDomain/ProjectDocumentationPipelineFlowAdapter.cs:52/73`), and when no orchestrator is configured the flow falls back to a direct `SubmitUserTaskApi` call (`BOOTSTRAP_FALLBACK`). So this is real orchestration structure with honestly-not-yet-trained wiki grounding — not a claim that the documentation model is trained.

Last updated: 2026-07-24.
