# Layer 3 — Teaching Unit Pairs (008)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Method note

File 008 covers TransitionType: feature_addition vs documentation — the rule table.

---

## B06-L3-008-001 — TransitionType determination rule

```
source_id: xiigen-b06-l3-008-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "When TransitionType is feature_addition vs documentation"
  SourceFiles: ["layer-0c-knowledge-edge-pairs-009.md", "layer-1-transition-orchestrator-pairs-002.md", "layer-1-transition-orchestrator-pairs-003.md"]

B:
  TeachingUnit: "TRANSITION-TYPE-RULE-001"
  Content:
    title: "TransitionType: feature_addition vs documentation"
    determination_rule:
      feature_addition_triggers:
        - "IMPLEMENT predicate present"
        - "UI predicate present"
        note: "Either IMPLEMENT or UI (or both) is sufficient to force feature_addition"
      documentation_condition: "DOCUMENT only, or DOCUMENT + ARCHITECTURE only (no IMPLEMENT, no UI)"
    complete_rule_table:
      "DOCUMENT only":                "documentation"
      "DOCUMENT + ARCHITECTURE":      "documentation"
      "DOCUMENT + IMPLEMENT":         "feature_addition"
      "DOCUMENT + UI":                "feature_addition"
      "DOCUMENT + ARCHITECTURE + UI": "feature_addition"
      "DOCUMENT + ARCHITECTURE + IMPLEMENT": "feature_addition"
      "All four predicates":          "feature_addition"
      "ARCHITECTURE only":            "documentation"
      "IMPLEMENT only":               "feature_addition"
      "UI only":                      "feature_addition"
    why_architecture_is_documentation: "ARCHITECTURE produces design artifacts (diagrams, ADRs (Architecture Decision Records)), not executable code or UI. It is a documentation activity."
    key_rule: "Ask: 'Is code being written or UI being built?' If yes → feature_addition. If only documents/designs are produced → documentation."

---

## B06-L3-008-002 — TransitionType propagation rule

```
source_id: xiigen-b06-l3-008-002
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "How TransitionType propagates to all transitions in a multi-predicate scenario"

B:
  TeachingUnit: "TRANSITION-TYPE-PROPAGATION-001"
  Content:
    title: "TransitionType Propagation in Multi-Predicate Scenarios"
    propagation_rule: "When one predicate forces feature_addition, all transitions in the same logical operation share that TransitionType."
    example:
      predicates_fired: ["DOCUMENT", "IMPLEMENT", "ARCHITECTURE"]
      transition_2_1: "feature_addition (IMPLEMENT present in same operation)"
      transition_3_1: "feature_addition (same)"
      transition_4_1: "feature_addition (same)"
      note: "Even though 2.1 (DOCUMENT) and 3.1 (ARCHITECTURE) would individually be 'documentation', the presence of IMPLEMENT in the same user request makes all transitions feature_addition."
    rationale: "A single user request represents a single logical operation. The TransitionType describes the nature of that operation, not individual sub-phase outputs."
    source_rule: "B06 Rule 9 — TransitionType is set at the logical operation level, not per predicate."

---

## B06-L3-008-003 — TransitionType and business impact

```
source_id: xiigen-b06-l3-008-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "TRANSITION-TYPE-IMPACT-001"
  Content:
    title: "Why TransitionType Matters for Downstream Processing"
    downstream_effects:
      feature_addition:
        - "Version bump may be required"
        - "Deployment pipeline activated"
        - "QA (Quality Assurance) regression gate required"
        - "Change log entry generated"
      documentation:
        - "No version bump"
        - "No deployment"
        - "Documentation review only"
        - "Knowledge base update"
    arbiter_behavior:
      feature_addition: "ArbiterSelector (N07) selects code quality + testability arbiter"
      documentation: "ArbiterSelector selects completeness + accuracy arbiter"
    v6_error_consequence: "v6-03 error: documentation TransitionType when feature_addition required → wrong arbiter selected → QA gates skipped → production risk"
    training_importance: "The model must infer TransitionType correctly before selecting arbiter at N07, not after."
