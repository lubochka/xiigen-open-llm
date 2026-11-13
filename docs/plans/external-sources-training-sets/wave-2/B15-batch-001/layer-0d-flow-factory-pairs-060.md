# Layer 0-D — Flow Factory Selection Pairs (060)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-060-001 — Full MonitoringService factory sequence (049–060 reference case)

A: "Build MonitoringService, integrate with Prometheus, secure the metrics endpoint, document health checks, and QA before release."
Factory calls (execution order):
1. API_CONTRACT → 3.3 → universal-nine-node-contract-flow-3.3 (deterministic_generic)
2. SECURITY → security → universal-security-audit-flow (deterministic_generic)
3. IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1 (deterministic_generic)
4. DOCUMENT → 2.1 → universal-nine-node-documentation-flow-2.1 (parallel with 4.1)
5. QA → 7.1 → universal-qa-gating-flow-7.1 (deterministic_generic) — last

---

## B15-factory-060-002 — Complete factory ID reference for 049–060 range

All 049–060 services use identical factory IDs:
- IMPLEMENT: universal-nine-node-development-flow-4.1
- ARCHITECTURE: universal-nine-node-architecture-flow-3.1
- API_CONTRACT: universal-nine-node-contract-flow-3.3
- SECURITY: universal-security-audit-flow
- TEST: universal-nine-node-testing-flow-5.1 (5.3 for E2E only)
- DOCUMENT: universal-nine-node-documentation-flow-2.1
- QA: universal-qa-gating-flow-7.1 (7.3 for production release gate only)

---

## B15-factory-060-003 — refactoring TransitionType confirmed for 049–060 services

CacheService (054): "Refactor CacheService" → feature: universal-nine-node-development-flow-4.1 + TransitionType: refactoring.
CircuitBreakerService (056): "Refactor CircuitBreakerService" → same.
CompensationService (052): "Refactor CompensationService" → same.
All three: TransitionType: refactoring is metadata field on ImplementationFlow41 call only.

---

## B15-factory-060-004 — QA factory: 7.1 default, 7.3 for explicit production release

"QA before release" (unqualified) → universal-qa-gating-flow-7.1 (staging default).
"QA before production release" or "production release gate" → universal-release-gate-flow-7.3.
MonitoringService (059): "QA the alert thresholds before release" → 7.1 default.

---

## B15-factory-060-005 — deterministic_generic confirmed for all 037–060 factory calls

No probationary or trusted states for any service in 037–060 range.
All factory calls: TrustState: deterministic_generic.
