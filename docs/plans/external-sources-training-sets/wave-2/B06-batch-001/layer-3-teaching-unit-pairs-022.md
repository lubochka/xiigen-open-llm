# Layer 3 — Teaching Unit Pairs (022)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## Method note

File 022 covers transition ordering for new domains and the canonical ordering rule — three pairs.

---

## B06-L3-022-001 — Teaching: canonical transition ordering for new domains

```
source_id: xiigen-b06-l3-022-001
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-ORDERING-NEW-DOMAINS"
  Content:
    canonical_order: "2.1 → 3.1 → 2.4 → 1.1 → 4.1"
    new_domain_ordering_examples:
      shipping_DOCUMENT_IMPLEMENT: "2.1 → 4.1 (requirements first, implementation last)"
      loyalty_stakeholder: "2.1 → 2.4 (business requirements first, stakeholder after)"
      tax_DOCUMENT_ARCHITECTURE: "2.1 → 3.1 (requirements first, architecture after)"
      subscription_DOCUMENT_IMPLEMENT: "2.1 → 4.1"
    ordering_violations:
      - "4.1 before 2.1" → always wrong (implementation before requirements)
      - "3.1 before 2.1" → always wrong (architecture before requirements)
      - "2.4 before 2.1" → always wrong (stakeholder requirements before business requirements)

---

## B06-L3-022-002 — Teaching: nine-node N01 always receives SelectedLogicalTransition

```
source_id: xiigen-b06-l3-022-002
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-N01-RULE-013-024"
  Content:
    rule: "N01 A input is always the SelectedLogicalTransition from Layer 1"
    new_domain_n01_inputs:
      shipping_013: "TransitionId: transition-shipping-013-001; SubPhaseId: 2.1; TransitionType: documentation"
      loyalty_014: "TransitionId: transition-loyalty-014-001; SubPhaseId: 2.1; TransitionType: documentation"
      subscription_019: "TransitionId: transition-subscription-019-001; SubPhaseId: 2.1; TransitionType: feature_addition"
    anti_pattern: "N01 receiving the raw user request without going through Layer 0-A, 0-B, 0-D, and Layer 1"
    correct: "N01 always receives the output of Layer 1 (TransitionOrchestratorUnit)"

---

## B06-L3-022-003 — Teaching: nine-node parallel flow rule

```
source_id: xiigen-b06-l3-022-003
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-PARALLEL-FLOWS-013-024"
  Content:
    rule: "Multiple predicates → multiple nine-node flows run concurrently"
    new_domain_parallel_examples:
      "write shipping spec" → 2 flows (RequirementsAuthoringFlow21, ImplementationFlow41)
      "document tax requirements and design architecture" → 2 flows (RequirementsAuthoringFlow21, ArchitectureFlow31)
      "describe loyalty stakeholder requirements" → 2 flows (RequirementsAuthoringFlow21, StakeholderRequirementsFlow24)
    teaching_note: "Each flow is independent — has its own N01 through N09 chain.
      Results are assembled after all flows complete. DecisionCount == FlowCount."
