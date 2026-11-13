# Layer 0-B — Router Pairs (039)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-039-001 — "quality gate" routing

**Case A:** "Enforce quality gate." → knowledge_edge | project-management | SCL
**Case B:** "Review quality gate." → QA sub-phase | software-development
**Case C:** "Define quality gate criteria." → DocumentationFlow (2.1) | software-development
**Case D:** "Build quality gate service." → BackendServiceFlow41 (4.1) | software-development
**Case E:** "Review quality gate criteria for stakeholders." → QA sub-phase + DocumentationFlow? → ["QA", "DOCUMENT"] → both routes | software-development

---

## B37-0B-039-002 — "sign-off process" routing

**Case A:** "Manage sign-off." → knowledge_edge | project-management
**Case B:** "Document sign-off process for stakeholders." → DocumentationFlow (2.1)
**Case C:** "Create sign-off document." → BackendServiceFlow41 (4.1) + DocumentationFlow (2.1) — "create" IMPLEMENT + "document" DOCUMENT

---

## B37-0B-039-003 — "handover document" routing

**Case A:** "Manage handover." → knowledge_edge | project-management
**Case B:** "Prepare handover document." → DocumentationFlow (2.1) — "document" token fires DOCUMENT
**Case C:** "Write the handover document for stakeholders." → DocumentationFlow (2.1) — "write" + "document" + "stakeholders" → ONE DOCUMENT

---

## B37-0B-039-004 — "post-mortem" routing

**Case A:** "Run post-mortem." → knowledge_edge | project-management | SCL
**Case B:** "Document post-mortem for stakeholders." → DocumentationFlow (2.1)
**Case C:** "Review post-mortem findings." → QA sub-phase
**Case D:** "Build post-mortem tracking service." → BackendServiceFlow41 (4.1)

---

## B37-0B-039-005 — PM+IMPLEMENT combo routing rule

**Rule:** When IMPLEMENT fires in a PM managing context:

- Route: BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- SCL: NOT emitted
- Gap warnings for managing vocabulary: forwarded alongside 4.1 routing
- No PM-specific factory is created or implied

Examples:
- "Coordinate delivery of the tracking service" → 4.1 + gap warnings
- "Manage the implementation of the reporting module" → 4.1 + gap warnings
- "Implement SAFe tooling" → 4.1 + gap warnings (SAFe = gap warning)
