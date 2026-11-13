# Layer 0-A — Compiler Pairs (008)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-008-001 — UI + IMPLEMENT: vibe coding mixed with action verbs

**Request:** "Vibe code the user profile screen and implement the avatar upload component."

**Compiler analysis:**

Step 1 — action predicate scan:
- "code" (from "vibe code") → IMPLEMENT fires ✓ (root match)
- "implement" → IMPLEMENT fires ✓ (direct match)
- Both fire the same predicate; result: IMPLEMENT (deduplicated)

Step 2 — UI vocabulary scan:
- "screen" → UI fires ✓
- "component" → UI fires ✓
- Both fire the same predicate; result: UI (deduplicated)

Step 3 — gap vocabulary:
- "vibe code" → gap-vibe-coding-style-001
- "vibe" → covered by gap-vibe-coding-style-001

**Result:** `["UI", "IMPLEMENT"]`

**CapabilityGapWarnings:** gap-vibe-coding-style-001 ("vibe code")

**OutputDecision:** route to UI flow + IMPLEMENT flow

**TransitionType:** feature_addition

**DomainId:** "software-development"

---

## B32-0A-008-002 — "vibe code" + "page": UI fires; IMPLEMENT also fires from "code" root

**Request:** "Vibe code a landing page for the product."

**Compiler analysis:**

- "code" (root from "vibe code") → IMPLEMENT fires ✓
- "page" → UI fires ✓
- "vibe" → GAP; gap-vibe-coding-style-001

**Result:** `["UI", "IMPLEMENT"]`

**CapabilityGapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to UI + IMPLEMENT flows

---

## B32-0A-008-003 — "vibe" + "interface": UI fires; no IMPLEMENT (no action verb)

**Request:** "Vibe out a clean interface for the settings panel."

**Compiler analysis:**

- "vibe" → GAP; gap-vibe-coding-style-001
- "out" → not an action predicate
- "interface" → UI fires ✓
- No IMPLEMENT trigger words (implement, build, create, write, code)

**Result:** `["UI"]`

**CapabilityGapWarnings:** gap-vibe-coding-style-001

**OutputDecision:** route to UI flow only

---

## B32-0A-008-004 — "creative mode" + "build a component": IMPLEMENT + UI

**Request:** "In creative mode — build a sleek modal component for the notifications."

**Compiler analysis:**

- "build" → IMPLEMENT fires ✓
- "modal" → UI fires ✓
- "component" → UI fires ✓ (both fire UI; deduplicated)
- "creative mode" → GAP; gap-vibe-coding-style-001

**Result:** `["UI", "IMPLEMENT"]`

**CapabilityGapWarnings:** gap-vibe-coding-style-001 ("creative mode")

**OutputDecision:** route to UI + IMPLEMENT flows

---
