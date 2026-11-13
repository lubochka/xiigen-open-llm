# Held-Out and Corruption Pairs (022)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-022-001 — HELD-OUT: "build a dashboard screen" → IMPLEMENT + UI + gap-dashboard-001

**Request:** "Build a dashboard screen for the analytics."

**Expected output:**
```
Predicates: ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-dashboard-001]
RouteTarget: [ImplementationFlow, UIFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-HO-022-002 — CORRUPTION: "dashboard" fires UI (WRONG)

**Corrupted output:**
```
Request: "Build a dashboard."
Predicates: ["IMPLEMENT", "UI"]
Reason: dashboard = UI artifact
```

**Why this is wrong:**
- "dashboard" is NOT in UI lexicon → UI must NOT fire on "dashboard" alone
- Correct output: Predicates: ["IMPLEMENT"] + GapWarnings: [gap-vibe-coding-dashboard-001]
- UI fires only if one of the registered UI lexicon terms is present

**Correct predicates for "Build a dashboard":** ["IMPLEMENT"] + gap-dashboard-001 (no UI)

---

## B32-HO-022-003 — CORRUPTION: "dashboard screen" causes "dashboard" to be absorbed into UI (WRONG)

**Corrupted output:**
```
Request: "Build a dashboard screen."
Predicates: ["IMPLEMENT", "UI"]
GapWarnings: []  ← WRONG — gap-dashboard-001 missing
```

**Why this is wrong:**
- "screen" fires UI → correct
- "dashboard" still produces gap-vibe-coding-dashboard-001 → this warning must appear
- The presence of "screen" (UI) does not absorb "dashboard" into the UI predicate
- Both effects are independent and must both appear in the output

**Correct output:**
```
Predicates: ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-dashboard-001]
```

---
