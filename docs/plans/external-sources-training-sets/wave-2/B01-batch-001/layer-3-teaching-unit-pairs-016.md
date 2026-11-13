# Layer 3 — Teaching Unit Pairs (016)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B01-L3-016-001 — PairParserUnit: files 016

```
source_id: xiigen-b01-l3-016-001
pair_kind: teaching_unit
```

A: Theme: "SearchResults and FilterPanel expansion"
B:
  ParsedPairs:
    - Layer: "0-A" | PairCount: 5 | Theme: "SearchResults and FilterPanel UI cases"
    - Layer: "0-B" | PairCount: 4 | Theme: "SearchResults and FilterPanel routing"
    - Layer: "0-C" | PairCount: 3 | Theme: "form IS UI for search; search algorithm → IMPLEMENT; card sorting → gap"
    - Layer: "0-D" | PairCount: 3 | Theme: "trusted_learned, deterministic_generic, probationary_accepted"
    - Layer: "1" | PairCount: 4 | Theme: "AlertBanner inventory, documentation, wireframe+spec, four-predicate"
    - Layer: "2" | PairCount: 9 | Theme: "SearchAndFilterScreens UI+IMPLEMENT"
    - Layer: "3" | PairCount: 3 | Theme: "teaching unit 016"
  TotalPairCount: 31 | ChainIntegrityCheck: "passed"

---

## B01-L3-016-002 — ModelTrainerUnit: files 016

```
source_id: xiigen-b01-l3-016-002
pair_kind: teaching_unit
```

A: [ParsedPairCollection — 31 pairs, file 016]
B:
  Metrics: total_pairs_ingested: "31" | pairs_accepted: "31" | gate_b_passed: "true"
  WaveAcceptanceVerdict: "ACCEPTED"

---

## B01-L3-016-003 — Teaching summary: file 016 key lessons

```
source_id: xiigen-b01-l3-016-003
pair_kind: teaching_unit
```

Key lessons from file 016:
  1. "form" fires UI even in search-related context (form is in UI lexicon, not domain-specific)
  2. "logic" fires IMPLEMENT — useful for algorithm implementation requests
  3. "card sorting" is gap vocabulary — UX research methodology, no predicate
  4. UI+IMPLEMENT nine-node: DomainSplitter produces 2 packets (UI + IMPLEMENT)
  5. Four-predicate sweep: all four predicates route to all four sub-phases independently
