# Layer 0-A — Compiler Pairs (045)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-045-001 — negation pattern: "no tests needed" alone

**Request:** "No tests needed — just ship it."

```
Token scan:
"no tests needed" → negation; TEST does NOT fire
"just" → intensifier; no predicate
"ship it" → delivery phrase; no registered predicate

Predicates: []
GapWarnings: []
Note: No gap vocabulary present; no SCL emitted; no predicates fired
DomainId: "software-development"
```

Note: Negation patterns like "no tests needed" never fire predicates. No gap vocabulary is present here either.

---

## B32-0A-045-002 — negation + gap qualifier: still no TEST

**Request:** "Good enough — no tests needed."

```
Token scan:
"good enough" → gap-vibe-coding-style-001
"no tests needed" → negation; TEST does NOT fire

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0A-045-003 — "don't bother with tests" + IMPLEMENT: only IMPLEMENT fires

**Request:** "Lean sprint — implement the feature, don't bother with tests."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"don't bother with tests" → negation of TEST; TEST does NOT fire

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-045-004 — "skip the tests" + IMPLEMENT: only IMPLEMENT fires

**Request:** "Pragmatic — skip the tests, just implement the upload."

```
Token scan:
"pragmatic" → gap-vibe-coding-style-001
"skip the tests" → negation; TEST does NOT fire
"implement" → IMPLEMENT fires ✓
"upload" → scope

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-045-005 — "no documentation needed" + IMPLEMENT: only IMPLEMENT

**Request:** "Expedient — no docs needed, just build the parser."

```
Token scan:
"expedient" → gap-vibe-coding-style-001
"no docs needed" → negation; DOCUMENT does NOT fire
"build" → IMPLEMENT fires ✓
"parser" → scope

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
