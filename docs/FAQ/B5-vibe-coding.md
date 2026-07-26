# Does LlmFromScratch support vibe coding and natural-language intent?

Not yet as a working feature. LlmFromScratch has scaffolds aimed at turning a prompt into a task/plan — `PromptToTaskMachine` and `OrchestrationReasoningMachine` — but both `43.SmartSkillsDomain` machines are SKELETONS (not trained): they implement the `ILearningMachine` contract and honestly return not-ready until a trained state is loaded, so this is not a shipped vibe-coding capability.

LlmFromScratch has scaffolds aimed squarely at turning a prompt into a task/plan: `PromptToTaskMachine` is meant to translate a natural-language prompt into a structured task, and `OrchestrationReasoningMachine` is meant to reason about how to orchestrate it. Both implement the `ILearningMachine` contract so they can plug into the same trainable hierarchy as the rest of the stack. In the code they are `43.SmartSkillsDomain/PromptToTaskMachine.cs:77` (class `PromptToTaskMachine`; its `NotTrainedStatus` is `"not_trained_no_loaded_state"`) and `43.SmartSkillsDomain/OrchestrationReasoningMachine.cs:94` (class `OrchestrationReasoningMachine`; its `NotTrainedStatus` is `"not_trained_no_learned_guide_state"`). Both are SKELETONS: they implement the contract but are NOT trained, and they honestly return not-ready until a trained state is loaded. There is no working path today that turns a free-form natural-language intent into a running plan; the intent-to-task and orchestration-reasoning steps both return not-ready before anything executes. They are scaffolds for future training, not a working vibe-coding feature — do not read them as a shipped capability.

### What is the industry trend?

**Trend.** "Vibe coding" means generating, suggesting, and refactoring code from a high-level intent in real time — lowering the barrier to building software by letting the developer express what they want rather than how to do it.

### Where is this in the code?

Both sit at hierarchy level 5 (`ILearningMachine`, declared in `00.Core/LearningMachineContracts.cs`), so once trained they would expose a runtime coordinator through the contract (`GetOrchestratorAsync`); today they return not-ready before any orchestration happens.

### Honesty boundary

Both are SKELETONS: they implement the `ILearningMachine` contract but are NOT trained, and they honestly return not-ready (`not_trained_no_loaded_state` / `not_trained_no_learned_guide_state`) until a trained state is loaded. They are scaffolds for future training, not a working vibe-coding feature — do not read them as a shipped capability.

Last updated: 2026-07-24.
