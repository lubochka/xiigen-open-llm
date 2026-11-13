# Layer 3 — Teaching Unit Pairs (006)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## T006-001 — "ADR" abbreviation fires ARCHITECTURE

Rule: "ADR" → ARCHITECTURE (Architecture Decision Record)
  "write the ADR for the messaging choice" → ARCHITECTURE + DOCUMENT
  Teaching: "ADR" fires ARCHITECTURE via abbreviation expansion. Full form also fires ARCHITECTURE.

---

## T006-002 — "C4" model fires ARCHITECTURE

Rule: "C4" → ARCHITECTURE (C4 modeling framework: Context, Containers, Components, Code)
  "produce a C4 model for the platform" → ARCHITECTURE + DOCUMENT
  Teaching: "C4" fires ARCHITECTURE as an abbreviation for the architectural documentation framework.

---

## T006-003 — "CQRS" fires ARCHITECTURE

Rule: "CQRS" (Command Query Responsibility Segregation) → ARCHITECTURE
  "design the CQRS pattern for OrderService" → ARCHITECTURE
  Teaching: Architectural pattern abbreviations (CQRS, SAGA, CDC, EDA) fire ARCHITECTURE.

---

## T006-004 — "IaC" fires ARCHITECTURE+IMPLEMENT

Rule: "IaC" (Infrastructure as Code) → ARCHITECTURE + IMPLEMENT
  "implement the IaC pipeline" → ARCHITECTURE (infra design) + IMPLEMENT (code writing)
  Teaching: "IaC" fires ARCHITECTURE (infrastructure design concept) and, when combined with "implement", fires IMPLEMENT.

---

## T006-005 — "RBAC" fires ARCHITECTURE+AUTHORIZATION

Rule: "RBAC" (Role-Based Access Control) → ARCHITECTURE + AUTHORIZATION
  "design the RBAC architecture" → ARCHITECTURE + AUTHORIZATION
  Teaching: "RBAC" fires both ARCHITECTURE (structural design) and AUTHORIZATION (permission model). Independent predicates.

