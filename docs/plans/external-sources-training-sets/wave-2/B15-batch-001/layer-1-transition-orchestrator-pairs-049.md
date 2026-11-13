# Layer 1 — Transition Orchestrator Pairs (049)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-049-001 — Complete SubPhaseIds ordering reference for 028–060

SubPhaseIds ordering (all B15 services):
1. "3.1" — ARCHITECTURE (always first when present)
2. "3.3" — API_CONTRACT
3. "security" — SECURITY
4. "4.1" — IMPLEMENT (always present)
5. "5.1" — TEST (unit/integration) or "5.3" (E2E)
6. "2.1" — DOCUMENT (parallel-eligible with "4.1")
7. "7.1" — QA staging or "7.3" — QA production (always last)

---

## B15-t1-049-002 — ContextCarryForward: true for all B15 transitions (confirmed)

ContextCarryForward: true on every transition in 028–060.
Architecture output feeds API_CONTRACT input. Implementation output feeds TEST input.
No B15 transition sets ContextCarryForward: false.

---

## B15-t1-049-003 — TransitionType: refactoring confirmed for 028–060 range

Services with confirmed refactoring TransitionType:
- CustomerProfileService (029): "Refactor CustomerProfileService to split read and write models"
- ExportService (036): "Refactor ExportService to support streaming exports"
- MigrationService (037): "Refactor MigrationService to use a streaming pipeline"
- CompensationService (041): "Refactor CompensationService to use a registry pattern"
- CacheService (043): "Refactor CacheService to add two-level cache hierarchy"
- CircuitBreakerService (045): "Refactor CircuitBreakerService to add threshold configuration"

---

## B15-t1-049-004 — "define contracts" dual SubPhaseIds rule confirmed for 028–060

"define [X] contracts" → API_CONTRACT + DOCUMENT → SubPhaseIds includes both "3.3" and "2.1".
Confirmed for: ExportService (036), SagaOrchestrator (040), IdempotencyService (042).

---

## B15-t1-049-005 — QA "7.1" vs. "7.3" in SubPhaseIds

"QA before release" (unqualified) → "7.1" in SubPhaseIds (staging gate default).
"QA before production release" → "7.3" in SubPhaseIds (production gate).
MonitoringService (048): "QA before release" → "7.1".
