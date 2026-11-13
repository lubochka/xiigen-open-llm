# Held-Out and Corruption Pairs (004)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-004-A — Held-out: "prompt injection prevention" SECURITY edge

A: "Add prompt injection prevention middleware to the agent API gateway."
B: Predicates: ["IMPLEMENT", "SECURITY"] | ImplementationFlow41 → SecurityAuditFlow | TransitionType: feature_addition
Reason: "add" + middleware (code artifact) → IMPLEMENT; "prompt injection" → SECURITY. Both fire.
Phrasing-B: IMPLEMENT+SECURITY — "add [code artifact]" fires IMPLEMENT; "prompt injection" fires SECURITY independently.

---

## HC-004-B — Held-out: "system prompt" — no predicate

A: "Rewrite the agent's system prompt to be more restrictive."
B: Predicates: [] | GapHandler → gap-ai-agent-llm-001
Reason: "rewrite system prompt" = AI configuration vocabulary. "rewrite" in this context is not a code-action verb.
Phrasing-B: Gap — system prompt rewriting is AI configuration, not software code artifact creation.

---

## HC-004-C — Corruption v6-01: PROMPT predicate invented

v6 corrupt: A: "Optimize the system prompt" → Predicates: ["PROMPT"] | PromptOptimizationFlow invoked
Correct: Predicates: [] → GapHandler
Error type: v6-01 — PROMPT does not exist in the nine-predicate lexicon. No prompt flow exists.
Fix: "prompt" (other than prompt injection) is AI vocabulary → gap. No predicate fires.
