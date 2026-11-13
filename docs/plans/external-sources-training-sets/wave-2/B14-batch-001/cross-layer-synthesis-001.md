# Cross-Layer Synthesis (001)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## S001 — End-to-end trace: "Architect the microservices platform infrastructure and document the design decisions"

### Layer 0-A
Input: "Architect the microservices platform infrastructure — define the service mesh topology, define the data layer, and document the design decisions as ADRs."
  "architect" → ARCHITECTURE | "service mesh topology" → ARCHITECTURE ("topology") | "data layer" → ARCHITECTURE ("layer") | "define" → ARCHITECTURE | "document" → DOCUMENT | "ADRs" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]

### Layer 0-B
  ARCHITECTURE → SubPhase 3.5 (infra vocabulary: service mesh, data layer) | DOCUMENT → SubPhase 6.2
  DecisionSet: [ArchitectureDecision(3.5), DocumentDecision(6.2)]

### Layer 0-D
  FlowFactory.CreateMulti(["ARCHITECTURE","DOCUMENT"]) → [InfrastructureArchitectureFlow35, DocumentationFlow62]

### Layer 1
  TransitionType: documentation (no IMPLEMENT verb) | Sequential: ARCHITECTURE design → DOCUMENT recording

### Layer 2 (InfrastructureArchitectureFlow35)
  Service mesh: Istio STRICT mTLS | Data layer: primary-replica PostgreSQL | Availability: 3-AZ anti-affinity
  Arbiter: ArchitectureCompletenessArbiter | Node 7: PASS — all layers covered

### Layer 3
  Teaching: "architect" fires ARCHITECTURE. "document" fires DOCUMENT. "ADRs" fires ARCHITECTURE independently. Sub-phase 3.5 from infra vocabulary.

### Cross-layer invariant
  ARCHITECTURE and DOCUMENT are independent predicates. ADR writing is always a documentation TransitionType when no IMPLEMENT predicate present.

