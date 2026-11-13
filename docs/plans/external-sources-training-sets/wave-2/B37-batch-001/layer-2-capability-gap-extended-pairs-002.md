# Layer 2 — Capability-Gap-Extended Pairs (002)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-002-001 — "track progress" vocabulary: full knowledge_edge trace

**Input:** "Track the delivery progress across all project workstreams and report weekly."

**Full knowledge_edge trace:**

Step 1 — Compiler scan:
- "Track" → NOT in TEST lexicon (Rule 2: track ≠ TEST). NOT in any predicate lexicon. → gap
- "delivery" → not in any predicate lexicon → gap
- "progress" → not in any predicate lexicon (Rule 12) → gap
- "workstreams" → not in any predicate lexicon → gap
- "report" → DOCUMENT does NOT fire on "report" (Rule 3) → gap context
- "weekly" → not in any predicate lexicon

Step 2 — Predicate list: []

Step 3 — Gap mapping:
- "track progress" → gap-project-managing-track-progress-001 (compound)
- "track" → gap-project-managing-track-001
- "delivery" → gap-project-managing-manage-delivery-001
- "workstreams" → gap-project-managing-work-stream-001

Step 4 — DomainId: "project-management"

Step 5 — Route: knowledge_edge

Step 6 — SCHEDULE_CAPABILITY_LEARNING: emitted
- gap_ids: [gap-project-managing-track-progress-001, gap-project-managing-manage-delivery-001, gap-project-managing-work-stream-001]

**Gap: gap-project-managing-track-progress-001**

---

## B37-L2-002-002 — "track status" vocabulary: on-track/off-track trace

**Input:** "Report the on-track or off-track status for each workstream to the PMO."

**Full knowledge_edge trace:**

Step 1 — Compiler scan:
- "Report" → DOCUMENT does NOT fire (Rule 3)
- "on-track" → not in any predicate lexicon → gap
- "off-track" → not in any predicate lexicon → gap
- "status" → not in any predicate lexicon (Rule 4)
- "workstream" → gap
- "PMO" → not in any predicate lexicon

Step 2 — Predicate list: []

Step 3 — Gap mapping:
- "on-track/off-track" → gap-project-managing-track-status-001
- "status" → gap (no gap ID; subsumed under track-status-001)
- "workstream" → gap-project-managing-work-stream-001

Step 4 — DomainId: "project-management"

Step 5 — Route: knowledge_edge

Step 6 — SCHEDULE_CAPABILITY_LEARNING: emitted

**Gap: gap-project-managing-track-status-001, gap-project-managing-work-stream-001**

---

## B37-L2-002-003 — "manage delivery": mixed-signal gap warning

**Input:** "Manage the delivery status and escalate any off-track workstreams to the steering committee."

**Full knowledge_edge trace:**

Step 1 — Compiler scan:
- "Manage" → not in any predicate lexicon (Rule 10) → gap
- "delivery" → gap
- "status" → gap (Rule 4)
- "escalate" → not in any predicate lexicon (Rule 13) → gap
- "off-track" → gap
- "workstreams" → gap
- "steering" → not in any predicate lexicon

Step 2 — Predicate list: []

Step 3 — Gap mapping:
- "manage delivery" → gap-project-managing-manage-delivery-001 (mixed-signal warning)
- "escalate" → gap-project-managing-escalation-001
- "off-track workstreams" → gap-project-managing-track-status-001, gap-project-managing-work-stream-001

Step 4 — DomainId: "project-management"

Step 5 — Route: knowledge_edge

Step 6 — SCHEDULE_CAPABILITY_LEARNING: emitted

**Note:** gap-project-managing-manage-delivery-001 is flagged as a mixed-signal warning because "manage" + "delivery" creates a false positive risk if IMPLEMENT tokens are later added (e.g., "manage delivery of the service").

---

## B37-L2-002-004 — "progress" alone: zero-predicate confirmation

**Input:** "Provide a progress update on the current delivery programme."

**Trace:**
- "progress" → not in any predicate lexicon (Rule 12) → gap
- "update" → not in any predicate lexicon
- "delivery" → gap
- "programme" → not in any predicate lexicon

Result: [], DomainId "project-management", knowledge_edge, SCL emitted

Gap IDs: gap-project-managing-progress-001, gap-project-managing-manage-delivery-001

**Gap: gap-project-managing-progress-001**

---

## B37-L2-002-005 — "track progress" + "stakeholder": predicate fires

**Input:** "Track the delivery progress and update stakeholders on the current status."

**Trace:**
- "Track" → gap (Rule 2)
- "delivery" → gap
- "progress" → gap (Rule 12)
- "update" → not in DOCUMENT trigger list
- "stakeholders" → DOCUMENT fires (Rule 6)
- "status" → gap (Rule 4)

Result: ["DOCUMENT"], DomainId "software-development"

CapabilityGapWarnings: gap-project-managing-track-progress-001, gap-project-managing-manage-delivery-001, gap-project-managing-stakeholder-update-001

**Key: "track progress" is gap; "stakeholders" fires DOCUMENT. Managing intent unmet despite DOCUMENT routing.**
