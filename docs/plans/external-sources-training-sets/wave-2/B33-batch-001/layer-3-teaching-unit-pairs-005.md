# Layer 3 — Teaching Unit Pairs (005)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-005 — "integrate" always fires API_CONTRACT

**Teaching goal**: "integrate" is the strongest API_CONTRACT trigger — fires in all contexts.

**Rule**: Whenever "integrate" appears as an action verb, API_CONTRACT fires. The object does not matter.

**Examples**:

| Request | Predicate |
|---------|-----------|
| "Integrate the agent with Salesforce" | API_CONTRACT |
| "Integrate the AI model with our database" | API_CONTRACT |
| "Integrate the LLM API with the frontend" | API_CONTRACT |
| "Connect the agent to Slack" | API_CONTRACT ("connect" also fires API_CONTRACT) |
| "Build the integration layer" | IMPLEMENT ("build" fires IMPLEMENT; "integration" as noun does not fire API_CONTRACT) |
| "Adapt the agent to work with Stripe" | [] gap ("adapt" fires gap; "work with" does not fire API_CONTRACT) |

**API_CONTRACT trigger verbs** (complete list for B33 context):
- integrate, connect, wire up, link (to external system), consume (API), call (external API), interface with

**Key distinction**: "integrate" as verb → API_CONTRACT. "integration" as noun → does not fire API_CONTRACT on its own.
