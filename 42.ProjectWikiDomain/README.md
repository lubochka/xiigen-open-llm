# 42.ProjectWikiDomain

Project-local documentation and wiki domain for .NET projects.

This project owns the path from a user documentation request to a verified project wiki answer:

```text
user request
-> ProjectDocumentationPipelineFlowAdapter
-> ProjectDocumentationPipelineOrchestrator
-> DocumentationOrchestrator
-> ProjectWikiTrainingOrchestrator
-> StackAndVersionClassifier
-> CodeTraceBuilder
-> DocumentationGenerator
-> NegativeDtoDocumentationOrchestrator
-> ProjectEvidenceIndex
-> ProjectWikiQueryPlanningFlow
-> WikiContextRetriever
-> ProjectLocalWikiOrchestrator
-> ProjectAnswerVerifier
```

## Current Status

The current implementation builds and verifies the documentation graph and project-local wiki answer layer. It includes 50 train / 50 held-out training gates, raw CPU/RAM/time metrics, manifest publication, Section 22 runner-report artifacts, and locator registration paths for the general project-wiki units listed below. Runner reports must mark unavailable reward/loss metrics as `UNKNOWN`; they must not invent them from tests or manifests. It does not claim that project-local trained weights exist inside every client repository yet. When the documentation graph passes coverage and no project-local trainer has published weights, the public flow reports:

```text
DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING
```

## File Map

| File | Responsibility |
|---|---|
| `ProjectWikiDomainModels.cs` | Shared request, trace, evidence, documentation, DPO, answer, and coverage records. |
| `DocumentationOrchestrator.cs` | Reads .NET project files, builds code trace, documentation units, call edges, value-flow edges, question seeds, and optional `.xiigen/wiki-domain/` snapshots. |
| `StackAndVersionClassifierUnit.cs` | Classifies .NET runtime family, target framework, C# language version, and SDK/non-SDK project style. |
| `StackAndVersionDpoGenerator.cs` / `StackAndVersionTrainingRunner.cs` / `StackAndVersionTrainingPipeline.cs` | Trainable 50-row stack/version classification gate with held-out metrics and manifest publication. |
| `CodeTraceBuilderUnit.cs` | Extracts method symbols, caller-callee edges, and value-flow edges for bottom-up documentation. |
| `CodeTraceBuilderDpoGenerator.cs` / `CodeTraceBuilderTrainingRunner.cs` / `CodeTraceBuilderTrainingPipeline.cs` | Trainable 50-row code-trace gate with method, call-edge, and value-flow recall. |
| `EntityImpactMapperUnit.cs` | Builds entity touchpoint maps for developer change-impact questions. |
| `EntityImpactMapperDpoGenerator.cs` / `EntityImpactMapperTrainingRunner.cs` / `EntityImpactMapperTrainingPipeline.cs` | Trainable 50-row entity-impact gate with evidence and touchpoint recall. |
| `DocumentationGeneratorUnit.cs` | Generates complete documentation text from accepted documentation units. |
| `DocumentationGeneratorDpoGenerator.cs` / `DocumentationGeneratorTrainingRunner.cs` / `DocumentationGeneratorTrainingPipeline.cs` | Trainable 50-row documentation-generation gate with required-field coverage. |
| `NegativeDtoDocumentationOrchestrator.cs` | Builds wrong-but-plausible rejected DPO candidates with `[WRONG: ...]` annotations from accepted documentation units. |
| `NegativeDpoGeneratorBatchBuilder.cs` / `NegativeDpoGeneratorTrainingRunner.cs` / `NegativeDpoGeneratorTrainingPipeline.cs` | Trainable 50-row negative-DPO generator gate. |
| `ProjectEvidenceIndex.cs` | Loads or builds evidence records and performs lexical BM25 retrieval through `12.SearchEngine`. |
| `ProjectWikiQueryPlanningFlow.cs` | Converts developer questions into an ordered wiki query plan before retrieval, including entity-impact, workflow-trace, sufficiency, missing-evidence, stale-version, and wrong-project checks. |
| `ProjectWikiQueryPlanningDpoGenerator.cs` | Builds the first validated 50-row DPO batch surface for training the query-planning flow, with multi-step chosen plans and `[WRONG: ...]` rejected plans. |
| `ProjectWikiQueryPlanningTrainingRunner.cs` | Runs the 50 train / 50 held-out gate for query-planning rows and reports raw accuracy, CPU, RAM, and elapsed time metrics. |
| `ProjectWikiQueryPlanningManifestPublisher.cs` | Publishes a passed query-planning gate as a checkpoint manifest through `25.ModelServer` and registers the remote unit locator entry. |
| `ProjectWikiQueryPlanningTrainingPipeline.cs` | Executes the no-stop unit loop for one query-planning batch: build 50 train rows, build 50 held-out rows, evaluate, publish if passed, then return the next action. |
| `WikiContextRoutingUnit.cs` / `WikiContextRoutingDpoGenerator.cs` / `WikiContextRoutingTrainingPipeline.cs` | Trainable route selection between Mode 3 lexical evidence, Mode 2 semantic hybrid, abstention, wrong-project rejection, and stale-document rejection. |
| `ProjectWikiAnswerComposerDpoGenerator.cs` / `ProjectWikiAnswerComposerTrainingRunner.cs` / `ProjectWikiAnswerComposerTrainingPipeline.cs` | Trainable answer-composition gate for grounded answers with citations, source paths, and developer reasoning notes. |
| `ProjectAnswerVerifier.cs` | Verifies that answers cite known evidence and remain inside the requested project. |
| `ProjectAnswerVerifierDpoGenerator.cs` / `ProjectAnswerVerifierTrainingRunner.cs` / `ProjectAnswerVerifierTrainingPipeline.cs` | Trainable 50-row verifier gate for invalid citations, wrong project answers, and missing-evidence abstention. |
| `WikiTraceAggregatorUnit.cs` | Aggregates `WikiQueryTrace` and `WikiUnitTrace` into failed-unit, remediation, and stop-blind-training decisions. |
| `WikiTraceAggregatorDpoGenerator.cs` / `WikiTraceAggregatorTrainingRunner.cs` / `WikiTraceAggregatorTrainingPipeline.cs` | Trainable 50-row trace-aggregation gate. |
| `ProjectWikiTrainingOrchestrator.cs` | Stage 0 training entry point: routes natural executor training prompts to the correct trainable unit pipeline, can read the learning asset registry, and can name the next weak unit before any new batch starts. |
| `ProjectWikiTrainingFlowAdapter.cs` | Registers the training orchestrator as a logical flow so the main orchestrator can run unit training from a natural request, writes the Section 22 runner-report artifact for real training-flow runs, and supports registry-only next-action planning without creating a batch. |
| `ProjectWikiRealProjectValidationRunner.cs` | Validates real project wiki domains with generated evidence questions plus developer-reasoning entity-impact questions, using `WikiQueryTrace` metrics for accuracy, CPU, RAM, and elapsed time. |
| `WikiContextRetriever.cs` | Chooses project-local context retrieval mode: Mode 3 lexical BM25 by default, optional Mode 2 semantic retrieval through `08.GraphRag`. |
| `ProjectLocalWikiModelUnit.cs` | Composes deterministic evidence-bound answers until trained project-local weights are available. |
| `ProjectLocalWikiOrchestrator.cs` | Exposes project-local wiki question answering as an `ILogicalOrchestrator`. |
| `ProjectWikiQuestionSeedGenerator.cs` | Generates meaning, location, parameter, return, caller, callee, workflow, why, and multi-hop questions from evidence. |
| `ProjectWikiCoverageVerifier.cs` | Runs coverage checks over project wiki evidence and generated question types. |
| `ProjectDocumentationPipelineOrchestrator.cs` | Runs the end-to-end documentation graph pipeline from one orchestrator request. |
| `ProjectDocumentationPipelineFlowAdapter.cs` | Registers the pipeline as a trainable logical flow entry point for `15.OrchestratorModel`. |
| `ProjectWikiDomainServiceCollectionExtensions.cs` | Factory methods for loading wiki orchestrators from persisted wiki domains or in-memory graphs, plus the training orchestrator entry point. |

## Integration Points

- `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs` registers `ProjectDocumentationPipelineFlowAdapter` and `ProjectWikiTrainingFlowAdapter`.
- `25.ModelServer` publishes passed query-planning checkpoints through `IModelRepository` and `IRemoteUnitLocator`.
- `12.SearchEngine` provides lexical BM25 evidence search.
- `08.GraphRag` can provide semantic retrieval when an active `HybridRetriever` is supplied.
- `Tests/ProjectWikiDomainTests.cs` is the executable acceptance surface.
