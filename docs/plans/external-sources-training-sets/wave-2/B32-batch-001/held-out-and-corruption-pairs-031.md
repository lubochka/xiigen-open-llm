# Held-Out and Corruption Pairs (031)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-031-001 — HELD-OUT: "lean barebones — implement, test, QA the release" → IMPLEMENT + TEST + QA

**Request:** "Lean barebones — implement the module, test it, and QA the release."

**Expected output:**
```
Predicates: ["IMPLEMENT", "TEST", "QA"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow, TestFlow, QAFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

---

## B32-HO-031-002 — HELD-OUT: "expedient pragmatic no-frills" alone → zero predicates

**Request:** "Expedient, pragmatic, no-frills approach."

**Expected output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
TransitionType: unknown
```

---

## B32-HO-031-003 — CORRUPTION: gap suppresses SECURITY from multi-predicate request (WRONG)

**Corrupted output:**
```
Request: "Pragmatic lean — implement, test, secure the endpoint."
Predicates: ["IMPLEMENT", "TEST"]
Reason: "pragmatic lean" framing = style-only → SECURITY suppressed as over-engineering
```

**Why this is wrong:**
- "secure the endpoint" is a positive imperative → SECURITY must fire
- Gap qualifiers cannot suppress SECURITY (Rule 3; v6-02 corruption)

**Correct predicates:** ["IMPLEMENT", "TEST", "SECURITY"]

---

## B32-HO-031-004 — CORRUPTION: six qualifiers fire six SCL events (WRONG)

**Corrupted output:**
```
Request: "Expedient, pragmatic, lean, barebones, stripped-down, no-frills."
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING × 6
Reason: one SCL per gap qualifier
```

**Why this is wrong:**
- SCL emits once per request regardless of qualifier count
- All six map to same gap ID; even distinct gap IDs produce one SCL

**Correct:** SCHEDULE_CAPABILITY_LEARNING × 1

---
