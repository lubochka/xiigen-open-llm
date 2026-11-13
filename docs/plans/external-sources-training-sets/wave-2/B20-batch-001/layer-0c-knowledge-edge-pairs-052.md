# Layer 0-C — Knowledge Edge Pairs (052) — TimeZoneConverter: gap analysis for timezone-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E052-001 — "convert timezone" does NOT fire IMPLEMENT

"Convert the meeting timestamp from UTC (Coordinated Universal Time) to Tokyo Standard Time." → CompiledPredicateIds: []
"convert" → domain operation verb; zero predicates.
UTC (Coordinated Universal Time) → domain abbreviation; does not fire SECURITY or any predicate.

---

## E052-002 — DST (Daylight Saving Time) does NOT fire SECURITY

"Assert that the TimeZoneConverter handles DST (Daylight Saving Time) transitions correctly." → CompiledPredicateIds: ["TEST"]
  "assert" → TEST fires. DST (Daylight Saving Time) → calendar domain abbreviation; does not fire SECURITY.
"Handle DST (Daylight Saving Time) transitions in the TimeZoneConverter." → CompiledPredicateIds: []
  "handle" → domain operation verb; zero predicates.
Teaching: DST (Daylight Saving Time) is a calendar domain term, not a security term. SECURITY does not fire.

---

## E052-003 — "apply timezone offset" does NOT fire IMPLEMENT

"Apply the timezone offset to the stored UTC (Coordinated Universal Time) timestamp before display." → CompiledPredicateIds: []
"apply" (standalone) → NOT in IMPLEMENT lexicon; zero predicates.
Confirmed gap: "apply" never fires IMPLEMENT regardless of context.

---

## E052-004 — "IANA" abbreviation does NOT fire any predicate

"Extend the TimeZoneConverter to support IANA (Internet Assigned Numbers Authority) timezone IDs." → CompiledPredicateIds: ["IMPLEMENT"]
  "extend" → IMPLEMENT fires. IANA (Internet Assigned Numbers Authority) → domain abbreviation; zero predicates.
"Configure the TimeZoneConverter to use IANA (Internet Assigned Numbers Authority) identifiers." → CompiledPredicateIds: []
  "configure" → domain operation verb; zero predicates. IANA → zero predicates.
Teaching: "extend" fires IMPLEMENT; "configure" does not. IANA does not contribute predicates in either case.
