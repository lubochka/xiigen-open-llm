# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B14-transition-011-001 — ARCHITECTURE+IMPLEMENT: "develop" verb

A: ["ARCHITECTURE", "IMPLEMENT"] | "design the ingress topology + develop the gateway configuration"
B: TransitionType: feature_addition ("develop" = IMPLEMENT trigger)

---

## B14-transition-011-002 — ARCHITECTURE+IMPLEMENT: "generate" verb

A: ["ARCHITECTURE", "IMPLEMENT"] | "architect the logging layer + generate the log aggregation pipeline code"
B: TransitionType: feature_addition ("generate" = IMPLEMENT trigger)

---

## B14-transition-011-003 — ARCHITECTURE+IMPLEMENT: "extend" verb

A: ["ARCHITECTURE", "IMPLEMENT"] | "extend the architecture to include a caching layer + implement the Redis cluster config"
B: TransitionType: feature_addition ("extend" = IMPLEMENT trigger)

---

## B14-transition-011-004 — ARCHITECTURE+IMPLEMENT: "add" verb in code context

A: ["ARCHITECTURE", "IMPLEMENT"] | "add the bulkhead pattern to the architecture + implement the thread pool isolation"
B: TransitionType: feature_addition ("add" in code/implementation context = IMPLEMENT trigger)
  Teaching: "add" in code context fires IMPLEMENT → feature_addition. "add a section to the diagram" would be DOCUMENT/ARCHITECTURE.

---

## B14-transition-011-005 — ARCHITECTURE alone: "add" in design context → documentation

A: ["ARCHITECTURE"] | "add a caching layer to the architecture diagram"
B: TransitionType: documentation ("add" in diagram context = updating design artifact, not implementing code)
  Teaching: "add" in design/diagram context fires ARCHITECTURE (not IMPLEMENT) → documentation TransitionType.

