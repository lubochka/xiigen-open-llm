# Layer 0-C — Knowledge-Edge Pairs (040)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-040-001 — Knowledge-edge: "flow state" + "value stream" + "SAFe" combined

1. "Monitor flow state, map value streams, coordinate SAFe." → knowledge_edge | manage-delivery-001, coordinate-001
2. "Track flow state and value stream metrics in SAFe context." → knowledge_edge | track-001
3. "Design flow state and value stream architecture." → NOT knowledge_edge (ARCHITECTURE fires)
4. "Build flow state and value stream monitoring service." → NOT knowledge_edge (IMPLEMENT fires)

---

## B37-0C-040-002 — Knowledge-edge: "agile transformation" + "benefits realization" combined

1. "Manage agile transformation and track benefits realization." → knowledge_edge | manage-delivery-001, track-001
2. "Coordinate agile transformation benefits activities." → knowledge_edge | coordinate-001
3. "Document agile transformation benefits for stakeholders." → NOT knowledge_edge (DOCUMENT fires)
4. "Implement agile transformation benefits platform." → NOT knowledge_edge (IMPLEMENT fires)

---

## B37-0C-040-003 — Knowledge-edge: "escalation process" + "steering committee" + "CCB"

1. "Run escalation, coordinate steering committee, manage CCB." → knowledge_edge | escalation-001, coordinate-001, manage-delivery-001
2. "Escalate via steering committee and CCB to programme board." → knowledge_edge | escalation-001
3. "Document escalation, steering committee, and CCB decisions for stakeholders." → NOT knowledge_edge
4. "Define escalation criteria for steering committee and CCB." → NOT knowledge_edge (DOCUMENT fires)

---

## B37-0C-040-004 — Knowledge-edge: "post-mortem" + "lessons learned" + "project closure"

1. "Run post-mortem, coordinate lessons learned, manage project closure." → knowledge_edge
2. "Track post-mortem and lessons learned action items." → knowledge_edge | action-items-001
3. "Document post-mortem and lessons learned for stakeholders." → NOT knowledge_edge
4. "Review post-mortem and lessons learned." → NOT knowledge_edge (QA fires)

---

## B37-0C-040-005 — Knowledge-edge: all-rounds maximum gap ID accumulation

**Input:** "Track milestones, coordinate blockers, manage risk log, escalate issues, run escalation process, coordinate CCB, monitor scope creep, manage risk mitigation, enforce quality gates, manage sign-off, run post-mortem, manage waterfall, coordinate agile transformation, run SAFe PI planning, manage portfolio, map value streams, monitor flow state, manage project closure, track benefits realization, monitor KPI."

Result: knowledge_edge | DomainId "project-management" | SCL emitted once | 20+ gap IDs forwarded

This is the maximum-accumulation case: all vocabulary from all rounds, zero predicates, one SCL.
