# Layer 0-B — Router Pairs (007)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: router-pairs

---

## B37-0B-007-001 — [IMPLEMENT] → 4.1: build in managing context

**Input:** "Manage the delivery of the authentication service and build the access control module."

**Compiler output:** ["IMPLEMENT"]

**Router decision:**
- IMPLEMENT present → route to 4.1 (BackendServiceFlow or equivalent IMPLEMENT flow)
- DomainId: "software-development"
- Route: 4.1

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001

**Router note:** "manage" and "delivery" are gap vocabulary. "service", "build", and "module" all fire IMPLEMENT → single route 4.1.

---

## B37-0B-007-002 — [IMPLEMENT] → 4.1: "create" + "service" in managing sentence

**Input:** "Create the status tracking service and deploy it to the project management environment."

**Compiler output:** ["IMPLEMENT"]

**Router decision:**
- IMPLEMENT (create, service) → 4.1
- DomainId: "software-development"
- Route: 4.1

**Gap warnings forwarded:**
- gap-project-managing-track-status-001 (status tracking intent)

---

## B37-0B-007-003 — [IMPLEMENT] → 4.1: "build" fires even in coordination sentence

**Input:** "Build the coordination dashboard module so teams can track blocker resolution."

**Compiler output:** ["IMPLEMENT"]

**Router decision:**
- IMPLEMENT (build, module) → 4.1
- DomainId: "software-development"
- Route: 4.1

**Gap warnings forwarded:**
- gap-project-managing-coordinate-001 (coordination intent)
- gap-project-managing-blockers-001 (blocker resolution intent)
- gap-project-managing-track-status-001 (track intent)

---

## B37-0B-007-004 — [IMPLEMENT, DOCUMENT] → 4.1 + 2.1: build + document in managing sentence

**Input:** "Build the escalation tracking module and document the escalation criteria."

**Compiler output:** ["IMPLEMENT", "DOCUMENT"]

**Router decision:**
- IMPLEMENT (build, module) → 4.1
- DOCUMENT (document, criteria) → 2.1
- DomainId: "software-development"
- Routes: 4.1 + 2.1

**Gap warnings forwarded:**
- gap-project-managing-escalation-001
- gap-project-managing-track-status-001

---

## B37-0B-007-005 — [IMPLEMENT, ARCHITECTURE] → 4.1 + 3.1: build + structure

**Input:** "Build the service module following the accountability structure design."

**Compiler output:** ["IMPLEMENT", "ARCHITECTURE"]

**Router decision:**
- IMPLEMENT (build, service, module) → 4.1
- ARCHITECTURE (structure, design) → 3.1
- DomainId: "software-development"
- Routes: 4.1 + 3.1

**Gap warnings forwarded:**
- gap-project-managing-accountability-001
