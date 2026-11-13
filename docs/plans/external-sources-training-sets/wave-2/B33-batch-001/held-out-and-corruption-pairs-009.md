# Held-Out and Corruption Pairs (009)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-009-A — Held-out: "context window" gap

A: "Optimize the agent's context window usage."
B: Predicates: [] | GapHandler → gap-ai-agent-llm-001
Reason: "optimize context window" = AI configuration/tuning vocabulary → gap.
Phrasing-B: Gap — context window optimization is AI operational vocabulary, not a code artifact task.

---

## HC-009-B — Held-out: "token budget" gap

A: "The agent is exceeding its token budget on complex queries."
B: Predicates: [] | GapHandler → gap-ai-agent-llm-001
Reason: Observation statement + AI configuration vocabulary → gap.
Phrasing-B: Gap — "exceeding token budget" is an observation about AI behavior. No action verb fires any predicate.

---

## HC-009-C — Held-out: "few-shot examples" gap

A: "Add few-shot examples to improve the agent's reasoning."
B: Predicates: [] | GapHandler → gap-ai-agent-llm-001
Reason: "add few-shot examples" = AI prompting technique → gap. "add" here refers to prompt modification, not code artifact addition.
Phrasing-B: Gap — adding few-shot examples is AI prompt engineering vocabulary. No software component created.

---

## HC-009-D — Held-out: "add" disambiguation (code vs AI)

Two meanings of "add" in B33 context:
- "Add a retry handler to the agent gateway" → IMPLEMENT ("add" + software component)
- "Add few-shot examples to the prompt" → gap ("add" + AI prompt content)
Rule: "add" fires IMPLEMENT only when followed by a software component/artifact noun. "add" + AI content/behavior → gap.
