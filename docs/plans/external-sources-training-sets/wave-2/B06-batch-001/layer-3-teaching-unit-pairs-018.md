# Layer 3 — Teaching Unit Pairs (018)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## Method note

File 018 covers DataPrivacy, StakeholderShipping teaching, and DOCUMENT→2.1+2.4 rule — three pairs.

---

## B06-L3-018-001 — Teaching: DataPrivacy "stakeholder" triggers 2.4

```
source_id: xiigen-b06-l3-018-001
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-PRIVACY-STKHDR-TEACH-001"
  Content:
    rule: "Rule 8: stakeholder in DOCUMENT request triggers 2.4 routing in addition to 2.1"
    privacy_example:
      request: "Specify the stakeholder requirements for data privacy in checkout"
      predicates: ["DOCUMENT"]
      routing: ["2.1", "2.4"]
      decision_count: 2
    shipping_example:
      request: "Describe the stakeholder requirements for shipping carrier integration"
      predicates: ["DOCUMENT"]
      routing: ["2.1", "2.4"]
      decision_count: 2
    v6_corruption: "v6-03: routing collapsed to 2.1 only. Must produce 2.1 AND 2.4."

---

## B06-L3-018-002 — Teaching: TransitionType for 2.1+2.4 dual route

```
source_id: xiigen-b06-l3-018-002
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-DUAL-ROUTE-TEACH-001"
  Content:
    rule: "DOCUMENT → 2.1+2.4 with no IMPLEMENT → TransitionType: documentation on both"
    case:
      predicates: ["DOCUMENT"]
      routing: ["2.1", "2.4"]
      transition_type_2.1: "documentation"
      transition_type_2.4: "documentation"
      note: "Both transitions are documentation. Only IMPLEMENT/UI → feature_addition."
    contrast_case:
      request: "Write the stakeholder requirements for loyalty"
      predicates: ["DOCUMENT", "IMPLEMENT"]
      routing: ["2.1", "2.4", "4.1"]
      transition_type: "feature_addition (all three, because IMPLEMENT present)"

---

## B06-L3-018-003 — Teaching: probationary_rejected flow continues with fallback

```
source_id: xiigen-b06-l3-018-003
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-PROBREJ-TEACH-001"
  Content:
    rule: "probationary_rejected does not halt production — universal generic flow used"
    scenario:
      trust_state: "probationary_rejected"
      selected_flow: "universal-nine-node-development-flow-2.1"
      fallback_used: true
      warnings: 3
      production_continues: true
    teaching_note: "The pipeline never halts for a rejected flow. Quality is reduced.
      Gate B retry is signaled via warnings. The model must produce these 3 warnings
      and set FallbackUsed: true — not null, not false, not 0 warnings."
