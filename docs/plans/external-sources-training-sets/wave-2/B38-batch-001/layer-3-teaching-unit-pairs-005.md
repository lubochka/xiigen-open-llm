# Layer 3 — Teaching Unit Pairs (005)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B38-TU-005-001 — Teaching: Complete QA lexicon in release context

LESSON: These terms fire the QA predicate in B38.

CORE QA TERMS (always fire QA in any B38 sentence):
1. review → QA
2. approve / approved / approval → QA
3. verify / verification → QA
4. validate / validation → QA
5. sign-off / sign off → QA
6. audit (in release or security context) → QA
7. quality → QA
8. compliance → QA (also SECURITY in security context)
9. inspect / inspection → QA
10. accept / acceptance (in quality/gate context) → QA

---

## B38-TU-005-002 — Teaching: QA term examples with sentences

1. review: "Review the release candidate." → ["QA"]
2. approve: "The release has been approved." → ["QA"]
3. verify: "Verify the build integrity." → ["QA","IMPLEMENT"] (build also fires)
4. validate: "Validate the deployment readiness." → ["QA"] (deployment is gap)
5. sign-off: "Get the sign-off from the QA team." → ["QA"]
6. audit: "Audit the release process." → ["QA"]
7. quality: "Quality gate check before release." → ["QA"]
8. compliance: "Ensure compliance before the go-live." → ["QA"] + gap warning on go-live
9. inspect: "Inspect the release artifacts." → ["QA"]
10. acceptance: "Acceptance review complete." → ["QA"]

---

## B38-TU-005-003 — Teaching: QA borderline terms

These terms are QA-adjacent but not guaranteed:

"check" → QA in quality-gate context; NOT QA in checklist context (checklist is gap)
"test" → NOT QA (TEST is a separate predicate; QA ≠ TEST in B38)
"confirm" → borderline; not a core QA term
"assess" → not in core QA lexicon
"evaluate" → not in core QA lexicon
"certify" → not in core QA lexicon for B38

When in doubt: if the term is not in the core 10-item list above, do not assume QA fires.

---

## B38-TU-005-004 — Teaching: Multiple QA terms in one sentence = one QA predicate

"Review and approve and verify and validate the release."
→ review (QA) + approve (QA) + verify (QA) + validate (QA)
→ ONE QA predicate (predicates are deduplicated)
→ PredicateSet: ["QA"]
→ DecisionCount: 1

Multiple QA-firing terms do NOT produce multiple QA predicates. They all consolidate into a single QA predicate. The predicate set is a set (no duplicates), not a list.
