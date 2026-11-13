# Held-Out and Corruption Pairs (017)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-017-A — Held-out: "self-healing" boundary cases

A (gap): "The agent should be self-healing." → Predicates: [] → GapHandler → gap-ai-agent-adapt-001
A (IMPLEMENT): "Implement self-healing retry logic for the agent." → Predicates: ["IMPLEMENT"]
A (ARCHITECTURE): "Design the self-healing architecture for failure recovery." → Predicates: ["ARCHITECTURE"]
Key distinction: AI behavioral property → gap. Code verb + software artifact → IMPLEMENT. Design verb + structural noun → ARCHITECTURE.

---

## HC-017-B — Held-out: "self-healing" as adjective does not suppress code verbs

A: "Build a self-healing circuit breaker for the agent service."
B: Predicates: ["IMPLEMENT"] | GapHandler not invoked.
Reason: "build" = code verb. "circuit breaker" = software artifact. "self-healing" is adjective modifying the artifact. Code verb fires.
Phrasing-B: IMPLEMENT fires — adjective "self-healing" does not gap-ify "build".

---

## HC-017-C — Corruption v6-01: "SELF_HEAL" invented predicate

v6 corrupt: "Enable self-healing in the agent" → Predicates: ["SELF_HEAL"]
Correct: Predicates: [] → GapHandler → gap-ai-agent-adapt-001
Error: v6-01. SELF_HEAL does not exist in the nine-predicate lexicon.

---

## HC-017-D — Held-out: "self-healing" + SECURITY mixed

A: "Enable self-healing and prevent prompt injection at the agent boundary."
B: Predicates: ["SECURITY"] | GapHandler suppressed; gap-ai-agent-adapt-001 silently skipped.
Reason: "prevent prompt injection" → SECURITY fires. "enable self-healing" = gap (skipped).
Phrasing-B: SECURITY fires from injection; self-healing gap is suppressed.
