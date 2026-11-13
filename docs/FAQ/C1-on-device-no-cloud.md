# C1 — OpenClaw / on-device, no cloud

**Trend.** A 2025-2026 consumer trend is on-device personal AI assistants (for example OpenClaw) that run entirely on the user's own machine and connect to many services without sending data to the cloud.

**How this project relates.** LlmFromScratch is local-first by construction: it is a full LLM / ML stack written from scratch in pure C# with no cloud dependency, and models are loaded locally through `25.ModelServer`.

**Where in the code.** `25.ModelServer/Contracts/IModelRepository.cs:11` declares `IModelRepository.LoadWeightsAsync` with a local fallback path, and `25.ModelServer/Local/LocalModelRepository.cs:13` is the local weight-loading implementation. The whole stack is from scratch in pure C# (Rule 10 — no external ML frameworks), so nothing here reaches out to a hosted service.

**Orchestration.** Local model loading is a repository/service concern, not one of the hierarchy levels: a caller invokes `IModelRepository.LoadWeightsAsync` and `LocalModelRepository` returns weights from the local path with no network call; those loaded weights are what the rest of the stack (for example the `OrchestratorFacade` pipeline) then runs on. Local model loading does not touch the `ILearningMachine` / `ILogicalOrchestrator` / `ILogicalFlow` / `ILogicalUnit` contract hierarchy levels (declared in `00.Core/LogicalContracts.cs` and `00.Core/LearningMachineContracts.cs`).

**Honesty boundary.** The on-device, no-cloud PRINCIPLE matches, but this is a local ML/LLM engineering stack, NOT an OpenClaw-style consumer assistant with messaging-app integrations. There are no WhatsApp / Telegram / Slack (or any other external service) connectors in this repository — the only claim is local-first model loading, not a personal-assistant product.
