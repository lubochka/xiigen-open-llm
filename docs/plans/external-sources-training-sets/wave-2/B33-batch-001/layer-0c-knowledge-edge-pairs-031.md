# Layer 0-C — Knowledge Edge Pairs (031)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-031-001 — Gap suppression rule for Round 2 mixed requests

Rule: When any code predicate fires in a mixed gap+code request (including Round 2 AI vocabulary), gap vocabulary is silently skipped.
Example: "embed the documents and integrate with the vector store API" → ["API_CONTRACT"]
"embed" (gap-ai-ml-model-001) is silently skipped; "integrate" (API_CONTRACT) fires. Result contains only predicates.

---

## KE-031-002 — One SCHEDULE_CAPABILITY_LEARNING per Round 2 gap request

Rule: Exactly one SCHEDULE_CAPABILITY_LEARNING event is emitted per gap request, regardless of how many Round 2 gap vocabulary terms appear.
Example: "tokenize, embed, vector search, RAG, context window, attention mechanism" in one request → one SCHEDULE_CAPABILITY_LEARNING, one Gap ID (gap-ai-ml-model-001).
Never emit multiple events for a single request regardless of vocabulary count.

---

## KE-031-003 — "latency optimization" in AI serving context

"latency optimization" = reducing response time for AI model inference. When used in the context of serving/inference, it is AI operational vocabulary.
Rule: "latency optimization for model serving" → gap. "refactor serving pipeline to reduce latency" → IMPLEMENT. "profile and optimize" without structural noun → gap.

---

## KE-031-004 — Round 2 vs Round 1 gap ID selection

Round 1 gap IDs (files 001–024): gap-ai-agent-adapt-001, gap-ai-agent-skill-001, gap-ai-agent-llm-001
Round 2 gap IDs (files 025–036): gap-ai-ml-model-001, gap-ai-ml-training-001
Selection: Use the most specific applicable gap ID based on the dominant vocabulary category in the request.
Mixed Round 1+2 request: apply Round 2 ID if model/training vocabulary is dominant.

---

## KE-031-005 — "model serving" vs "serve" disambiguation

"model serving" as a noun phrase = AI operational vocabulary → gap.
"serve" as a verb in code context: "build a service that serves responses" = IMPLEMENT (code artifact noun "service" present).
"serve via inference endpoint" alone → gap (no code verb; "serve" here describes the AI operation, not a code action).
