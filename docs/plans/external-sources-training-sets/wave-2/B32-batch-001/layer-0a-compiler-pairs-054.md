# Layer 0-A — Compiler Pairs (054)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-054-001 — DomainId invariant: always "software-development"

**Request:** "Lean sprint — implement the feature."

```
Output fields check:
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
DomainId: "software-development"  ← invariant; never null, never "project-management"
CapabilityGapWarning: style_qualifier_noted
```

DomainId is an invariant for all B32 outputs. This is checked in every layer.

---

## B32-0A-054-002 — DomainId invariant: knowledge_edge path

**Request:** "Be pragmatic."

```
Output fields check:
Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
DomainId: "software-development"  ← invariant; present even in knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
```

DomainId appears in knowledge_edge outputs too.

---

## B32-0A-054-003 — DomainId invariant: no gap vocabulary

**Request:** "Build the service."

```
Output fields check:
Predicates: ["IMPLEMENT"]
GapWarnings: []
DomainId: "software-development"  ← invariant; present even without gap vocabulary
```

DomainId is not conditional on gap vocabulary presence.

---

## B32-0A-054-004 — DomainId invariant: multi-gap multi-predicate

**Request:** "Expedient spike — implement the dashboard."

```
Output fields check:
"expedient" → gap-vibe-coding-style-001
"spike" → gap-vibe-coding-spike-001
"implement" → IMPLEMENT fires ✓
"dashboard" → gap-vibe-coding-dashboard-001

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-dashboard-001]
DomainId: "software-development"  ← invariant; unchanged by gap ID count
CapabilityGapWarning: style_qualifier_noted
```

Three gap IDs; DomainId is still exactly "software-development".

---
