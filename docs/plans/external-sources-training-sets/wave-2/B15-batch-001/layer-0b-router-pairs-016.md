# Layer 0-B — Router Pairs (016)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-016-001 — IMPLEMENT → ImplementationFlow41

A: Predicates: ["IMPLEMENT"] | Request: "Build the product catalog service with listing, search, and detail retrieval."
Router:
  - PredictedSubPhaseId: "4.1" | Confidence: 0.92 | DecisionCount: 1

---

## B15-router-016-002 — IMPLEMENT+ARCHITECTURE → two flows (domain model present)

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | Request: "Build the product catalog service and design the domain model for product categories and variants."
Router:
  - ArchitectureFlow31 (ARCHITECTURE) — sub-phase 3.1
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  DecisionCount: 2 | NearestWrongSubPhaseScore for 3.1: 0.38 (could confuse with 3.3).

---

## B15-router-016-003 — ARCHITECTURE+IMPLEMENT → same two flows, ARCHITECTURE fires first

A: Predicates: ["ARCHITECTURE", "IMPLEMENT"] | Request: "Design the catalog architecture and implement the product listing service."
Router:
  - ArchitectureFlow31 → ImplementationFlow41
  DecisionCount: 2 | Note: ARCHITECTURE listed first in predicates and executes first.

---

## B15-router-016-004 — ARCHITECTURE+API_CONTRACT+IMPLEMENT+DOCUMENT → four flows

A: Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "DOCUMENT"] | Request: "Design the catalog domain model, define the product API contracts, build the catalog service, and document the product endpoints."
Router:
  - ArchitectureFlow31 | APIContractFlow33 | ImplementationFlow41 | DocumentationFlow21
  DecisionCount: 4 | Ordering: 3.1 → 3.3 → 4.1 ‖ 2.1

---

## B15-router-016-005 — NearestWrongSubPhaseScore: 3.1 vs 3.3 disambiguation

Teaching point: Both ARCHITECTURE (3.1) and API_CONTRACT (3.3) are sub-phase 3. When both fire, each routes to its own sub-phase independently. NearestWrongSubPhaseScore for each: 3.1 NearestWrong → 3.3 (score: 0.38); 3.3 NearestWrong → 3.1 (score: 0.36). Both pass routing threshold.
