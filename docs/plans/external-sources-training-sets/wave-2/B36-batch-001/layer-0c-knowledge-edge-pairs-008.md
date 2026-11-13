# Layer 0-C — Knowledge-Edge Pairs (008)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B36-0C-008-001 — "story" fires DOCUMENT: false positive explained

**B36 Rule 2:** "story" (as standalone term or as substring in "stories") fires DOCUMENT. This is defined as a false positive: the user intended "story points" or "user stories" in the context of estimation, but the compiler fires DOCUMENT because "story" is in the DOCUMENT lexicon.

**Why it fires:**
The DOCUMENT lexicon contains "story" as a token. The compiler matches tokens literally. "story" in "user story", "user stories", "story points", "story mapping" all match.

---

## B36-0C-008-002 — why the model must NOT suppress DOCUMENT for "story"

**The model must not override the false positive.** Even though the model understands that "story points" is about estimation (gap vocabulary), not documentation, the model must output ["DOCUMENT"] (or include DOCUMENT in the predicate list).

**Reasons:**
1. The compiler contract is literal. Semantic reasoning happens downstream, not at the compiler stage.
2. Suppressing a predicate based on semantic context is v6-02 corruption.
3. The training data consistently shows DOCUMENT firing for "story". Suppressing it would create inconsistency.
4. False positives are intentional features: they document known lexicon limitations and teach the model that literal matching precedes semantic interpretation.

---

## B36-0C-008-003 — "story" false positive trace

**Input:** "Estimate the story points for the backlog items."

**Token analysis:**
- estimate: GAP (estimation-001)
- story: DOCUMENT lexicon — fires DOCUMENT
- points: no predicate
- backlog: GAP (backlog-001)
- items: no predicate

**Result:**
- predicates: ["DOCUMENT"]
- CapabilityGapWarnings: story-point-001, estimation-001, backlog-001
- DomainId: "software-development" (DOCUMENT fired)
- KNOWLEDGE_EDGE: false

**Teaching point:** The user's intent is estimation (gap), not documentation. But "story" fires DOCUMENT. The routing goes to Phase 2.1 (documentation), not to KNOWLEDGE_EDGE. This is the correct behavior — the false positive is correct output.

---

## B36-0C-008-004 — CapabilityGapWarning for story-point-001

The gap ID gap-project-planning-story-point-001 is emitted as a CapabilityGapWarning whenever "story points" vocabulary appears, even when DOCUMENT fires on "story." The gap warning records that the user's estimation intent is not being fully served by the DOCUMENT routing. This information flows to AutonomousLearningPipeline for future capability planning.

**Both happen simultaneously:**
- DOCUMENT fires (lexicon match on "story")
- gap-project-planning-story-point-001 emitted (estimation intent noted)
- Routing: Phase 2.1 (DOCUMENT)
- AutonomousLearningPipeline: notified of story-point gap

These are not contradictory — they reflect different concerns at different system layers.
