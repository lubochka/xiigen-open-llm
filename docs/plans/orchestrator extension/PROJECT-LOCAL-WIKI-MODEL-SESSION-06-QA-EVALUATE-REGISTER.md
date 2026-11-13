# SESSION-06 - Q/A Evaluate, Verify, Register

Status: EXECUTION SESSION.

Do not start this session until SESSION-05 has passed its stop gate.

## Goal

Train the project-local wiki adapter, evaluate held-out Q/A, verify citations,
publish the manifest through model-server infrastructure, and register the wiki
unit as a routing candidate.

This session is for Phase 2 Q/A behavior. It must not call the Phase 1
`NegativeDtoDocumentationOrchestrator` and must not generate negative DTO rows.

## Required Reads

1. `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md`
2. SESSION-01 `base-wiki-model-manifest.json`
3. SESSION-05 row validation ledger
4. SESSION-05 `.xiigen/wiki-domain/training/batch-001/wiki-qa-train-50.jsonl`
5. SESSION-05 `.xiigen/wiki-domain/heldout/batch-001/wiki-heldout-50.jsonl`
6. Selected project's `.xiigen/wiki-domain/model-manifest.json`
7. Selected project's `.xiigen/wiki-domain/evidence/evidence-index.jsonl`
8. Selected project's `.xiigen/wiki-domain/question-bank/question-bank-manifest.json`
9. SESSION-05 `.xiigen/wiki-domain/question-bank/question-selection-ledger-batch-001.jsonl`
10. `C:\Projects\llm_mvp_core\25.ModelServer\Contracts\IModelRepository.cs`
11. `C:\Projects\llm_mvp_core\25.ModelServer\Contracts\IRemoteUnitLocator.cs`
12. `C:\Projects\llm_mvp_core\25.ModelServer\Local\LocalModelRepository.cs`
13. `C:\Projects\llm_mvp_core\26.TrainableRouter\Contracts\ITrainableRoutingFlow.cs`
14. `C:\Projects\llm_mvp_core\12.SearchEngine` contracts for `SearchEngineFacade`
15. `C:\Projects\llm_mvp_core\08.GraphRag` contracts for `GraphRag.HybridRetriever`
16. The connected embedding/model project, either project 07 or project 09 in the
    current local tree, to locate the active `EmbeddingModel` dependency
17. `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md`
18. `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md`
19. `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md`
20. `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`
21. `EXECUTOR-DISCIPLINE-RULES-v1.md`
22. `ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md`
23. `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md`
24. `EXECUTOR-STAGE-PROMPT-v2.md`

## Runner

Current runner:

```powershell
dotnet run --project "C:\Projects\llm_mvp_core\experiments-v2\INTERNAL-TRAIN-READY-V191\runner\InternalTrainReadyDpoRunner\InternalTrainReadyDpoRunner.csproj" -- --source "<project-wiki-train-jsonl>" --run-id "<wiki-run-id>" --context 96 --batch-size 16 --save-every 25
```

## Citation Verification

`ProjectAnswerVerifier` must check:

- every answer has a preceding `ProjectWikiQueryPlan`;
- the query plan includes the required evidence categories for the question
  type;
- every cited evidence id exists in `evidence-index.jsonl`;
- every evidence id belongs to the same `project_id`;
- the answer's documentation version equals `domain-manifest.documentation_version`;
- answer text does not use another project's source path;
- missing-evidence questions abstain.

20-real-question pass gate:

- 20 of 20 answers stay inside the correct project;
- 20 of 20 answers include valid evidence ids or valid abstention;
- at least 19 of 20 answers match expected answer type;
- at least 19 of 20 query plans match expected plan shape;
- 0 hallucinated citations;
- 0 stale-document acceptances.
- question utility updates are written back to the question bank for every
  held-out question.

Every evaluated question must produce `WikiQueryTrace`. Every batch must
produce `WikiUnitTrace`. No unit may fail without a remediation action.

## Implementation Artifacts

Create or update the project slot:

```text
C:\Projects\llm_mvp_core\42.ProjectWikiDomain\42.ProjectWikiDomain.csproj
```

Required components in that project:

- `ProjectLocalWikiOrchestrator`
- `ProjectWikiQueryPlanningFlow`
- `WikiContextRetriever`
- `ProjectLocalWikiModelUnit`
- `ProjectEvidenceIndex`
- `ProjectAnswerVerifier`
- `ProjectDocumentationPipelineFlowAdapter`
- `ProjectWikiCoverageVerifier`
- `WikiTraceAggregator`
- `ProjectWikiDomainServiceCollectionExtensions`

Create or update these implementation files explicitly:

- `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\ProjectLocalWikiOrchestrator.cs`
- `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\ProjectWikiQueryPlanningFlow.cs`
- `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\WikiContextRetriever.cs`
- `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\ProjectLocalWikiModelUnit.cs`
- `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\ProjectEvidenceIndex.cs`
- `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\ProjectAnswerVerifier.cs`
- `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\ProjectDocumentationPipelineFlowAdapter.cs`
- `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\ProjectWikiCoverageVerifier.cs`
- `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\WikiTraceAggregator.cs`
- `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\ProjectWikiDomainServiceCollectionExtensions.cs`

`ProjectLocalWikiOrchestrator` owns the call sequence:

```text
ProjectWikiQueryPlanningFlow -> WikiContextRetriever -> ProjectLocalWikiModelUnit -> ProjectAnswerVerifier
```

`ProjectWikiQueryPlanningFlow` owns the investigation plan. `WikiContextRetriever`
executes that plan through Mode 2 or Mode 3. `ProjectEvidenceIndex` is an
internal dependency of `WikiContextRetriever` for Mode 3 evidence lookup. No
caller outside the orchestrator should directly bypass this sequence or call
`ProjectEvidenceIndex` as a pre-orchestrator service.

## Retrieval Mode Contract

`WikiContextRetriever` must expose two explicit modes:

```text
Mode 3 lexical evidence lookup
  -> SearchEngineFacade from 12.SearchEngine
  -> BM25 over .xiigen/wiki-domain/evidence/evidence-index.jsonl
  -> no embeddings required

Mode 2 semantic project wiki retrieval
  -> GraphRag.HybridRetriever from 08.GraphRag
  -> active EmbeddingModel from project 07 or 09, whichever is connected
  -> relation graph + BM25 evidence hits
```

Hard rule: `12.SearchEngine` is lexical BM25 only. Do not claim it provides
vector embeddings. If `EmbeddingModel` is missing, Mode 2 must return
`semantic_retrieval_unavailable` and either use Mode 3 evidence-only context or
abstain.

## Model Publication

- publish weights through the current code signature
  `LocalModelRepository.UploadAsync(GeneratedCapabilityCheckpointManifest manifest, string serverBaseUrl, CancellationToken ct = default)`
  unless a remote artifact repository is configured;
- write artifact reference and checksum into `model-manifest.json`;
- register/locate via `IRemoteUnitLocator`;
- add the unit as a candidate for `ProjectWikiRoutingFlow`;
- do not commit weight files to git.

## Stop Gate

Stop after writing or updating:

```text
.xiigen/wiki-domain/eval/batch-001-heldout-evaluation.json
.xiigen/wiki-domain/eval/batch-001-query-plan-evaluation.json
.xiigen/wiki-domain/eval/real-question-verification.json
.xiigen/wiki-domain/traces/batch-001/wiki-query-traces.jsonl
.xiigen/wiki-domain/traces/batch-001/wiki-unit-trace.json
.xiigen/wiki-domain/question-bank/question-utility-updates-batch-001.jsonl
.xiigen/wiki-domain/model-manifest.json  # update existing Phase 1 manifest; do not recreate from scratch
```

Do not start batch-002 until this session is reviewed.


