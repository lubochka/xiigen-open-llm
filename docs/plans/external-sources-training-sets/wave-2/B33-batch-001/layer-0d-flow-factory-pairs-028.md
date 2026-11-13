# Layer 0-D — Flow Factory Pairs (028)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-028-001 — IMPLEMENT: refactor for latency optimization

A: ["IMPLEMENT"] | "Refactor the serving pipeline to reduce inference latency."
Factory: [ImplementationFlow41] | TransitionType: refactoring
Teaching: "refactor" fires IMPLEMENT. TransitionType = refactoring (refactor without new feature).

---

## FF-028-002 — SECURITY: secure model serving endpoint

A: ["SECURITY"] | "Secure the model serving endpoint against unauthorized access."
Factory: [SecurityAuditFlow]
Teaching: "secure" fires SECURITY. "model serving endpoint" = object context.

---

## FF-028-003 — IMPLEMENT: build hallucination guard

A: ["IMPLEMENT"] | "Build a hallucination guard that validates model outputs against a fact store."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "hallucination guard" = implementation artifact.

---

## FF-028-004 — GapHandler: all Round 2 AI model vocabulary

A: [] | "Tokenize inputs, embed them, run the RAG pipeline, manage context window, apply attention, use transformer, serve via inference endpoint, optimize serving latency, mitigate hallucination."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: All AI model-level vocabulary. No code verb. One SCHEDULE_CAPABILITY_LEARNING emitted.

---

## FF-028-005 — ARCHITECTURE: design RAG platform architecture

A: ["ARCHITECTURE"] | "Design the tokenization and embedding architecture for the document store."
Factory: [ArchitectureFlow31] | TransitionType: feature_addition
Teaching: "design" + "architecture" (structural noun) fires ARCHITECTURE. AI vocabulary = object context.
