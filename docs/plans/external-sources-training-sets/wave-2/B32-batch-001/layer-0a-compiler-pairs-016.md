# Layer 0-A — Compiler Pairs (016)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-016-001 — "hack together" → gap only; zero predicates

**Request:** "Let's hack together a quick solution for the bug."

**Compiler analysis:**

- "hack together" → gap-vibe-coding-hack-001
- "quick" → style qualifier; not a predicate
- "solution" → domain noun

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-hack-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-016-002 — "hack the service" → IMPLEMENT fires on implied action verb "hack" in service context

**Request:** "Hack the payment service to support the new fee structure."

**Compiler analysis:**

- "hack" here is used as an action verb (to modify/patch/build), NOT as "hack together"
- When "hack" appears without "together/together/cobble" and targets a concrete system artifact, it roots-matches like "build"
- However: "hack" (alone as a verb) is still classified as gap-vibe-coding-hack-001 in this batch
- "support" → not in IMPLEMENT lexicon
- "fee structure" → domain noun

**Important clarification:** In B32 batch rules, "hack" alone (as a verb without "together") → gap-vibe-coding-hack-001. It does NOT fire IMPLEMENT. The phrase "hack together" is the canonical gap form; "hack [the X]" similarly produces gap vocabulary.

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-hack-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-016-003 — "hack together" + "build" → IMPLEMENT fires

**Request:** "Build and hack together the reporting dashboard — nothing fancy."

**Compiler analysis:**

- "hack together" → gap-vibe-coding-hack-001
- "build" → IMPLEMENT fires
- "dashboard" → NOT in UI lexicon → gap-vibe-coding-dashboard-001
- "nothing fancy" → gap-vibe-coding-nothing-fancy-001

**Result:** `["IMPLEMENT"]`

**GapWarnings:** gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-nothing-fancy-001

**OutputDecision:** route to IMPLEMENT flow

**DomainId:** "software-development"

---

## B32-0A-016-004 — "cobble together" → same gap as "hack together"

**Request:** "Cobble together a solution for the cache invalidation problem."

**Compiler analysis:**

- "cobble together" → gap-vibe-coding-hack-001 (same gap ID as "hack together")
- "solution" → domain noun; no predicate

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-hack-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---

## B32-0A-016-005 — "throw together" → same gap as "hack together"

**Request:** "Throw together a prototype — throwaway code is fine."

**Compiler analysis:**

- "throw together" → gap-vibe-coding-hack-001
- "prototype" → no action verb → gap-vibe-coding-prototype-001
- "throwaway" → gap-vibe-coding-throwaway-001

**Result:** `[]`

**GapWarnings:** gap-vibe-coding-hack-001, gap-vibe-coding-prototype-001, gap-vibe-coding-throwaway-001

**OutputDecision:** knowledge_edge

**DomainId:** "software-development"

**CapabilityAction:** SCHEDULE_CAPABILITY_LEARNING

---
