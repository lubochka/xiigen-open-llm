# Layer 1 — Transition Orchestrator Pairs (007)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-007-001 — API_CONTRACT+DOCUMENT: documentation TransitionType

**Input:** "Document the service API dependencies and coordinate the release schedule."

**Compiler output:** ["API_CONTRACT", "DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation (DOCUMENT) + api_contract (API_CONTRACT)
- Note: API_CONTRACT → "api_contract" TransitionType
- Next states: APIContractFlow33 (3.3) + DocumentationFlow (2.1)
- DomainId: "software-development"
- Parallel eligible: Yes

**Gap IDs forwarded:**
- gap-project-managing-coordinate-001
- gap-project-managing-manage-delivery-001

---

## B37-L1-007-002 — API_CONTRACT + DOCUMENT: integration context

**Input:** "Define the integration interface dependencies and document the REST API requirements."

**Compiler output:** ["API_CONTRACT", "DOCUMENT"]

**Transition orchestrator:**
- TransitionType: api_contract + documentation
- Next states: APIContractFlow33 (3.3) + DocumentationFlow (2.1)
- DomainId: "software-development"

**Gap IDs:** none (all vocabulary captured by predicates)

---

## B37-L1-007-003 — API_CONTRACT alone: api_contract TransitionType

**Input:** "Map all API endpoint dependencies for the integration layer."

**Compiler output:** ["API_CONTRACT"]

**Transition orchestrator:**
- TransitionType: api_contract
- Next state: APIContractFlow33 (3.3) only
- DomainId: "software-development"

**Gap IDs:** none

---

## B37-L1-007-004 — API_CONTRACT + IMPLEMENT + DOCUMENT: three types

**Input:** "Define the interface dependencies, build the service, and document the API contract."

**Compiler output:** ["API_CONTRACT", "DOCUMENT", "IMPLEMENT"]

**Transition orchestrator:**
- TransitionType: api_contract + feature_addition + documentation
- Next states: APIContractFlow33 + BackendServiceFlow41 + DocumentationFlow
- DomainId: "software-development"
- Parallel eligible: APIContractFlow33 + DocumentationFlow parallel; BackendServiceFlow41 may follow APIContractFlow33

---

## B37-L1-007-005 — api_contract TransitionType rules

**TransitionType = "api_contract" when:**
- API_CONTRACT predicate fired
- Trigger: "dependencies" + API co-occurrence token
- Next state: APIContractFlow33 (3.3)

**Compound with documentation:** Most common in B37 sentences involving "document the API dependencies"

**Not a managing transition:** Even when the request is managing ("coordinate API dependencies"), TransitionType is "api_contract" because the predicate governs, not the intent.
