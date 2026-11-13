# Layer 0-A — Compiler Pairs (012)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: compiler

---

## B36-0A-012-001 — Complete B36 compiler pattern summary

**B36 gap vocabulary: tokens with NO registered predicate**

| Token | Gap ID | Notes |
|-------|--------|-------|
| sprint | gap-project-planning-sprint-001 | Always gap |
| roadmap | gap-project-planning-roadmap-001 | Always gap |
| milestone | gap-project-planning-milestone-001 | Always gap |
| backlog | gap-project-planning-backlog-001 | Always gap |
| velocity | gap-project-planning-velocity-001 | Always gap |
| standup | gap-project-planning-standup-001 | Always gap |
| retrospective | gap-project-planning-retrospective-001 | Always gap — distinct from "review" |
| burndown | gap-project-planning-burndown-001 | Always gap |
| capacity (team) | gap-project-planning-capacity-001 | Gap in team context only |
| iteration | gap-project-planning-iteration-001 | Always gap |
| epic | gap-project-planning-epic-001 | Always gap |
| ticket | gap-project-planning-ticket-001 | Always gap |
| story-point | gap-project-planning-story-point-001 | Compound; "story" still fires DOCUMENT |
| estimation | gap-project-planning-estimation-001 | Always gap |
| release cycle | gap-project-planning-release-cycle-001 | Planning release context |

---

## B36-0A-012-002 — Predicate classification table for B36 boundary terms

**Tokens that DO fire predicates in B36 planning sentences**

| Token | Predicate fired | Notes |
|-------|----------------|-------|
| review | QA | Always — even "velocity review", "sprint review" |
| approve | QA | Always |
| test | TEST | Always |
| story | DOCUMENT | "story" substring fires DOCUMENT; false positive in "story points" |
| stories | DOCUMENT | "story" substring |
| document | DOCUMENT | Always |
| define | DOCUMENT | Always |
| specify | DOCUMENT | Always |
| requirements | DOCUMENT | Always |
| stakeholder | DOCUMENT | Always |
| criteria | DOCUMENT | Always — "acceptance criteria" too |
| implement | IMPLEMENT | Always |
| feature | IMPLEMENT | Always |
| build | IMPLEMENT | Always |
| create | IMPLEMENT | Always |
| write | IMPLEMENT | "write" (code) — fires even for "write user stories" |
| design | ARCHITECTURE | Always — even "sprint design" |
| architect | ARCHITECTURE | Always |
| structure | ARCHITECTURE | Always |
| architecture | ARCHITECTURE | Always |

---

## B36-0A-012-003 — Tokens that do NOT fire any predicate

| Token | Expected behavior | Rule |
|-------|-------------------|------|
| plan | No predicate | Rule 17 |
| run | No predicate (ceremony context) | Rule 18 |
| generate | No predicate | Rule 13 |
| capture | No predicate | Rule 14 |
| update (planning) | No predicate — needs code-level co-occurrence | Rule 12 |
| estimate | GAP vocabulary, no predicate | Rule 15 |
| capacity (team) | GAP vocabulary, no predicate (not ARCHITECTURE) | Rule 16 |
| groom | No predicate | Rule 1 |
| prioritize | No predicate | Rule 1 |
| facilitate | No predicate | Rule 1 |
| track | No predicate | Rule 1 |
| split | No predicate | Rule 1 |

---

## B36-0A-012-004 — DomainId determination summary

| Condition | DomainId | Example |
|-----------|----------|---------|
| Zero predicates fired | "project-management" | "Plan the sprint backlog" |
| Any predicate fired | "software-development" | "Review the sprint velocity" |
| Mixed: predicate + gap vocab | "software-development" | "Build the sprint tracker" |

**Hard rule:** DomainId is determined by whether any predicate fired — not by the balance of gap vocabulary versus development vocabulary. Even a single predicate token forces "software-development."
