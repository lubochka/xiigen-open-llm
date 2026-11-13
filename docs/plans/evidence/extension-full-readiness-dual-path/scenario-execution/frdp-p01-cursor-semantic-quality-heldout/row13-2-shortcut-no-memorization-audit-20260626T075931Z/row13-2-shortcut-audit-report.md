# Row13.2 Shortcut / No-Memorization Audit

Status: ROW13_2_NEEDS_PERMUTATION_HARNESS
Generated UTC: 2026-06-26T07:59:32.3346186Z

## Scope

Audited the staged Row13 candidate and Row13.1c 4/4 result for hidden deterministic shortcuts and memorization risk using only the allowed artifacts and scoped SkillSelectionMachine / decision code reads.

No training, checkpoint mutation, locator update, promotion, live Cursor UI run, staging, commit, or push was performed.

## Inputs

- Checkpoint: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-1b-evidence-only-adapter-train-export-20260626T073726Z\staged-candidate\skill-selection-checkpoint.json
- Row13.1c report: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-1c-heldout-rerun-staged-candidate-20260626T074556Z\row13-1c-heldout-rerun-report.json
- Row13.1c results: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-1c-heldout-rerun-staged-candidate-20260626T074556Z\row13-1c-scenario-results.jsonl
- Row13.1a training rows: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-1a-universal-only-dpo-rows-20260626T070919Z\row13-1a-universal-only-dpo-rows.jsonl

## Decision

Row13.3 should not start yet.

Reason: Row13.1c 4/4 is candidate-position confounded in the available evidence. All 8 training rows place the chosen skill at candidate index 0, all 4 Row13.1c selected held-out skills are also at candidate index 0, and the allowed Row13.1c artifacts do not contain prompt text or serialized SkillSelectionRequest needed for an independent permutation rerun.

## Required Missing Harness

Need serialized request per held-out row, or allowed access to the held-out source rows containing prompt text, plus a read-only runner that:

1. Loads the staged checkpoint.
2. Calls SkillSelectionMachine.DecideFromTrainedState for original, reversed, and rotated AvailableSkillIds.
3. Writes only Row13.2 evidence.
4. Performs no training/export/checkpoint/locator/model mutation.

## Key Findings

- R13_2_001 high_evidence_gap: NEEDS_PERMUTATION_HARNESS.
- R13_2_002 high_candidate_position_confound: training chosen index 0 = 8/8; eval selected index 0 = 4/4; checkpoint candidatePositionWeights has position 0 weight 8.
- R13_2_003 medium_stored_provenance_not_scoring_authority_by_code_read: pair ids, prompt key hashes, and sourcePairIds are stored, but inspected final-selection scoring uses prompt-token feature weights over available skill IDs.
- R13_2_004 medium_prompt_key_provenance_caveat: no direct ID or prompt-hash overlap found in available fields, but checkpoint trainedPromptKeys do not match hashes recomputed from the provided Row13.1a Prompt fields; prompt-key provenance is not independently traceable from the allowed training JSONL alone.
- R13_2_005 medium_exact_prompt_memorization_not_fully_auditable: exact eval prompt text is unavailable, so exact prompt memorization cannot be fully audited from the allowed artifacts.
- R13_2_006 low_overclaim_locks: overclaim locks remain false in the reports read.

## Overclaim Locks

- productReadinessClaimed: False
- tokenSavingsClaimed: False
- fullSemanticQualityClaimed: False
- commonModelReadinessClaimed: False
- locatorUpdateClaimed: False

## Evidence Boundary

The evidence supports only this Row13.2 shortcut/memorization audit over the listed artifacts plus scoped code reading. It does not verify product readiness, token savings, full semantic quality, common model readiness, locator update, extension readiness, or Row13.3 live Cursor UI duplicate readiness.
