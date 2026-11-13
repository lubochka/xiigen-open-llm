# Held-Out and Corruption Pairs (009)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## B38-HO-009-001 — HELD-OUT: "validate acceptance criteria" → QA+DOCUMENT

Input: "Validate the acceptance criteria for the new feature and sign off on the release."

Correct result:
- "Validate" → QA
- "acceptance criteria" → DOCUMENT (criteria fires DOCUMENT)
- "acceptance" → QA (in quality/gate context, acceptance is also a QA signal)
- "sign off" → QA
- "release" → context word
- PredicateSet: ["QA", "DOCUMENT"]
- CapabilityGapWarnings: []
- TransitionType: no_contribution
- Phase routing: 2.1 + 7.1

Gate B (held-out assertion): "DOCUMENT" in PredicateSet AND "IMPLEMENT" not in PredicateSet

---

## B38-HO-009-002 — CORRUPTION: DOCUMENT suppressed

Corrupt input processing:
- "Validate" → QA ✓
- "acceptance criteria" → QA only (WRONG — criteria should also fire DOCUMENT)
- (treating "acceptance criteria" as a QA phrase rather than a DOCUMENT trigger)
- "sign off" → QA ✓

Corrupted result:
- PredicateSet: ["QA"] (WRONG — DOCUMENT missing)
- DecisionCount: 1 (WRONG — should be 2)
- Phase routing: 7.1 only (WRONG — 2.1 missing)

Why this is wrong:
"acceptance criteria" contains the word "criteria". Rule 3 (DOCUMENT ALWAYS for criteria) applies.
Even when "acceptance" is a QA word, "criteria" still independently fires DOCUMENT.
Both predicates fire: "acceptance" → QA; "criteria" → DOCUMENT.
The compound "acceptance criteria" fires both predicates simultaneously.

---

## B38-HO-009-003 — HELD-OUT: both predicates fire from same compound phrase

"acceptance criteria" is a compound phrase where:
- "acceptance" → QA (in quality/gate context)
- "criteria" → DOCUMENT (always)

Both predicates fire. This is not a contradiction — predicates are not mutually exclusive.
One compound phrase can fire two predicates if it contains words from two different predicate lexicons.

Other compound examples:
"quality criteria" → QA (from "quality") + DOCUMENT (from "criteria")
"compliance criteria" → QA (from "compliance") + DOCUMENT (from "criteria")
"security criteria" → SECURITY (from "security") + DOCUMENT (from "criteria")
