# Layer 0-D — Flow Factory Pairs (009)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B14-factory-009-001 — Full IaC release: ARCHITECTURE+IMPLEMENT+TEST+DOCUMENT+QA

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT", "QA"]
B: FlowFactory.CreateMulti([...]) → [InfrastructureArchitectureFlow35, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62, QAGatingFlow73]
  Five flows. TransitionType: feature_addition. Sequential: design → implement → test → document → QA gate.

---

## B14-factory-009-002 — ARCHITECTURE+SECURITY+IMPLEMENT+TEST: hardened IaC

A: ["ARCHITECTURE", "SECURITY", "IMPLEMENT", "TEST"]
B: [InfrastructureArchitectureFlow35, SecurityAuditFlow63, ImplementationFlow41, UnitTestingFlow51]
  TransitionType: feature_addition. Four flows for security-hardened IaC cycle.

---

## B14-factory-009-003 — ARCHITECTURE alone: minimal flow

A: ["ARCHITECTURE"] | "design the service mesh architecture"
B: [InfrastructureArchitectureFlow35] | One flow | TransitionType: documentation
  Teaching: Single ARCHITECTURE predicate creates one flow. No additional flows invented.

---

## B14-factory-009-004 — v6-02 corruption: gap becomes ARCHITECTURE

A: [] | "set up the cluster"
B: CORRUPT: FlowFactory.Create("ARCHITECTURE") → v6-02 (fallback suppression, gap becomes ARCHITECTURE)
  Correct: FlowFactory.CreateGap() → GapFlow
  Teaching: v6-02 incorrectly defaults to ARCHITECTURE for operational commands. "set up", "configure", "deploy" are gap → GapFlow.

---

## B14-factory-009-005 — ARCHITECTURE+DOCUMENT+SECURITY: security architecture documentation

A: ["ARCHITECTURE", "SECURITY", "DOCUMENT"]
B: [InfrastructureArchitectureFlow35, SecurityAuditFlow63, DocumentationFlow62]
  TransitionType: documentation (no IMPLEMENT). Three flows for documenting security architecture decisions.

