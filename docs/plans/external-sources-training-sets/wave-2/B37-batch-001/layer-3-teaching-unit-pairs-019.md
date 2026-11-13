# Layer 3 — Teaching-Unit Pairs (019)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: teaching-unit-pairs

---

## B37-L3-019-001 — Teaching: "stakeholder" boundary rule (absolute)

**Concept:** "stakeholder" fires DOCUMENT in EVERY context. No exceptions.

Q: "Notify stakeholders." → predicates?
A: [DOCUMENT]. DomainId "software-development".

Q: "Escalate to stakeholders." → predicates?
A: [DOCUMENT]. DomainId "software-development".

Q: "Notify the programme board." → predicates?
A: []. DomainId "project-management". "programme board" ≠ "stakeholder".

The ONLY token that changes routing from knowledge_edge to 2.1 in this context is "stakeholder".

---

## B37-L3-019-002 — Teaching: "stakeholder" vs non-firing equivalents

| Token | Predicate |
|-------|----------|
| stakeholder | DOCUMENT |
| stakeholders | DOCUMENT |
| programme board | None |
| steering committee | None |
| senior leadership | None |
| delivery team | None |
| project sponsor | None |
| executive | None |

Only "stakeholder" and "stakeholders" fire DOCUMENT.

---

## B37-L3-019-003 — Teaching: v6-02 corruption pattern for stakeholders

**v6-02 is the DOCUMENT suppression corruption:**

v6-02 corrupt: "Send update to stakeholders" → Predicates: [] (suppressed because "this is communication")

Detection: "stakeholder" present in input + DOCUMENT absent from output = v6-02

**Exam question:** Is this output correct?
Input: "Update all stakeholders on delivery status."
Output: Predicates: [], DomainId "project-management"

A: NO. This is v6-02 corruption. "stakeholders" fires DOCUMENT (Rule 6 — absolute). Correct: Predicates: [DOCUMENT], DomainId "software-development".

---

## B37-L3-019-004 — Teaching: gap warning always accompanies stakeholder DOCUMENT

When "stakeholder" fires DOCUMENT in managing context:
- gap-project-managing-stakeholder-update-001 is ALWAYS raised
- Additional managing vocabulary gap IDs are raised as applicable
- SCHEDULE_CAPABILITY_LEARNING is NOT emitted

---

## B37-L3-019-005 — Teaching: stakeholder boundary exam

1. "Notify stakeholders." → [DOCUMENT]
2. "Escalate to stakeholders." → [DOCUMENT]
3. "Send update to programme board." → []
4. "Report status to steering committee." → []
5. "Update stakeholders on blockers." → [DOCUMENT]
6. "Coordinate with delivery team." → []
7. "Track progress for stakeholders." → [DOCUMENT]
8. "Track progress for project sponsor." → []
9. DomainId for question 3? → "project-management"
10. Identify corruption: "Notify all stakeholders. → Predicates: []" → v6-02 (DOCUMENT suppressed)
