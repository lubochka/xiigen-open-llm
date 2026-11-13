# Held-Out and Corruption Pairs (016)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-016-A — Held-out: "prompt engineering" boundary cases

A (gap): "Invest in prompt engineering for the agent." → Predicates: [] → GapHandler → gap-ai-agent-llm-001
A (SECURITY): "Prevent prompt injection attacks on agent input." → Predicates: ["SECURITY"]
A (IMPLEMENT): "Build a prompt engineering toolkit." → Predicates: ["IMPLEMENT"]
Key distinction: "prompt engineering" → gap. "prompt injection" → SECURITY. "build prompt toolkit" → IMPLEMENT.

---

## HC-016-B — Held-out: "prompt" alone is not injection

A: "Write a better system prompt for the support agent."
B: Predicates: [] | GapHandler → gap-ai-agent-llm-001
Reason: "write a system prompt" = AI configuration → gap. Only "prompt injection" fires SECURITY. "prompt" alone → gap.
Phrasing-B: Gap — writing prompts is AI configuration, not code.

---

## HC-016-C — Corruption v6-02: "prompt engineering" incorrectly fires IMPLEMENT

v6 corrupt: "Invest in prompt engineering" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler → gap-ai-agent-llm-001
Error: v6-02. "invest in" is not a code verb. "prompt engineering" is AI vocabulary. Gap.

---

## HC-016-D — Held-out: "prompt engineering" + DOCUMENT

A: "Document the prompt engineering best practices for the team."
B: Predicates: ["DOCUMENT"] | GapHandler suppressed; gap-ai-agent-llm-001 silently skipped.
Reason: "document" fires DOCUMENT. "prompt engineering" is the documentation subject (AI context). DOCUMENT fires regardless.
Phrasing-B: DOCUMENT fires on "document" verb even with AI vocabulary in the object.
