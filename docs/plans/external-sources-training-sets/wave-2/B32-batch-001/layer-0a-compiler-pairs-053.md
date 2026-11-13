# Layer 0-A — Compiler Pairs (053)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-053-001 — qualifier + multi-predicate: IMPLEMENT + TEST + DOCUMENT

**Request:** "Lean sprint — implement the service, write tests, and document the API."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"write tests" → TEST fires ✓
"document the API" → DOCUMENT fires ✓

Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-053-002 — qualifier + multi-predicate: IMPLEMENT + ARCHITECTURE + API_CONTRACT

**Request:** "Pragmatic sprint — architect the system, implement the core, define the API contract."

```
Token scan:
"pragmatic" → gap-vibe-coding-style-001
"architect" → ARCHITECTURE fires ✓
"implement" → IMPLEMENT fires ✓
"define the API contract" → API_CONTRACT fires ✓

Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-053-003 — qualifier + all eight predicates

**Request:** "Expedient sprint — implement, build the UI, architect, define contract, secure, test, document, QA."

```
Token scan:
"expedient" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"build the UI" → UI fires ✓ (also IMPLEMENT from "build")
"architect" → ARCHITECTURE fires ✓
"define contract" → API_CONTRACT fires ✓
"secure" → SECURITY fires ✓
"test" → TEST fires ✓
"document" → DOCUMENT fires ✓
"QA" → QA fires ✓

Predicates: ["IMPLEMENT", "UI", "ARCHITECTURE", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT", "QA"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-053-004 — qualifier + multi-predicate with negation: subset fires

**Request:** "Lean sprint — implement, test, don't bother with docs, skip architecture."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"test" → TEST fires ✓
"don't bother with docs" → DOCUMENT negated
"skip architecture" → ARCHITECTURE negated

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
