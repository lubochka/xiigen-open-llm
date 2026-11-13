# Held-Out and Corruption Pairs (007)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-007-A — Held-out: "Evaluate the agent's performance"

A: "Evaluate the agent's performance on benchmark tasks."
B: Predicates: [] | GapHandler → gap-ai-agent-llm-001
Reason: "evaluate" (AI performance) = assessment vocabulary → gap. Not a code-action verb.
Phrasing-B: Gap — AI performance evaluation is observational, not software implementation.

---

## HC-007-B — Held-out: "Build evaluation harness" vs "evaluate"

A: "Create an evaluation harness to measure the agent's accuracy."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | TransitionType: feature_addition
Reason: "create" = code verb; "evaluation harness" = software component (harness). Code verb overrides.
Phrasing-B: IMPLEMENT — "create an [noun] harness" is code-action. "Evaluate" alone (no artifact) is gap.

---

## HC-007-C — Held-out: Multi-verb with gap and code verbs

A: "Evaluate the agent's accuracy and create a reporting dashboard for the metrics."
Analysis: "evaluate" → gap; "create a reporting dashboard" → IMPLEMENT (create + artifact)
B: Predicates: ["IMPLEMENT"] | GapHandler NOT invoked
TransitionType: feature_addition
Phrasing-B: IMPLEMENT fires — "create a [noun]" fires IMPLEMENT; gap verb "evaluate" is silently skipped.

---

## HC-007-D — Corruption v6-01: "evaluate" maps to TESTING predicate

v6 corrupt: "Evaluate the agent's benchmark" → Predicates: ["TEST"]
Correct: Predicates: [] → GapHandler
Error type: v6-01 — evaluate (AI quality) ≠ TEST. TEST fires on test-writing verbs (write/create tests), not evaluation.
