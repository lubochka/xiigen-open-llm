# Held-Out and Corruption Pairs (023)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H023-001 — Held-out: "scaffold Terraform" fires IMPLEMENT

A: "Scaffold the Terraform module structure for the networking layer."
  Expected: ["IMPLEMENT", "ARCHITECTURE"]
  Held-out for evaluation.

---

## H023-002 — Held-out: RBAC configuration fires IMPLEMENT+SECURITY

A: "Configure the Kubernetes RBAC roles and credential bindings."
  Expected: ["IMPLEMENT", "SECURITY"]
  Held-out: "configure" (code context) → IMPLEMENT; "RBAC" + "credential bindings" → SECURITY.

---

## H023-003 — Held-out: "build the Dockerfile" fires IMPLEMENT

A: "Build the multi-stage Dockerfile for the payments service."
  Expected: ["IMPLEMENT"]
  Held-out for evaluation.

---

## C023-001 — Corruption v6-01: TERRAFORM invented as predicate

A: "Implement the Terraform modules for the VPC."
  Correct: ["IMPLEMENT"]
  Corrupt: ["IMPLEMENT", "TERRAFORM"] ← v6-01 (TERRAFORM is not a valid predicate)
  Teaching: "Terraform" is a tool name. It is gap vocabulary. Only "implement" fires IMPLEMENT.

---

## C023-002 — Corruption v6-03: "scaffold" fires ARCHITECTURE instead of IMPLEMENT

A: "Scaffold the Terraform module structure."
  Correct: ["IMPLEMENT"] (or ["IMPLEMENT", "ARCHITECTURE"] if layer/infrastructure noun present)
  Corrupt: ["ARCHITECTURE"] ← v6-03 (treats IaC scaffolding as architectural design)
  Teaching: "scaffold" fires IMPLEMENT (IaC code generation action), not ARCHITECTURE. ARCHITECTURE requires design/architect/structure/define verbs. "scaffold" is an IMPLEMENT action verb.
