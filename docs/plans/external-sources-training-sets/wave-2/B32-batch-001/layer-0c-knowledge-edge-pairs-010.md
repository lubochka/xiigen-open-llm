# Layer 0-C — Knowledge-Edge Pairs (010)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-010-001 — Complete B32 gap vocabulary list with gap IDs

**Knowledge edge:** Complete authoritative list of B32 gap vocabulary with their registered gap IDs.

### Registered B32 Gap IDs

| Gap ID | Triggers | Category |
|---|---|---|
| gap-vibe-coding-style-001 | vibe, vibe code, vibe coding, vibe out, creative mode, good enough, scaffold (alone) | Core vibe style |
| gap-vibe-coding-exploratory-001 | exploratory, explore (without design context) | Exploration |
| gap-vibe-coding-prototype-001 | prototype (alone, no action verb) | Artifact type |
| gap-vibe-coding-dashboard-001 | dashboard | Unrecognized UI noun |
| gap-vibe-coding-spike-001 | spike, "run a spike", "spike on" | Technical investigation |
| gap-vibe-coding-hack-001 | hack together, cobble together, throw together, put together, hack | Informal assembly |
| gap-vibe-coding-demo-001 | demo (standalone) | Informal presentation |
| gap-vibe-coding-rough-cut-001 | rough-cut, rough cut, rough draft | Draft quality |
| gap-vibe-coding-intuition-001 | intuition-driven, feel right, feels right, make it feel, intuitive (as style) | Intuition framing |
| gap-vibe-coding-throwaway-001 | throwaway, throwaway code, throwaway prototype | Disposability |
| gap-vibe-coding-quick-dirty-001 | quick and dirty, quick-and-dirty | Quality reduction |
| gap-vibe-coding-just-works-001 | just make it work, just get it working, just working | Minimum bar |
| gap-vibe-coding-nothing-fancy-001 | nothing fancy, not fancy, simple (when used as vibe qualifier) | Simplicity framing |

---

## B32-0C-010-002 — Gap ID naming convention

**Format:** `gap-vibe-coding-{term}-001`

All B32 gap IDs share the prefix `gap-vibe-coding-`. The `{term}` segment is a kebab-case descriptor of the gap category. The suffix `-001` is the version number (first registered version of this gap ID).

This prefix clearly distinguishes B32 gap IDs from:
- B36 gap IDs: `gap-b36-{term}-001`
- B37 gap IDs: `gap-b37-{term}-001`

---

## B32-0C-010-003 — Unregistered gap vocabulary: when new terms appear

**When a B32 request contains vibe-style vocabulary not in the registered gap ID list:**

The system should:
1. Recognize it as likely vibe vocabulary (pattern: informal, style-oriented, anti-quality framing)
2. Emit gap-vibe-coding-style-001 as the closest match (general vibe style)
3. Flag it as a candidate for a new gap ID in the learning pipeline
4. NOT invent a new gap ID inline (gap IDs are registered, not invented at runtime)

**Example:** "wing it" (not in registered list)
- Closest match: gap-vibe-coding-style-001 (general vibe style)
- Learning flag: candidate for gap-vibe-coding-wing-it-001

---
