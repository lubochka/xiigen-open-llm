# Layer 0-B — Router Pairs (025)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-025-001 — Gap → GapHandler: "tokenize" vocabulary

A: [] | "Tokenize the input before passing it to the model."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "tokenize" = AI model-level operation. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-025-002 — IMPLEMENT: code verb overrides tokenize object

A: ["IMPLEMENT"] | "Build a tokenizer that splits the input text into subword units."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "tokenizer" = implementation artifact. AI vocabulary in object position.

---

## C0B-025-003 — Gap → GapHandler: "embed" vocabulary

A: [] | "Embed the user query using the sentence transformer."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "embed" = AI model embedding operation. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-025-004 — API_CONTRACT: integrate embedding endpoint

A: ["API_CONTRACT"] | "Integrate the embedding endpoint into the search pipeline."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" always fires API_CONTRACT. "embedding endpoint" is AI context in object position.

---

## C0B-025-005 — IMPLEMENT + API_CONTRACT: build and integrate

A: ["IMPLEMENT", "API_CONTRACT"] | "Implement the vector search module and integrate it with the Pinecone API."
B: IMPLEMENT → ImplementationFlow41; API_CONTRACT → APIContractFlow33. Two flows.
Teaching: Two code verbs, two predicates. Ordering: IMPLEMENT → API_CONTRACT (contract informs impl, but both fired here).
