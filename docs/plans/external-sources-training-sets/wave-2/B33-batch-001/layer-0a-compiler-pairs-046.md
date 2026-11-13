# Layer 0-A — Compiler Pairs (046)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-046-001 — gap: BERT standalone

A: "BERT (bidirectional encoder representations from transformers) is used for sentence classification."
B: [] — "BERT" = specific AI model architecture. No code verb. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-046-002 — IMPLEMENT: build BERT inference client

A: "Build a BERT inference client that sends requests to the model server."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "BERT inference client" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-046-003 — gap: Llama (AI model family) standalone

A: "Llama 3 requires significant compute for inference at full precision."
B: [] — "Llama" = AI model family name. No code verb. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-046-004 — IMPLEMENT: implement Llama API client

A: "Implement a Llama API client with streaming response support."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. "Llama API client" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-046-005 — ARCHITECTURE+IMPLEMENT: design + build Llama serving layer

A: "Design the Llama serving architecture and implement the request routing layer."
B: ["ARCHITECTURE", "IMPLEMENT"] — "design" + "architecture" fires ARCHITECTURE; "implement" fires IMPLEMENT. Two predicates, standard order.
CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
