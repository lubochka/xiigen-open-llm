# Layer 0-A — Compiler Pairs (041)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-041-001 — gap + API_CONTRACT: API_CONTRACT fires despite gap framing

**Request:** "No-frills API — define the contract for the payment endpoints."

```
Token scan:
"no-frills" → gap-vibe-coding-style-001
"define the contract" → API_CONTRACT fires ✓
"payment endpoints" → scope

Predicates: ["API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-041-002 — gap + API_CONTRACT: expedient framing

**Request:** "Expedient API design — define the interface contract."

```
Token scan:
"expedient" → gap-vibe-coding-style-001
"define" + "contract" → API_CONTRACT fires ✓
"interface" → scoping noun, not UI trigger

Predicates: ["API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-041-003 — gap + API_CONTRACT + IMPLEMENT: two predicates

**Request:** "Pragmatic API — define the contract and implement the endpoint."

```
Token scan:
"pragmatic" → gap-vibe-coding-style-001
"define the contract" → API_CONTRACT fires ✓
"implement" → IMPLEMENT fires ✓
"endpoint" → scope

Predicates: ["API_CONTRACT", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-041-004 — "API contract" as noun only: no API_CONTRACT predicate

**Request:** "We need a lean API contract."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"API contract" → noun phrase; not an action directive
No "define" or "specify" imperative

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

Note: API_CONTRACT fires from action verbs (define, specify, document the contract), not from the noun "API contract" alone.

---

## B32-0A-041-005 — gap + API_CONTRACT alone (no IMPLEMENT): routing clarification

**Request:** "Stripped-down API — just define the contract."

```
Token scan:
"stripped-down" → gap-vibe-coding-style-001
"define" + "contract" → API_CONTRACT fires ✓
"just" → intensifier; no predicate effect
No "implement" or "build"

Predicates: ["API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
