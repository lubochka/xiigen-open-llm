# QUALITY-SEMANTIC-GATES-24

Status: PASS_QSG24_REAL_DPO_MATERIAL_SOURCE_DISJOINT_EVAL_RECORDED_NO_SEMANTIC_OR_LEARNABILITY_CLAIM

QSG-24 records real/reference DPO and held-out material for PromptToTaskMachine and SkillSelectionMachine. It replaces QSG-23 synthetic-only controls with a source manifest, normalized chosen/rejected rows, quarantine ledger, source-disjoint split, and material-only held-out ablation.

## What this proves

- Real/reference PromptToTask and SkillSelection DPO/eval rows are present and normalized with chosen, rejected, error code, source ref, provenance, and training route.
- Train-candidate source document ids are disjoint from held-out source document ids after quarantined rows are excluded from train authority.
- Stack-specific and leakage-prone material is routed to quarantine/evidence metadata instead of universal training authority.
- The evaluator is explicitly material-only and not a model-runtime proof.
- Production model, checkpoint, locator, and index refs were not mutated.

## What this does not prove

- Semantic quality is not claimed.
- Learnability is not claimed.
- Token savings, quality improvement, common readiness, production promotion, production rollback closure, and final readiness remain closed.
- QSG-24 did not run real continue-training, fresh-load a newly trained target, or evaluate a trained checkpoint.

## Counts

- Accepted material/eval rows: 50
- Quarantined rows: 12
- PromptToTask held-out rows: 4
- SkillSelection held-out rows: 12

## Next QSG step

QUALITY-SEMANTIC-GATES-25: create or approve train-ready source/license-cleared PromptToTask and SkillSelection DPO rows, run real continue-training from an existing staged/imported checkpoint, fresh-load the trained target, and run source-disjoint held-out plus ablation against the trained checkpoint before any semantic quality or learnability claim.
