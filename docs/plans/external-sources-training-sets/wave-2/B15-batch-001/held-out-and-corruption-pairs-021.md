# Held-Out and Corruption Pairs (021)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: held_out_corruption

## HC-021-A — Held-out: ReportingService three predicates

A: "Build the reporting service, integrate with the analytics data warehouse API, and document the report schemas."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"] | feature_addition | DecisionCount: 3

## HC-021-B — Held-out: "service contracts" fires API_CONTRACT

A: "Implement the reporting service and define the service contracts for report request and response schemas."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | "service contracts" → API_CONTRACT fires on "contract".

## HC-021-C — Held-out: "spec out" and "document" deduplicated

A: "Build the reporting service, spec out the report format, and document the scheduling configuration."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | DecisionCount: 2 | "spec out" + "document" → single DOCUMENT after deduplication.

## HC-021-D — Corruption v6-02: "service contracts" → API_CONTRACT suppressed

v6 corrupt: "Implement the reporting service and define the service contracts" → Predicates: ["IMPLEMENT", "DOCUMENT"]
Correct: Predicates: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"] | "contracts" fires API_CONTRACT; "define" fires DOCUMENT; both additional predicates fire.
Error: v6-02. "service contracts" must fire API_CONTRACT on "contract" keyword.

## HC-021-E — Corruption v6-03: three decisions collapsed to one

v6 corrupt: "Build reporting service, integrate with data warehouse API, document report schemas" → DecisionCount: 1
Correct: DecisionCount: 3 — ImplementationFlow41 + APIContractFlow33 + DocumentationFlow21
Error: v6-03 multi-branch collapse.
