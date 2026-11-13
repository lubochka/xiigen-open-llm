# Held-Out and Corruption Pairs (020)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-020-001 — HELD-OUT: multiple gap qualifiers + single action verb → single predicate + multiple warnings

**Request:** "Quick and dirty, throwaway, nothing fancy — just build the checkout flow."

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-quick-dirty-001, gap-vibe-coding-throwaway-001, gap-vibe-coding-nothing-fancy-001]
RouteTarget: ImplementationFlow
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-HO-020-002 — CORRUPTION: multiple gap qualifiers suppress predicate (WRONG)

**Corrupted output:**
```
Request: "Quick and dirty, throwaway, nothing fancy — build the checkout flow."
Predicates: []
Reason: three gap qualifiers signal highly informal → no formal routing needed
```

**Why this is wrong:**
- "build" is in IMPLEMENT lexicon → IMPLEMENT must fire
- The number of gap qualifiers has NO effect on whether predicates fire
- Three gap warnings do not overpower one action verb
- v6-02 corruption (most severe form: multiple qualifiers weaponized as suppressors)

**Correct predicates:** ["IMPLEMENT"]

---

## B32-HO-020-003 — CORRUPTION: SCHEDULE_CAPABILITY_LEARNING emitted when predicates non-empty (WRONG)

**Corrupted output:**
```
Request: "Build the checkout flow — quick and dirty."
Predicates: ["IMPLEMENT"]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING  ← WRONG
```

**Why this is wrong:**
- SCHEDULE_CAPABILITY_LEARNING is emitted ONLY when predicates = []
- When predicates are non-empty, CapabilityGapWarning: style_qualifier_noted is the correct field
- SCHEDULE_CAPABILITY_LEARNING must not appear alongside non-empty predicates

---
