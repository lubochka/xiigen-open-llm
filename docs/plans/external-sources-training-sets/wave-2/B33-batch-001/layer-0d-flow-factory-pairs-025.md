# Layer 0-D — Flow Factory Pairs (025)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-025-001 — GapHandler: "tokenize" vocabulary

A: [] | "Tokenize the input before passing it to the model."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "tokenize" = AI model-level operation. No code verb. Single SCHEDULE_CAPABILITY_LEARNING emitted.

---

## FF-025-002 — IMPLEMENT: build tokenizer

A: ["IMPLEMENT"] | "Build a tokenizer that splits the input text into subword units."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "tokenizer" = implementation artifact.

---

## FF-025-003 — GapHandler: "embed" vocabulary

A: [] | "Embed the user query using the sentence transformer."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "embed" = AI model embedding operation. No code verb. Single SCHEDULE_CAPABILITY_LEARNING emitted.

---

## FF-025-004 — API_CONTRACT: integrate embedding endpoint

A: ["API_CONTRACT"] | "Integrate the embedding endpoint into the search pipeline."
Factory: [APIContractFlow33]
Teaching: "integrate" fires API_CONTRACT. AI vocabulary in object position.

---

## FF-025-005 — IMPLEMENT + API_CONTRACT: build + integrate

A: ["IMPLEMENT", "API_CONTRACT"] | "Implement the vector search module and integrate it with the Pinecone API."
Factory: [ImplementationFlow41, APIContractFlow33] | TransitionType: feature_addition
Ordering: APIContractFlow33 → ImplementationFlow41 (contract informs implementation).
