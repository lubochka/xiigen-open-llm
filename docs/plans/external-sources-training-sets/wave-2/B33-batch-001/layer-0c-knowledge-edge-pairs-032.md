# Layer 0-C — Knowledge Edge Pairs (032)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-032-001 — "tokenize" vs "build a tokenizer" distinction

Two patterns with "tokenize/tokenizer":
1. "tokenize the input" — verb form describing AI model operation → gap (gap-ai-ml-model-001)
2. "build a tokenizer" — code verb + noun artifact → IMPLEMENT
3. "implement the tokenization service" — code verb + service noun → IMPLEMENT
4. "integrate the tokenizer API" — code verb "integrate" → API_CONTRACT
The presence of a code verb transforms the AI vocabulary into an object artifact.

---

## KE-032-002 — "embed" vs "build an embedder" distinction

Two patterns with "embed/embedding":
1. "embed the documents" — verb form describing AI operation → gap (gap-ai-ml-model-001)
2. "build an embedding service" — code verb + service noun → IMPLEMENT
3. "integrate the embedding endpoint" — "integrate" → API_CONTRACT
4. "design the embedding architecture" — "design" + "architecture" → ARCHITECTURE
The action verb determines the predicate; "embedding" as object does not fire any predicate.

---

## KE-032-003 — "vector search" API vs implementation boundary

"vector search" has two coding patterns depending on context:
1. Using an existing vector search service (e.g., Pinecone, Weaviate) → API_CONTRACT when "integrate" fires
2. Building a custom vector search component → IMPLEMENT when "build/implement" fires
3. "use vector search" alone → gap (no code verb)
The verb determines routing; "vector search" is always object context.

---

## KE-032-004 — "dataset curation" vs "implement data pipeline" distinction

"dataset curation" = AI data preparation concept → gap when no code verb.
"implement a data curation pipeline" → IMPLEMENT (code verb + artifact).
"write curation validation tests" → TEST (code verb + test form).
Never fires API_CONTRACT alone (not an API boundary concept).

---

## KE-032-005 — v6 corruptions for Round 2 vocabulary

New v6 corruption patterns for Round 2:
- v6-01: "tokenize" → TOKENIZE (invented predicate) — Correct: [] → GapHandler
- v6-01: "embed" → EMBED (invented predicate) — Correct: [] → GapHandler
- v6-01: "train neural network" → TRAIN (invented predicate) — Correct: [] → GapHandler
- v6-02: "gradient descent" → IMPLEMENT — Correct: [] → GapHandler
- v6-02: "benchmark the model" → TEST — Correct: [] → GapHandler (no "write/create tests" form)
- v6-03: collapse "implement + integrate" → ["IMPLEMENT"] only — Correct: ["IMPLEMENT", "API_CONTRACT"]
