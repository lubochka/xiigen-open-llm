# Layer 3 — Teaching Unit Pairs (024)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-024-001 — B32 complete rules: the 7 absolute rules

**Teaching unit: all B32 rules stated as absolute constraints**

**Rule 1:** Gap vocabulary alone → predicates = [] → knowledge_edge → SCHEDULE_CAPABILITY_LEARNING (once per request).

**Rule 2:** Gap vocabulary + action verb → predicate fires → active flow + CapabilityGapWarning: style_qualifier_noted. SCHEDULE_CAPABILITY_LEARNING NOT emitted.

**Rule 3:** No style qualifier suppresses any registered action predicate. Ever. (v6-02 corruption if violated.)

**Rule 4:** "dashboard" is NOT in UI lexicon. Always produces gap-vibe-coding-dashboard-001. Never fires UI.

**Rule 5:** "vibe code" / "vibe coding" contain "code" root → IMPLEMENT fires independently of "vibe" gap warning.

**Rule 6:** SCHEDULE_CAPABILITY_LEARNING emits exactly once per request when predicates = []. Multiple gap IDs do not multiply this event.

**Rule 7:** DomainId for all B32 outputs is "software-development". Never "project-management", never "vibe-coding".

---

## B32-L3-024-002 — The three corruption types for B32

**v6-01:** "VIBE_CODING" predicate used. VIBE_CODING is not a registered predicate. Any output including VIBE_CODING is corrupt.

**v6-02:** Action verb present but predicates = []. Style framing suppressed a predicate it has no authority to suppress.

**v6-03:** DomainId is "project-management" instead of "software-development". B32 always uses "software-development".

**Detection priority:** v6-02 is the most common corruption risk for the gap + action verb pattern. Always verify: if any of implement, build, create, write, code, design, architect, integrate, secure, test, document, QA are present → their predicate must appear in the output.

---
