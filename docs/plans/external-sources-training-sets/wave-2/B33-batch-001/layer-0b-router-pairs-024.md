# Layer 0-B — Router Pairs (024)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-024-001 — Gap → GapHandler: full B33 gap vocabulary, no code verb

A: [] | "Adapt the model, fine-tune the model, evolve the system, apply prompt engineering, use self-healing, add reinforcement, enable few-shot, use chain-of-thought, orchestrate, build skill, use agent."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: All B33 gap vocabulary, no code verb present. Gap ID: gap-ai-agent-llm-001 (dominant: LLM quality vocabulary).

---

## C0B-024-002 — IMPLEMENT: code verb overrides all AI objects

A: ["IMPLEMENT"] | "Build an agent that uses few-shot learning, chain-of-thought, and self-healing reinforcement."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. All AI vocabulary (few-shot, chain-of-thought, self-healing, reinforcement) is in object position. Code verb wins.

---

## C0B-024-003 — API_CONTRACT fires even with AI vocabulary

A: ["API_CONTRACT"] | "Integrate the fine-tuning orchestrator with the model registry API."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" always fires API_CONTRACT. "fine-tuning orchestrator" and "model registry" are AI context only. API_CONTRACT is not suppressed by AI vocabulary.

---

## C0B-024-004 — Gap: requirement statement with multiple AI terms

A: [] | "The orchestrator needs reinforcement and self-healing capabilities with few-shot learning support."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Requirement statement without code verb. "reinforcement", "self-healing", "few-shot learning", "orchestrator" → all gap. Gap ID: gap-ai-agent-adapt-001.

---

## C0B-024-005 — All seven predicates fire: complete B33 vocabulary + all code verbs

A: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] | "Architect the agent skill platform, integrate the orchestrator API, implement the fine-tuning scheduler, secure against prompt injection, write tests for chain-of-thought output, document the self-healing policy, and QA sign-off."
B: ARCHITECTURE → 3.1; API_CONTRACT → APIContractFlow33; IMPLEMENT → 4.1; SECURITY → SecurityAuditFlow; TEST → 5.1; DOCUMENT → DocumentationFlow62; QA → QAGatingFlow73 (7.3 — last, absolute). Seven flows.
