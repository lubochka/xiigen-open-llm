# Layer 0-D — Flow Factory Pairs (007)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: flow-factory

---

## B17-0D-007-001 — API_CONTRACT alone factory: 3.3 only (APIContractFlow33)

**Scenario:** "Integrate the Slack messaging API for notifications." — integrate only, no build verb.

**Factory input:**
```
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
DecisionCount: 1
TransitionType: documentation
```

**No 4.3 flow is selected.** API_CONTRACT alone → only 3.3 → only APIContractFlow33. IntegrationConnectorFlow43 (4.3) requires IMPLEMENT — it is never selected without IMPLEMENT.

---

## B17-0D-007-002 — APIContractFlow33 characteristics

**APIContractFlow33:**
- Handles: API contract definition, interface specification, endpoint documentation
- Typical output: OpenAPI (Open Application Programming Interface) specification, API schema, endpoint list
- Sub-phase 3.3 name: API Contract Definition
- Status: deterministic_generic (no candidate flow — API contract definition is a standard generic process)

**When IMPLEMENT is also present:** APIContractFlow33 runs alongside IntegrationConnectorFlow43. They are independent flows.

---

## B17-0D-007-003 — Factory routing: "integrate only" vs "build + integrate"

**"Integrate the Slack API" (API_CONTRACT only):**
```
SelectedFlows: [APIContractFlow33 (3.3)]
DecisionCount: 1
TransitionType: documentation
```

**"Build and integrate the Slack connector" (IMPLEMENT + API_CONTRACT):**
```
SelectedFlows: [IntegrationConnectorFlow43 (4.3), APIContractFlow33 (3.3)]
DecisionCount: 2
TransitionType: feature_addition
```

**Adding "build" activates IntegrationConnectorFlow43 (4.3), doubles DecisionCount, and changes TransitionType.**

---

## B17-0D-007-004 — APIContractFlow33 output types

**Typical outputs from APIContractFlow33:**
- OpenAPI 3.0 specification file
- API endpoint list with HTTP methods and schemas
- Request/response payload examples
- Error response catalog
- Rate limiting documentation

**These outputs are documentation artifacts, not code.** That is why API_CONTRACT alone produces TransitionType: documentation — the work product is a specification, not an implementation.
