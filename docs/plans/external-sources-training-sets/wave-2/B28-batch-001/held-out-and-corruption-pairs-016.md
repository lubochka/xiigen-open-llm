# Held-Out and Corruption Pairs (016)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H016-001 — Held-out: QA+DOCUMENT — feature flag criteria

A: "Document the feature flag release criteria and QA sign-off on the gate conditions."
  Expected: ["QA", "DOCUMENT"]
  Held-out: "document" → DOCUMENT; "criteria" → DOCUMENT (dedup); "QA sign-off" → QA. documentation.

---

## H016-002 — Held-out: gap — feature flag toggle

A: "Enable the dark-mode feature flag for all users in production."
  Expected: []
  Held-out: "enable feature flag" → gap; "production" → gap. knowledge_edge.

---

## H016-003 — Held-out: QA alone — feature flag acceptance gate

A: "QA gate the feature flag rollout for the RecommendationService."
  Expected: ["QA"]
  Held-out: "QA gate" → QA; "rollout" → gap. QA alone → no TransitionType.

---

## C016-001 — Corruption: "feature flag" fires an invented predicate

A: "Enable the feature flag for the dark-mode."
  Correct: [] — gap
  Corrupt: ["FEATURE_FLAG"] ← invented predicate (wrong)
  Teaching: "feature flag" is gap vocabulary. No predicate named FEATURE_FLAG exists.

---

## C016-002 — Corruption: "criteria" not recognized as DOCUMENT trigger

A: "Document the release criteria and QA sign-off."
  Correct: ["DOCUMENT", "QA"] — "criteria" → DOCUMENT
  Corrupt: ["QA"] only ← DOCUMENT suppressed (wrong)
  Teaching: "criteria" always fires DOCUMENT. It is an independent DOCUMENT predicate trigger.
