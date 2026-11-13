# Layer 3 — Teaching Unit Pairs (021)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B01-L3-021-001 — PairParserUnit: files 021

```
source_id: xiigen-b01-l3-021-001
pair_kind: teaching_unit
```

A: Theme: "DOCUMENT+UI dual-predicate routing cases"
B:
  ParsedPairs:
    - Layer: "0-A" | PairCount: 5 | Theme: "DOCUMENT+UI dual-predicate compiler cases"
    - Layer: "0-B" | PairCount: 4 | Theme: "DOCUMENT+UI dual-predicate router fan-out"
    - Layer: "0-C" | PairCount: 3 | Theme: "describe IS DOCUMENT; wireframe IS UI; dual-predicate → two RoutingDecisions"
    - Layer: "0-D" | PairCount: 3 | Theme: "trusted_learned, deterministic_generic, probationary_accepted"
    - Layer: "1" | PairCount: 4 | Theme: "spec + screen, define + modal, document + component, describe + layout"
    - Layer: "2" | PairCount: 9 | Theme: "DualPredicateDocumentUI documentation TransitionType"
    - Layer: "3" | PairCount: 3 | Theme: "teaching unit 021"
  TotalPairCount: 31 | ChainIntegrityCheck: "passed"

---

## B01-L3-021-002 — ModelTrainerUnit: files 021

```
source_id: xiigen-b01-l3-021-002
pair_kind: teaching_unit
```

A: [ParsedPairCollection — 31 pairs, file 021]
B:
  Metrics: total_pairs_ingested: "31" | pairs_accepted: "31" | gate_b_passed: "true"
  WaveAcceptanceVerdict: "ACCEPTED"

---

## B01-L3-021-003 — Teaching summary: file 021 key lessons

```
source_id: xiigen-b01-l3-021-003
pair_kind: teaching_unit
```

Key lessons from file 021:
  1. DOCUMENT+UI dual-predicate → two RoutingDecisions, not merged
  2. "describe" fires DOCUMENT — even when describing a UI element
  3. "wireframe" fires UI — even in documentation context
  4. DOCUMENT+UI without IMPLEMENT → TransitionType: documentation (no feature_addition)
  5. DomainSplitter receives two-predicate input → produces two domain packets independently
