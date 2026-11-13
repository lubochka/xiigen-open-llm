# Layer 0-A — Compiler Pairs (049)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-049-001 — "no tests needed" with IMPLEMENT: IMPLEMENT fires, TEST absent

**Request:** "Just build the API, no tests needed."

```
Token scan:
"build" → IMPLEMENT fires ✓
"API" → scope
"no tests needed" → TEST negated; does NOT fire

Predicates: ["IMPLEMENT"]
GapWarnings: []
DomainId: "software-development"
Note: No gap vocabulary present; no gap warning; IMPLEMENT fires from "build"
```

---

## B32-0A-049-002 — gap + "no tests needed" + IMPLEMENT: gap warning preserved

**Request:** "Lean sprint — build the API, no tests needed."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"build" → IMPLEMENT fires ✓
"no tests needed" → TEST negated

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-049-003 — "don't bother with" + ARCHITECTURE: ARCHITECTURE negated

**Request:** "Pragmatic — implement the feature, don't bother with architecture."

```
Token scan:
"pragmatic" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"don't bother with architecture" → ARCHITECTURE negated; does NOT fire

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-049-004 — "skip the" + SECURITY: SECURITY negated

**Request:** "Lean sprint — build the service, skip the security review."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"build" → IMPLEMENT fires ✓
"skip the security review" → SECURITY negated; does NOT fire

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-049-005 — all three negations + IMPLEMENT: only IMPLEMENT fires

**Request:** "Expedient sprint — implement the module, skip tests, skip docs, skip architecture."

```
Token scan:
"expedient" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"skip tests" → TEST negated
"skip docs" → DOCUMENT negated
"skip architecture" → ARCHITECTURE negated

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
