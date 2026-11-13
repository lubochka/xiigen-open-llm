#!/usr/bin/env python3
"""Read-only, source-bound mathematical probes and inventory. No native C# execution."""
import collections
import hashlib
import json
import math
import re
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE / "repos/core"
SOURCES = [
    "35.ExperimentsV2/Core/ProbeNeuralNetwork.cs",
    "42.ProjectWikiDomain/PromptFlowGroundingModel.cs",
    "42.ProjectWikiDomain/OrchestrationKnnStore.cs",
    "27.TranslationDomain/TranslationCorrectionLearningAdapter.cs",
    "docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md",
    "docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md",
]

def clipped_loss(target_logit):
    p = math.exp(target_logit) / (1 + math.exp(target_logit))
    return -math.log(max(p, 1e-7))

def exact_loss(target_logit):
    return math.log1p(math.exp(target_logit)) - target_logit

z = -20.0
epsilon = 1e-5
p = math.exp(z) / (1 + math.exp(z))
loss = {
    "kind": "independent scalar mathematical counterexample; Python double, not C# float execution",
    "logits": [0.0, z], "target_index": 1,
    "target_probability": p,
    "published_clipped_loss": clipped_loss(z),
    "true_cross_entropy_from_logits": exact_loss(z),
    "central_difference_clipped_loss": (clipped_loss(z+epsilon)-clipped_loss(z-epsilon))/(2*epsilon),
    "central_difference_true_loss": (exact_loss(z+epsilon)-exact_loss(z-epsilon))/(2*epsilon),
    "source_backward_gradient": p-1.0,
    "consequence": "reported clipped objective is locally flat while the cross-entropy gradient is nonzero; keep correct gradient, report stable log-sum-exp objective",
}

# Mirrors only the all-history compatibility predicate and input-feature merge,
# not the model's caller, training gate, normalization or checkpoint runtime.
lessons = [
    {"id": "old-a", "target": "A", "features": {"old": 1}},
    {"id": "new-b1", "target": "B", "features": {"beta": 1}},
    {"id": "new-b2", "target": "B", "features": {"gamma": 1}},
]
history, weights, ledger = [], {}, []
for row in lessons:
    compatible = bool(history) and all(old["target"] == row["target"] for old in history)
    fresh = dict(row["features"])
    if weights and compatible:
        for token, value in weights.items():
            fresh[token] = max(fresh.get(token, 0), value)
    history.append(row)
    weights = fresh
    ledger.append({"lesson": row["id"], "all_history_compatible": compatible,
                   "current_input_features": dict(weights)})
assert "beta" not in weights and "gamma" in weights

registry = (ROOT / SOURCES[-2]).read_text()
index = (ROOT / SOURCES[-1]).read_text()
work_ids = re.findall(r'<a id="(work-[^"]+)"', registry)
q_ids = re.findall(r'^\|\s*(Q\d{3})\s*\|', index, re.MULTILINE)
result = {
    "source_ref": "009bf226174441fa0fbf0336e74ad2f2b7174d92",
    "native_run": False,
    "source_sha256": {p: hashlib.sha256((ROOT/p).read_bytes()).hexdigest() for p in SOURCES},
    "loss_counterexample": loss,
    "active_revision_counterexample": {
        "kind": "source-equivalent compatibility-predicate simulation, not public runtime",
        "assumptions": "same nonempty FlowRef, same chosen FlowRef and route mode; different normalized target text A versus B",
        "ledger": ledger,
        "final_old_revision_blocks_new_paraphrase_accumulation": True,
        "required_behavior": "A -> B replaces A; B1 -> B2 retains input features of B1 and B2 under the active B revision only",
    },
    "empty_tokens": {
        "kind": "direct source predicate; not native caller test",
        "tokenize_punctuation": [], "jaccard_empty_empty_from_source": 1.0,
        "jaccard_one_empty_from_source": 0.0,
        "zero_overlap_candidate_filtered_by_store": False,
        "tie_breaker": "PairId ordinal; stable order is not semantic evidence",
    },
    "correction_provenance": {
        "kind": "source predicate truth table, not native delivery",
        "legitimate_source_echo": {"source": "Open the window", "observed_answer": "Open the window",
                                   "verified_correct_answer": "Открой окно", "current_admission": "rejects equality with source"},
        "fabricated_non_source": {"source": "Open the window", "observed_answer": "Open the window",
                                   "supplied_rejected": "Invented answer", "verified_correct_answer": "Открой окно",
                                   "current_text_checks": "nonempty provenance and inequality alone cannot establish this was the observed answer"},
    },
    "inventory": {
        "work_rows": len(work_ids), "unique_work_rows": len(set(work_ids)),
        "duplicate_work_rows": [k for k,v in collections.Counter(work_ids).items() if v>1],
        "experiment_rows": len(q_ids), "unique_experiment_rows": len(set(q_ids)),
        "first": min(q_ids), "last": max(q_ids),
        "scientific_validation_of_all_rows_claimed": False,
    },
}
(HERE / "core_findings_recomputed.json").write_text(json.dumps(result, ensure_ascii=False, indent=2)+"\n")
print(json.dumps(result, ensure_ascii=False, indent=2))
