# Layer 0-A — Compiler Pairs (008)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: compiler

---

## B36-0A-008-001 — "document" fires DOCUMENT; milestone is gap

**Input request:**
"Document the milestone deliverables and define the acceptance criteria for each sprint release."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| document | DOCUMENT lexicon | DOCUMENT |
| milestone | Not in any predicate lexicon | GAP |
| deliverables | Not in any predicate lexicon | neutral |
| define | DOCUMENT lexicon | DOCUMENT |
| acceptance | Not alone in DOCUMENT lexicon | neutral |
| criteria | DOCUMENT lexicon — "criteria" fires DOCUMENT | DOCUMENT |
| sprint | Not in any predicate lexicon | GAP |
| release | Not in any predicate lexicon | GAP |

**Predicate output:** `["DOCUMENT"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-milestone-001
- gap-project-planning-sprint-001
- gap-project-planning-release-cycle-001

**KNOWLEDGE_EDGE state:** false (DOCUMENT fired)
**DomainId:** "software-development"
**TransitionType:** documentation
**PhaseId:** 2.1
**recommended_action:** route to DOCUMENT phase

**Rule applied:** B36 Rule 5 — "document", "define", "criteria" fire DOCUMENT. B36 Rule 21 — "acceptance criteria" fires DOCUMENT (criteria is in DOCUMENT lexicon). Milestone, sprint, release are gap vocabulary.

---

## B36-0A-008-002 — "specify" fires DOCUMENT; "requirements" fires DOCUMENT

**Input request:**
"Specify the requirements for each milestone and document the sprint deliverables."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| specify | DOCUMENT lexicon | DOCUMENT |
| requirements | DOCUMENT lexicon — "requirements" fires DOCUMENT | DOCUMENT |
| milestone | Not in any predicate lexicon | GAP |
| document | DOCUMENT lexicon | DOCUMENT |
| sprint | Not in any predicate lexicon | GAP |
| deliverables | Not in any predicate lexicon | neutral |

**Predicate output:** `["DOCUMENT"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-milestone-001
- gap-project-planning-sprint-001

**DomainId:** "software-development"
**TransitionType:** documentation

---

## B36-0A-008-003 — "stakeholder" fires DOCUMENT

**Input request:**
"Gather stakeholder input on the sprint goals and document the backlog priorities."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| gather | Not in any predicate lexicon | neutral |
| stakeholder | DOCUMENT lexicon — "stakeholder" fires DOCUMENT | DOCUMENT |
| input | Not in any predicate lexicon | neutral |
| sprint | Not in any predicate lexicon | GAP |
| goals | Not in any predicate lexicon | neutral |
| document | DOCUMENT lexicon | DOCUMENT |
| backlog | Not in any predicate lexicon | GAP |
| priorities | Not in any predicate lexicon | neutral |

**Predicate output:** `["DOCUMENT"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-backlog-001

**DomainId:** "software-development"

**Rule applied:** B36 Rule 5 — "stakeholder" fires DOCUMENT.

---

## B36-0A-008-004 — "document the milestone": DOCUMENT fires; no other predicates

**Input request:**
"Document the milestone exit criteria and capture the burndown baseline."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| document | DOCUMENT lexicon | DOCUMENT |
| milestone | Not in any predicate lexicon | GAP |
| exit | Not in any predicate lexicon | neutral |
| criteria | DOCUMENT lexicon | DOCUMENT |
| capture | Not in DOCUMENT lexicon (B36 Rule 14) | neutral |
| burndown | Not in any predicate lexicon | GAP |
| baseline | Not in any predicate lexicon | neutral |

**Predicate output:** `["DOCUMENT"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-milestone-001
- gap-project-planning-burndown-001

**DomainId:** "software-development"

**Note:** "capture" does NOT fire DOCUMENT (B36 Rule 14). Only "document" and "criteria" fire DOCUMENT.
