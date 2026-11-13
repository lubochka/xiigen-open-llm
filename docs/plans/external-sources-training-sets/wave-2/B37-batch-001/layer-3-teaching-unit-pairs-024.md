# Layer 3 — Teaching-Unit Pairs (024)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: teaching-unit-pairs

---

## B37-L3-024-001 — Teaching: complete B37 gap vocabulary reference (base + extended)

**Base (002-012) — 16 gap vocabulary families:**
track, coordinate, blockers, on-track/off-track, RAG-status, RAID log, progress, status report, escalation, accountability, follow-up, action items, manage, delivery, workstream, dependencies (no API)

**Extended (013-024) — additional gap vocabulary:**
risk log, change request, project charter, decision log, lessons learned, RACI, sign-off, ownership, programme board, steering committee

All zero predicates: DomainId "project-management", knowledge_edge, SCL emitted.

---

## B37-L3-024-002 — Teaching: complete false positive trigger summary

**Four false positive categories (all B37 vocabulary):**

DOCUMENT: stakeholder, requirements, define, document, describe, spec, criteria, functional, behavior, use-case, story
ARCHITECTURE: structure, design, architecture, architectural
QA: review, validate
IMPLEMENT: build, create, implement, write, service, module
API_CONTRACT: dependencies + (api/contract/endpoint/interface/integration/swagger/openapi/graphql/rest/grpc/webhook)

All five fire regardless of managing context.

---

## B37-L3-024-003 — Teaching: 12 key B37 rules (extended version)

Points 1-12 from base vocabulary (see L3-012), PLUS:
13. "risk log" → gap: gap-project-managing-raid-log-001
14. "change request" → gap: gap-project-managing-manage-delivery-001
15. "project charter" → gap: gap-project-managing-manage-delivery-001
16. "decision log" → gap: gap-project-managing-manage-delivery-001
17. "lessons learned" → gap: gap-project-managing-manage-delivery-001
18. "RACI"/"ownership" → gap: gap-project-managing-accountability-001
19. "create" → IMPLEMENT (even for document creation)
20. "log" alone → zero predicates (not DOCUMENT)

---

## B37-L3-024-004 — Teaching: complete extended B37 exam (10 questions)

Q1: "Run lessons-learned session." → predicates?
A: []

Q2: "Maintain the risk log." → predicates?
A: []

Q3: "Define the accountability structure." → predicates?
A: [DOCUMENT, ARCHITECTURE]

Q4: "Map API dependencies between services." → predicates?
A: [API_CONTRACT, IMPLEMENT]

Q5: "Notify stakeholders of change request." → predicates?
A: [DOCUMENT]

Q6: "Process the change request." → predicates?
A: []

Q7: "Track cross-workstream dependencies." → predicates?
A: []

Q8: "Document lessons learned for stakeholders." → predicates?
A: [DOCUMENT]

Q9: "Create the project charter." → predicates?
A: [IMPLEMENT]

Q10: DomainId for Q6?
A: "project-management"

---

## B37-L3-024-005 — Teaching: extended B37 certification statement

**A model certified on B37-batch-001 (extended) demonstrates:**

1. Correct zero-predicate handling for all extended B37 vocabulary (risk log, change request, project charter, decision log, lessons learned, accountability, RACI)
2. Correct false positive identification: stakeholder → DOCUMENT, structure → ARCHITECTURE, review → QA, define → DOCUMENT, create → IMPLEMENT
3. Correct API co-occurrence rule: team/project/workstream dependencies → gap; API/integration/endpoint dependencies → API_CONTRACT
4. Correct DomainId computation: zero predicates → project-management; any predicate → software-development
5. Correct SCL emission: zero-predicate only
6. Zero v6-01, v6-02, v6-03 corruptions for extended vocabulary
7. All base vocabulary rules (from 002-012) maintained

Certification metric: Pass all 10 exam questions (B37-L3-024-004) + all 10 from base exam (B37-L3-012-002) with zero v6 corruptions.
