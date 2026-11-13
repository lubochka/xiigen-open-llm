# Held-Out and Corruption Pairs (032)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-032-001 — HELD-OUT: "expedient sprint — implement handler and secure the API" → IMPLEMENT + SECURITY

**Request:** "Expedient sprint — implement the handler and secure the API."

**Expected output:**
```
Predicates: ["IMPLEMENT", "SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow, SecurityFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

---

## B32-HO-032-002 — HELD-OUT: "hack together security fix — secure login form" → SECURITY + hack gap ID

**Request:** "Hack together a security fix — secure the login form."

**Expected output:**
```
Predicates: ["SECURITY"]
GapWarnings: [gap-vibe-coding-hack-001]
RouteTarget: [SecurityFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: no_contribution
```

**Key:** Gap ID is hack-001, not style-001.

---

## B32-HO-032-003 — CORRUPTION: "think about security" fires SECURITY (WRONG)

**Corrupted output:**
```
Request: "Lean approach — think about security."
Predicates: ["SECURITY"]
Reason: "security" keyword present
```

**Why this is wrong:**
- "think about" is a cognitive phrase; not in SECURITY trigger lexicon
- SECURITY requires imperative verb "secure" or "audit"
- v6-02 corruption

**Correct predicates:** []

---

## B32-HO-032-004 — CORRUPTION: "hack together" uses style-001 gap ID (WRONG)

**Corrupted output:**
```
Request: "Hack together a security fix — secure the login form."
GapWarnings: [gap-vibe-coding-style-001]
Reason: defaulting to style qualifier ID
```

**Why this is wrong:**
- "hack together" maps to gap-vibe-coding-hack-001, not gap-vibe-coding-style-001
- Gap IDs must match the actual vocabulary; incorrect ID = corrupted output

**Correct GapWarnings:** [gap-vibe-coding-hack-001]

---
