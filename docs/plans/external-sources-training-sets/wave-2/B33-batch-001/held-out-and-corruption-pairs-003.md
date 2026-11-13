# Held-Out and Corruption Pairs (003)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-003-A — Held-out: "Teach the agent to classify intent"

A: "Teach the agent to classify customer support intents."
B: Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | gap-ai-agent-skill-001
Reason: "teach" = AI training/capability vocabulary; not a code verb.
Phrasing-B: Gap — "teach the agent" is capability-acquisition vocabulary, never IMPLEMENT.

---

## HC-003-B — Held-out: "Build the intent classifier"

A: "Build the customer support intent classifier."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | TransitionType: feature_addition
Reason: "build" = code verb; "intent classifier" = software component. Code verb overrides AI object.
Phrasing-B: IMPLEMENT fires — "build the [noun]" is the code-verb pattern even with AI objects.

---

## HC-003-C — Corruption v6-02: Gap suppressed to IMPLEMENT

v6 corrupt: A: "Teach the agent to handle legal queries" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler
Error type: v6-02 — gap suppression. "teach" is AI vocabulary; it does not fire IMPLEMENT.
Fix: Only code-action verbs fire IMPLEMENT. "teach" is not a code-action verb regardless of object.
