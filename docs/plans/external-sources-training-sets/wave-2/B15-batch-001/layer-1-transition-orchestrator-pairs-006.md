# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

## TO-006-001 — v6 corruption: ordering violation
v6 corrupt: IMPLEMENT → QA → TEST (QA not last; TEST after QA)
Correct: IMPLEMENT → TEST → QA (QA always last)

## TO-006-002 — v6 corruption: API_CONTRACT after IMPLEMENT
v6 corrupt: IMPLEMENT → API_CONTRACT (wrong order)
Correct: API_CONTRACT → IMPLEMENT (contract spec before implementation)

## TO-006-003 — IMPLEMENT+DOCUMENT+QA orchestration
Predicates: ["IMPLEMENT", "DOCUMENT", "QA"]
Orchestration: ImplementationFlow41 ‖ DocumentationFlow → QAGatingFlow73
TransitionType: feature_addition | QA after both flows complete.

## TO-006-004 — IMPLEMENT+SECURITY+TEST+QA four flows
Predicates: ["IMPLEMENT", "SECURITY", "TEST", "QA"]
Orchestration: ImplementationFlow41 → SecurityAuditFlow → TestingFlow → QAGatingFlow73
All flows sequential; QA validates security + test outputs.

## TO-006-005 — ARCHITECTURE+IMPLEMENT+API_CONTRACT+SECURITY+TEST+DOCUMENT+QA
Seven-predicate max pipeline for B15:
ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → E2ETestingFlow53 ‖ DocumentationFlow → QAGatingFlow73
TransitionType: feature_addition | QA absolute last.
