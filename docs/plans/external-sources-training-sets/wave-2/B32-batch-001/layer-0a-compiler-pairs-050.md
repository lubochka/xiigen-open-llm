# Layer 0-A — Compiler Pairs (050)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-050-001 — informal + IMPLEMENT: "hack it together"

**Request:** "Hack it together — implement the report exporter."

```
Token scan:
"hack it together" → gap-vibe-coding-hack-001
"implement" → IMPLEMENT fires ✓
"report exporter" → scope

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-050-002 — informal + IMPLEMENT: "cobble something together"

**Request:** "Cobble something together — build the notification sender."

```
Token scan:
"cobble together" → gap-vibe-coding-hack-001
"build" → IMPLEMENT fires ✓
"notification sender" → scope

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-050-003 — informal + IMPLEMENT + TEST: "hack together" multi-predicate

**Request:** "Hack together the auth module and write some tests."

```
Token scan:
"hack together" → gap-vibe-coding-hack-001
"hack together the auth module" → IMPLEMENT fires ✓ (hack together + noun scope = implement)
"write tests" → TEST fires ✓

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

Note: "hack together X" where X is a module/service fires IMPLEMENT. "Hack together" is context-sensitive.

---

## B32-0A-050-004 — informal + IMPLEMENT alone (hack): TransitionType check

**Request:** "Throw it together — implement the data loader."

```
Token scan:
"throw it together" → gap-vibe-coding-hack-001
"implement" → IMPLEMENT fires ✓
"data loader" → scope

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
TransitionType: feature_addition
```

---

## B32-0A-050-005 — informal phrase alone: zero predicates

**Request:** "Let's hack something together."

```
Token scan:
"hack together" → gap-vibe-coding-hack-001
"something" → vague noun; not in predicate lexicon
"let's" → invitation; no predicate

Predicates: []
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---
