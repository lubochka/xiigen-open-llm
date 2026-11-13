# Layer 2 — Capability-Gap-Extended Pairs (004)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-004-001 — RAG-status reporting: full knowledge_edge trace

**Input:** "Produce the RAG-status report for all active delivery workstreams."

**Full knowledge_edge trace:**

Step 1 — Compiler scan:
- "Produce" → not in IMPLEMENT trigger list (produce ≠ build/create/implement/write)
- "RAG-status" → not in any predicate lexicon → gap
- "report" → DOCUMENT does NOT fire on "report" (Rule 3) → gap context
- "active" → not in any predicate lexicon
- "delivery" → gap
- "workstreams" → gap

Step 2 — Predicate list: []

Step 3 — Gap mapping:
- "RAG-status" → gap-project-managing-rag-status-001
- "report" (as status report intent) → gap-project-managing-status-report-001
- "delivery" → gap-project-managing-manage-delivery-001
- "workstreams" → gap-project-managing-work-stream-001

Step 4 — DomainId: "project-management"

Step 5 — Route: knowledge_edge

Step 6 — SCHEDULE_CAPABILITY_LEARNING emitted

**Gap: gap-project-managing-rag-status-001**

---

## B37-L2-004-002 — RAG-status with "stakeholder": DOCUMENT fires as false positive

**Input:** "Distribute the RAG-status report to all stakeholders and the steering board."

**Trace:**
- "RAG-status" → gap
- "report" → DOCUMENT does NOT fire on "report"
- "stakeholders" → DOCUMENT fires (Rule 6)
- "steering" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-rag-status-001, gap-project-managing-status-report-001, gap-project-managing-stakeholder-update-001

**Key: "RAG-status" is gap; "stakeholder" fires DOCUMENT as false positive. RAG-status intent unmet.**

---

## B37-L2-004-003 — RAG-status with "review": QA fires

**Input:** "Review the RAG-status for each workstream and escalate red items."

**Trace:**
- "Review" → QA fires (Rule 19)
- "RAG-status" → gap
- "workstream" → gap
- "escalate" → gap (Rule 13)

Result: ["QA"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-rag-status-001, gap-project-managing-work-stream-001, gap-project-managing-escalation-001

---

## B37-L2-004-004 — RAG-status with "define": DOCUMENT fires

**Input:** "Define the RAG criteria and specify the RAG-status thresholds for this project."

**Trace:**
- "Define" → DOCUMENT fires (Rule 8)
- "RAG criteria" → "criteria" fires DOCUMENT (Rule 8)
- "specify" → not directly in DOCUMENT trigger list
- "RAG-status" → gap
- "thresholds" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-rag-status-001

---

## B37-L2-004-005 — RAG-status pure gap: five confirmation cases

1. "Update the RAG-status dashboard." → [] (update + RAG-status = all gap)
2. "What is the current RAG-status?" → [] (interrogative, no predicate)
3. "Set RAG-status to amber for workstream 3." → [] (set + RAG-status = all gap)
4. "Track the RAG-status changes week over week." → [] (track family = gap)
5. "Escalate all red RAG-status items to the PMO." → [] (escalate = gap, RAG-status = gap)

All five: DomainId "project-management", knowledge_edge, SCL emitted, gap-project-managing-rag-status-001
