# DECISIONS-LOCKED.md — Locked architectural decisions

These decisions are final. Do not reopen without explicit approval from Luba.

| Decision | What | Rationale |
|---|---|---|
| D-01 | OperationResult<T> — no exceptions | Consistent error handling, testable |
| D-02 | Fabric First — interfaces not concrete | Swap providers without code changes |
| D-03 | 5-level semantic compression (E01→E17) | 20× compression validated across tiers |
| D-04 | Domain split: CODE_Q / BUG / PLAN / ARCH / REFACTOR | E18 95% accuracy; these are the canonical names |
| D-05 | LoRA (Low-Rank Adaptation) adapters per project (2 MB each) | Base model shared, specialization cheap |
| D-06 | 70/30 Replay Buffer | E43: prevents catastrophic forgetting |
| D-07 | StackDetector before normalization | E20: C#/TS/PY markers destroyed by E15 if run after |
| D-08 | StratifiedSplit for all experiments | E40: seed-42 distribution collapse with naive split |
| D-09 | max_weight = total / (classes × rare) | E44: constant ×10 caused catastrophic forgetting |
| D-10 | BM25 (Best Match 25) + K-NN (K-Nearest Neighbours) fusion | E34V2: hybrid beats either alone at +8% hit@1 |
| D-11 | No external ML frameworks | TensorCore (project 00) is the only tensor primitive |
| D-12 | Domain labels: short form in code, long form in comments | CODE_Q not CodeQuestion in enum; D-04 is the only source of truth |
| D-13 | Tier promotion requires coverage verification, not only metric passage | E51A Tier 1 passed metrics while still exposing thin neighborhoods; promotion must check subject density, distinct predicate profiles, and named edge risks before scaling |
| D-14 | Internal LLM always first; external AI requires explicit single-use approval with a named missing-competence statement; all results examined by the internal model before return; all examined results stored as training signals; training runs at idle time only under IdleDetector gate; a new adapter installs only if after-accuracy exceeds before-accuracy | E51A/E51B calibrated the 0.50 confidence boundary; CH-01 defines the external-help package shape, CH-02 keeps arbiter authority above external answers, and the idle-learning queue prevents external AI from becoming a permanent dependency |
| D-15 | Priority decides what to attempt; KNN uses arbiter history as memory; arbiters decide what is accepted; no priority score, KNN score, budget pressure, style preference, or external AI answer can override an arbiter rejection | This locks the authority separation proven by E51B CH-02 and needed before domain-scoped KNN and arbiter-aware retrieval are transferred into projects 21-23 |
