# Layer 0-A — Compiler Pairs (040)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-040-001 — "iterative" alone: zero predicates

**Request:** "Let's be iterative about this."

```
Token scan:
"iterative" → gap-vibe-coding-style-001 (process-style qualifier; implies incremental approach)
"be" → existential verb; not in predicate lexicon

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0A-040-002 — "iterative" + IMPLEMENT

**Request:** "Iterative build — implement the search feature."

```
Token scan:
"iterative" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"search feature" → scope

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-040-003 — gap + SECURITY: security predicate fires despite gap framing

**Request:** "Quick-win security fix — secure the token validation."

```
Token scan:
"quick-win" → gap-vibe-coding-style-001
"secure" → SECURITY fires ✓
"token validation" → scope

Predicates: ["SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

SECURITY fires independently of the "quick-win" framing. Gap warning is added to metadata.

---

## B32-0A-040-004 — gap + SECURITY: lean framing with secure verb

**Request:** "Lean security — secure the API gateway."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"secure" → SECURITY fires ✓
"API gateway" → scope

Predicates: ["SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-040-005 — "iterative" + gap + IMPLEMENT + TEST: multi-predicate

**Request:** "Iterative sprint — implement the dashboard and test the endpoints."

```
Token scan:
"iterative" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"dashboard" → gap-vibe-coding-dashboard-001 (NOT UI; dashboard is never UI)
"test" → TEST fires ✓
"endpoints" → scope

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
