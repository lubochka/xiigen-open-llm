# Layer 0-B — Router Pairs (009)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: router

---

## O1-router-009-001 — Routing ordering: ARCHITECTURE before API_CONTRACT

Predicates: ["ARCHITECTURE", "API_CONTRACT"] | Routes to: 3.x, 3.3
Routing decisions: 2
Decision 1: ARCHITECTURE → 3.x (architecture sub-phase, determined by scenario)
Decision 2: API_CONTRACT → 3.3
Ordering rule: ARCHITECTURE is always first in routing decision output
Note: "ARCHITECTURE → 3.x" means the specific architecture sub-phase for the scenario;
  in O1 the architecture sub-phase is determined by the scenario topology

---

## O1-router-009-002 — Routing ordering: IMPLEMENT before SECURITY before AUTHORIZATION

Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | Routes to: 4.1, 3.4
Routing decisions: 2
Ordering: canonical order is IMPLEMENT → SECURITY → AUTHORIZATION
  But SECURITY+AUTHORIZATION → same sub-phase (3.4), so they collapse into one decision
  Decision sequence: [4.1 (IMPLEMENT), 3.4 (SECURITY+AUTHORIZATION)]
Note: Canonical ordering governs the sequence of RoutingDecision[] in the output array

---

## O1-router-009-003 — Routing ordering: TEST before DOCUMENT (parallel)

Predicates: ["TEST", "DOCUMENT"] | Routes to: 5.1, 2.1
Routing decisions: 2
Decision 1: TEST → 5.1
Decision 2: DOCUMENT → 2.1
Ordering note: TEST and DOCUMENT are parallel (‖) in the canonical ordering.
  When both fire together, their order in the output is TEST before DOCUMENT (by convention).
  The orchestrator may execute them in parallel — the array order does not imply sequence.

---

## O1-router-009-004 — Routing ordering: QA always last

Predicates: ["SECURITY", "QA"] | Routes to: 3.4, 7.x
Routing decisions: 2
Decision 1: SECURITY → 3.4 (first in output)
Decision 2: QA → 7.x (always last)
Rule: QA is the absolute last routing decision regardless of predicate discovery order.
  Even if "QA" appears first in the user prompt, its routing decision is appended last.

---

## O1-router-009-005 — Full canonical ordering: all predicates present

Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]
Routes to: 3.x, 3.3, 4.1, 3.4, 5.1, 2.1, 7.x
Routing decisions: 7 (SECURITY+AUTHORIZATION share 3.4 → one decision)
Order: [3.x(ARCH), 3.3(API), 4.1(IMPL), 3.4(SEC+AUTH), 5.1(TEST), 2.1(DOC), 7.x(QA)]
This is the maximum fan-out scenario for O1: 8 predicates → 7 routing decisions
