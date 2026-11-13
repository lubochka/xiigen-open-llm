# Layer 0-C — Knowledge-Edge Pairs (010)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B36-0C-010-001 — Complete gap vocabulary list for B36 with gap IDs

**Primary gap terms (single token):**

| Term | Gap ID | Notes |
|------|--------|-------|
| sprint | gap-project-planning-sprint-001 | All sprint usages |
| roadmap | gap-project-planning-roadmap-001 | Product/project roadmaps |
| milestone | gap-project-planning-milestone-001 | Delivery milestones |
| backlog | gap-project-planning-backlog-001 | Product/sprint backlog |
| velocity | gap-project-planning-velocity-001 | Sprint velocity metric |
| standup | gap-project-planning-standup-001 | Daily standup ceremony |
| retrospective | gap-project-planning-retrospective-001 | Sprint retrospective |
| burndown | gap-project-planning-burndown-001 | Burndown chart/report |
| capacity | gap-project-planning-capacity-001 | Team planning capacity only |
| iteration | gap-project-planning-iteration-001 | Development iteration |
| epic | gap-project-planning-epic-001 | Epic-level work item |
| ticket | gap-project-planning-ticket-001 | Work ticket/issue |
| estimation | gap-project-planning-estimation-001 | Effort estimation |

---

## B36-0C-010-002 — Compound and mixed-signal gap IDs

**Compound gap IDs (registered for multi-term combinations):**

| Compound | Gap ID | Triggered when |
|----------|--------|---------------|
| sprint + roadmap | gap-project-planning-sprint-roadmap-001 | Both terms co-occur, zero predicates |
| standup + retrospective | gap-project-planning-standup-retrospective-001 | Both terms, zero predicates |
| burndown + milestone + tracker | gap-project-planning-burndown-milestone-tracker-001 | All three terms, zero predicates |
| velocity + retrospective | gap-project-planning-velocity-retrospective-001 | Both terms, zero predicates |
| release cycle | gap-project-planning-release-cycle-001 | Release planning context |

**Mixed-signal gap IDs (used in CapabilityGapWarnings when predicates fired):**

| Compound | Gap ID | Triggered when |
|----------|--------|---------------|
| story + (estimation intent) | gap-project-planning-story-point-001 | DOCUMENT fires on "story" |
| sprint (when predicate also fired) | gap-project-planning-sprint-not-captured | Sprint intent not served |
| backlog + estimation | gap-project-planning-backlog-estimation | Estimation intent in backlog context |

---

## B36-0C-010-003 — Gap ID naming convention

**Pattern:** `gap-project-planning-{term(s)}-{sequence}`

- `gap`: prefix for all gap IDs
- `project-planning`: domain identifier for B36
- `{term(s)}`: the gap vocabulary term(s), hyphenated for compounds
- `{sequence}`: three-digit sequence starting at 001

**Examples:**
- Single term: `gap-project-planning-sprint-001`
- Compound: `gap-project-planning-sprint-roadmap-001`
- Mixed-signal: `gap-project-planning-sprint-not-captured` (no sequence — warning form)

---

## B36-0C-010-004 — Gap ID usage rules

1. Emit the most specific gap ID that matches the terms present
2. For compound terms, emit both the individual gap IDs AND the compound gap ID
3. Mixed-signal gap IDs (without sequence) are used in CapabilityGapWarnings when predicates fired
4. Standard gap IDs (with -001 sequence) are used in both KNOWLEDGE_EDGE and CapabilityGapWarnings
5. Registered IDs are canonical — do not invent new formats
6. New gap IDs require registration (adding to the batch manifest) before use
