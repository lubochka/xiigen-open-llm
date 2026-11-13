# Held-Out and Corruption Pairs (035)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-035-001 — HELD-OUT: "bang out an implementation and write tests" → IMPLEMENT + TEST + hack gap

**Request:** "Bang out an implementation and write some tests."

**Expected output:**
```
Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-hack-001]
RouteTarget: [ImplementationFlow, TestFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

---

## B32-HO-035-002 — HELD-OUT: "throw together UI for the settings screen" → UI + hack gap

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

## B32-HO-035-003 — CORRUPTION: "whip up" assigns style-001 gap ID (WRONG)

**Corrupted output:**
```
Request: "Whip up a quick implementation of the search."
GapWarnings: [gap-vibe-coding-style-001]
Reason: defaulting to style qualifier ID
```

**Why this is wrong:**
- "whip up" is an informal action phrase → gap-vibe-coding-hack-001
- style-001 is for quality-bar adjectives (lean, pragmatic, expedient)
- Gap ID must match actual vocabulary

**Correct GapWarnings:** [gap-vibe-coding-hack-001]

---

## B32-HO-035-004 — CORRUPTION: "dashboard" in "dashboard settings screen" fires UI (WRONG)

**Corrupted output:**
```
Request: "Throw together UI for the dashboard settings screen."
Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-hack-001]
Note: "dashboard" treated as UI context only
```

**Why this is wrong:**
- "dashboard" must also add gap-vibe-coding-dashboard-001 to GapWarnings
- Both "dashboard" gap ID and "screen" UI predicate apply independently

**Correct GapWarnings:** [gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001]

---
