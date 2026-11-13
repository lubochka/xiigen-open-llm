# SESSION-01 - Base Wiki Model Definition And Validation

Status: EXECUTION SESSION.

Do not execute later sessions from this file.

## Goal

Define, build, and validate `xiigen-small-doc-wiki-base` before any
project-local wiki adapter is trained.

## Required Reads

1. `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md`
2. `PROJECT-LOCAL-WIKI-MODEL-STATE.json`
3. `C:\Projects\llm_mvp_core\25.ModelServer\Remote\RemoteUnitManifest.cs`
4. `C:\Projects\llm_mvp_core\25.ModelServer\Contracts\IModelRepository.cs`
5. `C:\Projects\llm_mvp_core\26.TrainableRouter\Contracts\ITrainableRoutingFlow.cs`
6. `EXECUTOR-DISCIPLINE-RULES-v1.md`
7. `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md`
8. `EXECUTOR-STAGE-PROMPT-v2.md`
9. `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`

## Outputs

1. `base-wiki-model-manifest.json`
2. `base-wiki-training-plan.md`
3. `base-wiki-heldout-evaluation.json`
4. `base-wiki-acceptance-report.md`

`base-wiki-training-plan.md` must contain: base model id, approved source
families, train/held-out split, pair taxonomy, runner command, expected metrics,
acceptance gate, artifact output paths, and a statement that no project-local
facts are included in the base model.

## Runner

Use the current DPO runner unless a dedicated base wiki runner exists:

```powershell
dotnet run --project "C:\Projects\llm_mvp_core\experiments-v2\INTERNAL-TRAIN-READY-V191\runner\InternalTrainReadyDpoRunner\InternalTrainReadyDpoRunner.csproj" -- --source "<base-wiki-train-jsonl>" --run-id "<base-wiki-run-id>" --context 96 --batch-size 16 --save-every 25
```

## Acceptance Gate

- generic documentation Q/A accuracy >= 0.90;
- query plan accuracy >= 0.90;
- required touchpoint recall >= 0.95;
- citation format accuracy >= 0.98;
- missing-evidence abstention accuracy >= 0.95;
- hallucination rate == 0.00;
- answer contract includes project id, documentation version, evidence ids, and
  confidence;
- no project-specific facts are required to pass this base gate.

## Stop Gate

Stop after writing the acceptance report.

Do not create project-local `.xiigen/wiki-domain/` folders in this session.

