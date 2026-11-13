# Layer 2 — Capability Gap Extended Pairs (replaces nine-node layer)
# Batch: B33-batch-001
# Scenario: B33 — AI agent adaptation — building and adapting AI agents and skills
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: capability_gap_extended

---

## Pair B33B1-WDA-gap-001 — AI agent adaptation vocabulary map

```
source_id: xiigen-b33-gap-001
pair_kind: capability_gap_extended
```

A: Identify all AI agent adaptation gap terms, their gap IDs, and why each has no predicate.

B:
  GapVocabularyMap:
    - term: "agent" / "AI agent" / "agentic"
      predicate_match: null
      gap_id: "gap-ai-agent-adapt-001"
      notes: "Agent as an AI software entity; distinct from 'agent' as a business/human agent"

    - term: "skill" / "skills" (AI capability unit)
      predicate_match: null
      gap_id: "gap-ai-agent-skill-001"
      notes: "AI skill = a discrete learned or configured capability unit; not a human skill"

    - term: "autonomous" / "autonomously"
      predicate_match: null
      gap_id: "gap-ai-agent-adapt-001"
      notes: "Describes agent behavior mode; no predicate captures autonomy"

    - term: "adapt" / "adaptation" (in AI context)
      predicate_match: null
      gap_id: "gap-ai-agent-adapt-001"
      notes: "AI adaptation (self-modification, fine-tuning, behavior adjustment) ≠ IMPLEMENT"

    - term: "LLM" / "large language model"
      predicate_match: null
      gap_id: "gap-ai-agent-llm-001"
      notes: "LLM as AI model class; no predicate covers AI model vocabulary"

    - term: "prompt engineering" / "prompting"
      predicate_match: null
      gap_id: "gap-ai-agent-llm-001"
      notes: "Prompt design discipline; not an IMPLEMENT or ARCHITECTURE term"

    - term: "RAG" / "Retrieval-Augmented Generation"
      predicate_match: null
      gap_id: "gap-ai-agent-llm-001"
      notes: "AI architecture pattern; 'retrieval' alone does not fire any predicate"

    - term: "chain-of-thought" / "few-shot" / "zero-shot"
      predicate_match: null
      gap_id: "gap-ai-agent-llm-001"
      notes: "LLM prompting strategies; entirely outside predicate vocabulary"

    - term: "tool-use" / "tool calling"
      predicate_match: null
      gap_id: "gap-ai-agent-skill-001"
      notes: "AI agent capability; 'tool' alone fires no predicate"

    - term: "orchestrator" (AI concept)
      predicate_match: null
      gap_id: "gap-ai-agent-adapt-001"
      notes: "AI orchestration ≠ software architecture; 'orchestrator' not in ARCHITECTURE lexicon"

    - term: "fine-tune" / "fine-tuning"
      predicate_match: null
      gap_id: "gap-ai-agent-llm-001"
      notes: "ML model tuning; not in any predicate lexicon"

  DomainId_when_zero_predicates: "software-development"
  Note: "AI agent adaptation is software development — the gap is in AI-specific vocabulary,
    not in domain. Contrast with B36/B37 (project-management domain gap)."

---

## Pair B33B1-WDA-gap-002 — "orchestrate" vs "orchestrator" boundary

```
source_id: xiigen-b33-gap-002
pair_kind: capability_gap_extended
```

A: "Design a system to orchestrate the AI agents across multiple tasks." Does ARCHITECTURE fire?

B:
  Analysis:
    "Design" → ARCHITECTURE (action verb)
    "system" → ARCHITECTURE (IMPLEMENT/ARCHITECTURE shared term — "design" tips to ARCHITECTURE)
    "orchestrate" — verb form; not in predicate lexicon alone
    "AI agents" — gap vocabulary

  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-ai-agent-adapt-001: 'orchestrate', 'AI agents' are gap vocabulary"

  Teaching: "design" + "system" fires ARCHITECTURE even when the system being designed
  is an AI agent orchestration system. The AI framing produces gap warnings, not a
  routing block. ARCHITECTURE routes to 3.1.

---

## Pair B33B1-WDA-gap-003 — "integrate" boundary in AI context: API_CONTRACT, NOT IMPLEMENT

```
source_id: xiigen-b33-gap-003
pair_kind: capability_gap_extended
```

A: "Integrate the AI agent with the CRM system to handle autonomous customer queries."
   Which predicate fires? Is this IMPLEMENT or API_CONTRACT?

B:
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-ai-agent-adapt-001: 'AI agent', 'autonomous' are gap vocabulary"

  Predicate evidence:
    "integrate" → API_CONTRACT (always, in any context)
    "CRM system" — no predicate (product term)
    "AI agent" — gap
    "autonomous" — gap
    "handle" — not in any predicate lexicon
    Result: ["API_CONTRACT"] only. NOT ["IMPLEMENT"].

  Teaching: "integrate" fires API_CONTRACT regardless of whether the integration
  target is a CRM, an AI agent, a payment service, or any other system. There is no
  reading of "integrate" that fires IMPLEMENT. This boundary is absolute.

---

## Pair B33B1-WDA-gap-004 — gap-ID DomainId for AI agent vocabulary

```
source_id: xiigen-b33-gap-004
pair_kind: capability_gap_extended
```

A: "When pure AI agent vocabulary fires zero predicates, what DomainId does the orchestrator
   emit? Why 'software-development' and not 'ai-development' or 'machine-learning'?"

B:
  DomainId: "software-development"
  Reason: "The orchestrator's domain taxonomy has two registered gap domains:
    'software-development' (for development-flavored zero-predicate requests) and
    'project-management' (for project planning/managing vocabulary). AI agent adaptation
    is a form of software development — the gap is in AI-specific vocabulary, not in
    domain classification. There is no registered 'ai-development' domain; all AI agent
    requests with zero predicates fall into 'software-development' until new gap domains
    are registered via SCHEDULE_CAPABILITY_LEARNING outcomes."

  Contrast:
    B36 (sprint/roadmap/backlog) → "project-management" domain gap
    B37 (track/coordinate/stakeholder status) → "project-management" domain gap
    B32 (vibe/prototype/exploratory) → "software-development" domain gap
    B33 (agent/LLM/RAG/autonomous) → "software-development" domain gap
