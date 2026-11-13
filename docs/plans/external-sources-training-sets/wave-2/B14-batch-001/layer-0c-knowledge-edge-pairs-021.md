# Layer 0-C — Knowledge Edge Pairs (021)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-021-001 — "GitOps" pattern fires ARCHITECTURE

A: "Design the GitOps reconciliation architecture — declarative state, drift detection loop, and automated remediation pipeline."
  "design" → ARCHITECTURE | "GitOps reconciliation architecture" → ARCHITECTURE | "drift detection loop" → ARCHITECTURE ("pattern") | "automated remediation pipeline" → ARCHITECTURE ("pipeline")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "GitOps" as an architectural pattern fires ARCHITECTURE when paired with design vocabulary. "reconciliation architecture" and "pipeline" both reinforce it.

---

## B14-edge-021-002 — "merge to main" does not fire any predicate

A: "Merge the feature branch to main after the CI (Continuous Integration) pipeline passes."
  "merge" → gap | "feature branch" → gap | "CI pipeline passes" → gap
  CompiledPredicateIds: []
  Gap signal: gap-devops-git-001 — Git workflow vocabulary is gap.
  Teaching: Source control operations (merge, push, commit, branch) are gap. To fire ARCHITECTURE: "design the branch promotion topology for the GitOps pipeline."

---

## B14-edge-021-003 — "deployment pipeline" with design verb fires ARCHITECTURE

A: "Architect the canary deployment pipeline topology — traffic splitting, automated rollback triggers, and promotion criteria."
  "architect" → ARCHITECTURE | "canary deployment pipeline topology" → ARCHITECTURE ("pipeline", "topology", "deployment") | "traffic splitting" → ARCHITECTURE | "automated rollback triggers" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "canary" is an architectural pattern (canary deployment). "pipeline topology" fires ARCHITECTURE. All three structural nouns fire the predicate.

---

## B14-edge-021-004 — "ArgoCD" alone does not fire ARCHITECTURE

A: "Sync the ArgoCD application to the latest Git revision."
  "sync" → gap (operational action) | "ArgoCD application" → gap | "Git revision" → gap
  CompiledPredicateIds: []
  Gap signal: gap-devops-gitops-001 — ArgoCD operational commands are gap.
  Teaching: "sync" is an operational command. To fire IMPLEMENT: "implement the ArgoCD application sync configuration."
