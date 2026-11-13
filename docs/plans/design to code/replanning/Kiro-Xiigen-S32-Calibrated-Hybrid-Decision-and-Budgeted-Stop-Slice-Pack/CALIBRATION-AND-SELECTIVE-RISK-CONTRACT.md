# S32 — proposed versioned evidence / selective-decision contract (NOT observed code)

## Example event (illustrative schema, not an existing API)
```json
{
 "schema":"xiigen.s32.evidence.v1",
 "request_id":"actual-run-id", "ordinary_consumer_ref":"K method / cold host",
 "subject_domain":"csharp-api", "subject_law_ref":"csharp-vX-source-proven",
 "intent_d04":"CODE_Q", "project_scope":"consented-local",
 "source_family_id":"SEALED-ONLY-ID", "source_rights_ref":"approved-ref",
 "input_digest":"sha256", "feature_schema_ref":"verified-v1",
 "state":{"student":"checkpoint-or-NONE","lora":"adapter-or-NONE","index":"index-version","graph":"graph-version","calibration":"curve-or-NONE"},
 "producer_observations":[
  {"producer":"kNN","raw_score_kind":"cosine-distance","raw_score":0.21,"smaller_is_better":true,
   "candidate_refs":["A","B"],"used_candidate_refs":["A","B"],"score_scope":"same-law-compatible"},
  {"producer":"Graph","raw_score_kind":"route-score","raw_score":4.7,"smaller_is_better":false,
   "path_ref":"executed-legal-path"}
 ],
 "calibration":{"event":"lawful_and_correct_answer_given_context","scope_ref":"domain-law-intent-state",
  "sample_n":null,"heldout_metric_ref":null,"reliability":null,"status":"NOT_CALIBRATED"},
 "decision":{"hard_law_veto":false,"type_admitted":true,"rights_admitted":true,
   "action":"ABSTAIN_OR_ASK","reason":"NO_VALID_CALIBRATION"},
 "cost_ref":"whole-pc-and-all-branches-receipt","trace_ref":"real-readback"
}
```

## Semantics
- Domain, intent, source family, version and state are separate. A D-04 `CODE_Q` classifier does NOT identify C# as a subject. Mixed and unknown use multi-law/UNKNOWN statuses.
- Define the **target event and supervision**: e.g. lawful+factually correct *final answer within fixed budget*, judged blind at specified source-family split. A correct retrieval neighbor is not the same event as a good end-user answer.
- Per-producer raw scores retain units, polarity, scaling, missingness, candidate/OutputRef associations, actual usage, and version identity. Never add incomparable raw scores. If no independent labels, `NOT_CALIBRATED`, not `p=.5`.
- Calibrated reliability is a *statistical claim conditional on supported sample* with n/CI/split/method/risk/tolerance, not a permission to disobey hard laws. Backoff for sparse or unseen domain is explicit conservative rule, never borrowed unseen accuracy.
- Training updates, if any, go through preexisting RP11 state owner with unique bulk, parent, checkpoint, consent, rollback and RP09 independent examination; S32 integration remains a consumer adapter only.
- Evidence/provenance may include law-critical token features. Semantic projection and LoRA are distinct; mismatch → `INCOMPATIBLE_COORDINATE_SYSTEM`, not silent numeric normalization.
- If allowed alternatives have costs, record expected utility of additional branch only with independent historical evidence; include cancelled/failed branch cost, CPU/GPU/RAM/latency and deadline in the decision. Otherwise `NO_EVI_ESTIMATE`.
- D-15 final arbiter and permission checks are strictly deterministic; uncertainty cannot override them. Any stored calibration sourced from withdrawn rights / changed law must be ineligible at cold reload and delegated to S26/RP14/15 for state cleanup.

## Concrete anti-examples
**GOOD:** same source-family-disjoint SQL case, eligible paths, confidence label calibrated on past independent SQL cases under same law; risk bound allows ANSWER and independent judge agrees.
**BAD:** BM25=16.2 + cosine=0.84 + Student logprob=-3.1 => `confidence=93%` without common score meaning.
**EDGE:** n=3 (thin domain) and obsolete Angular law version => no promoted calibration; fall back to hard-law safe query/abstention. Another branch producing a valid answer after timeout is not free.

## Required diagnostic decisions
`ANSWER`, `GATHER_ALLOWED`, `ABSTAIN_OR_ASK`, `HARD_VETO`, `NO_LEGAL_PATH`, `NOT_CALIBRATED`, `INSUFFICIENT_PC_BUDGET`, `INCOMPATIBLE_STATE`, `REUSE_NO_CHANGE`. Record unknown distinctly from 0 and `not measured` distinctly from failure.
