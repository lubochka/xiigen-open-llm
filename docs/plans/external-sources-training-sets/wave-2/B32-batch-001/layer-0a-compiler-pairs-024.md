# Layer 0-A — Compiler Pairs (024)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-024-001 — Complete gap vocabulary reference: zero-predicate tokens

**Reference pair — all B32 gap tokens that produce zero predicates when alone:**

| Token / phrase | Gap ID | Notes |
|---|---|---|
| vibe (alone, no "code") | gap-vibe-coding-style-001 | "vibe" alone → no predicate |
| vibe code | gap-vibe-coding-style-001 + IMPLEMENT | "code" root fires IMPLEMENT independently |
| vibe coding | gap-vibe-coding-style-001 + IMPLEMENT | "coding" root fires IMPLEMENT |
| vibe out | gap-vibe-coding-style-001 | No "code" root; no IMPLEMENT |
| exploratory | gap-vibe-coding-exploratory-001 | Never fires any predicate |
| explore | gap-vibe-coding-exploratory-001 | Never fires any predicate |
| prototype (alone) | gap-vibe-coding-prototype-001 | No action verb = zero predicates |
| dashboard | gap-vibe-coding-dashboard-001 | Not in UI lexicon |
| spike | gap-vibe-coding-spike-001 | Never fires any predicate |
| hack together | gap-vibe-coding-hack-001 | "together" form → gap only |
| hack alone (as verb) | gap-vibe-coding-hack-001 | Same gap; no IMPLEMENT |
| cobble together | gap-vibe-coding-hack-001 | Synonym for "hack together" |
| throw together | gap-vibe-coding-hack-001 | Synonym for "hack together" |
| demo (standalone) | gap-vibe-coding-demo-001 | "demo" as noun/standalone → gap |
| rough-cut / rough cut | gap-vibe-coding-rough-cut-001 | Two-word or hyphenated |
| intuition-driven | gap-vibe-coding-intuition-001 | — |
| feel right / feels right | gap-vibe-coding-intuition-001 | — |
| throwaway / throwaway code | gap-vibe-coding-throwaway-001 | — |
| quick and dirty | gap-vibe-coding-quick-dirty-001 | — |
| just make it work | gap-vibe-coding-just-works-001 | — |
| just get it working | gap-vibe-coding-just-works-001 | — |
| nothing fancy | gap-vibe-coding-nothing-fancy-001 | — |
| scaffold (alone) | gap-vibe-coding-style-001 | — |
| good enough | gap-vibe-coding-style-001 | — |
| creative mode | gap-vibe-coding-style-001 | — |
| scrappy | gap-vibe-coding-style-001 | — |
| low-fi / lo-fi | gap-vibe-coding-style-001 | — |
| quick win (alone) | gap-vibe-coding-style-001 | — |
| fast and loose (alone) | gap-vibe-coding-style-001 | — |

---

## B32-0A-024-002 — Style qualifier rule: qualifiers NEVER suppress action predicates

**Master rule:** All B32 vibe/gap vocabulary functions as style qualifiers. Style qualifiers emit gap warnings but do NOT suppress registered action predicates.

**Proof table:**

| Style qualifier | Action verb present | IMPLEMENT fires? |
|---|---|---|
| quick and dirty | build | YES |
| vibe coding | implement | YES |
| hack together | create | YES |
| nothing fancy | write | YES |
| throwaway | code | YES |
| just make it work | build | YES |
| scrappy | implement | YES |
| fast and loose | build | YES |
| rough-cut | design | ARCHITECTURE (not IMPLEMENT) |

**Any output that shows style qualifier suppressing an action predicate is v6-02 corrupt.**

**DomainId for all B32 outputs:** "software-development"

---
