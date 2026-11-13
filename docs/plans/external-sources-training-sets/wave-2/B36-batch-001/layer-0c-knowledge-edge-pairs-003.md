# Layer 0-C — Knowledge-Edge Pairs (003)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B36-0C-003-001 — "capacity" gap in team context vs ARCHITECTURE system capacity

**The boundary rule (B36 Rule 16):**

"capacity" in team planning context → NOT ARCHITECTURE → GAP (capacity-001)
"capacity" in system design context → ARCHITECTURE fires

**Context distinction:**

| Sentence | "capacity" context | Predicate | Reasoning |
|----------|--------------------|-----------|-----------|
| "Plan the team capacity for the sprint" | team/planning | none (GAP) | Human resource allocation |
| "Design the system capacity model" | system design | ARCHITECTURE | System architecture term |
| "Capture capacity for the backlog" | team/planning | none (GAP) | Planning artifact |
| "Architect the database capacity" | system design | ARCHITECTURE | Technical capacity design |

---

## B36-0C-003-002 — how the compiler disambiguates "capacity"

The compiler uses co-occurring tokens as context signals:

**GAP signals (team context):**
- Co-occurs with: team, sprint, backlog, iteration, standup, planning, allocation, headcount, story points
- None of these co-occurring tokens are in ARCHITECTURE lexicon
- Result: "capacity" → GAP

**ARCHITECTURE signals (system context):**
- Co-occurs with: design, architect, system, database, infrastructure, load, throughput, scaling
- "design" or "architect" tokens already fire ARCHITECTURE independently
- When capacity co-occurs with these terms, ARCHITECTURE fires via the co-occurring token (not via "capacity" itself)

**Key insight:** "capacity" alone never fires ARCHITECTURE. If ARCHITECTURE fires in a sentence with "capacity," it fired because of "design" or "architect" — not because of "capacity."

---

## B36-0C-003-003 — knowledge-edge trace for team capacity

**Input:** "Calculate the team capacity for the next sprint iteration."

**Token analysis:**
- calculate: no predicate
- team: no predicate
- capacity: team context → GAP (capacity-001)
- sprint: GAP (sprint-001)
- iteration: GAP (iteration-001)

**KNOWLEDGE_EDGE state:**
- predicates: []
- FinalHumanResult: null
- DomainId: "project-management"
- CapabilityGapWarnings: capacity-001, sprint-001, iteration-001
- recommended_action: SCHEDULE_CAPABILITY_LEARNING

---

## B36-0C-003-004 — "design the capacity plan" fires ARCHITECTURE

**Input:** "Design the capacity plan for the sprint team."

**Token analysis:**
- design: ARCHITECTURE fires (Rule 4)
- capacity: team context → would be GAP, but ARCHITECTURE already fired via "design"
- plan: no predicate
- sprint: GAP
- team: no predicate

**Result:**
- predicates: ["ARCHITECTURE"]
- KNOWLEDGE_EDGE: false
- DomainId: "software-development"
- CapabilityGapWarnings: capacity-001 (capacity intent is planning), sprint-001

**Teaching point:** "design" fires ARCHITECTURE even though the sentence is about team capacity planning. The capacity planning intent is noted via gap warning; ARCHITECTURE routing proceeds because "design" fired.
