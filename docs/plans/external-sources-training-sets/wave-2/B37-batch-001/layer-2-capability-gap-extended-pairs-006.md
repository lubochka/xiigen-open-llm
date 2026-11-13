# Layer 2 — Capability-Gap-Extended Pairs (006)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-006-001 — "stakeholder" fires DOCUMENT: full mixed-signal trace

**Input:** "Send a progress update to stakeholders and confirm delivery is on track."

**Full mixed-signal trace:**

Step 1 — Compiler scan:
- "Send" → not in any predicate lexicon
- "progress" → not in any predicate lexicon (Rule 12) → gap
- "update" → not in DOCUMENT trigger list
- "stakeholders" → DOCUMENT fires (Rule 6: ALWAYS — even in this sentence)
- "confirm" → not in any predicate lexicon
- "delivery" → gap
- "on track" → gap

Step 2 — Predicate list: ["DOCUMENT"]

Step 3 — DomainId: "software-development" (predicate fired)

Step 4 — Route: DocumentationFlow (2.1)

Step 5 — SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicate fired)

Step 6 — CapabilityGapWarnings:
- gap-project-managing-progress-001 (progress update intent unmet)
- gap-project-managing-track-status-001 ("on track" delivery tracking unmet)
- gap-project-managing-manage-delivery-001 (delivery status unmet)
- gap-project-managing-stakeholder-update-001 (intent: stakeholder notification, not documentation)

**Gap: gap-project-managing-stakeholder-update-001 (managing intent unmet despite DOCUMENT routing)**

---

## B37-L2-006-002 — "stakeholder" false positive taxonomy

**Three categories of "stakeholder" appearance in B37:**

Category 1 — Genuine documentation (correct DOCUMENT):
"Gather stakeholder requirements for the specification." → DOCUMENT fires correctly. Intent = documentation. No gap warning for stakeholder.

Category 2 — Managing notification (false positive DOCUMENT):
"Notify stakeholders of project blockers." → DOCUMENT fires as false positive. Intent = managing notification. Gap warning: gap-project-managing-stakeholder-update-001.

Category 3 — Mixed intent:
"Share the updated project spec with stakeholders." → DOCUMENT fires on both "spec" and "stakeholders". Intent = documentation sharing. Gap warning for stakeholder-update only if the primary intent was notification, not documentation.

---

## B37-L2-006-003 — CapabilityGapWarning: gap-project-managing-stakeholder-update-001

**This gap ID is emitted when:**
1. "stakeholder(s)" appears in the sentence
2. The sentence framing is managing (update, notify, communicate, inform, escalate to)
3. DOCUMENT fires as a lexical false positive

**This gap ID is NOT emitted when:**
1. The stakeholder reference is in a genuine documentation context (requirements gathering, spec review)
2. The sentence intent is clearly documentation work

**Practical rule:** Emit gap-project-managing-stakeholder-update-001 whenever DOCUMENT fires on "stakeholder" in a B37 managing context.

---

## B37-L2-006-004 — "stakeholders" in escalation sentence

**Input:** "Escalate the critical delivery risk to senior stakeholders and the steering committee."

**Trace:**
- "Escalate" → gap (Rule 13)
- "delivery" → gap
- "risk" → not in any predicate lexicon (gap — RAID-R)
- "stakeholders" → DOCUMENT fires (Rule 6)
- "steering" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-escalation-001, gap-project-managing-manage-delivery-001, gap-project-managing-stakeholder-update-001, gap-project-managing-raid-log-001

---

## B37-L2-006-005 — Five stakeholder false-positive examples with full gap handling

1. "Update stakeholders on blocker status." → ["DOCUMENT"]; gaps: progress-001, blockers-001, stakeholder-update-001
2. "Inform stakeholders of the escalation." → ["DOCUMENT"]; gaps: escalation-001, stakeholder-update-001
3. "Send the RAG-status report to stakeholders." → ["DOCUMENT"]; gaps: rag-status-001, status-report-001, stakeholder-update-001
4. "Coordinate with stakeholders to resolve blockers." → ["DOCUMENT"]; gaps: coordinate-001, blockers-001, stakeholder-update-001
5. "Keep stakeholders informed of on-track/off-track status." → ["DOCUMENT"]; gaps: track-status-001, stakeholder-update-001

In all five: DOCUMENT routes to 2.1. All managing intent captured in gap warnings. SCL not emitted.
