# Held-Out and Corruption Pairs (027)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-027-001 — HELD-OUT: "lean and agile — architect the microservices" → ARCHITECTURE + gap warning

**Request:** "Lean and agile — architect the microservices."

**Expected output:**
```
Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ArchitectureFlow31]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

**Reasoning:**
- "lean" → gap-vibe-coding-style-001
- "agile" → gap-vibe-coding-style-001 (same ID; one entry)
- "architect" → ARCHITECTURE fires

---

## B32-HO-027-002 — HELD-OUT: "keep it lean, QA the integration" → QA + gap warning

**Request:** "Keep it lean, QA the integration."

**Expected output:**
```
Predicates: ["QA"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [QAFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-HO-027-003 — CORRUPTION: "lean agile" fires two gap IDs separately → double SCL (WRONG)

**Corrupted output:**
```
Request: "Keep it lean and agile."
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING × 2
Reason: two gap qualifiers → two SCL events
```

**Why this is wrong:**
- SCL is emitted exactly once per request regardless of gap ID count
- Even two distinct gap IDs produce one SCL event

**Correct:** SCHEDULE_CAPABILITY_LEARNING emitted once.

---

## B32-HO-027-004 — CORRUPTION: "agile" fires Agile methodology predicates (WRONG)

**Corrupted output:**
```
Request: "Stay agile about it."
Predicates: ["SPRINT", "SCRUM"]
Reason: "agile" → methodology predicates
```

**Why this is wrong:**
- SPRINT and SCRUM are not registered predicates
- Informal "agile" maps to gap-vibe-coding-style-001 only
- No action verb present → predicates = []

**Correct predicates:** []

---
