# Layer 0-A — Compiler Pairs (024)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-024-001 — gap vocabulary reference: complete B33 gap trigger list

A: "Adapt the model, fine-tune the model, evolve the system, prompt engineering, self-healing, reinforcement, few-shot, chain-of-thought, orchestrate, skill, agent."
B: [] — all B33 gap vocabulary. No code verb present. Entire request → gap.
CompiledPredicateIds: []

---

## C0A-024-002 — IMPLEMENT overrides all AI objects: code verb wins

A: "Build an agent that uses few-shot learning, chain-of-thought reasoning, and self-healing reinforcement loops."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. All AI vocabulary (few-shot, chain-of-thought, self-healing, reinforcement) is object context. Code verb wins.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-024-003 — gap: B33 gap vocabulary without code verb

A: "The orchestrator needs reinforcement and self-healing capabilities with few-shot learning support."
B: [] — requirement statement. No code verb fires. "reinforcement", "self-healing", "few-shot learning", "orchestrator" → all gap vocabulary. Gap.
CompiledPredicateIds: []

---

## C0A-024-004 — API_CONTRACT fires even in pure AI vocabulary sentence

A: "Integrate the fine-tuning orchestrator with the model registry API."
B: ["API_CONTRACT"] — "integrate" always fires API_CONTRACT. "fine-tuning orchestrator" and "model registry" are AI context only. API_CONTRACT is not suppressed.
CompiledPredicateIds: ["API_CONTRACT"]

---

## C0A-024-005 — All B33 code predicates fire: full AI vocabulary + all code verbs

A: "Architect the agent skill platform, integrate the orchestrator API, implement the fine-tuning scheduler, secure against prompt injection, write tests for chain-of-thought output, document the self-healing policy, and QA sign-off."
B: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] — seven predicates. All AI vocabulary items are objects; all code verbs fire independently.
CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
