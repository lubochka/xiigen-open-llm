# Layer 0-D — Flow Factory Pairs (019)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-019-001 — IMPLEMENT+API_CONTRACT: build + integrate agent API

A: ["IMPLEMENT", "API_CONTRACT"] | "Build the agent service and integrate with the CRM API."
Factory: [ImplementationFlow41, APIContractFlow33] | TransitionType: feature_addition
Ordering: APIContractFlow33 before ImplementationFlow41 (contract spec informs implementation).

---

## FF-019-002 — API_CONTRACT: "integrate the agent with Salesforce"

A: ["API_CONTRACT"] | "Integrate the agent with Salesforce to retrieve customer history."
Factory: [APIContractFlow33]
Teaching: "integrate" always fires API_CONTRACT. AI context does not suppress. No IMPLEMENT fires (no build/create verb).

---

## FF-019-003 — IMPLEMENT+API_CONTRACT+SECURITY: full agent API integration with security

A: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] | "Implement the integration layer, integrate with the payment gateway, and secure against prompt injection."
Factory: [APIContractFlow33, ImplementationFlow41, SecurityAuditFlow] | TransitionType: feature_addition
Ordering: API_CONTRACT → IMPLEMENT → SECURITY.

---

## FF-019-004 — GapHandler: "agent API" as strategic discussion

A: [] | "We should expose the agent as an API so other services can call it."
Factory: GapHandler | Gap ID: gap-ai-agent-skill-001
Reason: "should consider" = strategic discussion. No code verb fires.

---

## FF-019-005 — Factory selection summary: agent API vocabulary

| Request form | Factory selected | Reason |
|-------------|-----------------|--------|
| build the agent API | ImplementationFlow41 | Code verb |
| integrate the agent with Salesforce | APIContractFlow33 | "integrate" always |
| expose the agent as an API | GapHandler | Strategic discussion, no code verb |
| secure the agent API against injection | SecurityAuditFlow | Injection = security |
| document the agent API contract | DocumentationFlow62 | Document verb |
