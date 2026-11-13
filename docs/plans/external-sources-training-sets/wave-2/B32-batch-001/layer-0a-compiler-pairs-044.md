# Layer 0-A — Compiler Pairs (044)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-044-001 — gap + SECURITY + IMPLEMENT: two predicates

**Request:** "Lean sprint — implement the auth flow and secure the tokens."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"secure" → SECURITY fires ✓
"tokens" → scope

Predicates: ["IMPLEMENT", "SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-044-002 — "just secure it" with gap: SECURITY fires

**Request:** "Pragmatic — just secure the database connection."

```
Token scan:
"pragmatic" → gap-vibe-coding-style-001
"just" → intensifier; no predicate effect
"secure" → SECURITY fires ✓
"database connection" → scope

Predicates: ["SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-044-003 — "hack together" + SECURITY: hack gap + SECURITY both apply

**Request:** "Hack together a security fix — secure the login form."

```
Token scan:
"hack together" → gap-vibe-coding-hack-001
"security fix" → context noun; not a SECURITY trigger alone
"secure" → SECURITY fires ✓
"login form" → scope

Predicates: ["SECURITY"]
GapWarnings: [gap-vibe-coding-hack-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

Note: "hack together" maps to gap-vibe-coding-hack-001, not gap-vibe-coding-style-001.

---

## B32-0A-044-004 — "security" in context without "secure" verb: no SECURITY

**Request:** "Lean approach — think about security."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"think about" → cognitive phrase; not in predicate lexicon
"security" → noun object; no action verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

"Think about security" does not fire SECURITY. Only imperative "secure X" fires.

---

## B32-0A-044-005 — gap + SECURITY + ARCHITECTURE: two predicates, no IMPLEMENT

**Request:** "Expedient design — architect the auth service and secure the API."

```
Token scan:
"expedient" → gap-vibe-coding-style-001
"architect" → ARCHITECTURE fires ✓
"secure" → SECURITY fires ✓
"API" → scope

Predicates: ["ARCHITECTURE", "SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

TransitionType: no_contribution (SECURITY; no IMPLEMENT present; ARCHITECTURE → documentation, but SECURITY → no_contribution; IMPLEMENT not present to override).

Actually: mixed ARCHITECTURE + SECURITY → ARCHITECTURE is documentation; SECURITY is no_contribution. Since neither IMPLEMENT nor UI present, TransitionType is determined by highest-priority predicate present. ARCHITECTURE → documentation takes effect as the primary content-producing predicate.

---
