# SUBPROJECT-MAP.md — LlmFromScratch subproject reference

Last updated: 2026-06-04

## Documentation Navigation

This file is the compact subproject reference. Detailed per-project
implementation documentation is indexed in
[`docs/projects/PROJECT-DOCUMENTATION-INDEX.md`](projects/PROJECT-DOCUMENTATION-INDEX.md).
Use the index to reach the new project docs, including the extension
documentation entries for Claude, Cursor, and Codex.

| # | Dir | Name | What it does | Status | Key files |
|---|---|---|---|---|---|
| 00a | 00.TensorCore | TensorCore | Tensor ops, SIMD (Single Instruction Multiple Data), autograd | Done | TensorOps.cs |
| 00b | 00.Core | Core | Core contracts — ILogicalUnit, ILogicalFlow, ILogicalOrchestrator, ILearningMachine&lt;TDomain&gt;, OperationResult&lt;T&gt; | Active | LogicalContracts.cs, LearningMachineContracts.cs |
| 01 | 01.Tokenizer | Tokenizer | BPE (Byte Pair Encoding) tokenizer | Done | BpeTokenizer.cs |
| 02 | 02.DatasetPipeline | DatasetPipeline | Data loading and batching | Done | DatasetLoader.cs |
| 03 | 03.TransformerModel | TransformerModel | Transformer architecture, attention blocks | Done | TransformerModel.cs |
| 04 | 04.TrainingLoop | TrainingLoop | Training with loss tracking | Done | TrainingLoop.cs |
| 05 | 05.Embeddings | Embeddings | Word embeddings | Done | EmbeddingLayer.cs |
| 06 | 06.VectorSearch | VectorSearch | Local vector store, K-NN (K-Nearest Neighbours) | Done | IVectorStorage |
| 07 | 07.GraphEngine | GraphEngine | Symbol graph + PageRank | Done | SymbolGraph.cs |
| 08 | 08.GraphRag | GraphRag | Graph-based RAG (Retrieval-Augmented Generation) | Done | HybridRetriever.cs |
| 09 | 09.Inference | Inference | TextGenerator, sampler | Done | TextGenerator.cs |
| 10 | 10.LoRA | LoRA | LoRA (Low-Rank Adaptation) fine-tuning | Done | LoraApplicator.cs |
| 11 | 11.DPO | DPO | DPO (Direct Preference Optimization) training | Done | DpoTrainer.cs |
| 12a | 12.SearchEngine | SearchEngine | BM25 (Best Match 25) + K-NN search engine | Done | SearchEngine.cs |
| 12b | 12.SemanticCompiler | SemanticCompiler | Early semantic compiler (superseded by 29) | Superseded | — |
| 13 | 13.CodeOptimizer | CodeOptimizer | Roslyn-based code analysis and optimization | Done | CodeOptimizer.cs |
| 14a | 14.MachineLearning | MachineLearning | Apriori, KMeans, LinearRegression, NaiveBayes, PCA (Principal Component Analysis), Q-Learning | Done | — |
| 14b | 14.ContinuousLearning | ContinuousLearning | LoRA per-project adaptation, feedback routing, incremental training | **Done** | ContinuousLearningPipeline.cs |
| 15 | 15.OrchestratorModel | OrchestratorModel | Episode decomposition, dependency sort, quality checks; registers ProjectWikiOrchestrationFlow | **Done** | OrchestratorFacade.cs, LogicalOrchestratorAssistant.cs, ProjectWikiOrchestrationFlow.cs |
| 16 | 16.AutonomousLearning | AutonomousLearning | Night-time data collection + automated training cycle | **Done** | AutonomousLearningPipeline.cs |
| 17 | — | DynamicDomains | New domain detection, confidence gating | Planned | — |
| 18 | 18.CapabilityTraining | CapabilityTraining | DPO (Direct Preference Optimization) training pipeline, capability trainer, replay mixer, teaching registry | Active | — |
| 21 | 21.AgentInfrastructure | AgentInfrastructure | Agent loop, session storage, file editor, tool registry | Active | FileChangeApplier.cs, AgentLoop.cs |
| 22 | 22.AgentAlgorithms | AgentAlgorithms | Context window, task decomposer, multi-agent orchestrator, DependencyGraph | Active | AgentOrchestrator.cs, DependencyGraph.cs |
| 23 | 23.OssEquivalents | OssEquivalents | MCP (Model Context Protocol) transport, Roslyn analyzer, schema | Active | McpTransport.cs |
| 24 | 24.Compression | Compression | Semantic compression levels | Partial | — |
| 25 | 25.ModelServer | ModelServer | IModelRepository, IRemoteUnitLocator, RemoteUnitManifest, checkpoint versioning | Active | — |
| 26 | 26.TrainableRouter | TrainableRouter | TrainableRoutingFlow, ITrainableRoutingFlow, routing DPO training | Active | — |
| 27 | 27.TranslationDomain | TranslationDomain | RU↔EN and HE↔EN translation training domain; DPO (Direct Preference Optimization) pipeline for SemanticTokenizerUnit, ExecutorUnit, QualityVerifierUnit; language rule matrix; 100+100 training loop. Phase 38 adds model asset lifecycle via 27.TranslationDomain.Tools CLI: export-translation-model, import-translation-model, publish-translation-model, load-translation-model, continue-translation-training. Checkpoint includes real KNN (K-Nearest Neighbours)/trained state, not loop metadata only. | **Active** | TranslationMachine.cs, TranslationBatch029Runner.cs, TranslationSemanticTokenizer.cs, TranslationStructuredBatchBuilder.cs, TranslationTrainedUnits.cs |
| 28 | 28.SemanticNlp | SemanticNlp | NLP (Natural Language Processing) utilities: WordNet, FrameNet, SRL (Semantic Role Labelling) | Partial | — |
| 29 | 29.SemanticCompiler | SemanticCompiler | Full E01-E27 pipeline: AstTokenizer → RouteClassifier → Decomposer → TemplateBeautifier | **Done** | SemanticCompilerPipeline.cs |
| 30 | — | DesignConversionDomain | Design-to-code conversion training domain (not 29 — SemanticCompiler occupies that number) | **Planned** | — |
| 31 | — | AffiliateDomain | Affiliate personal+global machine pair, anonymised sync with global server | **Planned** | — |
| 33 | 33.MatrixAlgorithms | MatrixAlgorithms | Matrix ops for training, Cholesky, LU decomposition | Done | — |
| 35 | 35.ExperimentsV2 | ExperimentsV2 | E01-E50 experiment runner, harness, results tracking | Active | ExperimentRunner.cs |
| 36 | 36.LibraryComparison | LibraryComparison | Component benchmarks vs OSS (Open Source Software) | Active | — |
| 42 | 42.ProjectWikiDomain | ProjectWikiDomain | Project-local code documentation graph, negative DPO candidates, wiki evidence retrieval, logical flow adapter, Bootstrap ILogicalUnit wrappers, and ILogicalOrchestrator plugin path. Phase 37 additions: OrchestrationPlanMachine (implements ILearningMachine&lt;OrchestrationPlanDomain&gt;), PromptFlowGroundingModel (trained subcomponent; 3 modes: Mode1LearnedFlowGrounding, Mode2Generalization, Mode3HumanExplanation), FlowGroundingResult, OrchestrationFlowGroundingWeights, OrchestrationGenerationResult. FlowReferenceDetector is build-time corpus/audit only — not a runtime component. exactPromptLeakageRate must be 0.0 (any eval/train overlap fails gate). | Active | DocumentationOrchestrator.cs, ProjectDocumentationPipelineFlowAdapter.cs, ProjectLocalWikiOrchestrator.cs, OrchestrationUnitContracts.cs, BootstrapOrchestrationUnits.cs, ProjectWikiPluginOrchestrationAdapter.cs |
| 43a | 43.LearningLocalWikiData | LearningLocalWikiData | Project-local wiki training data, DPO orchestration protocol, local wiki learning session storage | Active | — |
| 43b | 43.SmartSkillsDomain | SmartSkillsDomain | Smart skills and AI extension domain. SkillSelectionMachine implements ILearningMachine&lt;SkillSelectionDomain&gt; with IMachineTraining&lt;SkillSelectionDomain&gt; adapter. SmartSkillsSemanticUnitCatalog: 11 trainable semantic units (prompt_understanding, task_understanding, task_type, routing, skill_candidates, skill_selection, prompt_context, result_gap, output_validation, task_completion, intermediate_checks). Thin-client API boundary: XiigenExtensionApiContracts (XiigenExtensionRequest / XiigenExtensionResponse) consumed by three host extension repos. Phase40ExtensionContracts: Phase40ExtensionHostKind, Phase40EvidenceScope, Phase40CoreDecisionSourceKind. QUALITY-02 adds ExtensionEvalOnlyQualityRunner and a thin Quality02EvalOnlyRunner console project for local/private eval-only scoring; this runner requires prompt SHA, baseline-vs-adaptive mode, and `local_private_eval_only` evidence scope before scoring, emits synthetic-vs-approved-eval boundary fields, and keeps semantic, trainable, runtime, token-savings, learnability, target-promotion, locator-update and final-readiness claims false. Also contains: OrchestrationReasoningMachine (skeleton — status: not_trained_no_learned_guide_state; NOT trained), PromptToTaskMachine (skeleton — NOT trained). Training data: corpus-snapshot.jsonl + corpus-pointer.json (NOT .gz — pointer/snapshot pattern). Key files: SkillSelectionMachine.cs, SkillSelectionContracts.cs (contains ServerSkillLibraryRegistry, ProjectLocalSkillState, SkillLibraryUpgradePlanner), SmartSkillsSemanticUnitCatalog.cs, XiigenExtensionApiContracts.cs, Phase40ExtensionContracts.cs, ExtensionEvalOnlyQualityRunner.cs, OrchestrationReasoningMachine.cs. Three external host repos (thin API/check-set clients, NOT in llm_mvp_core): claude-xiigen-smart-skills, codex-xiigen-smart-skills, cursor-xiigen-smart-skills — located in C:\Users\lubao\Documents\xiigen\extensions reasearch\. | Active (Phase 39/40 + QUALITY-02 eval-only runner — 2026-06) | SkillSelectionMachine.cs, SkillSelectionContracts.cs, SmartSkillsSemanticUnitCatalog.cs, XiigenExtensionApiContracts.cs, Phase40ExtensionContracts.cs, ExtensionEvalOnlyQualityRunner.cs, OrchestrationReasoningMachine.cs |

## Dependency graph (key edges)

```
00.Core                (no dependencies — pure interface contracts)

14.ContinuousLearning  →  10.LoRA  →  03.TransformerModel  →  00.TensorCore
                       →  11.DPO
                       →  21.AgentInfrastructure
                       →  29.SemanticCompiler

15.OrchestratorModel   →  21.AgentInfrastructure
                       →  22.AgentAlgorithms  →  21.AgentInfrastructure
                       →  29.SemanticCompiler
                       →  42.ProjectWikiDomain
                       →  00.Core

16.AutonomousLearning  →  14.ContinuousLearning (IncrementalTrainer, DomainModelRegistry, DomainDatasetCache)
                       →  29.SemanticCompiler  (ISemanticCompiler29, E20 gate)
                       →  21.AgentInfrastructure (BaseService, ISessionStorage)

21.AgentInfrastructure →  00.Core

29.SemanticCompiler    →  21.AgentInfrastructure  (BaseService, ISessionStorage)

22.AgentAlgorithms     →  21.AgentInfrastructure

42.ProjectWikiDomain   →  12.SearchEngine (BM25 evidence search)
                       →  08.GraphRag (optional semantic retrieval)
                       →  00.Core

43.SmartSkillsDomain   →  00.Core
                       →  42.ProjectWikiDomain
                       →  25.ModelServer
```

## Implementation notes

- **14.ContinuousLearning** injects `ISemanticCompiler29` from 29 — never reimplements StackDetector (Principle 4)
- **15.OrchestratorModel** reuses `DependencyGraph` from 22 — no duplication (E35)
- **42.ProjectWikiDomain** turns a user documentation request into a code trace, documentation graph, negative DPO rows, project-local evidence index, and wiki answer layer; current training status reports runner-missing rather than claiming trained weights.
- **27.TranslationDomain** (active): implements `ILearningMachine<TranslationDomain>`; four direction adapters RU_EN, EN_RU, HE_EN, EN_HE; language rule matrix with G01–G10 grammar-rule DPO families; `FindNearestCascade` (Full → Prefix → Structure → BootstrapFallback); 44 tests passing as of phase 24 (2026-05-29).
- **30.DesignConversionDomain** (planned): design-to-code conversion learning machine.
- **31.AffiliateDomain** (planned): `ILearningMachinePair<TPersonal, TGlobal>` with anonymised sync to global server; uses 25.ModelServer as the global endpoint. Number 28 is occupied by 28.SemanticNlp.
- **43.SmartSkillsDomain** (active, Phase 39/40): implements `ILearningMachine<SkillSelectionDomain>` + `IMachineTraining<SkillSelectionDomain>`; 11 trainable semantic units; all units have DeterministicFinalAuthority = false; deterministic helpers (BM25 (Best Match 25), regex, if-else) are bootstrap candidate generators only. ExtensionEvalOnlyQualityRunner is a QUALITY-02 eval-only helper that validates/scores local/private rows only after prompt SHA, mode, and `local_private_eval_only` scope gates pass; its reports preserve synthetic-fixture-vs-approved-eval boundaries while keeping product/readiness claims false. OrchestrationReasoningMachine and PromptToTaskMachine are NOT trained — skeletons only, under separate paused plan.
- **models/ directory** (Phase 37–39): `C:\Projects\llm_mvp_core\models\` — model asset registry: `models/orchestration/orchestration-v1/` (manifest.json, checkpoint, runner-report.json), `models/skill-selection/skill-selection-v1/` (manifest.json, checkpoint, runner-report.json), `models/translation/translation-v1/` (manifest.json, checkpoint, runner-report.json), `models/index.json` (registry of all domains). `C:\Projects\llm_mvp_core\training-data\` — training corpus per domain.
- **tools/ gate scripts** (Phase 39/40): `C:\Projects\llm_mvp_core\tools\` — 7 gate scripts: phase40-extension-core-api.ps1, phase40-extension-prompt-suite-gate.ps1, skill-selection-core-adaptation-gate.ps1, skill-selection-core-extension-boundary-gate.ps1, skill-selection-plan-coverage-gate.ps1, xiigen-extension-host-gate.ps1, xiigen-extension-plan-gate.ps1.
- **Working branch:** see current branch; do not assume main — follow branch convention in CLAUDE.md
- **Test baseline** (2026-05-29): ≥ 1488 passed, 0 failed (gate tests confirmed; TD1–TD3 added 9 new tests; 27.TranslationDomain suite: 44 passed as of phase 24). Phase 37 added 44 orchestration tests (OrchestrationPlanMachine + PromptFlowGroundingModel). Phase 38 added 6 translation model asset lifecycle tests. Phase 39 added smart-skills tests (SkillSelectionMachine, 11 semantic units). Phase 39/40 added 10 skeleton tests for unplanned machines: OrchestrationReasoningMachine (OrchestrationReasoningContractsTests, OrchestrationReasoningMachineSkeletonTests, OrchestrationReasoningStateLoadingTests) and PromptToTaskMachine (PromptToTaskContractsTests, PromptToTaskMachineSkeletonTests, PromptToTaskMachineStateLoadingTests, PromptToTaskToOrchestrationReasoningHandoffTests). NOTE: OrchestrationReasoningMachine and PromptToTaskMachine are NOT in ORCHESTRATION-MACHINE-TRAINING-PLAN-v20 — they are implemented per ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-STATE.json (separate plan, paused for operator input). Live test count re-verification required after Phase 37-40 additions.
