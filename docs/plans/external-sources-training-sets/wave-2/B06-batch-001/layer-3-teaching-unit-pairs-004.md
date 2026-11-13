# Layer 3 — Teaching Unit Pairs (004)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Method note

File 004 covers the one-predicate-to-multiple-sub-phases routing pattern (DOCUMENT → 2.1 + 2.4).

---

## B06-L3-004-001 — One predicate routing to multiple sub-phases

```
source_id: xiigen-b06-l3-004-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "How DOCUMENT alone can route to both 2.1 and 2.4 sub-phases"
  SourceFiles: ["layer-0b-router-pairs-005.md", "layer-0c-knowledge-edge-pairs-004.md", "layer-1-transition-orchestrator-pairs-007.md"]

B:
  TeachingUnit: "MULTI-SUBPHASE-ROUTING-001"
  Content:
    title: "One Predicate, Multiple Sub-Phase Routing"
    trigger_condition:
      predicate: "DOCUMENT"
      default_routing: "→ 2.1 (Business Requirements)"
      dual_routing_trigger: "stakeholder signal words present in request"
      dual_routing_result: "→ 2.1 (RequirementsAuthoringFlow21) AND → 2.4 (StakeholderRequirementsFlow24)"
    stakeholder_signal_words:
      - "stakeholder"
      - "external partner"
      - "marketplace vendor"
      - "payment partner"
      - "vendor requirements"
    canonical_example:
      request: "Define the checkout use cases and document the stakeholder requirements for external payment partners and marketplace vendors."
      routing_2_1: "PacketId: B06-packet-document-2.1 → RequirementsAuthoringFlow21"
      routing_2_4: "PacketId: B06-packet-document-2.4 → StakeholderRequirementsFlow24"
    key_rule: "One predicate instance is created. Two routing decisions are emitted from that one predicate. Two flows run independently. Both receive the same UserRequest."
    v6_error: "v6-03 routing collapse — routing only to 2.1 when 2.4 is also required"

---

## B06-L3-004-002 — Sub-phase 2.4 identity

```
source_id: xiigen-b06-l3-004-002
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "What sub-phase 2.4 (Stakeholder Requirements) produces"

B:
  TeachingUnit: "SUBPHASE-2-4-IDENTITY-001"
  Content:
    title: "Sub-Phase 2.4 — Stakeholder Requirements Identity"
    sub_phase_id: "2.4"
    flow_id: "StakeholderRequirementsFlow24"
    produces: "Formal stakeholder requirements document from the perspective of external parties"
    external_parties_covered:
      - "Payment gateway / payment partner"
      - "Marketplace vendor / seller"
      - "Shipping carrier"
      - "Tax and compliance service"
    contrast_with_2_1:
      "2.1 output": "Internal use cases, business rules, acceptance criteria for the system"
      "2.4 output": "External partner SLAs (Service-Level Agreements), integration requirements, partner-facing contracts"
    relationship: "2.1 and 2.4 are complementary — 2.1 is internal perspective, 2.4 is external partner perspective"
    ordering: "2.1 runs before 2.4 in the canonical ordering when both are present"

---

## B06-L3-004-003 — Multi-sub-phase routing summary table

```
source_id: xiigen-b06-l3-004-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "MULTI-SUBPHASE-TABLE-001"
  Content:
    title: "Complete Multi-Sub-Phase Routing Table for B06"
    routing_cases:
      "DOCUMENT (no stakeholder signal)":         "→ 2.1 only; 1 routing decision"
      "DOCUMENT (stakeholder signal present)":     "→ 2.1 + 2.4; 2 routing decisions"
      "DOCUMENT + ARCHITECTURE":                  "→ 2.1 + 3.1; 2 routing decisions"
      "DOCUMENT + IMPLEMENT":                     "→ 2.1 + 4.1; 2 routing decisions"
      "DOCUMENT + UI":                            "→ 2.1 + 1.1; 2 routing decisions"
      "DOCUMENT (stakeholder) + IMPLEMENT":       "→ 2.1 + 2.4 + 4.1; 3 routing decisions"
      "All four predicates":                      "→ 2.1 + 3.1 + 4.1 + 1.1; 4 routing decisions (2.4 if stakeholder signal also)"
    canonical_ordering: "2.1 → 3.1 → 2.4 → 1.1 → 4.1"
    key_rule: "Each sub-phase gets exactly one routing decision. The routing decision count equals the number of active sub-phases, not the number of predicates."
