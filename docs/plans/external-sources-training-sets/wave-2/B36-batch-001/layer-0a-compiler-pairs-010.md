# Layer 0-A — Compiler Pairs (010)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: compiler

---

## B36-0A-010-001 — IMPLEMENT+DOCUMENT+QA mixed with heavy planning vocab

**Input request:**
"Build the sprint tracking service, document the backlog requirements, and review the velocity reports."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| build | IMPLEMENT lexicon | IMPLEMENT |
| sprint | Not in any predicate lexicon | GAP |
| tracking | Not in any predicate lexicon | neutral |
| service | IMPLEMENT lexicon — "service" is code-level entity co-occurring with "build" | IMPLEMENT |
| document | DOCUMENT lexicon | DOCUMENT |
| backlog | Not in any predicate lexicon | GAP |
| requirements | DOCUMENT lexicon | DOCUMENT |
| review | QA lexicon | QA |
| velocity | Not in any predicate lexicon | GAP |
| reports | Not in any predicate lexicon | neutral |

**Predicate output:** `["IMPLEMENT", "DOCUMENT", "QA"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-backlog-001
- gap-project-planning-velocity-001

**KNOWLEDGE_EDGE state:** false (three predicates fired)
**DomainId:** "software-development"
**TransitionType:** feature_addition (IMPLEMENT is primary)
**PhaseId:** 4.1 (primary), 2.1, QA sub-phase
**recommended_action:** route to IMPLEMENT (primary), DOCUMENT, QA (last)

**Rule applied:** B36 Rule 11 — predicates dominate; gap vocabulary produces warnings only. Three predicates fired; all three routing decisions execute. QA executes last.

---

## B36-0A-010-002 — IMPLEMENT+QA without DOCUMENT

**Input request:**
"Build the sprint velocity tracker and review the performance metrics after each release."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| build | IMPLEMENT lexicon | IMPLEMENT |
| sprint | Not in any predicate lexicon | GAP |
| velocity | Not in any predicate lexicon | GAP |
| tracker | Not in any predicate lexicon | neutral |
| review | QA lexicon | QA |
| performance | Not in any predicate lexicon | neutral |
| metrics | Not in any predicate lexicon | neutral |
| release | Not in any predicate lexicon | GAP |

**Predicate output:** `["IMPLEMENT", "QA"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-velocity-001
- gap-project-planning-release-cycle-001

**DomainId:** "software-development"
**TransitionType:** feature_addition

---

## B36-0A-010-003 — DOCUMENT+QA without IMPLEMENT

**Input request:**
"Document the backlog requirements and review the milestone acceptance criteria."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| document | DOCUMENT lexicon | DOCUMENT |
| backlog | Not in any predicate lexicon | GAP |
| requirements | DOCUMENT lexicon | DOCUMENT |
| review | QA lexicon | QA |
| milestone | Not in any predicate lexicon | GAP |
| acceptance | Not standalone in DOCUMENT lexicon | neutral |
| criteria | DOCUMENT lexicon | DOCUMENT |

**Predicate output:** `["DOCUMENT", "QA"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-backlog-001
- gap-project-planning-milestone-001

**DomainId:** "software-development"
**TransitionType:** documentation

---

## B36-0A-010-004 — all three predicates again: different sentence structure

**Input request:**
"Implement the epic features, specify the sprint documentation, and review readiness."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| implement | IMPLEMENT lexicon | IMPLEMENT |
| epic | Not in any predicate lexicon | GAP |
| features | IMPLEMENT lexicon | IMPLEMENT |
| specify | DOCUMENT lexicon | DOCUMENT |
| sprint | Not in any predicate lexicon | GAP |
| documentation | DOCUMENT lexicon — "document" substring fires DOCUMENT | DOCUMENT |
| review | QA lexicon | QA |
| readiness | Not in any predicate lexicon | neutral |

**Predicate output:** `["IMPLEMENT", "DOCUMENT", "QA"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-epic-001
- gap-project-planning-sprint-001

**DomainId:** "software-development"
**recommended_action:** route to all three phases; QA last
