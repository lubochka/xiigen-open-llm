# Layer 0-A — Compiler Pairs (056)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-056-001 — complete gap vocabulary reference: all six gap IDs mapped

**Complete mapping of all B32 gap vocabulary to gap IDs:**

**gap-vibe-coding-style-001:**
scrappy, hack (adj.), quick win, throwaway, rough cut (solo), minimum viable, MVP, lo-fi, low-fi, fast and loose, good enough, lean, pragmatic, expedient, barebones, stripped-down, no-frills, agile (informal adj.), iterative, just get it working, clean up later, wing it, "just enough"

**gap-vibe-coding-hack-001:**
hack together, cobble together, throw together, bang out, knock out, whip up, kludge together, hack it together

**gap-vibe-coding-prototype-001:**
prototype, PoC (Proof-of-Concept), proof-of-concept, paper prototype, wireframe

**gap-vibe-coding-spike-001:**
spike, technical spike, tracer bullet, tracer-bullet, walking skeleton, thin slice, thin-slice, vertical slice

**gap-vibe-coding-rough-cut-001:**
rough cut, rough-cut, rough draft

**gap-vibe-coding-dashboard-001:**
dashboard (standalone or as scope noun)

---

## B32-0A-056-002 — "quick" as gap qualifier

**Request:** "Quick implementation — build the retry handler."

```
Token scan:
"quick" → gap-vibe-coding-style-001 (quick-win/quick-fix framing)
"build" → IMPLEMENT fires ✓
"retry handler" → scope

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-056-003 — "just enough" as gap qualifier

**Request:** "Just enough — implement the minimum."

```
Token scan:
"just enough" → gap-vibe-coding-style-001 (sufficiency framing)
"implement" → IMPLEMENT fires ✓
"minimum" → scope; not a gap qualifier here (acting as object noun)

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0A-056-004 — full gap vocabulary alone: largest multi-gap example

**Request:** "Lean pragmatic expedient barebones no-frills stripped-down spike prototype hack-together."

```
Token scan:
"lean" → gap-vibe-coding-style-001
"pragmatic" → gap-vibe-coding-style-001 (same)
"expedient" → gap-vibe-coding-style-001 (same)
"barebones" → gap-vibe-coding-style-001 (same)
"no-frills" → gap-vibe-coding-style-001 (same)
"stripped-down" → gap-vibe-coding-style-001 (same)
"spike" → gap-vibe-coding-spike-001
"prototype" → gap-vibe-coding-prototype-001
"hack-together" → gap-vibe-coding-hack-001

Predicates: []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-prototype-001, gap-vibe-coding-hack-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING (once)
DomainId: "software-development"
```

---
