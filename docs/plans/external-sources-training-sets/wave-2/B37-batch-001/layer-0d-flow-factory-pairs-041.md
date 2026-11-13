# Layer 0-D — Flow Factory Pairs (041)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-041-001 — Factory: "scope creep" + "CCB" + "waterfall" + "agile" combined

A: "Monitor scope creep, coordinate CCB, manage waterfall, track agile transformation." → [] → no factory
B: "Document scope, CCB, and agile for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Build scope, CCB, and waterfall tracking service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
D: "Design scope management architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)

---

## B37-0D-041-002 — Factory: "risk mitigation" + "quality gate" + "SAFe" combined

A: "Manage risk mitigation, enforce quality gates, coordinate SAFe." → [] → no factory
B: "Review risk mitigation and quality gate effectiveness in SAFe context." → [QA] → QA sub-phase
C: "Build risk mitigation and quality gate service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)

---

## B37-0D-041-003 — Factory: "communication plan" + "project kickoff" + "PI" combined

A: "Prepare communication plan, facilitate kickoff, run PI planning." → [] → no factory
B: "Document communication plan, kickoff, and PI for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Build communication, kickoff, and PI tracking service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)

---

## B37-0D-041-004 — Factory: "sign-off" + "project closure" + "benefits realization" combined

A: "Manage sign-off, run project closure, track benefits realization." → [] → no factory
B: "Document sign-off, closure, and benefits for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Build sign-off, closure, and benefits tracker service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)

---

## B37-0D-041-005 — Factory: "post-mortem" + "lessons learned" + "project closure"

A: "Run post-mortem, coordinate lessons learned, manage project closure." → [] → no factory
B: "Document post-mortem, lessons learned, and closure for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Review post-mortem and lessons learned outcomes." → [QA] → QA sub-phase
