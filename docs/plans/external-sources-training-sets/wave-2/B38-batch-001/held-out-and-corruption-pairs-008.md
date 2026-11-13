# Held-Out and Corruption Pairs (008)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## B38-HO-008-001 — HELD-OUT: four predicates (QA+IMPLEMENT+DOCUMENT+SECURITY)

Input: "Review the payment service, validate the release criteria, audit the security posture, and approve the release."

Correct result:
- "Review" → QA
- "payment service" → IMPLEMENT
- "validate" → QA
- "release criteria" → DOCUMENT (criteria fires)
- "audit" → QA + SECURITY (security context from "security posture")
- "security" → SECURITY
- "approve" → QA
- PredicateSet: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]
- CapabilityGapWarnings: []
- TransitionType: feature_addition
- Phase routing: 4.1 + 2.1 + 3.4 + 7.1 (QA last)
- DecisionCount: 4

Gate B (held-out assertion): len(PredicateSet) == 4 AND TransitionType == "feature_addition"

---

## B38-HO-008-002 — CORRUPTION: collapsed to QA only

Corrupt input processing (v6-03 variant):
- Multiple predicates detected but collapsed to highest-priority predicate
- "Review" → QA ✓ (kept)
- "payment service" → IMPLEMENT (dropped by collapse)
- "release criteria" → DOCUMENT (dropped by collapse)
- "security" → SECURITY (dropped by collapse)

Corrupted result:
- PredicateSet: ["QA"] (WRONG — should be all four)
- TransitionType: no_contribution (WRONG — should be feature_addition)
- DecisionCount: 1 (WRONG — should be 4)
- Phase routing: 7.1 only (WRONG — should include 4.1, 2.1, 3.4)

Why this is wrong:
v6-03 corruption. Rule 15: two predicates → two independent routing decisions. This applies to four predicates too: four predicates → four independent routing decisions. No collapse permitted.

---

## B38-HO-008-003 — HELD-OUT: detecting four-predicate inputs

An input is likely to produce four predicates when it contains:
1. A QA term (review/approve/validate/etc.)
2. A service/build/pipeline term (fires IMPLEMENT)
3. A criteria term (fires DOCUMENT)
4. A security term (fires SECURITY)

Any of these absent → three or fewer predicates.
All four present → all four predicates fire.

The held-out test for four-predicate inputs: look for all four vocabulary domains in the input text.
