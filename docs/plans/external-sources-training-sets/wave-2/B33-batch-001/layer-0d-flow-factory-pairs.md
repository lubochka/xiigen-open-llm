# Layer 0-D — Flow Factory Pairs
# Batch: B33-batch-001
# Scenario: B33 — AI agent adaptation — building and adapting AI agents and skills
# Contract: FlowFactoryUnit : ILogicalUnit<SubPhaseRouterResult, FlowInstanceDescriptor>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B33B1-WDA-factory-001 — API_CONTRACT / 3.3 → deterministic_generic

```
source_id: xiigen-b33-factory-001
transition_type: feature_addition
phase: 3
sub_phase: 3.3
pair_kind: factory_selection
```

A:
  sub_phase: 3.3
  predicate: API_CONTRACT
  compiled_text: "Adapt the AI agent to handle customer support skills and integrate autonomous ticket resolution."
  capability_gap_warnings: ["gap-ai-agent-adapt-001", "gap-ai-agent-skill-001"]

B:
  TrustState: deterministic_generic
  FlowId: "ApiContractDesignFlow33"
  FlowLabel: "API and Integration Design Flow — sub-phase 3.3"
  SelectionReason: "API_CONTRACT predicate at 3.3 with feature_addition transition — established
    flow pattern; capability gap warnings do not degrade trust state"
  NodeCount: 9
  CapabilityGapWarningPassthrough: true

Trust state reasoning: The routing is deterministic (API_CONTRACT → 3.3 is unambiguous).
Gap vocabulary in the request produces warnings but does not downgrade trust from
deterministic_generic. The gap is in vocabulary coverage, not in routing confidence.

---

## Pair B33B1-WDA-factory-002 — IMPLEMENT / 4.1 → deterministic_generic

```
source_id: xiigen-b33-factory-002
transition_type: feature_addition
phase: 4
sub_phase: 4.1
pair_kind: factory_selection
```

A:
  sub_phase: 4.1
  predicate: IMPLEMENT
  compiled_text: "Build the AI agent module and implement the skill registration system for autonomous task handling."
  capability_gap_warnings: ["gap-ai-agent-adapt-001"]

B:
  TrustState: deterministic_generic
  FlowId: "BackendServiceFlow41"
  FlowLabel: "Service and Logic Implementation Flow — sub-phase 4.1"
  SelectionReason: "IMPLEMENT predicate at 4.1 with feature_addition transition and
    high-density predicate evidence (build/module/implement/system) — deterministic_generic"
  NodeCount: 9
  CapabilityGapWarningPassthrough: true

Trust state reasoning: Four IMPLEMENT predicate terms present (build, module, implement,
system) — high-density match. The AI agent framing is gap vocabulary but does not
affect IMPLEMENT routing confidence.

---

## Pair B33B1-WDA-factory-003 — ARCHITECTURE / 3.1 → deterministic_generic

```
source_id: xiigen-b33-factory-003
transition_type: feature_addition
phase: 3
sub_phase: 3.1
pair_kind: factory_selection
```

A:
  sub_phase: 3.1
  predicate: ARCHITECTURE
  compiled_text: "Design the architecture for the LLM-powered agent orchestration layer with RAG and tool-use components."
  capability_gap_warnings: ["gap-ai-agent-llm-001"]

B:
  TrustState: deterministic_generic
  FlowId: "SystemArchitectureFlow31"
  FlowLabel: "System Architecture and Technical Design Flow — sub-phase 3.1"
  SelectionReason: "ARCHITECTURE predicate at 3.1 with feature_addition transition and
    high-density match (design/architecture/layer/components) — deterministic_generic"
  NodeCount: 9
  CapabilityGapWarningPassthrough: true

---

## Pair B33B1-WDA-factory-004 — zero predicates / knowledge_edge → knowledge_edge_synthetic

```
source_id: xiigen-b33-factory-004
transition_type: unknown
phase: null
sub_phase: null
pair_kind: factory_selection
```

A:
  sub_phase: null
  predicate: null
  compiled_text: "Adapt the agent to learn new skills autonomously."
  capability_gap_signal:
    gap_id: "gap-ai-agent-adapt-001"
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  FlowLabel: "No flow instantiated — capability gap"
  SelectionReason: "Zero predicates compiled → knowledge_edge_synthetic; no nine-node flow
    runs; SCHEDULE_CAPABILITY_LEARNING emitted to AutonomousLearningPipeline"
  NodeCount: 0
  CapabilityGapSignalForwarded: true

Trust state reasoning: knowledge_edge_synthetic is the only valid trust state when
CompiledPredicateIds is empty. No flow is selected. The pipeline terminates here
and the gap signal is forwarded for learning.
