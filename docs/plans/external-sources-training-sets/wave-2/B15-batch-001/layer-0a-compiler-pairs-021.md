# Layer 0-A — Compiler Pairs (021)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-021-001 — ReportingService: "build" → IMPLEMENT

A: "Build the reporting service that generates daily sales summaries, order volume reports, and revenue breakdowns."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Sub-phase: 4.1
Evidence: build → IMPLEMENT; reporting service → backend service noun.

---

## B15-compiler-021-002 — ReportingService: "document" → IMPLEMENT+DOCUMENT

A: "Build the reporting service and document the report generation API with endpoint specs and sample payloads."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; document → DOCUMENT (document/spec out fires DOCUMENT).

---

## B15-compiler-021-003 — ReportingService: "define contracts" → IMPLEMENT+API_CONTRACT

A: "Implement the reporting service and define the service contracts for report request and response schemas."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; "service contracts" → API_CONTRACT ("contract" fires API_CONTRACT; "service contracts" is the modifier but "contract" is the keyword).

---

## B15-compiler-021-004 — ReportingService: "spec out" → IMPLEMENT+DOCUMENT

A: "Create the sales reporting service and spec out the report format and scheduling configuration."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
Evidence: create → IMPLEMENT; "spec out" → DOCUMENT fires on "spec out".

---

## B15-compiler-021-005 — ReportingService: three predicates — build + integrate + document

A: "Build the reporting service, integrate with the analytics data warehouse API, and document the report schemas."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; integrate → API_CONTRACT; document → DOCUMENT. Three independent predicates.
