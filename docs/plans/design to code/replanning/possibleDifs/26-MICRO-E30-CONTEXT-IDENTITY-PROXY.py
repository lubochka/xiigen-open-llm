#!/usr/bin/env python3
"""Illustrative source-aligned E30 feature identity proxy. NOT an E30/.NET training run."""
import json

def file_vec():
    # Frequency feature from one full file; intentionally equal for both transitions.
    return tuple([1/3, 1/3, 1/3] + [0.0] * 19)

# File IDs (NOT semantic families) are disjoint between train/heldout.
train = [(f"train-{i}", cur, nxt, file_vec()) for i in range(1, 7)
         for cur, nxt in [("NULL_GUARD", "CALL"), ("CALL", "RETURN")]]
heldout = [(f"blind-{i}", cur, nxt, file_vec()) for i in range(1, 7)
           for cur, nxt in [("NULL_GUARD", "CALL"), ("CALL", "RETURN")]]

def legacy_predict(rows, query):
    # Mirrors the E30 first-equal-cosine winner for identical non-zero vectors.
    return rows[0][2] if rows else None

def conditioned_predict(rows, query):
    # Change exactly one feature: include Current in the learned lookup address.
    matches = [r for r in rows if r[1] == query[1] and r[3] == query[3]]
    return matches[0][2] if matches else None

baseline = [legacy_predict(train, x) for x in heldout]
conditioned = [conditioned_predict(train, x) for x in heldout]

# Negative control: corrupt ONLY one class's labels in training memory.
corrupt = [(name, current, "FAIL" if current == "CALL" else label, vec)
           for name, current, label, vec in train]
control = [conditioned_predict(corrupt, x) for x in heldout]
count_correct = lambda preds: sum(y == r[2] for y, r in zip(preds, heldout))
result = {
    "run_kind": "SYNTHETIC_PYTHON_PROXY_NOT_NATIVE_E30",
    "distinct_train_files": len({r[0] for r in train}),
    "distinct_heldout_files": len({r[0] for r in heldout}),
    "train_transitions": len(train),
    "heldout_transitions": len(heldout),
    "vector_dimension": 22,
    "different_labels_share_exact_vector": True,
    "baseline_correct": count_correct(baseline),
    "conditioned_correct": count_correct(conditioned),
    "poison_control_correct": count_correct(control),
    "baseline_prediction_counts": {x:baseline.count(x) for x in sorted(set(baseline))},
    "conditioned_prediction_counts": {x:conditioned.count(x) for x in sorted(set(conditioned))},
    "limitations": "Illustrative deterministic pair representation. Synthetic families reuse the same transition rule; no source-disjoint semantic generalization or product-quality claim."
}
assert result["baseline_correct"] == 6
assert result["conditioned_correct"] == 12
assert result["poison_control_correct"] == 6
print(json.dumps(result, ensure_ascii=False, sort_keys=True, indent=2))
