# Layer 0-B — Router Pairs (004)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-004-001 — ARCHITECTURE routes to 3.5 for AI infrastructure design

A: ["ARCHITECTURE"] | "Design the AI agent infrastructure with service mesh and message broker"
B: Sub-phase 3.5 → InfrastructureArchitectureFlow35 (infrastructure vocabulary co-present)

---

## C0B-004-002 — ARCHITECTURE routes to 3.1 for AI domain/service design

A: ["ARCHITECTURE"] | "Design the domain model for the multi-agent conversation flow"
B: Sub-phase 3.1 → AppArchitectureFlow31 (domain/service vocabulary)

---

## C0B-004-003 — IMPLEMENT+TEST: two flows, even with AI objects

A: ["IMPLEMENT", "TEST"] | "Build intent classifier and write unit tests"
B: IMPLEMENT → 4.1; TEST → 5.1 (unit context). Two flows. TransitionType: feature_addition.

---

## C0B-004-004 — IMPLEMENT+SECURITY: two flows for secure AI endpoint

A: ["IMPLEMENT", "SECURITY"] | "Implement agent API with prompt injection prevention"
B: IMPLEMENT → 4.1; SECURITY → security sub-phase. Two flows.

---

## C0B-004-005 — IMPLEMENT+TEST+DOCUMENT: three flows for AI feature

A: ["IMPLEMENT", "TEST", "DOCUMENT"] | "Build intent module, write tests, document taxonomy"
B: IMPLEMENT → 4.1; TEST → 5.1; DOCUMENT → DocumentationFlow62. Three flows.
