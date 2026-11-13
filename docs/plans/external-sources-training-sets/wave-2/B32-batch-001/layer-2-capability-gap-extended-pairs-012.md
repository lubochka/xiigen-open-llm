# Layer 2 — Capability Gap Extended Pairs (012)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-L2-012-001 — Complete B32 gap vocabulary map + gap-ID naming reference

**Purpose:** Complete reference map for all B32 gap vocabulary, their gap IDs, and naming convention.

### Gap ID naming convention

Format: `gap-vibe-coding-{descriptor}-{version}`

All B32 gap IDs use the prefix `gap-vibe-coding-` to namespace them as B32 vibe coding gaps.

### Complete B32 gap vocabulary map

| Gap ID | Primary triggers | Category | Zero-predicate? |
|---|---|---|---|
| gap-vibe-coding-style-001 | vibe, vibe code, vibe coding, creative mode, good enough | Core vibe style | If no action verb: yes |
| gap-vibe-coding-exploratory-001 | exploratory, explore (without design) | Exploration framing | If no action verb: yes |
| gap-vibe-coding-prototype-001 | prototype (alone) | Artifact type qualifier | Yes (alone); No (with action verb) |
| gap-vibe-coding-dashboard-001 | dashboard | Unrecognized UI noun | Yes (alone) |
| gap-vibe-coding-spike-001 | spike, run a spike | Technical investigation | Yes |
| gap-vibe-coding-hack-001 | hack together, cobble together, throw together | Informal assembly | Yes (alone) |
| gap-vibe-coding-demo-001 | demo (standalone) | Informal presentation | Yes |
| gap-vibe-coding-rough-cut-001 | rough-cut, rough cut, rough draft | Draft quality | No (gap warning only) |
| gap-vibe-coding-intuition-001 | intuition-driven, feel right, feels right | Intuition framing | No (gap warning only) |
| gap-vibe-coding-throwaway-001 | throwaway, throwaway code | Disposability framing | No (gap warning only) |
| gap-vibe-coding-quick-dirty-001 | quick and dirty | Quality reduction | No (gap warning only) |
| gap-vibe-coding-just-works-001 | just make it work, just get it working | Minimum bar | No (gap warning only) |
| gap-vibe-coding-nothing-fancy-001 | nothing fancy | Simplicity framing | No (gap warning only) |

**Zero-predicate column:** "Yes" = this term alone (without action verbs) causes zero predicates → knowledge_edge. "No" = this term is always a gap warning alongside firing predicates.

---

## B32-L2-012-002 — Gap ID behavioral summary

### Gap IDs that cause knowledge_edge when alone:
- gap-vibe-coding-spike-001
- gap-vibe-coding-hack-001 (when no action verb co-occurs)
- gap-vibe-coding-demo-001
- gap-vibe-coding-prototype-001 (when no action verb)
- gap-vibe-coding-dashboard-001
- gap-vibe-coding-style-001 (when "vibe" alone, without "code" root)
- gap-vibe-coding-exploratory-001 (when "explore/exploratory" alone, without "coding")

### Gap IDs that are always gap warnings (never alone-cause knowledge_edge):
- gap-vibe-coding-rough-cut-001
- gap-vibe-coding-intuition-001
- gap-vibe-coding-throwaway-001
- gap-vibe-coding-quick-dirty-001
- gap-vibe-coding-just-works-001
- gap-vibe-coding-nothing-fancy-001

**Reason:** These "always warning" gap IDs are modifiers — they modify the quality/style of an action. They don't appear without an action context. "Quick and dirty" is always modifying something ("quick and dirty implementation"); it doesn't stand alone.

---
