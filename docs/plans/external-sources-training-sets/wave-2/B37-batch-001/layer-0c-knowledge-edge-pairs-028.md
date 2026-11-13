# Layer 0-C — Knowledge-Edge Pairs (028)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-028-001 — "escalation process" knowledge-edge handling

**Zero-predicate requests for "escalation process":**

1. "Run the escalation process." → knowledge_edge | gap-project-managing-escalation-001
2. "Manage escalation paths." → knowledge_edge | gap-project-managing-escalation-001
3. "Escalate delivery blockers." → knowledge_edge | gap-project-managing-escalation-001 + blockers-001
4. "Coordinate escalation with programme board." → knowledge_edge | gap-project-managing-escalation-001 + coordinate-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-028-002 — "communication plan" knowledge-edge handling (alone)

**Zero-predicate requests for "communication plan":**

1. "Prepare the communication plan." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Circulate the communication plan." → knowledge_edge | gap-project-managing-manage-delivery-001
3. "Update the communication plan." → knowledge_edge | gap-project-managing-manage-delivery-001
4. "Review the communication plan." → SCL NOT emitted: "review" → QA fires

Corrected Item 4: "Coordinate the communication plan update." → knowledge_edge | SCL emitted

---

## B37-0C-028-003 — "project kickoff" knowledge-edge handling

**Zero-predicate requests:**

1. "Facilitate project kickoff." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Coordinate project kickoff activities." → knowledge_edge | gap-project-managing-coordinate-001 + manage-delivery-001
3. "Run the project kickoff session." → knowledge_edge | gap-project-managing-manage-delivery-001
4. "Manage project kickoff preparation." → knowledge_edge | gap-project-managing-manage-delivery-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-028-004 — "steering committee" knowledge-edge handling

**Zero-predicate requests:**

1. "Present to the steering committee." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Coordinate steering committee preparation." → knowledge_edge | gap-project-managing-coordinate-001
3. "Track steering committee action items." → knowledge_edge | gap-project-managing-action-items-001 + track-001
4. "Escalate to the steering committee." → knowledge_edge | gap-project-managing-escalation-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

**Boundary:** "steering committee" ≠ "stakeholders" — committee names do NOT fire DOCUMENT.

---

## B37-0C-028-005 — "change control board" knowledge-edge handling

**Zero-predicate requests:**

1. "Submit to the change control board." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Coordinate CCB process." → knowledge_edge | gap-project-managing-coordinate-001
3. "Track CCB decisions." → knowledge_edge | gap-project-managing-track-001
4. "Escalate to the CCB." → knowledge_edge | gap-project-managing-escalation-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".
