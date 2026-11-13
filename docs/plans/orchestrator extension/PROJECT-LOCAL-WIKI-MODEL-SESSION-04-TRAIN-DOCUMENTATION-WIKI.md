# SESSION-04 - Train Documentation Wiki

Status: EXECUTION SESSION.

Do not start this session until SESSION-03 has passed its stop gate.

This is Phase 1. It trains the local documentation wiki from accepted
documentation DTOs, relation graph rows, and validated negative documentation
DTOs. It does not create or answer user questions.

Phase 1 does not require semantic retrieval. It may use Mode 3 lexical evidence
lookup through `12.SearchEngine/SearchEngineFacade` for citation validation, but
it must not require `GraphRag.HybridRetriever` or `EmbeddingModel` until Phase 2
question answering.

## Goal

Train and evaluate the project-local documentation wiki adapter, then publish
the local documentation unit as a candidate `ProjectLocalWikiOrchestrator`.

## Required Reads

1. `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md`
2. `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md`
3. `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md`
4. SESSION-01 `base-wiki-model-manifest.json`
5. SESSION-03 `ProjectCodeTrace`
6. SESSION-03 `ProjectDocumentationKnowledgeGraph`
7. SESSION-03 `.xiigen/wiki-domain/training/batch-001/documentation-train-50.jsonl`
8. SESSION-03 `.xiigen/wiki-domain/heldout/batch-001/documentation-heldout-50.jsonl`
9. SESSION-03 `.xiigen/wiki-domain/training/batch-001/negative-dto-validation-ledger.jsonl`
10. Selected project's `.xiigen/wiki-domain/relations/documentation-units.jsonl`
11. Selected project's `.xiigen/wiki-domain/relations/call-edges.jsonl`
12. Selected project's `.xiigen/wiki-domain/relations/value-flow-edges.jsonl`
13. `ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md`
14. `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md`
15. `EXECUTOR-DISCIPLINE-RULES-v1.md`
16. `EXECUTOR-STAGE-PROMPT-v2.md`
17. `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`
18. Selected project's `.xiigen/wiki-domain/model-manifest.json`
19. `C:\Projects\llm_mvp_core\25.ModelServer\Contracts\IModelRepository.cs`
20. `C:\Projects\llm_mvp_core\25.ModelServer\Contracts\IRemoteUnitLocator.cs`
21. `C:\Projects\llm_mvp_core\25.ModelServer\Local\LocalModelRepository.cs`
21. `C:\Projects\llm_mvp_core\12.SearchEngine` contracts for lexical evidence validation

## Runner

Current runner:

```powershell
dotnet run --project "C:\Projects\llm_mvp_core\experiments-v2\INTERNAL-TRAIN-READY-V191\runner\InternalTrainReadyDpoRunner\InternalTrainReadyDpoRunner.csproj" -- --source "<documentation-train-jsonl>" --run-id "<documentation-wiki-run-id>" --context 96 --batch-size 16 --save-every 25
```

If `42.ProjectWikiDomain` later provides a dedicated documentation wiki runner,
it must expose the same minimum metrics: documentation DTO accuracy, relation
edge accuracy, negative DTO rejection accuracy, citation accuracy, CPU, RAM,
elapsed time, and adapter artifact path.

## Held-Out Evaluation

Evaluate against `documentation-heldout-50.jsonl`.

Required metrics:

- `documentation_unit_accuracy`;
- `meaning_accuracy`;
- `parameter_accuracy`;
- `caller_accuracy`;
- `return_accuracy`;
- `callee_accuracy`;
- `argument_mapping_accuracy`;
- `return_usage_accuracy`;
- `relation_edge_accuracy`;
- `negative_dto_rejection_accuracy`;
- `citation_accuracy`;
- `hallucination_rate`;
- `cpu_ms_per_row`;
- `elapsed_ms_per_row`;
- `peak_ram_mb`;
- `adapter_size_mb`.
- code-trace deltas:
  - methods_documented_delta;
  - call_edges_covered_delta;
  - value_flow_edges_covered_delta;
  - missing_transit_edges_remaining;
  - missing_workflow_methods_remaining.

## Model Publication

- publish weights through the current code signature
  `LocalModelRepository.UploadAsync(GeneratedCapabilityCheckpointManifest manifest, string serverBaseUrl, CancellationToken ct = default)`
  unless a remote artifact repository is configured;
- write artifact reference and checksum into `model-manifest.json`;
- register/locate via `IRemoteUnitLocator`;
- add the unit as a candidate for Phase 2 project wiki routing;
- do not commit weight files to git.

## Stop Gate

Stop after the training run either produces an accepted checkpoint or writes the
explicit status `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING`.

Only then write or update:

```text
.xiigen/wiki-domain/eval/documentation-heldout-evaluation.json
.xiigen/wiki-domain/eval/documentation-training-run-report.json
.xiigen/wiki-domain/model-manifest.json
```

Do not create Phase 2 question-bank rows in this session.
