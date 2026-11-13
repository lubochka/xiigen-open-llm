# Layer 0-B — Router Pairs (034)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-034-001 — ProrationService: IMPLEMENT single route

A: Predicates: ["IMPLEMENT"] from "Build ProrationService."
B: Route: ImplementationFlow41 (sub-phase 4.1) | DecisionCount: 1

---

## B15-router-034-002 — ProrationService: IMPLEMENT+ARCHITECTURE routing

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"] from "Build ProrationService + architect proration model."
B: Routes: ArchitectureFlow31 (3.1) → ImplementationFlow41 (4.1) | DecisionCount: 2

---

## B15-router-034-003 — ProrationService: IMPLEMENT+API_CONTRACT routing

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] from "Build ProrationService + integrate with subscription system."
B: Routes: APIContractFlow33 (3.3) → ImplementationFlow41 (4.1) | DecisionCount: 2

---

## B15-router-034-004 — ProrationService: IMPLEMENT+TEST+DOCUMENT three routes

A: Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"] from "Build + write unit tests + document rules."
B: Routes: ImplementationFlow41 (4.1) → TestingFlow51 (5.1) ‖ DocumentationFlow21 (2.1) | DecisionCount: 3
DOCUMENT parallel with 4.1 and 5.1. TEST after IMPLEMENT.

---

## B15-router-034-005 — ContextCarryForward through routes

ContextCarryForward: true — output of each completed flow feeds as input context to the next flow.
Architecture decisions from ArchitectureFlow31 become N01 input context for ImplementationFlow41.
Contract definitions from APIContractFlow33 inform implementation scope in ImplementationFlow41.
