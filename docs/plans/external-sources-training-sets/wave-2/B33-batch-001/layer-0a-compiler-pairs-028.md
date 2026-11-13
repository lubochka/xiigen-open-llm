# Layer 0-A — Compiler Pairs (028)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-028-001 — gap: "transformer" architecture vocabulary

A: "The transformer architecture needs to be adjusted for our use case."
B: [] — "transformer" = AI model architecture (not a software design artifact). No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-028-002 — gap: "inference endpoint" vocabulary without integrate

A: "The inference endpoint serves the model predictions."
B: [] — "inference endpoint" describes an AI serving component. No code verb fires. Gap.
CompiledPredicateIds: []

---

## C0A-028-003 — API_CONTRACT fires: integrate inference endpoint

A: "Integrate the inference endpoint with the application backend."
B: ["API_CONTRACT"] — "integrate" fires API_CONTRACT. "inference endpoint" is object context.
CompiledPredicateIds: ["API_CONTRACT"]

---

## C0A-028-004 — IMPLEMENT fires: build inference serving layer

A: "Build the inference serving layer that wraps the model endpoint."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "inference serving layer" and "model endpoint" are object context.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-028-005 — gap: "model serving" vocabulary without code verb

A: "Model serving latency is too high in production."
B: [] — "model serving" = AI operational concern. No code verb fires. Gap.
CompiledPredicateIds: []
