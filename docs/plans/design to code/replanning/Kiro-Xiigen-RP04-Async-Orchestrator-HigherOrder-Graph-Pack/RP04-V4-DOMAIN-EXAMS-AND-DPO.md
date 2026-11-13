# RP04 V4 — four-domain law passports, DPO quality and bulk-to-bulk scientific exam

**Fixtures only, not executed examples or Chinese-author DPO.** When the real source of law is not available, label \`SYNTHETIC_FIXTURE\`. Do not claim these templates prove real legal compliance. A subject law may be a C# type-system rule, language morphosyntactic constraint, explicit consent requirement, or accessibility requirement, but record exact authority, version/date and applicability. Never infer law correctness from embedding cosine distance or kNN top-1.

## A. Every case must be machine-auditable and human-readable

\`domain_id; case_id; intent; source_family; train|dev|sealed_T; law/source/version/jurisdiction_or_SYNTHETIC; permitted/forbidden/abstain actions; roles/type/actor/time/negation/referent; TInput→IQueryable<TOutput> with real OutputRefs; RP02 tokenization/embedding/LoRA_projection/dynamic_k_profile; RP03 Unit/Flow schema; E3 plan candidates and E4 ordered pattern alternatives; snapshot versions; ordinary user entry→arbiter→output; raw outcome/judge; branch/accounting\`.

**Do not compare across changed vocabularies**; for a kNN intervention, freeze all downstream scorers and explicitly compare no retrieval, lawful fixed-k, law-aware dynamic-k, law-aware dynamic-k + role/relation scorer at SAME available index. The RP02 owner implements that work; RP04 consumes its version, cannot silently modify and attribute improvements to E3/E4.

## B. Practical domain deck with wrong, correct and edge

| ID and domain source | Development GOOD and *why* | Development BAD and *why* | EDGE and blind T novel family |
|---|---|---|---|
| CODE-01 · C# type/nullable + SQL dependency | \`int? x\` -> check \`HasValue\`/safe branch → parameterized SQL dependency check → emit typed **proposed** patch, no DB write. Good: type/dependency laws preserved | \`int y=x.Value\` without guard or SQL string concatenation; invalid despite high Graph/kNN similarity. Wrong ordering executes a write before schema validation | Sealed T: \`Task<int?>\`, transactions, schema migration of distinct AST family. Missing C# version/schema → abstain; no silent new SQL effect |
| LANG-01 · EN→RU syntax/grammar | \`the red car\` → \`красная машина\`; POS adjective agrees with noun **gender/number/case**, same referent, object role/syntax slot. Good: semantic/grammatical law, not nearest-word closeness | \`красный машина\` or use previous feminine token across a new referent; rule violation. A near-neighbor car vector alone is insufficient | Sealed T: several clauses with changing subject/speaker, quotation or anaphora to competing antecedents. Unclear antecedent → preserve uncertainty/clarify, not blindly force gender |
| ID-01 · scoped identity + consent + expiry, **SYNTHETIC** | Run read-only identity checks concurrently; join valid scoped consent and non-expired permission before **one** activation. Good: lawful dependency DAG/side-effect | Activate because first identity result was fast, or run two activations in parallel; no consent, duplicate effect even if lower latency | Sealed T: revocation between fan-in and publish, actor switch, duplicate callback + stale law version; safe result deny/rollback |
| DTC-01 · design→code accessibility and stack | From authorized design screenshot + explicit requirements produce typed component tree, accessible \`<label for="q">...<input id="q">\` in requested stack, mark absent assets. Good: user output/contract observed | Output \`<input>\` with no accessible name, silently choose different framework, or hallucinate unseen mobile layout to satisfy picture. Wrong: violates source/requirements | Sealed T: different responsive family nested reusable component and unknown mobile state. Ask evidence/clarification if missing |
| MIXED-01 / UNKNOWN | Russian instruction about C# SQL migration must keep both language intent and CODE+SQL constraints; unknown unsupported law => abstain | Force domain from top cosine neighbor, invent jurisdiction/date, remove an inconvenient rule for faster path | Blind T: combined rule conflict across versions; explicit uncertainty required |

For each case record **E3 candidate path** and **E4 directed motif** separately, e.g. CODE \`Parse AST→Type law→SQL binding→Propose patch\`, ID \`Read identity || Read profile→Consent→Expiry→one effect\`; E4 order-3 \`(Parse→Type)→SQL→Proposal\`. Ensure real caller supports output types. The blind T case must change *underlying source/structure*, not just rename a variable, reorder a sentence or paraphrase a prompt. A correct holdout result means independently judged final requested output, not simply route picked or score increased.

## C. First lesson then meaningful multi-bulk study

**S0 preparation**: define source-family disjoint TRAIN, DEV, sealed T; two domains for first trial. Pre-register method, comparison, N, time/memory budget, meaningful delta, critical-law veto, independent evaluator before T is opened.

**Pilot B0 (development, may be small)**: one legal observed update signal → \`P\` stored parent; \`C0\` same software zero-update; \`U\` one E3 or E4 update; exact parameter delta + signal lineage; cold process on real ordinary API; raw 2-domain lawful examples and negatives. Success level **STATE_CHANGED + ORDINARY_USED only**.

**B1**: pre-selected next disjoint training family, same admission rules/one target; evaluate untouched T for initial 4 domains where licensed and executable, mixed/unknown. Track valid lawful result / total, critical-law violation veto, macro and worst-domain, abstain correctness.

**B2**: another fresh bulk/new subject family; measure new improvement vs B1 and forgotten ability on B0/B1 domains; capture B0→B1→B2 per-domain matrix. If resources/data unavailable do not invent B1/B2 or claim continuation. \`N\` and uncertainty always from real observations.

**Causal arms**, identical candidates and source laws:
- E3: fixed legal plan / same-code no-update policy / U learned E3 policy; freeze E4, RP02 representation/kNN, RP03 Unit/Flow, Student.
- E4: fixed admitted order-1 baseline / fixed order-2/3 topology + C0 no-update / U learned order-2/3 preference; freeze E3 and same typed candidates.
- Optional follow-on hybrid factorial: only after isolated E3 and E4 establish separate effects, measure combination vs both standalone, without confounding domain representation or LoRA changes.
- Negative-control: same policy code, equal training orchestration but **ZERO accepted signal / update**, should not improve from training-induced path.

**Minimum raw row:** experiment_id, lane, case/source-family/split, law_version, passed_admission, candidate_hash, input_hash, parent/C0/U snapshot hashes, observed_signal, old/new target parameters, exact update id, chosen path, typed user output, arbiter validity, score and judge id, abstain reason, wall_ms, cpu_ms, gpu_ms or unavailable, branch_count, branch_success_fail_cancel, branch_sum_cost, peak RAM/VRAM, tokenizer/kNN/graph/checkpoint bytes, cold load_ms, pc_config, trace_hash. Aggregate latency/accuracy per domain & worst domain; PC whole runtime includes runner, ranker, retrieval, canceled branches and storage; never report only the winning branch.

Stop/promotion: illegal effect, severe law breach, outdated consent/version, duplicate training signal or uncontrolled privacy => **veto/rollback**. Better CPU but worse lawful accuracy => no blanket improvement. \`TIE/REGRESSION/INCONCLUSIVE\` is a valid science result. CODEX independent final reviewer must examine exact raw data and candidate SHA, not an edited summary.

## D. Exact DPO example and operator roles (not actual generated data)

For an authorized teaching case \`x\` of CODE-01, CODEX GPT-6 Astra 6 UltraCode first selects the actual **human lesson**: \`Explain nullable-to-SQL patch under exact C# and SQL source constraints, no database mutation\`. *Only then*, a separately approved **external OPEN CHINESE** model authors **BOTH final** chosen/rejected for *identical immutable* \`x+context+law\`. Good illustrative intended distinction: chosen explains guard, parameterization and no-effect; rejected recommends \`x.Value\` without guard and concatenated SQL as a plausible but materially inferior answer. BAD: use another input for rejected, quote a stale law, only ask Chinese for chosen while using Student output as rejected, fabricate teacher provenance, produce both-correct/TIE, or choose cartoonishly absurd unrelated negative. EDGE: two answers are equally legally good, private training context unauthorized, source applicability uncertain => quarantine \`TIE/BOTH_BAD/UNJUDGEABLE\` and no optimizer update.

A **separate CODEX GPT-6 Astra 6 UltraCode invocation** judges unchanged pair bytes and context: legal/semantic validity, meaningful rank, rights/license, near duplicate/heldout leakage, identical \`x\` and author evidence. It returns \`ADMIT/REJECT/TIE/BOTH_BAD/UNJUDGEABLE\`; no auto-rewrite. The post-train blind evaluator is yet another independent role; the CODEX judge is NOT DPO reference model.

If admitted to **Student DPO**, verify actual response-only masks and \`log p_policy - log p_reference\` preference difference in Student optimizer; RP01 owns. If admitted to E3/E4, derive a **separately source-proven** ranking objective/credit update and explicit recipient; pair text itself does not magically update Graph or claim standard DPO. Always record actual model name/version/grant/raw response hashes, else \`NOT_INVOKED\` not "teacher result".

## E. Independent examiner challenge examples

1. A learned Graph outputs the right C# suggestion but took 3× CPU and broke ID consent — **REJECT adoption**, even if average accuracy rose.
2. More correct EN→RU output appears because tokenizer was changed between C0 and U — **not proven E3 gain**, split RP02 experiment.
3. kNN returns close feminine adjective but underlying noun referent switched to masculine — **law-aware role check must precede similarity**, otherwise negative sample.
4. Fewer Windows tests failed while no learner parameter changed — **NO_LEARNING**.
5. Unknown domain: abstention rather than hallucinated legal answer can be a correct outcome if defined before exam.
6. No teacher grant/actual CODEX Astra invocation — preserve runnable fixtures, leave model receipts \`NOT_INVOKED\`; never claim selection/judging performed.
