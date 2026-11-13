# Layer 0-C — Knowledge Edge Pairs (018)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-018-001 — Edge: "reinforcement" learning vs "reinforcement" as code pattern

"reinforcement learning for the agent" → gap (AI training vocabulary; no code verb)
"implement a reinforcement signal collector" → IMPLEMENT (code verb; "collector" = software artifact)
"design the reinforcement feedback architecture" → ARCHITECTURE (design verb + "architecture" structural noun)

---

## KE-018-002 — Edge: "RLHF" abbreviation follows same rules as "reinforcement learning"

"RLHF (Reinforcement Learning from Human Feedback) for the production agent" → gap (abbreviation = AI training vocabulary)
"implement the RLHF reward model service" → IMPLEMENT (code verb + "service" = software artifact)
Teaching: Abbreviations follow the same rules as their expanded forms. RLHF (Reinforcement Learning from Human Feedback) → gap vocabulary unless preceded by a code verb applied to a software artifact.

---

## KE-018-003 — Edge: "reinforcement" does not suppress API_CONTRACT

"integrate the reinforcement training pipeline with the feedback API" → ["API_CONTRACT"]
Teaching: "integrate" always fires API_CONTRACT. "reinforcement training pipeline" is AI context only. API_CONTRACT fires regardless.

---

## KE-018-004 — Edge: "reward" in reinforcement context = gap vocabulary

"design the reward function for agent behavior optimization" → gap ("reward function" = AI training design; "design" here modifies "reward function", not a structural software noun)
"implement the reward calculation service" → IMPLEMENT (code verb + "service" = software artifact)
Teaching: "reward function" as AI design artifact → gap. "reward calculation service" as software artifact → IMPLEMENT.

---

## KE-018-005 — Edge: "reinforcement" + "self-healing" stacked AI vocabulary

"use reinforcement and self-healing to optimize agent behavior" → gap (both terms AI vocabulary; "use" does not fire code predicate)
"build a reinforcement-driven self-healing agent service" → IMPLEMENT (code verb; "service" = software artifact)
Teaching: Multiple stacked AI vocabulary terms do not change gap classification. Single code verb is sufficient to fire IMPLEMENT.
