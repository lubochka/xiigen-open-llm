#!/usr/bin/env python3
"""Recount FIX-P56 saved evidence. Does not run .NET or train/load any model.

python codex-product-evidence-recheck.py --core /pinned/core --output /new/result.json
"""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import xml.etree.ElementTree as ET


def require(condition, message):
    if not condition:
        raise ValueError(message)


def blob(data):
    return hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--core", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    require(not args.output.exists(), "Existing output is immutable")
    manifest = json.loads(Path(__file__).with_name("codex-product-inputs.json").read_text())
    before = {}
    for name, metadata in manifest["inputs"].items():
        data = (args.core / name).read_bytes()
        require(blob(data) == metadata["git_blob_sha1"], "Input changed: " + name)
        before[name] = data

    base = "artifacts/fbe/FIX-P56/attempt-0001/"
    def data(name):
        require(base + name in before, "Unpinned input: " + name)
        return before[base + name]

    prefix = "LlmFromScratch.Tests."
    ordinary = prefix + "TranslationRequiredWholeTermOrdinaryPathTests."
    direct = prefix + "TranslationRequiredWholeTermLearningTests."
    ordinary_names = {ordinary + "FixturePrecondition_NativeSignaturesAreUsableAndDistinct",
                      ordinary + "OrdinaryConsumerPath_RequiredTermIsCheckedAsWholeWords"}
    all_names = ordinary_names | {direct + "BaselineArm_OutcomesThatMustHoldBeforeAndAfterFix",
        direct + "CandidateArm_LearnedTermMustMatchWholeWords_AndSurvivesFreshLoad",
        direct + "OldSubstringLogic_FailsTheSameOracle_ProvingTheNewAssertionsAreMutationSensitive"}
    expected_failed = {ordinary + "OrdinaryConsumerPath_RequiredTermIsCheckedAsWholeWords",
                       direct + "CandidateArm_LearnedTermMustMatchWholeWords_AndSurvivesFreshLoad"}
    ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
    trx = []
    for filename, expected_names, failures in [
        ("trx-run-01-before.trx", ordinary_names, set()),
        ("trx-run-02-mutant.trx", all_names, expected_failed),
        ("trx-run-03-after.trx", all_names, set()),
        ("trx-run-04-after-repair.trx", all_names, set())]:
        root = ET.fromstring(data(filename))
        rows = root.findall(".//t:UnitTestResult", ns)
        require(len(rows) == len(expected_names), "Result row count: " + filename)
        require({r.attrib["testName"] for r in rows} == expected_names, "Result names: " + filename)
        require(len({r.attrib["testId"] for r in rows}) == len(rows), "Duplicate testId: " + filename)
        definitions = {d.attrib["id"]: d.attrib["name"] for d in root.findall(".//t:UnitTest", ns)}
        require(all(definitions.get(r.attrib["testId"]) == r.attrib["testName"] for r in rows),
                "Definitions do not match result identities: " + filename)
        require({r.attrib["testName"] for r in rows if r.attrib["outcome"] == "Failed"} == failures,
                "Wrong failed tests: " + filename)
        require(all(r.attrib["outcome"] in ("Passed", "Failed") for r in rows), "Incomplete run: " + filename)
        counts = Counter(r.attrib["outcome"] for r in rows)
        counters = root.find(".//t:Counters", ns).attrib
        for key, count in [("total", len(rows)), ("executed", len(rows)),
                           ("passed", counts["Passed"]), ("failed", counts["Failed"])]:
            require(int(counters[key]) == count, "Counter differs from rows: " + filename + ":" + key)
        trx.append({"file": filename, "total": len(rows), "passed": counts["Passed"],
                    "failed": counts["Failed"], "failed_names": sorted(failures)})

    stages = ["до обучения", "после обучения", "после загрузки"]
    cases = {"S1": ("ReadyFlag готов", "general", "ReadyFlag ready", True),
             "S2": ("ReadyFlag готов", "code", "ReadyFlag already", False),
             "S3": ("null готов", "general", "null is ready", True),
             "S4": ("null готов", "code", "null is already", False)}
    receipts = []
    for folder in ["", "runs/20261111T214753341Z-pid10396/", "runs/20261111T215454183Z-pid4764/"]:
        receipt = json.loads(data(folder + "ordinary-path-receipt.json"))
        require(receipt["УчебныеСтроки"] == {"Подано": 6, "ФактическиОбучено": 6}, "Training row count")
        snapshot_bytes = len(data(folder + "checkpoint-after-training.json"))
        require(receipt["ФайлСостояния"]["РазмерБайт"] == snapshot_bytes, "Snapshot size differs")
        rows = receipt["Исходы"]
        keys = [(r["Стадия"], r["Код"]) for r in rows]
        require(len(keys) == 12 and set(keys) == {(s, c) for s in stages for c in cases}, "Receipt row inventory")
        outcomes = {}
        for row in rows:
            stage, case = row["Стадия"], row["Код"]
            source, topic, candidate, accepted = cases[case]
            expected_adapter = "executor-ru_en-exact-source" if accepted and stage != stages[0] else "bootstrap-ru_en"
            expected_text = candidate if accepted and stage != stages[0] else "[RU_EN:BOOTSTRAP] " + source
            require((row["ИсходныйТекст"], row["ТемаЗапроса"], row["ВыученныйОтвет"]) == (source, topic, candidate), "Fixture identity changed")
            require(row["ФактическоеОпознаниеПути"] == expected_adapter, "Wrong adapter " + stage + case)
            require(row["ФактическийТекстОтвета"] == expected_text, "Wrong output " + stage + case)
            require(row["ОтветИзКартыФраз"] is False and not expected_adapter.endswith("-phrase-map"), "Phrase branch leaked")
            outcomes[stage + ":" + case] = {"adapter": expected_adapter, "text": expected_text}
            if folder and stage == stages[0]:
                require(row["ОжидалсяИсход"] == "ответ заготовки-основы: выученной памяти ещё нет", "Cold expected label still stale")
        require(all(outcomes[stages[1] + ":" + c] == outcomes[stages[2] + ":" + c] for c in cases), "Saved restore output differs")
        receipts.append({"folder": folder or "frozen-root", "rows": len(rows), "training_rows_recorded": 6,
                         "snapshot_bytes": snapshot_bytes, "observed_rows_match_independent_oracle": True,
                         "cold_expected_label": "known stale frozen field" if not folder else "corrected",
                         "outcomes": outcomes})

    hashes = {label: data("hash-product-" + label + ".txt").decode().split()[0]
              for label in ["before", "mutant", "restored"]}
    require(hashes["before"] == hashes["restored"] != hashes["mutant"], "Recorded restoration hashes")
    require(all(len(v) == 64 and all(c in "0123456789abcdef" for c in v) for v in hashes.values()), "Invalid recorded hash")
    # Exact old expression on the ordinary-path counterexample literals, not a .NET run.
    old_contains = {"S2": "ready" in "ReadyFlag already".lower(),
                    "S4": "is ready" in "null is already".lower()}
    require(old_contains == {"S2": True, "S4": False}, "Old substring semantics")
    for name, original in before.items():
        require((args.core / name).read_bytes() == original, "Input mutated: " + name)
    result = {"source_ref": manifest["source_ref"], "kind": "saved-evidence-recount-not-native-test-run",
              "verified_input_blobs": len(before), "trx": trx, "receipts": receipts,
              "recorded_product_hashes": hashes, "old_substring_on_counterexamples": old_contains,
              "inputs_unchanged": True,
              "limits": ["No native build or test executed", "No fresh Train, Flow, Save or Load",
                         "Recorded hashes verify historical before/restore equality, not current runtime DLL identity",
                         "Root cold expectation was already documented as stale; raw preserved",
                         "Broad deletion mutant differs from historical Contains expression"]}
    with args.output.open("x", encoding="utf-8") as out:
        json.dump(result, out, ensure_ascii=False, indent=2)
        out.write("\n")
    print(json.dumps({"verified_input_blobs": len(before), "trx_files": len(trx),
                      "receipt_rows": sum(r["rows"] for r in receipts), "inputs_unchanged": True}))


if __name__ == "__main__":
    main()
