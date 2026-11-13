# Layer 0-C — Knowledge Edge Pairs (038)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-038-001 — Gradient descent gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| gradient descent (standalone) | No code verb | gap-ai-ml-training-001 |
| gradient descent optimizer (with "implement") | Code verb present | IMPLEMENT fires |
| gradient descent platform (with "integrate") | "integrate" present | API_CONTRACT fires |

---

## KE-038-002 — Backpropagation gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| backpropagation (standalone) | No code verb | gap-ai-ml-training-001 |
| backpropagation routine (with "write") | Code verb present | IMPLEMENT fires |
| backpropagation API (with "integrate") | "integrate" present | API_CONTRACT fires |

---

## KE-038-003 — Gradient descent vs. backpropagation: both gap-ai-ml-training-001

Rule: Both "gradient descent" and "backpropagation" belong to gap-ai-ml-training-001.
When both appear without code verb → one event, one gap ID.
Example: "backpropagation uses gradient descent" → [] → gap-ai-ml-training-001 (one event).

---

## KE-038-004 — "Write" fires IMPLEMENT when applied to code artifacts

Rule: "write" fires IMPLEMENT when the object is a code artifact (routine, module, service, function, class).
"write" does NOT fire IMPLEMENT for: "write a report", "write a document" (→ DOCUMENT), "write tests" (→ TEST).
Example: "Write the backpropagation routine" → IMPLEMENT (code artifact).
Example: "Write docs for the gradient descent algorithm" → DOCUMENT.

---

## KE-038-005 — Seven-predicate pattern applies to training vocabulary

Rule: All seven predicates (ARCHITECTURE, API_CONTRACT, IMPLEMENT, SECURITY, TEST, DOCUMENT, QA) can fire on requests containing training AI vocabulary.
Training vocabulary (gradient descent, backpropagation, etc.) is always in object position when code verbs are present.
Example: "architect gradient descent platform, integrate backpropagation API, implement optimizer, secure endpoint, test gradients, document algorithm, QA" → all seven predicates.
