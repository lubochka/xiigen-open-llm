# Layer 0-D — Flow Factory Pairs (024)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-024-001 — GapHandler: full B33 gap vocabulary, no code verb

A: [] | "Adapt the model, fine-tune, evolve the system, prompt engineering, self-healing, reinforcement, few-shot, chain-of-thought, orchestrate, skill, agent."
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001 (dominant: LLM quality vocabulary)
Reason: All B33 gap vocabulary, no code verb. Single SCHEDULE_CAPABILITY_LEARNING emitted.

---

## FF-024-002 — IMPLEMENT: code verb overrides all AI objects

A: ["IMPLEMENT"] | "Build an agent using few-shot learning, chain-of-thought, self-healing reinforcement."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. All AI vocabulary in object position. Code verb wins.

---

## FF-024-003 — API_CONTRACT: integrate fires despite AI vocabulary

A: ["API_CONTRACT"] | "Integrate the fine-tuning orchestrator with the model registry API."
Factory: [APIContractFlow33]
Teaching: "integrate" always fires API_CONTRACT. AI vocabulary does not suppress it.

---

## FF-024-004 — GapHandler: requirement statement with multiple AI terms

A: [] | "The orchestrator needs reinforcement and self-healing with few-shot learning support."
Factory: GapHandler | Gap ID: gap-ai-agent-adapt-001
Reason: Requirement statement without code verb. Multiple AI terms → gap.

---

## FF-024-005 — Seven flows: complete B33 vocabulary + all code verbs

A: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] | "Architect the agent skill platform, integrate the orchestrator API, implement the fine-tuning scheduler, secure against prompt injection, write tests for chain-of-thought output, document the self-healing policy, and QA sign-off."
Factory: [ArchitectureFlow31, APIContractFlow33, ImplementationFlow41, SecurityAuditFlow, E2ETestingFlow53, DocumentationFlow62, QAGatingFlow73] | TransitionType: feature_addition
Ordering: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → E2ETestingFlow53 ‖ DocumentationFlow62 → QAGatingFlow73 (last, absolute).
