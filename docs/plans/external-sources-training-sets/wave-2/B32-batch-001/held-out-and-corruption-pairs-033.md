# Held-Out and Corruption Pairs (033)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-033-001 — HELD-OUT: "pragmatic — build it, skip tests, skip docs" → IMPLEMENT only

**Request:** "Pragmatic sprint — build it, skip the tests, skip the docs."

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

**Reasoning:** "build" → IMPLEMENT; "skip tests" → TEST negated; "skip docs" → DOCUMENT negated.

---

## B32-HO-033-002 — HELD-OUT: "no tests needed" alone → no predicates, no SCL

**Request:** "No tests needed — just ship it."

**Expected output:**
```
Predicates: []
GapWarnings: []
CapabilityAction: (none)
DomainId: "software-development"
```

---

## B32-HO-033-003 — CORRUPTION: "no tests needed" fires TEST (WRONG)

**Corrupted output:**
```
Request: "Good enough — no tests needed."
Predicates: ["TEST"]
Reason: "tests" keyword appears in request
```

**Why this is wrong:**
- "no tests needed" is a negation expression
- Negated vocabulary never fires its predicate
- v6-02 corruption variant: predicate fires from negated keyword

**Correct predicates:** []

---

## B32-HO-033-004 — CORRUPTION: negation suppresses IMPLEMENT (WRONG)

**Corrupted output:**
```
Request: "Lean sprint — implement feature, don't bother with tests."
Predicates: []
Reason: "don't bother" framing = casual/informal = knowledge_edge
```

**Why this is wrong:**
- "implement" is a positive action imperative → IMPLEMENT must fire
- "don't bother with tests" negates TEST only; it cannot suppress IMPLEMENT
- v6-02 corruption

**Correct predicates:** ["IMPLEMENT"]

---
