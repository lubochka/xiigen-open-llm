# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

## TO-012-001 — v6 corruptions summary
v6-01: Invented FULFILLMENT/PAYMENT/ORDER_PROCESSING predicates → IMPLEMENT
v6-02: Missed "data contracts" → API_CONTRACT; missed "domain model" → ARCHITECTURE  
v6-03: Multi-predicate collapsed to IMPLEMENT only

## TO-012-002 — Canonical single-flow B15 pattern
"[build/implement/create/scaffold/bootstrap/extend] the [service/module/handler/processor]"
→ IMPLEMENT → ImplementationFlow41 → feature_addition

## TO-012-003 — Canonical two-flow B15 pattern (most common)
"[build the service] and [integrate with the [API]]"
→ [IMPLEMENT, API_CONTRACT] → APIContractFlow33 → ImplementationFlow41 → feature_addition

## TO-012-004 — Canonical three-flow B15 pattern
"[build service] + [integrate API] + [write tests]"
→ [IMPLEMENT, API_CONTRACT, TEST] → API → Impl → Test → feature_addition

## TO-012-005 — DomainId not applicable for B15
B15 has no gaps. No GapHandler invoked. No SCHEDULE_CAPABILITY_LEARNING emitted.
DomainId is only relevant for capability gap batches (B33, B34, B32, B36, B37).
