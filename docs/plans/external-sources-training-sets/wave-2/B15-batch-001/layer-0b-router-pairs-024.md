# Layer 0-B — Router Pairs (024)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-024-001 — Complete flow routing table for B15 predicates

| Predicate | Flow | Sub-phase |
|-----------|------|-----------|
| IMPLEMENT | ImplementationFlow41 | 4.1 |
| API_CONTRACT | APIContractFlow33 | 3.3 |
| ARCHITECTURE | ArchitectureFlow31 | 3.1 |
| SECURITY | SecurityAuditFlow | security phase |
| TEST (unit/integration) | TestingFlow51 | 5.1 |
| TEST (E2E / end-to-end) | E2ETestingFlow53 | 5.3 |
| DOCUMENT | DocumentationFlow21 | 2.1 |
| QA | QAGatingFlow73 | 7.1 or 7.3 |

---

## B15-router-024-002 — Ordering rules summary

1. ARCHITECTURE before API_CONTRACT before IMPLEMENT (contract informs implementation; architecture precedes all).
2. SECURITY before IMPLEMENT (security constraints define implementation constraints).
3. IMPLEMENT before TEST (implement first, then test).
4. QA always last (absolute ordering; no exception).
5. DOCUMENT: parallel-eligible with IMPLEMENT (documentation can proceed alongside implementation).

---

## B15-router-024-003 — DecisionCount rule

DecisionCount = number of unique predicates that fired after deduplication.
Multiple triggers for same predicate → still one routing decision for that predicate.
Example: three IMPLEMENT triggers → DecisionCount contribution from IMPLEMENT: 1.

---

## B15-router-024-004 — NearestWrongSubPhaseScore reference

| Flow | NearestWrong | Typical Score |
|------|-------------|---------------|
| 4.1 (IMPLEMENT) | 4.2 Integration | 0.40 |
| 3.3 (API_CONTRACT) | 3.1 Architecture | 0.36 |
| 3.1 (ARCHITECTURE) | 3.3 API Contract | 0.38 |
| 5.1 (TEST) | 5.3 E2E | 0.42 |

---

## B15-router-024-005 — TransitionType propagation through router

TransitionType: refactoring set by compiler (when "refactor" verb detected) must propagate as metadata through router to flow factory and flow execution. Router does not reset TransitionType. Flow execution reads TransitionType from SelectedLogicalTransition.
