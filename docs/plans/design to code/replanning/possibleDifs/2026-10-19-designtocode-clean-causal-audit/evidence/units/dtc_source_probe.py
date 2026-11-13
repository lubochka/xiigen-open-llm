#!/usr/bin/env python3
"""Read-only arithmetic/source projection probe. Does NOT execute C# or train a model.

Usage: python dtc_source_probe.py [--repo /path/to/llm_mvp_core]
Inputs are exact GitHub source snapshots recorded by source-manifest.json.
All example lessons and seeded accumulator states below are artificial fixtures.
The probe does not mint admission, mutate repository data, or call a teacher.
"""
from pathlib import Path
import argparse
import hashlib
import json
import math
import subprocess

ROOT = Path(__file__).resolve().parent
args = argparse.ArgumentParser(description=__doc__)
args.add_argument("--repo", type=Path, help="Read pinned files with git show from this existing checkout; performs no git mutations.")
options = args.parse_args()
MACHINE = "44.DesignConversionDomain/DesignConversionMachine.cs"
UNITS = "44.DesignConversionDomain/DesignConversionTrainedUnits.cs"
manifest = json.loads((ROOT / "source-manifest.json").read_text())

def read_exact(kind, path):
    record = next(x for x in manifest if x["kind"] == kind and x["path"] == path)
    raw = (subprocess.run(["git", "-C", str(options.repo), "show", record["commit"] + ":" + path],
                          check=True, capture_output=True).stdout
           if options.repo else (ROOT / "src" / kind / path).read_bytes())
    blob = hashlib.sha1(b"blob " + str(len(raw)).encode() + b"\0" + raw).hexdigest()
    assert blob == record["sha"], (kind, path, "source drift")
    return raw.decode("utf-8")

current = read_exact("current", MACHINE)
clean = read_exact("clean", MACHINE)
units = read_exact("current", UNITS)

def section(text, begin, end):
    start = text.index(begin)
    return text[start:text.index(end, start + len(begin))]

def method_equal(begin, end):
    return section(current, begin, end) == section(clean, begin, end)

delta_method = section(current,
    "    private static IReadOnlyList<LearnedWeightDelta> BuildLearnedWeightDeltas(",
    "    private static void AddCorrectionCategoryDeltas(")
assert "preferenceMargin = chosenScore - rejectedScore;" in delta_method
assert '"internal_vector:mean", vector.Average()' in delta_method
assert '"internal_vector:dimension", vector.Count' in delta_method
assert '$"source_kind:{sourceKind}", 1.0' in delta_method
assert 'rewardMargin = margin;' in delta_method
category_method = section(current,
    "    private static void AddCorrectionCategoryDeltas(",
    "    private static IReadOnlyList<string> ReadCorrectionRuleIds(")
assert 'DesignConversionRuntimeScaffoldImproverUnit.UnitId' in category_method
assert '"chosen_correction_category_preference"' in category_method
assert '"rejected_correction_category_penalty"' in category_method
assert '-1.0' in category_method
runtime_features = section(current,
    "    private static IReadOnlyDictionary<string, double> BuildCriticRuntimeFeatures(",
    "    private static IReadOnlyDictionary<string, double> BuildImproverRuntimeFeatures(")
assert 'internal_vector' not in runtime_features
assert '$"source_kind:{sourceKind}"] = 1.0;' in runtime_features
heldout_features = section(current,
    "    private static IReadOnlyDictionary<string, double> BuildHeldOutEvaluationFeatures(",
    "    private static void AddHeldOutFeature(")
assert '"internal_vector:mean"' in heldout_features
assert '"internal_vector:dimension"' in heldout_features
rule_plan = section(units,
    "    private static LocalImprovementPlan BuildLocalImprovementPlan(",
    "    private static LocalImprovementRule[] BuildLocalImprovementRules(")
assert 'item.Preference.Weight > 0' in rule_plan
assert 'if (ranked.Length == 0)' in rule_plan
assert 'SelectedRules: availableRules' in rule_plan
assert 'Weight: Math.Round(_weightedSum / ObservationCount, 6)' in current
assert '_weightedSum = checkpoint.Weight * checkpoint.ObservationCount' in current

def critic_projection(prompt, chosen, rejected):
    # Restricted to valid finite JSON numeric fields and canonical figma critic.
    assert prompt["unit_id"] == "design_conversion_runtime_scaffold_critic"
    vector = prompt["internal_vector"]
    deltas = {
        "internal_vector:mean": round(sum(vector) / len(vector), 6),
        "internal_vector:dimension": len(vector),
        "source_kind:figma": 1.0,
    }
    return {"reward_margin": round(chosen["visual_score"] - rejected["visual_score"], 6),
            "feature_contributions": deltas}

prompt = {"unit_id": "design_conversion_runtime_scaffold_critic",
          "source_kind": "figma", "internal_vector": [0.0, 1.0, 0.0, 1.0]}
chosen = {"visual_score": 0.95, "html": "<button>Save</button>", "css": "button{visibility:visible}"}
rejected = {"visual_score": 0.20, "html": "<button>Save</button>", "css": "button{visibility:hidden}"}
p_good = critic_projection(prompt, chosen, rejected)
p_swapped = critic_projection(prompt, rejected, chosen)
assert p_good["reward_margin"] == -p_swapped["reward_margin"]
assert p_good["feature_contributions"] == p_swapped["feature_contributions"]

def adjustment(weight_map, features):
    values = [round(round(v, 6) * round(weight_map[k], 6), 6)
              for k, v in features.items() if k in weight_map]
    return round(math.tanh(sum(values) / 2.0) * 0.5, 6) if values else 0.0

weights = p_good["feature_contributions"]
eval_features = dict(weights)
live_features = {"source_kind:figma": 1.0}
eval_score = round(min(1.0, max(0.0, 0.5 + adjustment(weights, eval_features))), 6)
live_score = round(min(1.0, max(0.0, 0.5 + adjustment(weights, live_features))), 6)
assert eval_score != live_score

def choose_local_rules(available, learned):
    # Same branch decision for one valid preference per RuleId, equal observation counts.
    positive = sorted((r for r in available if learned.get(r, 0.0) > 0),
                      key=lambda r: (-learned[r], r))
    return {"selected_rules": positive[:1] if positive else sorted(available),
            "learned_selection_applied": bool(positive)}

learned_rules = {"contrast": 1.0, "spacing": -1.0}
both = choose_local_rules(["contrast", "spacing"], learned_rules)
spacing_only = choose_local_rules(["spacing"], learned_rules)
no_memory = choose_local_rules(["spacing"], {})
assert both["selected_rules"] == ["contrast"]
assert spacing_only["selected_rules"] == ["spacing"]
assert spacing_only == no_memory

def continue_accumulator(initial_weight, initial_count, contributions, cold, preserve_sum=False):
    weighted_sum = initial_weight * initial_count
    count = initial_count
    for contribution in contributions:
        weighted_sum += round(contribution, 6)
        count += 1
        if cold and not preserve_sum:
            weighted_sum = round(weighted_sum / count, 6) * count
    return {"weight": round(weighted_sum / count, 6), "observation_count": count,
            "internal_sum": weighted_sum}

small_inputs = [0.250002] * 2
small = {label: continue_accumulator(0.25, 4, small_inputs, cold, preserve)
         for label, cold, preserve in [("warm", False, False),
                                      ("current_cold_each_lesson", True, False),
                                      ("proposed_sum_preserving_cold", True, True)]}
assert small["warm"]["weight"] == 0.250001
assert small["current_cold_each_lesson"]["weight"] == 0.25
assert small["proposed_sum_preserving_cold"] == small["warm"]

# This is an artificially seeded high-count checkpoint, not two million actual lessons.
stress_inputs = [1.0] * 1000
stress = {label: continue_accumulator(0.0, 2_000_000, stress_inputs, cold, preserve)
          for label, cold, preserve in [("warm", False, False),
                                       ("current_cold_each_lesson", True, False),
                                       ("proposed_sum_preserving_cold", True, True)]}
assert stress["warm"]["weight"] == 0.0005
assert stress["current_cold_each_lesson"]["weight"] == 0.0

result = {
    "status": "SOURCE_BOUND_ARITHMETIC_ONLY_NOT_NATIVE_NOT_MODEL_QUALITY",
    "all_fixtures_artificial": True,
    "teacher_called": False,
    "optimizer_run": False,
    "admission_minted_or_bypassed": False,
    "source_commits": {"current": "358af351e22a92a3f7938d1f9221d5aeb7ddb053",
                       "DesignToCode-clean": "e346b88a3cc1fcce48aefa626bfc7b00509eefd6"},
    "source_equivalence": {
        "critic_delta_whole_method_identical": method_equal(
            "    private static IReadOnlyList<LearnedWeightDelta> BuildLearnedWeightDeltas(",
            "    private static void AddCorrectionCategoryDeltas("),
        "heldout_feature_whole_method_identical": method_equal(
            "    private static IReadOnlyDictionary<string, double> BuildHeldOutEvaluationFeatures(",
            "    private static void AddHeldOutFeature("),
        "accumulator_class_identical": method_equal(
            "    private sealed class LearnedWeightAccumulator",
            "    private sealed class LearnedWeightCriticUnit("),
        "trained_units_both_tree_blob_sha": "3ef9f245d70df2c6dee673f7b9af5a15b3584701",
        "common_critic_projection_present_both": all(term in clean and term in current for term in [
            '"internal_vector:mean", vector.Average()',
            '"internal_vector:dimension", vector.Count',
            '$"source_kind:{sourceKind}", 1.0',
            'rewardMargin = margin;']),
        "difference_caveat": "Clean also used identity/pair-family features; current removes them and accepts distinct JSON sides without visual_score. Those whole methods are not byte-identical.",
    },
    "critic_polarity": {"good_pair": p_good, "swapped_pair": p_swapped,
                        "same_feature_contributions": True,
                        "limit": "Not a legal reversed production lesson; diagnostic of the projection only."},
    "critic_eval_vs_live": {"learned_weights": weights, "eval_features": eval_features,
                            "runtime_features": live_features, "feature_fit_score": eval_score,
                            "runtime_score_given_artificial_baseline_0_5": live_score,
                            "runtime_passed_remains_artificial_baseline_false": True,
                            "limit": "The existing evaluator explicitly labels feature-fit scope, not visual quality."},
    "negative_rule_fallback": {"learned_weights": learned_rules,
                               "both_rules_available": both,
                               "only_penalized_spacing_available": spacing_only,
                               "no_memory_control": no_memory,
                               "source_css_spacing_rule": "gap:12px;padding:16px;",
                               "limit": "No DOM was rendered; applying this rule is not itself proof of harm."},
    "checkpoint_roundtrip": {"small_finite_fixture": {"initial_weight": 0.25, "initial_count": 4, "contributions": small_inputs},
                             "small_finite_case": small,
                             "artificial_high_count_stress_case": stress,
                             "limit": "Python double arithmetic transcription; C# rounding and native caller need confirmation."},
}
(ROOT / "dtc-source-probe-result.json").write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n")
print(json.dumps({"status": result["status"], "source_equivalence": result["source_equivalence"],
                  "critic_margins": [p_good["reward_margin"], p_swapped["reward_margin"]],
                  "critic_same_contributions": True,
                  "eval_live_scores": [eval_score, live_score],
                  "negative_spacing_still_selected": spacing_only["selected_rules"],
                  "small_warm_cold_weights": [small["warm"]["weight"], small["current_cold_each_lesson"]["weight"]],
                  "stress_warm_cold_weights": [stress["warm"]["weight"], stress["current_cold_each_lesson"]["weight"]]},
                 ensure_ascii=False, indent=2))
