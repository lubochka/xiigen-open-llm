# Held-Out and Corruption Pairs (036)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-036-001 — HELD-OUT: "lean iterative — implement, skip tests, document the API" → IMPLEMENT + DOCUMENT

**Request:** "Lean iterative sprint — implement the feature, skip the tests, document the API."

**Expected output:**
```
Predicates: ["IMPLEMENT", "DOCUMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow, DocumentFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

**Reasoning:**
- "lean" + "iterative" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires ✓
- "skip the tests" → TEST negated
- "document the API" → DOCUMENT fires ✓

---

## B32-HO-036-002 — HELD-OUT: Round 3 complete qualifier + DomainId check

**Request:** "Expedient pragmatic barebones — just get it working, implement the handler."

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

**Key check:** DomainId must be "software-development" (not "project-management", not null).

---

## B32-HO-036-003 — CORRUPTION: "document" fires from negated "skip the docs" (WRONG)

**Corrupted output:**
```
Request: "Lean — implement, skip the docs."
Predicates: ["IMPLEMENT", "DOCUMENT"]
Reason: "docs" keyword appears in request
```

**Why this is wrong:**
- "skip the docs" is a negation expression → DOCUMENT does NOT fire
- Only positive imperative "document X" fires DOCUMENT
- v6-02 corruption variant: predicate fires from negated keyword

**Correct predicates:** ["IMPLEMENT"]

---

## B32-HO-036-004 — CORRUPTION: DomainId = "vibe-coding" (WRONG)

**Corrupted output:**
```
Request: "Lean iterative sprint."
DomainId: "vibe-coding"
Reason: request uses vibe vocabulary
```

**Why this is wrong:**
- DomainId is always "software-development" for B32 (v6-09 corruption)
- "vibe-coding" is not a valid DomainId
- The scenario name (B32 — Vibe coding gap) does not become the DomainId

**Correct DomainId:** "software-development"

---
