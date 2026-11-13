# Layer 0-A — Compiler Pairs (018)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-018-001 — "throwaway" alone: zero predicates

**Request:** "Throwaway code — just throwaway, we'll rewrite later."

**Compiler analysis:**

- "throwaway" → gap-vibe-coding-throwaway-001
- No action verbs present
- "rewrite" → present but the sentence frames it as future intent, not a current request; however "rewrite" roots to "write" → evaluate carefully

**Step: evaluate "rewrite later":**
- "rewrite later" states a future action not being requested now
- The immediate action framing is "throwaway code" with no imperative action verb
- Treating "rewrite" as firing IMPLEMENT would be over-triggering on future-tense intent
- Conservative rule: "rewrite later" (future frame) does not fire IMPLEMENT for the current request

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-throwaway-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-018-002 — "throwaway" + "write" → IMPLEMENT fires on "write"

**Request:** "Write throwaway code for the data migration — we'll toss it after."

**Compiler analysis:**

- "throwaway" → gap-vibe-coding-throwaway-001
- "write" → IMPLEMENT fires

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-throwaway-001

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-018-003 — "throwaway prototype" alone: both are gap; no predicate

**Request:** "Just a throwaway prototype — delete it after the demo."

**Compiler analysis:**

- "throwaway" → gap-vibe-coding-throwaway-001
- "prototype" → no action verb → gap-vibe-coding-prototype-001
- "demo" → standalone → gap-vibe-coding-demo-001

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-throwaway-001, gap-vibe-coding-prototype-001, gap-vibe-coding-demo-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-018-004 — "throwaway code" + "implement" → IMPLEMENT fires

**Request:** "Implement a throwaway version to validate the concept."

**Compiler analysis:**

- "throwaway" → gap-vibe-coding-throwaway-001
- "implement" → IMPLEMENT fires

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-throwaway-001

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-018-005 — "throwaway UI" — "throwaway" is gap; "UI" alone is not a UI lexicon trigger

**Request:** "Throwaway UI for the admin panel — rough throwaway."

**Compiler analysis:**

- "throwaway" → gap-vibe-coding-throwaway-001
- "UI" → "UI" as a noun abbreviation is NOT in the UI lexicon trigger list (screen, page, interface, component, form, layout, button, input, widget, modal are the triggers)
- "panel" → NOT in UI lexicon
- No action verbs present

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-throwaway-001

**Note:** "UI" as an abbreviation and "panel" are NOT UI lexicon triggers. The UI predicate requires one of: screen, page, interface, component, form, layout, button, input, widget, modal.

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---
