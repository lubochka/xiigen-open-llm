# Layer 2 — Capability-Gap-Extended Pairs (008)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-008-001 — Escalation + accountability: full knowledge_edge trace; compound gap ID

**Input:** "Establish escalation and accountability procedures for all critical delivery risks."

**Full knowledge_edge trace:**

Step 1 — Compiler scan:
- "Establish" → not in any predicate lexicon
- "escalation" → not in any predicate lexicon (Rule 13) → gap
- "accountability" → not in any predicate lexicon → gap
- "procedures" → not in any predicate lexicon
- "critical" → not in any predicate lexicon
- "delivery" → gap
- "risks" → gap (RAID-R)

Step 2 — Predicate list: []

Step 3 — Gap mapping:
- "escalation" → gap-project-managing-escalation-001
- "accountability" → gap-project-managing-accountability-001
- "escalation + accountability" → gap-project-managing-escalation-accountability-001 (compound)
- "delivery" → gap-project-managing-manage-delivery-001
- "risks" → gap-project-managing-raid-log-001

Step 4 — DomainId: "project-management"

Step 5 — Route: knowledge_edge

Step 6 — SCHEDULE_CAPABILITY_LEARNING emitted

**Gap: gap-project-managing-escalation-accountability-001 (compound), plus individual IDs**

---

## B37-L2-008-002 — "define" fires DOCUMENT: escalation + accountability sentence

**Input:** "Define the escalation and accountability framework for the project governance."

**Trace:**
- "Define" → DOCUMENT fires (Rule 8)
- "escalation" → gap (Rule 13)
- "accountability" → gap
- "framework" → not in any predicate lexicon
- "project governance" → not in any predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-escalation-accountability-001, gap-project-managing-escalation-001, gap-project-managing-accountability-001

---

## B37-L2-008-003 — "structure" fires ARCHITECTURE: escalation + accountability + structure

**Input:** "Design the escalation and accountability structure for the delivery programme."

**Trace:**
- "Design" → ARCHITECTURE fires (Rule 20: design → ARCHITECTURE)
- "escalation" → gap (Rule 13)
- "accountability" → gap
- "structure" → ARCHITECTURE fires (Rule 7)
- "delivery" → gap
- "programme" → not in any predicate lexicon

Result: ["ARCHITECTURE"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-escalation-accountability-001, gap-project-managing-escalation-001, gap-project-managing-accountability-001, gap-project-managing-manage-delivery-001

---

## B37-L2-008-004 — Pure escalation: no accountability vocabulary

**Input:** "Escalate all amber and red RAID items to the programme director."

**Trace:**
- "Escalate" → not in any predicate lexicon (Rule 13) → gap
- "RAID" → gap
- "items" → not in any predicate lexicon
- "programme" → not in any predicate lexicon

Result: []

DomainId: "project-management"

Gap IDs: gap-project-managing-escalation-001, gap-project-managing-raid-log-001

SCL emitted

---

## B37-L2-008-005 — Pure accountability: no escalation vocabulary

**Input:** "Assign clear accountability for each project deliverable and track ownership."

**Trace:**
- "Assign" → not in any predicate lexicon
- "accountability" → gap
- "track" → gap (Rule 2: track ≠ TEST)
- "ownership" → not in any predicate lexicon

Result: []

DomainId: "project-management"

Gap IDs: gap-project-managing-accountability-001, gap-project-managing-track-001

SCL emitted

**Note:** Even though "accountability" and "track" are both common words in non-managing contexts that do fire predicates (structure → ARCHITECTURE, coverage → TEST), neither "accountability" nor "track" is a predicate trigger.
