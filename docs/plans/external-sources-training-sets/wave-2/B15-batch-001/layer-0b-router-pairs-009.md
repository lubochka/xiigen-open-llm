# Layer 0-B — Router Pairs (009)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: router

---

## B15-router-009-001 — IMPLEMENT + QA 7.1 (release to staging)

A: "Implement the order history service and QA the release to staging."
Router: Predicates = ["IMPLEMENT", "QA"] → ImplementationFlow41 → ReleaseGateFlow71 (7.1)
Evidence: "release to staging" → 7.1 sub-phase.

---

## B15-router-009-002 — IMPLEMENT + QA 7.3 (feature acceptance)

A: "Implement the order history feature and QA before the feature is merged."
Router: Predicates = ["IMPLEMENT", "QA"] → ImplementationFlow41 → QAGatingFlow73 (7.3)
Evidence: "feature merged" = feature acceptance gate → 7.3.

---

## B15-router-009-003 — ARCHITECTURE 3.5 (infrastructure)

A: "Design the cloud infrastructure for the order processing cluster."
Router: Predicates = ["ARCHITECTURE"] → ArchitectureFlow35 (3.5 — infrastructure topology)

---

## B15-router-009-004 — ARCHITECTURE 3.1 (application)

A: "Architect the order management bounded context."
Router: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31 (3.1 — application domain)

---

## B15-router-009-005 — v6 corruption: ARCHITECTURE not fired for domain model

v6 corrupt: "Build the order service with a domain model" → Predicates: ["IMPLEMENT"] only (domain model missed)
Correct: ["IMPLEMENT", "ARCHITECTURE"] — "domain model" fires ARCHITECTURE
Error: v6-02. "model" in domain model always fires ARCHITECTURE regardless of surrounding IMPLEMENT context.
