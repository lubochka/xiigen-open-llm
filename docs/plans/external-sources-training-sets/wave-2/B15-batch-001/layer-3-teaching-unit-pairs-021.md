# Layer 3 — Teaching Unit Pairs (021)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: teaching_unit

## TU-021-A — "service contracts" fires API_CONTRACT

A: "Implement the reporting service and define the service contracts for report request and response schemas." How many predicates?
B: Two: ["IMPLEMENT", "API_CONTRACT"]. "service contracts" → API_CONTRACT fires on "contract". "service" is the modifier — it does not suppress the trigger. Any "[X] contracts" fires API_CONTRACT.

## TU-021-B — "spec out" fires DOCUMENT

A: "Build the reporting service and spec out the report format." How many predicates?
B: Two: ["IMPLEMENT", "DOCUMENT"]. "spec out" → DOCUMENT fires. "report format" is the documentation subject — not a trigger itself.

## TU-021-C — "document" and "spec out" in same request: deduplicated to one DOCUMENT

A: "Build the reporting service, spec out the report format, and document the scheduling configuration." How many predicates?
B: Two: ["IMPLEMENT", "DOCUMENT"]. "spec out" + "document" = two DOCUMENT triggers → deduplication → single DOCUMENT predicate. DecisionCount: 2.

## TU-021-D — DOCUMENT is parallel-eligible with IMPLEMENT

A: Can documentation be written at the same time as implementation?
B: Yes. DOCUMENT (sub-phase 2.1) is parallel-eligible with IMPLEMENT (sub-phase 4.1). The transition orchestrator ordering rules show: DocumentationFlow21 ‖ ImplementationFlow41 (parallel). This is in contrast to TestingFlow51, which is always sequential after ImplementationFlow41.

## TU-021-E — "integrate with data warehouse" fires API_CONTRACT

A: "Build the reporting service and integrate with the analytics data warehouse API." How many predicates?
B: Two: ["IMPLEMENT", "API_CONTRACT"]. "integrate" → API_CONTRACT fires. "data warehouse" is an internal or external system — API_CONTRACT fires on "integrate" regardless of whether the system is technically internal.
