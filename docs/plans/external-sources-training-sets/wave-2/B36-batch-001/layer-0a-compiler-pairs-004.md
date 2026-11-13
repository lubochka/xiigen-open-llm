# Layer 0-A — Compiler Pairs (004)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: compiler

---

## B36-0A-004-001 — "write" fires IMPLEMENT; "stories" fires DOCUMENT; "story points" fires DOCUMENT on "story"

**Input request:**
"Write the user stories for the checkout feature and estimate the story points."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| write | IMPLEMENT lexicon — "write" (code) fires IMPLEMENT | IMPLEMENT |
| user | Not in any predicate lexicon | neutral |
| stories | DOCUMENT lexicon — "story" substring in "stories" fires DOCUMENT | DOCUMENT |
| checkout | Not in any predicate lexicon | neutral |
| feature | IMPLEMENT lexicon — "feature" fires IMPLEMENT | IMPLEMENT |
| estimate | Not in any predicate lexicon | GAP |
| story | DOCUMENT lexicon — "story" fires DOCUMENT | DOCUMENT |
| points | Not in any predicate lexicon | neutral |

**Predicate output:** `["IMPLEMENT", "DOCUMENT"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-story-point-001 (estimation intent not captured)

**KNOWLEDGE_EDGE state:** false (predicates fired)
**DomainId:** "software-development"
**TransitionType:** feature_addition
**PhaseId:** 4.1 (IMPLEMENT) and 2.1 (DOCUMENT)
**recommended_action:** route to IMPLEMENT (primary) + DOCUMENT

**Rule applied:** B36 Rule 2 — "story" fires DOCUMENT. B36 Rule 6 — "write" and "feature" fire IMPLEMENT. B36 Rule 11 — predicates fire alongside gap vocabulary; routing proceeds normally; gap vocabulary produces CapabilityGapWarnings only.

---

## B36-0A-004-002 — "write" fires IMPLEMENT even in documentation context

**Input request:**
"Write a document about sprint planning and story point estimation techniques."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| write | IMPLEMENT lexicon | IMPLEMENT |
| document | DOCUMENT lexicon — "document" fires DOCUMENT | DOCUMENT |
| sprint | Not in any predicate lexicon | GAP |
| planning | Not in any predicate lexicon | neutral |
| story | DOCUMENT lexicon | DOCUMENT |
| point | Not in any predicate lexicon | neutral |
| estimation | Not in any predicate lexicon | GAP |
| techniques | Not in any predicate lexicon | neutral |

**Predicate output:** `["IMPLEMENT", "DOCUMENT"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-estimation-001
- gap-project-planning-story-point-001

**Note:** "write" fires IMPLEMENT regardless of the object being "a document." The compiler scans tokens literally, not semantically. The developer's intent to write documentation is overridden by the literal "write" → IMPLEMENT rule.

**DomainId:** "software-development"

---

## B36-0A-004-003 — "estimate" only: zero predicates

**Input request:**
"Estimate the capacity needed for the next sprint iteration."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| estimate | Not in any predicate lexicon | GAP |
| capacity | ARCHITECTURE check: "capacity" in team planning context does NOT fire ARCHITECTURE (B36 Rule 16) | GAP |
| sprint | Not in any predicate lexicon | GAP |
| iteration | Not in any predicate lexicon | GAP |

**Predicate output:** `[]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-estimation-001
- gap-project-planning-capacity-001
- gap-project-planning-sprint-001
- gap-project-planning-iteration-001

**KNOWLEDGE_EDGE state:** true
**DomainId:** "project-management"

**Rule applied:** B36 Rule 15 — "estimate" is gap. B36 Rule 16 — "capacity" in team planning context is NOT ARCHITECTURE (ARCHITECTURE fires on system capacity terms, not team planning capacity).

---

## B36-0A-004-004 — "feature" fires IMPLEMENT: false positive in planning sentence

**Input request:**
"Add the payment feature to the sprint backlog and estimate the story points."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| add | Not in any predicate lexicon | neutral |
| payment | Not in any predicate lexicon | neutral |
| feature | IMPLEMENT lexicon | IMPLEMENT |
| sprint | Not in any predicate lexicon | GAP |
| backlog | Not in any predicate lexicon | GAP |
| estimate | Not in any predicate lexicon | GAP |
| story | DOCUMENT lexicon | DOCUMENT |
| points | Not in any predicate lexicon | neutral |

**Predicate output:** `["IMPLEMENT", "DOCUMENT"]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-backlog-001
- gap-project-planning-estimation-001
- gap-project-planning-story-point-001

**DomainId:** "software-development"

**Note:** "feature" fires IMPLEMENT even in a planning/backlog sentence. The user intends to add an item to the backlog, not build code — but the compiler fires on the literal token "feature." This is the defined behavior per B36 Rule 6.
