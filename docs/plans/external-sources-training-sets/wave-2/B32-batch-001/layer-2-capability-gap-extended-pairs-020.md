# Layer 2 — Capability Gap Extended Pairs (020)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-020-001 — Multiple style qualifiers in one request: all produce gap warnings; predicates still fire

A: "Quick and dirty, throwaway, nothing fancy — just build the checkout flow."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-quick-dirty-001, gap-vibe-coding-throwaway-001, gap-vibe-coding-nothing-fancy-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: IMPLEMENT fires on "build"; all three gap IDs logged

---

## B32-gap-020-002 — Vibe + hack + scrappy all present; single action verb fires

A: "Vibe code it, hack together a solution, scrappy is fine — implement the storage adapter."
B: Predicates: [IMPLEMENT] | GapId: gap-vibe-coding-style-001, gap-vibe-coding-hack-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: "code" in vibe code + "implement" both trigger IMPLEMENT (deduplicates)

---

## B32-gap-020-003 — Low-fi + rough cut + fast and loose together: triple style gap; design fires

A: "Low-fi, rough-cut, fast and loose architecture design — just good enough."
B: Predicates: [ARCHITECTURE] | GapId: gap-vibe-coding-style-001, gap-vibe-coding-rough-cut-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: "good enough" maps to gap-vibe-coding-style-001 (same ID)

---

## B32-gap-020-004 — Spike + vibe + hack together with no action verb: all gap; zero predicates

A: "Run a spike, vibe it out, hack together something — just a rough prototype."
B: Predicates: [] | GapId: gap-vibe-coding-spike-001, gap-vibe-coding-style-001, gap-vibe-coding-hack-001, gap-vibe-coding-prototype-001 | DomainId: software-development
   GapWarning: SCHEDULE_CAPABILITY_LEARNING emitted once (not 4 times) | Note: "vibe out" (not "vibe code") — no IMPLEMENT

---
