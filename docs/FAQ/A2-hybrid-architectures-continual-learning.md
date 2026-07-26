# How does LlmFromScratch handle continual learning and catastrophic forgetting?

LlmFromScratch handles it with a real from-scratch LoRA adapter mechanism (`10.LoRA`) wrapped in two continual-learning pipelines — but honestly: the pipelines are scaffolding whose incremental / idle-time training step is not yet wired to a live model, so this is honest continual-learning plumbing, not a solved lifelong-learning system.

LlmFromScratch approaches continual learning with a real from-scratch low-rank adapter wrapped in two pipelines. `10.LoRA/LoraApplicator.cs:17` (`LoraApplicator.Apply`) freezes the base parameters and returns LoRA adapters for the attention layers — a genuine from-scratch mechanism in the verified-trained list. `14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs:22` runs the per-project cycle (index -> cache -> feedback DPO pairs -> train -> register) and `16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs:24` adds idle detection plus a promote-only-if-improved gate. Honesty boundary: LoRA is real and implemented, but the incremental training step at `14.ContinuousLearning/Training/IncrementalTrainer.cs:55` is not wired to a live model — it records the intended `{domain}.lora` path and returns rather than running the gradient update. Trained adapters are held per domain by an in-memory `DomainModelRegistry` rather than a persistent store. The whole stack is pure C# with no cloud dependency, which is the edge / on-device angle. So this is real low-rank machinery plus honest continual-learning scaffolding, not a solved lifelong-learning system.

### Where is this in the code?

Continual learning is itself pipeline-driven: `ContinuousLearningPipeline` and `AutonomousLearningPipeline` are the orchestrators, and trained adapters are held per domain by an in-memory `DomainModelRegistry` (`14.ContinuousLearning/Registry/DomainModelRegistry.cs:14`). Publishing and re-loading accepted checkpoints through `25.ModelServer` (`IModelRepository`) is the project's model-server contract, but these two pipelines are not wired to it — that is a separate path that is not yet connected.

### What is the industry trend?

Current research pushes continual (lifelong) learning to counter catastrophic forgetting, edge/on-device distillation, and hybrid architectures that keep learning after deployment.

### Honesty boundary

LoRA is a real, implemented from-scratch mechanism (in the verified list: DpoTrainer, LoraApplicator, Trainer, TranslationMachine). The continual / autonomous pipelines (`14.ContinuousLearning`, `16.AutonomousLearning`) provide the pipeline plumbing; the actual incremental / idle-time training step is not yet wired to a live model — it records the intended adapter path rather than running the gradient update — and the pipelines are not connected to `25.ModelServer`. So this answer does not claim more than the verified list: it is real low-rank machinery plus honest continual-learning scaffolding, not a solved lifelong-learning system and not a claim that the continual loop trains end-to-end today.

Last updated: 2026-07-24
