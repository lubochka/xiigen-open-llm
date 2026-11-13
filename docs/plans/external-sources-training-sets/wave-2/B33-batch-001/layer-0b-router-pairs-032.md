# Layer 0-B — Router Pairs (032)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-032-001 — IMPLEMENT: build dataset curation tool

A: ["IMPLEMENT"] | "Implement a dataset curation pipeline that filters and deduplicates examples."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. "dataset curation pipeline" = artifact.

---

## C0B-032-002 — IMPLEMENT: mixed AI gap + code verb

A: ["IMPLEMENT"] | "Build the training data pipeline and tune the hyperparameters to reduce overfitting."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "tune hyperparameters" and "overfitting" = AI gap vocabulary suppressed by predicate.

---

## C0B-032-003 — ARCHITECTURE: design tokenization architecture

A: ["ARCHITECTURE"] | "Design the tokenization and embedding architecture for the document store."
B: ARCHITECTURE → ArchitectureFlow31.
Teaching: "design" + "architecture" (structural noun) fires ARCHITECTURE. AI vocabulary in object position.

---

## C0B-032-004 — Gap: complete Round 2 AI gap vocabulary

A: [] | "Tokenize, embed, vector search, RAG pipeline, context window, attention mechanism, transformer, inference endpoint, model serving, latency optimization, hallucination mitigation."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: All Round 2 AI/ML vocabulary items. No code verb. Single event. Gap ID: gap-ai-ml-model-001.

---

## C0B-032-005 — IMPLEMENT + TEST: build and test embedding pipeline

A: ["IMPLEMENT", "TEST"] | "Implement the embedding pipeline and write unit tests for the vector output."
B: IMPLEMENT → ImplementationFlow41; TEST → E2ETestingFlow53.
Teaching: Two code verbs, two flows. Ordering: IMPLEMENT → TEST (tests run on built artifact).
