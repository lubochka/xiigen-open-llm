# Layer 0-C — Knowledge Edge Pairs (034)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-034-001 — AI vocabulary in subordinate clause does not fire predicates

Rule: In compound sentences, the primary action verb determines routing. Subordinate AI vocabulary phrases do not fire predicates.
Example: "Build a service rather than trying to tokenize manually" → ["IMPLEMENT"] ("build" fires; "tokenize" is subordinate)
Example: "Implement the pipeline instead of relying on the RAG inference endpoint" → ["IMPLEMENT"] ("implement" fires; "RAG inference endpoint" = subordinate context)

---

## KE-034-002 — Negative AI clauses do not fire gap

Rule: Negative AI vocabulary ("not tokenizing", "avoiding embedding", "without using transformer") does not trigger gap or predicates.
Example: "Build a search engine that does not rely on embedding" → ["IMPLEMENT"] (code verb "build" fires; negative "embedding" = irrelevant)
Example: "Implement routing without relying on neural network inference" → ["IMPLEMENT"]

---

## KE-034-003 — "latency" disambiguation: AI serving vs software performance

"latency" in AI model serving context = gap-ai-ml-model-001 vocabulary when no code verb present.
"latency" in general software context = not a gap if addressed by code verb:
1. "model serving latency is high" → gap (AI operational concern)
2. "refactor the serving pipeline to reduce latency" → IMPLEMENT (code verb fires)
3. "optimize database query latency" → IMPLEMENT (code verb + non-AI artifact)
The presence of "model serving" or "inference" marks it as AI vocabulary.

---

## KE-034-004 — "evaluation metric" as gap vs TEST boundary

"evaluation metric" boundary clarification:
1. "report F1 and BLEU scores" → gap (report = description, not code verb)
2. "evaluation metrics need to be reviewed" → gap (description)
3. "write tests measuring F1 score" → TEST (write tests form)
4. "create an evaluation metric dashboard" → IMPLEMENT (create + artifact noun)
The test-writing form ("write/create tests") is required for TEST to fire.

---

## KE-034-005 — Round 2 summary: all gap boundary distinctions

Complete boundary summary for Round 2 AI/ML vocabulary:
| AI vocabulary | Gap trigger | Code trigger (example) |
|--------------|-------------|----------------------|
| tokenize | verb: tokenize X | build tokenizer / implement tokenization |
| embed | verb: embed X | build embedding service / integrate endpoint |
| vector search | use/run vector search | build component / integrate API |
| RAG pipeline | set up / needs RAG | implement RAG / design RAG architecture |
| context window | manage / configure | build context manager |
| attention mechanism | tune / adjust | implement attention module |
| transformer | adjust / use | build transformer wrapper |
| inference endpoint | serves / is needed | integrate endpoint / secure endpoint |
| model serving | is needed / optimize | refactor serving layer |
| hallucination | mitigate / reduce | build hallucination guard |
| neural network | needs / is failing | build training infrastructure |
| training loop | runs / needs | build training executor |
| gradient descent | is unstable | implement optimizer |
| backpropagation | is failing | implement backprop engine |
| hyperparameter | tune / adjust | build HPO (HyperParameter Optimization) service |
| overfitting | is occurring | implement regularization |
| regularization | apply | implement regularization module |
| cross-validation | shows / results | build CV (Cross-Validation) framework / write CV tests |
| benchmark (AI) | benchmark model | write benchmark tests |
| evaluation metric | report / show | write metric tests |
| dataset curation | needs / remove | implement curation pipeline |
