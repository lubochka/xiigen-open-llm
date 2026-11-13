# Held-Out and Corruption Pairs (004)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## B38-HO-004-001 — HELD-OUT: "build artifacts" fires IMPLEMENT → QA+IMPLEMENT

Input: "Inspect the build artifacts before approving the release."

Correct result:
- "Inspect" → QA
- "build" → IMPLEMENT (rule 6: build fires even in artifact context)
- "artifacts" → context word (not a standalone predicate)
- "approving" → QA
- PredicateSet: ["QA", "IMPLEMENT"]
- CapabilityGapWarnings: []
- TransitionType: feature_addition
- Phase routing: 4.1 + 7.1

Gate B (held-out assertion): PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"

---

## B38-HO-004-002 — CORRUPTION: "build" suppressed in artifact context

Corrupt input processing:
- "Inspect" → QA ✓
- "build" → context word (WRONG — "build artifacts" treated as a compound where "build" doesn't fire)
- "artifacts" → context word ✓
- "approving" → QA ✓

Corrupted result:
- PredicateSet: ["QA"] (WRONG — IMPLEMENT should be present from "build")
- TransitionType: no_contribution (WRONG — should be feature_addition)
- Phase routing: 7.1 only (WRONG — 4.1 missing)

Why this is wrong:
Rule 6 is absolute: "build" fires IMPLEMENT even in "build artifacts" and "build provenance". The noun "artifacts" does not suppress the IMPLEMENT-firing power of "build". "Build" is evaluated independently.

Diagnostic: If "build" appears in input but IMPLEMENT is absent from PredicateSet, rule 6 is not being applied.

---

## B38-HO-004-003 — HELD-OUT: "build provenance" fires IMPLEMENT

Input: "Review the build provenance records to verify the artifact chain."
Correct: ["QA", "IMPLEMENT"] — "build" fires IMPLEMENT; "review"/"verify" fire QA
Phase routing: 4.1 + 7.1; TransitionType: feature_addition

This is the same rule as "build artifacts". The word "provenance" (meaning origin/history) does not suppress IMPLEMENT. "Build" fires IMPLEMENT in both "build artifacts" and "build provenance".
