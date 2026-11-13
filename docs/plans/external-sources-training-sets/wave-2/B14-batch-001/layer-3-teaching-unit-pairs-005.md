# Layer 3 — Teaching Unit Pairs (005)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## T005-001 — "component" fires ARCHITECTURE in structural design context

Rule: "component" + design verb → ARCHITECTURE
  "define the component boundaries" → ARCHITECTURE
  "design the component dependencies" → ARCHITECTURE
  Teaching: "component" fires ARCHITECTURE in architectural design contexts (boundaries, dependencies, diagrams).

---

## T005-002 — "boundary" fires ARCHITECTURE

Rule: "boundary" in architectural context → ARCHITECTURE
  "define the service boundary" → ARCHITECTURE
  "design the trust boundary" → ARCHITECTURE
  Teaching: "boundary" as an architectural demarcation noun fires ARCHITECTURE.

---

## T005-003 — "strategy" fires ARCHITECTURE in design context

Rule: "strategy" as design decision noun → ARCHITECTURE
  "define the caching strategy" → ARCHITECTURE
  "design the retry strategy" → ARCHITECTURE
  Teaching: "strategy" in architecture design context (caching, retry, partitioning, eviction) fires ARCHITECTURE.

---

## T005-004 — "pattern" fires ARCHITECTURE in structural design context

Rule: "pattern" as structural design concept → ARCHITECTURE
  "define the circuit breaker pattern" → ARCHITECTURE
  "design the sidecar pattern" → ARCHITECTURE
  Teaching: Named architectural patterns (circuit breaker, sidecar, CQRS, SAGA, bulkhead, strangler fig) fire ARCHITECTURE.

---

## T005-005 — Operational verbs are gap for ARCHITECTURE

Rule: configure, deploy, install, start, stop, restart, rollback, update, upgrade → gap
  "configure the gateway" → gap | "deploy the service" → gap | "restart the pods" → gap
  Teaching: Operational verbs are gap. ARCHITECTURE requires design verbs: design, architect, define, structure, blueprint, diagram.

