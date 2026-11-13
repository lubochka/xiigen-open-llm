# Held-Out and Corruption Pairs (014)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H014-001 — Held-out: QA+IMPLEMENT — canary controller

A: "Build the canary release controller and QA the routing logic before the 5% rollout."
  Expected: ["QA", "IMPLEMENT"]
  Held-out: "build" → IMPLEMENT; "controller" → code artifact; "QA" → QA. "rollout" → gap. feature_addition.

---

## H014-002 — Held-out: QA+DOCUMENT — canary criteria

A: "Document the canary release criteria and get QA approval before promoting to 100%."
  Expected: ["QA", "DOCUMENT"]
  Held-out: "document" → DOCUMENT; "criteria" → DOCUMENT (dedup); "QA approval" → QA (approval in QA compound). documentation.

---

## H014-003 — Held-out: gap — canary percentage

A: "Promote the canary to 50% traffic after error rate stabilizes."
  Expected: []
  Held-out: "promote canary", "traffic percentage", "error rate stabilizes" → all gap. knowledge_edge.

---

## C014-001 — Corruption: "rollout" incorrectly fires IMPLEMENT

A: "Plan the canary rollout strategy."
  Correct: [] — gap ("rollout" is gap vocabulary; "plan" alone is gap)
  Corrupt: ["IMPLEMENT"] ← wrong ("rollout" treated as implementation)
  Teaching: "rollout" is gap vocabulary. Does not fire IMPLEMENT.

---

## C014-002 — Corruption: QA drives TransitionType instead of IMPLEMENT

A: "Build the canary service and QA approve."
  Correct: ["IMPLEMENT", "QA"] | TransitionType: feature_addition (driven by IMPLEMENT)
  Corrupt: ["IMPLEMENT", "QA"] | TransitionType: qa_approval ← invented (wrong)
  Teaching: QA never contributes TransitionType. IMPLEMENT drives feature_addition.
