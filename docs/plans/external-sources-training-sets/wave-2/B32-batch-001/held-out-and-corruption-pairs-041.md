# Held-Out and Corruption Pairs (041)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-041-001 — HELD-OUT: "lean — test and document" → TEST + DOCUMENT + documentation type

**Request:** "Lean sprint — write tests and document the API."

**Expected output:**
```
Predicates: ["TEST", "DOCUMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [TestFlow, DocumentFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: documentation
```

---

## B32-HO-041-002 — HELD-OUT: "pragmatic — architect, implement, define contract" → three predicates

**Request:** "Pragmatic sprint — architect the system, implement the core, define the API contract."

**Expected output:**
```
Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ArchitectureFlow31, ImplementationFlow, APIContractFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

---

## B32-HO-041-003 — CORRUPTION: "lean" suppresses TEST from multi-predicate (WRONG)

**Corrupted output:**
```
Request: "Lean sprint — implement, write tests, document the API."
Predicates: ["IMPLEMENT", "DOCUMENT"]
Reason: "lean" framing = only essential predicates; TEST is not essential
```

**Why this is wrong:**
- "write tests" is a positive imperative → TEST fires
- "lean" cannot suppress any predicate (Rule 3; v6-02 corruption)

**Correct predicates:** ["IMPLEMENT", "TEST", "DOCUMENT"]

---

## B32-HO-041-004 — CORRUPTION: TransitionType = documentation for [IMPLEMENT, TEST, DOCUMENT] (WRONG)

**Corrupted output:**
```
Request: "Lean sprint — implement, write tests, document the API."
TransitionType: documentation
Reason: DOCUMENT and TEST suggest documentation focus
```

**Why this is wrong:**
- IMPLEMENT is present → feature_addition always takes precedence
- documentation type requires absence of IMPLEMENT and UI

**Correct TransitionType:** feature_addition

---
