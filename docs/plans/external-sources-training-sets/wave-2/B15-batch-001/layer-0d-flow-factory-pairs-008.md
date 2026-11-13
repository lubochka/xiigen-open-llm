# Layer 0-D — Flow Factory Pairs (008)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: flow_factory

## FF-008-001 — IMPLEMENT+API_CONTRACT+SECURITY+QA four flows
Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "QA"]
Factory: APIContractFlow33 + ImplementationFlow41 + SecurityAuditFlow + QAGatingFlow73
TransitionType: feature_addition | QA last

## FF-008-002 — IMPLEMENT+AUTHORIZATION+QA three flows
Predicates: ["IMPLEMENT", "AUTHORIZATION", "QA"]
Factory: ImplementationFlow41 + AuthorizationFlow + QAGatingFlow73 | QA last

## FF-008-003 — ARCHITECTURE+IMPLEMENT+DOCUMENT three flows
Predicates: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
Factory: ArchitectureFlow31 + ImplementationFlow41 ‖ DocumentationFlow | TransitionType: feature_addition

## FF-008-004 — IMPLEMENT+API_CONTRACT+TEST+QA four flows (carrier integration)
Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST", "QA"]
Factory: APIContractFlow33 + ImplementationFlow41 + E2ETestingFlow53 + QAGatingFlow73

## FF-008-005 — v6 corruption: PAYMENT predicate invented
v6 corrupt: "Build the payment processor" → Predicates: ["PAYMENT"] → PaymentFlow
Correct: Predicates: ["IMPLEMENT"] → ImplementationFlow41
Error type: v6-01. PAYMENT is not a predicate. Payment processing is an IMPLEMENT concern.
