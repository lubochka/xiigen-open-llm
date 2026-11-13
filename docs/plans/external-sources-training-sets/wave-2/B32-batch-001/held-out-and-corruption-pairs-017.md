# Held-Out and Corruption Pairs (017)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-017-001 — HELD-OUT: "low-fi build + page" → IMPLEMENT + UI; gap warning

**Request:** "Build a low-fi version of the settings page."

**Expected output:**
```
Predicates: ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow, UIFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-HO-017-002 — CORRUPTION: "low-fi" suppresses UI (WRONG)

**Corrupted output:**
```
Request: "Build a low-fi version of the settings page."
Predicates: ["IMPLEMENT"]  ← UI missing
Reason: low-fi framing = wireframe-level = UI not needed
```

**Why this is wrong:**
- "page" is in UI lexicon → UI must fire regardless of "low-fi" framing
- "low-fi" is a quality modifier → warning only; never suppresses UI
- This is v6-02 corruption applied to UI predicate

**Correct predicates:** ["IMPLEMENT", "UI"]

---

## B32-HO-017-003 — CORRUPTION: "lo-fi" treated as different from "low-fi" (WRONG)

**Corrupted output:**
```
Request: "Lo-fi implementation."
GapWarnings: []  ← WRONG — gap warning missing
OR
GapWarnings: [gap-vibe-coding-lo-fi-001]  ← WRONG — non-existent gap ID
```

**Why this is wrong:**
- "lo-fi" and "low-fi" both map to gap-vibe-coding-style-001
- There is no separate "gap-vibe-coding-lo-fi-001"
- The gap warning must be present

**Correct output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
```

---
