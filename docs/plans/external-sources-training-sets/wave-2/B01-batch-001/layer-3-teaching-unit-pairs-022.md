# Layer 3 — Teaching Unit Pairs (022)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B01-L3-022-001 — PairParserUnit: files 022

```
source_id: xiigen-b01-l3-022-001
pair_kind: teaching_unit
```

A: Theme: "IMPLEMENT+UI dual-predicate routing cases"
B:
  ParsedPairs:
    - Layer: "0-A" | PairCount: 5 | Theme: "IMPLEMENT+UI dual-predicate compiler cases"
    - Layer: "0-B" | PairCount: 4 | Theme: "IMPLEMENT+UI dual-predicate router fan-out"
    - Layer: "0-C" | PairCount: 3 | Theme: "build IS IMPLEMENT; screen IS UI; dual-predicate → feature_addition"
    - Layer: "0-D" | PairCount: 3 | Theme: "trusted_learned, deterministic_generic, probationary_rejected"
    - Layer: "1" | PairCount: 4 | Theme: "build dashboard, implement modal, create form, develop component"
    - Layer: "2" | PairCount: 9 | Theme: "DualPredicateImplementUI feature_addition TransitionType"
    - Layer: "3" | PairCount: 3 | Theme: "teaching unit 022"
  TotalPairCount: 31 | ChainIntegrityCheck: "passed"

---

## B01-L3-022-002 — ModelTrainerUnit: files 022

```
source_id: xiigen-b01-l3-022-002
pair_kind: teaching_unit
```

A: [ParsedPairCollection — 31 pairs, file 022]
B:
  Metrics: total_pairs_ingested: "31" | pairs_accepted: "31" | gate_b_passed: "true"
  WaveAcceptanceVerdict: "ACCEPTED"

---

## B01-L3-022-003 — Teaching summary: file 022 key lessons

```
source_id: xiigen-b01-l3-022-003
pair_kind: teaching_unit
```

Key lessons from file 022:
  1. IMPLEMENT+UI dual-predicate → TransitionType: feature_addition (IMPLEMENT drives)
  2. "build" fires IMPLEMENT — always triggers feature_addition regardless of subject
  3. "screen" fires UI — always regardless of IMPLEMENT co-occurrence
  4. Two RoutingDecisions for IMPLEMENT+UI; ScreenInventoryFlow11 + ImplementationFlow41 both selected
  5. DomainSplitter nine-node: both UI and IMPLEMENT packets processed in separate domain chains
