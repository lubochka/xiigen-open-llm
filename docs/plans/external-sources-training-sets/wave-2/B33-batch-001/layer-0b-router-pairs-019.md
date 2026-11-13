# Layer 0-B — Router Pairs (019)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-019-001 — IMPLEMENT+API_CONTRACT: "build + integrate agent API"

A: ["IMPLEMENT", "API_CONTRACT"] | "Build the agent service and integrate it with the external CRM API."
B: IMPLEMENT → ImplementationFlow41; API_CONTRACT → APIContractFlow33. TransitionType: feature_addition.
Reason: "build" → IMPLEMENT; "integrate" → API_CONTRACT. Both fire despite agent AI context.

---

## C0B-019-002 — API_CONTRACT: "integrate the agent with Salesforce"

A: ["API_CONTRACT"] | "Integrate the agent with Salesforce to retrieve customer history."
B: API_CONTRACT → APIContractFlow33.
Reason: "integrate" always fires API_CONTRACT regardless of AI context.

---

## C0B-019-003 — IMPLEMENT+API_CONTRACT+SECURITY: full agent API integration with security

A: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] | "Implement the agent integration layer, integrate with the payment gateway, and secure the API against prompt injection."
B: IMPLEMENT → 4.1; API_CONTRACT → APIContractFlow33; SECURITY → SecurityAuditFlow. TransitionType: feature_addition. QA: sub-phase pending.
Ordering: API_CONTRACT before IMPLEMENT; SECURITY after IMPLEMENT.

---

## C0B-019-004 — Gap: "agent API" as strategic discussion

A: [] | "We should expose the agent as an API so other services can call it."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "should consider" = strategic discussion. No code verb fires. Gap ID: gap-ai-agent-skill-001.

---

## C0B-019-005 — API_CONTRACT+DOCUMENT: integrate + document agent API

A: ["API_CONTRACT", "DOCUMENT"] | "Integrate the agent endpoint and document the API contract for downstream consumers."
B: API_CONTRACT → APIContractFlow33; DOCUMENT → DocumentationFlow62. TransitionType: feature_addition.
