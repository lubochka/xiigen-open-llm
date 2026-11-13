# Layer 0-A — Compiler Pairs (003)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-003-001 — "build" overrides "quick and dirty": IMPLEMENT fires

**Request:** "Build something quick and dirty for the shopping cart — just make it work."

**Compiler analysis:**

Step 1 — scan for action predicates:
- "build" → IMPLEMENT fires on "build" ✓
- "quick and dirty" → GAP vocabulary; gap-vibe-coding-quick-dirty-001
- "just make it work" → GAP vocabulary; gap-vibe-coding-just-works-001
- "shopping cart" → domain noun; no predicate

Step 2 — UI vocabulary scan:
- No UI trigger words present (screen, page, interface, component, form, layout, button, input, widget, modal)

Step 3 — gap vocabulary:
- "quick and dirty" → gap-vibe-coding-quick-dirty-001
- "just make it work" → gap-vibe-coding-just-works-001

**Result:** `["IMPLEMENT"]`

**CapabilityGapWarnings:**
- gap-vibe-coding-quick-dirty-001 ("quick and dirty")
- gap-vibe-coding-just-works-001 ("just make it work")

**OutputDecision:** route to IMPLEMENT flow (BackendServiceFlow41)

**TransitionType:** feature_addition

**DomainId:** "software-development"

**Key rule confirmed:** Style qualifiers ("quick and dirty", "just make it work") NEVER suppress action predicates. IMPLEMENT fires on "build" regardless of surrounding style framing. Gap warnings are emitted alongside routing.

---

## B32-0A-003-002 — "just make it work" alone: no action verb → zero predicates

**Request:** "Just make it work — I don't care how."

**Compiler analysis:**

- "just make it work" → GAP vocabulary; gap-vibe-coding-just-works-001
- "make" → ambiguous; in isolation without subject ("make a service", "make it build") it does not fire IMPLEMENT
- No action predicates from IMPLEMENT lexicon (implement, build, create, write, code)

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-just-works-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-003-003 — "create" + "quick and dirty": IMPLEMENT fires; gap warnings alongside

**Request:** "Create a quick and dirty parser for the config files."

**Compiler analysis:**

- "create" → IMPLEMENT fires on "create" ✓
- "quick and dirty" → GAP; gap-vibe-coding-quick-dirty-001
- "parser" → domain noun; no additional predicate
- "config files" → domain noun

**Result:** `["IMPLEMENT"]`

**CapabilityGapWarnings:** gap-vibe-coding-quick-dirty-001

**OutputDecision:** route to IMPLEMENT flow

**TransitionType:** feature_addition

---

## B32-0A-003-004 — "write" + "just make it work": IMPLEMENT fires

**Request:** "Write something for the email sender — just make it work for the demo."

**Compiler analysis:**

- "write" → IMPLEMENT fires ✓
- "just make it work" → GAP; gap-vibe-coding-just-works-001
- "demo" (standalone) → GAP; gap-vibe-coding-demo-001
- "email sender" → domain noun

**Result:** `["IMPLEMENT"]`

**CapabilityGapWarnings:**
- gap-vibe-coding-just-works-001 ("just make it work")
- gap-vibe-coding-demo-001 ("demo")

**OutputDecision:** route to IMPLEMENT flow

---

## B32-0A-003-005 — "good enough": gap framing with "implement"

**Request:** "Implement something good enough for the search filter — nothing production-grade."

**Compiler analysis:**

- "implement" → IMPLEMENT fires ✓
- "good enough" → GAP; gap-vibe-coding-style-001
- "nothing" context: "nothing production-grade" is a qualifier, not a predicate suppressor

**Result:** `["IMPLEMENT"]`

**CapabilityGapWarnings:** gap-vibe-coding-style-001 ("good enough")

**OutputDecision:** route to IMPLEMENT flow

---
