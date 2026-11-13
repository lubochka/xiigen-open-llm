# SUBPROJECT-MAP.md — LlmFromScratch subproject reference

| # | Name | What it does | Status | Key files |
|---|---|---|---|---|
| 00 | TensorCore | Tensor operations, SIMD (Single Instruction Multiple Data) | Done | TensorOps.cs |
| 01 | Tokenizer | BPE (Byte Pair Encoding) tokenizer | Done | BpeTokenizer.cs |
| 02 | DatasetPipeline | Data loading and batching | Done | DatasetLoader.cs |
| 03 | TransformerModel | Transformer architecture | Done | TransformerModel.cs |
| 04 | TrainingLoop | Training with loss tracking | Done | TrainingLoop.cs |
| 05 | Embeddings | Word embeddings | Done | EmbeddingLayer.cs |
| 06 | VectorSearch | Local vector store | Done | IVectorStorage |
| 07 | GraphEngine | Symbol graph + PageRank | Done | SymbolGraph.cs |
| 08 | GraphRag | Graph-based RAG (Retrieval-Augmented Generation) | Done | HybridRetriever.cs |
| 09 | Inference | TextGenerator | Done | TextGenerator.cs |
| 10 | LoRA | LoRA (Low-Rank Adaptation) fine-tuning | Done | LoraLayer.cs |
| 11 | DPO | DPO (Direct Preference Optimization) training | Done | DpoTrainer.cs |
| 12 | SemanticCompiler (planned) | E01-E27 pipeline: compress, route, plan, translate, beautify | Planned | — |
| 13 | VisualStudioExtension (planned) | VS VSIX + chat UI | Planned | — |
| 14 | ContinuousLearning (planned) | LoRA per-project adapt | Planned | — |
| 15 | OrchestratorModel (planned) | Episode decomposition | Planned | — |
| 16 | AutonomousLearning (planned) | Night-time data + train | Planned | — |
| 17 | DynamicDomains (planned) | New domain detection | Planned | — |
| 21 | AgentInfrastructure | Agent loop, indexer, editor | Active | FileChangeApplier.cs |
| 22 | AgentAlgorithms | Context, decomposer, orchestrator | Active | ContextWindowManager.cs |
| 23 | OssEquivalents | MCP (Model Context Protocol), Roslyn analyzer, schema | Active | McpTransport.cs |
| 24 | Compression | Semantic compression levels | Partial | — |
| 28 | SemanticNlp | NLP (Natural Language Processing) utilities | Partial | — |
| 33 | MatrixAlgorithms | Matrix ops for training | Done | — |
| 35 | ExperimentsV2 | E01-E50 experiment runner | Active | ExperimentRunner.cs |
| 36 | LibraryComparison | Component benchmarks vs OSS (Open Source Software) | Active | — |
