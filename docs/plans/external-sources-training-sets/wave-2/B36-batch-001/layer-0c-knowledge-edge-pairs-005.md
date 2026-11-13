# Layer 0-C — Knowledge-Edge Pairs (005)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B36-0C-005-001 — "generate", "capture", "run", "plan": none in any predicate lexicon

**Complete list of planning-context tokens that do NOT fire any predicate:**

| Token | Rule | Notes |
|-------|------|-------|
| generate | B36 Rule 13 | Not in any lexicon |
| capture | B36 Rule 14 | Not in DOCUMENT lexicon |
| run | B36 Rule 18 | Ceremony context |
| plan | B36 Rule 17 | Not in any lexicon |
| facilitate | B36 Rule 1 | Not in any lexicon |
| track | B36 Rule 1 | Not in any lexicon |
| prioritize | B36 Rule 1 | Not in any lexicon |
| groom | B36 Rule 1 | Not in any lexicon |
| assign | B36 Rule 1 | Not in any lexicon |
| schedule | B36 Rule 1 | Not in any lexicon |
| estimate | B36 Rule 15 | Gap vocabulary |

---

## B36-0C-005-002 — "generate" in planning sentences: never fires

**Test cases for "generate":**

| Request | Expected predicates |
|---------|---------------------|
| "Generate the sprint report." | [] — "generate" not in any lexicon |
| "Generate the burndown chart data." | [] — no predicates |
| "Generate a backlog summary." | [] — no predicates |
| "Generate and review the velocity data." | ["QA"] — "review" fires, "generate" does not |

**Anti-pattern:** Treating "generate" as similar to "create" or "build." "create" and "build" are in the IMPLEMENT lexicon. "generate" is not. They are different tokens with different lexicon registrations.

---

## B36-0C-005-003 — "capture" is not "document": distinct tokens

**"document" fires DOCUMENT. "capture" does not.**

| Request | Expected predicates |
|---------|---------------------|
| "Capture the sprint outcomes." | [] — "capture" not in DOCUMENT lexicon |
| "Document the sprint outcomes." | ["DOCUMENT"] — "document" fires |
| "Capture and document the sprint outcomes." | ["DOCUMENT"] — only "document" fires |

**Why this distinction exists:** "capture" means to record informally; "document" implies creating a formal document artifact. The compiler maintains this distinction by registering "document" (and its derivatives) in DOCUMENT lexicon while leaving "capture" unregistered.

---

## B36-0C-005-004 — "run" in ceremony context: not IMPLEMENT

**B36 Rule 18 context:** "run" can appear in software contexts ("run the tests", "run the service") where it might be expected to fire a predicate. In ceremony context it does not fire.

| Request | Expected predicates | Notes |
|---------|---------------------|-------|
| "Run the standup." | [] | Ceremony context |
| "Run the retrospective." | [] | Ceremony context |
| "Run the sprint." | [] | Sprint context |
| "Run the test suite." | ["TEST"] if TEST is a registered predicate | Software context |

**Key:** "run" alone does not fire any predicate. Context beyond "run" determines whether another token fires. In pure planning sentences, no token fires alongside "run."
