# Held-Out and Corruption Pairs (002)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-002-A — Held-out: "Adapt the agent's response style"

A: "Adapt the agent's response style to be more formal."
B: Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | gap-ai-agent-adapt-001 | DomainId: software-development
Reason: "adapt" = AI vocabulary; "response style" = AI behavior; no code verb present.
Phrasing-B: Gap — response style adaptation is AI behavioral vocabulary, not a code-action task.

---

## HC-002-B — Corruption v6-01: Invented ADAPTATION predicate

v6 corrupt: A: "Adapt the agent's tone" → Predicates: ["ADAPTATION"] | AdaptationFlow invoked
Correct: Predicates: [] | GapHandler invoked
Error type: v6-01 — ADAPTATION does not exist in the nine-predicate lexicon. No adaptation flow exists.
Fix: AI adaptation vocabulary → gap only. Never invent predicates for AI vocabulary.

---

## HC-002-C — Held-out: "Implement tone control"

A: "Implement a tone control module for the agent."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | TransitionType: feature_addition
Reason: "implement" = code verb; "tone control module" = software component (module). Code verb overrides.
Phrasing-B: IMPLEMENT fires — "implement a [noun] module" is always a code-action task.
