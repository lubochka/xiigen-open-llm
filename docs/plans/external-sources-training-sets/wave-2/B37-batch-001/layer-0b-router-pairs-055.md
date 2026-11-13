# Layer 0-B — Router Pairs (055)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-055-001 — "agile transformation" + "benefits realization" + "KPI" combined routing

A: "Manage agile transformation, track benefits realization, monitor KPI." → knowledge_edge | PM | SCL
B: "Document agile transformation benefits and KPIs for stakeholders." → DocumentationFlow (2.1)
C: "Build agile transformation KPI dashboard service." → BackendServiceFlow41 (4.1)
D: "Review agile transformation benefits and KPI performance." → QA sub-phase

---

## B37-0B-055-002 — WBS + all-round combined routing

A: "Maintain work breakdown structure and coordinate programme." → ArchitectureFlow31 (3.1) — ARCHITECTURE fires on "structure"
B: "Create work breakdown structure and build tracking service." → ArchitectureFlow31 (3.1) + BackendServiceFlow41 (4.1)
C: "Maintain WBS and coordinate programme." → knowledge_edge — WBS abbreviation = gap

---

## B37-0B-055-003 — "handover document" + PM managing vocabulary combined

A: "Prepare handover document and manage sign-off process." → DocumentationFlow (2.1)
B: "Prepare handover document for stakeholders and manage sign-off." → DocumentationFlow (2.1) — ONE DOCUMENT
C: "Manage handover and sign-off process." → knowledge_edge — "handover" alone = gap; "sign-off" alone = gap

---

## B37-0B-055-004 — v6 corruptions for Round 4b vocabulary

**v6-01:**
- "Manage waterfall delivery" → WaterfallFlow — invented
- "Coordinate SAFe ceremonies" → SAFeFlow — invented
- "Track benefits realization" → BenefitsFlow — invented

**v6-02:**
- "Portfolio update for stakeholders" → DOCUMENT suppressed — incorrect
- "Benefits realization report for stakeholders" → DOCUMENT suppressed — incorrect

**v6-03:**
- "Design value stream architecture" → DomainId "project-management" — incorrect
- "Build KPI dashboard" → DomainId "project-management" — incorrect

---

## B37-0B-055-005 — B37 router final summary (all 59 router files 002-060)

**Universal routing principle confirmed across all B37 rounds:**
The router is purely predicate-driven. Project managing vocabulary — regardless of round, terminology, or specificity — never determines the route. The predicate scan output is the sole routing input. Zero predicates → knowledge_edge. One or more predicates → corresponding flow(s).
