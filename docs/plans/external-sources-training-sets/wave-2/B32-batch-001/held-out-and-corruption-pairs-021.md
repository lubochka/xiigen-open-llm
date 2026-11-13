# Held-Out and Corruption Pairs (021)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-021-001 — HELD-OUT: "vibe code a screen" → IMPLEMENT + UI; gap warning

**Request:** "Vibe code a screen for the user profile."

**Expected output:**
```
Predicates: ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow, UIFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-HO-021-002 — CORRUPTION: "vibe code" treated as pure gap; predicates = [] (WRONG)

**Corrupted output:**
```
Request: "Vibe code a screen for the user profile."
Predicates: []
Reason: "vibe code" = informal; no formal predicate
```

**Why this is wrong:**
- "code" in "vibe code" contains the "code" root → IMPLEMENT fires
- "screen" is in UI lexicon → UI fires
- "vibe" alone is gap, but "vibe code" contains "code" root → IMPLEMENT cannot be suppressed
- v6-02 corruption

**Correct predicates:** ["IMPLEMENT", "UI"]

---

## B32-HO-021-003 — HELD-OUT: "vibe out a screen" → UI only; IMPLEMENT absent (no code root)

**Request:** "Vibe out a screen for the settings."

**Expected output:**
```
Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: UIFlow
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

**Reasoning:** "vibe out" does not contain "code" root; IMPLEMENT does not fire. "screen" fires UI.

---
