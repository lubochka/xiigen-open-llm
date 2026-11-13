#!/usr/bin/env python3
"""Small deterministic mathematical probes of two retrieved source formulas.

This is NOT the C# implementation, a native model run, training, an evaluation
of translation quality, or a measurement of historical regression. It uses
Python binary64 arithmetic. Pinned C# files and a future native comparison are
specified in the adjacent audit. No repository, checkpoint, or index is edited.
"""
import json
import math

CORE_SHA = "a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6"
LANGUAGE_SHA = "f6032aff81f1f83de7c09cf9b9efe6b058dd12b9"


def cosine(left, right):
    return sum(a*b for a, b in zip(left, right)) / math.sqrt(
        sum(a*a for a in left) * sum(b*b for b in right))


def append_morph(vector, family_size, maximum_family_size):
    return [*vector, family_size / maximum_family_size]


def morphology_probe():
    # The candidate set, its base embeddings, and each own family size are fixed.
    query = (1.0, 0.0)
    candidates = {
        "same_family_candidate": (math.cos(math.radians(30)), math.sin(math.radians(30))),
        "other_family_candidate": (math.cos(math.radians(10)), math.sin(math.radians(10))),
    }
    family_sizes = {"query": 3, "same_family_candidate": 3, "other_family_candidate": 1}
    observations = []
    # max=6 means a third, unrelated vocabulary family grew. It is not a candidate.
    for query_max, index_max, label in [(3, 3, "old_complete_space"),
                                        (6, 6, "fully_rebuilt_new_space"),
                                        (6, 3, "new_query_old_index")]:
        q = append_morph(query, family_sizes["query"], query_max)
        scores = {name: cosine(q, append_morph(vector, family_sizes[name], index_max))
                  for name, vector in candidates.items()}
        observations.append({"case": label, "query_max_family": query_max,
                             "index_max_family": index_max, "cosines": scores,
                             "winner": max(scores, key=scores.get)})
    assert observations[0]["winner"] == "same_family_candidate"
    assert observations[1]["winner"] == "other_family_candidate"
    assert observations[2]["winner"] == "other_family_candidate"
    # A separate contrast holds vocabulary fixed and scales all base vectors.
    # Base-only cosine is invariant to this positive scale; appended cosine is not.
    scale_observations = []
    for scale in (1.0, 2.0):
        q = append_morph([scale*x for x in query], family_sizes["query"], 3)
        scores = {
            name: cosine(q, append_morph([scale*x for x in vector], family_sizes[name], 3))
            for name, vector in candidates.items()
        }
        scale_observations.append({"base_vector_scale": scale,
                                   "maximum_family_size": 3,
                                   "cosines": scores,
                                   "winner": max(scores, key=scores.get)})
    assert scale_observations[0]["winner"] == "same_family_candidate"
    assert scale_observations[1]["winner"] == "other_family_candidate"
    return {"finding": "LT-N05", "status": "FORMULA_RANK_FLIP_REPRODUCED",
            "base_embeddings": {"query": query, **candidates},
            "own_family_sizes_unchanged": family_sizes,
            "observations": observations,
            "separate_base_norm_contrast": scale_observations,
            "quality_claim": "No semantic gold, native stemmer, retrieval caller, or learning run evaluated."}


def schedule(step, warmup, total, maximum, minimum, clamp=False):
    if step < warmup:
        return maximum * step / max(1, warmup)
    progress = (step - warmup) / max(1, total - warmup)
    if clamp:
        progress = min(max(progress, 0.0), 1.0)
    return minimum + 0.5 * (1.0 + math.cos(math.pi * progress)) * (maximum - minimum)


def scheduler_probe():
    parameters = {"warmup": 0, "total": 4, "maximum": 0.0003, "minimum": 0.00003}
    rows = [{"supervised_step_before_call": step,
             "current_formula_lr": schedule(step, **parameters),
             "proposed_clamped_lr": schedule(step, **parameters, clamp=True)}
            for step in range(9)]
    assert all(abs(r["current_formula_lr"] - r["proposed_clamped_lr"]) < 1e-15
               for r in rows[:5])
    assert abs(rows[4]["current_formula_lr"] - parameters["minimum"]) < 1e-15
    assert abs(rows[8]["current_formula_lr"] - parameters["maximum"]) < 1e-15
    assert rows[8]["current_formula_lr"] > 9.99 * rows[8]["proposed_clamped_lr"]
    return {"finding": "LT-N06", "status": "FORMULA_REHEATING_REPRODUCED",
            "configuration": parameters, "observations": rows,
            "consumer_condition": "The same supervised Trainer must reach pre-call _supervisedStep > TotalSteps (sixth call for TotalSteps=4); RP01 DPO updates themselves do not use this scheduler.",
            "quality_claim": "No optimizer, neural weights, native Trainer, or task output evaluated."}


def main():
    result = {
        "schema": "language-translate-audit-source-formulas-v1",
        "evidence_kind": "PYTHON_BINARY64_SOURCE_FORMULA_MODEL",
        "core_sha": CORE_SHA, "language_translate_sha": LANGUAGE_SHA,
        "native_csharp_executions": 0, "optimizer_updates": 0,
        "external_model_calls": 0, "quality_evaluation_performed": False,
        "probes": [morphology_probe(), scheduler_probe()],
    }
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
