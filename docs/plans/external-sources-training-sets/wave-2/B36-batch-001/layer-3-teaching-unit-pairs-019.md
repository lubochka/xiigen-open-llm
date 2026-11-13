# Layer 3 — Teaching Unit Pairs (019)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B36-L3-019-001 — The "story" false positive: complete teaching

A:
  Question: "Why is DOCUMENT firing on 'story' called a false positive? Is it still correct?"

B:
  Answer: "It is called a false positive in intent terms because the developer writing
    'story points' means effort estimation units, not user stories (documentation).
    However, it is correct in lexicon terms because 'story' is in the DOCUMENT lexicon.
    The compiler fires on term presence, not on intent. The routing to Phase 2.1 (user
    stories sub-phase) is the lexicon-correct output even though the developer wanted
    effort estimation. This is correct behavior. The model must never suppress DOCUMENT
    to preserve the 'correct' intent."

---

## B36-L3-019-002 — "story" vs "stories" vs "story points": all fire DOCUMENT

A:
  Question: "Do 'story', 'stories', and 'story points' all fire DOCUMENT?"

B:
  Answer: "Yes. All three fire DOCUMENT via the 'story' substring:
    - 'story' → DOCUMENT (direct match)
    - 'stories' → DOCUMENT ('story' substring present)
    - 'story points' → DOCUMENT ('story' substring present)
    - 'user story' → DOCUMENT ('story' substring present)
    The DOCUMENT lexicon matches on 'story' as a token/substring. All forms fire."

---

## B36-L3-019-003 — "story" false positive changes DomainId

A:
  Question: "'Estimate story points for the backlog.' What is the DomainId? Why?"

B:
  Answer: "DomainId: 'software-development'. Because DOCUMENT fires on 'story'.
    When any predicate fires (even via false positive), DomainId: 'software-development'.
    The developer may have project management intent, but the predicate drives DomainId.
    DomainId: 'project-management' is ONLY possible when CompiledPredicateIds: []."

---

## B36-L3-019-004 — Preventing false-positive suppression: the hard rule

A:
  Question: "A model produces: CompiledPredicateIds: [] for 'Estimate story points for backlog.'
             arguing that the intent is estimation, not documentation. Is this correct?"

B:
  Answer: "No. This is a v6-02 corruption: suppressing a predicate because the sentence
    'feels like planning'. The DOCUMENT predicate fires on 'story' unconditionally.
    Suppressing it to preserve knowledge-edge is incorrect. The model must:
    1. Fire DOCUMENT on 'story'
    2. Emit gap warning for story-point estimation intent
    3. Route to Phase 2.1
    The intent is noted in the gap warning, not by suppressing the predicate."
