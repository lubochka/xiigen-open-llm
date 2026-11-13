# B5 — Vibe coding / expressing intent

**Trend.** "Vibe coding" means generating, suggesting, and refactoring code from a high-level intent in real time — lowering the barrier to building software by letting the developer express what they want rather than how to do it.

**How this project relates.** LlmFromScratch has scaffolds aimed squarely at turning a prompt into a task/plan: `PromptToTaskMachine` is meant to translate a natural-language prompt into a structured task, and `OrchestrationReasoningMachine` is meant to reason about how to orchestrate it. Both implement the `ILearningMachine` contract so they can plug into the same trainable hierarchy as the rest of the stack.

**Where in the code.** `43.SmartSkillsDomain/PromptToTaskMachine.cs:77` (class `PromptToTaskMachine`; its `NotTrainedStatus` is `"not_trained_no_loaded_state"`) and `43.SmartSkillsDomain/OrchestrationReasoningMachine.cs:94` (class `OrchestrationReasoningMachine`; its `NotTrainedStatus` is `"not_trained_no_learned_guide_state"`).

**Orchestration.** Both sit at hierarchy level 5 (`ILearningMachine`, declared in `00.Core/LearningMachineContracts.cs`), so once trained they would expose a runtime coordinator through the contract (`GetOrchestratorAsync`); today they return not-ready before any orchestration happens.

**Honesty boundary.** Both are SKELETONS: they implement the `ILearningMachine` contract but are NOT trained, and they honestly return not-ready (`not_trained_no_loaded_state` / `not_trained_no_learned_guide_state`) until a trained state is loaded. They are scaffolds for future training, not a working vibe-coding feature — do not read them as a shipped capability.
