#!/usr/bin/env python3
"""Recount pinned saved experimental observations; no C#/PowerShell/model invocation.

python codex-experiment-recompute.py --core /path/to/core --output /new/result.json
Uses codex-experiment-inputs.json beside this file. Refuses existing output.
"""
import argparse
import hashlib
import json
import math
import re
import struct
from collections import Counter, defaultdict
from pathlib import Path

def require(condition, message):
    if not condition:
        raise ValueError(message)

def fingerprint(path):
    data = path.read_bytes()
    return {"bytes": len(data), "sha256": hashlib.sha256(data).hexdigest(),
            "git_blob_sha1": hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()}

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--core", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    require(not args.output.exists(), "Output already exists; preserve historical evidence")
    root = args.core.resolve()
    manifest = json.loads(Path(__file__).with_name("codex-experiment-inputs.json").read_text())
    before = {}
    for name, metadata in manifest["inputs"].items():
        actual = fingerprint(root / name)
        require(actual["git_blob_sha1"] == metadata["git_blob_sha1"], "Input blob mismatch: " + name)
        before[name] = actual

    def read(name):
        require(name in before, "Unpinned input: " + name)
        return (root / name).read_text(encoding="utf-8-sig")

    def j(exp, attempt, name):
        return json.loads(read(f"experiments-v2/RP01-MICRO/{exp}/results/attempt-{attempt:04d}/{name}"))

    checks = []
    def checked(tag, condition):
        require(condition, "Recomputation disagreement: " + tag)
        checks.append(tag)

    def ceiling(rows, input_key="Input", gold_key="Gold"):
        histogram = Counter(row[gold_key] for row in rows)
        groups = defaultdict(Counter)
        for row in rows:
            groups[row[input_key]][row[gold_key]] += 1
        return {"rows": len(rows), "constant": max(histogram.values(), default=0),
                "input_function": sum(max(group.values()) for group in groups.values())}

    # P48: derive correctness from actual answer and target, independently of saved flags.
    arms48 = j("P4801", 3, "arms.json")
    keys48 = ["плечо_равных_весов", "контроль_механизма"]
    p48 = {"attempt": 3, "arms": {}}
    for name in keys48:
        arm = arms48[name]
        rows = arm["Panel"]
        require(len({r["RowId"] for r in rows}) == len(rows), "Duplicate P48 identity")
        for r in rows:
            checked("P48:" + name + ":" + r["RowId"], r["Correct"] == (r["Answer"] == r["Gold"]))
        calculated = sum(r["Answer"] == r["Gold"] for r in rows)
        checked("P48:" + name + ":total", calculated == arm["PanelCorrect"])
        exam = [r for r in rows if r["Kind"] == "EXAM"]
        train = [r for r in rows if r["Kind"] == "TRAIN_CONTROL"]
        train_pairs = {(r["Input"], r["Gold"]) for r in train}
        p48["arms"][name] = {"correct": calculated, "rows": len(rows),
            "exam_correct": sum(r["Answer"] == r["Gold"] for r in exam),
            "exam_rows": len(exam), "train_correct": sum(r["Answer"] == r["Gold"] for r in train),
            "train_rows": len(train), "ceilings": ceiling(rows),
            "exam_exact_pair_overlap": sum((r["Input"], r["Gold"]) in train_pairs for r in exam),
            "updates_reported": arm["UpdatesApplied"]}
    left = {r["RowId"]: r for r in arms48[keys48[0]]["Panel"]}
    right = {r["RowId"]: r for r in arms48[keys48[1]]["Panel"]}
    checked("P48:panel_identity", left.keys() == right.keys())
    p48["row_answers_different_between_arms"] = sum(left[k]["Answer"] != right[k]["Answer"] for k in left)
    p48["assigned_weighted_arm_executed_in_attempt3"] = j("P4801", 3, "status.json")["assigned_arm_b_executed_here"]
    p48["current_main_source_cost_derivation"] = {
        "arm_updates": sum(arms48[k]["UpdatesApplied"] for k in keys48),
        "extra_gradient_oracle_train_step": 1, "total_train_steps": 4801,
        "basis": "P4801WeightingCurriculumCeilingTests.cs:473-474; two RunArm calls:500-509",
        "native_observed_here": False}

    # P49: join raw observations by row identity, then recompute subsets and overlap.
    arms49 = j("P4901", 3, "arms.json")
    panel = arms49["состав"]["панель"]
    gold = {r["RowId"]: r for r in panel}
    checked("P49:unique_panel_ids", len(gold) == len(panel))
    measures = arms49["показания"]
    p49 = {"attempt": 3, "measurements": {}, "overlap_by_arm": {}}
    for name, rows in measures.items():
        checked("P49:" + name + ":same_ids", set(r["RowId"] for r in rows) == gold.keys())
        for r in rows:
            checked("P49:" + name + ":" + r["RowId"], r["Correct"] == (r["Answer"] == gold[r["RowId"]]["Gold"]))
        groups = {"all": panel, "EXAM": [r for r in panel if r["Kind"] == "EXAM"],
                  "WARM_SET": [r for r in panel if r["Group"] == "WARM_SET"],
                  "NEW_SET": [r for r in panel if r["Group"] == "NEW_SET"],
                  "OLD_ONLY": [r for r in panel if r["Overlap"] == "OLD_ONLY"],
                  "NEW_ONLY": [r for r in panel if r["Overlap"] == "NEW_ONLY"],
                  "NEW_PROJECT_B": [r for r in panel if r["ProjectId"] == "B"]}
        answers = {r["RowId"]: r["Answer"] for r in rows}
        p49["measurements"][name] = {
            group: {**ceiling(subset), "correct": sum(answers[r["RowId"]] == r["Gold"] for r in subset)}
            for group, subset in groups.items()}
    warm = arms49["состав"]["пары_подготовки"]
    new = arms49["состав"]["пары_нового_обучения"]
    exam = [r for r in panel if r["Kind"] == "EXAM"]
    for name, trained in [("cold_new_only", new), ("warm_plus_new", warm + new)]:
        seen_inputs = {r["Current"] for r in trained}
        seen_pairs = {(r["Current"], r["Next"]) for r in trained}
        rows = [{"id": r["RowId"], "input": r["Input"], "gold": r["Gold"],
            "category": "exact_pair_seen" if (r["Input"], r["Gold"]) in seen_pairs else
                        "input_seen_target_new" if r["Input"] in seen_inputs else "input_unseen"} for r in exam]
        p49["overlap_by_arm"][name] = {"rows": rows,
            "counts": dict(Counter(r["category"] for r in rows)),
            "input_overlap": sum(r["input"] in seen_inputs for r in rows),
            "pair_overlap": sum((r["input"], r["gold"]) in seen_pairs for r in rows)}
    p49["reported_training_budget"] = j("P4901", 3, "metrics.json")["бюджет"]

    # P50: compare consumer journals with independently retained gold order.
    cold50, arms50, after50, restored50 = (j("P5001", 2, f) for f in
        ("before.json", "arms.json", "after.json", "restore.json"))
    gold50 = {r["case_id"]: r["gold_steps"] for r in cold50["consumer_rows"]}
    for r in cold50["consumer_rows"]:
        checked("P50:cold:" + r["case_id"], r["correct"] == (r["journal"] == r["gold_steps"]))
    p50 = {"attempt": 2, "cold_correct": sum(r["journal"] == r["gold_steps"] for r in cold50["consumer_rows"]),
           "exam_rows": len(gold50), "constant_journal_ceiling": max(Counter(tuple(x) for x in gold50.values()).values()),
           "arms": {}}
    for key in ("arm_a_outcomes", "arm_b_outcomes"):
        rows = arms50[key]
        checked("P50:" + key + ":same_ids", set(r["CaseId"] for r in rows) == gold50.keys())
        for r in rows:
            expected = r["Journal"] == gold50[r["CaseId"]] and not r["HasCycle"] and r["MissingEdges"] == 0 and r["ExtraEdges"] == 0
            checked("P50:" + key + ":" + r["CaseId"], r["Correct"] == expected)
        p50["arms"][key] = {"correct": sum(r["Correct"] for r in rows), "rows": len(rows)}
    for r in after50["unit_rows"]:
        checked("P50:after:" + r["case_id"], r["correct"] == (r["answer_route"] == r["gold_route"]))
    for r in restored50["rows"]:
        checked("P50:saved_restore:" + r["case_id"], r["correct"] == (r["answer_route"] == r["gold_route"]))
    p50["saved_restore"] = {"rows": len(restored50["rows"]),
        "correct": sum(r["answer_route"] == r["gold_route"] for r in restored50["rows"]),
        "train_calls_as_recorded": restored50["train_calls_in_this_process"],
        "fresh_load_here": False}

    # P52: two independent axes, calculate constant ceilings from gold distribution.
    before52, arms52, metrics52 = (j("P5201", 2, f) for f in ("before.json", "arms.json", "metrics.json"))
    p52 = {"attempt": 2, "arms": {}}
    for name, rows in [("cold", before52["строки"]),
          *[(k, arms52[k]["Rows"]) for k in ("плечо_до_исправления", "плечо_после_исправления")]]:
        for index, r in enumerate(rows):
            checked("P52:" + name + ":answer:" + str(index), r["AnswerCorrect"] == (r["AnswerText"].lower() == r["GoldText"].lower()))
            checked("P52:" + name + ":tense:" + str(index), r["TenseCorrect"] == (r["TrainedTense"] == r["GoldTense"]))
        p52["arms"][name] = {"answers": sum(r["AnswerCorrect"] for r in rows),
            "tense_addresses": sum(r["TenseCorrect"] for r in rows), "rows": len(rows)}
    p52["answer_constant_ceiling"] = max(Counter(r["GoldText"].lower() for r in before52["строки"]).values())
    p52["tense_constant_ceiling"] = max(Counter(r["GoldTense"] for r in before52["строки"]).values())
    p52["saved_mislabeled_ceiling"] = metrics52["предел_постоянного_ответа"]["верных_ответов"]
    p52["current_writer_still_uses_cold"] = 'верных_ответов = cold.Count(row => row.AnswerCorrect)' in read(
        "Tests/ExperimentFixes/P5201RejectedAnnotationAddressArmsTests.cs").split("предел_постоянного_ответа = new", 1)[1].split("граница_вывода", 1)[0]

    # P58: compare answers and gold strings, keeping frozen evidence separate from current writer.
    before58, arms58, training58, metrics58 = (j("P5801", 2, f) for f in
        ("before.json", "arms.json", "train-control.json", "metrics.json"))
    groups58 = {"cold": before58["холодная_панель"]["строки"],
        "after_setup": before58["после_установочных_уроков"]["строки"],
        "train_cold": training58["до_обучения"]["строки"],
        "train_after": training58["после_обучения"]["строки"]}
    arm_names = ["плечо_A_строка_корпуса", "плечо_B_фактический_ошибочный_ответ", "контроль_измерителя_фразовая_пометка"]
    groups58.update({name: arms58[name]["PanelAfter"] for name in arm_names})
    p58 = {"attempt": 2, "panels": {}}
    for name, rows in groups58.items():
        for r in rows:
            checked("P58:" + name + ":" + r["Id"], r["Correct"] == (r["Answer"].lower() == r["Gold"].lower()))
        p58["panels"][name] = {"correct": sum(r["Correct"] for r in rows), "rows": len(rows)}
    p58["constant_answer_ceiling"] = max(Counter(r["Gold"].lower() for r in groups58["cold"]).values())
    a, b, control = [arms58[name] for name in arm_names]
    p58["counterexample_usable"] = [a["LessonChosenText"] != a["LessonRejectedText"], b["LessonChosenText"] != b["LessonRejectedText"]]
    p58["equal_start_hashes"] = len({r["StateAfterSetupHash"] for r in (a,b,control)}) == 1
    p58["a_b_state_hash_equal_after_correction"] = a["StateAfterCorrectiveHash"] == b["StateAfterCorrectiveHash"]
    p58["control_state_hash_distinct"] = control["StateAfterCorrectiveHash"] != a["StateAfterCorrectiveHash"]
    p58["a_b_answers_equal"] = [r["Answer"] for r in a["PanelAfter"]] == [r["Answer"] for r in b["PanelAfter"]]
    p58["historical_ceiling_field"] = metrics58["контроль_постоянного_ответа"]
    p58["current_writer_has_separate_ceiling"] = "верных = ceiling.Correct" in read("Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs")
    p58["current_main_source_cost_derivation"] = {
        "setup_reads": 8+8, "train_control_reads": 8+8, "arm_reads": 3*8,
        "total_reads": 56, "main_setup_lessons": 24, "three_arm_setup_lessons": 3*24,
        "three_arm_corrective_lessons": 3*3, "total_train_calls": 105, "native_observed_here": False}

    # WC01: source-equivalent replay of fixed 18-epoch perceptron; no model code called.
    source = read("42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs")
    fixture = json.loads(read("experiments-v2/RP01-MICRO/WC01/fixture.json"))
    snapshot = json.loads(read("experiments-v2/RP01-MICRO/WC01/results/attempt-001/A/after-snapshot.json"))
    labels = re.findall(r"ProjectWikiRequestRouteKind\.(\w+)",
        source.split("private static readonly ProjectWikiRequestRouteKind[] Labels =", 1)[1].split("];", 1)[0])
    checked("WC01:labels", labels == snapshot["labels"])
    require("private const int EpochCount = 18;" in source and "private const double LearningRate = 0.14;" in source, "WC01 algorithm version changed")
    def features(text):
        require(text.isascii(), "WC01 Python lexical equivalence bounded to ASCII")
        tokens = re.findall(r"[A-Za-z0-9_-]+", text.lower())
        counts = {}
        for token in tokens:
            counts[token] = counts.get(token, 0.) + 1.
        for left, right in zip(tokens, tokens[1:]):
            key = left + "_" + right
            counts[key] = counts.get(key, 0.) + 1.
        norm = max(1., math.sqrt(sum(v*v for v in counts.values())))
        return {k: v/norm for k,v in counts.items()}
    prepared = [(row, features(row["prompt"])) for row in fixture["train"]]
    weights, biases, epochs = {}, [0.]*len(labels), []
    for epoch in range(18):
        updates = []
        for row, feat in prepared:
            scores = list(biases)
            for key, value in feat.items():
                if key in weights:
                    for i in range(len(labels)):
                        scores[i] += weights[key][i] * value
            predicted = max(range(len(labels)), key=scores.__getitem__)
            wanted = labels.index(row["expectedRouteKind"])
            if wanted != predicted:
                updates.append(row["id"])
                for key, value in feat.items():
                    w = weights.setdefault(key, [0.]*len(labels))
                    w[wanted] += .14 * value
                    w[predicted] -= .14 * value
                biases[wanted] += .14
                biases[predicted] -= .14
        epochs.append(updates)
    checked("WC01:feature_keys", weights.keys() == snapshot["tokenWeights"].keys())
    pairs = list(zip(biases, snapshot["biases"]))
    for key, vector in weights.items():
        pairs.extend(zip(vector, snapshot["tokenWeights"][key]))
    checked("WC01:all_parameter_bits", all(struct.pack(">d",a) == struct.pack(">d",b) for a,b in pairs))
    wiki = {"updates_by_epoch":[len(x) for x in epochs], "update_rows_by_epoch":epochs,
        "parameters_compared":len(pairs), "bit_exact":True,
        "active_epochs":sum(bool(x) for x in epochs), "empty_epochs":sum(not x for x in epochs),
        "presentations":len(prepared)*18, "native_training_or_timing":False}

    probability = math.exp(-20)/(1+math.exp(-20))
    loss = {"logits":[0,-20], "gold_index":1, "probability":probability,
        "clipped_loss":-math.log(max(1e-7,probability)),
        "stable_loss":math.log1p(math.exp(-20))+20,
        "clipped_loss_derivative_at_this_point":0.0, "implemented_target_gradient":probability-1,
        "native_execution":False}

    checked("all_pinned_inputs_unchanged", all(fingerprint(root/name) == value for name,value in before.items()))
    result = {"source_ref":manifest["source_ref"], "reviewer":"/root/evidence_review",
        "native_runs":0, "production_model_calls":0,
        "verification_kind":"fresh Python recount of historical rows and source-equivalent bounded numerical replay",
        "input_fingerprints":before, "explicit_consistency_checks":len(checks),
        "p48":p48, "p49":p49, "p50":p50, "p52":p52, "p58":p58, "wiki_wc01":wiki, "loss_scalar":loss,
        "p54":"current product wiring, new helper and native receipts are reviewed separately by parent",
        "inputs_unchanged":True,
        "limits":["saved Correct flags were verified from answers/targets, not used as the sole oracle",
                  "saved state hashes were compared as recorded; P58 state bytes were not regenerated",
                  "P50 restore rows were recounted; no fresh restore or saved-index inference performed",
                  "WC01 method reproduces prior audit replay; not a second independent algorithm or .NET execution",
                  "not all 131 registered experiments or all 124 transfer rows independently rerun"]}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open("x", encoding="utf-8") as f:
        json.dump(result,f,ensure_ascii=False,indent=2)
        f.write("\n")
    print(json.dumps({"output":str(args.output), "verified_inputs":len(before),
        "explicit_consistency_checks":len(checks), "p49_cold":p49["measurements"]["холодное_плечо"]["all"],
        "p49_warm":p49["measurements"]["тёплое_плечо"]["all"], "wiki":wiki["updates_by_epoch"]},ensure_ascii=False))

if __name__ == "__main__":
    main()

