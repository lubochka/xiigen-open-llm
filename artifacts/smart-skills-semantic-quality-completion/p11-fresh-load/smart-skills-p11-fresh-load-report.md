# P11 fresh-load report

## Verdict
PARTIAL_PASS_WITH_PROVENANCE_REBUILD_REQUIRED.

P11 checks the two component checkpoint candidates listed by P10. PromptToTask completes the fresh-load lifecycle. OrchestrationReasoning is not accepted as loaded lifecycle evidence because the current source-admission gate requires a provenance rebuild for its legacy schema-v1 common checkpoint.

## Component facts
- PromptToTask: checkpoint hash matches P10, `LoadCheckpointAsync` and `ImportAsync` are available and exercised, and the held-out P7G sample produces the expected component task contract.
- OrchestrationReasoning: checkpoint hash matches P10, `LoadCheckpointAsync` is reached through the project path, and loading fails closed with `ORCHESTRATION_REASONING_CHECKPOINT_SOURCE_PROVENANCE_REBUILD_REQUIRED`.

## Limits
This is supported-component lifecycle evidence only. It does not claim a two-component lifecycle pass, full API semantic quality, SkillSelection readiness, extension runtime readiness, final model readiness, training, checkpoint overwrite, or DPO authoring.
