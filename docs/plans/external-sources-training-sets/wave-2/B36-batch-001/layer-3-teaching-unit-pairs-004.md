# Layer 3 — Teaching Unit Pairs (004)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B36-L3-004-001 — "capacity" in team context is gap; system design context → ARCHITECTURE

**Teaching objective:** Model learns to distinguish team capacity (gap) from system capacity (ARCHITECTURE trigger).

**The distinction rule (B36 Rule 16):**
"capacity" in team planning context → NOT ARCHITECTURE → GAP (capacity-001)
"capacity" in system design context → NOT triggered by "capacity" itself → ARCHITECTURE fires because "design" or "architect" is also present

---

## B36-L3-004-002 — Teaching pairs

**Pair A — team capacity (gap):**

```
Input: "Plan the team capacity for the next sprint."
Expected predicates: []
Expected gap warnings: [capacity-001, sprint-001]
Reasoning: "capacity" = team resource allocation = gap
```

**Pair B — system capacity with "design" (ARCHITECTURE fires):**

```
Input: "Design the database capacity model."
Expected predicates: ["ARCHITECTURE"]
Expected gap warnings: [capacity-001] (capacity planning intent noted)
Reasoning: "design" fires ARCHITECTURE (not "capacity")
```

**Pair C — "capacity" alone in planning sentence:**

```
Input: "Assess team capacity for the iteration."
Expected predicates: []
Expected gap warnings: [capacity-001, iteration-001]
Reasoning: "capacity" alone does not fire ARCHITECTURE; team context confirmed by "team"
```

---

## B36-L3-004-003 — Critical insight: "capacity" never fires ARCHITECTURE directly

**Key teaching point:** "capacity" is not in the ARCHITECTURE lexicon. ARCHITECTURE fires on "design", "architect", "structure", "architecture" — not on "capacity."

When ARCHITECTURE fires in a sentence with "capacity," the fire comes from one of the ARCHITECTURE lexicon tokens, not from "capacity." This is why team capacity sentences with no ARCHITECTURE token produce [] predicates.

**Test:** "Maximize the system capacity." → Does ARCHITECTURE fire?
- "maximize": no predicate
- "system": no predicate
- "capacity": not in ARCHITECTURE lexicon → no predicate
- Result: [] (zero predicates — neither ARCHITECTURE nor any other predicate fires)

---

## B36-L3-004-004 — Team capacity gap specifics

gap-project-planning-capacity-001 covers:
- Team velocity (story points per sprint)
- Developer headcount allocation
- Sprint capacity planning (how many tickets fit in a sprint)
- Cross-team capacity balancing
- Resource availability tracking

None of these is system architecture work. The gap correctly represents that these planning activities are not supported by the orchestrator.
