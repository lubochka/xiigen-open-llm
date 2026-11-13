# Layer 0-B — Router Pairs (028)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-028-001 — IMPLEMENT: refactor for latency optimization

A: ["IMPLEMENT"] | "Refactor the serving pipeline to reduce inference latency."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "refactor" fires IMPLEMENT. "inference latency" and "serving pipeline" = object context.

---

## C0B-028-002 — SECURITY: secure model serving endpoint

A: ["SECURITY"] | "Secure the model serving endpoint against unauthorized access."
B: SECURITY → SecurityAuditFlow.
Teaching: "secure" fires SECURITY. "model serving endpoint" = object context.

---

## C0B-028-003 — Gap → GapHandler: "hallucination mitigation" vocabulary

A: [] | "Hallucination mitigation should be applied to reduce incorrect outputs."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "hallucination mitigation" = AI quality concern. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-028-004 — IMPLEMENT: build hallucination guard

A: ["IMPLEMENT"] | "Build a hallucination guard that validates model outputs against a fact store."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "hallucination guard" = implementation artifact.

---

## C0B-028-005 — Gap: all Round 2 AI vocabulary, no code verb

A: [] | "Tokenize inputs, embed them, run the RAG pipeline, manage context window, apply attention, use transformer, serve via inference endpoint, optimize serving latency, mitigate hallucination."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: All AI model-level vocabulary. No code verb. Single event. Gap ID: gap-ai-ml-model-001.
