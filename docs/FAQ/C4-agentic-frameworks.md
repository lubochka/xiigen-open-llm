# Is LlmFromScratch an agentic framework like CrewAI or AutoGen?

LlmFromScratch has its own from-scratch agent-composition layer: a contract hierarchy `ILogicalUnit` -> `ILogicalFlow` -> `ILogicalOrchestrator` -> `ILearningMachine` that composes units and flows into orchestrated routes. It is a real C# CODE framework, NOT a visual no-code builder, and a from-scratch library, not a drop-in replacement for CrewAI or AutoGen.

LlmFromScratch has its own from-scratch agent-composition layer: a contract hierarchy `ILogicalUnit` -> `ILogicalFlow` -> `ILogicalOrchestrator` -> `ILearningMachine` that lets units and flows be composed into orchestrated routes. `00.Core/LogicalContracts.cs:102` (`ILogicalUnit<TInput,TOutput>`), `:175` (`ILogicalFlow<TInput>`), `:211` (`ILogicalOrchestrator`), and `00.Core/LearningMachineContracts.cs:213` (`ILearningMachine<TDomain>`) define the hierarchy; `22.AgentAlgorithms` provides the concrete multi-agent coordinator that actually fans work out. The concrete runtime coordinator is `22.AgentAlgorithms/MultiAgent/AgentOrchestrator` plus `DependencyGraph`, a deterministic Kahn topological sort into parallel levels. The honest boundary: this is a real code-level contract hierarchy for composing agents and flows — analogous in spirit to those frameworks — but it is a C# CODE framework, NOT a visual no-code builder, and a from-scratch library, not a drop-in replacement for CrewAI or AutoGen. The contracts themselves are real (the interfaces and request/result types exist and are used); this is not a claim that the units plugged into them are trained.

### What is the industry trend?

Agent frameworks lead current adoption — both visual no-code builders (Langflow, Dify, Flowise) and multi-agent coordinators (MetaGPT, CrewAI, AutoGen).

### Where is this in the code?

`00.Core/LogicalContracts.cs:102` (`ILogicalUnit<TInput,TOutput>`), `:175` (`ILogicalFlow<TInput>`), `:211` (`ILogicalOrchestrator`), and `00.Core/LearningMachineContracts.cs:213` (`ILearningMachine<TDomain>`) define the hierarchy; `22.AgentAlgorithms` provides the concrete multi-agent coordinator that actually fans work out.

These four contracts ARE the composition hierarchy (declared in `00.Core/LogicalContracts.cs` and `00.Core/LearningMachineContracts.cs`): a level-2 `ILogicalUnit` is composed by a level-3 `ILogicalFlow`, coordinated by a level-4 `ILogicalOrchestrator`, and a level-5 `ILearningMachine` exposes its own orchestrator through `GetOrchestratorAsync`. The concrete runtime coordinator is `22.AgentAlgorithms/MultiAgent/AgentOrchestrator` + `DependencyGraph` (a deterministic Kahn topological sort into parallel levels); note that this coordinator is concrete classes (`AgentOrchestrator`, `DependencyGraph`), NOT the `ILogicalOrchestrator` interface level of that hierarchy.

### Honesty boundary

This is a real code-level contract hierarchy for composing agents and flows — analogous in spirit to those frameworks — but it is a C# CODE framework, NOT a visual no-code builder, and it is a from-scratch library, not a drop-in replacement for CrewAI / AutoGen. The contracts themselves are real (the interfaces and request/result types exist and are used); this is not a claim that the units plugged into them are trained.

Last updated: 2026-07-24.
