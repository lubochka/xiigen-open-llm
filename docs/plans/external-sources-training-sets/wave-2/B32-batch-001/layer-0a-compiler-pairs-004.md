# Layer 0-A — Compiler Pairs (004)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-004-001 — "screen" fires UI even in vibe context

**Request:** "Vibe code a quick screen for the product listing — make it feel intuitive."

**Compiler analysis:**

Step 1 — scan for action predicates:
- "code" → IMPLEMENT fires on "code"? Note: "vibe code" — "code" here is part of "vibe code" phrasing; however "vibe code" is gap vocabulary. The "code" root fires IMPLEMENT on exact/root match.

Step 2 — UI vocabulary scan:
- "screen" → UI fires ✓ ("screen" is in UI lexicon: screen, page, interface, component, form, layout, button, input, widget, modal)

Step 3 — gap vocabulary:
- "vibe code" → gap-vibe-coding-style-001
- "feel intuitive" → gap-vibe-coding-intuition-001

Step 4 — predicate resolution:
- UI fires on "screen"
- IMPLEMENT fires on "code" (root match from "vibe code")

**Note on "vibe code":** The "code" root within "vibe code" fires IMPLEMENT; "vibe" marks the phrase as gap but does not suppress the "code" root trigger.

**Result:** `["UI", "IMPLEMENT"]`

**CapabilityGapWarnings:**
- gap-vibe-coding-style-001 ("vibe code")
- gap-vibe-coding-intuition-001 ("feel intuitive")

**OutputDecision:** route to UI flow + IMPLEMENT flow

**TransitionType:** feature_addition

---

## B32-0A-004-002 — "screen" alone (no vibe framing): clean UI result

**Request:** "Build a product listing screen with filtering options."

**Compiler analysis:**

- "build" → IMPLEMENT fires ✓
- "screen" → UI fires ✓
- No gap vocabulary present

**Result:** `["UI", "IMPLEMENT"]`

**GapWarnings:** none

**OutputDecision:** route to UI + IMPLEMENT flows

---

## B32-0A-004-003 — "vibe" alone with screen: gap warning emitted; UI still fires

**Request:** "Vibe out a screen for the settings panel."

**Compiler analysis:**

- "vibe" → GAP; gap-vibe-coding-style-001
- "screen" → UI fires ✓
- "out" → not an action predicate
- "settings panel" → domain noun

**Result:** `["UI"]`

**CapabilityGapWarnings:** gap-vibe-coding-style-001 ("vibe")

**OutputDecision:** route to UI flow

---

## B32-0A-004-004 — "intuitive" and "feel right" as gap markers

**Request:** "Build the onboarding flow screen — make it feel right, intuition-driven design."

**Compiler analysis:**

- "build" → IMPLEMENT fires ✓
- "screen" → UI fires ✓
- "feel right" → GAP; gap-vibe-coding-intuition-001
- "intuition-driven" → GAP; gap-vibe-coding-intuition-001

**Result:** `["UI", "IMPLEMENT"]`

**CapabilityGapWarnings:** gap-vibe-coding-intuition-001 (multiple occurrences; one warning emitted)

**OutputDecision:** route to UI + IMPLEMENT flows

---
