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
