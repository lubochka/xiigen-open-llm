# Held-Out and Corruption Pairs (013)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H013-001 — Held-out: QA alone — blue-green gate

A: "QA the blue-green cutover for the CheckoutService before production."
  Expected: ["QA"]
  Held-out: "QA" abbreviation fires QA. "cutover" is gap. QA alone → no TransitionType.

---

## H013-002 — Held-out: QA+IMPLEMENT — blue-green service

A: "Build the blue-green deployment service and QA the traffic routing."
  Expected: ["QA", "IMPLEMENT"]
  Held-out: "build" → IMPLEMENT; "service" → IMPLEMENT (dedup); "QA" → QA. feature_addition.

---

## H013-003 — Held-out: gap — traffic switch

A: "Switch traffic to the green stack and decommission the blue stack."
  Expected: []
  Held-out: "switch traffic" and "decommission" are pure gap vocabulary. knowledge_edge.

---

## C013-001 — Corruption: "cutover" incorrectly fires IMPLEMENT

A: "Execute the blue-green cutover."
  Correct: [] — gap
  Corrupt: ["IMPLEMENT"] ← wrong ("cutover" treated as implementation verb)
  Teaching: "cutover" is gap vocabulary. It does not fire IMPLEMENT. Only "build/implement/create [artifact]" fires IMPLEMENT.

---

## C013-002 — Corruption: QA+IMPLEMENT collapsed to QA alone

A: "Build the blue-green service and QA the gate."
  Correct: ["IMPLEMENT", "QA"]
  Corrupt: ["QA"] ← wrong (IMPLEMENT suppressed)
  Teaching: "build" fires IMPLEMENT independently of QA. Both predicates must be present.
