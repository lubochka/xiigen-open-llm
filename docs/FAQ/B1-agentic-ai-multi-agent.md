# B1 — Agentic AI / multi-agent / Agent HQ

**Trend.** The 2025-2026 shift is away from copilots that only suggest code toward agents that explore, act, and iterate on their own — often as multi-agent systems where several specialized agents coordinate on one task.

**How this project relates.** LlmFromScratch has a real multi-agent coordinator: it decomposes a task into roles, orders those roles by their dependencies, launches the independent ones in parallel, and forwards each agent's output as context to the agents that depend on it.

**Where in the code.** `22.AgentAlgorithms/MultiAgent/AgentOrchestrator.cs:18` runs decompose -> dependency levels -> parallel launch via `Task.WhenAll` -> context forwarding; `22.AgentAlgorithms/MultiAgent/DependencyGraph.cs:14` is a Kahn topological sort that groups roles into parallel levels; `21.AgentInfrastructure/Loop/AgentLoop.cs:25` is a ReAct-like agent loop with a tool registry.

**Orchestration.** `AgentOrchestrator` is the multi-agent orchestrator: `DependencyGraph` computes the parallel levels, `Task.WhenAll` runs each level, and results are threaded forward as context to later levels. This machinery also backs the `sort -> launch` stage of the 8-step `OrchestratorFacade` pipeline (`15.OrchestratorModel/OrchestratorFacade.cs`).

**Honesty boundary.** This is real, concrete multi-agent coordination — a deterministic Kahn topological sort plus parallel execution, implemented as concrete classes (`AgentOrchestrator`, `DependencyGraph`), NOT through the `ILogicalOrchestrator` interface level of the contract hierarchy declared in `00.Core/LogicalContracts.cs`. It is orchestration machinery, not a trained policy that learns how to split work: the decomposition into levels and the ordering are computed deterministically, not learned.
