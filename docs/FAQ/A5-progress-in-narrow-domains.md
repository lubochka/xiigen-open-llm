# How does LlmFromScratch compare to narrow-domain systems like AlphaGeometry?

LlmFromScratch offers real narrow-domain engineering — one genuinely trained RU<->EN translation model plus deterministic domain pipelines (a semantic compiler and a Roslyn code optimizer) — not superhuman theorem-proving. Unlike AlphaGeometry or AlphaProof it makes no competition-level or theorem-proving claim; only `27.TranslationDomain` is actually trained, and only for Russian<->English.

LlmFromScratch has several domain-specific pipelines whose honesty status differs. `29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs:18` is a deterministic pipeline (experiments E01-E27) that compiles input into predicates; `13.CodeOptimizer/Pipeline/OptimizationPipeline.cs:34` runs deterministic Roslyn AST passes; and `27.TranslationDomain/TranslationMachine.cs:12` is a genuinely trained `ILearningMachine` with a real train / evaluate / export / import lifecycle — in the verified-trained list. It declares four translation directions (RU->EN, EN->RU, HE->EN, EN->HE) in code. The four directions are declared in `27.TranslationDomain/TranslationContracts.cs:20` (`enum TranslationDirection { RU_EN, EN_RU, HE_EN, EN_HE }`) and wired in `27.TranslationDomain/TranslationMachine.cs:559-601`, but the training and held-out data shipped is for Russian<->English only (`27.TranslationDomain/SourceData/sentence-index-ru-en-pilot-100.jsonl`, `27.TranslationDomain/TrainingData/tr-ho-ru-en-product-heldout-v1.jsonl`); there is no Hebrew training or held-out data file anywhere in the repository, so the Hebrew<->English directions are declared, wired adapters rather than data-backed trained pairs. Honesty boundary: this is real narrow-domain engineering — a trained translation model plus deterministic domain pipelines — not superhuman theorem-proving, and it makes no AlphaProof-style claim.

### Where is this in the code?

The semantic compiler is orchestrated as a fixed E01-E27 pipeline (`SemanticCompilerPipeline`); the translation machine exposes its runtime coordinator through the `ILearningMachine` contract (`GetOrchestratorAsync`), so it plugs into the same hierarchy as the rest of the stack.

### What is the industry trend?

Systems such as AlphaGeometry, FunSearch, and AlphaProof show that narrow, well-specified domains can beat hand-written search and even reach competition-level results.

### Honesty boundary

Translation is a genuinely trained `ILearningMachine` (in the verified-trained list); the semantic compiler and the code optimizer are deterministic (not trained models). The machine declares four directions in code, but the training / held-out data shipped is for Russian<->English — the Hebrew<->English directions exist as declared, wired adapters rather than data-backed trained pairs in this repository. This is real narrow-domain engineering — a trained translation model plus deterministic domain pipelines — not superhuman theorem-proving, and it makes no AlphaProof-style claim.

Last updated: 2026-07-24
