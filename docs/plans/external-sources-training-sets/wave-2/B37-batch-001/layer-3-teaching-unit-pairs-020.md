# Layer 3 — Teaching-Unit Pairs (020)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: teaching-unit-pairs

---

## B37-L3-020-001 — Teaching: "structure" boundary rule (absolute)

**Concept:** "structure" fires ARCHITECTURE in EVERY context. No exceptions.

Q: "Define the governance structure." → predicates?
A: [DOCUMENT, ARCHITECTURE]. "define" → DOCUMENT; "structure" → ARCHITECTURE.

Q: "Establish the governance framework." → predicates?
A: []. "framework" ≠ "structure". Zero predicates.

**The difference:** "structure" fires ARCHITECTURE. "framework" does not.

---

## B37-L3-020-002 — Teaching: "structure" vs non-firing equivalents

| Token | Predicate |
|-------|----------|
| structure | ARCHITECTURE |
| design | ARCHITECTURE |
| architecture | ARCHITECTURE |
| architectural | ARCHITECTURE |
| framework | None |
| model | None |
| approach | None |
| hierarchy | None |
| organisation | None |

Only structure/design/architecture/architectural fire ARCHITECTURE.

---

## B37-L3-020-003 — Teaching: v6-03 corruption pattern for structure sentences

**v6-03 is the wrong DomainId corruption:**

v6-03 corrupt: "Map the org structure for the delivery team." → DomainId "project-management" (wrong — "structure" fires ARCHITECTURE → DomainId must be "software-development")

Detection: "structure" present in input + DomainId "project-management" in output = v6-03

**Exam question:** Is this output correct?
Input: "Establish the team structure for the programme."
Output: Predicates: [ARCHITECTURE], DomainId "project-management"

A: NO. v6-03 corruption. DomainId must be "software-development" when any predicate fires (Rule 14).

---

## B37-L3-020-004 — Teaching: "structure" with "define" double trigger

When both "define" and "structure" appear:
- DOCUMENT fires on "define"
- ARCHITECTURE fires on "structure"
- Result: [DOCUMENT, ARCHITECTURE]
- Routes: 2.1 + 3.1
- DomainId: "software-development"

This is the most common multi-predicate pattern in B37 managing sentences.

---

## B37-L3-020-005 — Teaching: structure boundary exam

1. "Define the governance structure." → [DOCUMENT, ARCHITECTURE]
2. "Map the org structure." → [ARCHITECTURE]
3. "Establish the accountability framework." → []
4. "Design the team structure." → [ARCHITECTURE]
5. "Clarify the reporting hierarchy." → []
6. "Document the programme structure." → [DOCUMENT, ARCHITECTURE]
7. "Define the escalation model." → [DOCUMENT]
8. "Establish the team hierarchy." → []
9. DomainId for question 3? → "project-management"
10. Identify corruption: "Map org structure. → DomainId: project-management" → v6-03
