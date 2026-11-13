# Layer 0-B — Router Pairs (038)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-038-001 — "communication plan" routing: DOCUMENT fires when documented

**Case A (zero predicates):**
Input: "Prepare the communication plan."
Route: knowledge_edge
DomainId: "project-management"
SCL: emitted

**Case B (DOCUMENT fires):**
Input: "Document the communication plan for stakeholders."
Predicates: ["DOCUMENT"]
Route: DocumentationFlow (2.1)
DomainId: "software-development"
SCL: NOT emitted

---

## B37-0B-038-002 — "steering committee" routing

**Case A:** "Present to the steering committee." → knowledge_edge | project-management
**Case B:** "Present to the steering committee and stakeholders." → DocumentationFlow (2.1) | software-development
**Case C:** "Define steering committee governance structure." → ArchitectureFlow31 (3.1) + DocumentationFlow (2.1) | software-development

---

## B37-0B-038-003 — "change control board" routing

**Case A:** "Submit to the change control board." → knowledge_edge
**Case B:** "Document CCB criteria for stakeholders." → DocumentationFlow (2.1)
**Case C:** "Design the CCB governance architecture." → ArchitectureFlow31 (3.1)
**Case D:** "Build the CCB tracking service." → BackendServiceFlow41 (4.1)

---

## B37-0B-038-004 — "scope creep" routing

**Case A:** "Monitor scope creep." → knowledge_edge
**Case B:** "Define scope to prevent scope creep." → DocumentationFlow (2.1)
**Case C:** "Implement scope control service." → BackendServiceFlow41 (4.1)
**Case D:** "Review scope creep." → QA sub-phase

---

## B37-0B-038-005 — "risk mitigation" routing

**Case A:** "Run risk mitigation." → knowledge_edge
**Case B:** "Document risk mitigation plan." → DocumentationFlow (2.1)
**Case C:** "Implement risk mitigation service." → BackendServiceFlow41 (4.1)
**Case D:** "Design risk mitigation architecture." → ArchitectureFlow31 (3.1)
**Case E:** "Review risk mitigation." → QA sub-phase

All five: different predicates → different routes. "risk mitigation" itself never determines the route.
