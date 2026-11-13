# Layer 0-A — Compiler Pairs (051)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Firebase connector

---

## B17-0A-051-001 — Firebase: predicate firing

**Input:** "Build the Firebase Realtime Database connector with webhook push notifications."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| webhook | API_CONTRACT | 5 (reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Integration vocabulary present: connector, webhook → IMPLEMENT routes to 4.3

---

## B17-0A-051-002 — Firebase: service account token fires SECURITY

**Input:** "Implement the Firebase connector with service account authentication token."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-051-003 — Firebase: OAuth + token

**Input:** "Build the Firebase connector with OAuth 2.0 access token for admin SDK operations."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0A-051-004 — Firebase: "integrate" alone

**Input:** "Integrate the Firebase Cloud Messaging API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — Firebase domain has no exception to Rule 1.

---

## B17-0A-051-005 — Firebase: push/sync verbs not IMPLEMENT

**Input:** "Push data updates to Firebase Realtime Database and sync offline state."

Triggers: none ("push" and "sync" are not IMPLEMENT triggers)
CompiledPredicateIds: []
TransitionType: none
Teaching: "push", "sync", "update" do not fire IMPLEMENT.
