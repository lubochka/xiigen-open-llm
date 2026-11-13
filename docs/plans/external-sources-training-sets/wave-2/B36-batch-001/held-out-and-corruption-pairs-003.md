# Held-Out and Corruption Pairs (003)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B36-HO-003-001 — HELD-OUT: "estimate story points" → DOCUMENT (false positive on "story")

**Input:** "Estimate the story points for the authentication epic."

**Expected output:**
- predicates: ["DOCUMENT"] ("story" fires DOCUMENT)
- KNOWLEDGE_EDGE: false
- DomainId: "software-development"
- CapabilityGapWarnings: estimation-001, story-point-001, epic-001
- TransitionType: documentation
- PhaseId: 2.1
- FinalHumanResult: populated by DocumentationFlow

**Why this is a held-out case:**
Tests whether the model correctly fires DOCUMENT on "story" in estimation context — the defined false positive. The model must output ["DOCUMENT"] despite the estimation intent.

---

## B36-HO-003-002 — Generalization test for B36-HO-003-001

The model generalizes correctly if it:
1. Fires DOCUMENT on "story" (lexicon match)
2. Does NOT output [] (which would suppress the false positive)
3. Emits story-point-001 as CapabilityGapWarning (estimation intent noted)
4. Routes to Phase 2.1 (not KNOWLEDGE_EDGE)

---

## B36-COR-003-001 — CORRUPTION: "story" suppressed; predicates: []

**Corrupt input/output (DO NOT REPRODUCE):**

```
Input: "Estimate the story points for the authentication epic."
Corrupt output: predicates: []
Corrupt reasoning: "The user means estimation (gap), not documentation. Suppressing DOCUMENT."
```

**Why this is wrong:**
"story" is in the DOCUMENT lexicon. Lexicon matches fire regardless of semantic context. Suppressing DOCUMENT because the context is estimation is v6-02 corruption.

**Correct output:**
- predicates: ["DOCUMENT"]
- DomainId: "software-development"
- CapabilityGapWarnings include story-point-001 (estimation intent noted)

---

## B36-COR-003-002 — Second corruption variant: omitting story-point-001 warning

**Corrupt output variant:**

```
Input: "Estimate the story points for the authentication epic."
predicates: ["DOCUMENT"]  ← correct
capabilityGapWarnings: []  ← WRONG (story-point-001 and estimation-001 missing)
```

Why this is wrong: Even though DOCUMENT fires and routing proceeds to Phase 2.1, the estimation intent (estimation-001) and story-point gap (story-point-001) must still appear in CapabilityGapWarnings. The model must emit both the predicate AND the gap warnings.
