# Layer 0-B — Router Pairs (005)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: router-pairs

---

## B37-0B-005-001 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1: API dependencies with co-occurrence

**Input:** "Document the service API dependencies and coordinate the release schedule with the platform team."

**Compiler output:** ["API_CONTRACT", "DOCUMENT"]

**Router decision:**
- API_CONTRACT present → route to 3.3 (APIContractFlow)
- DOCUMENT present → route to 2.1 (DocumentationFlow)
- DomainId: "software-development"
- Routes: 3.3 + 2.1 (parallel eligible)

**Gap warnings forwarded:**
- gap-project-managing-coordinate-001
- gap-project-managing-manage-delivery-001

**Router note:** "coordinate" and "release schedule" are gap vocabulary — not routed. Only the API_CONTRACT and DOCUMENT predicates produce routes.

---

## B37-0B-005-002 — [API_CONTRACT] → 3.3 only: API endpoint dependencies

**Input:** "Map the API endpoint dependencies for the payment integration."

**Compiler output:** ["API_CONTRACT"]

**Router decision:**
- API_CONTRACT (dependencies + endpoint + integration co-occurrence) → 3.3
- No other predicates → single route
- DomainId: "software-development"
- Route: 3.3

**Gap warnings forwarded:** none

---

## B37-0B-005-003 — [API_CONTRACT, DOCUMENT, IMPLEMENT] → 3.3 + 2.1 + 4.1: full service dependency

**Input:** "Define the interface dependencies, build the service module, and document the API contract."

**Compiler output:** ["API_CONTRACT", "DOCUMENT", "IMPLEMENT"]

**Router decision:**
- API_CONTRACT (dependencies + interface co-occurrence) → 3.3
- DOCUMENT (define, document) → 2.1
- IMPLEMENT (build, service, module) → 4.1
- DomainId: "software-development"
- Routes: 3.3 + 2.1 + 4.1

**Gap warnings forwarded:** none (all vocabulary captured)

---

## B37-0B-005-004 — [] → knowledge_edge: "project dependencies" without API co-occurrence

**Input:** "Map out project dependencies and identify blockers holding up delivery."

**Compiler output:** []

**Router decision:**
- No API co-occurrence → no API_CONTRACT
- No other predicates
- DomainId: "project-management"
- Route: knowledge_edge
- SCHEDULE_CAPABILITY_LEARNING emitted

**Gap warnings forwarded:**
- gap-project-managing-dependencies-team-001
- gap-project-managing-blockers-001
- gap-project-managing-manage-delivery-001

---

## B37-0B-005-005 — [API_CONTRACT, DOCUMENT] → 3.3 + 2.1: coordinate + API dep sentence

**Input:** "Coordinate the API dependencies across services and document the integration contracts."

**Compiler output:** ["API_CONTRACT", "DOCUMENT"]

**Router decision:**
- API_CONTRACT (dependencies + API + integration + contract co-occurrence) → 3.3
- DOCUMENT (document) → 2.1
- DomainId: "software-development"
- Routes: 3.3 + 2.1

**Gap warnings forwarded:**
- gap-project-managing-coordinate-001
