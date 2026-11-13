# Layer 0-C — Knowledge-Edge Pairs (045)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-045-001 — Knowledge-edge: "escalation process" boundary matrix

| Input | knowledge_edge? | Why |
|-------|----------------|-----|
| "Run escalation process." | Yes | all gap |
| "Define escalation process." | No | "define" → DOCUMENT |
| "Document escalation for stakeholders." | No | "document" + "stakeholders" → DOCUMENT |
| "Design escalation architecture." | No | "design" → ARCHITECTURE |
| "Implement escalation service." | No | "service" → IMPLEMENT |

---

## B37-0C-045-002 — Knowledge-edge: "steering committee" boundary matrix

| Input | knowledge_edge? | Why |
|-------|----------------|-----|
| "Coordinate steering committee." | Yes | all gap |
| "Escalate to steering committee." | Yes | all gap |
| "Escalate to steering committee and stakeholders." | No | "stakeholders" → DOCUMENT |
| "Design steering committee governance structure." | No | "design" + "structure" → ARCHITECTURE |
| "Build steering committee reporting service." | No | "service" → IMPLEMENT |

---

## B37-0C-045-003 — Knowledge-edge: "waterfall" boundary matrix

| Input | knowledge_edge? | Why |
|-------|----------------|-----|
| "Manage waterfall delivery." | Yes | all gap |
| "Design waterfall phase structure." | No | "structure" → ARCHITECTURE |
| "Document waterfall phases for stakeholders." | No | "document" + "stakeholders" → DOCUMENT |
| "Build waterfall tracking service." | No | "service" → IMPLEMENT |
| "Review waterfall outcomes." | No | "review" → QA |

---

## B37-0C-045-004 — Knowledge-edge: "benefits realization" boundary matrix

| Input | knowledge_edge? | Why |
|-------|----------------|-----|
| "Track benefits realization." | Yes | all gap |
| "Monitor benefits realization." | Yes | all gap |
| "Define benefits realization criteria." | No | "define" + "criteria" → DOCUMENT |
| "Build benefits tracker service." | No | "service" → IMPLEMENT |
| "Review benefits realization." | No | "review" → QA |

---

## B37-0C-045-005 — Knowledge-edge: "KPI" boundary matrix

| Input | knowledge_edge? | Why |
|-------|----------------|-----|
| "Monitor KPI metrics." | Yes | all gap |
| "Track KPI performance." | Yes | all gap |
| "Report KPI status." | Yes | "report" ≠ DOCUMENT (Rule 3) |
| "Build KPI dashboard." | No | "build" → IMPLEMENT |
| "Design KPI architecture." | No | "design" → ARCHITECTURE |
