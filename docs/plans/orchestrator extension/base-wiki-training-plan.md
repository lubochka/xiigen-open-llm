# Base Wiki Training Plan

Status: SESSION-01 OUTPUT - BASE MODEL NOT YET ACCEPTED.

This document defines how to train `xiigen-small-doc-wiki-base`. It is not a
project-local adapter plan and it must not include facts from `askapediaweb`,
`GenieDynamicServer`, `wizGameserver`, `genietasks`, or any other customer or
validation project.

## Base Model Id

`xiigen-small-doc-wiki-base`

Purpose:

- learn generic documentation Q/A behavior;
- learn citation discipline;
- learn missing-evidence abstention;
- learn stack/version awareness as a generic answer contract;
- learn wrong-project rejection without memorizing any project-specific facts.
- learn query-planning discipline: turn a developer question into ordered
  evidence searches before composing an answer.

The query-planning behavior is the base capability for
`ProjectWikiQueryPlanningFlow`; it is trained generically before project-local
validation.

## Approved Source Families

Allowed for the base model:

- generic documentation contracts authored for the documentation-learning
  system;
- accepted open-license documentation examples that are already registered in
  the documentation source registry;
- synthetic generic wiki pairs only when they are explicitly labeled as
  `SYNTHETIC_BASE_WIKI_SEED` and do not claim external source authority;
- internal runner contract examples that describe answer shape, evidence shape,
  and abstention behavior without project-local facts.

Blocked for the base model:

- source code facts from local validation projects;
- customer code or documentation without explicit training approval;
- project-local `.xiigen/wiki-domain/` evidence;
- stale documentation outputs that have not passed the documentation acceptance
  gate;
- rows without `[WRONG: ...]` in the rejected answer.

## Train And Held-Out Split

Early training uses the same diagnostic rhythm as the documentation and
orchestration streams:

```text
one learning step = 50 train pairs + 50 disjoint held-out questions
```

The 50/50 split is a diagnostic ratio for the first base wiki phase. It checks
whether the model learns the answer contract, citation requirement, and
abstention behavior before any project facts are introduced. After the base
model passes at least 10 stable steps, the split can move to a larger training
share, but only after hallucination remains zero and citation accuracy remains
above the gate.

## Pair Taxonomy

Each base wiki batch should balance these pair kinds:

| Kind | Chosen behavior | Rejected behavior |
|---|---|---|
| `generic_symbol_answer` | Explain one documented symbol from supplied evidence. | Invent behavior from the symbol name. |
| `generic_caller_answer` | Explain caller-to-callee meaning from accepted summaries. | Skip the callee meaning. |
| `generic_workflow_answer` | Explain a workflow using ordered evidence ids. | Produce a top-down workflow without evidence. |
| `generic_query_plan` | Produce a multi-step evidence query plan before answering. | Do one direct lookup or answer before planning required evidence. |
| `generic_entity_impact_plan` | Plan DTO/entity, mapper, validator, caller, storage, output, and test lookups for change-impact questions. | Search only the DTO/entity name. |
| `generic_feature_answer` | Answer feature-level question only from accepted docs. | Describe an ideal architecture instead of observed behavior. |
| `citation_required` | Include evidence ids and documentation/source paths. | Answer without evidence ids. |
| `missing_evidence_abstain` | State that evidence is missing and refuse to invent. | Guess a confident answer. |
| `wrong_project_reject` | Refuse or route when the question belongs to another project. | Answer from the wrong project. |
| `stale_documentation_reject` | Block answer when source/documentation version is stale. | Treat stale docs as current truth. |
| `stack_version_preserve` | Preserve target framework and language version from evidence. | Assume modern .NET or a different stack. |
| `wrong_level_reject` | Match answer depth to requested level. | Give method-level answer for workflow/system-level prompt. |

Every rejected answer must contain a specific `[WRONG: ...]` annotation that
states the weakness and the consequence.

## Runner Command

Current DPO runner:

```powershell
dotnet run --project "C:\Projects\llm_mvp_core\experiments-v2\INTERNAL-TRAIN-READY-V191\runner\InternalTrainReadyDpoRunner\InternalTrainReadyDpoRunner.csproj" -- --source "<base-wiki-train-jsonl>" --run-id "<base-wiki-run-id>" --context 96 --batch-size 16 --save-every 25
```

Observed runner contract:

- accepts `id` or defaults to a generated row id;
- reads `prompt` first, then falls back to `a_side`;
- requires non-empty `chosen`;
- requires non-empty `rejected`;
- writes DPO training metrics, CPU percent, working set delta, elapsed time, and
  checkpoints.

Important limitation:

The runner trains DPO rows and reports training metrics. It does not perform
held-out natural-language Q/A generation, citation verification, or abstention
evaluation. A separate base wiki Q/A evaluator must read the held-out rows and
write `base-wiki-heldout-evaluation.json`.

## Expected Metrics

Training metrics recorded by the DPO runner:

- loss;
- DPO accuracy;
- reward margin;
- elapsed ms per batch;
- CPU percent per batch;
- working set delta MB per batch;
- checkpoint bytes.

Acceptance metrics required by the base wiki evaluator:

- `query_plan_accuracy >= 0.90`;
- `required_touchpoint_recall >= 0.95`;
- `multi_hop_sequence_accuracy >= 0.90`;
- `generic_documentation_qa_accuracy >= 0.90`;
- `citation_format_accuracy >= 0.98`;
- `missing_evidence_abstention_accuracy >= 0.95`;
- `hallucination_rate == 0.00`;
- answer contract includes project id, documentation version, evidence ids, and
  confidence;
- no project-specific facts are required to answer.

## Acceptance Gate

The base model is accepted only when both conditions are true:

1. DPO runner completes successfully on the base wiki train file and writes a
   checkpoint.
2. Held-out Q/A evaluator passes every acceptance metric above.

Until both are true:

- `base-wiki-model-manifest.json.status` stays
  `BASE_MODEL_DEFINED_NOT_ACCEPTED`;
- no project-local wiki adapter may be trained;
- SESSION-02 must not begin.

## Artifact Output Paths

Expected SESSION-01 outputs:

```text
C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator extension/base-wiki-model-manifest.json
C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator extension/base-wiki-training-plan.md
C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator extension/base-wiki-heldout-evaluation.json
C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator extension/base-wiki-acceptance-report.md
```

Expected future runner outputs:

```text
C:/Projects/llm_mvp_core/experiments-v2/INTERNAL-TRAIN-READY-V191/results/<base-wiki-run-id>/
```

Expected future model repository publication:

```text
IModelRepository.UploadAsync(GeneratedCapabilityCheckpointManifest manifest, string serverBaseUrl, CancellationToken ct = default)
```

This corrects the earlier prose assumption that `UploadAsync` takes a direct
policy checkpoint path. The current code publishes through
`GeneratedCapabilityCheckpointManifest`.

## No Project-Local Facts

The base model must not memorize local project behavior. Project facts belong in
project-local adapters and evidence indexes. The base model should learn only
how to answer from evidence, cite it, abstain when evidence is missing, and
reject wrong-project or stale-document answers.
