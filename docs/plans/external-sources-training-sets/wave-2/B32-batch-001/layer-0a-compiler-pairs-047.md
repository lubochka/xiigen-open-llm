# Layer 0-A — Compiler Pairs (047)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-047-001 — informal + IMPLEMENT: "whip up" pattern

**Request:** "Whip up a quick implementation of the search."

```
Token scan:
"whip up" → gap-vibe-coding-hack-001 (informal action phrase; similar to "hack together")
"implementation" → IMPLEMENT fires ✓ (noun form of implement; triggers predicate)
"search" → scope

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-047-002 — informal + UI: "throw together" + screen

**Request:** "Throw together a UI for the dashboard settings screen."

```
Token scan:
"throw together" → gap-vibe-coding-hack-001
"UI" → context noun
"dashboard" → gap-vibe-coding-dashboard-001 (dashboard is never UI)
"settings screen" → UI fires ✓ (screen → UI predicate)

Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

Note: "settings screen" fires UI. "Dashboard" still adds gap-vibe-coding-dashboard-001 even though screen is present.

---

## B32-0A-047-003 — informal + TEST: "bang out" pattern

**Request:** "Bang out some tests for the payment module."

```
Token scan:
"bang out" → gap-vibe-coding-hack-001
"tests" → TEST fires ✓ (positive imperative: "bang out tests")
"payment module" → scope

Predicates: ["TEST"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-047-004 — informal + IMPLEMENT + TEST: "knock out" pattern

**Request:** "Knock out the implementation and write a few tests."

```
Token scan:
"knock out" → gap-vibe-coding-hack-001
"implementation" → IMPLEMENT fires ✓
"write tests" → TEST fires ✓

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-047-005 — informal phrase alone: zero predicates

**Request:** "Just wing it."

```
Token scan:
"wing it" → gap-vibe-coding-style-001 (informal improvisation framing)
No action verbs present

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---
