# Layer 0-C — Knowledge Edge Pairs (005)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-005-001 — "LLM" keyword and predicate interaction

"LLM" alone → gap
"LLM performance" → gap
"LLM cost" → gap
"LLM provider" → gap (choosing OpenAI/Anthropic/etc. is config)
"LLM response time" → gap (operational metric)

When code verb is present:
"build an LLM proxy service" → IMPLEMENT
"design the LLM gateway architecture" → ARCHITECTURE
"integrate the LLM API" → API_CONTRACT
"document the LLM API interface" → DOCUMENT

---

## KE-005-002 — Prompt injection as SECURITY trigger

"prompt injection" → SECURITY (security vulnerability class, analogous to SQL injection)
"adversarial prompting" → SECURITY
"jailbreak prevention" → SECURITY
"input sanitization for LLM" → SECURITY
"prevent prompt leakage" → SECURITY

Rule: Prompt injection and adversarial AI input attacks → SECURITY predicate fires. This is an exception to general AI vocabulary being gap.

---

## KE-005-003 — "autonomous" disambiguation

"autonomous agent" (AI sense) → gap
"autonomous vehicle" (robotics) → gap
"autonomous deployment pipeline" (software, no AI sense) → depends on verb:
  "build an autonomous deployment pipeline" → IMPLEMENT (code verb; "autonomous" is adjective)
  "design autonomous scaling" → ARCHITECTURE (design verb)

Rule: "autonomous" as AI/agent descriptor → gap. "autonomous" as software feature adjective → verb determines predicate.

---

## KE-005-004 — "agent" fires no predicate

"agent" in any form does NOT fire a predicate by itself:
- AI agent / software agent / agent-based / agentic → gap (as nouns/adjectives)
- "agent" as subject, object, or modifier → gap unless paired with code verb
- Exception: code verbs applied to agents → predicate fires from the verb, not "agent"

---

## KE-005-005 — "chain-of-thought" is gap

"chain-of-thought prompting" → gap
"chain-of-thought reasoning" → gap
"add chain-of-thought to the prompt" → gap (adding to prompt, not code)
"implement chain-of-thought reasoning engine" → IMPLEMENT (implementing as software module)
