# Layer 0-A — Compiler Pairs (005)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: compiler

---

## B36-0A-005-001 — "design" fires ARCHITECTURE even in sprint sentence

**Input request:**
"Design the system architecture for the sprint deliverables and plan the milestone dependencies."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| design | ARCHITECTURE lexicon — "design" fires ARCHITECTURE | ARCHITECTURE |
| system | Not in any predicate lexicon | neutral |
| architecture | ARCHITECTURE lexicon — "architect" substring fires ARCHITECTURE | ARCHITECTURE |
| sprint | Not in any predicate lexicon | GAP |
| deliverables | Not in any predicate lexicon | neutral |
| plan | Not in any predicate lexicon | neutral |
| milestone | Not in any predicate lexicon | GAP |
| dependencies | Not in any predicate lexicon | neutral |

**Predicate output:** `["ARCHITECTURE"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-milestone-001

**KNOWLEDGE_EDGE state:** false (ARCHITECTURE fired)
**DomainId:** "software-development" (predicate fired)
**TransitionType:** documentation (ARCHITECTURE maps to documentation transition)
**PhaseId:** 3.1
**recommended_action:** route to ARCHITECTURE phase

**Rule applied:** B36 Rule 4 — "design" and "architect/architecture" fire ARCHITECTURE even when embedded in project planning sentences. Sprint and milestone are gap vocabulary producing CapabilityGapWarnings.

---

## B36-0A-005-002 — "architect" in planning context: still fires ARCHITECTURE

**Input request:**
"Architect the data flow for the upcoming sprint and document the milestone delivery plan."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| architect | ARCHITECTURE lexicon | ARCHITECTURE |
| data | Not in any predicate lexicon | neutral |
| flow | Not in any predicate lexicon | neutral |
| sprint | Not in any predicate lexicon | GAP |
| document | DOCUMENT lexicon | DOCUMENT |
| milestone | Not in any predicate lexicon | GAP |
| delivery | Not in any predicate lexicon | neutral |
| plan | Not in any predicate lexicon | neutral |

**Predicate output:** `["ARCHITECTURE", "DOCUMENT"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-milestone-001

**DomainId:** "software-development"
**TransitionType:** documentation

**Note:** Both ARCHITECTURE and DOCUMENT fire. Sprint and milestone produce gap warnings but do not alter routing.

---

## B36-0A-005-003 — "structure" fires ARCHITECTURE in sprint planning sentence

**Input request:**
"Define the structure of the sprint workflow and specify the milestone exit criteria."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| define | DOCUMENT lexicon — "define" fires DOCUMENT | DOCUMENT |
| structure | ARCHITECTURE lexicon — "structure" fires ARCHITECTURE | ARCHITECTURE |
| sprint | Not in any predicate lexicon | GAP |
| workflow | Not in any predicate lexicon | neutral |
| specify | DOCUMENT lexicon — "specify" fires DOCUMENT | DOCUMENT |
| milestone | Not in any predicate lexicon | GAP |
| exit | Not in any predicate lexicon | neutral |
| criteria | DOCUMENT lexicon — "criteria" fires DOCUMENT | DOCUMENT |

**Predicate output:** `["DOCUMENT", "ARCHITECTURE"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-milestone-001

**DomainId:** "software-development"

**Rule applied:** B36 Rule 4 — "structure" fires ARCHITECTURE. B36 Rule 5 — "define", "specify", "criteria" fire DOCUMENT. B36 Rule 21 — "acceptance criteria" / "exit criteria" fires DOCUMENT (criteria is in DOCUMENT lexicon).

---

## B36-0A-005-004 — "plan" does not fire; "milestone" is gap

**Input request:**
"Plan the milestone schedule and build the roadmap for Q3."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| plan | Not in any predicate lexicon | neutral |
| milestone | Not in any predicate lexicon | GAP |
| schedule | Not in any predicate lexicon | neutral |
| build | IMPLEMENT lexicon — "build" fires IMPLEMENT | IMPLEMENT |
| roadmap | Not in any predicate lexicon | GAP |
| Q3 | Not in any predicate lexicon | neutral |

**Predicate output:** `["IMPLEMENT"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-milestone-001
- gap-project-planning-roadmap-001

**DomainId:** "software-development"

**Rule applied:** B36 Rule 6 — "build" fires IMPLEMENT regardless of the object ("roadmap"). The user means to build the roadmap document, but the compiler fires on the literal token "build."
