# Layer 0-A — Compiler Pairs (035)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-035-001 — CrossAccountIAM: AUTHORIZATION trigger

A: "Define the cross-account IAM roles and authorize inter-account access for shared services."
  CompiledPredicateIds: ["AUTHORIZATION", "ARCHITECTURE"]
  AUTHORIZATION: "authorize" + "IAM roles" (RBAC/IAM roles context) | ARCHITECTURE: "define" (structural context) + "roles" (structural)
  Gap: "cross-account" (operational term), "IAM" alone without binding context

---

## B14-compiler-035-002 — CrossAccountIAM: SECURITY trigger

A: "Enforce IAM permission boundaries and configure credential rotation for cross-account roles."
  CompiledPredicateIds: ["SECURITY"]
  SECURITY: "IAM" (security binding) + "credential rotation" + "permission boundaries" (security constraint)
  Gap: "configure" (operational); SECURITY fires from IAM + credential vocabulary

---

## B14-compiler-035-003 — CrossAccountIAM: IMPLEMENT + AUTHORIZATION

A: "Write the Terraform IAM role modules and authorize the CI/CD (Continuous Integration/Continuous Delivery) pipeline service account."
  CompiledPredicateIds: ["IMPLEMENT", "AUTHORIZATION"]
  IMPLEMENT: "write" + "IAM role modules" (code artifact) | AUTHORIZATION: "authorize" + "service account" (IAM roles binding)
  TransitionType: feature_addition

---

## B14-compiler-035-004 — CrossAccountIAM: ARCHITECTURE + SECURITY + AUTHORIZATION

A: "Design the cross-account IAM topology, enforce zero-trust credential boundaries, and authorize the assume-role trust policies."
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY", "AUTHORIZATION"]
  ARCHITECTURE: "design" + "topology" | SECURITY: "zero-trust" + "credential" | AUTHORIZATION: "authorize" + "trust policies" (IAM)
  TransitionType: feature_addition (AUTHORIZATION drives feature_addition)

---

## B14-compiler-035-005 — CrossAccountIAM: gap vs AUTHORIZATION boundary

A: "Switch to the staging account and assume the deployment role."
  CompiledPredicateIds: []
  Gap: "switch" (operational), "assume" (operational CLI action — not IAM policy authoring), "staging account" (runtime reference)
  Teaching: Operational console/CLI actions on IAM are gap. Authorizing/defining IAM roles fires AUTHORIZATION.
