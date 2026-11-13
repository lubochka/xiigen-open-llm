# Layer 0-C — Knowledge-Edge Pairs (052)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-052-001 — Knowledge-edge: "agile transformation" + "waterfall" + "SAFe" combined

1. "Transition waterfall to agile transformation with SAFe." → knowledge_edge
2. "Coordinate waterfall, agile transformation, and SAFe across teams." → knowledge_edge | coordinate-001
3. "Design waterfall-to-SAFe agile transformation architecture." → NOT knowledge_edge (ARCHITECTURE fires)
4. "Implement waterfall-to-agile transformation platform." → NOT knowledge_edge (IMPLEMENT fires)

---

## B37-0C-052-002 — Knowledge-edge: "program increment" + "portfolio management" + "value stream"

1. "Run PI planning, manage portfolio management, map value streams." → knowledge_edge
2. "Coordinate PI, portfolio, and value stream delivery." → knowledge_edge | coordinate-001
3. "Design PI, portfolio, and value stream architecture." → NOT knowledge_edge (ARCHITECTURE fires)
4. "Document PI, portfolio, and value stream for stakeholders." → NOT knowledge_edge (DOCUMENT fires)

---

## B37-0C-052-003 — Knowledge-edge: "flow state" + "KPI" + "benefits realization" + "project closure"

1. "Monitor flow state, track KPI, manage benefits realization, run project closure." → knowledge_edge
2. "Coordinate flow state, KPI, and benefits realization for project closure." → knowledge_edge | coordinate-001
3. "Build flow state, KPI, and benefits realization dashboard service." → NOT knowledge_edge (IMPLEMENT fires)
4. "Review flow state, KPI, and benefits realization." → NOT knowledge_edge (QA fires)

---

## B37-0C-052-004 — Knowledge-edge: escalation + CCB + steering committee + post-mortem

1. "Manage escalation, coordinate CCB, present to steering committee, run post-mortem." → knowledge_edge
2. "Escalate blockers to CCB, steering committee." → knowledge_edge | escalation-001 + blockers-001
3. "Escalate to CCB, steering committee, and stakeholders." → NOT knowledge_edge (stakeholders fires DOCUMENT)
4. "Document escalation, CCB, and post-mortem for stakeholders." → NOT knowledge_edge (DOCUMENT fires)

---

## B37-0C-052-005 — Knowledge-edge: complete Layer 0-C summary (all 59 files 002-060)

59 knowledge-edge files confirm one universal principle:
The knowledge_edge handler is B37's output for all zero-predicate PM managing requests.
SCL is emitted once per request, regardless of vocabulary breadth.
DomainId "project-management" accompanies every knowledge_edge response.
Gap IDs are forwarded but do not affect routing.
The only "self-triggering" exception in all B37 rounds: "work breakdown structure" → ARCHITECTURE.
