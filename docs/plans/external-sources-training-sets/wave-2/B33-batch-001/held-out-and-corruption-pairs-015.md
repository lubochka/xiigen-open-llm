# Held-Out and Corruption Pairs (015)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-015-A — Held-out: "chain of thought" boundary cases

A (gap): "Enable chain of thought reasoning in the agent." → Predicates: [] → GapHandler → gap-ai-agent-llm-001
A (IMPLEMENT): "Implement the chain-of-thought prompt renderer." → Predicates: ["IMPLEMENT"] → ImplementationFlow41
A (ARCHITECTURE): "Design the chain-of-thought reasoning layer." → Predicates: ["ARCHITECTURE"] → ArchitectureFlow31
Distinction: AI reasoning vocabulary → gap. Code verb + software artifact → IMPLEMENT. Design verb + structural noun → ARCHITECTURE.

---

## HC-015-B — Held-out: "CoT" abbreviation boundary

A (gap): "Apply CoT (chain-of-thought) prompting to the agent." → Predicates: [] → GapHandler → gap-ai-agent-llm-001
A (IMPLEMENT): "Implement a CoT prompt formatter service." → Predicates: ["IMPLEMENT"]
Reason: Abbreviation CoT (chain-of-thought) follows same rules. "apply" in AI strategy → gap. "implement service" → IMPLEMENT.

---

## HC-015-C — Corruption v6-01: "CHAIN_OF_THOUGHT" invented predicate

v6 corrupt: "Enable chain of thought reasoning" → Predicates: ["CHAIN_OF_THOUGHT"]
Correct: Predicates: [] → GapHandler
Error: v6-01 (predicate invention). CHAIN_OF_THOUGHT does not exist in the nine-predicate lexicon.

---

## HC-015-D — Held-out: "chain of thought" + "prompt injection" mixed

A: "Enable chain of thought reasoning while preventing prompt injection."
B: Predicates: ["SECURITY"] | GapHandler suppressed; gap-ai-agent-llm-001 silently skipped.
Reason: "prevent prompt injection" → SECURITY fires. "enable chain of thought" = gap (silently skipped).
Phrasing-B: SECURITY fires from prompt injection; AI reasoning gap is suppressed.
