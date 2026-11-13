# Eval-only gates

Run: `quality-controls-user-adaptive-eval-policy-20260618-090220`

Эта фаза не запускает eval. Она определяет gates, без которых будущий user-local eval-only runner не имеет права стартовать.

1. `G1_CURRENT_APPROVAL`: свежий approval на user-local eval-only, точные scenario/pair ids, approved user-local root, no external AI/web unless separately authorized, no training/model/checkpoint/locator mutation.
2. `G2_PRIVACY_REDACTION`: raw private input stays user-local; repo gets only redacted shared evidence refs; privacy class and redaction status are recorded.
3. `G3_SOURCE_SEPARATION`: every row keeps `USER_ADAPTIVE_ONLY_EVAL_ONLY_LATER`; common training remains false; rows are not mixed into common train/held-out sets.
4. `G4_NO_COMMON_MUTATION_LOCK`: common sets, common checkpoints, common manifests and repo fixtures remain unchanged except redacted evidence artifacts.
5. `G5_NO_TRAINING_MUTATION_LOCK`: training, continue-training, DPO training, model/checkpoint/target/locator mutation remain false.
6. `G6_EVIDENCE_LABELS`: output says `local_private_eval_only` and includes "what this proves / what this does not prove".
7. `G7_EVIDENCE_ONLY_EXCLUSION`: 5 evidence-only scenarios and 33 pair rows are excluded from eval/training inputs.
8. `G8_ROLLBACK_SAFETY_IF_TRAINING_LATER_AUTHORIZED`: if a later separate training approval exists, it must name base/target/rollback checkpoint shas, backup/export/import, fresh-load, held-out/ablation and locator mutation approval before promotion. Eval-only evidence cannot become rollback or promotion proof.

Current verdict: policy defined, no eval executed.

Claim locks stay closed: token savings, quality improvement, semantic readiness, learnability, live-host readiness, common readiness, training-ready, strict `QUALITY-03`, target promotion, locator update.
