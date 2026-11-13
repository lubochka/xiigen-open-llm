# Layer 0-B — Router Pairs (008)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: router

---

## O1-router-008-001 — IMPLEMENT alone → 4.1 (implementation sub-phase)

Predicates: ["IMPLEMENT"] | Routes to: 4.1
Routing decisions: 1
Evidence: IMPLEMENT alone maps to sub-phase 4.1 (ImplementationFlow41 — generic)
Note: IMPLEMENT by itself does not trigger 3.4. It triggers the implementation sub-phase.
  3.4 only activates when SECURITY or AUTHORIZATION co-fires.

---

## O1-router-008-002 — IMPLEMENT+SECURITY → 4.1+3.4 (but 3.4 is priority for O1)

Predicates: ["IMPLEMENT", "SECURITY"] | Routes to: 4.1, 3.4
Routing decisions: 2
Decision 1: IMPLEMENT → 4.1 | EvidencePredicates: ["IMPLEMENT"]
Decision 2: SECURITY → 3.4 | EvidencePredicates: ["SECURITY"]
O1 priority rule: In the O1 scenario, when SECURITY fires alongside IMPLEMENT,
  3.4 (SecurityVerification34Flow) is the priority flow for security model work.
  4.1 handles general implementation scaffolding that is not security-specific.
  The orchestrator sequences 4.1 → 3.4 (implementation before verification).

---

## O1-router-008-003 — IMPLEMENT+AUTHORIZATION → 4.1+3.4

Predicates: ["IMPLEMENT", "AUTHORIZATION"] | Routes to: 4.1, 3.4
Routing decisions: 2
Decision 1: IMPLEMENT → 4.1
Decision 2: AUTHORIZATION → 3.4
Source: "Build the role assignment service for tenant administrators."
Same priority rule: 3.4 is the security+authorization model sub-phase;
  4.1 handles implementation scaffolding

---

## O1-router-008-004 — IMPLEMENT+SECURITY+AUTHORIZATION → 4.1+3.4 (two decisions, not three)

Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | Routes to: 4.1, 3.4
Routing decisions: 2
Decision 1: IMPLEMENT → 4.1
Decision 2: SECURITY+AUTHORIZATION → 3.4 (co-predicate, one decision)
Key rule: IMPLEMENT gets its own decision; SECURITY+AUTHORIZATION share a single 3.4 decision

---

## O1-router-008-005 — IMPLEMENT+API_CONTRACT+SECURITY → 4.1+3.3+3.4 (three decisions)

Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] | Routes to: 4.1, 3.3, 3.4
Routing decisions: 3
Decision 1: IMPLEMENT → 4.1
Decision 2: API_CONTRACT → 3.3
Decision 3: SECURITY → 3.4
Source: "Build the secure tenant API with OpenAPI contract."
Ordering: IMPLEMENT before API_CONTRACT before SECURITY per canonical predicate order
  (Note: ordering here shows routing decision sequence, not priority override)
