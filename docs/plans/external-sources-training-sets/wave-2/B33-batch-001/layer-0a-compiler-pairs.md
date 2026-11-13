# Layer 0-A — Compiler Pairs
# Batch: B33-batch-001
# Scenario: B33 — AI agent adaptation — building and adapting AI agents and skills
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Method note

B33 is a CAPABILITY GAP batch. AI agent adaptation vocabulary has no registered predicate:
agent, skill (as an AI capability unit), autonomous, agentic, LLM, prompt engineering,
chain-of-thought, RAG (Retrieval-Augmented Generation), tool-use, orchestrator (as an
AI concept), adapt (in AI context). When mixed with development predicates (IMPLEMENT,
ARCHITECTURE), those predicates fire and routing proceeds. The "agent"/"skill" framing
produces CapabilityGapWarnings.

---

## Pair B33B1-WDA-compiler-001 — pure agent vocabulary: zero predicates

```
source_id: xiigen-b33-compiler-001
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: compiler
```

A:
  user_request: "Adapt the AI agent to handle customer support skills and integrate autonomous ticket resolution."

B:
  DocumentId: "B33-compiler-001-prompt"
  DocumentTitle: "AI agent adaptation for customer support skills"
  CompiledText: "Adapt the AI agent to handle customer support skills and integrate autonomous ticket resolution."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "adapt" — not in any predicate lexicon (adapt ≠ IMPLEMENT which needs build/code/write)
  "AI agent" — "agent" not in any predicate lexicon
  "skills" — "skill" not in any predicate lexicon (skill as AI capability unit)
  "integrate" → API_CONTRACT fires on "integrate"!
  "autonomous" — not in any predicate
  "ticket resolution" — not in any predicate

  Correction: "integrate" fires API_CONTRACT.
  CompiledPredicateIds: ["API_CONTRACT"]

Corrected B:
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-ai-agent-adapt-001: 'adapt', 'AI agent', 'skills', 'autonomous' are gap vocabulary"
    - "gap-ai-agent-skill-001: AI skill vocabulary not captured by API_CONTRACT routing"

Teaching: "integrate" fires API_CONTRACT even in an AI agent context. The agent/skill intent
is not captured — only the integration aspect routes.

---

## Pair B33B1-WDA-compiler-002 — agent build with IMPLEMENT: fires

```
source_id: xiigen-b33-compiler-002
transition_type: feature_addition
phase: 4
sub_phase: 4.1
coverage_status: mixed_signal
pair_kind: compiler
```

A:
  user_request: "Build the AI agent module and implement the skill registration system for autonomous task handling."

B:
  DocumentId: "B33-compiler-002-prompt"
  DocumentTitle: "AI agent module and skill registration system"
  CompiledText: "Build the AI agent module and implement the skill registration system for autonomous task handling."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-ai-agent-adapt-001: 'AI agent', 'autonomous', 'skill registration' are gap vocabulary"

Predicate evidence:
  "build" → IMPLEMENT; "module" → IMPLEMENT; "implement" → IMPLEMENT; "system" → IMPLEMENT (system ≈ service/module)
  "AI agent" — gap; "skill registration" — gap; "autonomous" — gap
  Result: ["IMPLEMENT"] high-density

---

## Pair B33B1-WDA-compiler-003 — LLM architecture design: ARCHITECTURE fires

```
source_id: xiigen-b33-compiler-003
transition_type: feature_addition
phase: 3
sub_phase: 3.1
coverage_status: mixed_signal
pair_kind: compiler
```

A:
  user_request: "Design the architecture for the LLM-powered agent orchestration layer with RAG and tool-use components."

B:
  DocumentId: "B33-compiler-003-prompt"
  DocumentTitle: "LLM agent orchestration architecture with RAG and tool-use"
  CompiledText: "Design the architecture for the LLM-powered agent orchestration layer with RAG and tool-use components."
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-ai-agent-llm-001: 'LLM', 'RAG', 'tool-use', 'agent orchestration' are gap vocabulary"

Predicate evidence:
  "design" → ARCHITECTURE; "architecture" → ARCHITECTURE; "layer" → ARCHITECTURE; "components" → ARCHITECTURE
  "LLM" — not in any predicate lexicon
  "RAG" — not in any predicate lexicon
  "agent orchestration" — "agent" not in predicate; "orchestration" not in predicate
  "tool-use" — not in any predicate
  Result: ["ARCHITECTURE"] via design + architecture + layer + components
