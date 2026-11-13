# MANIFEST — B33-batch-001
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 (001–012); 2026-05-06 (013–024, synthesis 005–008)
# Total files: 201

---

## File Count by Layer

| Layer | Files |
|-------|-------|
| Layer 0-A (compiler pairs) | 24 (001–024) |
| Layer 0-B (router pairs) | 24 (001–024) |
| Layer 0-C (knowledge edge pairs) | 24 (001–024) |
| Layer 0-D (flow factory pairs) | 24 (001–024) |
| Layer 1 (transition orchestrator pairs) | 24 (001–024) |
| Layer 2 (capability gap extended pairs) | 24 (001–024) |
| Layer 3 (teaching unit pairs) | 24 (001–024) |
| Held-out and corruption pairs | 24 (001–024) |
| Cross-layer synthesis | 8 (001–008) |
| MANIFEST | 1 |
| **Total** | **201** |

---

## Layer 2 Capability Gap Extended Pairs

| File | Request type | Predicate result | Verdict |
|------|-------------|-----------------|---------|
| 001 | Original batch | mixed | — |
| 002 | GapHandler: adapt for legal domain | [] gap | GAP_LOGGED |
| 003 | GapHandler: fine-tune on proprietary data | [] gap | GAP_LOGGED |
| 004 | ImplementationFlow41: build RAG pipeline | [IMPLEMENT] | IMPLEMENT_APPROVED |
| 005 | GapHandler: chain-of-thought reasoning | [] gap | GAP_LOGGED |
| 006 | SecurityAuditFlow: prompt injection prevention | [SECURITY] | SECURITY_APPROVED |
| 007 | ArchitectureFlow31: multi-agent platform design | [ARCHITECTURE] | ARCHITECTURE_APPROVED |
| 008 | GapHandler: autonomous decision-making | [] gap | GAP_LOGGED |
| 009 | APIContractFlow33: integrate with Stripe | [API_CONTRACT] | API_CONTRACT_APPROVED |
| 010 | GapHandler: tool-calling skills | [] gap | GAP_LOGGED |
| 011 | ImplementationFlow41: build episodic memory | [IMPLEMENT] | IMPLEMENT_APPROVED |
| 012 | GapHandler: grounding + gap ID summary | [] gap | GAP_LOGGED |
| 013 | GapHandler: fine-tune the model | [] gap | GAP_LOGGED |
| 014 | GapHandler: chain-of-thought prompting | [] gap | GAP_LOGGED |
| 015 | Mixed: build agent + fine-tune | [IMPLEMENT] + GapWarning | IMPLEMENT_APPROVED |
| 016 | GapHandler: self-healing agent | [] gap | GAP_LOGGED |
| 017 | GapHandler: reinforcement learning loop | [] gap | GAP_LOGGED |
| 018 | GapHandler: prompt engineering workflow | [] gap | GAP_LOGGED |
| 019 | IMPLEMENT+API_CONTRACT: build agent API | [IMPLEMENT, API_CONTRACT] | IMPLEMENT+API_CONTRACT_APPROVED |
| 020 | API_CONTRACT fires: orchestrate AI workflows via API | [API_CONTRACT] + GapWarning | API_CONTRACT_APPROVED |
| 021 | ARCHITECTURE+IMPLEMENT: architect+implement agent platform | [ARCHITECTURE, IMPLEMENT] | ARCHITECTURE+IMPLEMENT_APPROVED |
| 022 | Multiple AI gap vocabulary in one request | [] gap | GAP_LOGGED |
| 023 | AI gap vs code predicate disambiguation table | reference | — |
| 024 | Complete B33 capability-gap summary | reference | — |

---

## Key Teaching Points

1. **AI vocabulary → gap** — adapt, fine-tune, orchestrate, RAG (as goal), chain-of-thought, LLM, autonomous, agentic, tool-calling (as capability), grounding, hallucination, temperature, context window, system prompt, few-shot, self-healing (AI), reinforcement/RLHF, prompt engineering, evolve the system → all gap
2. **Code verb overrides AI object** — "build a RAG pipeline" → IMPLEMENT; "adapt the agent" → gap. The ACTION VERB determines the predicate, not the object.
3. **prompt injection is the only SECURITY exception** — "prompt injection" fires SECURITY; all other "prompt" uses → gap. Rationale: injection = attack vector class.
4. **"integrate" always fires API_CONTRACT** — in all contexts including AI, regardless of object.
5. **"orchestrate" is NOT ARCHITECTURE** — "orchestrate" = AI vocabulary → gap. ARCHITECTURE requires design/architect verbs + structural nouns.
6. **DomainId is always software-development** — no ai-development, ml-development, or llm-development domain exists.
7. **Three gap IDs** — gap-ai-agent-adapt-001 (adaptation), gap-ai-agent-skill-001 (skill/capability), gap-ai-agent-llm-001 (LLM quality/reasoning). One SCHEDULE_CAPABILITY_LEARNING per request.
8. **One firing predicate suppresses GapHandler** — mixed gap+code requests: gap verbs silently skipped if any predicate fires.
9. **TransitionType unchanged by AI context** — feature_addition > refactoring > documentation. Same hierarchy as all batches.
10. **QA always last** — absolute ordering constraint; QA validates all preceding flows.
11. **Framework names are gap** — LangChain, AutoGPT, CrewAI, LlamaIndex, Semantic Kernel, etc. → gap unless preceded by code verb.
12. **v6 corruptions** — v6-01: invented predicates (FINE_TUNE, FEW_SHOT, REASONING, SELF_HEAL, REINFORCE, AI_AGENT); v6-02: gap suppressed (adapt→IMPLEMENT, orchestrate→ARCHITECTURE, fine-tune→IMPLEMENT); v6-03: multi-predicate collapsed
13. **Primary verb rule** — In compound sentences, the primary action verb determines routing. Subordinate/negative clause verbs (e.g., "rather than build", "instead of adapting") do not fire predicates.
14. **Adjectives do not suppress code verbs** — "self-healing", "few-shot-capable", "AI-powered", "autonomous" as adjectives modifying software artifacts do not prevent code verbs from firing.
15. **One SCHEDULE_CAPABILITY_LEARNING per request** — regardless of how many gap vocabulary terms appear, exactly one event is emitted per gap request.

---

## Gap IDs Registered

- gap-ai-agent-adapt-001: adapt/agentic/autonomous/persona/orchestrate/self-directed/self-healing/evolve vocabulary
- gap-ai-agent-skill-001: tool-calling/skill/capability/teach/enable (AI behavior)/expose as API vocabulary
- gap-ai-agent-llm-001: LLM/fine-tune/chain-of-thought/RAG/grounding/hallucination/temperature/context window/few-shot/reinforcement/RLHF/prompt engineering vocabulary

---

## Cross-Layer Synthesis Traces

| File | Scenario | Predicate Set |
|------|----------|---------------|
| 001 | Build guardrails + prompt injection prevention | IMPLEMENT+SECURITY |
| 002 | Gap: "Adapt agent for healthcare domain" | [] → GapHandler |
| 003 | Design skill engine + build dispatcher + unit tests | ARCHITECTURE+IMPLEMENT+TEST |
| 004 | Full AI agent platform cycle | ARCHITECTURE+API_CONTRACT+IMPLEMENT+SECURITY+TEST+DOCUMENT+QA |
| 005 | Pure gap: "fine-tune the orchestrator" | [] → GapHandler → gap-ai-agent-llm-001 |
| 006 | "build the agent skill API" + integrate | IMPLEMENT+API_CONTRACT + gap warning |
| 007 | "architect the AI agent system" | ARCHITECTURE fires + gap warning |
| 008 | ARCHITECTURE+API_CONTRACT+IMPLEMENT+SECURITY alongside AI gap vocabulary | Four predicates; gap warnings only |
