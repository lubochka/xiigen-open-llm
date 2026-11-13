# Layer 3 — Teaching Unit Pairs (003)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Method note

File 003 covers the dual-predicate rule: "write" fires IMPLEMENT alongside DOCUMENT lexicon terms.

---

## B06-L3-003-001 — "write" fires IMPLEMENT: dual-predicate rule

```
source_id: xiigen-b06-l3-003-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "Why 'write user stories' fires both DOCUMENT and IMPLEMENT"
  SourceFiles: ["layer-0a-compiler-pairs-003.md", "layer-0c-knowledge-edge-pairs-003.md"]

B:
  TeachingUnit: "DUAL-PREDICATE-WRITE-001"
  Content:
    title: "The Dual-Predicate Rule: 'write' + DOCUMENT noun"
    mechanism:
      step_1: "Scan request for DOCUMENT lexicon terms → found 'stories', 'criteria' → DOCUMENT fires"
      step_2: "Scan request for IMPLEMENT lexicon terms → found 'write' → IMPLEMENT fires"
      step_3: "Both predicates are independent — neither suppresses the other"
      step_4: "Two DomainPackets created: one DOCUMENT (→2.1), one IMPLEMENT (→4.1)"
    canonical_example:
      request: "Write the user stories and acceptance criteria for checkout"
      document_terms: ["stories", "criteria"]
      implement_terms: ["write"]
      result: "DOCUMENT + IMPLEMENT both fire"
    contrast_example:
      request: "Define the acceptance criteria for checkout"
      document_terms: ["criteria", "define"]
      implement_terms: []
      result: "DOCUMENT only — 'define' is in DOCUMENT lexicon, not IMPLEMENT"
    key_rule: "IMPLEMENT fires on action verbs (write, build, implement, create, develop). DOCUMENT fires on specification nouns and verbs from its own 17-term lexicon. The two lexicons are independent."
    routing_consequence: "Two independent flows run: RequirementsAuthoringFlow21 (2.1) and ImplementationFlow41 (4.1)"
    transition_type_consequence: "IMPLEMENT presence forces TransitionType → feature_addition"

---

## B06-L3-003-002 — IMPLEMENT lexicon action verbs

```
source_id: xiigen-b06-l3-003-002
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "IMPLEMENT lexicon action verbs that co-fire with DOCUMENT terms"

B:
  TeachingUnit: "IMPLEMENT-VERBS-IN-DOC-CONTEXT-001"
  Content:
    title: "IMPLEMENT Action Verbs That Fire Alongside DOCUMENT Terms"
    implement_firing_verbs:
      write:     "write the spec → DOCUMENT(spec) + IMPLEMENT(write)"
      build:     "build the use cases → DOCUMENT(use-case) + IMPLEMENT(build)"
      create:    "create the requirements → DOCUMENT(requirement) + IMPLEMENT(create)"
      develop:   "develop the feature brief → DOCUMENT(feature-brief) + IMPLEMENT(develop)"
      implement: "implement the capability → DOCUMENT(capability) + IMPLEMENT(implement)"
      code:      "code the business logic → DOCUMENT(business) + IMPLEMENT(code)"
    non_implement_verbs_in_doc_context:
      define:    "define is in DOCUMENT lexicon — DOCUMENT only, never IMPLEMENT"
      describe:  "describe is in DOCUMENT lexicon — DOCUMENT only"
      specify:   "specify is neutral context verb — does not fire any predicate alone"
    key_rule: "If the action verb is 'write', 'build', 'create', 'develop', 'implement', or 'code', IMPLEMENT fires regardless of what nouns accompany it."

---

## B06-L3-003-003 — Dual-predicate routing consequence

```
source_id: xiigen-b06-l3-003-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "DUAL-PREDICATE-ROUTING-001"
  Content:
    title: "What Dual DOCUMENT+IMPLEMENT Predicate Means for Routing"
    routing_table:
      "DOCUMENT only":         "→ 2.1 only; TransitionType: documentation"
      "DOCUMENT + IMPLEMENT":  "→ 2.1 + 4.1; TransitionType: feature_addition"
      "DOCUMENT + UI":         "→ 2.1 + 1.1; TransitionType: feature_addition"
      "DOCUMENT + ARCH":       "→ 2.1 + 3.1; TransitionType: documentation"
      "All four":              "→ 2.1 + 3.1 + 4.1 + 1.1; TransitionType: feature_addition"
    flow_count_rule: "One routing decision per predicate per sub-phase. Two predicates = two routing decisions = two concurrent flows."
    arbiter_rule: "Each flow runs its own N07/N08 arbiter independently."
    source_rule: "B06 Rule 1 — predicates are independent; B06 Rule 9 — IMPLEMENT+UI → feature_addition."
