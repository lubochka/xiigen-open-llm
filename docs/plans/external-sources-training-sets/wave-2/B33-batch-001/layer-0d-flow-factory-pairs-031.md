# Layer 0-D — Flow Factory Pairs (031)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-031-001 — GapHandler: hyperparameter standalone

A: [] | "The hyperparameter search takes too long with grid search."
Factory: GapHandler | Gap ID: gap-ai-ml-training-001
Reason: "hyperparameter" = AI training parameter. No code verb.

---

## FF-031-002 — IMPLEMENT: build hyperparameter tuning service

A: ["IMPLEMENT"] | "Build a hyperparameter tuning service that wraps the training job API."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "hyperparameter tuning service" = software artifact.

---

## FF-031-003 — SECURITY: secure hyperparameter endpoint

A: ["SECURITY"] | "Secure the hyperparameter configuration endpoint against unauthorized modification."
Factory: [SecurityAuditFlow] | TransitionType: N/A (security audit)
Teaching: "secure" fires SECURITY. "hyperparameter configuration endpoint" = object.

---

## FF-031-004 — GapHandler: overfitting without code verb

A: [] | "Overfitting is detected when validation loss diverges from training loss."
Factory: GapHandler | Gap ID: gap-ai-ml-training-001
Reason: "overfitting" = AI quality problem. No code verb.

---

## FF-031-005 — IMPLEMENT+API_CONTRACT: build hyperparameter service and integrate

A: ["IMPLEMENT", "API_CONTRACT"] | "Build the hyperparameter configuration service and integrate it with the training orchestrator API."
Factory: [APIContractFlow33, ImplementationFlow41] | TransitionType: feature_addition
Ordering: APIContractFlow33 → ImplementationFlow41 (API_CONTRACT before IMPLEMENT, canonical rule 2).
