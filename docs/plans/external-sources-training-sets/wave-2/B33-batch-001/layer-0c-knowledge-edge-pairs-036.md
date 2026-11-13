# Layer 0-C — Knowledge Edge Pairs (036)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-036-001 — Complete Round 2 AI/ML gap trigger list

Full list of Round 2 gap trigger terms (none fire predicates when used as primary verbs/nouns without code verbs):

| Term | Gap category | Gap ID |
|------|-------------|--------|
| tokenize the input | Model-level operation | gap-ai-ml-model-001 |
| embed (AI vector conversion) | Model-level operation | gap-ai-ml-model-001 |
| vector search (standalone) | AI retrieval method | gap-ai-ml-model-001 |
| RAG pipeline (goal) | AI system pattern | gap-ai-ml-model-001 |
| context window | AI model property | gap-ai-ml-model-001 |
| attention mechanism | AI model component | gap-ai-ml-model-001 |
| transformer (AI architecture) | AI model type | gap-ai-ml-model-001 |
| inference endpoint (standalone) | AI serving description | gap-ai-ml-model-001 |
| model serving (standalone) | AI operational concern | gap-ai-ml-model-001 |
| hallucination mitigation | AI quality concern | gap-ai-ml-model-001 |
| neural network | AI model type | gap-ai-ml-training-001 |
| training loop | AI training operation | gap-ai-ml-training-001 |
| gradient descent | AI optimization algorithm | gap-ai-ml-training-001 |
| backpropagation | AI training mechanism | gap-ai-ml-training-001 |
| hyperparameter (standalone) | AI training parameter | gap-ai-ml-training-001 |
| overfitting | AI quality problem | gap-ai-ml-training-001 |
| regularization (standalone) | AI training technique | gap-ai-ml-training-001 |
| cross-validation | AI evaluation method | gap-ai-ml-training-001 |
| benchmark (AI evaluation) | AI evaluation activity | gap-ai-ml-training-001 |
| evaluation metric (F1/BLEU/ROUGE) | AI quality measurement | gap-ai-ml-training-001 |
| dataset curation | AI data preparation | gap-ai-ml-training-001 |

---

## KE-036-002 — Code verbs that override all Round 2 AI objects

Code verbs that fire predicates regardless of AI vocabulary in the object (same as all batches):

| Verb | Predicate | Note |
|------|-----------|------|
| build, implement, create, develop, write code, refactor | IMPLEMENT | Code-action verbs |
| design, architect, plan, spec + structural noun | ARCHITECTURE | Structural noun required |
| integrate, connect, wire, interface with | API_CONTRACT | Always fires; no restriction |
| secure, harden, prevent prompt injection | SECURITY | "prompt injection" is sufficient |
| write tests, create tests, add tests | TEST | Test-writing form required |
| document, write docs | DOCUMENT | Documentation verbs |
| QA (abbreviation) | QA | Abbreviation only; must be last |

---

## KE-036-003 — Gap suppression rule for Round 2 mixed requests

Rule: When any code predicate fires in a mixed gap+code request, Round 2 gap vocabulary is silently skipped.
Example: "embed the documents and integrate with the vector store API" → ["API_CONTRACT"]
"embed" (gap-ai-ml-model-001) silently skipped. "integrate" fires API_CONTRACT. Result: predicates only.

---

## KE-036-004 — DomainId is always software-development for all Round 2 gaps

All Round 2 gap IDs use DomainId: software-development.
There is no "ai-development", "ml-development", "data-science", or "model-development" domain in the registry.
This rule applies to: tokenize / embed / vector search / RAG / context window / attention / transformer / inference / serving / hallucination / neural network / training loop / gradient descent / backpropagation / hyperparameter / overfitting / regularization / cross-validation / benchmark / evaluation metric / dataset curation gaps.

---

## KE-036-005 — One SCHEDULE_CAPABILITY_LEARNING per Round 2 gap request

Rule: Exactly one SCHEDULE_CAPABILITY_LEARNING event is emitted per Round 2 gap request, regardless of how many gap vocabulary terms appear.
Example: "neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, dataset curation" in one request → one SCHEDULE_CAPABILITY_LEARNING, one Gap ID (gap-ai-ml-training-001).
Never emit multiple SCHEDULE_CAPABILITY_LEARNING events for a single request.
