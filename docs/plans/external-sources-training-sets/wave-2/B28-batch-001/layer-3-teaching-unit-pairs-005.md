# Layer 3 — Teaching Unit Pairs (005)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU005-001 — QA role titles fire QA

A: "The QA engineer flagged a regression in the payment flow."
B: ["QA"] — "QA engineer" is a QA role title → QA predicate.
Reason: QA role titles (QA engineer, QA lead, QA analyst, QA tester, QA manager) all contain QA abbreviation → fire QA.
CompiledPredicateIds: ["QA"]

---

## TU005-002 — QA lead fires QA

A: "Assign this to the QA lead for final review."
B: ["QA"] — "QA lead" role title → QA predicate.
Reason: Role title with QA abbreviation → fires QA predicate.
CompiledPredicateIds: ["QA"]

---

## TU005-003 — QA team fires QA

A: "Forward the build to the QA team."
B: ["QA"] — "QA team" → QA predicate.
Reason: QA team, QA department, QA group → all fire QA predicate via abbreviation.
CompiledPredicateIds: ["QA"]

---

## TU005-004 — "tester" alone is gap (contrast with QA tester)

A: "A tester should review this change."
B: [] — "tester" without QA abbreviation → gap.
Reason: Generic role nouns (tester, reviewer, analyst) without QA abbreviation → gap vocabulary.
CompiledPredicateIds: []

---

## TU005-005 — QA+IMPLEMENT multi-predicate via role title

A: "Implement the retry logic and hand off to the QA team."
B: ["IMPLEMENT", "QA"] — "implement" → IMPLEMENT; "QA team" → QA.
Reason: Multi-predicate. IMPLEMENT drives TransitionType; QA gates final output. Two flows instantiated.
CompiledPredicateIds: ["IMPLEMENT", "QA"]
