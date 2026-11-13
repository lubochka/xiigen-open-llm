# Layer 3 — Teaching Unit Pairs (002)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## Method note

File 002 covers the DOCUMENT lexicon teaching: complete list and examples.

---

## B06-L3-002-001 — DOCUMENT lexicon complete teaching

```
source_id: xiigen-b06-l3-002-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "DOCUMENT predicate lexicon — complete list with examples"
  SourceFiles: ["layer-0a-compiler-pairs-002.md", "layer-0c-knowledge-edge-pairs-002.md"]

B:
  TeachingUnit: "DOCUMENT-LEXICON-001"
  Content:
    title: "DOCUMENT Predicate Lexicon for B06"
    lexicon_count: 17
    terms_with_examples:
      document:      "document the requirements"
      spec:          "write the spec"
      specification: "create the specification"
      requirement:   "define the requirements"
      story:         "write the user stories"
      criteria:      "define the acceptance criteria"
      describe:      "describe the use cases"
      define:        "define the business rules"
      functional:    "functional requirements"
      behavior:      "specify the behavior"
      use-case:      "use cases for checkout"
      narrative:     "product narrative"
      feature-brief: "feature brief for cart"
      capability:    "capability requirements"
      epics:         "define the epics"
      business:      "business requirements"
      stakeholder:   "stakeholder expectations"
    key_rule: "Any one of these 17 terms in the user request fires DOCUMENT."
    signal_density: "Multiple terms increase confidence but do not create multiple predicate instances."

---

## B06-L3-002-002 — DOCUMENT lexicon non-terms teaching

```
source_id: xiigen-b06-l3-002-002
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "Terms that do NOT fire DOCUMENT — false positive prevention"

B:
  TeachingUnit: "DOCUMENT-LEXICON-NONTERMS-001"
  Content:
    title: "Terms That Do NOT Fire DOCUMENT"
    non_firing_terms:
      process: "not in DOCUMENT lexicon"
      flow:    "not in DOCUMENT lexicon"
      scenario: "not in DOCUMENT lexicon"
      capture: "not in any predicate lexicon"
      improve: "not in any predicate lexicon"
      better:  "not in any predicate lexicon"
      user:    "not in any predicate lexicon (ux fires UI)"
      feature: "fires IMPLEMENT, not DOCUMENT"
    key_rule: "Do not infer DOCUMENT from context. Only fire on lexicon terms."

---

## B06-L3-002-003 — DOCUMENT lexicon: B06 rule citations

```
source_id: xiigen-b06-l3-002-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "DOCUMENT-LEXICON-RULES-001"
  Content:
    title: "B06 Core Rules for DOCUMENT Lexicon Terms"
    rules:
      "Rule 7":  "business fires DOCUMENT"
      "Rule 8":  "stakeholder fires DOCUMENT"
      "Rule 13": "narrative fires DOCUMENT"
      "Rule 14": "feature-brief fires DOCUMENT"
      "Rule 15": "epics fires DOCUMENT"
      "Rule 16": "capability fires DOCUMENT"
    b06_rule_5: "describe fires DOCUMENT (not IMPLEMENT)"
    b06_rule_6: "define fires DOCUMENT"
    source_file: "layer-0a-compiler-pairs-002.md through 012.md"
