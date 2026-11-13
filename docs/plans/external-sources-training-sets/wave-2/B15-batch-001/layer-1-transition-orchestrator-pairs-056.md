# Layer 1 — Transition Orchestrator Pairs (056)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-056-001 — Complete predicate-to-SubPhaseId mapping (final reference)

| Predicate | SubPhaseId |
|-----------|-----------|
| ARCHITECTURE | "3.1" |
| API_CONTRACT | "3.3" |
| SECURITY | "security" |
| IMPLEMENT | "4.1" |
| TEST (unit/integration) | "5.1" |
| TEST (E2E) | "5.3" |
| DOCUMENT | "2.1" |
| QA (staging) | "7.1" |
| QA (production) | "7.3" |

---

## B15-t1-056-002 — v6 corruption risk in SubPhaseIds (orchestrator level)

v6-01: SubPhaseIds: [] (empty) — IMPLEMENT suppressed by domain noun. Incorrect for all B15 services.
v6-03: SubPhaseIds: ["4.1"] when multiple predicates are present — multi-predicate collapsed. Incorrect.
Correct: SubPhaseIds contains one entry per unique predicate after deduplication.

---

## B15-t1-056-003 — "process" verb → feature_addition TransitionType (confirmed)

A: "Process incoming payment events in the EventConsumerService."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | ContextCarryForward: true
"process" → IMPLEMENT. feature_addition TransitionType.

---

## B15-t1-056-004 — "code" and "generate" verbs → feature_addition TransitionType

A: "Code the retry logic for TransientFailureHandler in RetryService."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | ContextCarryForward: true
"code" → IMPLEMENT. feature_addition.

---

## B15-t1-056-005 — Zero-predicate requests impossible in B15

All B15 requests contain at least one IMPLEMENT trigger verb.
SubPhaseIds always has at least ["4.1"].
GapHandler is never triggered in the B15 scenario.
