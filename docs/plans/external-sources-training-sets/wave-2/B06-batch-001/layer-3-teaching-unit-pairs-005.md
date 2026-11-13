# Layer 3 — Teaching Unit Pairs (005)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Method note

File 005 covers the Phase 2 sub-phase map: what each sub-phase (2.1, 2.2, 2.3, 2.4) produces and its signal words.

---

## B06-L3-005-001 — Phase 2 sub-phase map

```
source_id: xiigen-b06-l3-005-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "What Phase 2 sub-phases exist and what signals route to each"
  SourceFiles: ["layer-0c-knowledge-edge-pairs-005.md", "layer-0a-compiler-pairs-005.md"]

B:
  TeachingUnit: "PHASE-2-SUBPHASE-MAP-001"
  Content:
    title: "Phase 2 Sub-Phase Map for B06 DOCUMENT Predicate"
    sub_phases:
      "2.1":
        name: "Business Requirements and Use Cases"
        flow: "RequirementsAuthoringFlow21"
        default_for: "All DOCUMENT signals (default routing)"
        signal_words: ["requirement", "use-case", "story", "criteria", "business", "define", "describe", "functional", "behavior", "epics", "capability"]
      "2.2":
        name: "Technical Requirements"
        flow: "TechnicalRequirementsFlow22"
        signal_words: ["technical requirements", "system requirements", "non-functional", "performance", "scalability"]
        note: "Not a primary flow for B06; mentioned for completeness"
      "2.3":
        name: "Non-Functional Requirements"
        flow: "NonFunctionalRequirementsFlow23"
        signal_words: ["reliability", "availability", "SLA", "latency", "throughput", "security requirements"]
        note: "Not a primary flow for B06; referenced in cross-cutting requirements"
      "2.4":
        name: "Stakeholder Requirements"
        flow: "StakeholderRequirementsFlow24"
        signal_words: ["stakeholder", "external partner", "marketplace vendor", "payment partner", "vendor requirements"]
        trigger: "Only fires when stakeholder signal words present alongside DOCUMENT"
    b06_primary_sub_phases: ["2.1", "2.4"]
    key_rule: "B06 scenario is B06 — Checkout flow business requirements. Sub-phase 2.1 is always the default. Sub-phase 2.4 activates on stakeholder signal words."

---

## B06-L3-005-002 — Sub-phase signal word disambiguation

```
source_id: xiigen-b06-l3-005-002
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "How to distinguish 2.1 signals from 2.4 signals"

B:
  TeachingUnit: "SUBPHASE-SIGNAL-DISAMBIGUATION-001"
  Content:
    title: "Sub-Phase 2.1 vs 2.4 Signal Word Disambiguation"
    disambiguation_table:
      "requirements": "→ 2.1 (internal business requirements)"
      "stakeholder requirements": "→ 2.1 + 2.4 (both internal and stakeholder perspective)"
      "business rules": "→ 2.1 only"
      "external partner requirements": "→ 2.4 (stakeholder signal)"
      "use cases": "→ 2.1 only"
      "vendor requirements": "→ 2.4 (external partner)"
      "payment partner SLA": "→ 2.4 (external facing SLA)"
      "acceptance criteria": "→ 2.1 only (internal testability)"
    ambiguous_cases:
      "partner use cases": "→ 2.1 (use-case fires DOCUMENT→2.1; 'partner' alone does not trigger 2.4)"
      "external stakeholder": "→ 2.4 (stakeholder explicitly present)"
    key_rule: "2.4 requires the word 'stakeholder' or an explicit external party name (payment partner, marketplace vendor). 'partner' alone without 'stakeholder' or 'external' context routes to 2.1 only."

---

## B06-L3-005-003 — Sub-phase 2.1 canonical output structure

```
source_id: xiigen-b06-l3-005-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "SUBPHASE-2-1-OUTPUT-STRUCTURE-001"
  Content:
    title: "Canonical Output Structure for Sub-Phase 2.1"
    output_sections:
      use_cases: "Named use cases with UC-NNN IDs; Actor, Preconditions, Main Flow, Alternative Flows, Postconditions"
      business_rules: "BR-NNN coded rules; concise single-sentence assertions"
      acceptance_criteria: "Given/When/Then format; testable without UI dependencies"
      success_gates: "Measurable KPIs (Key Performance Indicators) with thresholds (e.g., completion rate >= 85%)"
    minimum_viable_output:
      use_case_count: ">= 3"
      acceptance_criteria_count: ">= 5"
      business_rule_count: ">= 2"
    arbiter_review_types:
      N07_selector: "ArbiterSelector selects use-case completeness arbiter"
      N08_review_1: "Use case completeness (all main and alternative flows present)"
      N08_review_2: "Testability (all acceptance criteria are independently verifiable)"
    source_evidence: "layer-2-nine-node-2.1-pairs-002.md through 012.md"
