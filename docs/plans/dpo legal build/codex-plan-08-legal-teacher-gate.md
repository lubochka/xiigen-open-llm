# Codex Plan 08 - Legal Teacher Gate

Artifact: `codex-plan-08-legal-teacher-gate.md`
Status: PLAN-ONLY, Codex-owned.
Encoding recovery note: recovered after cycle-42 status-sync encoding fault.

## Authority Anchor

```text
CURRENT TASK: Preserve legal teacher/provenance planning and finish 50 real review cycles.
NOT THE TASK: No implementation, tests, builds, git, training, Claude-file edits, or false cycle claims.
EVIDENCE-ONLY: Claude benchmark and old packets.
```

## Canon

Teachers for distillation/training must be open-weight permissive or otherwise explicitly legal. Claude, OpenAI, and Gemini are never training teachers. They may be judges only when `used_for_training=false`.

Closed proprietary model used for training means `DATASET_REBUILD_REQUIRED`.

`TeacherProvenance` has 18 fields: `TeacherId`, `ModelName`, `ModelVersion`, `Provider`, `ProviderType`, `LicenseId`, `LicenseUrl`, `LicenseSnapshotRef`, `WeightsAvailability`, `TrainingUseAllowed`, `UsedForTraining`, `UsedForJudgingOnly`, `ClosedProprietaryModel`, `TermsSnapshotRef`, `GeographyExportConstraint`, `SourceCategory`, `RawPrivateTextCopied`, `RedactionPolicyRef`.

`SourceCategory` values are: local, human, open_weight, hosted, deterministic_baseline, self_judge, client_unverified, third_party.

`LegScope` values are: CommonGlobal, PersonalAdaptive, EvalOnly, Quarantine.

Verdicts: ACCEPT, BLOCK_FOR_HOSTED, BLOCK_FOR_COMMON, DATASET_REBUILD_REQUIRED, QUARANTINE, EVAL_ONLY.

Hosted raw/private misuse without redaction proof blocks common/global training. Judge-only closed models can be accepted only when not used for training and no raw private text is copied into train-ready rows.

## Gate B

- `legal-provenance-manifest.json` contains only ACCEPT rows for train-ready data.
- `self_judge_train_ready_without_legal_gate==0`.
- `triad_field_presence_rate==1.0`.
- `train_ready_without_triad_count==0`.
- Q37/Q38 remain decision refs in body; actual questions stay only in final sections.
- Current comparison-run packet records: 50/50 after C49-C50 returned and integrated.

## Final Questions Only

1. For FLOW-38 better-output, can the chosen side ever originate from hosted/private feedback without forcing `PersonalAdaptive` or `Quarantine`?
2. What exact evidence makes a hosted source eligible for `CommonGlobal` rather than `PersonalAdaptive`?
