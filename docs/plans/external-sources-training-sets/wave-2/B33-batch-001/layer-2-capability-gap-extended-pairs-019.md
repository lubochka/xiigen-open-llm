# Layer 2 — Capability Gap Extended Pairs (019)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B33-gap-019 — IMPLEMENT+API_CONTRACT fires: "build API for the agent"

Request: "Build the agent API endpoint so downstream services can call the agent, and integrate it with the service mesh."

### Flow: Multi-Predicate

**Node 1**: Parse request. "build the agent API endpoint" = code verb + code artifact. "integrate with service mesh" = "integrate" = integration verb.
**Node 2**: Predicate scan: "build" → IMPLEMENT; "integrate" → API_CONTRACT. Both fire. Gap not logged (predicates fired).
**Node 3**: Predicate set = [IMPLEMENT, API_CONTRACT]
**Node 4**: GapWarning: agent AI context noted but no gap vocabulary fired as primary action.
**Node 5**: DomainId: software-development
**Node 6**: Flows: APIContractFlow33 → ImplementationFlow41
**Node 7**: Ordering: API_CONTRACT before IMPLEMENT (contract spec informs implementation).
**Node 8**: TransitionType: feature_addition
**Node 9**: Verdict: IMPLEMENT_APPROVED + API_CONTRACT_APPROVED

---

## B33-gap-019b — Gap analysis pairs: agent API vocabulary

A: "Build the agent API endpoint."
Predicates: [IMPLEMENT] | GapId: N/A | DomainId: software-development

A: "Integrate the agent with Salesforce."
Predicates: [API_CONTRACT] | GapId: N/A | DomainId: software-development | Note: "integrate" always fires API_CONTRACT.

A: "We should expose the agent as an API."
Predicates: [] | GapId: gap-ai-agent-skill-001 | DomainId: software-development | Note: "should expose" = strategic discussion → gap.

A: "Build the agent API and prevent prompt injection at the endpoint."
Predicates: [IMPLEMENT, SECURITY] | GapId: N/A | DomainId: software-development

A: "Integrate the agent endpoint and document the API contract."
Predicates: [API_CONTRACT, DOCUMENT] | GapId: N/A | DomainId: software-development
