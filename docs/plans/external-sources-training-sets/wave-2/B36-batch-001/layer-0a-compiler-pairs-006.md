# Layer 0-A — Compiler Pairs (006)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: compiler

---

## B36-0A-006-001 — pure gap: standup / capacity: zero predicates

**Input request:**
"Run the daily standup and capture team capacity for the sprint planning session."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| run | Not in any predicate lexicon (B36 Rule 18) | neutral |
| daily | Not in any predicate lexicon | neutral |
| standup | Not in any predicate lexicon | GAP |
| capture | Not in DOCUMENT lexicon (B36 Rule 14) | neutral |
| team | Not in any predicate lexicon | neutral |
| capacity | ARCHITECTURE check: team planning context → NOT ARCHITECTURE (B36 Rule 16) | GAP |
| sprint | Not in any predicate lexicon | GAP |
| planning | Not in any predicate lexicon | neutral |
| session | Not in any predicate lexicon | neutral |

**Predicate output:** `[]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-standup-001
- gap-project-planning-capacity-001
- gap-project-planning-sprint-001

**KNOWLEDGE_EDGE state:** true
**FinalHumanResult:** null
**TransitionType:** unknown
**PhaseId:** null
**DomainId:** "project-management"
**recommended_action:** SCHEDULE_CAPABILITY_LEARNING

**Rules applied:**
- B36 Rule 14 — "capture" not in DOCUMENT lexicon
- B36 Rule 16 — "capacity" in team planning context is NOT ARCHITECTURE
- B36 Rule 18 — "run" in ceremony context is not in any predicate lexicon
- B36 Rule 22 — one SCHEDULE_CAPABILITY_LEARNING for the whole request

---

## B36-0A-006-002 — "review" would fire QA — showing contrast with "capture"

**Incorrect variant (do NOT produce this output):**
"Review team capacity for the sprint planning session."
→ Would fire ["QA"] because "review" fires QA

**Correct variant (B36-0A-006-001 above):**
"Capture team capacity for the sprint planning session."
→ "capture" is not in any predicate lexicon → `[]`

**Teaching point:** The choice of "capture" vs "review" is the entire difference between knowledge_edge and QA routing in a capacity planning sentence. The compiler is literal. "review" always fires QA. "capture" never fires any predicate.

---

## B36-0A-006-003 — "generate" in standup context: zero predicates

**Input request:**
"Generate a standup report and share the sprint capacity allocation with the team."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| generate | Not in any predicate lexicon (B36 Rule 13) | neutral |
| standup | Not in any predicate lexicon | GAP |
| report | Not in any predicate lexicon | neutral |
| share | Not in any predicate lexicon | neutral |
| sprint | Not in any predicate lexicon | GAP |
| capacity | team planning context → GAP | GAP |
| allocation | Not in any predicate lexicon | neutral |

**Predicate output:** `[]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-standup-001
- gap-project-planning-sprint-001
- gap-project-planning-capacity-001

**KNOWLEDGE_EDGE state:** true
**DomainId:** "project-management"

**Rule applied:** B36 Rule 13 — "generate" is not in any predicate lexicon.

---

## B36-0A-006-004 — compound gap: standup + retrospective + capacity

**Input request:**
"Facilitate the standup, plan the retrospective, and track team capacity across the sprint."

**Compiler token scan:**

| Token | Lexicon check | Result |
|-------|--------------|--------|
| facilitate | Not in any predicate lexicon | neutral |
| standup | Not in any predicate lexicon | GAP |
| plan | Not in any predicate lexicon | neutral |
| retrospective | Not in any predicate lexicon | GAP |
| track | Not in any predicate lexicon | neutral |
| team | Not in any predicate lexicon | neutral |
| capacity | team planning context → GAP | GAP |
| sprint | Not in any predicate lexicon | GAP |

**Predicate output:** `[]`

**CapabilityGapWarnings emitted:**
- gap-project-planning-standup-001
- gap-project-planning-retrospective-001
- gap-project-planning-capacity-001
- gap-project-planning-sprint-001
- Also: gap-project-planning-standup-retrospective-001 (compound gap ID)

**KNOWLEDGE_EDGE state:** true
**DomainId:** "project-management"
**recommended_action:** SCHEDULE_CAPABILITY_LEARNING (once, for the whole request)
