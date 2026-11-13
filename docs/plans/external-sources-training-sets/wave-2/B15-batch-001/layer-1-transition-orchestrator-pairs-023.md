# Layer 1 — Transition Orchestrator Pairs (023)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-023-001 — TaxService: feature_addition

A:
  user_request: "Implement the tax calculation service that applies jurisdiction-specific tax rates."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  TransitionId: "transition-B15-t1-023-001-4.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "4.1"
  PredicateIds: ["IMPLEMENT"]

---

## B15-t1-023-002 — TaxService + domain model: IMPLEMENT+ARCHITECTURE

A:
  user_request: "Build the tax service and design the domain model for tax jurisdiction rules."
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]

B:
  TransitionId: "transition-B15-t1-023-002-multi-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseIds: ["3.1", "4.1"]
  PredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Metadata:
    ordering: "ArchitectureFlow31 before ImplementationFlow41"

---

## B15-t1-023-003 — TaxService four predicates

A:
  user_request: "Architect the tax service domain model, define the tax API contracts, implement the calculation engine, and document the tax rules."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "DOCUMENT"]

B:
  TransitionId: "transition-B15-t1-023-003-multi-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseIds: ["3.1", "3.3", "4.1", "2.1"]
  PredicateIds: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "DOCUMENT"]
  Metadata:
    ordering: "ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 ‖ DocumentationFlow21"

---

## B15-t1-023-004 — TaxService six predicates

A:
  user_request: "Design tax domain model, define tax API contracts, build tax service, secure tax data endpoints, write tax calculation tests, and document the tax rules."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT"]

B:
  TransitionId: "transition-B15-t1-023-004-multi-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseIds: ["3.1", "3.3", "security", "4.1", "5.1", "2.1"]
  Metadata:
    ordering: "ArchitectureFlow31 → APIContractFlow33 → SecurityAuditFlow → ImplementationFlow41 → TestingFlow51 ‖ DocumentationFlow21"
