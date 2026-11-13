# Layer 0-A — Compiler Pairs (052)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-052-001 — informal + TEST: "bang out tests" pattern

**Request:** "Bang out some unit tests for the checkout flow."

```
Token scan:
"bang out" → gap-vibe-coding-hack-001
"unit tests" → TEST fires ✓ (unit tests = explicit TEST scope)
"checkout flow" → scope

Predicates: ["TEST"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-052-002 — style qualifier + TEST: "quick" framing

**Request:** "Quick — write tests for the payment handler."

```
Token scan:
"quick" → gap-vibe-coding-style-001 (quick-win style qualifier)
"write tests" → TEST fires ✓
"payment handler" → scope

Predicates: ["TEST"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-052-003 — "throwaway tests" + TEST: TEST fires

**Request:** "Write throwaway tests for the upload module."

```
Token scan:
"throwaway" → gap-vibe-coding-style-001 (disposable quality framing)
"write tests" → TEST fires ✓
"upload module" → scope

Predicates: ["TEST"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-052-004 — "test" alone without verb: no TEST

**Request:** "Lean approach — some testing would be nice."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"testing would be nice" → conditional/aspirational; not an imperative verb
"some" → vague quantifier

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

"Testing would be nice" is not a TEST imperative. TEST fires from "test X", "write tests", "unit test X".

---

## B32-0A-052-005 — gap + TEST + IMPLEMENT: informal full-stack pattern

**Request:** "Hack together the module and bang out tests for it."

```
Token scan:
"hack together" → gap-vibe-coding-hack-001
"module" → IMPLEMENT fires ✓ (specific software artifact)
"bang out tests" → TEST fires ✓

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
