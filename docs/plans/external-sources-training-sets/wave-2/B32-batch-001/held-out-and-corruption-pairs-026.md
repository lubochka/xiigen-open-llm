# Held-Out and Corruption Pairs (026)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-026-001 — HELD-OUT: "barebones build — implement the data pipeline" → IMPLEMENT

**Request:** "Barebones build — implement the data pipeline."

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

**Reasoning:**
- "barebones" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires
- "data pipeline" → scope

---

## B32-HO-026-002 — HELD-OUT: "stripped-down design — architect the service layer" → ARCHITECTURE

**Request:** "Stripped-down design — architect the service layer."

**Expected output:**
```
Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ArchitectureFlow31]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-HO-026-003 — CORRUPTION: "stripped-down UI" fires UI without verb (WRONG)

**Corrupted output:**
```
Request: "Stripped-down UI."
Predicates: ["UI"]
Reason: "UI" keyword present in request
```

**Why this is wrong:**
- "stripped-down UI" has no action verb
- UI predicate requires a build/create/implement verb alongside a screen/interface scope
- "UI" as a noun in "stripped-down UI" is not an action directive

**Correct predicates:** []

---

## B32-HO-026-004 — CORRUPTION: "barebones" fires IMPLEMENT (WRONG)

**Corrupted output:**
```
Request: "Barebones prototype."
Predicates: ["IMPLEMENT"]
Reason: "barebones" implies build intent
```

**Why this is wrong:**
- "barebones" is a style qualifier, not an action verb
- "prototype" is gap-vibe-coding-prototype-001, also not an action verb
- No IMPLEMENT trigger present

**Correct predicates:** []

---
