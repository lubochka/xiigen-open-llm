# DPO Pair Batch — Phase 1 UI/UX Definition — Batch 004
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md + EXAMINATION-METHOD-ORCHESTRATION-v8.md

---

## Purpose

This batch teaches the orchestrator to distinguish the four Phase 1 UI
sub-phases that share the UI predicate but own different decisions:

- 1.1 Design System and Tokens — visual tokens, component primitives,
       reusable rules.
- 1.2 Layout and Responsive Structure — page layout, density, information
       structure.
- 1.3 Interaction and Navigation Model — interaction states, navigation
       paths, controls.
- 1.4 Accessibility and Usability Review — accessibility constraints,
       usability risks.

The v2 executor prompt called out the tight intra-phase confusion as the
main risk: 1.1 vs 1.2 vs 1.3 share the UI predicate and the routing decision
turns on which decision the prompt is asking for, not on which terms appear.

---

## Files in this batch

```
dpo-pairs-ui-batch-004/
  MANIFEST.md                                          (this file)
  ui-pairs-1-1-design-system-tokens.md                 (4 pairs)
  ui-pairs-1-2-layout-responsive-structure.md          (4 pairs)
  ui-pairs-1-3-interaction-navigation-model.md         (4 pairs)
  ui-pairs-1-4-accessibility-usability-review.md       (4 pairs)
```

Total: 16 DPO pairs.

---

## Sub-phase coverage

| Sub-phase | Pairs | Primary rejected target |
|---|---|---|
| 1.1 Design System and Tokens | 4 | 1.2 (2), 1.3 (1), 4.2 (1) |
| 1.2 Layout and Responsive Structure | 4 | 1.1 (2), 1.3 (1), 4.2 (1) |
| 1.3 Interaction and Navigation Model | 4 | 1.1 (1), 1.2 (1), 4.2 (2) |
| 1.4 Accessibility and Usability Review | 4 | 6.1 (1), 1.3 (1), 5.1 (1), 5.3 (1) |

---

## Why this batch matters

The four UI sub-phases are the classic case where shared predicates produce
ambiguous routing. A prompt mentioning "buttons" can route to 1.1 (token
specification), 1.2 (button placement), 1.3 (button click behaviour), or
1.4 (button accessibility). The DPO pairs in this batch make the difference
clear by name and by content.

Every rejected response is a complete, plausible response from an adjacent
UI sub-phase — never a rejection or a negation phrase.

---

## Sources

All pairs are first-party.

```
source_license_state: first_party
attribution: xiigen-ui-batch-004
```
