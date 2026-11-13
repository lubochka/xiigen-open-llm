# Layer 0-C — Knowledge-Edge Pairs (002)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B36-0C-002-001 — "retrospective" vs "review": sharp boundary

**The boundary rule:**

| Token | Predicate fired | Evidence |
|-------|----------------|----------|
| review | QA | Always — lexicon match |
| retrospective | none | Not in any lexicon |

**Why this matters in B36:**
A sprint retrospective is a ceremony where the team reflects on the previous sprint. A sprint review is a demonstration to stakeholders. In natural language, users may conflate these. The compiler does not. "retrospective" is gap vocabulary (gap-project-planning-retrospective-001). "review" fires QA.

**Test case A — only "retrospective":**
- Request: "Schedule the sprint retrospective for Friday."
- Predicates: []
- KNOWLEDGE_EDGE: true, DomainId: "project-management"

**Test case B — only "review":**
- Request: "Schedule the sprint review for Friday."
- Predicates: ["QA"]
- KNOWLEDGE_EDGE: false, DomainId: "software-development"

**Test case C — both in one sentence:**
- Request: "Plan the retrospective and review the sprint outcomes."
- Predicates: ["QA"] (from "review")
- CapabilityGapWarnings: retrospective-001, sprint-001
- KNOWLEDGE_EDGE: false (QA fired)

---

## B36-0C-002-002 — "velocity review" is not the same as "retrospective"

**Common confusion:** Both "velocity review" and "retrospective" involve looking back at past performance. The compiler does not reason about semantic similarity.

- "velocity review" → "review" fires QA → predicates: ["QA"]
- "velocity retrospective" → retrospective is gap → predicates: []

**The word "review" fires QA regardless of what precedes it.**

---

## B36-0C-002-003 — knowledge-edge state for retrospective-only requests

**Input:** "Run the retrospective ceremony and capture the action items."

**Token analysis:**
- run: no predicate (Rule 18)
- retrospective: gap (retrospective-001)
- ceremony: no predicate
- capture: no predicate (Rule 14)
- action: no predicate
- items: no predicate

**KNOWLEDGE_EDGE state:**
- predicates: []
- FinalHumanResult: null
- DomainId: "project-management"
- CapabilityGapWarnings: gap-project-planning-retrospective-001
- recommended_action: SCHEDULE_CAPABILITY_LEARNING

---

## B36-0C-002-004 — boundary summary: retrospective vs review

**Hard rule:** Any token that fires QA is in the QA lexicon. Any token NOT in any lexicon is gap vocabulary. These two categories are mutually exclusive. A token cannot be both gap and a predicate trigger. "retrospective" is gap. "review" is QA. They are never interchangeable in the compiler's lexicon.

If a future iteration registers "retrospective" as a predicate, that would require a new entry in a lexicon — it would not happen automatically by semantic proximity to "review." Until registered, "retrospective" remains gap.
