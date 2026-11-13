# Layer 0-A — Compiler Pairs (009)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: compiler

---

## B36-0A-009-001 — "review" fires QA on "sprint burndown"

**Input request:**
"Review the sprint burndown report and approve the release readiness."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| review | QA lexicon | QA |
| sprint | Not in any predicate lexicon | GAP |
| burndown | Not in any predicate lexicon | GAP |
| report | Not in any predicate lexicon | neutral |
| approve | QA lexicon — "approve" fires QA | QA |
| release | Not in any predicate lexicon | GAP |
| readiness | Not in any predicate lexicon | neutral |

**Predicate output:** `["QA"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-burndown-001
- gap-project-planning-release-cycle-001

**KNOWLEDGE_EDGE state:** false (QA fired)
**DomainId:** "software-development"
**TransitionType:** no_contribution
**PhaseId:** QA sub-phase
**recommended_action:** route to QA

**Rule applied:** B36 Rule 3 — "review" fires QA even in "sprint burndown review" context. Sprint, burndown, release are gap vocabulary producing warnings.

---

## B36-0A-009-002 — "approve" fires QA in release planning

**Input request:**
"Approve the sprint completion checklist before the milestone release date."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| approve | QA lexicon | QA |
| sprint | Not in any predicate lexicon | GAP |
| completion | Not in any predicate lexicon | neutral |
| checklist | Not in any predicate lexicon | neutral |
| milestone | Not in any predicate lexicon | GAP |
| release | Not in any predicate lexicon | GAP |
| date | Not in any predicate lexicon | neutral |

**Predicate output:** `["QA"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-milestone-001
- gap-project-planning-release-cycle-001

**DomainId:** "software-development"

---

## B36-0A-009-003 — burndown alone: zero predicates

**Input request:**
"Track the burndown chart for the current sprint and flag velocity deviations."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| track | Not in any predicate lexicon | neutral |
| burndown | Not in any predicate lexicon | GAP |
| chart | Not in any predicate lexicon | neutral |
| sprint | Not in any predicate lexicon | GAP |
| flag | Not in any predicate lexicon | neutral |
| velocity | Not in any predicate lexicon | GAP |
| deviations | Not in any predicate lexicon | neutral |

**Predicate output:** `[]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-burndown-001
- gap-project-planning-sprint-001
- gap-project-planning-velocity-001

**KNOWLEDGE_EDGE state:** true
**DomainId:** "project-management"
**recommended_action:** SCHEDULE_CAPABILITY_LEARNING

---

## B36-0A-009-004 — "review" fires QA; "burndown" is gap: DomainId forced to software-development

**Input request:**
"Review the burndown trend and identify capacity bottlenecks for the next sprint."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| review | QA lexicon | QA |
| burndown | Not in any predicate lexicon | GAP |
| trend | Not in any predicate lexicon | neutral |
| identify | Not in any predicate lexicon | neutral |
| capacity | team planning context → GAP | GAP |
| bottlenecks | Not in any predicate lexicon | neutral |
| sprint | Not in any predicate lexicon | GAP |

**Predicate output:** `["QA"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-burndown-001
- gap-project-planning-capacity-001
- gap-project-planning-sprint-001

**DomainId:** "software-development" — QA fired; "project-management" would be WRONG here.

**Key teaching:** Even when the entire sentence is about project planning (burndown, capacity, sprint), if any predicate fires (QA via "review"), DomainId must be "software-development." B36 Rule 7.
