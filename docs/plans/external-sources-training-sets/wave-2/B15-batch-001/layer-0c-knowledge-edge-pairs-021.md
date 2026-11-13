# Layer 0-C — Knowledge-Edge Pairs (021)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-021-001 — "daily summaries" is a domain noun, not a predicate trigger

A: "Build the reporting service that generates daily sales summaries and revenue breakdowns."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "daily sales summaries" — domain noun; "generates" — action verb in IMPLEMENT scope; "revenue breakdowns" — domain noun. Single IMPLEMENT fires on "build".

---

## B15-kedge-021-002 — "service contracts" fires API_CONTRACT on "contract" keyword

A: "Implement the reporting service and define the service contracts for report request and response schemas."
B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Edge analysis: "service contracts" → API_CONTRACT fires on "contract". "service" is the modifier — irrelevant to firing. API_CONTRACT fires on "contract" with any preceding modifier: data contracts, API contracts, service contracts, message contracts, all fire API_CONTRACT.

---

## B15-kedge-021-003 — "document the API" fires DOCUMENT

A: "Build the reporting service and document the report generation API with endpoint specs and sample payloads."
B:
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]
  Edge analysis: "document" → DOCUMENT fires. "endpoint specs" — "specs" could also fire DOCUMENT but deduplication applies — still single DOCUMENT predicate.

---

## B15-kedge-021-004 — "spec out" and "document" both fire DOCUMENT — deduplicated

A: "Build the reporting service, spec out the report format, and document the scheduling configuration."
B:
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]
  Edge analysis: "spec out" → DOCUMENT fires; "document" → DOCUMENT fires again. Deduplication: two DOCUMENT triggers → single DOCUMENT predicate.

---

## B15-kedge-021-005 — "integrate with analytics data warehouse" fires API_CONTRACT

A: "Build the reporting service and integrate with the analytics data warehouse API."
B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Edge analysis: "integrate" → API_CONTRACT fires. "analytics data warehouse API" — named external system confirms integration context. API_CONTRACT fires on "integrate" regardless of whether the system is internal or external.
