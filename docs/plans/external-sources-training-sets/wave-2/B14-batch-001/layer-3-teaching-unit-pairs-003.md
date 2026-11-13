# Layer 3 — Teaching Unit Pairs (003)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## T003-001 — "blueprint" fires ARCHITECTURE without separate design verb

Rule: "blueprint" → ARCHITECTURE independently
  "produce the infrastructure blueprint" → ARCHITECTURE
  "provide the deployment blueprint" → ARCHITECTURE
  Teaching: "blueprint" is a standalone ARCHITECTURE trigger (design artifact noun).

---

## T003-002 — "topology" fires ARCHITECTURE in infra context

Rule: "topology" → ARCHITECTURE in infrastructure, network, data context
  "define the network topology" → ARCHITECTURE
  "architect the replication topology" → ARCHITECTURE
  Teaching: "topology" is an ARCHITECTURE trigger noun. No separate design verb required.

---

## T003-003 — "layer" fires ARCHITECTURE as structural noun

Rule: "layer" in structural design context → ARCHITECTURE
  "define the caching layer" → ARCHITECTURE
  "design the API layer" → ARCHITECTURE
  Teaching: "layer" fires ARCHITECTURE as a structural partition noun in design contexts.

---

## T003-004 — "plan" is gap for ARCHITECTURE

Rule: "plan" → gap (ARCHITECTURE requires design/architect/structure/blueprint/diagram)
  "plan the infrastructure" → gap
  "plan the migration" → gap
  Teaching: "plan" does not fire ARCHITECTURE. ARCHITECTURE requires an active design verb.

---

## T003-005 — "diagram" and "document" together → ARCHITECTURE+DOCUMENT

Rule: "diagram" → ARCHITECTURE; "document" → DOCUMENT
  "diagram the infrastructure and document the design decisions" → ARCHITECTURE + DOCUMENT
  Teaching: "diagram" fires ARCHITECTURE. "document" fires DOCUMENT. Both present → two predicates.

