# Layer 0-A — Compiler Pairs (003)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: compiler

---

## B36-0A-003-001 — velocity + retrospective: "review" fires QA

**Input request:**
"Review the team velocity from the last sprint and plan the retrospective agenda."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| review | QA lexicon — "review" fires QA | QA |
| team | Not in any predicate lexicon | neutral |
| velocity | Not in any predicate lexicon | GAP |
| sprint | Not in any predicate lexicon | GAP |
| plan | Not in any predicate lexicon | neutral (B36 Rule 17) |
| retrospective | Not in any predicate lexicon | GAP |
| agenda | Not in any predicate lexicon | neutral |

**Predicate output:** `["QA"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-velocity-001
- gap-project-planning-sprint-001
- gap-project-planning-retrospective-001

**KNOWLEDGE_EDGE state:** false (QA fired)
**DomainId:** "software-development" (predicate fired)
**TransitionType:** no_contribution
**PhaseId:** QA sub-phase
**recommended_action:** route to QA

**Rule applied:** B36 Rule 3 — "review" fires QA even in "velocity review" and "sprint review" contexts. "retrospective" is gap — it does NOT fire QA (distinct term).

---

## B36-0A-003-002 — "retrospective" ≠ "review": sharp boundary

**Input request:**
"Schedule the sprint retrospective and make sure we capture action items."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| schedule | Not in any predicate lexicon | neutral |
| sprint | Not in any predicate lexicon | GAP |
| retrospective | Not in any predicate lexicon | GAP |
| make | Not in any predicate lexicon | neutral |
| capture | Not in DOCUMENT lexicon (B36 Rule 14) | neutral |
| action | Not in any predicate lexicon | neutral |
| items | Not in any predicate lexicon | neutral |

**Predicate output:** `[]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-retrospective-001

**KNOWLEDGE_EDGE state:** true
**DomainId:** "project-management"
**recommended_action:** SCHEDULE_CAPABILITY_LEARNING

**Key distinction:** "retrospective" and "review" are different tokens. "review" fires QA. "retrospective" does NOT fire QA. The user intent may be similar, but the compiler is literal. B36 Rule 3 and Rule 14 ("capture" not in DOCUMENT lexicon).

---

## B36-0A-003-003 — "velocity review" compound: only "review" portion fires

**Input request:**
"Run a velocity review at the end of each sprint and log the burndown results."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| run | Not in any predicate lexicon (B36 Rule 18) | neutral |
| velocity | Not in any predicate lexicon | GAP |
| review | QA lexicon | QA |
| end | Not in any predicate lexicon | neutral |
| sprint | Not in any predicate lexicon | GAP |
| log | Not in any predicate lexicon | neutral |
| burndown | Not in any predicate lexicon | GAP |
| results | Not in any predicate lexicon | neutral |

**Predicate output:** `["QA"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-velocity-001
- gap-project-planning-sprint-001
- gap-project-planning-burndown-001

**Key point:** "run" does not fire IMPLEMENT (B36 Rule 18 — "run" in ceremony context is not in any predicate lexicon). Only "review" fires, and it fires QA.

**DomainId:** "software-development"

---

## B36-0A-003-004 — "plan" alone does not fire any predicate

**Input request:**
"Plan the next sprint and set velocity targets for the team."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| plan | Not in any predicate lexicon (B36 Rule 17) | neutral |
| sprint | Not in any predicate lexicon | GAP |
| set | Not in any predicate lexicon | neutral |
| velocity | Not in any predicate lexicon | GAP |
| targets | Not in any predicate lexicon | neutral |
| team | Not in any predicate lexicon | neutral |

**Predicate output:** `[]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-velocity-001

**KNOWLEDGE_EDGE state:** true
**DomainId:** "project-management"
**recommended_action:** SCHEDULE_CAPABILITY_LEARNING

**Rule applied:** B36 Rule 17 — "plan" alone is not in any predicate lexicon.
