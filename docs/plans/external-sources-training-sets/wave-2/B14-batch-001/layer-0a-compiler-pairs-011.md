# Layer 0-A — Compiler Pairs (011)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: compiler

---

## B14-compiler-011-001 — ARCHITECTURE: pod anti-affinity

A: "Design the pod scheduling architecture — define anti-affinity rules to distribute replicas across availability zones."
  "design" → ARCHITECTURE | "pod scheduling architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "anti-affinity rules" → ARCHITECTURE ("rules") | "availability zones" → ARCHITECTURE (infrastructure boundary concept)
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-011-002 — ARCHITECTURE: database migration topology

A: "Architect the database migration topology — design the dual-write pattern for zero-downtime transition between the legacy and new schema."
  "architect" → ARCHITECTURE | "database migration topology" → ARCHITECTURE | "design" → ARCHITECTURE | "dual-write pattern" → ARCHITECTURE ("pattern") | "zero-downtime transition" → ARCHITECTURE (structural continuity constraint)
  CompiledPredicateIds: ["ARCHITECTURE"]

---

## B14-compiler-011-003 — ARCHITECTURE: egress control

A: "Structure the egress control architecture — define the outbound traffic inspection layer and the allowlist policy for external service calls."
  "structure" → ARCHITECTURE | "egress control architecture" → ARCHITECTURE | "define" → ARCHITECTURE | "outbound traffic inspection layer" → ARCHITECTURE ("layer") + SECURITY | "allowlist policy" → SECURITY + ARCHITECTURE ("policy")
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]

---

## B14-compiler-011-004 — ARCHITECTURE+AUTHORIZATION: RBAC (Role-Based Access Control)

A: "Design the RBAC (Role-Based Access Control) architecture for the Kubernetes cluster — define namespace-scoped roles and cluster-level permission boundaries."
  "design" → ARCHITECTURE | "RBAC architecture" → ARCHITECTURE + AUTHORIZATION | "define" → ARCHITECTURE | "namespace-scoped roles" → AUTHORIZATION ("roles") | "permission boundaries" → AUTHORIZATION ("permission") + ARCHITECTURE ("boundaries")
  CompiledPredicateIds: ["ARCHITECTURE", "AUTHORIZATION"]

---

## B14-compiler-011-005 — gap: "is the architecture good?"

A: "Is our current architecture good enough for the expected load?"
  "is" → gap (interrogative, no action verb) | "architecture" without design verb → gap | "expected load" → gap
  CompiledPredicateIds: [] → gap
  Teaching: Questions about architecture ("is it good?", "does it scale?") are gap. ARCHITECTURE requires a design imperative: design, architect, define, structure, blueprint, diagram.

