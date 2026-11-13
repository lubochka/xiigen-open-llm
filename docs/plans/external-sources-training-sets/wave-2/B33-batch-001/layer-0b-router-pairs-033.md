# Layer 0-B — Router Pairs (033)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-033-001 — Gap → GapHandler: tokenize + embed compound

A: [] | "Tokenize the queries and embed them using the sentence model."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "tokenize" and "embed" both = AI model operations. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-033-002 — Gap → GapHandler: context window + attention compound

A: [] | "Manage the context window and tune the attention mechanism for long documents."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "context window" and "attention mechanism" both = AI model properties. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-033-003 — IMPLEMENT: create serving router

A: ["IMPLEMENT"] | "Create a serving router that dispatches requests to the appropriate inference endpoint."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "create" fires IMPLEMENT. AI vocabulary in object position.

---

## C0B-033-004 — Gap → GapHandler: hallucination + RAG compound

A: [] | "Apply hallucination mitigation to the RAG pipeline output."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "hallucination mitigation" and "RAG pipeline" both = AI vocabulary. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-033-005 — API_CONTRACT: wire tokenizer service

A: ["API_CONTRACT"] | "Wire the tokenizer service into the preprocessing pipeline via API."
B: API_CONTRACT → APIContractFlow33.
Teaching: "wire into" fires API_CONTRACT. "tokenizer service" = object context.
