# Layer 3 — Teaching Unit Pairs (013)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## Method note

File 013 covers ShippingRequirements teaching — three pairs reinforcing DOCUMENT
predicate rules for the shipping domain.

---

## B06-L3-013-001 — Teaching: DOCUMENT fires on shipping vocabulary

```
source_id: xiigen-b06-l3-013-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"; TeachingFocus: "ShippingRequirements DOCUMENT predicate teaching"

B:
  TeachingUnit: "B06-SHIPPING-TEACH-001"
  Content:
    domain: "ShippingRequirements"
    key_rule: "DOCUMENT fires on requirements vocabulary regardless of domain"
    examples:
      fires: ["define shipping requirements", "describe carrier use cases", "specify shipping spec"]
      does_not_fire: ["plan carrier integration", "manage shipping SLA", "track delivery"]
    anti_pattern: "Treating domain words (shipping, carrier, delivery) as predicate terms"
    correct: "Only the 17 DOCUMENT lexicon terms fire DOCUMENT"

---

## B06-L3-013-002 — Teaching: "write shipping spec" fires DOCUMENT+IMPLEMENT

```
source_id: xiigen-b06-l3-013-002
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

A:
  TeachingFocus: "ShippingRequirements dual-predicate rule"

B:
  TeachingUnit: "B06-SHIPPING-TEACH-002"
  Content:
    rule: "write + requirements noun → DOCUMENT + IMPLEMENT (both fire)"
    shipping_examples:
      "write the shipping specification" → ["DOCUMENT", "IMPLEMENT"]
      "define the shipping requirements" → ["DOCUMENT"]
      "describe the shipping use cases" → ["DOCUMENT"]
    key_distinction: "'write' fires IMPLEMENT always; 'define'/'describe'/'specify' fire DOCUMENT only"

---

## B06-L3-013-003 — Teaching: shipping domain does not affect sub-phase routing

```
source_id: xiigen-b06-l3-013-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

A:
  TeachingFocus: "ShippingRequirements sub-phase routing invariant"

B:
  TeachingUnit: "B06-SHIPPING-TEACH-003"
  Content:
    rule: "DOCUMENT → 2.1 regardless of domain. Shipping domain does not change sub-phase."
    shipping_routing:
      "define shipping requirements" → 2.1
      "describe carrier use cases" → 2.1
      "specify shipping behavior" → 2.1
    anti_pattern: "Routing shipping requirements to a logistics sub-phase (does not exist)"
    correct: "2.1 (Business Requirements and Use Cases) is the only sub-phase for DOCUMENT"
