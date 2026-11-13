# Layer 3 — Teaching Unit Pairs (020)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU020-001 — QA alone: A/B testing gate (no TransitionType)

A: "QA review the A/B testing release before go-live."
B: ["QA"] — "QA review" → QA; "A/B testing" → gap; "go-live" → gap.
Reason: QA alone → no TransitionType. Gate-only output.
CompiledPredicateIds: ["QA"]

---

## TU020-002 — Gap: A/B configuration

A: "Configure the A/B test with 50/50 traffic split between control and treatment."
B: [] — "configure A/B test" → gap; "traffic split" → gap; "control/treatment" → gap.
Reason: A/B test configuration → gap. knowledge_edge.
CompiledPredicateIds: []

---

## TU020-003 — QA+IMPLEMENT: A/B experiment service

A: "Build the A/B experiment assignment service and QA the variant selection logic."
B: ["IMPLEMENT", "QA"] — "build" → IMPLEMENT; "service" → IMPLEMENT (dedup); "QA" → QA.
Reason: Two predicates. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## TU020-004 — QA+TEST: variant verification (no IMPLEMENT)

A: "Verify the A/B variant assignment and QA the statistical significance criteria."
B: ["TEST", "QA", "DOCUMENT"] — "verify" → TEST; "QA" → QA; "criteria" → DOCUMENT.
Reason: Three predicates. "criteria" fires DOCUMENT independently. TransitionType: documentation (no IMPLEMENT).
CompiledPredicateIds: ["TEST", "QA", "DOCUMENT"]

---

## TU020-005 — "A/B" is not a predicate trigger; "QA" is

A: "A/B test the checkout flow." vs "QA the A/B test for the checkout flow."
B1: [] — "A/B test" as verb → gap; no QA abbreviation.
B2: ["QA"] — "QA" fires; "A/B test" object → gap.
Reason: "A/B" alone never fires any predicate. Only "QA" fires QA predicate.
CompiledPredicateIds (B1): [] | CompiledPredicateIds (B2): ["QA"]
