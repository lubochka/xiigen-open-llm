# Layer 0-B — Router Pairs (011)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: router-pairs

---

## B37-0B-011-001 — v6 corruption: DomainId wrong when IMPLEMENT fires with managing vocabulary

**Corrupted output (v6):**
Request: "Build the status tracking module and manage delivery progress across workstreams."
v6 result: ["IMPLEMENT"], DomainId: "project-management"

**Correct output:**
- IMPLEMENT fires (build, module)
- DomainId: "software-development"
- Route: 4.1
- Gap warnings: gap-project-managing-track-status-001, gap-project-managing-manage-delivery-001, gap-project-managing-progress-001, gap-project-managing-work-stream-001

v6 error: v6-03 — DomainId "project-management" set because surrounding vocabulary is project managing. ANY predicate → "software-development".

---

## B37-0B-011-002 — v6 corruption: QA route suppressed in managing context

**Corrupted output (v6):**
Request: "Review the RAID log and track all open risks before the steering committee meeting."
v6 result: DomainId: "project-management", knowledge_edge — QA suppressed because "reviewing RAID is managing activity"

**Correct output:**
- QA fires (review)
- DomainId: "software-development"
- Route: QA sub-phase
- Gap warnings: gap-project-managing-raid-log-001, gap-project-managing-track-001

v6 error: "review" fires QA regardless of context. Router cannot override lexical firing based on semantic intent.

---

## B37-0B-011-003 — v6 corruption: DOCUMENT suppressed for stakeholder in managing context

**Corrupted output (v6):**
Request: "Notify stakeholders of the current escalation status and provide a progress update."
v6 result: [], DomainId: "project-management" — DOCUMENT suppressed because "this is a notification, not documentation"

**Correct output:**
- DOCUMENT fires (stakeholders — Rule 6: fires ALWAYS)
- DomainId: "software-development"
- Route: 2.1
- Gap warnings: gap-project-managing-escalation-001, gap-project-managing-progress-001, gap-project-managing-stakeholder-update-001

v6 error: v6-02 — DOCUMENT suppressed for "stakeholder" because context is notification/managing.

---

## B37-0B-011-004 — Correct handling of all three v6 corruptions in one test case

**Input:** "Coordinate the escalation, notify stakeholders of progress, and build the reporting module."

**Correct analysis:**
- "Coordinate" → gap (Rule 11)
- "escalation" → gap (Rule 13)
- "stakeholders" → DOCUMENT fires (Rule 6 — cannot suppress)
- "progress" → gap (Rule 12)
- "build" → IMPLEMENT fires (Rule 9)
- "reporting" → not a trigger
- "module" → IMPLEMENT fires (Rule 9)

Correct result: ["DOCUMENT", "IMPLEMENT"]
DomainId: "software-development"
Routes: 2.1 + 4.1

Gap warnings: gap-project-managing-coordinate-001, gap-project-managing-escalation-001, gap-project-managing-progress-001

v6 corruptions this example catches:
- v6-01: Would invent MANAGEMENT for "coordinate"
- v6-02: Would suppress DOCUMENT for "stakeholders"
- v6-03: Would set DomainId "project-management" despite IMPLEMENT firing

---

## B37-0B-011-005 — Complete B37 v6 corruption router diagnostic

**Three v6 router corruption tests:**

Test 1 (v6-01 detection):
Input: "Coordinate workstreams and manage blockers."
Expected: [], knowledge_edge, DomainId "project-management"
v6 failure: Routes to invented ManagementFlow

Test 2 (v6-02 detection):
Input: "Update stakeholders on current status."
Expected: ["DOCUMENT"], route 2.1, DomainId "software-development"
v6 failure: Returns [], knowledge_edge, DomainId "project-management"

Test 3 (v6-03 detection):
Input: "Build the tracking service and manage delivery progress."
Expected: ["IMPLEMENT"], route 4.1, DomainId "software-development"
v6 failure: Returns ["IMPLEMENT"], route 4.1, DomainId "project-management" (wrong domain)

All three v6 tests must pass for B37 router certification.
