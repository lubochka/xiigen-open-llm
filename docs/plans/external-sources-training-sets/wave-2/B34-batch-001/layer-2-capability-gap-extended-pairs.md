# Layer 2 — Capability Gap Extended Pairs (replaces nine-node layer)
# Batch: B34-batch-001
# Scenario: B34 — Skills and guidance gap
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: capability_gap_extended

---

## Pair B34B1-WDA-gap-001 — skills and guidance vocabulary map

```
source_id: xiigen-b34-gap-001
pair_kind: capability_gap_extended
```

A: Identify all skills/guidance gap terms and their gap IDs.

B:
  GapVocabularyMap:
    - term: "mentor" / "mentoring"
      predicate_match: null
      gap_id: "gap-skills-guidance-mentor-001"
      notes: "Human-to-human knowledge transfer; no predicate"

    - term: "coach" / "coaching"
      predicate_match: null
      gap_id: "gap-skills-guidance-mentor-001"
      notes: "Skill development guidance role; no predicate"

    - term: "teach" / "teaching"
      predicate_match: null
      gap_id: "gap-skills-guidance-mentor-001"
      notes: "Instructional action; 'teach' ≠ DOCUMENT ≠ IMPLEMENT"

    - term: "guide" / "guidance" (as action verb or noun)
      predicate_match: null
      gap_id: "gap-skills-guidance-mentor-001"
      notes: "Neither the verb nor noun form fires any predicate"

    - term: "onboard" / "onboarding"
      predicate_match: null
      gap_id: "gap-skills-guidance-onboard-001"
      notes: "Developer ramp-up process; onboard ≠ IMPLEMENT"

    - term: "walkthrough" / "walk through"
      predicate_match: null
      gap_id: "gap-skills-guidance-onboard-001"
      notes: "Guided tour of a codebase or process; not DOCUMENT"

    - term: "tutorial"
      predicate_match: null
      gap_id: "gap-skills-guidance-onboard-001"
      notes: "Instructional content type; tutorial ≠ DOCUMENT unless paired with write/document"

    - term: "explain" (tutorial/teaching context)
      predicate_match: null
      gap_id: "gap-skills-guidance-mentor-001"
      notes: "Explanation as pedagogical act; explain ≠ DOCUMENT"

    - term: "train" / "training" (human learning)
      predicate_match: null
      gap_id: "gap-skills-guidance-train-001"
      notes: "Human skill training (not ML model training); no predicate"

    - term: "learning path" / "skill-building"
      predicate_match: null
      gap_id: "gap-skills-guidance-train-001"
      notes: "Developer growth framework terms; no predicate"

    - term: "TDD" (as practice abbreviation)
      predicate_match: null
      gap_id: "gap-skills-guidance-mentor-001"
      notes: "'TDD' as an abbreviation does not fire TEST; TEST fires on 'test' the word"

  DomainId_when_zero_predicates: "software-development"

---

## Pair B34B1-WDA-gap-002 — "explain authentication" does not fire SECURITY

```
source_id: xiigen-b34-gap-002
pair_kind: capability_gap_extended
```

A: "Explain the OAuth flow to the team." Does SECURITY fire?

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapSignal:
    gap_id: "gap-skills-guidance-mentor-001"
    DomainId: "software-development"

  Analysis:
    "explain" — gap vocabulary; no predicate
    "OAuth flow" — "OAuth" is a security term, but the action is "explain", not
      implement/enforce/audit/configure. SECURITY fires on security-action verbs,
      not on security-topic nouns in a teaching context.
    Result: zero predicates → knowledge_edge

  SECURITY predicate boundary:
    FIRES: "Implement OAuth", "Configure authentication", "Audit security controls",
           "Enforce access policies", "Review security vulnerabilities"
    DOES NOT FIRE: "Explain OAuth", "Teach authentication", "Walk through the security model"

---

## Pair B34B1-WDA-gap-003 — "guide" vs "document": critical boundary

```
source_id: xiigen-b34-gap-003
pair_kind: capability_gap_extended
```

A: "Guide the team through writing the API documentation." Does DOCUMENT fire?

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-skills-guidance-mentor-001: 'guide' is gap vocabulary"

  Analysis:
    "guide" — gap (guidance verb; no predicate)
    "writing the API documentation" — "writing" + "documentation" → DOCUMENT fires
      The object of the guidance is a documentation task, and the embedded action
      verb "writing" triggers DOCUMENT even though the outer frame is "guide".
    Result: ["DOCUMENT"] via writing + documentation

  Teaching: The predicate compiler parses all action verbs in the sentence.
  "Guide ... through writing the documentation" contains the embedded action "writing
  documentation" which fires DOCUMENT. The outer "guide" frame is gap vocabulary
  but does not suppress the inner action predicate.

---

## Pair B34B1-WDA-gap-004 — "TDD" abbreviation does not fire TEST

```
source_id: xiigen-b34-gap-004
pair_kind: capability_gap_extended
```

A: "Coach the team on TDD and help them practice test-first development." Does TEST fire?

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapSignal:
    gap_id: "gap-skills-guidance-mentor-001"
    DomainId: "software-development"

  Analysis:
    "coach" — gap
    "TDD" — abbreviation for Test-Driven Development; does not fire TEST predicate.
      TEST fires on the word "test" appearing directly in the request, not on abbreviations
      that contain the concept of testing.
    "test-first development" — "test" appears here! Does TEST fire?
      Yes, but: "test-first development" is a methodology description in a coaching
      context. The outer action verb is "coach" (gap) and "help them practice" (gap).
      Correction: "test" appears in "test-first" — TEST may fire.

  Correction:
    "test-first" contains "test" as a compound modifier. The predicate compiler
    evaluates "test" as the literal term. If the compiler finds "test" in "test-first",
    TEST fires as a weak signal.

  Corrected CompiledPredicateIds: ["TEST"] (weak signal from "test-first")
  CapabilityGapWarnings:
    - "gap-skills-guidance-mentor-001: 'coach', 'TDD', 'practice' are gap vocabulary"
  SourceStatus: "ready (weak signal)"

  Teaching: The literal presence of "test" (even as a compound modifier prefix) fires
  the TEST predicate. "TDD" as an abbreviation alone does not. This is a signal-strength
  boundary — the predicate fires on the literal term, not on conceptual synonyms.
