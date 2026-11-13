# Layer 3 — Teaching Unit Pairs (040)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-040-001 — Teaching: informal + TEST patterns

**Teaching unit:** Hack phrases that fire TEST

"Bang out tests" → TEST fires.
"Knock out some unit tests" → TEST fires.
"Whip up a few tests" → TEST fires (tests as object).

All use hack vocabulary (gap-vibe-coding-hack-001) + "tests" as the object.

---

## B32-L3-040-002 — Teaching: aspirational mentions do not fire predicates

**Teaching unit:** Imperatives vs aspirational/modal expressions

Imperatives fire predicates: "test X", "write tests", "secure X", "implement X".
Aspirational/modal do NOT: "would be nice", "should test", "might implement", "considering testing".

This applies to all predicates, not just TEST. "We should secure it" does not fire SECURITY.

---

## B32-L3-040-003 — Teaching: "throwaway" as gap qualifier

**Teaching unit:** "throwaway" → gap-vibe-coding-style-001

"Throwaway" signals disposable-quality framing. It does not contain an action verb.

"Write throwaway tests" → "throwaway" is an adjective modifier; "write tests" fires TEST.
"Throwaway implementation" → "implement" fires IMPLEMENT if explicit; "throwaway" does not.

---

## B32-L3-040-004 — Teaching: TEST TransitionType = no_contribution

**Teaching unit:** TEST's TransitionType

TEST alone → no_contribution.
TEST + IMPLEMENT → feature_addition (IMPLEMENT overrides).
TEST + ARCHITECTURE → documentation (ARCHITECTURE is primary content predicate).

no_contribution does not mean the flow is empty. TestFlow is created; the session is active. The "no_contribution" label refers to the lack of a new feature addition or documentation artifact.

---
