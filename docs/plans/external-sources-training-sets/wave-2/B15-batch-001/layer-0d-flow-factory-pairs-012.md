# Layer 0-D — Flow Factory Pairs (012)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: flow_factory

## FF-012-001 — All possible single-predicate factories
[IMPLEMENT] → ImplementationFlow41
[API_CONTRACT] → APIContractFlow33
[ARCHITECTURE] → ArchitectureFlow31 or 35
[TEST] → UnitTestingFlow51 or E2ETestingFlow53
[DOCUMENT] → DocumentationFlow
[QA] → QAGatingFlow73 or ReleaseGateFlow71
[SECURITY] → SecurityAuditFlow
[AUTHORIZATION] → AuthorizationFlow

## FF-012-002 — IMPLEMENT is the anchor predicate for B15
IMPLEMENT appears in nearly every B15 request. Other predicates augment it.
Pure API_CONTRACT, pure DOCUMENT, pure QA requests are rare in B15 but valid.

## FF-012-003 — QA sub-phase selection in B15 context
"release to staging" → 7.1 | "deployment" → 7.1 | "before feature ships" → 7.3 | "sign-off" → 7.3 (default)

## FF-012-004 — TEST sub-phase selection in B15 context
"unit tests for [algorithm/function/calculator]" → 5.1
"integration/E2E/smoke tests for [service/API/flow]" → 5.3

## FF-012-005 — Maximum pipeline: seven flows
ARCHITECTURE → API_CONTRACT → IMPLEMENT → SECURITY → TEST ‖ DOCUMENT → QA
All seven predicates → all seven flows → QA validates all outputs.
