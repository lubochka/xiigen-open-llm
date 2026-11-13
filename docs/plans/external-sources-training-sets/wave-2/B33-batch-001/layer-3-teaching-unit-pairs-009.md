# Layer 3 — Teaching Unit Pairs (009)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-009 — Mixed requests: one firing predicate suppresses GapHandler

**Teaching goal**: If any predicate fires, GapHandler is NOT invoked — even if gap verbs are also present.

**Rule**: GapHandler is invoked ONLY when predicate set = []. One firing predicate = no GapHandler.

**Examples**:

| Request | Gap verbs | Code verbs | Predicates | GapHandler? |
|---------|-----------|------------|------------|-------------|
| "Adapt the agent for finance and build the classifier" | adapt | build | [IMPLEMENT] | NO |
| "Fine-tune and implement the response formatter" | fine-tune | implement | [IMPLEMENT] | NO |
| "Orchestrate workflows and integrate with CRM" | orchestrate | integrate | [API_CONTRACT] | NO |
| "Adapt and orchestrate the agent" | adapt, orchestrate | none | [] | YES |
| "Design the agent platform and architect the infra" | none | design/architect | [ARCHITECTURE] | NO |

**Key insight**: Gap verbs are silently ignored when at least one predicate fires. The system does not log gaps for the ignored gap verbs — only one path executes per request (predicate flows OR GapHandler, never both).

**Exception to "never both"**: There is no exception. GapHandler and predicate flows are mutually exclusive per request.
