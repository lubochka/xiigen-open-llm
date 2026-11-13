# Layer 0-C — Knowledge-Edge Pairs (058)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-058-001 — Knowledge-edge: stress test — all Round 3a vocabulary combined

**Input:** "Manage escalation process, circulate communication plan, facilitate project kickoff, coordinate steering committee, submit to change control board, monitor scope creep, run risk mitigation, enforce quality gates, manage sign-off process, manage handover, run post-mortem."

All tokens: gap vocabulary → knowledge_edge
DomainId: "project-management"
SCL: emitted once
11 vocabulary families → multiple gap IDs → ONE SCL event

---

## B37-0C-058-002 — Knowledge-edge: stress test — all Round 4b vocabulary combined

**Input:** "Coordinate waterfall delivery, lead agile transformation, manage SAFe adoption, run PI planning, coordinate portfolio management, map value streams, monitor flow state, run project closure, track benefits realization, monitor KPI dashboard."

All tokens: gap vocabulary → knowledge_edge
DomainId: "project-management"
SCL: emitted once
10 vocabulary families → ONE SCL event

---

## B37-0C-058-003 — Knowledge-edge: stress test — all rounds combined (complete)

**Input:** All base (002-012) + extended (013-024) + Round 2 (025-036) + Round 3a + Round 4b vocabulary.

All tokens: gap vocabulary → knowledge_edge
DomainId: "project-management"
SCL: emitted once
30+ vocabulary families → ONE SCL event
This is the theoretical maximum B37 request — all zero predicates.

---

## B37-0C-058-004 — Knowledge-edge: one-token addition changes everything

**Stress test + "for stakeholders" added:**
All of 058-003 input → DOCUMENT fires → NOT knowledge_edge → DocumentationFlow (2.1)
DomainId: "software-development"
SCL: NOT emitted
All gap IDs: forwarded as CapabilityGapWarnings

One "stakeholders" token changes the entire routing outcome.

---

## B37-0C-058-005 — Knowledge-edge: one-token addition (service)

**Stress test + "and deploy the notification service" added:**
All of 058-003 input → IMPLEMENT fires → NOT knowledge_edge → BackendServiceFlow41 (4.1)
DomainId: "software-development"
SCL: NOT emitted

One "service" token changes routing from knowledge_edge to 4.1.
