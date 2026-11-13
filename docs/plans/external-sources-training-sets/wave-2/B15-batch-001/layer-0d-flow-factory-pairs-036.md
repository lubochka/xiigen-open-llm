# Layer 0-D — Flow Factory Selection Pairs (036)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-036-001 — Complete factory selection table for Round 2

| Predicate | Sub-phase | Universal Flow ID |
|-----------|-----------|-------------------|
| IMPLEMENT | 4.1 | universal-nine-node-development-flow-4.1 |
| API_CONTRACT | 3.3 | universal-nine-node-contract-flow-3.3 |
| ARCHITECTURE | 3.1 | universal-nine-node-architecture-flow-3.1 |
| SECURITY | security | universal-security-audit-flow |
| TEST (unit/integration) | 5.1 | universal-nine-node-testing-flow-5.1 |
| TEST (E2E) | 5.3 | universal-nine-node-e2e-testing-flow-5.3 |
| DOCUMENT | 2.1 | universal-nine-node-documentation-flow-2.1 |
| QA (staging) | 7.1 | universal-qa-gating-flow-7.1 |
| QA (production) | 7.3 | universal-release-gate-flow-7.3 |

---

## B15-factory-036-002 — TrustState transitions (reference)

| TrustState | Condition | Flow Selected |
|------------|-----------|---------------|
| deterministic_generic | No registered candidate | Universal (fallback) |
| probationary_accepted | Candidate registered, Gate B passed | Candidate |
| probationary_rejected | Candidate registered, Gate B failed | Universal (fallback) |
| trusted_learned | Promoted (≥25 evidence pairs, Gate B) | Candidate |

---

## B15-factory-036-003 — TransitionType passthrough summary (Round 2)

TransitionType: refactoring — set when "refactor" verb is present.
Scope: passed only to factory call for sub-phase 4.1 (ImplementationFlow41).
All other factory calls receive no TransitionType metadata.
ImplementationFlow41 uses TransitionType to determine if backward-compatibility check is required.

---

## B15-factory-036-004 — "develop" verb → factory call identical to "build"/"create"

All IMPLEMENT trigger verbs produce the same factory call:
Predicate: IMPLEMENT + Sub-phase: 4.1 → universal-nine-node-development-flow-4.1.
Verb identity (build/develop/code/generate/process) does not affect factory selection.

---

## B15-factory-036-005 — QA always last: factory call ordering enforced at orchestrator level

Factory calls are ordered by orchestrator (Layer 1) before factory execution.
QA factory call always placed last in the call sequence — after IMPLEMENT, TEST, DOCUMENT.
Factory itself does not enforce ordering — ordering is enforced by the transition orchestrator.
