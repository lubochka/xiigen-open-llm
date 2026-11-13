# Layer 3 — Teaching Unit Pairs (030)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B06-L3-030-001 — DropshippingRules: domain teaching summary

```
source_id: xiigen-b06-l3-030-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"; TeachingFocus: "DropshippingRules — teaching summary"

B:
  TeachingUnit: "B06-DROPSHIPPINGRULES-SUMMARY"
  Content:
    title: "DropshippingRules Teaching Summary"
    domain: "DropshippingRules"
    predicate_patterns:
      DOCUMENT_alone: "→ documentation on RequirementsAuthoringFlow21 (SubPhase 2.1)"
      DOCUMENT_IMPLEMENT: "→ feature_addition on RequirementsAuthoringFlow21 + ImplementationFlow41"
      DOCUMENT_ARCHITECTURE: "→ documentation on RequirementsAuthoringFlow21 + ArchitectureFlow31"
      DOCUMENT_ARCHITECTURE_IMPLEMENT: "→ feature_addition on all three flows"
    domain_id_confirmed: "software-development (not dropshippingrules)"
    knowledge_edge_cases: "non-lexicon verbs (analyze, evaluate, plan, consider) → KNOWLEDGE_EDGE"
    trust_state: "RequirementsAuthoringFlow21: trusted_learned; ImplementationFlow41: deterministic_generic"

---

## B06-L3-030-002 — DropshippingRules: file inventory

```
source_id: xiigen-b06-l3-030-002
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-DROPSHIPPINGRULES-INVENTORY"
  Content:
    title: "DropshippingRules File Inventory"
    layer_0a: "5 pairs — DOCUMENT, DOC+IMPL, IMPL, knowledge-edge, DOC+ARCH"
    layer_0b: "4 pairs — DOCUMENT→2.1, DOC+IMPL→2.1+4.1, DOC+ARCH→2.1+3.1, DOC+ARCH+IMPL→2.1+3.1+4.1"
    layer_0c: "3 pairs — gap verbs, boundary cases, gap vs spec vocabulary"
    layer_0d: "3 pairs — DOCUMENT→RequirementsAuthoringFlow21, DOC+IMPL→2 flows, DomainId invariant"
    layer_1:  "4 pairs — documentation, feature_addition combos"
    layer_2:  "3 sections — N01 SelectedLogicalTransition, N02 TechnicalStatement, N09 chain summary"
    layer_3:  "3 pairs — teaching summary, inventory, Gate B"
    held_out: "4 pairs — 2 held-out + 2 v6 corruption"

---

## B06-L3-030-003 — DropshippingRules: Gate B assertions

```
source_id: xiigen-b06-l3-030-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-DROPSHIPPINGRULES-GATE-B"
  Content:
    gate_b_assertions:
      "DOCUMENT predicate fires": "any of 17 lexicon terms in DropshippingRules request → DOCUMENT in PredicateIds"
      "DomainId": "always software-development — never dropshippingrules"
      "TransitionType with IMPLEMENT": "feature_addition on every flow in the set"
      "TransitionType without IMPLEMENT": "documentation"
      "KNOWLEDGE_EDGE": "gap verb alone → KNOWLEDGE_EDGE; predicate term present → not KNOWLEDGE_EDGE"
      "CanonicalOrdering": "2.1 appears before 3.1; 3.1 appears before 4.1"
      "TrustState RequirementsAuthoringFlow21": "trusted_learned"
      "TrustState ImplementationFlow41": "deterministic_generic"
    wave_acceptance_verdict: "ACCEPTED"
