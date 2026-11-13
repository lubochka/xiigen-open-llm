# Layer 0-C — Knowledge Edge Pairs (049)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-049-001 — AI agent gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| AI agent (standalone) | No code verb | gap-ai-ml-model-001 |
| AI agent runtime (with "build") | Code verb present | IMPLEMENT fires |
| AI agent tool dispatcher (with "implement") | Code verb present | IMPLEMENT fires |
| AI agent architecture (with "document") | "document" present | DOCUMENT fires |
| AI agent endpoints (with "secure") | "secure" present | SECURITY fires |

---

## KE-049-002 — Multi-agent gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| multi-agent (standalone) | No code verb | gap-ai-ml-model-001 |
| multi-agent coordination architecture (with "design") | "design" + structural noun | ARCHITECTURE fires |
| multi-agent dispatcher (with "build") | Code verb present | IMPLEMENT fires |
| multi-agent message broker (with "implement") | Code verb present | IMPLEMENT fires |

---

## KE-049-003 — AI agent vs. agent vocabulary from earlier B33 rounds

Cross-reference rule:
- "AI agent" (round 3) → gap-ai-ml-model-001
- "adapt the agent" (earlier B33 rounds) → gap-ai-agent-adapt-001
- "agent tool-calling" (earlier B33 rounds) → gap-ai-agent-skill-001
These are distinct gap IDs. "AI agent" as a model-level term uses gap-ai-ml-model-001.
Context determines which gap ID: if request is about the model type → gap-ai-ml-model-001; if about adapting behavior → gap-ai-agent-adapt-001.

---

## KE-049-004 — IMPLEMENT+SECURITY: injection context fires SECURITY alongside IMPLEMENT

When "secure" + "injection" appear alongside IMPLEMENT:
"Build the multi-agent dispatcher and secure the agent communication endpoints against injection" → [IMPLEMENT, SECURITY].
"Injection" context (not only "prompt injection") triggers SECURITY when combined with "secure".
Standard flow ordering: IMPLEMENT then SECURITY.

---

## KE-049-005 — Multi-agent ARCHITECTURE requires structural noun

Rule: "multi-agent coordination" alone + "design" does not fire ARCHITECTURE unless structural noun is present.
"Design multi-agent coordination" → may not fire ARCHITECTURE (no structural noun).
"Design the multi-agent coordination architecture" → ARCHITECTURE fires ("architecture" = structural noun).
"Design the multi-agent coordination platform" → ARCHITECTURE fires ("platform" = structural noun).
