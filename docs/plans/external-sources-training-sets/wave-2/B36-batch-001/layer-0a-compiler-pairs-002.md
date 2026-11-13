# Layer 0-A — Compiler Pairs (002)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: compiler

---

## B36-0A-002-001 — backlog grooming: zero predicates → knowledge_edge

**Input request:**
"Groom the product backlog and prioritize the tickets for the next iteration."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| groom | Not in any predicate lexicon | GAP |
| product | Not in any predicate lexicon | neutral |
| backlog | Not in any predicate lexicon | GAP |
| prioritize | Not in any predicate lexicon | GAP |
| tickets | Not in any predicate lexicon | GAP |
| iteration | Not in any predicate lexicon | GAP |

**Predicate output:** `[]` (empty — zero predicates fired)

**CapabilityGapWarnings emitted:**
- gap-project-planning-backlog-001
- gap-project-planning-iteration-001
- gap-project-planning-ticket-001

**KNOWLEDGE_EDGE state:** true
- FinalHumanResult: null
- TransitionType: unknown
- PhaseId: null
- DomainId: "project-management"
- recommended_action: SCHEDULE_CAPABILITY_LEARNING

**Rule applied:** B36 Rule 1 — "backlog", "tickets", "iteration" are all gap vocabulary with no registered predicate. B36 Rule 8 — TransitionType unknown and PhaseId null when zero predicates fire. B36 Rule 20 — FinalHumanResult null for knowledge_edge.

---

## B36-0A-002-002 — "prioritize" is not in any predicate lexicon

**Input request:**
"Prioritize the sprint backlog and assign story points to each ticket."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| prioritize | Not in any predicate lexicon | GAP |
| sprint | Not in any predicate lexicon | GAP |
| backlog | Not in any predicate lexicon | GAP |
| assign | Not in any predicate lexicon | neutral |
| story | DOCUMENT lexicon — "story" substring fires DOCUMENT | DOCUMENT |
| points | Not in any predicate lexicon | neutral |
| ticket | Not in any predicate lexicon | GAP |

**Predicate output:** `["DOCUMENT"]` (story fires DOCUMENT)

**CapabilityGapWarnings emitted:**
- gap-project-planning-sprint-001
- gap-project-planning-backlog-001
- gap-project-planning-story-point-001

**KNOWLEDGE_EDGE state:** false (DOCUMENT fired)
**DomainId:** "software-development" (predicate fired — B36 Rule 7)
**TransitionType:** documentation
**PhaseId:** 2.1
**recommended_action:** route to DOCUMENT phase

**Rule applied:** B36 Rule 2 — "story" fires DOCUMENT even in "story points" context. This is the defined false-positive behavior; the compiler must not suppress it.

---

## B36-0A-002-003 — pure gap: "groom", "backlog", "prioritize", "tickets", "iteration"

**Input request:**
"Can you help me groom the backlog, split the epics into iterations, and set ticket priorities?"

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| help | Not in any predicate lexicon | neutral |
| groom | Not in any predicate lexicon | GAP |
| backlog | Not in any predicate lexicon | GAP |
| split | Not in any predicate lexicon | neutral |
| epics | Not in any predicate lexicon | GAP |
| iterations | Not in any predicate lexicon | GAP |
| ticket | Not in any predicate lexicon | GAP |
| priorities | Not in any predicate lexicon | neutral |

**Predicate output:** `[]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-backlog-001
- gap-project-planning-epic-001
- gap-project-planning-iteration-001
- gap-project-planning-ticket-001

**KNOWLEDGE_EDGE state:** true
- FinalHumanResult: null
- DomainId: "project-management"
- recommended_action: SCHEDULE_CAPABILITY_LEARNING

**Rule applied:** B36 Rule 22 — one SCHEDULE_CAPABILITY_LEARNING emitted for the entire request (not one per gap term, even though four gap IDs are named in warnings).

---

## B36-0A-002-004 — "estimate" is gap vocabulary

**Input request:**
"Estimate the effort for the backlog items and update the roadmap."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| estimate | Not in any predicate lexicon | GAP |
| effort | Not in any predicate lexicon | neutral |
| backlog | Not in any predicate lexicon | GAP |
| items | Not in any predicate lexicon | neutral |
| update | IMPLEMENT lexicon check: requires code-level co-occurrence | check |
| roadmap | Not in any predicate lexicon | GAP |

**"update" analysis:**
"update" appears with "roadmap" — a planning artifact, not a code-level entity (function / class / logic / module / code). B36 Rule 12 — "update" in project/planning context does NOT fire IMPLEMENT. No code-level co-occurrence present.

**Predicate output:** `[]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-estimation-001
- gap-project-planning-backlog-001
- gap-project-planning-roadmap-001

**KNOWLEDGE_EDGE state:** true
**DomainId:** "project-management"
**recommended_action:** SCHEDULE_CAPABILITY_LEARNING

**Rule applied:** B36 Rules 12 and 15 — "update" alone in planning context does not fire; "estimate" is gap vocabulary per Rule 15.
