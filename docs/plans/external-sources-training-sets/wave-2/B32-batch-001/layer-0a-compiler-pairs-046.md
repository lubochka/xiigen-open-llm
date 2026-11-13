# Layer 0-A — Compiler Pairs (046)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-046-001 — "don't bother with" pattern: DOCUMENT negated

**Request:** "Lean sprint — implement the endpoint, don't bother documenting it."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"don't bother documenting" → DOCUMENT negated; does NOT fire

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-046-002 — "skip the" pattern: QA negated

**Request:** "Pragmatic push — implement the feature, skip the QA."

```
Token scan:
"pragmatic" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"skip the QA" → QA negated; does NOT fire

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-046-003 — "just get it working" as gap qualifier

**Request:** "Just get it working."

```
Token scan:
"just get it working" → gap-vibe-coding-style-001 (vibe-code phrasing; emphasis on minimal working state)
No explicit action verbs in IMPLEMENT lexicon

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

Note: "get it working" is a vibe-code phrase. "get" and "working" together do not fire IMPLEMENT; "implement" or "build" must be explicit.

---

## B32-0A-046-004 — "just get it working" + explicit IMPLEMENT

**Request:** "Just get it working — implement the login flow."

```
Token scan:
"just get it working" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"login flow" → scope

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-046-005 — "clean up later" as gap qualifier

**Request:** "Clean up later — just implement the core now."

```
Token scan:
"clean up later" → gap-vibe-coding-style-001 (deferred quality framing)
"implement" → IMPLEMENT fires ✓
"core" → scope

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
