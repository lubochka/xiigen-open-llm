# Layer 2 — Capability Gap Extended Pairs (011)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B36-L2-011-001 — "document the backlog requirements" → DOCUMENT fires; backlog is gap warning

**Input:** "Document the backlog requirements and specify the acceptance criteria for each sprint item."

**Stage 1 — Compiler:**
- document: DOCUMENT fires
- backlog: GAP → backlog-001
- requirements: DOCUMENT fires
- specify: DOCUMENT fires
- acceptance: no predicate alone
- criteria: DOCUMENT fires (Rule 21)
- sprint: GAP → sprint-001
- item: no predicate

**Predicate output:** ["DOCUMENT"]
**CapabilityGapWarnings:** backlog-001, sprint-001

**Stage 2 — Router:**
- DOCUMENT → Phase 2.1
- DomainId: "software-development"

**Stage 4 — Orchestrator:**
- transitionType: documentation
- phaseId: "2.1"
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted
- capabilityGapWarnings: backlog-001, sprint-001

---

## B36-L2-011-002 — backlog-estimation compound: mixed-signal gap

gap-project-planning-backlog-estimation is a mixed-signal gap ID used when backlog and estimation co-occur in a request that also fires a predicate.

**Example:**
Input: "Document the backlog estimation methodology for the sprint."
- DOCUMENT fires: document
- Backlog: gap → backlog-001
- Estimation: gap → estimation-001
- Sprint: gap → sprint-001
- Mixed-signal gap: gap-project-planning-backlog-estimation (compound mixed-signal)

---

## B36-L2-011-003 — "specify the backlog items": DOCUMENT fires; backlog is warning

"Specify" is in the DOCUMENT lexicon (Rule 5). In "specify the backlog items," DOCUMENT fires on "specify." The backlog context is a CapabilityGapWarning.

**Routing:** Phase 2.1 (documentation)
**Gap warning:** backlog-001
**DomainId:** "software-development"

The user may have wanted to define backlog items in a project management tool — but the orchestrator routes to documentation because "specify" fired DOCUMENT. The backlog context is noted.

---

## B36-L2-011-004 — mixed-signal taxonomy for B36

| Predicate fired | Gap vocabulary | Mixed-signal gap ID used |
|----------------|----------------|-------------------------|
| IMPLEMENT | sprint | gap-project-planning-sprint-not-captured |
| IMPLEMENT | backlog | gap-project-planning-backlog-001 (standard) |
| IMPLEMENT | epic | gap-project-planning-epic-001 (standard) |
| DOCUMENT | backlog + estimation | gap-project-planning-backlog-estimation |
| DOCUMENT | story (false positive) | gap-project-planning-story-point-001 |
| QA | sprint | gap-project-planning-sprint-001 (standard) |
| QA | velocity | gap-project-planning-velocity-001 (standard) |

Mixed-signal gap IDs (without -001 sequence) are used when the gap represents a mismatch between user intent and routing outcome. Standard gap IDs (with -001 sequence) are used when the gap is simply a noted limitation alongside a successful routing.
