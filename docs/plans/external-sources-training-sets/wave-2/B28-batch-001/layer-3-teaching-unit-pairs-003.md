# Layer 3 — Teaching Unit Pairs (003)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU003-001 — QA compound nouns trigger QA

A: "The QA checklist must be completed before the release cut."
B: ["QA"] — "QA checklist" → compound noun with QA abbreviation → QA predicate.
Reason: All QA-prefixed compound nouns fire QA predicate: checklist, gate, sign-off, report, pass, fail, approval, block, review.
CompiledPredicateIds: ["QA"]

---

## TU003-002 — QA sign-off fires QA

A: "We need QA sign-off on the UserService changes."
B: ["QA"] — "QA sign-off" → compound noun → QA predicate.
Reason: "sign-off" alone is gap; "QA sign-off" (with QA abbreviation) fires QA.
CompiledPredicateIds: ["QA"]

---

## TU003-003 — QA gate fires QA

A: "The build is blocked at the QA gate."
B: ["QA"] — "QA gate" → QA predicate.
Reason: "gate" alone is gap; "QA gate" fires QA.
CompiledPredicateIds: ["QA"]

---

## TU003-004 — QA block fires QA

A: "This deployment is under QA block."
B: ["QA"] — "QA block" → QA predicate.
Reason: "QA block" is a QA compound noun. "block" alone is not a trigger.
CompiledPredicateIds: ["QA"]

---

## TU003-005 — sign-off alone is gap (contrast)

A: "Get sign-off before deployment."
B: [] — "sign-off" without QA abbreviation → gap.
Reason: sign-off, approval, release, clearance, go-ahead alone are gap vocabulary. QA abbreviation required.
CompiledPredicateIds: []
