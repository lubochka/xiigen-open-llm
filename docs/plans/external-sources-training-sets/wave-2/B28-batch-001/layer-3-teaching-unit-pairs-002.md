# Layer 3 — Teaching Unit Pairs (002)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU002-001 — QA abbreviation gate (not "quality assurance")

A: "Quality assurance sign-off needed before release."
B: [] — "quality assurance" spelled out → gap. QA predicate fires ONLY on abbreviation "QA".
Reason: Strict abbreviation gate. Full-form "quality assurance" is gap vocabulary regardless of context.
CompiledPredicateIds: []

---

## TU002-002 — QA fires on abbreviation in any syntactic position

A: "The QA team has not approved this build."
B: ["QA"] — QA appears as noun modifier → fires QA predicate.
Reason: "QA" in compound noun "QA team" → QA predicate. Position is irrelevant; presence is sufficient.
CompiledPredicateIds: ["QA"]

---

## TU002-003 — QA abbreviation case-sensitive

A: "qa sign-off required."
B: ["QA"] — lowercase "qa" is treated as abbreviation equivalent to "QA" → QA predicate.
Reason: Abbreviation gate is case-insensitive. "qa", "Qa", "QA" all fire QA predicate.
CompiledPredicateIds: ["QA"]

---

## TU002-004 — "quality" alone is gap

A: "Improve the quality of the payment flow."
B: [] — "quality" without "assurance" or "QA" abbreviation → gap.
Reason: Neither design verb nor QA abbreviation present. Vague quality improvement → gap.
CompiledPredicateIds: []

---

## TU002-005 — "assurance" alone is gap

A: "We need assurance that the build is stable."
B: [] — "assurance" without "QA" abbreviation → gap.
Reason: "Assurance" is not a trigger keyword. Gap sentence.
CompiledPredicateIds: []
