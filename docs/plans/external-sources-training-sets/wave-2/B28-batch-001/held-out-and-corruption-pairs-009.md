# Held-Out and Corruption Pairs (009)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H009-001 — Held-out: QA — "QA pass"

A: "The build needs a QA pass before it goes to beta users."
  Expected: ["QA"]
  Held-out: "QA pass" is a QA compound noun → fires QA predicate.

---

## H009-002 — Held-out: ARCHITECTURE+QA

A: "Design the service mesh topology and get QA sign-off on the architecture."
  Expected: ["ARCHITECTURE", "QA"]
  Held-out: "design service mesh topology" → ARCHITECTURE (sub-phase 3.5); "QA sign-off" → QA. TransitionType: documentation (IMPLEMENT absent).

---

## H009-003 — Held-out: gap — "feature flag rollout"

A: "Gradually roll out the new search feature using feature flags."
  Expected: []
  Held-out: Feature flag rollout without QA abbreviation or test-writing verb → gap. Operational deployment activity.

---

## C009-001 — Corruption v6-01: RELEASE invented

A: "QA sign-off needed before the release."
  Correct: ["QA"]
  Corrupt: ["QA", "RELEASE"] ← v6-01 (RELEASE is not a valid predicate)
  Teaching: RELEASE is not a predicate. Release context does not add a predicate. QA fires via abbreviation.

---

## C009-002 — Corruption v6-03: ARCHITECTURE+QA collapsed to ARCHITECTURE only

A: "Architect the data layer and get QA sign-off."
  Correct: ["ARCHITECTURE", "QA"]
  Corrupt: ["ARCHITECTURE"] ← v6-03 (QA suppressed)
  Teaching: QA predicate is never suppressible. "QA sign-off" fires QA predicate independently regardless of other predicates present.
