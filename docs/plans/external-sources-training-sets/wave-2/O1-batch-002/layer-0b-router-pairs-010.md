# Layer 0-B — Router Pairs (010)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: router

---

## O1b-0B-010-001 — SECURITY+AUTHORIZATION dedup rule (core teaching)

A: Predicates: ["SECURITY", "AUTHORIZATION"] | RoutingQuestion: "How many 3.4 routing decisions?"

B: Answer: ONE routing decision to 3.4. SECURITY maps to 3.4. AUTHORIZATION also maps to 3.4. Since they share the same sub-phase target, they dedup to exactly one 3.4 routing decision. This is NOT an error — it is the correct dedup behavior.

---

## O1b-0B-010-002 — Dedup confirmed with 3 co-mapping predicates

A: Predicates: ["SECURITY", "AUTHORIZATION", "SECURITY", "AUTHORIZATION"] | RoutingQuestion: "How many 3.4 routing decisions?"

B: Answer: ONE. No matter how many predicates map to the same sub-phase, the routing decision count for that sub-phase is always exactly one.

---

## O1b-0B-010-003 — Dedup does not apply across different sub-phases

A: Predicates: ["SECURITY", "API_CONTRACT"] | RoutingQuestion: "How many total routing decisions?"

B: Answer: TWO. SECURITY → 3.4 (one decision). API_CONTRACT → 3.3 (one decision). These are different sub-phases, so no dedup applies. Total: two decisions.

---

## O1b-0B-010-004 — SECURITY+AUTHORIZATION+API_CONTRACT dedup summary

A: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | RoutingQuestion: "Enumerate routing decisions."

B: RoutingDecisions: [{ sub_phase: "3.4", reason: "SECURITY+AUTHORIZATION dedup" }, { sub_phase: "3.3", reason: "API_CONTRACT" }]
   Summary: 3 predicates → 2 routing decisions (because 2 of 3 predicates share the same target sub-phase).

---

## O1b-0B-010-005 — Dedup rule stated as axiom

A: Routing dedup axiom for O1b

B: AXIOM: For any set of predicates in O1b, the number of routing decisions equals the number of DISTINCT sub-phases targeted, not the number of predicates. SECURITY+AUTHORIZATION → 1 sub-phase (3.4) → 1 decision. SECURITY+AUTHORIZATION+API_CONTRACT → 2 sub-phases (3.4, 3.3) → 2 decisions.
