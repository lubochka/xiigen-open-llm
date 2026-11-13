# Layer 0-A — Compiler Pairs (007)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: compiler

---

## B36-0A-007-001 — "implement" fires IMPLEMENT; epic and sprint are gap

**Input request:**
"Implement the features from Epic E-14 and close the sprint tickets when complete."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| implement | IMPLEMENT lexicon | IMPLEMENT |
| features | IMPLEMENT lexicon — "feature" fires IMPLEMENT | IMPLEMENT |
| Epic | Not in any predicate lexicon | GAP |
| E-14 | Not in any predicate lexicon | neutral |
| close | Not in any predicate lexicon | neutral |
| sprint | Not in any predicate lexicon | GAP |
| tickets | Not in any predicate lexicon | GAP |
| complete | Not in any predicate lexicon | neutral |

**Predicate output:** `["IMPLEMENT"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-epic-001
- gap-project-planning-sprint-001
- gap-project-planning-ticket-001

**KNOWLEDGE_EDGE state:** false (IMPLEMENT fired)
**DomainId:** "software-development"
**TransitionType:** feature_addition
**PhaseId:** 4.1
**recommended_action:** route to IMPLEMENT phase

**Rule applied:** B36 Rule 6 — "implement" and "features" fire IMPLEMENT. B36 Rule 11 — epic, sprint, tickets produce CapabilityGapWarnings but do not alter routing.

---

## B36-0A-007-002 — "create" fires IMPLEMENT in sprint-deliverable sentence

**Input request:**
"Create the authentication module before the sprint milestone deadline."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| create | IMPLEMENT lexicon — "create" fires IMPLEMENT | IMPLEMENT |
| authentication | Not in any predicate lexicon | neutral |
| module | IMPLEMENT lexicon — "module" co-occurs as code-level entity | IMPLEMENT |
| sprint | Not in any predicate lexicon | GAP |
| milestone | Not in any predicate lexicon | GAP |
| deadline | Not in any predicate lexicon | neutral |

**Predicate output:** `["IMPLEMENT"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-milestone-001

**DomainId:** "software-development"
**TransitionType:** feature_addition

---

## B36-0A-007-003 — "build" fires IMPLEMENT; "epic" is gap

**Input request:**
"Build the features listed in the epic backlog for this sprint."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| build | IMPLEMENT lexicon | IMPLEMENT |
| features | IMPLEMENT lexicon | IMPLEMENT |
| epic | Not in any predicate lexicon | GAP |
| backlog | Not in any predicate lexicon | GAP |
| sprint | Not in any predicate lexicon | GAP |

**Predicate output:** `["IMPLEMENT"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-epic-001
- gap-project-planning-backlog-001
- gap-project-planning-sprint-001

**DomainId:** "software-development"

---

## B36-0A-007-004 — "epic" + "ticket" alone: zero predicates

**Input request:**
"Break down Epic E-7 into tickets and assign them to the iteration."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| break | Not in any predicate lexicon | neutral |
| down | Not in any predicate lexicon | neutral |
| Epic | Not in any predicate lexicon | GAP |
| tickets | Not in any predicate lexicon | GAP |
| assign | Not in any predicate lexicon | neutral |
| iteration | Not in any predicate lexicon | GAP |

**Predicate output:** `[]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-epic-001
- gap-project-planning-ticket-001
- gap-project-planning-iteration-001

**KNOWLEDGE_EDGE state:** true
**DomainId:** "project-management"
**recommended_action:** SCHEDULE_CAPABILITY_LEARNING
