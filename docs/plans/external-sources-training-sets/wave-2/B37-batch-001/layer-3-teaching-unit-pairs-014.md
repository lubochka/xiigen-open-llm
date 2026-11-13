# Layer 3 — Teaching-Unit Pairs (014)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: teaching-unit-pairs

---

## B37-L3-014-001 — Teaching: change request is gap vocabulary

**Concept:** "Change request" and change management vocabulary are B37 gap vocabulary with no registered predicate.

Q: "Process the change request." → predicates?
A: []. DomainId "project-management". knowledge_edge. SCL emitted.

Q: "Coordinate change request approval." → predicates?
A: []. DomainId "project-management".

Q: "Track open change requests." → predicates?
A: []. "track" ≠ TEST (Rule 2). DomainId "project-management".

---

## B37-L3-014-002 — Teaching: "process" ≠ IMPLEMENT boundary

**Key boundary:** "process" is NOT an IMPLEMENT trigger.

IMPLEMENT fires on: build, implement, create, write, service, module.
"Process" is NOT on this list.

Q: "Process the change request." → predicates?
A: []. Zero predicates.

Q: "Implement the change request." → predicates?
A: [IMPLEMENT]. "implement" fires IMPLEMENT.

Q: "Build the change request system." → predicates?
A: [IMPLEMENT]. "build" fires IMPLEMENT.

---

## B37-L3-014-003 — Teaching: change request gap ID

**Gap ID:** gap-project-managing-manage-delivery-001

Change request vocabulary family (all → same gap ID):
- change request
- change log
- change advisory board (CAB)
- change freeze
- change impact assessment

---

## B37-L3-014-004 — Teaching: false positive scenarios for change request

| Trigger | Predicate | Gap warning |
|---------|----------|------------|
| document | DOCUMENT | manage-delivery-001 |
| stakeholder | DOCUMENT | manage-delivery-001 + stakeholder-update-001 |
| review | QA | manage-delivery-001 |
| implement | IMPLEMENT | manage-delivery-001 |
| build | IMPLEMENT | manage-delivery-001 |
| create | IMPLEMENT | manage-delivery-001 |

---

## B37-L3-014-005 — Teaching: change request exam

1. "Process the change request." → []
2. "Track open change requests." → []
3. "Coordinate CAB approval." → []
4. "Escalate the change request." → []
5. "Review change requests." → [QA]
6. "Document change request." → [DOCUMENT]
7. "Notify stakeholders of change." → [DOCUMENT]
8. "Implement the change request." → [IMPLEMENT]
9. "Build the change log system." → [IMPLEMENT]
10. DomainId for question 3? → "project-management"

Questions 1-4: knowledge_edge, SCL emitted.
Questions 5-9: predicate flows, no SCL.
