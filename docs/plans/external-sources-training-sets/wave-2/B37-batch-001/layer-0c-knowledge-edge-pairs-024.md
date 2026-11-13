# Layer 0-C — Knowledge-Edge Pairs (024)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-024-001 — Complete B37 gap vocabulary reference: all terms (base + extended)

**Complete B37 gap vocabulary (zero-predicate tokens):**

Base vocabulary (002-012):
track, coordinate, blockers, on-track/off-track, RAG-status, RAID log, progress, status report, escalation, accountability, follow-up, action items, manage, delivery (standalone), workstream, dependencies (no API co-occurrence)

Extended vocabulary (013-024):
risk log, change request, project charter, decision log, lessons learned, RACI, sign-off, programme board, steering committee, ownership, change advisory board, change log, project milestone, delivery outcome

All extended vocabulary: zero predicates, gap-project-managing-* gap IDs, DomainId "project-management".

---

## B37-0C-024-002 — False positive trigger summary: all four categories

**Category 1: DOCUMENT triggers**
Token: stakeholder, requirements, define, document, describe, spec, criteria, functional, behavior, use-case, story
Context in B37: notification, communication, governance — all produce DOCUMENT false positive

**Category 2: ARCHITECTURE triggers**
Token: structure, design, architecture, architectural
Context in B37: governance, org design, team organisation, accountability — all produce ARCHITECTURE false positive

**Category 3: QA triggers**
Token: review, validate
Context in B37: managing review of logs, retrospectives — all produce QA false positive

**Category 4: IMPLEMENT triggers**
Token: build, create, implement, write, service, module
Context in B37: creating artefacts, managing services — all produce IMPLEMENT false positive

---

## B37-0C-024-003 — API co-occurrence rule: final reference

**Rule 5 (dependencies):**
- "dependencies" alone or with team/project/workstream/delivery → gap: gap-project-managing-dependencies-team-001
- "dependencies" with api/contract/endpoint/interface/integration/swagger/openapi/graphql/rest/grpc/webhook → API_CONTRACT fires
- "service dependencies" → IMPLEMENT fires (on "service") + gap warning (dependencies without API co-occurrence)

---

## B37-0C-024-004 — DomainId rule: final confirmation

**DomainId rule for all B37 vocabulary (base + extended):**

DomainId "project-management":
- Zero predicates from entire token scan
- All vocabulary is B37 gap vocabulary (base or extended)
- SCHEDULE_CAPABILITY_LEARNING emitted

DomainId "software-development":
- ANY predicate fires (TEST, QA, DOCUMENT, ARCHITECTURE, IMPLEMENT, API_CONTRACT)
- Even if intent is pure project managing
- Even if most vocabulary is gap
- SCHEDULE_CAPABILITY_LEARNING NOT emitted
- Gap warnings forwarded alongside routing

This rule has NO exceptions in B37.

---

## B37-0C-024-005 — Ten certification test cases for extended B37 vocabulary

1. "Maintain the risk log." → [] — project-management
2. "Track change requests." → [] — project-management
3. "Circulate the project charter." → [] — project-management
4. "Update the decision log." → [] — project-management
5. "Run the lessons-learned session." → [] — project-management
6. "Establish RACI accountability." → [] — project-management
7. "Notify stakeholders of risk log updates." → [DOCUMENT] — software-development
8. "Define the accountability structure." → [DOCUMENT, ARCHITECTURE] — software-development
9. "Map API dependencies between services." → [API_CONTRACT, IMPLEMENT] — software-development
10. "Review lessons learned from the programme." → [QA] — software-development

Cases 1-6: knowledge_edge, SCL emitted.
Cases 7-10: routed to flows, gap warnings forwarded, SCL NOT emitted.
