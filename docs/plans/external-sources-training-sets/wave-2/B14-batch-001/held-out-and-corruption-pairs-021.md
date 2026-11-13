# Held-Out and Corruption Pairs (021)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H021-001 — Held-out: "GitOps reconciliation architecture" fires ARCHITECTURE

A: "Design the GitOps reconciliation architecture for the multi-environment pipeline."
  Expected: ["ARCHITECTURE"]
  Held-out for evaluation.

---

## H021-002 — Held-out: "merge to main" is gap

A: "Merge the hotfix branch to main after approval."
  Expected: []
  Held-out: "merge" is Git operational gap.

---

## H021-003 — Held-out: "canary deployment" fires ARCHITECTURE via "pattern"

A: "Design the canary deployment pipeline architecture."
  Expected: ["ARCHITECTURE"]
  Held-out for evaluation.

---

## C021-001 — Corruption v6-01: GITOPS invented as predicate

A: "Design the GitOps deployment topology."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "GITOPS"] ← v6-01 (GITOPS is not a valid predicate)
  Teaching: "GitOps" is a methodology name. Only "deployment topology" fires ARCHITECTURE via "topology" + "design". No GITOPS predicate exists.

---

## C021-002 — Corruption v6-03: "sync ArgoCD" fires IMPLEMENT

A: "Sync the ArgoCD application to the latest revision."
  Correct: []
  Corrupt: ["IMPLEMENT"] ← v6-03 (treats ArgoCD sync as implementation)
  Teaching: "sync" is an operational ArgoCD command. It is not in IMPLEMENT lexicon. IMPLEMENT fires on write/build/create/scaffold/implement/configure (code artifact context).
