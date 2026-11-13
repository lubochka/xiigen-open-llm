# Layer 2 — Capability-Gap-Extended Pairs (024)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-024-001 — Complete B37 gap vocabulary + boundary rule summary (base + extended)

**Complete gap vocabulary reference:**

Base vocabulary (002-012):
track, coordinate, blockers, on-track/off-track, RAG-status, RAID log, progress, status report, escalation, accountability, follow-up, action items, manage, delivery, workstream, dependencies (no API co-occurrence)

Extended vocabulary (013-024):
risk log, change request, project charter, decision log, lessons learned, RACI, sign-off, programme board, steering committee, ownership

All zero predicates: GapId gap-project-managing-*, DomainId project-managing.

---

## B37-L2-024-002 — Boundary rules summary (all B37 rules)

A: "Provide the complete B37 boundary rule set for the extended vocabulary (013-024)."

Predicates: []
GapId: (reference pair — no input token firing)
DomainId: project-managing

Boundary rules:
1. track ≠ TEST (Rule 2) — applies to all B37 vocabulary
2. report ≠ DOCUMENT (Rule 3) — applies to all B37 vocabulary
3. status → zero predicates (Rule 4) — applies to all B37 vocabulary
4. dependencies + API co-occurrence → API_CONTRACT; without → gap (Rule 5)
5. stakeholder → DOCUMENT always (Rule 6)
6. structure/design/architecture → ARCHITECTURE always (Rule 7)
7. create → IMPLEMENT (false positive for document creation)
8. DomainId binary: zero predicates → project-management; any predicate → software-development
9. SCL: zero-predicate only; one per request
10. v6-01/v6-02/v6-03 unchanged for extended vocabulary

---

## B37-L2-024-003 — Five canonical extended vocabulary gap traces

A: "Run the lessons-learned session." → [], project-managing, knowledge_edge, SCL, gap-project-managing-manage-delivery-001
A: "Maintain the risk log." → [], project-managing, knowledge_edge, SCL, gap-project-managing-raid-log-001
A: "Track change requests." → [], project-managing, knowledge_edge, SCL, gap-project-managing-manage-delivery-001 + track-001
A: "Coordinate RACI accountability." → [], project-managing, knowledge_edge, SCL, gap-project-managing-accountability-001
A: "Resolve team dependencies." → [], project-managing, knowledge_edge, SCL, gap-project-managing-dependencies-team-001

All five: zero predicates, knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-L2-024-004 — Five canonical false positive traces (extended vocabulary)

A: "Notify stakeholders of risk log updates." → [DOCUMENT], software-development, 2.1
A: "Define the accountability structure." → [DOCUMENT, ARCHITECTURE], software-development, 3.1+2.1
A: "Review the decision log." → [QA], software-development, QA sub-phase
A: "API dependencies between services." → [API_CONTRACT, IMPLEMENT], software-development, 3.3+4.1
A: "Build the change request tracking service." → [IMPLEMENT], software-development, 4.1

All five: predicates fire → DomainId "software-development" → no SCL → gap warnings forwarded.

---

## B37-L2-024-005 — B37 gap capability evolution: what 013-024 extension adds

A: "What does the 013-024 extension add to B37-batch-001 capability?"

After training on extended files (013-024), the model should additionally:
1. Recognise risk-log vocabulary as RAID-R gap (gap-project-managing-raid-log-001)
2. Recognise change-request vocabulary as delivery management gap
3. Recognise project-charter vocabulary as delivery management gap
4. Recognise decision-log vocabulary as delivery management gap
5. Recognise lessons-learned vocabulary as delivery management gap
6. Recognise accountability/RACI/ownership as gap-project-managing-accountability-001
7. Apply "create" IMPLEMENT false positive awareness for document creation
8. Apply all existing false positive rules (stakeholder, structure, review, define) to extended vocabulary
9. Maintain all B37 base rules (002-012) unchanged
10. Achieve DomainId "project-management" for all extended vocabulary in zero-predicate requests
