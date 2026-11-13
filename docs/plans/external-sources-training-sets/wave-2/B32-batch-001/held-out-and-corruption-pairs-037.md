# Held-Out and Corruption Pairs (037)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-037-001 — HELD-OUT: "expedient — implement handler, skip tests, skip docs" → IMPLEMENT only

**Request:** "Expedient sprint — implement the handler, skip the tests, skip the docs."

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

---

## B32-HO-037-002 — HELD-OUT: "build it, no tests needed" (no gap vocab) → IMPLEMENT only, no gap warning

**Request:** "Build it, no tests needed."

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: []
RouteTarget: [ImplementationFlow]
DomainId: "software-development"
CapabilityGapWarning: (absent)
TransitionType: feature_addition
```

No CapabilityGapWarning because no gap vocabulary present.

---

## B32-HO-037-003 — CORRUPTION: "skip tests" adds gap ID (WRONG)

**Corrupted output:**
```
Request: "Lean sprint — build the feature, skip the tests."
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-test-001]
Reason: "skip tests" signals test awareness gap
```

**Why this is wrong:**
- Negation patterns never add gap IDs
- There is no registered gap-vibe-coding-test-001 in B32
- "skip tests" is a negation; it only suppresses TEST predicate

**Correct GapWarnings:** [gap-vibe-coding-style-001]

---

## B32-HO-037-004 — CORRUPTION: "no gap vocabulary" request emits SCL (WRONG)

**Corrupted output:**
```
Request: "Build the service, no tests needed."
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
Reason: "no tests" signals capability gap
```

**Why this is wrong:**
- SCL only emits when gap vocabulary is present
- "no tests needed" is a negation, not gap vocabulary
- No gap vocabulary in this request → no SCL

**Correct:** CapabilityAction absent (no gap vocabulary).

---
