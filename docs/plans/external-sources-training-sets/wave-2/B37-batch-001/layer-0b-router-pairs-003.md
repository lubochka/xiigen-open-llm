# Layer 0-B — Router Pairs (003)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: router-pairs

---

## B37-0B-003-001 — [DOCUMENT] → 2.1: stakeholder fires DOCUMENT

**Input:** "Send a progress update to stakeholders and report the on-track status for each workstream."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate present → route to sub-phase 2.1 (Documentation flow)
- DomainId: "software-development"
- Route: 2.1 (DocumentationFlow)
- CapabilityGapWarnings forwarded to gap handler

**Gap warnings forwarded:**
- gap-project-managing-progress-001
- gap-project-managing-track-status-001
- gap-project-managing-work-stream-001
- gap-project-managing-stakeholder-update-001

**Router note:** The predicate fires on "stakeholder" — managing intent (progress update) is not captured by routing. Sub-phase 2.1 will handle documentation context. The managing gap warnings persist.

---

## B37-0B-003-002 — [DOCUMENT] → 2.1: "define" fires DOCUMENT in managing framing

**Input:** "Define the escalation criteria and share with the project stakeholders."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT predicate present → route to 2.1
- DomainId: "software-development"
- Route: 2.1

**Gap warnings forwarded:**
- gap-project-managing-escalation-001

---

## B37-0B-003-003 — [DOCUMENT] → 2.1: "requirements" fires in delivery context

**Input:** "Document the requirements for the delivery governance process and share with stakeholders."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT fires on "Document", "requirements", "stakeholders" → route 2.1
- DomainId: "software-development"
- Route: 2.1

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001
- gap-project-managing-accountability-001 (governance)

---

## B37-0B-003-004 — [DOCUMENT] → 2.1: router note on false positive in managing context

**Input:** "Notify stakeholders of the current delivery status."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT fires on "stakeholders" → route 2.1
- DomainId: "software-development"
- Route: 2.1

**Gap warnings forwarded:**
- gap-project-managing-status-report-001
- gap-project-managing-manage-delivery-001

**Router note (false positive advisory):** Intent = stakeholder notification (B37 managing). Routing = 2.1 DocumentationFlow. The router does not suppress DOCUMENT for "stakeholder" even when intent is notification-only. The gap warning gap-project-managing-stakeholder-update-001 captures the unmet managing intent.

---

## B37-0B-003-005 — [DOCUMENT] → 2.1 with multiple DOCUMENT triggers

**Input:** "Describe the accountability framework and define the criteria for escalation."

**Compiler output:** ["DOCUMENT"]

**Router decision:**
- DOCUMENT fires on "Describe", "define", "criteria" → three triggers, one predicate, route 2.1
- DomainId: "software-development"
- Route: 2.1

**Gap warnings forwarded:**
- gap-project-managing-accountability-001
- gap-project-managing-escalation-001
