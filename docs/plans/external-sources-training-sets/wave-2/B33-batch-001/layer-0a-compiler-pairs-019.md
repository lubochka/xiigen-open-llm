# Layer 0-A — Compiler Pairs (019)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-019-001 — IMPLEMENT+API_CONTRACT fires: build + integrate agent API

A: "Build the agent service and integrate it with the external CRM API."
B: ["IMPLEMENT", "API_CONTRACT"] — "build" → IMPLEMENT; "integrate" → API_CONTRACT. Both fire despite agent AI context.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]

---

## C0A-019-002 — API_CONTRACT fires: integrate the agent with Salesforce

A: "Integrate the agent with Salesforce to retrieve customer history."
B: ["API_CONTRACT"] — "integrate" always fires API_CONTRACT regardless of AI context or object.
CompiledPredicateIds: ["API_CONTRACT"]

---

## C0A-019-003 — IMPLEMENT+API_CONTRACT+SECURITY fires: full agent API integration with security

A: "Implement the agent integration layer, integrate with the payment gateway, and secure the API endpoints against prompt injection."
B: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — three code verbs. "prompt injection" → SECURITY; "integrate" → API_CONTRACT; "implement" → IMPLEMENT.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

---

## C0A-019-004 — gap: "agent API" as discussion

A: "We should consider exposing the agent as an API so other services can call it."
B: [] — "should consider" = strategic discussion. No code verb fires. Observation → gap.
CompiledPredicateIds: []

---

## C0A-019-005 — API_CONTRACT+DOCUMENT fires: integrate + document agent API

A: "Integrate the agent endpoint into the platform and document the API contract for downstream consumers."
B: ["API_CONTRACT", "DOCUMENT"] — "integrate" → API_CONTRACT; "document" → DOCUMENT. Agent context does not suppress either predicate.
CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
