# Layer 2 — Capability Gap Extended Pairs (043)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-043-001 — complete gap vocabulary summary (all rounds)

**Summary of all gap vocabulary introduced across all rounds:**

| Round | Files | New gap vocabulary |
|---|---|---|
| Round 1 | 001-012 | scrappy, hack, quick win, throwaway, rough cut, MVP, lo-fi, fast and loose, vibe code, dashboard |
| Round 2 | 013-036 | good enough, minimum viable, PoC, spike, tracer bullet, walking skeleton, thin slice, paper prototype, rough draft |
| Round 3 | 037-054 | expedient, pragmatic, no-frills, barebones, stripped-down, lean, agile (informal), iterative, just get it working, clean up later, wing it, whip up/bang out/knock out |
| Round 4 | 049-060 | Negation patterns (not new gap vocabulary; suppress predicates) |

---

## B32-gap-043-002 — CapabilityGapType complete reference

**CapabilityGapType values:**

| Type | Condition |
|---|---|
| style_qualifier_solo | Gap vocabulary present; predicates = [] |
| style_qualifier_with_predicates | Gap vocabulary present; predicates non-empty |
| (none) | No gap vocabulary present |

---

## B32-gap-043-003 — GapCategory values

**GapCategory values (derived from gap ID):**

| GapCategory | Gap ID | Vocabulary |
|---|---|---|
| quality_bar_framing | gap-vibe-coding-style-001 | Style adjectives + vibe phrases |
| hack_approach | gap-vibe-coding-hack-001 | Informal action phrases |
| prototype_scope | gap-vibe-coding-prototype-001 | PoC/prototype vocabulary |
| spike_scope | gap-vibe-coding-spike-001 | Spike/tracer/skeleton vocabulary |
| rough_cut_scope | gap-vibe-coding-rough-cut-001 | Rough cut/draft vocabulary |
| dashboard_scope | gap-vibe-coding-dashboard-001 | Dashboard specifically |

---

## B32-gap-043-004 — B32 final rules (complete)

**Nine rules governing all B32 behavior:**

1. Gap vocabulary alone → knowledge_edge → SCL (once)
2. Gap + action verb → predicate fires + gap warning (no SCL)
3. No qualifier suppresses any predicate
4. "dashboard" never fires UI; always gap-vibe-coding-dashboard-001
5. "vibe code" / "vibe coding" → IMPLEMENT fires (from "code" root)
6. SCL emits exactly once per request
7. DomainId = "software-development" (invariant, always)
8. Negated vocabulary does not fire its predicate
9. Gap ID must match actual vocabulary family

---
