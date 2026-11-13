# Layer 0-B — Router Pairs (006)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: router-pairs

---

## B37-0B-006-001 — [QA] → QA sub-phase: review escalation log

**Input:** "Review the escalation log and follow up with the engineering team on open action items."

**Compiler output:** ["QA"]

**Router decision:**
- QA present → QA sub-phase routing
- DomainId: "software-development"
- Route: QA sub-phase

**Gap warnings forwarded:**
- gap-project-managing-escalation-001
- gap-project-managing-follow-up-001
- gap-project-managing-action-items-001

**Router note:** "review" fires QA regardless of managing context. The intent (reviewing an escalation log, following up on action items) is managing vocabulary — all gap. Only QA routes.

---

## B37-0B-006-002 — [QA] → QA sub-phase: review action items list

**Input:** "Review the weekly action items and confirm all items have owners assigned."

**Compiler output:** ["QA"]

**Router decision:**
- QA (review) → QA sub-phase
- DomainId: "software-development"
- Route: QA sub-phase

**Gap warnings forwarded:**
- gap-project-managing-action-items-001
- gap-project-managing-accountability-001 (owners = accountability)

---

## B37-0B-006-003 — [QA] → QA sub-phase: review RAID log

**Input:** "Review all RAID log entries before the steering committee meeting."

**Compiler output:** ["QA"]

**Router decision:**
- QA (review) → QA sub-phase
- DomainId: "software-development"
- Route: QA sub-phase

**Gap warnings forwarded:**
- gap-project-managing-raid-log-001

---

## B37-0B-006-004 — [] → knowledge_edge: escalation without "review"

**Input:** "Escalate all critical risks from the RAID log to the programme board."

**Compiler output:** []

**Router decision:**
- No predicates → knowledge_edge
- DomainId: "project-management"
- Route: knowledge_edge
- SCHEDULE_CAPABILITY_LEARNING emitted

**Gap warnings forwarded:**
- gap-project-managing-escalation-001
- gap-project-managing-raid-log-001

**Router contrast note:** Adding "review" to the request above would fire QA and route to QA sub-phase. Without "review", the request stays as zero predicates → knowledge_edge.

---

## B37-0B-006-005 — [QA, DOCUMENT] → QA sub-phase + 2.1: review + document in managing sentence

**Input:** "Review the escalation criteria and document the approval thresholds."

**Compiler output:** ["QA", "DOCUMENT"]

**Router decision:**
- QA (review) → QA sub-phase
- DOCUMENT (document, criteria) → 2.1
- DomainId: "software-development"
- Routes: QA sub-phase + 2.1

**Gap warnings forwarded:**
- gap-project-managing-escalation-001
