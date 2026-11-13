# Held-Out and Corruption Pairs (034)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-034-001 — HELD-OUT: "just get it working — implement auth" → IMPLEMENT + gap warning

**Request:** "Just get it working — implement the auth flow."

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

## B32-HO-034-002 — HELD-OUT: "lean — implement, skip QA, skip docs" → IMPLEMENT only

**Request:** "Lean sprint — implement the module, skip QA, skip the docs."

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-HO-034-003 — CORRUPTION: "just get it working" fires IMPLEMENT (WRONG)

**Corrupted output:**
```
Request: "Just get it working."
Predicates: ["IMPLEMENT"]
Reason: "get it working" implies implementation
```

**Why this is wrong:**
- "get it working" is a vibe-code phrase; not in the IMPLEMENT trigger lexicon
- Only explicit "implement", "build", "create", "write", "code" fire IMPLEMENT
- v6-02 corruption

**Correct predicates:** []

---

## B32-HO-034-004 — CORRUPTION: "skip the QA" fires QA negated as gap ID (WRONG)

**Corrupted output:**
```
Request: "Pragmatic — implement feature, skip the QA."
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-qa-001]
Reason: "skip QA" signals QA gap awareness
```

**Why this is wrong:**
- "skip the QA" is a negation pattern; it adds no gap ID to GapWarnings
- There is no registered gap-vibe-coding-qa-001 in B32
- Negation patterns are invisible to the gap vocabulary scanner

**Correct GapWarnings:** [gap-vibe-coding-style-001]

---
