# Layer 2 — Capability Gap Extended Pairs (005)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B36-L2-005-001 — velocity + retrospective: full gap path

**Input:** "Analyze the team velocity and plan the retrospective agenda for this sprint."

**Stage 1 — Compiler:**
- analyze: no predicate
- team: no predicate
- velocity: GAP → velocity-001
- plan: no predicate (Rule 17)
- retrospective: GAP → retrospective-001
- agenda: no predicate
- sprint: GAP → sprint-001

**Predicate output:** []

**Stage 2 — Router:**
- KNOWLEDGE_EDGE path
- CapabilityGapWarnings: velocity-001, retrospective-001, sprint-001

**Stage 4 — Orchestrator:**
- SCHEDULE_CAPABILITY_LEARNING emitted with all three gap IDs
- DomainId: "project-management"
- FinalHumanResult: null

---

## B36-L2-005-002 — note: "review" would fire QA (contrast case)

**Input (QA fires):** "Review the team velocity and plan the retrospective agenda for this sprint."
- "review" → QA fires
- Predicate: ["QA"]
- KNOWLEDGE_EDGE: false
- CapabilityGapWarnings: velocity-001, retrospective-001, sprint-001

The single token change ("analyze" → "review") converts the request from KNOWLEDGE_EDGE to QA routing. Both sentences are about velocity and retrospective. The routing difference is entirely due to the presence of "review."

---

## B36-L2-005-003 — velocity-retrospective compound gap

When both velocity and retrospective appear in a zero-predicate request, the compound gap ID gap-project-planning-velocity-retrospective-001 is emitted.

**Gap IDs for "Analyze the team velocity and plan the retrospective":**
- gap-project-planning-velocity-001 (individual)
- gap-project-planning-retrospective-001 (individual)
- gap-project-planning-sprint-001 (individual)
- gap-project-planning-velocity-retrospective-001 (compound — velocity + retrospective together)

---

## B36-L2-005-004 — what the velocity-retrospective gap represents

gap-project-planning-velocity-retrospective-001 represents the combined capability of:
- Analyzing velocity trends across sprints
- Connecting velocity trends to retrospective action items
- Providing recommendations for velocity improvement based on retrospective patterns

This is a planning analysis capability that goes beyond what either velocity tracking or retrospective facilitation alone would require. The compound gap signals that the user wants integrated sprint performance analysis — not currently available.
