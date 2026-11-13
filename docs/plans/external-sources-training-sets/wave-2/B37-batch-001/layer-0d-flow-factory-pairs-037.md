# Layer 0-D — Flow Factory Pairs (037)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-037-001 — Factory: "escalation process" + "communication plan" combined

A: "Run escalation process, prepare communication plan." → [] → no factory
B: "Document escalation and communication plan for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Build escalation and communication platform service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)

---

## B37-0D-037-002 — Factory: "project kickoff" + "steering committee" + "CCB" combined

A: "Facilitate project kickoff, coordinate steering committee, submit to CCB." → [] → no factory
B: "Document kickoff, steering committee, and CCB decisions for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)

---

## B37-0D-037-003 — Factory: "scope creep" + "risk mitigation" combined

A: "Monitor scope creep and run risk mitigation." → [] → no factory
B: "Document scope and risk mitigation for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Build scope and risk mitigation tracking service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
D: "Design scope and risk mitigation architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)

---

## B37-0D-037-004 — Factory: "quality gate" + "sign-off process" combined

A: "Enforce quality gates and manage sign-off." → [] → no factory
B: "Review quality gates and validate sign-off." → [QA] → QA sub-phase
C: "Document quality gate and sign-off criteria for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)

---

## B37-0D-037-005 — Factory: "post-mortem" + "handover document" combined

A: "Run post-mortem and manage handover." → [] → no factory (handover alone = gap)
B: "Run post-mortem and prepare handover document." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Build post-mortem and handover tracking service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
