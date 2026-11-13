# Held-Out and Corruption Pairs (002)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## B38-HO-002-001 — HELD-OUT: "sign-off" → QA; "launch" → GAP

Input: "Collect sign-offs from all team leads before the product launch."

Correct result:
- "sign-offs" → QA
- "launch" → GAP (gap-project-delivery-launch-001)
- "Collect" → not a predicate
- PredicateSet: ["QA"]
- CapabilityGapWarnings: ["launch"]
- TransitionType: no_contribution
- Phase routing: 7.1
- ResultState: routed

Gate B (held-out assertion): PredicateSet == ["QA"] AND "launch" in CapabilityGapWarnings

---

## B38-HO-002-002 — CORRUPTION: "launch" wrongly fires IMPLEMENT

Corrupt input processing:
- "sign-offs" → QA ✓
- "launch" → IMPLEMENT (WRONG — launch is gap, not code authoring)

Corrupted result:
- PredicateSet: ["QA", "IMPLEMENT"] (WRONG)
- TransitionType: feature_addition (WRONG — should be no_contribution)
- Phase routing: 4.1 + 7.1 (WRONG — 4.1 should not be included)

Why this is wrong:
"launch" is explicitly B38 gap vocabulary (gap-project-delivery-launch-001).
"launch" does not fire IMPLEMENT because launch = go-live-adjacent operational term, not code authoring.
The IMPLEMENT lexicon is: service, build, pipeline, implementation, code. "launch" is not in it.

Diagnostic: If TransitionType = feature_addition for an input with only "sign-off" and "launch" (no service/build/pipeline), v6-03 or a predicate map error is present.

---

## B38-HO-002-003 — HELD-OUT variant: "sign off" as verb vs noun

"Please sign off on the release." → QA (verb form; "sign off" fires QA)
"The sign-off register is complete." → QA (noun form; "sign-off" fires QA)
"We need all sign-offs by Friday." → QA (plural noun; "sign-offs" fires QA)

All three forms fire QA. The sign-off rule applies to all grammatical forms of the term.
