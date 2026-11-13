# Layer 3 — Teaching Unit Pairs (003)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B36-L3-003-001 — "story" fires DOCUMENT: false positives are correct outputs

**Teaching objective:** Model learns that "story" fires DOCUMENT and must NOT suppress this output even when the context is project planning.

**The false positive:**
- Token: "story" (in "user story", "user stories", "story points", "story mapping")
- Lexicon match: DOCUMENT
- User intent: usually NOT documentation (usually estimation or work organization)
- Required output: include DOCUMENT in predicate list anyway

---

## B36-L3-003-002 — Teaching pairs

**Pair A:**

```
Input: "Write the user stories for the checkout module."
Expected predicates: ["IMPLEMENT", "DOCUMENT"]
  - "write" → IMPLEMENT
  - "stories" (story substring) → DOCUMENT
  - "module" → IMPLEMENT
```

**Pair B:**

```
Input: "Estimate the story points for the backlog."
Expected predicates: ["DOCUMENT"]
  - "story" → DOCUMENT (false positive on estimation intent)
  - "estimate" → GAP (not a predicate)
  - "backlog" → GAP
```

**Pair C:**

```
Input: "Map the user stories to epic E-3."
Expected predicates: ["DOCUMENT"]
  - "stories" (story substring) → DOCUMENT
  - "epic" → GAP
```

---

## B36-L3-003-003 — Why the model must NOT suppress

**The suppression temptation:**
The model understands that "story points" means estimation, not documentation. It might be tempted to output [] instead of ["DOCUMENT"] to "help" the user by redirecting to KNOWLEDGE_EDGE.

**Why suppression is wrong:**
1. It violates the literal lexicon contract
2. It introduces inconsistency — sometimes DOCUMENT fires, sometimes it doesn't, depending on context
3. It is exactly v6-02 corruption: predicates suppressed based on semantic reasoning
4. The training data expects ["DOCUMENT"] — a model that suppresses it will fail evaluations

**The correct mental model:** False positives are documented limitations. They are correct outputs. The CapabilityGapWarning (story-point-001) documents the mismatch between the literal DOCUMENT firing and the user's estimation intent.

---

## B36-L3-003-004 — False positive vs false negative

**False positive (correct behavior):** DOCUMENT fires on "story" even though user meant estimation. Model outputs ["DOCUMENT"]. This is correct.

**False negative (wrong behavior):** DOCUMENT suppressed for "story" because context is estimation. Model outputs []. This is v6-02 corruption.

The terminology here is from the perspective of the user's intent. The model's job is not to serve the user's intent — it is to apply the literal lexicon contract. False positives in that contract are intentional.
