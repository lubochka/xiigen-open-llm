# Layer 3 — Teaching Unit Pairs (028)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-028-001 — Teaching: "iterative" is a style qualifier

**Teaching unit:** "iterative" → gap-vibe-coding-style-001

"Iterative" describes a process preference (incremental steps). It does not contain an action verb.

**Rule:** "iterative" alone → predicates = [] → knowledge_edge → SCL.

**Anti-pattern:** Treating "iterative" as equivalent to "implement incrementally" is v6-02 corruption.

---

## B32-L3-028-002 — Teaching: SECURITY fires from "secure" verb, not from "security" noun

**Teaching unit:** SECURITY predicate trigger

SECURITY fires when:
- "secure" appears as a positive imperative ("secure the endpoint", "secure the handler")
- "audit security" appears

SECURITY does NOT fire when:
- "security" appears as a noun object ("we need security", "good security")
- "security" appears in "security team", "security policy" (organizational context)

Gap qualifiers never suppress SECURITY when "secure" imperative is present.

---

## B32-L3-028-003 — Teaching: IMPLEMENT takes precedence for TransitionType

**Teaching unit:** TransitionType precedence rule

When IMPLEMENT or UI is present in predicates alongside any other predicate:
- TransitionType = feature_addition (always)

Example:
- [IMPLEMENT, TEST] → feature_addition (not no_contribution)
- [IMPLEMENT, SECURITY] → feature_addition
- [UI, ARCHITECTURE] → feature_addition

DOCUMENT and API_CONTRACT alone → documentation.
TEST, QA, SECURITY alone → no_contribution.

---

## B32-L3-028-004 — Teaching: dashboard gap co-existence with IMPLEMENT

**Teaching unit:** "dashboard" scoping with IMPLEMENT

When "implement the dashboard" appears:
- "implement" → IMPLEMENT fires ✓
- "dashboard" → gap-vibe-coding-dashboard-001 (never UI predicate)
- Both effects are independent

GapWarnings contains gap-vibe-coding-dashboard-001. Predicates contains ["IMPLEMENT"]. The dashboard gap warning does not prevent ImplementationFlow from being created.

---
