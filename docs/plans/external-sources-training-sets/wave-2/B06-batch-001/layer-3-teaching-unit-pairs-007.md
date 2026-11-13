# Layer 3 — Teaching Unit Pairs (007)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Method note

File 007 covers narrative (Rule 13), feature-brief (Rule 14), epics (Rule 15), capability (Rule 16) as DOCUMENT lexicon terms.

---

## B06-L3-007-001 — narrative, feature-brief, epics, capability fire DOCUMENT

```
source_id: xiigen-b06-l3-007-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "Rules 13-16: narrative, feature-brief, epics, capability all fire DOCUMENT"
  SourceFiles: ["layer-0a-compiler-pairs-006.md", "layer-0c-knowledge-edge-pairs-007.md"]

B:
  TeachingUnit: "RULES-13-16-DOCUMENT-001"
  Content:
    title: "Rules 13-16: The Product Documentation Cluster"
    rules:
      "Rule 13":
        term: "narrative"
        example: "product narrative → DOCUMENT"
        note: "Fires on 'narrative' even without explicit doc/spec verbs"
      "Rule 14":
        term: "feature-brief"
        example: "feature brief for cart → DOCUMENT"
        critical_distinction: "'feature' alone fires IMPLEMENT; 'feature-brief' (hyphenated compound) fires DOCUMENT"
      "Rule 15":
        term: "epics"
        example: "define the epics → DOCUMENT (epics) + DOCUMENT (define)"
        note: "Agile product management term — always DOCUMENT, never IMPLEMENT"
      "Rule 16":
        term: "capability"
        example: "capability requirements → DOCUMENT"
        note: "'capability' in the requirements context fires DOCUMENT; 'capability' in implementation context also fires DOCUMENT (not IMPLEMENT)"
    cluster_name: "Product Documentation Cluster — four terms describing high-level product artifacts"
    key_rule: "All four terms (narrative, feature-brief, epics, capability) are in the DOCUMENT lexicon. None of them fire IMPLEMENT unless an IMPLEMENT action verb (write, build, create) co-occurs."

---

## B06-L3-007-002 — feature-brief vs feature: the critical distinction (Rule 14)

```
source_id: xiigen-b06-l3-007-002
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "Rule 14: 'feature-brief' fires DOCUMENT; 'feature' alone fires IMPLEMENT"

B:
  TeachingUnit: "FEATURE-BRIEF-VS-FEATURE-001"
  Content:
    title: "Rule 14 Critical Distinction: feature-brief vs feature"
    rule_14_text: "feature-brief fires DOCUMENT (Rule 14)"
    feature_alone_rule: "'feature' alone fires IMPLEMENT (not DOCUMENT)"
    examples:
      "feature brief for cart":      "DOCUMENT (feature-brief as compound)"
      "feature-brief for checkout":  "DOCUMENT (hyphenated form)"
      "the feature brief":           "DOCUMENT (full compound noun)"
      "build the feature":           "IMPLEMENT (build + feature; feature alone fires IMPLEMENT)"
      "implement the checkout feature": "IMPLEMENT (implement + feature)"
      "write the feature brief":     "DOCUMENT (feature-brief) + IMPLEMENT (write) — DUAL"
    parsing_rule: "The parser must recognize 'feature brief' (two words) and 'feature-brief' (hyphenated) as the DOCUMENT compound term. 'Feature' appearing without 'brief' is an IMPLEMENT signal."
    v6_error_risk: "v6-01 error type: missing 'feature-brief' as DOCUMENT signal and treating it as IMPLEMENT-only"

---

## B06-L3-007-003 — Agile terms in DOCUMENT lexicon

```
source_id: xiigen-b06-l3-007-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "AGILE-TERMS-DOCUMENT-001"
  Content:
    title: "Agile Product Management Terms in the DOCUMENT Lexicon"
    agile_document_terms:
      story:        "Rule: user story is a documentation artifact; always DOCUMENT"
      epics:        "Rule 15: epic is a product planning artifact; always DOCUMENT"
      criteria:     "acceptance criteria are a requirements artifact; always DOCUMENT"
      narrative:    "Rule 13: product narrative describes the vision; always DOCUMENT"
      feature-brief: "Rule 14: feature brief specifies scope; DOCUMENT"
      capability:   "Rule 16: capability is a product-level requirement; DOCUMENT"
    why_not_implement: "Agile terms describe what the system should do (requirements), not how to build it (implementation). IMPLEMENT fires on construction verbs and code-level nouns."
    combined_example:
      request: "Write the epics and user stories for the checkout feature, then build the implementation."
      predicates: "DOCUMENT (epics, story) + IMPLEMENT (write, build, feature)"
      note: "All three IMPLEMENT signals co-occur with two DOCUMENT signals."
