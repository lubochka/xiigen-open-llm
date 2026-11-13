# Held-Out and Corruption Pairs (024)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-024-A — Held-out: complete B33 gap vocabulary — all gap

A: "Fine-tune, few-shot, chain-of-thought, prompt engineering, self-healing, reinforcement, orchestrate, adapt the model, evolve the system."
B: Predicates: [] | GapHandler → gap-ai-agent-llm-001 | One SCHEDULE_CAPABILITY_LEARNING emitted.
Teaching: Any number of gap vocabulary terms → one gap ID, one event, zero predicates.

---

## HC-024-B — Held-out: complete B33 gap vocabulary + single code verb

A: "Fine-tune, few-shot, chain-of-thought, prompt engineering, self-healing, reinforcement, orchestrate, adapt — but integrate the result with the training API."
B: Predicates: ["API_CONTRACT"] | GapHandler suppressed; all gap vocabulary silently skipped.
Teaching: A single "integrate" fires API_CONTRACT and suppresses all gap vocabulary in the request. One predicate is sufficient to suppress gap.

---

## HC-024-C — Corruption v6-03: all predicates collapsed to single predicate in rich request

v6 corrupt: "Fine-tune the model, build the training pipeline, integrate with the API, prevent injection, write tests, document, QA" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT", "QA"] (six predicates; "fine-tune" = gap silently skipped)
Error: v6-03 extreme collapse. Each code verb fires independently. No collapsing permitted.

---

## HC-024-D — Held-out: DomainId is always software-development for all B33 gaps

A (any B33 gap request): DomainId: software-development
Never: ai-development, ml-development, llm-development, agent-development.
This rule is absolute for all three B33 gap IDs: gap-ai-agent-adapt-001, gap-ai-agent-skill-001, gap-ai-agent-llm-001.
Phrasing-B: DomainId: software-development — no AI-specific domain exists in the registry.
