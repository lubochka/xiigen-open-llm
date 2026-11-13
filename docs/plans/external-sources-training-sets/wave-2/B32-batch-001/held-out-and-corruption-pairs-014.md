# Held-Out and Corruption Pairs (014)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-014-001 — HELD-OUT: "rough cut design" → ARCHITECTURE fires; rough-cut is warning

**Request:** "Rough cut design for the distributed system architecture."

**Expected output:**
```
Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-rough-cut-001]
RouteTarget: ArchitectureFlow31
TransitionType: documentation
DomainId: "software-development"
```

---

## B32-HO-014-002 — CORRUPTION: "rough cut" treated as zero predicates (WRONG)

**Corrupted output:**
```
Predicates: []
Reason: "rough cut" signals informal/exploratory → no formal design requested
```

**Why this is wrong:**
- "design" is in ARCHITECTURE lexicon → ARCHITECTURE must fire
- "rough cut" is a quality modifier → gap warning only
- "architecture" further confirms ARCHITECTURE should fire
- This is v6-02 corruption

**Correct predicates:** ["ARCHITECTURE"]

---

## B32-HO-014-003 — CORRUPTION: "rough-cut" and "rough cut" treated as different classifications (WRONG)

**Corrupted output:**
```
Request: "Rough-cut implementation"
Predicates: [IMPLEMENT]  ← correct
GapWarnings: []  ← WRONG — gap-vibe-coding-rough-cut-001 missing
```

**Why this is wrong:**
- "rough-cut" (hyphenated) maps to gap-vibe-coding-rough-cut-001
- Both "rough cut" and "rough-cut" must produce the same gap ID
- The gap warning must be present

**Correct output:**
```
Predicates: [IMPLEMENT]
GapWarnings: [gap-vibe-coding-rough-cut-001]
```

---
