# Layer 3 — Teaching-Unit Pairs (022)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: teaching-unit-pairs

---

## B37-L3-022-001 — Teaching: managing vocab + DOCUMENT (stakeholder = DOCUMENT)

**Concept:** When managing vocabulary and "stakeholder" both appear, DOCUMENT fires on "stakeholder" and managing vocabulary becomes gap warnings.

Q: "Track blockers and report to stakeholders." → predicates?
A: [DOCUMENT]. "stakeholders" fires DOCUMENT. "track" and "blockers" → gap warnings.

Q: "Track blockers and report to programme board." → predicates?
A: []. No "stakeholder" → zero predicates. DomainId "project-management".

---

## B37-L3-022-002 — Teaching: the single-token difference

**Teaching point:** In B37 managing sentences, the single token "stakeholder" is often the only difference between zero predicates and DOCUMENT.

Sentence A: "Track progress, coordinate escalations, and update the programme board."
→ []

Sentence B: "Track progress, coordinate escalations, and update stakeholders."
→ [DOCUMENT]

Same vocabulary, same intent, same managing framing. One token changes the result.

---

## B37-L3-022-003 — Teaching: gap warning count with managing + stakeholder

When managing vocabulary + "stakeholder" produces [DOCUMENT]:
- Many gap IDs fire alongside the one predicate
- One route: DocumentationFlow (2.1)
- Many gap warnings forwarded

Example: "Track blockers, coordinate escalations, follow up on action items, update stakeholders."
Result: [DOCUMENT] — one predicate, many gap IDs, one route.

---

## B37-L3-022-004 — Teaching: v6-03 for managing + stakeholder

**v6-03 risk:** DomainId "project-management" despite DOCUMENT firing on "stakeholders".

Example: "Update stakeholders on project status."
v6-03: DomainId "project-management" (wrong — intent is managing, so managing DomainId)
Correct: DomainId "software-development" (DOCUMENT fired → any predicate → "software-development")

---

## B37-L3-022-005 — Teaching: managing + DOCUMENT exam

1. "Track progress for stakeholders." → [DOCUMENT]
2. "Track progress for steering committee." → []
3. "Coordinate escalations and update stakeholders." → [DOCUMENT]
4. "Coordinate escalations and update programme board." → []
5. "Report blockers to stakeholders." → [DOCUMENT]
6. "Report blockers to delivery team." → []
7. "Follow up on action items with stakeholders." → [DOCUMENT]
8. DomainId for question 2? → "project-management"
9. DomainId for question 1? → "software-development"
10. Identify corruption: "Update stakeholders. → DomainId: project-management" → v6-03
