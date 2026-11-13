# Layer 3 — Teaching-Unit Pairs (018)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: teaching-unit-pairs

---

## B37-L3-018-001 — Teaching: accountability is gap vocabulary

**Concept:** "Accountability", "ownership", "RACI" are B37 gap vocabulary with no registered predicate.

Q: "Establish accountability." → predicates?
A: []. DomainId "project-management". knowledge_edge.

Q: "Assign RACI to activities." → predicates?
A: []. DomainId "project-management".

Q: "Define the accountability structure." → predicates?
A: [DOCUMENT, ARCHITECTURE]. "define" fires DOCUMENT; "structure" fires ARCHITECTURE.

---

## B37-L3-018-002 — Teaching: accountability + structure boundary (most important)

**The accountability + structure boundary is the most common false positive in 013-024:**

"accountability" alone → zero predicates
"accountability" + "structure" → ARCHITECTURE fires
"accountability" + "define" → DOCUMENT fires
"accountability" + "define" + "structure" → DOCUMENT + ARCHITECTURE fire

This boundary applies equally to:
- "governance structure"
- "org structure"
- "team structure"
- "reporting structure"
- "programme structure"

---

## B37-L3-018-003 — Teaching: accountability compound gap ID

When both escalation and accountability appear:

Compound gap ID: gap-project-managing-escalation-accountability-001

Q: "Coordinate accountability and escalation." → predicates?
A: []. Compound gap ID fires. DomainId "project-management".

Individual gap IDs also raised:
- gap-project-managing-accountability-001
- gap-project-managing-escalation-001

---

## B37-L3-018-004 — Teaching: accountability false positive table

| Trigger | Predicate | Gap warning |
|---------|----------|------------|
| structure | ARCHITECTURE | accountability-001 |
| define + structure | [DOCUMENT, ARCHITECTURE] | accountability-001 |
| document | DOCUMENT | accountability-001 |
| build | IMPLEMENT | accountability-001 |
| create | IMPLEMENT | accountability-001 |
| review | QA | accountability-001 |
| (none of the above) | None | accountability-001 |

---

## B37-L3-018-005 — Teaching: accountability exam

1. "Establish accountability for workstreams." → []
2. "Coordinate RACI across teams." → []
3. "Assign ownership for delivery." → []
4. "Define the accountability structure." → [DOCUMENT, ARCHITECTURE]
5. "Map the accountability structure." → [ARCHITECTURE]
6. "Document the accountability framework." → [DOCUMENT]
7. "Build the RACI matrix." → [IMPLEMENT]
8. "Review the accountability protocols." → [QA]
9. DomainId for question 1? → "project-management"
10. DomainId for question 5? → "software-development"
