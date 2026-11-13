# Layer 0-A — Compiler Pairs (043)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-043-001 — mixed multi-qualifier + IMPLEMENT + TEST + SECURITY

**Request:** "Pragmatic lean approach — implement the handler, test it, and secure the endpoint."

```
Token scan:
"pragmatic" → gap-vibe-coding-style-001
"lean" → gap-vibe-coding-style-001 (same ID)
"implement" → IMPLEMENT fires ✓
"test" → TEST fires ✓
"secure" → SECURITY fires ✓
"endpoint" → scope

Predicates: ["IMPLEMENT", "TEST", "SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-043-002 — mixed multi-qualifier + full stack predicates

**Request:** "Expedient barebones sprint — architect the system, implement the API, define the contract, and secure it."

```
Token scan:
"expedient" → gap-vibe-coding-style-001
"barebones" → gap-vibe-coding-style-001
"architect" → ARCHITECTURE fires ✓
"implement" → IMPLEMENT fires ✓
"define the contract" → API_CONTRACT fires ✓
"secure" → SECURITY fires ✓

Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-043-003 — multi-qualifier with dashboard: dashboard not UI

**Request:** "No-frills stripped-down sprint — implement the dashboard and test it."

```
Token scan:
"no-frills" → gap-vibe-coding-style-001
"stripped-down" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"dashboard" → gap-vibe-coding-dashboard-001 (not UI)
"test" → TEST fires ✓

Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-043-004 — multi-qualifier alone: all silent

**Request:** "Expedient, pragmatic, lean, barebones, stripped-down, no-frills."

```
Token scan:
All six → gap-vibe-coding-style-001 (same ID)
No action verbs present

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING (once)
DomainId: "software-development"
```

---
