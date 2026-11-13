# Layer 3 — Teaching Unit Pairs (010)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Method note

File 010 covers DOCUMENT vs IMPLEMENT boundary cases — where the line is drawn.

---

## B06-L3-010-001 — DOCUMENT vs IMPLEMENT boundary: core cases

```
source_id: xiigen-b06-l3-010-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "Where DOCUMENT ends and IMPLEMENT begins — boundary cases"
  SourceFiles: ["layer-0c-knowledge-edge-pairs-010.md", "layer-0a-compiler-pairs-010.md"]

B:
  TeachingUnit: "DOCUMENT-IMPLEMENT-BOUNDARY-001"
  Content:
    title: "DOCUMENT vs IMPLEMENT Boundary Cases"
    boundary_table:
      "define the requirements":          "DOCUMENT only (define is in DOCUMENT lexicon)"
      "implement the requirements":       "DOCUMENT (requirements) + IMPLEMENT (implement)"
      "write the specification":          "DOCUMENT (specification) + IMPLEMENT (write)"
      "document the specification":       "DOCUMENT only (document + specification = two DOCUMENT signals)"
      "describe the behavior":            "DOCUMENT only (describe + behavior = two DOCUMENT signals)"
      "code the behavior":                "DOCUMENT (behavior) + IMPLEMENT (code)"
      "build the feature brief":          "DOCUMENT (feature-brief) + IMPLEMENT (build)"
      "author the feature brief":         "DOCUMENT only ('author' is not in IMPLEMENT lexicon)"
      "create the use cases":             "DOCUMENT (use-case) + IMPLEMENT (create)"
      "map the use cases":                "DOCUMENT only ('map' is not in IMPLEMENT lexicon)"
      "develop the acceptance criteria":  "DOCUMENT (criteria) + IMPLEMENT (develop)"
      "validate the acceptance criteria": "DOCUMENT only ('validate' is not in IMPLEMENT lexicon)"
    boundary_principle: "The boundary is defined by the IMPLEMENT action verb list: write, build, create, develop, implement, code. Any other verb that accompanies DOCUMENT nouns does NOT fire IMPLEMENT."

---

## B06-L3-010-002 — DOCUMENT verbs that are NOT IMPLEMENT

```
source_id: xiigen-b06-l3-010-002
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "Verbs that appear with DOCUMENT nouns but do NOT fire IMPLEMENT"

B:
  TeachingUnit: "NON-IMPLEMENT-VERBS-001"
  Content:
    title: "Verbs That Do NOT Fire IMPLEMENT (Even When Accompanying DOCUMENT Nouns)"
    non_implement_verbs:
      author:    "author the spec → DOCUMENT only"
      draft:     "draft the requirements → DOCUMENT only"
      outline:   "outline the use cases → DOCUMENT only"
      review:    "review the specifications → DOCUMENT only"
      validate:  "validate the acceptance criteria → DOCUMENT only"
      map:       "map the use cases → DOCUMENT only"
      capture:   "capture the requirements → DOCUMENT only ('capture' not in any predicate lexicon)"
      detail:    "detail the business rules → DOCUMENT only"
      enumerate: "enumerate the stakeholder requirements → DOCUMENT only"
      list:      "list the use cases → DOCUMENT only"
    key_rule: "Only six verbs fire IMPLEMENT: write, build, create, develop, implement, code. All other verbs that accompany DOCUMENT nouns produce DOCUMENT only."
    common_mistake: "Treating 'draft', 'author', or 'outline' as IMPLEMENT verbs. They are documentation verbs and do not fire IMPLEMENT."

---

## B06-L3-010-003 — Noun-verb pair classification table

```
source_id: xiigen-b06-l3-010-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "NOUN-VERB-CLASSIFICATION-001"
  Content:
    title: "Noun-Verb Pair Classification: DOCUMENT-only vs DOCUMENT+IMPLEMENT"
    document_only_pairs:
      - "define + requirements"
      - "describe + use-cases"
      - "document + specification"
      - "draft + stories"
      - "author + narrative"
      - "outline + epics"
      - "detail + business rules"
      - "enumerate + stakeholder expectations"
    document_and_implement_pairs:
      - "write + requirements"
      - "write + stories"
      - "write + specification"
      - "build + use-cases"
      - "create + criteria"
      - "develop + narrative"
      - "implement + requirements"
      - "code + business logic"
    classification_algorithm:
      step_1: "Extract all nouns and verbs from request"
      step_2: "Check each noun against DOCUMENT lexicon (17 terms) → if found, DOCUMENT fires"
      step_3: "Check each verb against IMPLEMENT lexicon (write, build, create, develop, implement, code) → if found, IMPLEMENT fires"
      step_4: "Check each noun/verb against DOCUMENT verb lexicon (describe, define) → if found, DOCUMENT fires"
      step_5: "The two checks are independent; never let one suppress the other"
