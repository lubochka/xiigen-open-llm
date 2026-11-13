#!/usr/bin/env python3
"""Read saved JSON and queue text only; never invoke model, training or native code."""
from pathlib import Path
from collections import Counter, defaultdict
import csv
import hashlib
import json
import posixpath
import re
import argparse
import tempfile

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--snapshot-root', type=Path, default=Path(__file__).resolve().parents[1], help='Frozen evidence bundle root containing repositories/ and snapshot_metadata/.')
parser.add_argument('--output-dir', type=Path, help='New directory that does not yet exist. Never use the accepted evidence directory.')
args = parser.parse_args()
ROOT = args.snapshot_root.resolve()
CORE = ROOT / "repositories/llm_mvp_core"
if args.output_dir is not None:
    OUT = args.output_dir.resolve()
    OUT.mkdir(parents=True, exist_ok=False)
else:
    OUT = Path(tempfile.mkdtemp(prefix='xiigen-audit-recompute-'))
SHA = "f37a8faa338a705c340579a2ad225dfd27b42cf7"
QUEUE = "docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md"
BASE = "experiments-v2/RP01-MICRO/"
inputs = set()

def read(path):
    inputs.add(path)
    return json.loads((CORE / path).read_text(encoding="utf-8-sig"))

def write(name, data):
    (OUT / name).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n")

def score(rows):
    return {"correct": sum(r["Actual"] == r["Gold"] for r in rows), "total": len(rows),
            "stored_correct_flags_match": all(r["Correct"] == (r["Actual"] == r["Gold"]) for r in rows)}

def ceilings(rows, ik="Input", gk="Gold"):
    grouped = defaultdict(Counter)
    for r in rows:
        grouped[r[ik]][r[gk]] += 1
    return {"rows": len(rows), "constant_answer_ceiling": max(Counter(r[gk] for r in rows).values(), default=0),
            "deterministic_input_only_ceiling": sum(max(c.values()) for c in grouped.values()),
            "conflicting_inputs": {str(k): dict(v) for k,v in grouped.items() if len(v)>1}}

result = {"mode": "SAVED_JSON_AND_STATIC_INDEX_RECOMPUTATION_ONLY", "source_commit": SHA,
          "native_execution": False, "new_training_updates": 0,
          "limitations": ["JSON observations are recounted; state payload bytes are not independently loaded.",
                          "No timing, gradient oracle or model readout is rerun.",
                          "Git LFS pointers are not numerical weight payloads."]}

# P49: re-score every saved readout rather than trusting aggregate accuracy fields.
p49 = {}
all_rows = {}
for order in ("inplace", "prestep"):
    prefix = BASE + f"P4901/results/attempt-0002/{order}/"
    before, after = read(prefix + "before.json"), read(prefix + "after.json")
    panels = {"initial": before["panel_at_initial"], "warm_before": before["panel_at_warm"],
              "cold_after": after["panel_at_arm_A_after"], "warm_after": after["panel_at_arm_B_after"]}
    all_rows[order] = panels
    p49[order] = {stage: {"all": score(rows), "exam": score([r for r in rows if r["Kind"]=="EXAM"]),
                               "new_material_project_B_including_exam": score([r for r in rows if r["ProjectId"]=="B"]),
                               "new_training_material_only": score([r for r in rows if r["Group"]=="NEW_SET"]),
                               "old_skill_training_rows": score([r for r in rows if r["Group"]=="WARM_SET"]),
                               "old_only": score([r for r in rows if r["Overlap"]=="OLD_ONLY"])}
                  for stage,rows in panels.items()}
    initial = {r["RowId"]: r for r in panels["initial"]}
    warm = {r["RowId"]: r for r in panels["warm_before"]}
    p49[order]["cold_gained_lost"] = {"gained": sum(not initial[r["RowId"]]["Correct"] and r["Correct"] for r in panels["cold_after"]),
                                           "lost": sum(initial[r["RowId"]]["Correct"] and not r["Correct"] for r in panels["cold_after"])}
    p49[order]["warm_gained_lost"] = {"gained": sum(not warm[r["RowId"]]["Correct"] and r["Correct"] for r in panels["warm_after"]),
                                           "lost": sum(warm[r["RowId"]]["Correct"] and not r["Correct"] for r in panels["warm_after"])}
    panel = before["panel"]
    train = {(r["input"],r["gold"]) for r in panel if r["Kind"]=="TRAIN_CONTROL"}
    exam = [r for r in panel if r["Kind"]=="EXAM"]
    p49[order]["exam_train_pair_overlap"] = {"matching": sum((r["input"],r["gold"]) in train for r in exam), "total": len(exam)}
    p49[order]["ceilings"] = {"all": ceilings(panel,"input","gold"),
                              "train": ceilings([r for r in panel if r["Kind"]=="TRAIN_CONTROL"],"input","gold"),
                              "exam": ceilings(exam,"input","gold")}
p49["between_backward_orders"] = {"paired_observations": 64,
     "identical_actual_answers": sum(a["Actual"]==b["Actual"] for k in all_rows["inplace"]
                                      for a,b in zip(all_rows["inplace"][k],all_rows["prestep"][k])),
     "all_row_ids_aligned": all(a["RowId"]==b["RowId"] for k in all_rows["inplace"]
                                for a,b in zip(all_rows["inplace"][k],all_rows["prestep"][k]))}
result["P49"] = p49

# P48: recompute answer counts and attainable ceiling; preserve source-only numerical-health status.
p48_before = read(BASE + "P4801/results/attempt-0002/raw-before.json")
p48_after = read(BASE + "P4801/results/attempt-0002/raw-after.json")
p48_metrics = read(BASE + "P4801/results/attempt-0002/metrics.json")
p48_arms = read(BASE + "P4801/results/attempt-0002/arms.json")
panel = p48_before["panel_rows"]
train = {(r["Input"],r["Gold"]) for r in panel if r["Kind"]=="TRAIN_CONTROL"}
exam = [r for r in panel if r["Kind"]=="EXAM"]
p48 = {"ceilings": ceilings(panel), "exam_train_pair_overlap": {"matching": sum((r["Input"],r["Gold"]) in train for r in exam),"total": len(exam)}, "arms": {}}
for arm in p48_after["arms"]:
    rows = arm["answers"]
    p48["arms"][arm["arm"]] = {"all": score(rows), "exam": score([r for r in rows if r["Kind"]=="EXAM"]),
                                      "nonfinite_readout_rows_as_recorded": sum(not r["Finite"] for r in rows),
                                      "distinct_answers": len({r["Actual"] for r in rows})}
new = {a["arm"]:a["answers"] for a in p48_after["arms"] if a["arm"].startswith("new-")}
p48["corrected_answers_differ"] = sum(a["Actual"] != b["Actual"] for a,b in zip(new["new-arm-A"],new["new-arm-B"]))
health=[]
for arm in p48_metrics["arms"]:
    s=arm["state"]
    health.append({"arm":arm["arm"],"finite":s["finite"],"nan":s["nan"],"infinite":s["infinite"],
                   "total":s["weights_total"],"block_totals_consistent":all(sum(b[k] for b in s["blocks"])==s[v] for k,v in (("Finite","finite"),("Nan","nan"),("Infinite","infinite"),("Total","weights_total"))),
                   "independently_counted_weight_payload":False})
p48["source_recorded_weight_health_internal_arithmetic"] = health
p48["recorded_primary_arm_updates"] = sum(a["updates"] for a in p48_arms["arms"])
p48["first_nonfinite_weight_steps_recorded"] = {a["arm"]:a["divergence"]["first_non_finite_weight_step"] for a in p48_arms["arms"]}
result["P48"] = p48

# P50: compare raw consumer journals to independent gold, not just saved Correct fields.
p50_before=read(BASE+"P5001/results/attempt-0002/before.json")
p50_after=read(BASE+"P5001/results/attempt-0002/after.json")
p50_arms=read(BASE+"P5001/results/attempt-0002/arms.json")
p50_restore=read(BASE+"P5001/results/attempt-0002/restore.json")
gold={r["case_id"]:r["gold_steps"] for r in p50_before["consumer_rows"]}
train_inputs={r["input"] for r in p50_after["unit_rows"] if r["kind"]=="TRAIN"}
exam_rows=[r for r in p50_after["unit_rows"] if r["kind"]=="EXAM"]
p50={"cold_consumer_correct":sum(r["journal"]==r["gold_steps"] for r in p50_before["consumer_rows"]),
      "exam_total":len(gold), "unit_train_correct":sum(r["answer_route"]==r["gold_route"] for r in p50_after["unit_rows"] if r["kind"]=="TRAIN"),
      "unit_exam_correct":sum(r["answer_route"]==r["gold_route"] for r in exam_rows),
      "literal_exam_train_input_overlap":sum(r["input"] in train_inputs for r in exam_rows),
      "constant_journal_ceiling":max(Counter(tuple(v) for v in gold.values()).values())}
for key in ("arm_a_outcomes","arm_b_outcomes"):
    rows=p50_arms[key]
    p50[key]={"correct_from_journal_vs_gold":sum(r["Journal"]==gold[r["CaseId"]] for r in rows),
              "stored_correct_flags_match":all(r["Correct"]==(r["Journal"]==gold[r["CaseId"]]) for r in rows),
              "distinct_journals":len({tuple(r["Journal"]) for r in rows})}
p50["heuristic_ties_from_pairwise_scores"] = sum(r["overlap_with_review_node"]==r["overlap_with_implement_node"] for r in p50_arms["arm_a_detail"])
p50["restore_saved_rows"]={"correct_routes":sum(r["answer_route"]==r["gold_route"] for r in p50_restore["rows"]),
                          "total":len(p50_restore["rows"]),"nearest_id_oracle_matches":sum(r["nearest_lesson_id_from_loaded_index"]==r["oracle_nearest_lesson_id_plain_arithmetic"] for r in p50_restore["rows"]),
                          "recorded_train_calls":p50_restore["train_calls_in_this_process"],
                          "scope":"experimental separate-process reader; not native KnnLogicalUnit import/ExecuteAsync acceptance"}
result["P50"]=p50

# All 131 queue rows are enumerated without treating RESULT_RECORDED as QA acceptance.
inputs.add(QUEUE)
rows=[]
selected_report_ids=set(range(30,51))|{52}
for lineno,line in enumerate((CORE/QUEUE).read_text().splitlines(),1):
    m=re.match(r"^\|\s*(Q\d{3})\s*\|\s*\[([^\]]+)\]\(([^)]+)\)\s*\|\s*(.*?)\s*\|\s*(.*)\|\s*$",line)
    if not m: continue
    q,card_id,relative,title,status=m.groups()
    number=int(q[1:])
    if number in (48,49,50):
        qa="SELECTED_SOURCE_REPORT_AND_SAVED_JSON_RECOMPUTED_NO_NATIVE_EXECUTION"
    elif number in selected_report_ids:
        qa="SELECTED_REPORT_OR_RELEVANT_SOURCE_PARTIALLY_REVIEWED_NO_NATIVE_EXECUTION"
    else:
        qa="NOT_INDEPENDENTLY_RECHECKED_IN_THIS_EXPERIMENT_REVIEW"
    rows.append({"row_id":q,"card_id":card_id,"title":title,"source_card_path":posixpath.normpath(posixpath.join(posixpath.dirname(QUEUE),relative)),
                 "recorded":bool(re.search(r"\bRESULT_RECORDED(?:_PARTIAL)?\b",status)),
                 "qa_evidence_status":qa,"native_qa_rechecked":False,
                 "source_status_line":lineno,"source_status_verbatim":status,
                 "source_url":f"https://github.com/lubochka/llm_mvp_core/blob/{SHA}/{QUEUE}#L{lineno}"})
assert len(rows)==131 and len({r["row_id"] for r in rows})==131
inventory={"source_commit":SHA,"source_index":QUEUE,"total_rows":len(rows),"unique_row_ids":len({r["row_id"] for r in rows}),
           "result_recorded_count":sum(r["recorded"] for r in rows),"native_qa_rechecked_count":0,"qa_accepted_count":None,
           "qa_count_note":"Acceptance is not inferred from ROOT_ACCEPTED substrings or RESULT_RECORDED. Source statements retain their own scope; selected reviews below are this audit's bounded work, not a new QA PASS.",
           "review_scope_counts":dict(Counter(r["qa_evidence_status"] for r in rows)),"rows":rows}
write("experiment_coverage_131.json",inventory)
fields=["row_id","card_id","title","source_card_path","recorded","qa_evidence_status","native_qa_rechecked","source_status_line","source_url"]
with (OUT/"experiment_coverage_131.csv").open("w",newline="",encoding="utf-8-sig") as f:
    w=csv.DictWriter(f,fieldnames=fields,extrasaction="ignore");w.writeheader();w.writerows(rows)
result["queue"]={k:v for k,v in inventory.items() if k!="rows"}

# Prove the exact bytes of every input used by these recomputations.
expected={}
for metadata in ("experiment_review_trees.json","experiment_exact_manifest.json","core_expected_blobs.json"):
    for x in json.loads((ROOT/"snapshot_metadata"/metadata).read_text()):
        if "sha" in x: expected[x["path"]]=x["sha"]
expected[QUEUE]="62d74aa69eb5a2532d6146bf20f1e137eda34bb0"
checks=[]
for path in sorted(inputs):
    b=(CORE/path).read_bytes()
    actual=hashlib.sha1(b"blob "+str(len(b)).encode()+b"\0"+b).hexdigest()
    checks.append({"path":path,"bytes":len(b),"expected_git_blob":expected.get(path),"actual_git_blob":actual,"exact":expected.get(path)==actual})
assert all(x["exact"] for x in checks),[x for x in checks if not x["exact"]]
result["input_git_blob_checks"]=checks
write("experiment_saved_recomputation.json",result)
print(json.dumps({"output_directory":str(OUT),"queue_rows":len(rows),"recorded":inventory["result_recorded_count"],"exact_saved_inputs":len(checks),
                  "P49_64_answers_equal":p49["between_backward_orders"]["identical_actual_answers"],
                  "P48_new_answers_differ":p48["corrected_answers_differ"],"P50":p50},ensure_ascii=False,indent=2))
