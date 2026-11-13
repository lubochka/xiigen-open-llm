# Layer 3 — Teaching Unit Pairs (021)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## T021-001 — "GitOps deployment topology" fires ARCHITECTURE

A: "Design the GitOps deployment topology." ARCHITECTURE fires?
B: Yes. "design" → ARCHITECTURE. "GitOps deployment topology" → ARCHITECTURE via "topology" and "deployment" (design context). Route to 3.5.

---

## T021-002 — "merge to main" is Git operational gap

A: "Merge the feature branch to main after the pipeline passes." Predicates?
B: Zero predicates. "merge" → Git operational action, gap. "feature branch", "main", "pipeline passes" → all gap. Result: [] → knowledge_edge.

---

## T021-003 — "canary deployment pipeline" fires ARCHITECTURE via "pipeline" + "pattern"

A: "Architect the canary deployment pipeline topology." Predicates?
B: ["ARCHITECTURE"]. "architect" → ARCHITECTURE. "canary deployment pipeline topology" → ARCHITECTURE via "topology", "pipeline" (infra context), and "canary" (architectural deployment pattern). Route to 3.5.

---

## T021-004 — "run the deployment pipeline" is operational gap

A: "Run the deployment pipeline for the payment-service." Predicates?
B: Zero predicates. "run" → operational execution gap. "deployment pipeline" without design verb → gap in this context. Result: [] → knowledge_edge.

---

## T021-005 — ARCHITECTURE+IMPLEMENT for ArgoCD GitOps setup

A: "Design the GitOps topology and implement the ArgoCD application set for the multi-environment pipeline." Predicates?
B: ["ARCHITECTURE", "IMPLEMENT"]. "design" + "GitOps topology" → ARCHITECTURE. "implement" + "ArgoCD application set" → IMPLEMENT. TransitionType: feature_addition. Two flows: 3.5 + 4.1.
