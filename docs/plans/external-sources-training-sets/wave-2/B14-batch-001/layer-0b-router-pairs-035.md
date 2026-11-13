# Layer 0-B — Router Pairs (035)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-035-001 — CrossAccountIAM routing: AUTHORIZATION sub-phase

AUTHORIZATION routes to its designated sub-phase (access control / authorization flow):
  "authorize cross-account IAM roles" → AUTHORIZATION routing
  Concurrent with ARCHITECTURE (3.5) when design verb + topology present
  Concurrent with SECURITY (6.3) when credential/encryption vocabulary present

---

## B14-router-035-002 — CrossAccountIAM routing: SECURITY → 6.3

SECURITY → 6.3 for credential and permission boundary vocabulary:
  "IAM permission boundaries" → 6.3 | "credential rotation for cross-account roles" → 6.3
  SECURITY fires on IAM in security binding context → always routes 6.3
  Concurrent with AUTHORIZATION when both predicates fire

---

## B14-router-035-003 — CrossAccountIAM routing: IMPLEMENT + AUTHORIZATION concurrent

IMPLEMENT (4.1) + AUTHORIZATION concurrent routing:
  "write Terraform IAM role modules and authorize the CI/CD service account" → IMPLEMENT (4.1) + AUTHORIZATION
  Sequential: AUTHORIZATION before IMPLEMENT in ordering (policy defined before code uses it)
  TransitionType: feature_addition

---

## B14-router-035-004 — CrossAccountIAM routing: gap boundary for operational assume-role

"Switch to staging account and assume deployment role" → zero predicates:
  CLI/console assume-role action is operational → gap → knowledge_edge_synthetic
  No routing to AUTHORIZATION: "assume" as CLI action is operational gap
  AUTHORIZATION fires only on "authorize", "grant", "revoke", "define IAM role" design actions
