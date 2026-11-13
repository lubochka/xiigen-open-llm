# Layer 0-A — Compiler Pairs (005)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-005-001 — "dashboard" does NOT fire UI

**Request:** "Throw together a quick dashboard for the admin panel — nothing fancy."

**Compiler analysis:**

Step 1 — scan for action predicates:
- "throw together" → GAP vocabulary; gap-vibe-coding-hack-001
- No action verbs from IMPLEMENT lexicon (implement, build, create, write, code) present

Step 2 — UI vocabulary scan:
- "dashboard" → NOT in UI lexicon
  UI fires on: screen, page, interface, component, form, layout, button, input, widget, modal
  "dashboard" is NOT in this list → UI does not fire
- "admin panel" → "panel" is not in UI lexicon → no UI trigger

Step 3 — gap vocabulary:
- "throw together" → gap-vibe-coding-hack-001
- "nothing fancy" → gap-vibe-coding-nothing-fancy-001
- "quick" → gap qualifier (general vibe framing)

**Result:** `[]` — zero predicates fire

**CapabilityGapWarnings:**
- gap-vibe-coding-hack-001 ("throw together")
- gap-vibe-coding-nothing-fancy-001 ("nothing fancy")
- gap-vibe-coding-dashboard-001 ("dashboard" — domain noun in gap context)

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-005-002 — "dashboard" + "build": IMPLEMENT fires; dashboard still not UI

**Request:** "Build a dashboard for the analytics team."

**Compiler analysis:**

- "build" → IMPLEMENT fires ✓
- "dashboard" → NOT in UI lexicon; UI does not fire
- No other UI vocabulary present

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-dashboard-001 ("dashboard" — unrecognized UI noun)

**OutputDecision:** route to IMPLEMENT flow

**Note:** "dashboard" causes a gap warning but does not trigger UI. The system recognizes it is attempting UI work but cannot route it through the UI predicate lexicon. This is the boundary behavior.

---

## B32-0A-005-003 — "dashboard" + "screen": screen fires UI; dashboard still gap

**Request:** "Build a dashboard with a main screen showing the metrics."

**Compiler analysis:**

- "build" → IMPLEMENT fires ✓
- "screen" → UI fires ✓
- "dashboard" → NOT in UI lexicon; gap warning emitted

**Result:** `["UI", "IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-dashboard-001 ("dashboard")

**OutputDecision:** route to UI + IMPLEMENT flows

**Teaching point:** Even when "dashboard" is present, if the request also contains a registered UI term ("screen"), UI fires on that registered term. The "dashboard" gap warning is still emitted.

---

## B32-0A-005-004 — "nothing fancy" alone (no dashboard): gap warning; may or may not fire predicates

**Request:** "Nothing fancy — just put together a settings form."

**Compiler analysis:**

- "nothing fancy" → GAP; gap-vibe-coding-nothing-fancy-001
- "form" → UI fires ✓ ("form" is in UI lexicon)
- "put together" → GAP synonym; gap-vibe-coding-hack-001
- "settings" → domain noun

**Result:** `["UI"]`

**CapabilityGapWarnings:**
- gap-vibe-coding-nothing-fancy-001 ("nothing fancy")
- gap-vibe-coding-hack-001 ("put together")

**OutputDecision:** route to UI flow

---
