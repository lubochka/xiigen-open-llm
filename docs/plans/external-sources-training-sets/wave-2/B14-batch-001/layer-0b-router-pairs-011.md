# Layer 0-B — Router Pairs (011)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: router

---

## B14-router-011-001 — ARCHITECTURE alone: single flow

A: ["ARCHITECTURE"]
B: → 3.5 | InfrastructureArchitectureFlow35 created | One flow
  Teaching: Single predicate → single flow. No suppression, no additional predicates invented.

---

## B14-router-011-002 — ARCHITECTURE+IMPLEMENT: two flows (not one)

A: ["ARCHITECTURE", "IMPLEMENT"]
B: → 3.5 + 4.1 | Two flows created
  v6-03 trap: collapsing to only 4.1 because "implement" seems dominant → incorrect
  Teaching: Both flows always created. ARCHITECTURE is never suppressed by IMPLEMENT.

---

## B14-router-011-003 — ARCHITECTURE+TEST: validate infra

A: ["ARCHITECTURE", "TEST"]
B: → 3.5 + 5.1 | Two flows
  Teaching: Infrastructure validation tests (connectivity, provisioning assertions) fire TEST → 5.1. ARCHITECTURE flow remains independent.

---

## B14-router-011-004 — ARCHITECTURE+API_CONTRACT+IMPLEMENT: API platform

A: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT"]
B: → 3.5 + 3.3 + 4.1 | Three flows | TransitionType: feature_addition
  Teaching: Designing and implementing an API gateway platform: three flows, three sub-phases.

---

## B14-router-011-005 — gap: architectural-sounding but no trigger

A: "The system needs to handle more load."
  "needs to handle more load" → gap (operational intent without architecture verb)
  CompiledPredicateIds: [] → GapHandler
  Teaching: Load-related requests without "design/architect the scaling architecture" are gap. Intent ≠ architecture verb.

