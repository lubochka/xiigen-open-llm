# P11 evidence card

- Verdict: PARTIAL_PASS_WITH_PROVENANCE_REBUILD_REQUIRED.
- Components checked: 2.
- Component pass count: 1.
- Provenance rebuild required: 1.
- PromptToTask: project `LoadCheckpointAsync` path plus `ImportAsync` exercised; held-out sample passed.
- OrchestrationReasoning: project `LoadCheckpointAsync` boundary reached; checkpoint rejected with `ORCHESTRATION_REASONING_CHECKPOINT_SOURCE_PROVENANCE_REBUILD_REQUIRED`.
- Hashes: checkpoint bytes match P10 manifest hashes.
- Safety: no training, no DPO authoring, no checkpoint overwrite, no production code change.
- Claims: full API semantic quality, SkillSelection readiness, extension runtime readiness, and final model readiness remain false.
