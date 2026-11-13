# Layer 2 — Capability Gap Extended Pairs (009)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B36-L2-009-001 — milestone dependency mapping: knowledge_edge (no predicates fire)

**Input:** "Map the milestone dependencies for the Q3 roadmap and identify critical path items."

**Stage 1 — Compiler:**
- map: no predicate
- milestone: GAP → milestone-001
- dependencies: no predicate
- Q3: no predicate
- roadmap: GAP → roadmap-001
- identify: no predicate
- critical: no predicate
- path: no predicate
- items: no predicate

**Predicate output:** []

**Stage 2 — Router:**
- KNOWLEDGE_EDGE
- CapabilityGapWarnings: milestone-001, roadmap-001

**Stage 4 — Orchestrator:**
- knowledgeEdge: true
- domainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: milestone-001, roadmap-001

---

## B36-L2-009-002 — "identify" is not a predicate trigger

"Identify" is not in any lexicon. It does not fire IMPLEMENT, DOCUMENT, QA, or ARCHITECTURE. In "identify the critical path," it is neutral. The subject ("critical path") is also not in any lexicon.

---

## B36-L2-009-003 — milestone dependency gap: what it represents

gap-project-planning-milestone-001 in dependency mapping context represents:
- Cannot map dependencies between milestones
- Cannot calculate critical path from milestone dependencies
- Cannot identify blocking milestones or parallel milestone tracks
- Cannot forecast delivery dates from dependency chains

Dependency mapping and critical path analysis are project management disciplines requiring specialized planning tools.

---

## B36-L2-009-004 — compound gap: sprint-roadmap in milestone context

When sprint, roadmap, and milestone all appear together:

**Input:** "Plan the sprint roadmap with milestone checkpoints."
**Gap IDs:**
- gap-project-planning-sprint-001
- gap-project-planning-roadmap-001
- gap-project-planning-milestone-001
- gap-project-planning-sprint-roadmap-001 (compound: sprint + roadmap together)

The compound ID gap-project-planning-sprint-roadmap-001 is emitted when sprint and roadmap co-occur in a zero-predicate request, indicating the user wants sprint-to-roadmap alignment — a specific integrated planning capability.
