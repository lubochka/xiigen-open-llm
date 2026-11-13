# Layer 0-A — Compiler Pairs (052)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-052-001 — "waterfall" + "agile transformation" combined — zero predicates

**Input:** "Transition from waterfall to agile transformation across all programmes."

Token scan:
- "Transition" → not in predicate lexicon → gap
- "waterfall" → gap
- "agile transformation" → gap
- "programmes" → gap

Predicates: []
DomainId: "project-management"
SCL: emitted

---

## B37-0A-052-002 — "project closure" + "benefits realization" combined

**Input:** "Run project closure activities and begin tracking benefits realization."

Token scan:
- "Run" → gap; "project closure" → gap; "activities" → gap
- "begin" → gap; "tracking" → NOT TEST → gap; "benefits realization" → gap

Predicates: []
DomainId: "project-management"
SCL: emitted

---

## B37-0A-052-003 — "KPI dashboard" fires IMPLEMENT+DOCUMENT combo

**Input:** "Build and document the KPI dashboard for stakeholders."

Token scan:
- "Build" → IMPLEMENT fires
- "document" → DOCUMENT fires
- "KPI dashboard" → gap
- "stakeholders" → DOCUMENT fires (already fired)

Predicates: ["IMPLEMENT", "DOCUMENT"]
DomainId: "software-development"
GapIds: manage-delivery-001 forwarded

---

## B37-0A-052-004 — "work breakdown structure" + IMPLEMENT combo

**Input:** "Create the work breakdown structure."

Token scan:
- "Create" → IMPLEMENT fires
- "work breakdown structure" → "structure" fires ARCHITECTURE

Predicates: ["IMPLEMENT", "ARCHITECTURE"]
DomainId: "software-development"

---

## B37-0A-052-005 — Complete B37 all-round DomainId certification reference

**DomainId "project-management" — ALL rounds zero-predicate vocabulary:**

Round 1 (002-012): track, coordinate, blockers, RAG-status, RAID-log, progress, status-report, escalation, accountability, follow-up, action-items, manage, delivery, workstream
Round 2 (013-024): risk log, change request, project charter, decision log, lessons learned, RACI, sign-off, programme board, steering committee, ownership
Round 3 (025-036): status-report, progress-update, milestone-tracking, gantt-chart, resource-allocation, capacity-planning, dependency-mapping, critical-path, budget-tracking, vendor-management, SLA-tracking
Round 4a (037-048): escalation-process, communication-plan, project-kickoff, steering-committee, change-control-board, scope-creep, risk-mitigation, quality-gate, sign-off-process, handover, post-mortem
Round 4b (049-060): waterfall, agile-transformation, SAFe, program-increment, portfolio-management, value-stream, flow-state, project-closure, benefits-realization, KPI-monitoring

**Exception — always fires ARCHITECTURE:**
work breakdown structure (on "structure" token)
