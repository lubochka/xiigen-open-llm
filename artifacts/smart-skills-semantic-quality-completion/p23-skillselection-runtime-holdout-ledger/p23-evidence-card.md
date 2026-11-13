# P23 evidence card

P23 tried to create an accepted SkillSelection runtime holdout ledger before any side-by-side comparison or promotion.

Result: no accepted holdout rows were created.

Why:
- P16E and P16E2 rows are explicitly schema-only, not runtime evaluation.
- P21 says source-disjoint proof is false.
- P22 found zero accepted runtime-eval rows.
- P16B and P17C candidate rows are adaptive, train-side, already represented by checkpoint families, or missing an accepted runtime-ready record.

What was written:
- `p23-runtime-holdout-ledger.jsonl` is intentionally empty.
- `p23-runtime-holdout-gap-ledger.jsonl` lists rejected candidates and exact reasons.
- No model, checkpoint, training data, production code, runtime/common path, or promotion target was changed.

No-overclaim statement:
This is a holdout-ledger gap artifact only. It does not claim semantic quality, SkillSelection readiness, full API readiness, runtime promotion, or common model replacement.

Следующий repair:
Создать новые redacted local SkillSelection runtime holdout rows из approved source families, которых нет ни в одном из двух checkpoints; доказать pair-id и source-family exclusion против обоих checkpoints; затем заново запустить ledger gate до любого comparison.