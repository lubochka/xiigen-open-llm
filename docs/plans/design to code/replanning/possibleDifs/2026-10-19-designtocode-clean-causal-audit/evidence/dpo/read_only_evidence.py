"""Read-only arithmetic and comparison of retained primary receipts.
No model, native runtime, optimizer object, network call, teacher or training.

Portable usage with an existing DesignToCode-clean checkout or materialized tree:
  python read_only_evidence.py --source-root /path/to/llm_mvp_core \
    --manifest /path/to/source-integrity.json --output /tmp/dpo-evidence.json

Each primary file actually read must match its pinned Git blob SHA in the
manifest. The checkout HEAD name is not accepted as proof of source identity.
The original two batch JSON files are UTF-16LE with BOM; their original bytes
are checked before decoding. No file from the input tree is changed.
"""
from __future__ import annotations
from pathlib import Path
import argparse, json, struct, math, hashlib
from fractions import Fraction

ROOT=Path(__file__).resolve().parent
SOURCE_REF="e346b88a3cc1fcce48aefa626bfc7b00509eefd6"
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument("--source-root",type=Path,default=ROOT/"source"/"clean",
    help="Root of an existing clean source checkout/materialized tree; it contains 43.LearningLocalWikiData.")
parser.add_argument("--manifest",type=Path,default=ROOT/"source-integrity.json",
    help="Published pinned source-integrity manifest.")
parser.add_argument("--output",type=Path,default=ROOT/"arithmetic-and-retained-receipts.json",
    help="Destination for the computed report; never a source-tree path.")
args=parser.parse_args()
SRC=args.source_root.resolve()
OUTPUT=args.output.resolve()
if OUTPUT.is_relative_to(SRC):
    parser.error("--output must be outside --source-root; source inputs are read-only")
manifest=json.loads(args.manifest.read_text(encoding="utf-8"))
expected={entry["path"]:entry for entry in manifest["sources"]
    if entry["branch"]=="clean" and entry["ref"]==SOURCE_REF}
verified_inputs={}
SERIES=SRC/"43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-project-wiki-query-planning-sequence-002-dpo"
def readtext(path):
    relative=path.relative_to(SRC).as_posix()
    entry=expected.get(relative)
    if entry is None:
        raise ValueError(f"No pinned clean source entry: {relative}")
    b=path.read_bytes()
    blob_sha=hashlib.sha1(b"blob "+str(len(b)).encode("ascii")+b"\0"+b).hexdigest()
    sha256=hashlib.sha256(b).hexdigest()
    if blob_sha!=entry["git_blob_sha"] or sha256!=entry["sha256"] or len(b)!=entry["bytes"]:
        raise ValueError(f"Pinned source bytes differ: {relative}")
    verified_inputs[relative]={"ref":SOURCE_REF,"git_blob_sha":blob_sha,"sha256":sha256,"bytes":len(b)}
    return b.decode("utf-16") if b.startswith((b"\xff\xfe", b"\xfe\xff")) else b.decode("utf-8-sig")
def f32(x):
    return struct.unpack("<f", struct.pack("<f", x))[0]
def scalar_reduce(xs):
    s=f32(0)
    for x in xs: s=f32(s+x)
    return s
def chunk_round_reduce(xs,width):
    # Not a general emulation of .NET Vector.Dot/JIT reduction order.
    # Each nonzero adversarial chunk has at most two nonzero exact products;
    # its correctly rounded float sum is enough for this illustrative case.
    s=f32(0)
    for i in range(0,len(xs),width):
        exact=sum((Fraction.from_float(x) for x in xs[i:i+width]),Fraction(0))
        s=f32(s+f32(float(exact)))
    return s
def first_adam_delta(g):
    b1,b2,eps,lr=map(f32,(.9,.95,1e-8,1e-5))
    bc1=f32(1-b1);bc2=f32(1-b2)
    m=f32(bc1*g)
    v=f32(f32(bc2*g)*g)
    mh=f32(m/bc1);vh=f32(v/bc2)
    return f32(f32(lr*mh)/f32(f32(math.sqrt(vh))+eps))
epsilon=f32(2**-26)
cases=[]
for width in (4,8,16):
    xs=[f32(0)]*(2*width)
    xs[0]=f32(1); xs[width]=f32(-1);xs[width+1]=epsilon
    a=scalar_reduce(xs);b=chunk_round_reduce(xs,width)
    exact=float(sum((Fraction.from_float(x) for x in xs),Fraction(0)))
    benign=[f32(1/64)]*(2*width)
    cases.append(dict(vector_width=width,terms=xs,exact_sum=exact,
        clean_scalar_fp32=a,chunk_rounded_fp32=b,
        scalar_first_adam_delta=first_adam_delta(a),
        chunk_first_adam_delta=first_adam_delta(b),
        benign_scalar=scalar_reduce(benign),
        benign_chunk=chunk_round_reduce(benign,width)))
records={}
for num in (23,25):
    data=json.loads(readtext(SERIES/f"batch-{num:03d}-run-output.json"))
    run=data["dpoMetrics"]["runId"]
    diag=SERIES/"dpo-runs/project-wiki-query-planning-flow"/run/"dpo-pair-diagnostics.jsonl"
    rows=[json.loads(x) for x in readtext(diag).splitlines() if x.strip()]
    diagnostic_by_id={r["PairId"]:r for r in rows if r["Split"]=="heldout"}
    pairs=data["heldOutBatch"]["pairs"]
    by_triple={}
    for p in pairs:
        key=(p["a"],p["chosen"],p["rejected"])
        if key in by_triple: raise ValueError("Duplicate raw heldout triple")
        by_triple[key]=diagnostic_by_id[p["pairId"]]
    records[num]=(data,by_triple)
d23,r23=records[23];d25,r25=records[25]
common=sorted(set(r23)&set(r25))
panel_bytes=json.dumps(common,ensure_ascii=False,separators=(",",":")).encode("utf-8")
summary={
 "status":"SOURCE_ARITHMETIC_AND_RETAINED_RECEIPTS_ONLY",
 "model_runs":0,"native_runs":0,"training_updates":0,"teacher_calls":0,
 "source_verification":{"pinned_ref":SOURCE_REF,"verified_inputs":verified_inputs,
    "scope":"Only the primary files actually consumed by this arithmetic/receipt script; the full audit manifest contains more reviewed sources."},
 "arithmetic":{
   "caveat":"No .NET/JIT or actual q/v gradient was executed; chunk-rounded cancellation illustration only.",
   "adam_settings":{"beta1":.9,"beta2":.95,"epsilon":1e-8,"learning_rate":1e-5,"initial_m_v_weight":0},
   "cases":cases
 },
 "retained_wiki_023_to_025":{
   "source_ref":SOURCE_REF,
   "historical_execution_sha":"NOT_RECORDED_IN_RECEIPTS",
   "before_count":len(r23),"after_count":len(r25),
   "identical_raw_heldout_triples":len(common),
   "canonical_matched_panel_sha256":hashlib.sha256(panel_bytes).hexdigest(),
   "before_saved_preference_positive":sum(r23[k]["Accuracy"] for k in common),
   "after_saved_preference_positive":sum(r25[k]["Accuracy"] for k in common),
   "before_saved_mean_margin":math.fsum(r23[k]["RewardMargin"] for k in common)/len(common),
   "after_saved_mean_margin":math.fsum(r25[k]["RewardMargin"] for k in common)/len(common),
   "before_summary_margin":d23["dpoMetrics"]["heldOutRewardMargin"],
   "after_summary_margin":d25["dpoMetrics"]["heldOutRewardMargin"],
   "new_saved_positive_pairs":sum(r23[k]["Accuracy"]==0 and r25[k]["Accuracy"]==1 for k in common),
   "lost_saved_positive_pairs":sum(r23[k]["Accuracy"]==1 and r25[k]["Accuracy"]==0 for k in common),
   "metric_scope":"Reference-relative preference on supplied responses. No new free answers."
 }
}
OUTPUT.write_text(json.dumps(summary,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print(json.dumps({k:summary[k] for k in ["status","model_runs","native_runs","training_updates","teacher_calls","retained_wiki_023_to_025"]},ensure_ascii=False,indent=2))
print("cancellation:",cases[0])

