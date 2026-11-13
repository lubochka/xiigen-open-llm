# Layer 0-B — Router Pairs (002)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: router

---

## B14-router-002-001 — ARCHITECTURE alone → 3.5

A: ["ARCHITECTURE"]
B: SubPhase 3.5 (InfrastructureArchitectureFlow35)
  Teaching: ARCHITECTURE in cloud/infra context (infrastructure, topology, layer, data tier, service mesh) routes to 3.5. Application/domain architecture routes to 3.1.

---

## B14-router-002-002 — ARCHITECTURE+IMPLEMENT → 3.5, 4.1

A: ["ARCHITECTURE", "IMPLEMENT"]
B: SubPhase 3.5 (InfrastructureArchitectureFlow35) + SubPhase 4.1 (ImplementationFlow41)
  Routing: ARCHITECTURE → 3.5 independent of IMPLEMENT; IMPLEMENT → 4.1 independent of ARCHITECTURE. Both flows created.

---

## B14-router-002-003 — ARCHITECTURE+DOCUMENT → 3.5, 6.2

A: ["ARCHITECTURE", "DOCUMENT"]
B: SubPhase 3.5 (InfrastructureArchitectureFlow35) + SubPhase 6.2 (DocumentationFlow62)
  Teaching: DOCUMENT routes to 6.2 (DocumentationFlow) independently. Architecture diagrams and ADR documentation both route here.

---

## B14-router-002-004 — ARCHITECTURE+TEST → 3.5, 5.1

A: ["ARCHITECTURE", "TEST"]
B: SubPhase 3.5 (InfrastructureArchitectureFlow35) + SubPhase 5.1 (UnitTestingFlow51)
  Teaching: Writing infrastructure tests (validate provisioning, connectivity tests) fires TEST → 5.1 alongside ARCHITECTURE → 3.5.

---

## B14-router-002-005 — gap → GapHandler

A: []
B: GapHandler (no flow instantiated)
  SCHEDULE_CAPABILITY_LEARNING emitted to subproject 16 (AutonomousLearningPipeline (16))
  DomainId: "software-development"

