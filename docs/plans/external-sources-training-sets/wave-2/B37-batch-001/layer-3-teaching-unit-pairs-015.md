# Layer 3 — Teaching-Unit Pairs (015)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: teaching-unit-pairs

---

## B37-L3-015-001 — Teaching: project charter is gap vocabulary

**Concept:** "Project charter" is B37 gap vocabulary. It is a programme initiation document, not a software specification.

Q: "Circulate the project charter." → predicates?
A: []. DomainId "project-management". knowledge_edge.

Q: "Approve the project charter." → predicates?
A: []. DomainId "project-management".

**Important false positive:**
Q: "Create the project charter." → predicates?
A: [IMPLEMENT]. "create" fires IMPLEMENT (Rule 9) even for document creation. DomainId "software-development".

---

## B37-L3-015-002 — Teaching: "create" false positive for governance documents

**Key teaching point:** "create" fires IMPLEMENT in ALL contexts.

Q: "Create the project charter." → [IMPLEMENT]
Q: "Create the risk log." → [IMPLEMENT]
Q: "Create the RACI matrix." → [IMPLEMENT]
Q: "Create the decision log." → [IMPLEMENT]

All four fire IMPLEMENT on "create". To avoid IMPLEMENT false positive for document creation, use: "prepare", "circulate", "finalise", "distribute", "sign off" — none of these fire IMPLEMENT.

---

## B37-L3-015-003 — Teaching: project charter gap ID

**Gap ID:** gap-project-managing-manage-delivery-001

Project charter vocabulary family:
- project charter
- project initiation document (PID)
- business case (project context)
- programme mandate

---

## B37-L3-015-004 — Teaching: false positive scenarios for project charter

| Trigger | Predicate | Gap warning |
|---------|----------|------------|
| create | IMPLEMENT | manage-delivery-001 |
| define | DOCUMENT | manage-delivery-001 |
| stakeholder | DOCUMENT | manage-delivery-001 + stakeholder-update-001 |
| review | QA | manage-delivery-001 |
| validate | QA | manage-delivery-001 |

"Circulate/Finalise/Distribute/Approve" → zero predicates.

---

## B37-L3-015-005 — Teaching: project charter exam

1. "Circulate the project charter." → []
2. "Finalise the project charter." → []
3. "Approve the project charter." → []
4. "Create the project charter." → [IMPLEMENT]
5. "Define scope in the charter." → [DOCUMENT]
6. "Share charter with stakeholders." → [DOCUMENT]
7. "Review the project charter." → [QA]
8. "Define the charter structure." → [DOCUMENT, ARCHITECTURE]
9. DomainId for question 1? → "project-management"
10. DomainId for question 4? → "software-development"
