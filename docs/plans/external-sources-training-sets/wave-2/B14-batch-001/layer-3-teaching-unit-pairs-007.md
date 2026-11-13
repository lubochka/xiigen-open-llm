# Layer 3 — Teaching Unit Pairs (007)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## T007-001 — "migrate" alone is gap

Rule: "migrate" (operational action) → gap
  "migrate to AWS" → gap | "migrate the database" → gap
  vs "architect the migration topology" → ARCHITECTURE
  Teaching: "migrate" is gap. ARCHITECTURE requires "architect/design the migration [topology/strategy/architecture]".

---

## T007-002 — "microservices" alone is gap; "microservices architecture" fires ARCHITECTURE

Rule: "microservices" as standalone → gap
  "use microservices" → gap
  "design the microservices architecture" → ARCHITECTURE
  Teaching: Architectural style names alone are gap. Design verbs activate ARCHITECTURE.

---

## T007-003 — "SAGA", "strangler fig", "bulkhead" fire ARCHITECTURE

Rule: Recognized architectural patterns → ARCHITECTURE
  "design the SAGA pattern" → ARCHITECTURE
  "design the strangler fig migration" → ARCHITECTURE
  "define the bulkhead isolation" → ARCHITECTURE
  Teaching: All named architectural patterns fire ARCHITECTURE in design-verb context.

---

## T007-004 — "EDA" (Event-Driven Architecture) fires ARCHITECTURE

Rule: "EDA" → ARCHITECTURE
  "design the EDA backbone" → ARCHITECTURE
  Teaching: "EDA" fires ARCHITECTURE as an architectural paradigm abbreviation.

---

## T007-005 — Gap accumulation: multiple gap tokens → still gap

Rule: Multiple cloud/operational/platform tokens together → still gap if no design verb
  "set up AWS ECS with Kubernetes ingress and configure Redis" → gap (all operational/platform vocabulary)
  Teaching: Accumulating more gap tokens does not trigger ARCHITECTURE. Only design verbs activate ARCHITECTURE.

