# Held-Out and Corruption Pairs (043)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-043-001 — HELD-OUT: complex multi-gap multi-predicate request

**Request:** "Lean pragmatic spike — implement the module, write tests, define the API contract."

**Expected output:**
```
Predicates: ["IMPLEMENT", "TEST", "API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001]
RouteTarget: [ImplementationFlow, TestFlow, APIContractFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

---

## B32-HO-043-002 — HELD-OUT: vibe coding request with multiple negations

**Request:** "Expedient — implement the handler, skip tests, skip docs, skip security."

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

## B32-HO-043-003 — CORRUPTION: v6-02 + v6-07 combined (WRONG)

**Corrupted output:**
```
Request: "Lean pragmatic spike — implement the module."
Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Reason: "lean pragmatic spike" = heavy qualifier framing = no predicates; wrong ID for spike
```

**Why this is wrong:**
- "implement" is a positive imperative → IMPLEMENT fires (v6-02: predicate suppressed)
- "spike" → gap-vibe-coding-spike-001 is missing from GapWarnings (v6-07: wrong/incomplete gap IDs)

**Correct predicates:** ["IMPLEMENT"]
**Correct GapWarnings:** [gap-vibe-coding-style-001, gap-vibe-coding-spike-001]

---

## B32-HO-043-004 — CORRUPTION: v6-06 + v6-03 combined (WRONG)

**Corrupted output:**
```
Request: "Expedient — implement handler, skip tests."
Predicates: ["IMPLEMENT", "TEST"]
DomainId: "project-management"
Reason: "skip tests" mentioned → TEST fires; "expedient sprint" = PM context
```

**Why this is wrong:**
- v6-06: "skip tests" is negation → TEST should NOT fire
- v6-03: DomainId must be "software-development"

**Correct predicates:** ["IMPLEMENT"]
**Correct DomainId:** "software-development"

---
