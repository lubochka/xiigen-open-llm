# Layer 3 — Teaching Unit Pairs (006)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Method note

File 006 covers "business" (Rule 7) and "stakeholder" (Rule 8) as DOCUMENT lexicon terms.

---

## B06-L3-006-001 — "business" fires DOCUMENT (Rule 7)

```
source_id: xiigen-b06-l3-006-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "Rule 7: the term 'business' fires DOCUMENT predicate"
  SourceFiles: ["layer-0a-compiler-pairs-002.md", "layer-0c-knowledge-edge-pairs-006.md"]

B:
  TeachingUnit: "BUSINESS-FIRES-DOCUMENT-001"
  Content:
    title: "Rule 7 — 'business' Fires DOCUMENT"
    rule_number: 7
    rule_text: "The term 'business' in a user request fires the DOCUMENT predicate"
    firing_examples:
      - "business requirements → DOCUMENT"
      - "business rules → DOCUMENT"
      - "business logic documentation → DOCUMENT"
      - "checkout business requirements → DOCUMENT"
    non_firing_clarification:
      "business logic implementation": "DOCUMENT (business) + IMPLEMENT (implement) — both fire"
      "business process": "'process' is not in any predicate lexicon; 'business' alone fires DOCUMENT"
    context_independence: "The word 'business' fires DOCUMENT regardless of what follows it. It is a standalone DOCUMENT signal."
    b06_note: "This rule is particularly relevant for B06 (Checkout flow business requirements) because 'business requirements' is the canonical request pattern for this scenario."
    key_rule: "Rule 7 is unconditional. Any occurrence of 'business' in the user request fires DOCUMENT."

---

## B06-L3-006-002 — "stakeholder" fires DOCUMENT (Rule 8)

```
source_id: xiigen-b06-l3-006-002
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "Rule 8: the term 'stakeholder' fires DOCUMENT predicate AND triggers 2.4 routing"

B:
  TeachingUnit: "STAKEHOLDER-FIRES-DOCUMENT-001"
  Content:
    title: "Rule 8 — 'stakeholder' Fires DOCUMENT (and Triggers 2.4)"
    rule_number: 8
    rule_text: "The term 'stakeholder' fires the DOCUMENT predicate"
    dual_effect:
      effect_1: "DOCUMENT predicate fires (like any lexicon term)"
      effect_2: "Sub-phase routing includes 2.4 (StakeholderRequirementsFlow24) in addition to 2.1"
    firing_examples:
      - "stakeholder requirements → DOCUMENT → routes to 2.1 + 2.4"
      - "document the stakeholder expectations → DOCUMENT (stakeholder + document) → 2.1 + 2.4"
      - "stakeholder expectations → DOCUMENT → 2.1 + 2.4"
    contrast_with_business:
      "business": "fires DOCUMENT → routes to 2.1 only (no 2.4 trigger unless stakeholder also present)"
      "stakeholder": "fires DOCUMENT → routes to 2.1 AND 2.4 (external partner perspective needed)"
    key_rule: "Rule 8 has two consequences: (1) DOCUMENT fires, (2) sub-phase 2.4 activates. Always check for both when 'stakeholder' is present."

---

## B06-L3-006-003 — Rule 7 and Rule 8 in combination

```
source_id: xiigen-b06-l3-006-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "BUSINESS-STAKEHOLDER-COMBINATION-001"
  Content:
    title: "Rule 7 + Rule 8: 'business' and 'stakeholder' Together"
    combination_examples:
      "document the business and stakeholder requirements":
        terms_found: ["document", "business", "stakeholder"]
        predicates_fired: ["DOCUMENT"]
        sub_phases: ["2.1", "2.4"]
        note: "Three DOCUMENT signals → one predicate instance; multi-term = higher confidence, not multiple predicates"
      "define the business rules and stakeholder expectations":
        terms_found: ["define", "business", "stakeholder"]
        predicates_fired: ["DOCUMENT"]
        sub_phases: ["2.1", "2.4"]
      "business requirements for payment partners":
        terms_found: ["business", "requirements"]
        predicates_fired: ["DOCUMENT"]
        sub_phases: ["2.1"]
        note: "No 'stakeholder' term present — 2.4 does NOT activate from 'payment partners' alone"
    signal_density_rule: "Multiple DOCUMENT lexicon terms increase confidence (e.g., 0.91 vs 0.85) but produce exactly one predicate instance."
    confidence_table:
      "1 term": "0.82–0.85"
      "2-3 terms": "0.87–0.91"
      "4+ terms": "0.92"
