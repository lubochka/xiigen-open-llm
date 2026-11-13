# Layer 2 — Capability Gap Extended Pairs (023)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-023-001 — Complete style qualifier taxonomy: all 13 gap categories with examples

**Reference: every style qualifier category in B32 with a canonical example**

| Category | Gap ID | Canonical example | Zero predicates? |
|---|---|---|---|
| Quality-bar framing | gap-vibe-coding-style-001 | "scrappy solution" | Yes (alone) |
| Exploratory framing | gap-vibe-coding-exploratory-001 | "explore the options" | Yes |
| Artifact-type noun | gap-vibe-coding-prototype-001 | "a prototype" | Yes (alone) |
| Technical investigation | gap-vibe-coding-spike-001 | "run a spike" | Yes |
| Casual assembly | gap-vibe-coding-hack-001 | "hack together" | Yes (alone) |
| Demonstration framing | gap-vibe-coding-demo-001 | "demo it" (standalone) | Yes |
| Non-UI display | gap-vibe-coding-dashboard-001 | "build a dashboard" | No — IMPLEMENT fires |
| Low-fidelity cut | gap-vibe-coding-rough-cut-001 | "rough-cut design" | No — ARCHITECTURE fires |
| Vibe/intuition | gap-vibe-coding-intuition-001 | "feels right" | Yes |
| Disposability | gap-vibe-coding-throwaway-001 | "throwaway code" | Yes (alone) |
| Low-quality compound | gap-vibe-coding-quick-dirty-001 | "quick and dirty" | Yes (alone) |
| Minimal correctness | gap-vibe-coding-just-works-001 | "just make it work" | Yes (alone) |
| Low-ambition | gap-vibe-coding-nothing-fancy-001 | "nothing fancy" | Yes (alone) |

---

## B32-gap-023-002 — Style qualifier + action verb: the override rule

A: "Quick and dirty, throwaway, scrappy, nothing fancy — build the API."
B: Predicates: [IMPLEMENT, API_CONTRACT] | GapId: gap-vibe-coding-quick-dirty-001, gap-vibe-coding-throwaway-001, gap-vibe-coding-style-001, gap-vibe-coding-nothing-fancy-001 | DomainId: software-development
   GapWarning: CapabilityGapWarning: style_qualifier_noted | Note: 4 style qualifiers present; IMPLEMENT fires on "build", API_CONTRACT fires on "API"; neither suppressed

---

## B32-gap-023-003 — Style qualifier taxonomy: the pure-gap category members

**Pure gap items (always zero predicates alone, no root that fires any predicate):**
vibe (alone), vibe out, exploratory, explore, prototype (alone), spike, hack together, cobble together, throw together, demo (standalone), intuition-driven, feel right, throwaway (alone), quick and dirty (alone), just make it work (alone), nothing fancy (alone), good enough, scaffold (alone), creative mode

**Important exception in "pure gap" list:**
- "vibe code" / "vibe coding" = gap-vibe-coding-style-001 + IMPLEMENT (not pure gap; "code" root fires)

---
