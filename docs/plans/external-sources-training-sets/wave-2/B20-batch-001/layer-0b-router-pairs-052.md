# Layer 0-B — Router Pairs (052) — TimeZoneConverter: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R052-001 — TEST+IMPLEMENT: "extend TimeZoneConverter and add regression tests" routes to 5.1 + 4.1

Request: "Extend the TimeZoneConverter to support IANA (Internet Assigned Numbers Authority) timezone IDs and add regression tests."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R052-002 — TEST alone: "verify" on timezone offset routes to 5.1, documentation

Request: "Verify that the TimeZoneConverter correctly converts UTC (Coordinated Universal Time) to GMT (Greenwich Mean Time) -5 for New York."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R052-003 — Zero predicate: "convert timezone" routes to nowhere

Request: "Convert the meeting timestamp from UTC (Coordinated Universal Time) to Tokyo Standard Time."
  "convert" → domain operation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R052-004 — Zero predicate: "apply timezone offset" routes to nowhere

Request: "Apply the timezone offset to the stored UTC (Coordinated Universal Time) timestamp before display."
  "apply" (standalone) → domain operation verb; not in IMPLEMENT lexicon.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge
