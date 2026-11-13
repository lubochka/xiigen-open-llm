# Layer 3 — Teaching Unit Pairs (042)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B06-L3-042-001 — CouponStackingRules: domain teaching summary

```
source_id: xiigen-b06-l3-042-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"; TeachingFocus: "CouponStackingRules — teaching summary (Round 3)"

B:
  TeachingUnit: "B06-COUPONSTACKINGRULES-SUMMARY"
  Content:
    title: "CouponStackingRules Teaching Summary (Round 3)"
    domain: "CouponStackingRules"
    round: "3 (files 037-048)"
    predicate_patterns:
      DOCUMENT_alone: "→ documentation on RequirementsAuthoringFlow21 (SubPhase 2.1)"
      DOCUMENT_IMPLEMENT: "→ feature_addition on RequirementsAuthoringFlow21 + ImplementationFlow41"
      DOCUMENT_ARCHITECTURE: "→ documentation on RequirementsAuthoringFlow21 + ArchitectureFlow31"
      DOCUMENT_ARCHITECTURE_IMPLEMENT: "→ feature_addition on all three flows"
    domain_id_confirmed: "software-development (not couponstackingrules)"
    knowledge_edge_cases: "gap verbs without any lexicon term → KNOWLEDGE_EDGE"
    trust_state: "RequirementsAuthoringFlow21: trusted_learned; ImplementationFlow41: deterministic_generic"
    new_rule_reinforced: "No new flow aliases introduced in Round 3 — same 4 flows as Rounds 1-2"

---

## B06-L3-042-002 — CouponStackingRules: file inventory

```
source_id: xiigen-b06-l3-042-002
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-COUPONSTACKINGRULES-INVENTORY"
  Content:
    title: "CouponStackingRules File Inventory (Round 3)"
    layer_0a: "5 pairs — DOCUMENT, DOC+IMPL, IMPL, knowledge-edge, DOC+ARCH"
    layer_0b: "4 pairs — routing decisions per predicate combination"
    layer_0c: "3 pairs — gap verbs, boundary, gap vs spec"
    layer_0d: "3 pairs — flow factory selection"
    layer_1:  "4 pairs — transition orchestrator decisions"
    layer_2:  "3 sections — N01, N02, N09"
    layer_3:  "3 pairs — teaching summary, inventory, Gate B"
    held_out: "4 pairs — 2 held-out + 2 v6 corruption"

---

## B06-L3-042-003 — CouponStackingRules: Gate B assertions

```
source_id: xiigen-b06-l3-042-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-COUPONSTACKINGRULES-GATE-B"
  Content:
    gate_b_assertions:
      "DOCUMENT predicate fires": "any of 17 lexicon terms in CouponStackingRules request → DOCUMENT in PredicateIds"
      "DomainId": "always software-development — never couponstackingrules"
      "TransitionType with IMPLEMENT": "feature_addition on every flow in the set"
      "TransitionType without IMPLEMENT": "documentation"
      "KNOWLEDGE_EDGE trigger": "zero predicate-lexicon terms in request → KNOWLEDGE_EDGE"
      "CanonicalOrdering": "2.1 → 3.1 → 4.1 when all three SubPhases present"
      "No new flows in Round 3": "flow registry unchanged since Round 1"
    wave_acceptance_verdict: "ACCEPTED"
