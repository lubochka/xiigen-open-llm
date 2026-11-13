# Layer 0-B — Router Pairs (044)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-044-001 — "escalation process" routing stress test

**Five inputs, five routes:**

A: "Manage escalation process." → knowledge_edge | PM
B: "Define escalation criteria." → 2.1 | SD
C: "Design escalation architecture." → 3.1 | SD
D: "Implement escalation service." → 4.1 | SD
E: "Review escalation process." → QA | SD

Pattern confirmed: escalation process = gap; route determined by co-occurring predicate token.

---

## B37-0B-044-002 — "project kickoff" + "communication plan" combined routing

**Case A:** "Facilitate project kickoff and prepare communication plan." → knowledge_edge | PM | SCL
**Case B:** "Facilitate project kickoff and document communication plan for stakeholders." → DocumentationFlow (2.1) | SD
**Case C:** "Build project kickoff communication platform." → BackendServiceFlow41 (4.1) | SD

---

## B37-0B-044-003 — "scope creep" + "risk mitigation" combined routing

**Case A:** "Monitor scope creep and manage risk mitigation." → knowledge_edge | PM | SCL
**Case B:** "Define scope boundaries and document risk mitigation." → DocumentationFlow (2.1) | SD
**Case C:** "Implement risk mitigation service and control scope." → BackendServiceFlow41 (4.1) | SD

---

## B37-0B-044-004 — "quality gate" + "sign-off" combined routing

**Case A:** "Enforce quality gates and manage sign-off." → knowledge_edge | PM | SCL
**Case B:** "Review quality gate and validate sign-off criteria." → QA sub-phase | SD
**Case C:** "Document quality gate and sign-off criteria for stakeholders." → DocumentationFlow (2.1) | SD

---

## B37-0B-044-005 — "post-mortem" + "change control board" combined routing

**Case A:** "Run post-mortem and coordinate change control board." → knowledge_edge | PM | SCL
**Case B:** "Document post-mortem and CCB findings for stakeholders." → DocumentationFlow (2.1) | SD
**Case C:** "Review post-mortem and CCB outcomes." → QA sub-phase | SD

All six pairs confirm: managing vocabulary never determines route — only predicate tokens route.
