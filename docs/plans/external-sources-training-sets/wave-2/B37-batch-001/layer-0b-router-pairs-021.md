# Layer 0-B — Router Pairs (021)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-021-001 — API dependencies: [API_CONTRACT, IMPLEMENT] → 3.3+4.1

**Input:** "Map all API dependencies between the payment service and the user authentication endpoint."

**Compiler output:** ["API_CONTRACT", "IMPLEMENT"]

**Router decision:**
- API_CONTRACT predicate → APIContractFlow33 (3.3)
- IMPLEMENT predicate (from "service") → BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Routes: 3.3 + 4.1

**Gap warnings forwarded:** none (all vocabulary covered by predicates)

---

## B37-0B-021-002 — team dependencies: [] → knowledge_edge

**Input:** "Map all team dependencies for the delivery programme and track blockers."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-dependencies-team-001, gap-project-managing-manage-delivery-001, gap-project-managing-track-001, gap-project-managing-blockers-001

**Key boundary:** "team dependencies" → zero predicates. "API dependencies" → API_CONTRACT fires.

---

## B37-0B-021-003 — integration dependencies: [API_CONTRACT, IMPLEMENT] → 3.3+4.1

**Input:** "Identify all integration dependencies between the notification service and the messaging module."

**Compiler output:** ["API_CONTRACT", "IMPLEMENT"]

**Router decision:**
- API_CONTRACT (from integration co-occurrence + dependencies) → APIContractFlow33 (3.3)
- IMPLEMENT (from "service" + "module") → BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Routes: 3.3 + 4.1

**Gap warnings forwarded:** none

---

## B37-0B-021-004 — cross-workstream dependencies: [] → knowledge_edge

**Input:** "Resolve cross-workstream dependencies before the upcoming delivery milestone."

**Compiler output:** []

**Router decision:**
- Predicate list empty → knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge payload:**
- Gap IDs: gap-project-managing-dependencies-team-001, gap-project-managing-work-stream-001, gap-project-managing-manage-delivery-001

---

## B37-0B-021-005 — swagger dependencies with "review": [QA, API_CONTRACT] → QA+3.3

**Input:** "Review all swagger dependencies and update the API contract documentation."

**Compiler output:** ["QA", "API_CONTRACT"]

**Router decision:**
- QA predicate (from "review") → ReviewFlow (QA sub-phase)
- API_CONTRACT (from "swagger" + "API" + "dependencies") → APIContractFlow33 (3.3)
- DomainId: "software-development"
- Routes: QA sub-phase + 3.3

**Gap warnings forwarded:** none
