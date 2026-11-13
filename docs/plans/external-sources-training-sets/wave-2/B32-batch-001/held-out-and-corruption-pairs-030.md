# Held-Out and Corruption Pairs (030)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-030-001 — HELD-OUT: "barebones design — architect the pipeline" → ARCHITECTURE + gap warning

**Request:** "Barebones design — architect the pipeline."

**Expected output:**
```
Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ArchitectureFlow31]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: documentation
```

---

## B32-HO-030-002 — HELD-OUT: "lean iterative — architect service, implement handler, define contract" → three predicates

**Request:** "Lean iterative — architect the service, implement the handler, define the contract."

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

## B32-HO-030-003 — CORRUPTION: "lean, agile, iterative" fires IMPLEMENT (WRONG)

**Corrupted output:**
```
Request: "Lean, agile, iterative approach."
Predicates: ["IMPLEMENT"]
Reason: "iterative" implies building incrementally
```

**Why this is wrong:**
- No action verb present; all three are style qualifiers
- None of lean/agile/iterative contains an action verb
- v6-02 corruption

**Correct predicates:** []

---

## B32-HO-030-004 — CORRUPTION: "architecture" noun fires ARCHITECTURE (WRONG)

**Corrupted output:**
```
Request: "We need a lean architecture."
Predicates: ["ARCHITECTURE"]
Reason: "architecture" keyword present
```

**Why this is wrong:**
- "architecture" is a noun; ARCHITECTURE requires an action verb
- "need" is not an ARCHITECTURE trigger
- v6-02 corruption

**Correct predicates:** []

---
