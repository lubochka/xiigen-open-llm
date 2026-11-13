# Held-Out and Corruption Pairs (040)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-040-001 — HELD-OUT: "throwaway tests — write unit tests for the handler" → TEST + style gap

**Request:** "Write throwaway unit tests for the payment handler."

**Expected output:**
```
Predicates: ["TEST"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [TestFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: no_contribution
```

---

## B32-HO-040-002 — HELD-OUT: "lean approach — testing would be nice" → knowledge_edge + SCL

**Request:** "Lean approach — some testing would be nice."

**Expected output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
TransitionType: unknown
```

---

## B32-HO-040-003 — CORRUPTION: "testing would be nice" fires TEST (WRONG)

**Corrupted output:**
```
Request: "Lean approach — some testing would be nice."
Predicates: ["TEST"]
Reason: "testing" appears in request
```

**Why this is wrong:**
- "testing would be nice" is aspirational; not an imperative
- TEST fires from imperatives only
- v6-02 corruption variant: predicate fires from aspirational vocabulary

**Correct predicates:** []

---

## B32-HO-040-004 — CORRUPTION: "throwaway" suppresses TEST (WRONG)

**Corrupted output:**
```
Request: "Write throwaway unit tests for the payment handler."
Predicates: []
Reason: "throwaway" = vibe framing = no real tests
```

**Why this is wrong:**
- "write tests" is a positive imperative → TEST fires
- "throwaway" is a gap qualifier; cannot suppress TEST (v6-02 corruption)

**Correct predicates:** ["TEST"]

---
