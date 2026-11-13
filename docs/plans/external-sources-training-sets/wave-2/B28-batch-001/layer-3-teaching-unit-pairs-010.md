# Layer 3 — Teaching Unit Pairs (010)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU010-001 — QA automation fires QA

A: "Set up QA automation for the regression suite."
B: ["QA"] — "QA automation" → QA compound noun → QA predicate.
Reason: "QA automation" contains QA abbreviation → fires QA. "set up" is operational gap on its own.
CompiledPredicateIds: ["QA"]

---

## TU010-002 — QA automation + IMPLEMENT

A: "Implement the QA automation harness using Selenium."
B: ["IMPLEMENT", "QA"] — "implement" → IMPLEMENT; "QA automation harness" → QA.
Reason: "implement" fires IMPLEMENT; "QA automation harness" fires QA. TransitionType: feature_addition (IMPLEMENT present).
CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## TU010-003 — QA criteria fires QA

A: "Define the QA criteria for the new search feature."
B: ["QA"] — "QA criteria" → QA compound noun → QA predicate.
Reason: "define" + "QA criteria" → QA fires. "define" alone does not fire ARCHITECTURE (no structural noun).
CompiledPredicateIds: ["QA"]

---

## TU010-004 — QA metrics fires QA

A: "Review the QA metrics from last sprint's gate."
B: ["QA"] — "QA metrics" → QA predicate.
Reason: QA metrics, QA KPIs, QA data → all QA compound nouns → fire QA predicate.
CompiledPredicateIds: ["QA"]

---

## TU010-005 — QA process fires QA

A: "Improve the QA process to reduce cycle time."
B: ["QA"] — "QA process" → QA predicate.
Reason: QA process, QA workflow, QA pipeline → QA compound nouns → fire QA predicate.
CompiledPredicateIds: ["QA"]
