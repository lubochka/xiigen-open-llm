# Layer 0-A — Compiler Pairs (037)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-037-001 — "expedient" alone: zero predicates

**Request:** "Let's do the expedient thing here."

```
Token scan:
"expedient" → gap-vibe-coding-style-001 (style qualifier; pragmatic-shortcut framing)
No action verbs present

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0A-037-002 — "expedient" + IMPLEMENT

**Request:** "Expedient solution — implement the auth bypass."

```
Token scan:
"expedient" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"auth bypass" → scope (not SECURITY; user says implement, not secure)

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

Note: "auth bypass" is a scope term here, not a security audit request. SECURITY fires only when the imperative is to secure or audit.

---

## B32-0A-037-003 — "expedient" + SECURITY

**Request:** "Expedient security check — just secure the login endpoint."

```
Token scan:
"expedient" → gap-vibe-coding-style-001
"secure" → SECURITY fires ✓
"login endpoint" → scope

Predicates: ["SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-037-004 — "pragmatic" alone: zero predicates

**Request:** "Be pragmatic about this."

```
Token scan:
"pragmatic" → gap-vibe-coding-style-001 (quality-bar framing; no-frills intent)
No action verbs present

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0A-037-005 — "pragmatic" + TEST

**Request:** "Pragmatic approach — just test the critical path."

```
Token scan:
"pragmatic" → gap-vibe-coding-style-001
"test" → TEST fires ✓ (positive imperative)
"critical path" → scope

Predicates: ["TEST"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
