# Layer 0-C — Knowledge Edge Pairs (010)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E010-001 — "QA" in any compound fires QA universally

A: "Schedule the QA review session." → ["QA"]
  A: "Write the QA test plan." → ["QA", "TEST"] (TEST fires on "test")
  A: "Run the QA regression suite." → ["QA", "TEST"] ("regression suite" = TEST)
  Teaching: "QA" in any compound noun or imperative fires QA. No context dependency.

---

## E010-002 — "CI/CD" vocabulary is universally gap

A: "Configure the CI/CD (Continuous Integration/Continuous Delivery) pipeline trigger." → []
  Teaching: CI/CD, continuous integration, continuous delivery, automated pipeline, workflow trigger → gap.

---

## E010-003 — "build" fires IMPLEMENT (not QA)

A: "Build the QA test runner and get QA sign-off." → ["QA", "IMPLEMENT"]
  "build" → IMPLEMENT | "QA test runner" → QA | "QA sign-off" → QA (deduplicated with QA from compound)
  Teaching: "build" fires IMPLEMENT. "QA" fires QA. Two separate predicates from one sentence.

---

## E010-004 — "validate" fires TEST (not QA)

A: "Validate the OrderService API responses before QA sign-off." → ["QA", "TEST"]
  "validate" → TEST (universal rule) | "QA sign-off" → QA
  Teaching: "validate" always fires TEST. Never fires QA. QA requires "QA" abbreviation.

---

## E010-005 — "QA criteria" fires QA

A: "Define the QA criteria for the production readiness review." → ["QA"]
  "QA criteria" → QA | "define" → not ARCHITECTURE here (QA criteria ≠ structural design)
  Teaching: "QA criteria" fires QA. "define" in QA criteria context does not fire ARCHITECTURE.

