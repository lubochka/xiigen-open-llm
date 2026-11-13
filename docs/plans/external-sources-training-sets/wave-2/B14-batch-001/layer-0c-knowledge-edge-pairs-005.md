# Layer 0-C — Knowledge Edge Pairs (005)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E005-001 — "tier" fires ARCHITECTURE in structural context

A: "Design the three-tier architecture: presentation, application, and data tiers."
  "design" → ARCHITECTURE | "three-tier architecture" → ARCHITECTURE | "presentation, application, data tiers" → ARCHITECTURE ("tiers")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "tier" in structural context (data tier, presentation tier, application tier) fires ARCHITECTURE.

---

## E005-002 — "mesh" fires ARCHITECTURE in service mesh context

A: "Architect the service mesh for the microservices cluster."
  "architect" → ARCHITECTURE | "service mesh" → ARCHITECTURE ("mesh" as a structural topology concept)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "service mesh" fires ARCHITECTURE. "mesh" as a structural topology concept is architecture vocabulary.

---

## E005-003 — "C4" fires ARCHITECTURE (C4 model abbreviation)

A: "Produce a C4 (Context, Containers, Components, Code) model for the platform."
  "C4" → ARCHITECTURE (C4 Model = architectural documentation framework) | "Context, Containers, Components, Code" → ARCHITECTURE | "model" → ARCHITECTURE (in documentation context) + DOCUMENT
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  Teaching: "C4" fires ARCHITECTURE as an abbreviation for the C4 architectural modeling framework.

---

## E005-004 — "VPC" fires ARCHITECTURE in network design

A: "Design the VPC (Virtual Private Cloud) peering topology for cross-account communication."
  "design" → ARCHITECTURE | "VPC peering topology" → ARCHITECTURE | "cross-account communication" → ARCHITECTURE (network boundary concept)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "VPC" in design/topology context fires ARCHITECTURE. "VPC" as a purely operational reference (create a VPC) may be gap if no design verb is present.

---

## E005-005 — "structure" fires ARCHITECTURE

A: "Structure the event processing pipeline with clear consumer group boundaries."
  "structure" → ARCHITECTURE | "event processing pipeline" → ARCHITECTURE | "consumer group boundaries" → ARCHITECTURE ("boundaries")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "structure" fires ARCHITECTURE as a design verb. "Structure the X" = ARCHITECTURE wherever X is a system component.

