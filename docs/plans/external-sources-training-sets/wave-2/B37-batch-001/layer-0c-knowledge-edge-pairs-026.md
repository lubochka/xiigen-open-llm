# Layer 0-C — Knowledge-Edge Pairs (026)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-026-001 — "Gantt chart" knowledge-edge handling

**Zero-predicate requests for "Gantt chart":**

1. "Maintain the Gantt chart." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Update milestone dates in the Gantt chart." → knowledge_edge | gap-project-managing-manage-delivery-001 + track-001
3. "Share the Gantt chart with the delivery team." → knowledge_edge | gap-project-managing-manage-delivery-001
4. "Circulate the Gantt chart to the programme board." → knowledge_edge | gap-project-managing-manage-delivery-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-026-002 — "resource allocation" knowledge-edge handling

**Zero-predicate requests for "resource allocation":**

1. "Manage resource allocation for the workstream." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Coordinate resource allocation across teams." → knowledge_edge | gap-project-managing-coordinate-001 + manage-delivery-001
3. "Track resource allocation utilisation." → knowledge_edge | gap-project-managing-track-001 + manage-delivery-001
4. "Flag resource allocation blockers to the programme board." → knowledge_edge | gap-project-managing-manage-delivery-001 + blockers-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-026-003 — "capacity planning" knowledge-edge handling

**Zero-predicate requests for "capacity planning":**

1. "Run the capacity planning session." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Facilitate capacity planning for Q4." → knowledge_edge | gap-project-managing-manage-delivery-001
3. "Coordinate capacity planning across workstreams." → knowledge_edge | gap-project-managing-coordinate-001 + manage-delivery-001 + work-stream-001
4. "Review capacity planning outcomes and update resource allocation." → gap: SCL NOT emitted ("review" fires QA)

Wait — Item 4 has "review" → QA fires. Corrected:
4. "Coordinate capacity planning outcomes and update resource allocation." → knowledge_edge | SCL emitted

---

## B37-0C-026-004 — "dependency mapping" knowledge-edge handling

**Zero-predicate requests for dependency mapping:**

1. "Map team dependencies." → knowledge_edge | gap-project-managing-dependencies-team-001
2. "Map cross-workstream dependencies." → knowledge_edge | gap-project-managing-dependencies-team-001 + work-stream-001
3. "Track delivery dependencies across workstreams." → knowledge_edge | gap-project-managing-dependencies-team-001 + track-001
4. "Identify critical dependencies for the programme." → knowledge_edge | gap-project-managing-dependencies-team-001 + manage-delivery-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

**"service dependencies" — special case:**
"Resolve service dependencies" → "service" → IMPLEMENT fires. Predicates: ["IMPLEMENT"]. knowledge_edge NOT the route.

---

## B37-0C-026-005 — DomainId rule for Round 2 vocabulary

**DomainId "project-management" for Round 2:**
- Zero predicates from entire token scan
- All tokens are Round 2 or base B37 gap vocabulary
- SCHEDULE_CAPABILITY_LEARNING emitted

**DomainId "software-development" for Round 2:**
- ANY of: review, validate, stakeholder, define, document, criteria, structure, design, architecture, build, create, implement, write, service, module fires
- OR: dependencies + API co-occurrence fires API_CONTRACT
- Even if majority of vocabulary is PM gap vocabulary
- SCHEDULE_CAPABILITY_LEARNING NOT emitted
- Gap warnings forwarded alongside routing

Rule has NO exceptions for Round 2 vocabulary.
