# Layer 3 — Teaching Unit Pairs (009)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Method note

File 009 covers the three v6 corruption pattern types in teaching format.

---

## B06-L3-009-001 — v6-01 corruption: DOCUMENT missed

```
source_id: xiigen-b06-l3-009-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "v6-01 corruption: DOCUMENT predicate is missed entirely"
  SourceFiles: ["layer-0a-compiler-pairs-011.md", "layer-0c-knowledge-edge-pairs-008.md", "held-out-and-corruption-pairs.md"]

B:
  TeachingUnit: "V6-01-DOCUMENT-MISSED-001"
  Content:
    title: "v6-01 Corruption: DOCUMENT Predicate Missed"
    corruption_type: "v6-01"
    description: "The DOCUMENT predicate is not fired even though one or more DOCUMENT lexicon terms are present in the request."
    canonical_trigger: "Model sees context suggesting technical work and defaults to IMPLEMENT only, missing the requirements documentation aspect."
    example:
      request: "Write the requirements spec and implement the checkout service"
      correct: "DOCUMENT (spec, requirements) + IMPLEMENT (write, implement)"
      corrupt: "IMPLEMENT only — DOCUMENT missed"
      downstream_error: "Sub-phase 2.1 flow never starts; no requirements documentation produced"
    detection_signals:
      - "Request contains 'requirements', 'spec', 'specification', 'story', 'criteria', 'describe', 'define', 'functional', 'behavior', 'use-case', 'narrative', 'feature-brief', 'capability', 'epics', 'business', 'stakeholder' AND no DOCUMENT routing decision was made"
    b06_high_risk_requests:
      - "Write the user stories for checkout" — risk: model focuses on 'write' → IMPLEMENT only
      - "Build the requirements spec" — risk: model focuses on 'build' → IMPLEMENT only
      - "Create the acceptance criteria" — risk: model focuses on 'create' → IMPLEMENT only
    key_rule: "DOCUMENT signals are NOUN and VERB lexicon terms. They fire regardless of which action verb accompanies them. Scan for DOCUMENT terms first, then scan for IMPLEMENT terms separately."

---

## B06-L3-009-002 — v6-02 corruption: IMPLEMENT suppressed

```
source_id: xiigen-b06-l3-009-002
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "v6-02 corruption: IMPLEMENT predicate is suppressed when it should co-fire"

B:
  TeachingUnit: "V6-02-IMPLEMENT-SUPPRESSED-001"
  Content:
    title: "v6-02 Corruption: IMPLEMENT Predicate Suppressed"
    corruption_type: "v6-02"
    description: "The IMPLEMENT predicate is suppressed because the model incorrectly treats the request as documentation-only when a construction action verb is present."
    canonical_trigger: "Model sees many DOCUMENT lexicon terms and treats all action verbs as part of the documentation context, suppressing IMPLEMENT."
    example:
      request: "Write the user stories and define the acceptance criteria"
      correct: "DOCUMENT (story, criteria, define) + IMPLEMENT (write)"
      corrupt: "DOCUMENT only — IMPLEMENT(write) suppressed"
      downstream_error: "Sub-phase 4.1 flow never starts; TransitionType wrongly set to documentation"
    b06_note: "This is particularly common with 'write' because 'write' can feel like a documentation verb. It is not. 'Write' is an IMPLEMENT action verb."
    test_for_v6_02: "After firing DOCUMENT, re-scan the request specifically for: write, build, create, develop, implement, code. If any found → IMPLEMENT must fire."
    key_rule: "The two lexicons never suppress each other. DOCUMENT firing does not reduce the probability of IMPLEMENT firing."

---

## B06-L3-009-003 — v6-03 corruption: routing collapsed

```
source_id: xiigen-b06-l3-009-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "V6-03-ROUTING-COLLAPSED-001"
  Content:
    title: "v6-03 Corruption: Routing Collapsed to Single Sub-Phase"
    corruption_type: "v6-03"
    description: "The routing decision collapses multiple required sub-phase routing decisions into a single decision, missing required flows."
    three_forms:
      form_1:
        scenario: "DOCUMENT should route to 2.1 + 2.4 but only 2.1 is produced"
        trigger: "Stakeholder signal present but 2.4 routing decision not generated"
      form_2:
        scenario: "DOCUMENT + IMPLEMENT should route to 2.1 + 4.1 but only 2.1 is produced"
        trigger: "IMPLEMENT fired but 4.1 routing decision not generated"
      form_3:
        scenario: "Four predicates should route to 4 sub-phases but only 1-2 routing decisions produced"
        trigger: "Model collapses parallel flows into a single sequential flow"
    detection: "Count the number of routing decisions. Must equal the number of active sub-phases."
    repair: "For each active predicate+sub-phase combination, one routing decision must be emitted. No deduplication across predicates."
    key_rule: "Routing decisions are cheap to emit and must not be collapsed. Each active sub-phase gets its own routing decision and its own independent nine-node flow execution."
