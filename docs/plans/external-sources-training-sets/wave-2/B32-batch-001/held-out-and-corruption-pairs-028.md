# Held-Out and Corruption Pairs (028)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-028-001 — HELD-OUT: "lean iterative approach — secure the auth module" → SECURITY + gap warning

**Request:** "Lean iterative approach — secure the auth module."

**Expected output:**
```
Predicates: ["SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [SecurityFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: no_contribution
```

**Reasoning:**
- "lean" → gap-vibe-coding-style-001
- "iterative" → gap-vibe-coding-style-001 (same ID)
- "secure" → SECURITY fires
- "auth module" → scope

---

## B32-HO-028-002 — HELD-OUT: "iterative build — implement dashboard and test endpoints" → IMPLEMENT + TEST + two gap IDs

**Request:** "Iterative build — implement the dashboard and test the endpoints."

**Expected output:**
```
Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]
RouteTarget: [ImplementationFlow, TestFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

---

## B32-HO-028-003 — CORRUPTION: "iterative" fires IMPLEMENT (WRONG)

**Corrupted output:**
```
Request: "Let's be iterative about this."
Predicates: ["IMPLEMENT"]
Reason: "iterative" implies incremental implementation
```

**Why this is wrong:**
- "iterative" is a style qualifier; no action verb present
- Inferring IMPLEMENT from a style adjective is v6-02 corruption

**Correct predicates:** []

---

## B32-HO-028-004 — CORRUPTION: dashboard fires UI predicate (WRONG)

**Corrupted output:**
```
Request: "Iterative sprint — implement the dashboard."
Predicates: ["IMPLEMENT", "UI"]
Reason: "dashboard" is UI context
```

**Why this is wrong:**
- "dashboard" is explicitly NOT in the UI predicate lexicon
- "dashboard" always maps to gap-vibe-coding-dashboard-001
- UI fires only from "screen", "interface", or direct UI action verbs

**Correct predicates:** ["IMPLEMENT"]

---
