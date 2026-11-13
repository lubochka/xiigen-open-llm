#!/usr/bin/env python3
"""Three mathematical/mechanical probes, NOT native or learning-quality tests.

Source repository: lubochka/llm_mvp_core
Pinned source: 44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76
Source links and limits: ../01-NEW-HYPOTHESES.md
No models, checkpoints, network, file mutation, external packages, or DPO data.
Run: python mechanical-probes.py > mechanical-results.json
Float32 casts emulate source arithmetic; native MathF bit parity is not asserted.
"""
from collections import Counter
import json
import math
import struct

def f32(x):
    return struct.unpack('<f', struct.pack('<f', x))[0]


def scalar_adamw_update(step, gradient, first_moment=0.0, second_moment=0.0,
                       weight=0.0, lr=1e-5, weight_decay=.01):
    b1, b2, eps = f32(.9), f32(.95), f32(1e-8)
    g = f32(gradient)
    m = f32(f32(b1*first_moment) + f32(f32(1-b1) * g))
    v = f32(f32(b2*second_moment) + f32(f32(f32(1-b2) * g) * g))
    bc1 = f32(1-f32(math.pow(b1, step)))
    bc2 = f32(1-f32(math.pow(b2, step)))
    mhat, vhat = f32(m/bc1), f32(v/bc2)
    update = f32(f32(f32(lr)*mhat) / f32(f32(math.sqrt(vhat))+eps))
    next_weight = f32(f32(f32(weight)*f32(1-f32(f32(lr)*f32(weight_decay))))-update)
    return {'step_used_for_bias_correction':step,
            'gradient':g,'first_moment':m,'second_moment':v,
            'adam_direction':f32(mhat/f32(f32(math.sqrt(vhat))+eps)),
            'adaptive_weight_decrement':update, 'weight_after':next_weight}


def first_moment_update(global_step, gradient=1.0, lr=1e-5):
    r=scalar_adamw_update(global_step,gradient,lr=lr)
    r['first_nonnull_gradient']=r.pop('gradient')
    r['weight_decrement_with_zero_initial_weight']=r.pop('adaptive_weight_decrement')
    return r


def dense_all_active_control():
    # Both parameters have a gradient carrier on every step. Explicit zero
    # gradients also count as observations, as in the current AdamW source.
    # This tests the stated age distinction in a formula shadow, not native code.
    global_states=[{'first_moment':0.0,'second_moment':0.0,'weight_after':0.0} for _ in range(2)]
    local_states=[dict(s) for s in global_states]
    local_ages=[0,0]
    differences=[]
    for step in range(1,51):
        gradients=(1.0, 0.0 if step%5==0 else (.5 if step%2==0 else -.25))
        for index,g in enumerate(gradients):
            local_ages[index]+=1
            for states,age in ((global_states,step),(local_states,local_ages[index])):
                old=states[index]
                states[index]=scalar_adamw_update(age,g,old['first_moment'],
                    old['second_moment'],old['weight_after'])
            for key in ('first_moment','second_moment','weight_after'):
                differences.append(abs(global_states[index][key]-local_states[index][key]))
    return {'description':'Two dense parameters, 50 steps, same nonnull gradient carriers; the second includes 10 explicit zero gradients.',
            'steps':50,'final_parameter_observation_ages':local_ages,
            'source_global_age_final_states':global_states,
            'proposed_observation_age_final_states':local_states,
            'all_values_finite':all(math.isfinite(v) for s in global_states+local_states for v in s.values()),
            'maximum_absolute_state_difference_over_all_steps':max(differences)}


def cosine_f32(a, b):
    # 06.VectorSearch/Distances.cs: separate FP32 products/additions and sqrt.
    dot = left = right = f32(0)
    for x, y in zip(a, b):
        dot = f32(dot + f32(x * y))
        left = f32(left + f32(x * x))
        right = f32(right + f32(y * y))
    denominator = f32(f32(math.sqrt(left)) * f32(math.sqrt(right)))
    return f32(0 if denominator < 1e-9 else dot / denominator)


def filter_probe(scores, interpretation):
    eligible_ids = [f"skill-{i}" for i in range(6)]
    values = scores if interpretation == "raw" else [(value + 1) / 2 for value in scores]
    confidence = values[0] / (sum(values) + 1e-9)
    fallback = confidence < 0.40
    return {"interpretation": interpretation, "scores": values,
            "score_sum": sum(values), "confidence": confidence,
            "fallback": fallback,
            "selected_ids": eligible_ids if fallback else eligible_ids[:5]}


def validated_mass_probe(scores):
    # Diagnostic contract arm: this consumer declares it accepts only finite,
    # nonnegative score mass; unsupported raw scores trigger existing fallback.
    # This is NOT a claim that cosine should be globally clipped or retrained.
    valid = (bool(scores) and all(math.isfinite(score) and score >= 0 for score in scores)
             and sum(scores) > 0)
    if valid:
        return {**filter_probe(scores, "raw"), "declared_mass_domain_supported": True}
    return {"interpretation": "declared_nonnegative_mass_contract",
            "declared_mass_domain_supported": False, "confidence": None,
            "fallback": True, "selected_ids": [f"skill-{i}" for i in range(6)]}


def confidence_evidence():
    query = [f32(1), f32(0)]
    requested = [0.6, 0.2, -0.2, -0.29, -0.3]
    vectors = [[f32(value), f32(math.sqrt(1 - value * value))]
               for value in requested]
    cosines = [cosine_f32(query, item) for item in vectors]
    raw = filter_probe(cosines, "raw")
    control = filter_probe(cosines, "cosine_to_nonnegative_mass")
    assert raw["confidence"] > 1
    assert raw["selected_ids"] != control["selected_ids"]
    assert not raw["fallback"] and control["fallback"]
    assert all(raw["scores"][i] >= raw["scores"][i + 1] for i in range(4))
    assert all(control["scores"][i] >= control["scores"][i + 1] for i in range(4))

    nonnegative = filter_probe([0.9, 0.04, 0.03, 0.02, 0.01], "raw")
    near_uniform = filter_probe([0.22, 0.21, 0.20, 0.19, 0.18], "raw")
    assert not nonnegative["fallback"] and near_uniform["fallback"]
    contract_arm = validated_mass_probe(cosines)
    assert contract_arm["fallback"] and contract_arm["selected_ids"] != raw["selected_ids"]
    negative_controls = []
    for scores in ([0.9, 0.04, 0.03, 0.02, 0.01], [0.22, 0.21, 0.20, 0.19, 0.18]):
        existing, contracted = filter_probe(scores, "raw"), validated_mass_probe(scores)
        assert existing["selected_ids"] == contracted["selected_ids"]
        negative_controls.append({"current": existing, "contract_arm": contracted})

    # A small perturbation of the LAST score preserves every ID/rank and the
    # winning score. Only cancellation changes sign in the denominator.
    tail_vector = [f32(-0.32), f32(math.sqrt(1 - 0.32 * 0.32))]
    perturbed_cosines = cosines[:4] + [cosine_f32(query, tail_vector)]
    perturbed = filter_probe(perturbed_cosines, "raw")
    assert perturbed["fallback"] and perturbed["confidence"] < 0
    assert all(perturbed_cosines[i] >= perturbed_cosines[i + 1] for i in range(4))

    # The known NK-01 producer: same term/ID indexed twice; N does not grow.
    # This is NOT a fresh discovery of the stale doc-frequency defect.
    idf_before = math.log((1 - 1 + 0.5) / (1 + 0.5) + 1)
    idf_after = math.log((1 - 2 + 0.5) / (2 + 0.5) + 1)
    assert idf_before > 0 and idf_after < 0
    return {"method": "Python source-equivalent arithmetic and branch control only",
            "query_vector": query, "candidate_vectors": vectors,
            "f32_cosine_scores": cosines,
            "all_returned_ids_eligible": True,
            "identical_candidate_ids_order_k_and_scope": True,
            "raw_arm": raw, "nonnegative_mapping_diagnostic_control": control,
            "declared_nonnegative_mass_contract_arm": contract_arm,
            "contract_arm_nonnegative_controls": negative_controls,
            "tail_perturbation_same_rank": {"last_vector": tail_vector,
                                           "current_formula": perturbed},
            "all_nonnegative_dominant_control": nonnegative,
            "all_nonnegative_uniform_control": near_uniform,
            "known_nk01_producer": {"N": 1, "df_before": 1, "df_after": 2,
                                   "idf_before": idf_before, "idf_after": idf_after},
            "mapping_is_proposed_numeric_control_not_validated_probability": True,
            "mapping_can_change_nonnegative_cases_and_is_not_the_contract_control": True,
            "native_run": False, "ordinary_product_injection_found": False,
            "learning_or_quality_gain_established": False}


def ordinal_distance(a, b, clamp):
    left, right = (min(a, 255), min(b, 255)) if clamp else (a, b)
    # All OTHER fields are exactly identical, and SiblingCount is unused.
    return 0.3 if left != right else 0.0


def vote(query_position, examples, clamp, k=5):
    nearest = sorted(examples,
                     key=lambda row: ordinal_distance(query_position, row[0], clamp))[:k]
    counts = Counter(row[1] for row in nearest)
    prediction = min(counts, key=lambda label: (-counts[label], label))
    return {"query_position": query_position,
            "encoded_position": min(query_position, 255) if clamp else query_position,
            "nearest": [{"position": position, "diagnostic_label": label,
                         "distance": ordinal_distance(query_position, position, clamp)}
                        for position, label in nearest],
            "prediction": prediction}


def ordinal_evidence():
    # Labels 10 and 20 are diagnostic integers; they are NOT parsed SyntaxKind.
    rows = [(255, 10)] * 3 + [(256, 20)] * 3
    current = [vote(pos, rows, True) for pos in (255, 256)]
    wide = [vote(pos, rows, False) for pos in (255, 256)]
    assert [r["prediction"] for r in current] == [10, 10]
    assert [r["prediction"] for r in wide] == [10, 20]
    control_rows = [(254, 10)] * 3 + [(255, 20)] * 3
    control_current = [vote(pos, control_rows, True) for pos in (254, 255)]
    control_wide = [vote(pos, control_rows, False) for pos in (254, 255)]
    assert [r["prediction"] for r in control_current] == [10, 20]
    assert [r["prediction"] for r in control_wide] == [10, 20]
    return {"method": "Python source-equivalent PositionInParent clamp, distance and vote",
            "diagnostic_labels_have_no_claimed_syntax_semantics": True,
            "only_changed_factor": "PositionInParent cap; all other features and rows fixed",
            "distance_boundaries": [{"positions": [a, b],
                                     "current": ordinal_distance(a, b, True),
                                     "wide_ordinal": ordinal_distance(a, b, False)}
                                    for a, b in [(254, 255), (255, 256), (256, 257)]],
            "examples": rows, "k": 5, "current": current, "wide_ordinal": wide,
            "below_boundary_control_current": control_current,
            "below_boundary_control_wide": control_wide,
            "roslyn_extraction_run": False, "native_knn_run": False,
            "real_corpus_saturation_frequency_measured": False,
            "learning_or_quality_gain_established": False}



def main():
    cold = first_moment_update(1)
    ages = []
    for elapsed in (0, 1, 9, 49, 99, 999):
        case = first_moment_update(elapsed + 1)
        case['prior_steps_with_parameter_grad_null'] = elapsed
        case['ratio_to_same_first_gradient_at_parameter_age_1'] = case['adam_direction'] / cold['adam_direction']
        ages.append(case)
    dense = dense_all_active_control()
    assert ages[0]['ratio_to_same_first_gradient_at_parameter_age_1'] == 1
    assert dense['all_values_finite']
    assert dense['maximum_absolute_state_difference_over_all_steps'] == 0
    result = {
        'scope': 'PYTHON_FORMULA_AND_BRANCH_PROBES_ONLY',
        'source_commit': '44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76',
        'native_dotnet_executed': False,
        'roslyn_executed': False,
        'dpo_training_executed': False,
        'external_dpo_generation_executed': False,
        'learning_quality_gain_measured': False,
        'NEW-AGE': {'cases': ages, 'dense_negative_control': dense},
        'NEW-SCORE': confidence_evidence(),
        'NEW-POSITION': ordinal_evidence()
    }
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
