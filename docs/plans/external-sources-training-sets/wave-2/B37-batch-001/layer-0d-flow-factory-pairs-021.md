# Layer 0-D — Flow Factory Pairs (021)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-021-001 — API dependencies: APIContractFlow33 + BackendServiceFlow41

**Input:** "Map all API dependencies between the payment service and the user authentication endpoint."

**Compiler output:** ["API_CONTRACT", "IMPLEMENT"]

**Flow factory decision:**
- API_CONTRACT → APIContractFlow33 (3.3)
- IMPLEMENT → BackendServiceFlow41 (4.1)
- Both factories selected
- DomainId: "software-development"
- Gap warnings: none

---

## B37-0D-021-002 — team dependencies: knowledge_edge

**Input:** "Map all team dependencies for the delivery programme and track blockers."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: gap-project-managing-dependencies-team-001, gap-project-managing-manage-delivery-001, gap-project-managing-track-001, gap-project-managing-blockers-001

---

## B37-0D-021-003 — integration dependencies: APIContractFlow33 + BackendServiceFlow41

**Input:** "Identify all integration dependencies between the notification service and the messaging module."

**Compiler output:** ["API_CONTRACT", "IMPLEMENT"]

**Flow factory decision:**
- API_CONTRACT → APIContractFlow33 (3.3)
- IMPLEMENT → BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Gap warnings: none

---

## B37-0D-021-004 — cross-workstream dependencies: knowledge_edge

**Input:** "Resolve cross-workstream dependencies before the delivery milestone."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → knowledge_edge activated
- SCHEDULE_CAPABILITY_LEARNING emitted
- Gap IDs: gap-project-managing-dependencies-team-001, gap-project-managing-work-stream-001, gap-project-managing-manage-delivery-001

---

## B37-0D-021-005 — "service dependencies" (service fires IMPLEMENT; no API_CONTRACT)

**Input:** "Resolve service dependencies between the auth and payment modules."

**Compiler output:** ["IMPLEMENT"]

**Flow factory decision:**
- IMPLEMENT → BackendServiceFlow41 (4.1)
- No API_CONTRACT (service is NOT an API co-occurrence token)
- DomainId: "software-development"
- Gap warnings: gap-project-managing-dependencies-team-001

**Factory note:** "service" fires IMPLEMENT but NOT API_CONTRACT. The dependencies rule requires API co-occurrence; "service" is not in that list. APIContractFlow33 is NOT selected.
