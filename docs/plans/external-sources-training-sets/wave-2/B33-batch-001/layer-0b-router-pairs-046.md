# Layer 0-B — Router Pairs (046)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-046-001 — Gap: BERT standalone

A: [] | "BERT (bidirectional encoder representations from transformers) is used for sentence classification."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "BERT" = AI model architecture. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-046-002 — IMPLEMENT: build BERT inference client

A: ["IMPLEMENT"] | "Build a BERT inference client that sends requests to the model server."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "BERT inference client" = implementation artifact.

---

## C0B-046-003 — Gap: Llama standalone

A: [] | "Llama 3 requires significant compute for inference at full precision."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "Llama" = AI model family. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-046-004 — IMPLEMENT: implement Llama API client

A: ["IMPLEMENT"] | "Implement a Llama API client with streaming response support."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. "Llama API client" = implementation artifact.

---

## C0B-046-005 — ARCHITECTURE+IMPLEMENT: design + build Llama serving layer

A: ["ARCHITECTURE", "IMPLEMENT"] | "Design the Llama serving architecture and implement the request routing layer."
B: ARCHITECTURE → ArchitectureFlow31 (first); IMPLEMENT → ImplementationFlow41 (after ARCHITECTURE).
Teaching: Two code verbs, two flows. ARCHITECTURE before IMPLEMENT per canonical ordering.
