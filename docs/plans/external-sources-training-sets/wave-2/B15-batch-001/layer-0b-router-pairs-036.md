# Layer 0-B — Router Pairs (036)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-036-001 — Complete routing table for Round 2 services

| Predicate Set | Flow Order | DecisionCount |
|--------------|-----------|---------------|
| IMPLEMENT only | 4.1 | 1 |
| IMPLEMENT+SECURITY | SecurityAudit → 4.1 | 2 |
| IMPLEMENT+API_CONTRACT | 3.3 → 4.1 | 2 |
| IMPLEMENT+ARCHITECTURE | 3.1 → 4.1 | 2 |
| IMPLEMENT+TEST | 4.1 → 5.1 | 2 |
| IMPLEMENT+DOCUMENT | 4.1 ‖ 2.1 | 2 |

---

## B15-router-036-002 — Complete routing ordering rules summary (Round 2)

1. ARCHITECTURE (3.1) before API_CONTRACT (3.3) before SECURITY before IMPLEMENT (4.1)
2. IMPLEMENT (4.1) before TEST (5.1/5.3)
3. DOCUMENT (2.1) parallel with IMPLEMENT and TEST
4. QA (7.1/7.3) always last — no exception
5. TransitionType propagated unchanged through all routing decisions

---

## B15-router-036-003 — NearestWrongSubPhase complete reference

| Flow | NearestWrong | Score |
|------|-------------|-------|
| 4.1 (IMPLEMENT) | 4.2 Integration | 0.40 |
| 3.3 (API_CONTRACT) | 3.1 Architecture | 0.36 |
| 3.1 (ARCHITECTURE) | 3.3 API Contract | 0.38 |
| 5.1 (TEST) | 5.3 E2E | 0.42 |
| SecurityAuditFlow | 3.3 API Contract | 0.35 |
| 2.1 (DOCUMENT) | 3.3 API Contract | 0.33 |

---

## B15-router-036-004 — "develop" verb routes to 4.1

"develop" → IMPLEMENT → ImplementationFlow41 (sub-phase 4.1).
Same flow as "build", "create", "implement", "scaffold". No special routing for any IMPLEMENT verb variant.

---

## B15-router-036-005 — DecisionCount anti-pattern: must match predicate count

Anti-pattern: returning DecisionCount: 1 for a three-predicate request.
Anti-pattern: routing all predicates to a single flow instead of separate flows.
Correct: one factory call and one flow per unique predicate after deduplication.
