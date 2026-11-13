# Layer 2 — Capability Gap Extended Pairs (012)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B36-L2-012-001 — Complete B36 gap vocabulary map + gap-ID naming convention reference

**Primary gap vocabulary with IDs:**

| Term | Gap ID | Category |
|------|--------|----------|
| sprint | gap-project-planning-sprint-001 | Sprint planning |
| roadmap | gap-project-planning-roadmap-001 | Roadmapping |
| milestone | gap-project-planning-milestone-001 | Delivery tracking |
| backlog | gap-project-planning-backlog-001 | Backlog management |
| velocity | gap-project-planning-velocity-001 | Team metrics |
| standup | gap-project-planning-standup-001 | Ceremonies |
| retrospective | gap-project-planning-retrospective-001 | Ceremonies |
| burndown | gap-project-planning-burndown-001 | Progress tracking |
| capacity (team) | gap-project-planning-capacity-001 | Resource planning |
| iteration | gap-project-planning-iteration-001 | Iteration planning |
| epic | gap-project-planning-epic-001 | Work organization |
| ticket | gap-project-planning-ticket-001 | Work items |
| story-point | gap-project-planning-story-point-001 | Estimation units |
| estimation | gap-project-planning-estimation-001 | Effort estimation |
| release cycle | gap-project-planning-release-cycle-001 | Release planning |

---

## B36-L2-012-002 — Compound gap IDs

| Compound | Gap ID | Triggered when |
|----------|--------|---------------|
| sprint + roadmap | gap-project-planning-sprint-roadmap-001 | Both terms, zero predicates |
| standup + retrospective | gap-project-planning-standup-retrospective-001 | Both terms, zero predicates |
| burndown + milestone + tracker | gap-project-planning-burndown-milestone-tracker-001 | All three, zero predicates |
| velocity + retrospective | gap-project-planning-velocity-retrospective-001 | Both terms, zero predicates |

---

## B36-L2-012-003 — Mixed-signal gap IDs (no -001 sequence)

| Mixed-signal gap | Triggered when |
|-----------------|---------------|
| gap-project-planning-sprint-not-captured | sprint present but IMPLEMENT (or other predicate) also fired |
| gap-project-planning-backlog-estimation | backlog + estimation + DOCUMENT fired |

---

## B36-L2-012-004 — Gap ID naming convention rules

**Format:** `gap-project-planning-{term(s)}-{sequence}`

1. All B36 gap IDs start with `gap-project-planning-`
2. Single-term IDs end with `-001` (first registered instance of that term)
3. Compound IDs combine term names with hyphens: `sprint-roadmap`, `standup-retrospective`
4. Mixed-signal IDs omit the sequence number and use descriptive suffixes: `-not-captured`, `-estimation`
5. Future gap IDs for new B36 terms follow the same convention
6. Gap IDs are stable once registered — they do not change across batches
7. A new gap ID requires a manifest entry before use in training files
