# Layer 3 — Teaching Unit Pairs (023)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-023-001 — Complete style qualifier taxonomy: all 13 categories

**Teaching unit:** B32 gap vocabulary falls into 13 categories. Each category has one or more registered gap IDs and a set of trigger terms.

| # | Category | Gap ID | Trigger terms |
|---|---|---|---|
| 1 | Quality-bar framing | gap-vibe-coding-style-001 | scrappy, low-fi, lo-fi, good enough, quick win, fast and loose, scaffold (alone), creative mode |
| 2 | Vibe framing | gap-vibe-coding-style-001 | vibe (alone), vibe out |
| 3 | Exploratory framing | gap-vibe-coding-exploratory-001 | exploratory, explore |
| 4 | Artifact-type noun | gap-vibe-coding-prototype-001 | prototype (alone, no action verb) |
| 5 | Technical investigation | gap-vibe-coding-spike-001 | spike, technical spike, run a spike |
| 6 | Casual assembly | gap-vibe-coding-hack-001 | hack together, cobble together, throw together, hack (dev verb) |
| 7 | Demonstration | gap-vibe-coding-demo-001 | demo (standalone) |
| 8 | Non-UI display | gap-vibe-coding-dashboard-001 | dashboard |
| 9 | Low-fidelity cut | gap-vibe-coding-rough-cut-001 | rough cut, rough-cut |
| 10 | Vibe/intuition | gap-vibe-coding-intuition-001 | intuition-driven, feel right, feels right |
| 11 | Disposability | gap-vibe-coding-throwaway-001 | throwaway, throwaway code |
| 12 | Low-quality compound | gap-vibe-coding-quick-dirty-001 | quick and dirty |
| 13 | Minimal-correctness | gap-vibe-coding-just-works-001 | just make it work, just get it working |
| — | Low-ambition | gap-vibe-coding-nothing-fancy-001 | nothing fancy |

**Note:** "vibe code" / "vibe coding" are listed separately because they contain "code" root and fire IMPLEMENT in addition to producing gap-vibe-coding-style-001.

---

## B32-L3-023-002 — Style qualifier rule: the universal statement

**The single universal rule for all B32 style qualifiers:**

A style qualifier communicates HOW the developer wants to work. The system routes on WHAT technical action to perform. Style qualifiers NEVER suppress action predicates. They produce gap warnings when no action verb is present, and CapabilityGapWarning annotations when action verbs are present.

**DomainId:** Always "software-development" for all B32 outputs.

---
