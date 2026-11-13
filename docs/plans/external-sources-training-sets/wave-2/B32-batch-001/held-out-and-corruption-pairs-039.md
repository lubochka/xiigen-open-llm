# Held-Out and Corruption Pairs (039)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-039-001 — HELD-OUT: "throw together a UI for the settings screen" → UI + hack gap

**Request:** "Throw together a UI for the settings screen."

**Expected output:**
```
Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-hack-001]
RouteTarget: [UIFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

---

## B32-HO-039-002 — HELD-OUT: "lean — build the dashboard screen" → IMPLEMENT + UI + dashboard gap

**Request:** "Lean sprint — build the dashboard screen."

**Expected output:**
```
Predicates: ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]
RouteTarget: [ImplementationFlow, UIFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

"screen" fires UI; "dashboard" adds gap-vibe-coding-dashboard-001 to GapWarnings.

---

## B32-HO-039-003 — CORRUPTION: "lean UI" fires UI predicate (WRONG)

**Corrupted output:**
```
Request: "Lean UI."
Predicates: ["UI"]
Reason: "UI" keyword present
```

**Why this is wrong:**
- "UI" is a noun here; no action verb present
- UI predicate requires an action verb
- v6-02 corruption

**Correct predicates:** []

---

## B32-HO-039-004 — CORRUPTION: "dashboard" fires UI predicate (WRONG)

**Corrupted output:**
```
Request: "Lean sprint — build the dashboard."
Predicates: ["IMPLEMENT", "UI"]
Reason: "dashboard" is a UI component
```

**Why this is wrong:**
- "dashboard" is not in the UI predicate lexicon (Rule 4)
- "dashboard" always maps to gap-vibe-coding-dashboard-001 only
- v6-08 corruption

**Correct predicates:** ["IMPLEMENT"]
**Correct GapWarnings:** [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]

---
