# Layer 0-B — Router Pairs (006)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: router

---

## B36-0B-006-001 — [] → knowledge_edge (standup/capacity gap)

**Input predicate list:** `[]`
**Request origin:** "Run the daily standup and capture team capacity for the sprint planning session."

**Router decision:**
- predicates: [] → KNOWLEDGE_EDGE path
- No sub-phase selected
- CapabilityGapWarnings: standup-001, capacity-001, sprint-001
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- FinalHumanResult: null

**What the router does NOT do:**
- Does NOT route to QA (no "review" token)
- Does NOT route to DOCUMENT (no "document", "define", "specify", "requirements")
- Does NOT route to IMPLEMENT (no "implement", "build", "create", "write", "feature")
- Does NOT route to ARCHITECTURE (no "design", "architect", "structure")

---

## B36-0B-006-002 — pure gap: epic/ticket/iteration

**Input predicate list:** `[]`
**Request origin:** "Break down Epic E-7 into tickets and assign them to the iteration."

**Router decision:**
- predicates: [] → KNOWLEDGE_EDGE
- CapabilityGapWarnings: epic-001, ticket-001, iteration-001
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

---

## B36-0B-006-003 — pure gap: burndown/velocity/sprint

**Input predicate list:** `[]`
**Request origin:** "Track the burndown and velocity targets across the sprint."

**Router decision:**
- predicates: [] → KNOWLEDGE_EDGE
- CapabilityGapWarnings: burndown-001, velocity-001, sprint-001
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

---

## B36-0B-006-004 — router path decision table for B36

| Predicate list | Router path | DomainId | Notes |
|----------------|-------------|----------|-------|
| [] | KNOWLEDGE_EDGE | project-management | All gap vocab |
| [QA] | QA sub-phase | software-development | "review" or "approve" fired |
| [IMPLEMENT] | Phase 4.1 | software-development | Code-level verb fired |
| [DOCUMENT] | Phase 2.1 | software-development | DOCUMENT fired |
| [ARCHITECTURE] | Phase 3.1 | software-development | ARCHITECTURE fired |
| [IMPLEMENT, DOCUMENT] | Phase 4.1 + 2.1 | software-development | Parallel eligible |
| [IMPLEMENT, QA] | Phase 4.1 + QA | software-development | QA last |
| [DOCUMENT, QA] | Phase 2.1 + QA | software-development | QA last |
| [IMPLEMENT, DOCUMENT, QA] | Phase 4.1 + 2.1 + QA | software-development | QA last |
| [ARCHITECTURE, DOCUMENT] | Phase 3.1 + 2.1 | software-development | Both documentation |

**Key invariant:** Gap vocabulary in the request never appears in this routing table. It only appears in CapabilityGapWarnings.
