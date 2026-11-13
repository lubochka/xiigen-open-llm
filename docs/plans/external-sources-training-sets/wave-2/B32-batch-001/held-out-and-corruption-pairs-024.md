# Held-Out and Corruption Pairs (024)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-024-001 — HELD-OUT: "rough architecture, build it scrappy, no tests needed" → ARCHITECTURE + IMPLEMENT

**Request:** "Rough architecture, build it scrappy, no tests needed."

**Expected output:**
```
Predicates: ["ARCHITECTURE", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ArchitectureFlow31, ImplementationFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

**Reasoning:**
- "rough" alone → no registered gap ID
- "architecture" → ARCHITECTURE fires
- "build" → IMPLEMENT fires
- "scrappy" → gap-vibe-coding-style-001
- "no tests needed" → style preference; TEST does NOT fire from a negated mention

---

## B32-HO-024-002 — CORRUPTION: "no tests needed" fires TEST (WRONG)

**Corrupted output:**
```
Request: "Rough architecture, build it scrappy, no tests needed."
Predicates: ["ARCHITECTURE", "IMPLEMENT", "TEST"]
Reason: "tests" appears in request
```

**Why this is wrong:**
- "no tests needed" is a negation — the user is explicitly NOT requesting tests
- TEST fires only when "test", "unit test" appears as a positive action imperative
- Negated mentions do not fire predicates
- v6-02 variant: predicate fires from negated vocabulary

**Correct predicates:** ["ARCHITECTURE", "IMPLEMENT"] — no TEST

---

## B32-HO-024-003 — CORRUPTION: "no tests needed" suppresses IMPLEMENT (WRONG)

**Corrupted output:**
```
Predicates: ["ARCHITECTURE"]  ← IMPLEMENT missing
Reason: "no tests needed, scrappy" = informal = only architecture matters
```

**Why this is wrong:**
- "build" is in IMPLEMENT lexicon → IMPLEMENT must fire regardless of "scrappy" framing
- "scrappy" is a gap warning only; it cannot suppress IMPLEMENT
- v6-02 corruption

**Correct predicates:** ["ARCHITECTURE", "IMPLEMENT"]

---
