# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B36-L1-011-001 — Canonical B36 request patterns and orchestration outcomes

**Pattern 1: Pure gap request**

Request: "Plan the sprint roadmap and set the milestone dates."
Predicates: []
Orchestration: KNOWLEDGE_EDGE, TransitionType: unknown, DomainId: project-management, SCHEDULE_CAPABILITY_LEARNING emitted

**Pattern 2: Review fires QA in planning sentence**

Request: "Review the sprint velocity."
Predicates: ["QA"]
Orchestration: QA sub-phase, TransitionType: no_contribution, DomainId: software-development

**Pattern 3: Implementation with planning context**

Request: "Implement the sprint authentication feature."
Predicates: ["IMPLEMENT"]
Orchestration: Phase 4.1, TransitionType: feature_addition, DomainId: software-development

**Pattern 4: Documentation with planning context**

Request: "Document the sprint milestone requirements."
Predicates: ["DOCUMENT"] (document + requirements fire)
Orchestration: Phase 2.1, TransitionType: documentation, DomainId: software-development

---

## B36-L1-011-002 — Canonical B36 request patterns (continued)

**Pattern 5: Architecture in planning sentence**

Request: "Design the sprint workflow architecture."
Predicates: ["ARCHITECTURE"]
Orchestration: Phase 3.1, TransitionType: documentation, DomainId: software-development

**Pattern 6: Mixed IMPLEMENT + DOCUMENT**

Request: "Write the sprint service and document the backlog requirements."
Predicates: ["IMPLEMENT", "DOCUMENT"] ("write" fires IMPLEMENT; "document" + "requirements" fire DOCUMENT)
Orchestration: Phase 4.1 + 2.1 parallel, TransitionType: feature_addition

**Pattern 7: Triple predicate**

Request: "Build the velocity tracker, document the sprint requirements, review the burndown."
Predicates: ["IMPLEMENT", "DOCUMENT", "QA"]
Orchestration: 4.1 + 2.1 parallel → QA last, TransitionType: feature_addition

---

## B36-L1-011-003 — CapabilityGapWarnings in all canonical patterns

Every canonical pattern that involves B36 gap vocabulary includes CapabilityGapWarnings. The warnings are present in all patterns (1 through 7). Their presence is independent of the orchestration path taken.

**Pattern 1:** Warnings are the primary output (knowledge_edge — warnings + SCHEDULE_CAPABILITY_LEARNING)
**Patterns 2-7:** Warnings are supplementary (predicate routing is primary; warnings are metadata)

---

## B36-L1-011-004 — Orchestration decision summary

| Pattern | Knowledge Edge | TransitionType | DomainId | SCHEDULE_CAPABILITY_LEARNING |
|---------|---------------|---------------|----------|------------------------------|
| Pure gap | YES | unknown | project-management | YES |
| QA in planning | NO | no_contribution | software-development | NO |
| IMPLEMENT in planning | NO | feature_addition | software-development | NO |
| DOCUMENT in planning | NO | documentation | software-development | NO |
| ARCHITECTURE in planning | NO | documentation | software-development | NO |
| IMPLEMENT+DOCUMENT | NO | feature_addition | software-development | NO |
| IMPLEMENT+DOCUMENT+QA | NO | feature_addition | software-development | NO |
