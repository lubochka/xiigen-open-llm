# Layer 0-C — Knowledge Edge Pairs (019)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-019-001 — Edge: "integrate the agent" always fires API_CONTRACT

"integrate the agent with Salesforce" → ["API_CONTRACT"] (integrate always fires)
"integrate the agent's fine-tuning pipeline with the model registry" → ["API_CONTRACT"] (integrate fires despite AI context)
"integrate the self-healing agent API" → ["API_CONTRACT"] (integrate fires; "self-healing agent" is object context)

---

## KE-019-002 — Edge: "build the agent API" vs "expose the agent as API"

"build the agent API endpoint" → IMPLEMENT ("build" = code verb; "endpoint" = software artifact)
"expose the agent as an API" → gap ("expose" does not fire a code predicate; strategic/planning language → gap)
"develop the agent API service" → IMPLEMENT ("develop" = code verb equivalent to "build")

---

## KE-019-003 — Edge: IMPLEMENT+API_CONTRACT vs API_CONTRACT alone

"build the agent service and integrate with the CRM API" → ["IMPLEMENT", "API_CONTRACT"] (both verbs fire)
"integrate the existing agent with the CRM API" → ["API_CONTRACT"] (only integrate; "existing" = no build action)
Teaching: "build" + "integrate" in same request → both predicates fire. "integrate" alone (no build) → API_CONTRACT only.

---

## KE-019-004 — Edge: "orchestrate via API" fires API_CONTRACT not gap

"orchestrate agent calls via the API" → ["API_CONTRACT"] ("via API" pattern triggers API_CONTRACT; "orchestrate" AI context does not suppress)
"orchestrate agent calls via the orchestration service" → gap ("orchestrate" + no code verb + no "via API" pattern → gap)
Teaching: "via API" is sufficient to trigger API_CONTRACT even with AI orchestration verbs present.

---

## KE-019-005 — Edge: prompt injection in agent API context fires SECURITY

"build the agent API and prevent prompt injection at the ingress layer" → ["IMPLEMENT", "SECURITY"]
"integrate the agent with the chat API and secure against prompt injection" → ["API_CONTRACT", "SECURITY"]
Teaching: "prompt injection" fires SECURITY in all contexts including agent API. Both API_CONTRACT and SECURITY can co-fire.
