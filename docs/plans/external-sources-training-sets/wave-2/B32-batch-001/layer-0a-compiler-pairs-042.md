# Layer 0-A — Compiler Pairs (042)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-042-001 — gap + ARCHITECTURE: ARCHITECTURE fires despite gap framing

**Request:** "No-frills architecture — architect the service boundary."

```
Token scan:
"no-frills" → gap-vibe-coding-style-001
"architect" → ARCHITECTURE fires ✓
"service boundary" → scope

Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-042-002 — gap + ARCHITECTURE: lean framing

**Request:** "Lean architecture — design the data layer."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"design" → ARCHITECTURE fires ✓ (design as architecture verb)
"data layer" → scope

Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-042-003 — gap + ARCHITECTURE + IMPLEMENT: two predicates

**Request:** "Iterative design — architect the modules and implement the core."

```
Token scan:
"iterative" → gap-vibe-coding-style-001
"architect" → ARCHITECTURE fires ✓
"implement" → IMPLEMENT fires ✓
"core" → scope

Predicates: ["ARCHITECTURE", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-042-004 — "architecture" as noun only: no ARCHITECTURE predicate

**Request:** "We need a lean architecture."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"architecture" → noun; not an action verb
"need" → desire verb; not in predicate lexicon

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0A-042-005 — mixed multi-qualifier: all same gap ID

**Request:** "Lean, agile, iterative approach."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"agile" → gap-vibe-coding-style-001
"iterative" → gap-vibe-coding-style-001
"approach" → noun; no action verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING (once)
DomainId: "software-development"
```

Three qualifiers, same gap ID, one SCL emission.

---
