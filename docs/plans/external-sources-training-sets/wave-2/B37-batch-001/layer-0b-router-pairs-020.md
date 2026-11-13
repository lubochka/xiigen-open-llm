# Layer 0-B — Router Pairs (020)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-020-001 — governance structure: [DOCUMENT, ARCHITECTURE] → 3.1+2.1

**Input:** "Define the governance structure for the programme delivery board."

**Compiler output:** ["DOCUMENT", "ARCHITECTURE"]

**Router decision:**
- DOCUMENT (from "define") → DocumentationFlow (2.1)
- ARCHITECTURE (from "structure") → ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Routes: 3.1 + 2.1 (parallel eligible)

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001

---

## B37-0B-020-002 — org structure: [ARCHITECTURE] → 3.1

**Input:** "Map out the org structure for the delivery team and assign workstream accountability."

**Compiler output:** ["ARCHITECTURE"]

**Router decision:**
- ARCHITECTURE predicate (from "structure") → ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Route: 3.1

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001
- gap-project-managing-work-stream-001
- gap-project-managing-accountability-001

---

## B37-0B-020-003 — reporting structure: [ARCHITECTURE] → 3.1

**Input:** "Clarify the reporting structure and escalation paths for all project workstreams."

**Compiler output:** ["ARCHITECTURE"]

**Router decision:**
- ARCHITECTURE predicate (from "structure") → ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Route: 3.1

**Gap warnings forwarded:**
- gap-project-managing-escalation-001
- gap-project-managing-work-stream-001

---

## B37-0B-020-004 — escalation paths without "structure": [DOCUMENT] → 2.1

**Input:** "Define the escalation paths and accountability levels for all delivery workstreams."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate (from "define") → DocumentationFlow (2.1)
- DomainId: "software-development"
- No ARCHITECTURE (no "structure", "design", or "architecture" token)

**Gap warnings forwarded:**
- gap-project-managing-escalation-001
- gap-project-managing-accountability-001
- gap-project-managing-work-stream-001

**Routing contrast:** Compare to 020-003. Without "structure", ARCHITECTURE does not fire. Routing is 2.1 only.

---

## B37-0B-020-005 — team structure: [ARCHITECTURE] → 3.1

**Input:** "Establish the team structure for the agile delivery pod and confirm workstream ownership."

**Compiler output:** ["ARCHITECTURE"]

**Router decision:**
- ARCHITECTURE predicate (from "structure") → ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Route: 3.1
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Gap warnings forwarded:**
- gap-project-managing-work-stream-001
- gap-project-managing-accountability-001

**Note:** "team structure" fires ARCHITECTURE and routes to 3.1 even though the intent is team organisation, not software architecture. ARCHITECTURE always fires on "structure".
