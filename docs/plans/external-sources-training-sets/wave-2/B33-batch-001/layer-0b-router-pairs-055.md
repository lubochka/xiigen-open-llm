# Layer 0-B — Router Pairs (055)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-055-001 — Gap: AI agent + neural network combined

A: [] | "AI agents use neural networks for decision-making and action selection."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive. No code verb. Both gap terms. Gap ID: gap-ai-ml-model-001. One event.

---

## C0B-055-002 — IMPLEMENT: build AI agent with neural network backbone

A: ["IMPLEMENT"] | "Build an AI agent that uses a neural network backbone for intent classification."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. Both AI terms in object position.

---

## C0B-055-003 — Gap: LLM + training vocabulary bridge

A: [] | "The LLM was pretrained on a large dataset using gradient descent and fine-tuned with RLHF."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive history. No code verb. Multiple gap terms. → gap-ai-ml-model-001. One event.

---

## C0B-055-004 — IMPLEMENT+SECURITY: implement LLM client + secure prompt injection

A: ["IMPLEMENT", "SECURITY"] | "Implement the LLM client library and secure it against prompt injection attacks."
B: IMPLEMENT → ImplementationFlow41; SECURITY → SecurityAuditFlow.
Teaching: "implement" fires IMPLEMENT; "secure" + "prompt injection" fires SECURITY. Standard order.

---

## C0B-055-005 — DOCUMENT+QA: document AI stack + QA

A: ["DOCUMENT", "QA"] | "Document the LLM integration design and QA the vector database connector."
B: DOCUMENT → DocumentationFlow62; QA → QAGatingFlow73 (last, absolute).
Teaching: "document" fires DOCUMENT; "QA" fires QA. QA always last.
