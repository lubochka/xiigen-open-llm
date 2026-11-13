# Layer 1 — Transition Orchestrator Pairs (052)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-052-001 — CustomerProfileService full pipeline transition

A: "Build CustomerProfileService with a profile domain model, integrate with the identity provider, secure PII endpoints, write profile tests, and document the profile schema."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "3.3", "security", "4.1", "5.1", "2.1"] | ContextCarryForward: true

---

## B15-t1-052-002 — KYCService full pipeline transition

A: "Create KYCService with a KYC domain model, wire up the identity API, secure document upload, write tests, and document the verification flow."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "3.3", "security", "4.1", "5.1", "2.1"] | ContextCarryForward: true

---

## B15-t1-052-003 — ComplianceService + KYCService combined transition (multi-service request)

A: "Build ComplianceService and KYCService, secure all verification endpoints, and document the compliance rules."
B: TransitionType: feature_addition | SubPhaseIds: ["security", "4.1", "2.1"] | ContextCarryForward: true
Multiple services in one request → predicates union across all services → single SubPhaseIds set.

---

## B15-t1-052-004 — EventPublisherService + EventConsumerService combined transition

A: "Build EventPublisherService and EventConsumerService, integrate both with RabbitMQ, and write integration tests."
B: TransitionType: feature_addition | SubPhaseIds: ["3.3", "4.1", "5.1"] | ContextCarryForward: true
Two services → predicates union → API_CONTRACT from "integrate" (both services) + IMPLEMENT + TEST.

---

## B15-t1-052-005 — SagaOrchestrator + CompensationService combined transition

A: "Build SagaOrchestrator with a saga domain model and CompensationService with a compensation domain model."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "4.1"] | ContextCarryForward: true
Two ARCHITECTURE triggers (two domain models) → deduplicated to one "3.1" entry in SubPhaseIds.
