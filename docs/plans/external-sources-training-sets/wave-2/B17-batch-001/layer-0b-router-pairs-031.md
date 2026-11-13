# Layer 0-B — Router Pairs (031)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Segment analytics connector routing

---

## B17-0B-031-001 — SegmentAnalyticsConnector routing table

| Input pattern | Predicates | SubPhase(s) | TransitionType | DecisionCount |
|---|---|---|---|---|
| Build Segment connector with event endpoint | [IMPL, API] | 4.3, 3.3 | feature_addition | 2 |
| Integrate Segment customer data platform | [API] | 3.3 | documentation | 1 |
| Build Segment connector with write key token | [IMPL, API, SEC] | 4.3, 3.3, 3.4 | feature_addition | 3 |
| Refactor Segment connector, write unit tests | [IMPL, API, TEST] | 4.3, 3.3, 5.1 | refactoring | 3 |
| Build Segment connector, document event schema | [IMPL, API, DOC] | 4.3, 3.3, 2.1 | feature_addition | 3 |

---

## B17-0B-031-002 — Segment: refactor with test routing

Input: "Refactor the Segment connector and write unit tests for track and identify methods."
- "Refactor" → IMPLEMENT (TransitionType=refactoring)
- "connector" → API_CONTRACT (integration vocabulary → 4.3)
- "write unit tests" → TEST

DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 5.1) | TransitionType: refactoring
TransitionType hierarchy: refactoring > feature_addition — refactoring wins.

---

## B17-0B-031-003 — Segment vs Mixpanel: routing equivalence

Segment and Mixpanel share identical predicate patterns for the same input shapes:
- "Build [X] connector with event endpoint" → [IMPL, API] → 4.3 + 3.3
- "Integrate [X]" → [API] → 3.3
- "Build [X] connector with token" → [IMPL, API, SEC] → 4.3 + 3.3 + 3.4

The connector name (Segment vs Mixpanel) does NOT change routing. Only the vocabulary matters.
