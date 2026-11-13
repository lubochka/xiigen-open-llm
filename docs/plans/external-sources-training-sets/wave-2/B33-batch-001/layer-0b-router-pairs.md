# Layer 0-B — Router Pairs
# Batch: B33-batch-001
# Scenario: B33 — AI agent adaptation — building and adapting AI agents and skills
# Contract: PromptRoutingInputRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRouterResult>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B33B1-WDA-router-001 — API_CONTRACT → 3.3

```
source_id: xiigen-b33-router-001
transition_type: feature_addition
phase: 3
sub_phase: 3.3
coverage_status: ready
pair_kind: router
```

A:
  compiled_predicate_ids: ["API_CONTRACT"]
  compiled_text: "Adapt the AI agent to handle customer support skills and integrate autonomous ticket resolution."

B:
  RoutingDecisions:
    - predicate: API_CONTRACT
      sub_phase: 3.3
      sub_phase_label: "API and Integration Design"
      confidence: 0.92
      routing_note: "'integrate' fires API_CONTRACT → sub-phase 3.3 regardless of AI framing"

  TransitionType: feature_addition
  PhaseId: 3
  SubPhaseId: 3.3
  CapabilityGapWarnings:
    - "gap-ai-agent-adapt-001: 'adapt', 'AI agent', 'skills', 'autonomous' are gap vocabulary"
    - "gap-ai-agent-skill-001: AI skill vocabulary not captured by API_CONTRACT routing"

Teaching: "integrate" routes to 3.3 even when the sentence reads as AI agent work.
The API_CONTRACT predicate captures only the integration aspect; the agent/skill
dimension is unrouted gap vocabulary.

---

## Pair B33B1-WDA-router-002 — IMPLEMENT → 4.1

```
source_id: xiigen-b33-router-002
transition_type: feature_addition
phase: 4
sub_phase: 4.1
coverage_status: ready
pair_kind: router
```

A:
  compiled_predicate_ids: ["IMPLEMENT"]
  compiled_text: "Build the AI agent module and implement the skill registration system for autonomous task handling."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'build' + 'module' + 'implement' + 'system' → 4.1; no connector/migration vocabulary"

  TransitionType: feature_addition
  PhaseId: 4
  SubPhaseId: 4.1
  CapabilityGapWarnings:
    - "gap-ai-agent-adapt-001: 'AI agent', 'autonomous', 'skill registration' are gap vocabulary"

Sub-phase disambiguation: "module" + "system" → 4.1 (service/logic); not "connector"
or "webhook" (which would push to 4.3) and not "migrate" or "seed" (4.4).

---

## Pair B33B1-WDA-router-003 — ARCHITECTURE → 3.1

```
source_id: xiigen-b33-router-003
transition_type: feature_addition
phase: 3
sub_phase: 3.1
coverage_status: ready
pair_kind: router
```

A:
  compiled_predicate_ids: ["ARCHITECTURE"]
  compiled_text: "Design the architecture for the LLM-powered agent orchestration layer with RAG and tool-use components."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.1
      sub_phase_label: "System Architecture and Technical Design"
      confidence: 0.92
      routing_note: "'design' + 'architecture' + 'layer' + 'components' → 3.1; LLM/RAG/tool-use are gap but don't affect routing"

  TransitionType: feature_addition
  PhaseId: 3
  SubPhaseId: 3.1
  CapabilityGapWarnings:
    - "gap-ai-agent-llm-001: 'LLM', 'RAG', 'tool-use', 'agent orchestration' are gap vocabulary"

Teaching: Gap vocabulary in the sentence does not redirect routing. ARCHITECTURE fires
and routes to 3.1; the AI-specific dimensions are captured as gap warnings only.
