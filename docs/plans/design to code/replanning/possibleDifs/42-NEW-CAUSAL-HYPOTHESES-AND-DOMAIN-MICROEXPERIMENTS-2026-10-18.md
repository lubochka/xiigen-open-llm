# 42 · Новые причинные гипотезы и быстрые экспериментальные развилки — после PrepareToGPL

**18.10.2026 · SOURCE-AUDIT + HYPOTHESES + SMALL-EXPERIMENT WORKPACK / NOT_RUN**  
Core good [`31346d89` PrepareToGPL](https://github.com/lubochka/llm_mvp_core/tree/31346d8928be4469b01aaf313780e8f950ff91e2), core current [`a392b6be` codex/dpo-implementation](https://github.com/lubochka/llm_mvp_core/tree/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6). **Only target for any writing:** planning/llm_plan3/possibleDifs. Core `docs/plans/replanning-possible-diffs` remains **READ_ONLY**.

This packet is **not a claim** that experiments were run, that hypotheses are conclusively novel within all other agents' 367 files, or that the original model can legally be resurrected unchanged. It records *new independent branch-comparison distinctions* and the cheapest interventions that could falsify each.

## 0. Прежде всего: отдельные получатели и правила доказательства

The same word «обучение» labels at least four noninterchangeable recipients:
1. **RP01 neural Student**: tokenizer `B0`, frozen random base, q/v LoRA+DPO, then autoregressive free generation.
2. **Translation learning Unit**: semantic tokenizer pattern/signature corrections, trained exact/word/phrase/kNN mappings, verifier rules; `TrainStructuredBatchAsync` → unit memory.
3. **Orchestration guide-pattern machine**: accepted DPO-derived pattern state in model checkpoint, verified manifest and source admission, `Plan()` normal consumer.
4. **Structural/domain retrieval and flow**: code AST, BM25, kNN, GraphRAG/learned flow and domain laws; often non-neural or hybrid.

**Rule:** every claim needs `domain / question / recipient / exact branch SHA / source row IDs / train-admit / state before-after / normal consumer / paired holdout / negative control / evidence link / status`. Do not infer improvement from an increased DPO margin, a passing synthetic unit test, or a deterministic proxy.

Historical positive receipts are **real but bounded**, see [N38 master](38-MASTER-PRIORITIES-POSITIVE-HISTORY-NEW-HYPOTHESES-AND-MICRO-DECISIONS-2026-10-16.md): E09 structural kNN relative +45.33% in one Tier2 JSON (with non-identical evaluation populations), E14 context-action KNN small Tier2 .75→1.00 but Tier3 .8824→.8676, E26 contextual dictionary .40→1.00 on 15 fixed examples. Some failed matrix algorithms moved into successful domain-sensitive KNN: read [matrix→kNN source](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) before ever labeling such a feature failed. **Do not pretend any of these receipts demonstrate today's Student free-answer capacity.**

## 1. Priority list of testable differences (confirmed vs conditional)

| ID · classification | What differs, evidence | Which learner/answer path | Domain relevance | First micro |
|---|---|---|---|---|
| **G40-01 · confirmed source incompatibility** | [old guide loader](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L409-L445) accepts v1; [current](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L974-L1015) requires v2 and proof while shipped asset still v1 | Orchestration state restore, not Student | orchestration | E42-A |
| **G40-02 · confirmed provenance drift** | [dataset12→9/8train→5](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/dataset-card.json); [ledger zero admitted](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/training-data/orchestration-reasoning/source-admission-ledger.json); named state still train8/patterns15 | lawfully admitted checkpoint training | orchestration | E42-B |
| **G40-03 · conditional discovery/consumption** | old index registers guide model, current omits; but [current consumer directly reads manifest](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L1427-L1444) and records load error | live orchestration caller | orchestration, IDE | E42-C |
| **G41-01 · confirmed metric definition drift** | [old translation margin 0.03](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationMachine.cs#L69-L83) even legacy scaffold vs [current margin0](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L69-L86) | report, not necessarily learned state | translation only | E42-D |
| **G41-02 · confirmed PASS boundary drift** | old [bootstrap passes on confidence](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationRoutingFlow.cs#L82-L95), current [excludes bootstrap](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationRoutingFlow.cs#L82-L98) | PASS reporting vs translation | translation only | E42-D |
| **G41-03 · negative control** | identical blobs old/current for `TranslationTrainedUnits.cs`, `TranslationSemanticTokenizer.cs`, `HnswIndex.cs` | unchanged implementation, caller path may diverge | translation/unit memory | E42-E |
| **P42-01 · fresh falsifiable inference** | index gives **12** patterns, old manifest & named checkpoint **15** (different metadata namespaces); normal consumer loads from manifest/path, not simply index | exported registry vs actual loaded state | orchestration only | E42-C |
| **P42-02 · fresh falsifiable inference** | directory has **two distinct** `checkpoint.json` and `orchestration-reasoning-v1-checkpoint.json`; caller uses manifest `checkpointRef`, fallback filename only when resolution unavailable. The generic `checkpoint.json` is old-branch-identical, named state differs only dataset card digest | artifact identity / cold start fallback | orchestration | E42-C |
| **P42-03 · fresh falsifiable inference** | Current guide [schema v2 safety/admission](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L974-L1015) and [new dataset training_allowed=false](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/dataset-card.json): "quality lost" may actually be **inability to legally admit previous state**. Re-enabling raw old data is prohibited; quality assessment must be separated from provenance authority | rejected learned state vs untrained fallback | orchestration | E42-A/B |
| **P42-04 · research/testability boundary** | sibling branch `language_translate` exists but is an **older tree lacking `27.TranslationDomain` and `models/index.json`**, so it cannot be compared on the same later checkpoint path; inspect legacy translation implementation and 35.ExperimentsV2 there by source/commit, don't claim it contains today's files | historical branch archaeology | translation / syntax | E42-F |

**Parallel new findings already published in this directory:** [N40 corpus text vs checkpoint identity on resume; in-batch novelty vs contradictory rows](40-NEW-CORPUS-IDENTITY-AND-INTRABATCH-ADMISSION-2026-10-18.md) and [their shadow receipts](40-SOURCE-ONLY-NOVELTY-AND-RESUME-RESULT.json). Do not count these as discoveries of this 42 workpack; they are complementary **inherited** failure modes, present also in `PrepareToGPL`, not yet demonstrated RP01 neural root causes. [GOLD-RESTORE-first raw E09/E26/E43/E22 historical receipts](40-PREPARETOGPL-RESTORE-P0-INDEX-2026-10-18.md) and [GOLD domain checks](40-MICRO-CAUSAL-EXPERIMENTS-BY-DOMAIN-2026-10-18.md) take priority for the specific goal of recovering observed historical useful learning.

**Already investigated by earlier agents: NOT NEW:** random B0 SFT-vs-DPO, q/v LoRA vs LM head, response mask backward, BPE identity drift, E09 in-file holdout leakage, E13 parsing, E22 label transfer, E26 POS, E43 replay, fusion top-k and BM25 normalisation, Unit template/deterministic no-op, graph seedless PageRank, KNN state alias, category/semantic routing. Crosswalk [N39](39-RP01-CROSSWALK-NOVELTY-PRIORITIES-AND-MICRO-EXPERIMENTS-2026-10-16.md), [core research 80](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/80-research-resume-verified-and-pending.md), [existing core checks P81–P91](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/87-new-checks-38-39.md). These are vital, but calling them "new discoveries" would be false.

## 2. Эксперименты, которые прямо приближают к качеству PrepareToGPL

### E42-A · GuideState load truth (C/U; orchestration)
**Source:** [G40-01](40-PREPARETOGPL-RECOVERY-PRIORITY-CHAPTER-2026-10-18.md).  
**Hypothesis:** current normal orchestration output lacks archived learned pattern because the supported old artifact fails current checkpoint validation.  
**A:** good code+good source-admitted v1 artifact as historical/isolated read. **B:** current loader + same v1 artifact, no mutation. **C:** properly authorized, newly reconstructed v2 artifact (only if R17 approved and admitted; otherwise `BLOCKED`).  
**Inputs:** 4 archived held-out prompts and 4 newly authored source-disjoint lawful orchestration prompts with contrary dependencies.  
**Measure:** `LoadErrorCode, learned_pattern_ids, matched_dependency_count / total, false_positive_dependencies, time_ms, memory_mb, cold_restart_result`. **Negative:** corrupt proof/absent proof must FAIL.  
**Falsifier:** same normal output despite load difference ⇒ difference not cause for that consumer. **Stop:** any authority mismatch; no bypass.  
**Tier:** R+small C first; U and L-memory only after authorization. **Run status:** NOT_RUN.

### E42-B · Historical learned content vs lawful retained sources (R/C/L-memory; orchestration)
**Hypothesis:** 3 quarantined training rows contributed an old pattern capability that 5 retained rows do not teach; the current artifact hash update masked the missing retraining.  
**Inputs:** diff old 8 train rows vs 5 retained by pair-id and exact source provenance; 4 held-out plus 4 new prompts tagged with affected/unaffected capability.  
**A:** only **archived receipts** for old 8 (no training on disallowed rows). **B:** source-admitted retained5, new v2 proof/state if authorized. **C:** lawful augmentation of missing capability from *different admissible sources* with same training budget.  
**Measure:** correct lawful decisions per slice, false positives, missing-capability gap, admitted_rows, rejected_rows, state pattern count, training/restore cost. Do not call A→B causal if historic evaluator/source differs.  
**Stop:** if `computed_training_allowed=false`, preserve historical vs plan only and report blocked. **Run status:** NOT_RUN.

### E42-C · Registry, dual checkpoint identity & normal consumer (C/U; orchestration, IDE)
**Hypothesis:** current consumer chooses an incompatible named v1 checkpoint, then falls back to untrained orchestration even though a different `checkpoint.json` is present.  
**A/B:** toggle only **manifest checkpointRef** in an isolated *copy* to explicitly name generic vs named checkpoint; keep safety loader unchanged. **C:** lawful new v2 artifact.  
**Record:** selected ref literal, actual SHA256, source path, `orchestration_reasoning_asset_loaded`, load error, generated plan/abstention, directory owner; fixed 4 normal API prompts.  
**Falsifier:** if consumer selects neither or both always fail by same schema, file naming is *not* causal; actual blocker is schema/authorization.  
**Negative:** removing index entry alone should not be asserted to prevent direct manifest loading; assert how actual source behaves. **No implicit promotion or registry mutation**. **Run status:** NOT_RUN.

### E42-D · Metric-only and bootstrap smoke (C; translation)
**Hypothesis:** apparent "loss of success" after branch migration is partly reporting semantics; unchanged learned translation state can yield changed `RewardMargin` / `TRANSLATED` verdict.  
**A/B:** inject same 2 bootstrap outputs and same 4 trained-adapter outputs through old/current flow with same state; add 6 independently evaluated native translation outputs for true quality.  
**Measure:** output text exact/semantic correctness, `Passed`, reported margin, adapter type, source direction, latency.  
**Falsifier:** text meaning differs ⇒ investigate caller/state separately, not just reporting. **Negative:** bootstrap with copied source must be labeled not semantically trained even if old PASS. **Run status:** NOT_RUN.

### E42-E · Hybrid Unit actually reachable (C/L-memory/U; translation, code-doc)
**Hypothesis:** historical learned corrections work when Unit receives them, but RP01 sends input into a distinct LoRA-only path.  
**A:** current ordinary RP01 Student response on 6 ENRU and 6 code-doc heldout questions. **B:** invoke existing Unit learned-state consumer on **same exact questions**, if semantics and state permit; trace `unit_id, source, exact/signature/knn candidate, verifier rejection, final answer`. **C:** normal router opts into law-scoped Unit only when domain/profile matched.  
**Measure:** meaning/law correctness per case, illegal candidate rate, learned-hit share, time_ms, measured state bytes; report `NOT_REACHED` for RP01 Unit if not invoked.  
**Controls:** unrelated domain (should not adopt POS/AST blindly), shuffled mappings (should lose benefit). **No assertion of RP01 learning from B vs A**: B tests mechanism reachability, not same learner. **Run status:** NOT_RUN.

### E42-F · Branch archaeology: earlier language_translate feature (R/C; translation)
**Hypothesis:** one useful old semantic order/POS primitive exists in sibling history outside current `27.TranslationDomain`; earlier "failed matrix" result was later consumed in kNN representation or code path.  
**Discovery:** GitHub branch search independently returned `language_translate`; root there lacks current `27.TranslationDomain` and `models/index.json`, so a naive file path diff is invalid. Search `12.SemanticCompiler`, `35.ExperimentsV2`, `experiments-v2`, archived `Results`, and use commit-lineage + stored raw JSON. **Do not infer positive effectiveness from branch name**.  
**A:** historical feature absent in current domain-specific kNN; **B:** opt-in feature on only syntax-/order-sensitive cases. Separate C# AST (6 unseen files) from ENRU clause order (6 source-disjoint sentences).  
**Measure:** semantic accuracy or valid structural dependency@1, candidate ordering, uncovered queries, cost. **Negative:** enable syntactic order feature on deliberately order-insensitive bag tasks (must not degrade), anti-control on non-code.  
**Promote only if:** same data/split/consumer held-out supports domain-specific quality gain and no material negative transfer. **Run status:** NOT_RUN.

### E42-G · Student is not pattern-memory: choose the right learning recipient (L-neural + C; C# and EN→RU)
**Known existing hypothesis** (not new): [RP01 recovery audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) says base B0 random/untrained, vocab=629, hidden512, 4 layers, q/v LoRA rank8. `DPO+LoRA` changes preference but cannot be assumed to teach grammar/language structure from random base.  
**Minimal 3-arm:** A=frozen random B0+DPO 8–16 accepted pairs; B=*same B0* tiny chosen-only supervised CE warm-up then DPO; C=base-only CE warm-up (separate intervention). Strictly same tokenizer/vocab/heldout/PC budget and even number of real optimizer updates. **Do not use disallowed data or add synthetic "correct" chosen without review.**  
**Measure:** signed conditional logP on counterfactual lawful y+/y−, generated correct answers of 6 C# code and 6 ENRU translations, edited base/LoRA tensor count, forward state identity and time. **Stop:** nonfinite gradient, tokenizer mismatch, no source-authorized pair. **No target percentage promised**. **Run status:** NOT_RUN.

## 3. Requirements coverage — don't lose the other possible gaps

The table below is a **work routing map** into already collected detailed evidence, not false claims of newly reproduced regressions:

| Family | Existing source/evidence in possibleDifs | Domain and falsifier type |
|---|---|---|
| DPO chosen/rejected construction, teacher attribution, scoring/data admission | [02 DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [37 per-side teacher ID](37-NEW-PER-SIDE-DPO-TEACHER-MODEL-ID-LOSS-2026-10-16.md), [38 conditional DPO](38-NEW-RP01-INPUT-INSENSITIVITY-COUNTERFACTUAL-DPO-2026-10-16.md) | C# and translation; wrong answer must win/lose depending on prompt; provenance per side |
| LoRA trained projection/backprop, frozen base and answer path | [03 LoRA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [35 chain](35-RP01-ALL-RECIPIENTS-CAUSAL-PRIORITY-MAP-2026-10-16.md), [core gradient 41](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/41-current-dpo-gradient-chain-source-review.md) | C#/ENRU; finite difference C then L; counterfactual output not raw margin |
| Vector/BPE/token coordinate identity, embedding transport | [21 BPE](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md), [35 vector pooling](35-NEW-EMBEDDING-POOL-NORMALIZE-TRAINABILITY-CUT-2026-10-16.md) | per domain encoder fingerprint and cosine neighborhood stability |
| kNN topk, dynamic semantics, AST vs word-order, domain laws | [04 kNN](04-KNN-HYBRID-RETRIEVAL.md), [12 matrix rehabilitation](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [16 correction](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md), [38 fusion](38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md) | code AST only code; order only language; first-hit vs top-k and verified law |
| DPO→subunits→flow, hidden deterministic rules | [05 units](05-UNITS-FLOW-GRAPH-DETERMINISM.md), [21 structured gaps](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [38 T35 ignored context](38-NEW-T35-UNIT-INPUT-ERASURE-AND-CONFIDENCE-2026-10-16.md) | mixed; input counterfactual, unique updated recipient, actual normal consumer |
| Restore/replay/feedback/ordinary ProductHost | [06 checkpoint](06-CHECKPOINT-RESTORE-ADOPTION.md), [18 ProductHost](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), [38 replay](38-NEW-REPLAY-FUSION-LEXICAL-SHADOW-PATTERN-ALIAS-AND-SHORTLIST-GATE-2026-10-16.md) | state before-after-cold boot; heldout effect / mixed domain forgetting |
| Historical positives vs "failed matrix" misreads | [11 branches](11-BRANCH-ARCHAEOLOGY-AND-LEARNING-PROOFS.md), [12 matrix](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [38 master](38-MASTER-PRIORITIES-POSITIVE-HISTORY-NEW-HYPOTHESES-AND-MICRO-DECISIONS-2026-10-16.md) | re-evaluate *same* holdout & candidate pool, not raw PASS labels |

## 4. Acceptance and handoff for a real executor

Run **E42-A** first (fastest evidence of actual restore failure), then **E42-C** (ordinary consumer), then lawful **E42-B**. In parallel source-only E42-D/F may be done without training. Next scientific C#/ENRU run E42-G only after legal data and tokenizer identities are pinned. No full Windows/GitHub CI; target existing native C# minimal harness, no fixed quota of unit tests. A tiny source-logic proxy counts as S only, never quality learning.

Each run must output:

```text
experiment_id / status: NOT_RUN | BLOCKED | RUN_COMPLETE
domain / profile / source_branch_sha / implementation_sha
actual_learned_recipient / real_consumer_path
input_case_ids / train_ids / heldout_ids / source_disjoint
A_state_hash / B_state_hash / tokenizer_hash / admitted_rows
A_response / B_response / substantive correctness rationale
negative_control / edge_case / known confounders
metric_value + units / CPU_wall_time / peak_RSS if measured
verdict: SUPPORT | REFUTE | MIXED | INCONCLUSIVE | BLOCKED
evidence_paths / can_promote (default false) / next smallest probe
```

**Good example:** «Current loader returned `PROVENANCE_REBUILD_REQUIRED` on pinned v1 artifact; old loader succeeded; current normal consumer uses untrained fallback; learned v2 after R17 remained NOT_RUN.»  
**Bad example:** «Index record removed → kNN worse → train 500 pairs → fixed.» Causal chain never tested.  
**Good example:** «EN→RU semantic 5/6 remained identical but reported margin changed 0.03→0 — METRIC_DRIFT_ONLY.» *(illustrative, not measured)*.  
**Bad example:** «Previous release passes more checks, so its LLM learned language better.»

**Plan-review:** each intervention changes one factor; success must be domain-specific and use actual chosen recipient; documented negative result is still useful. No model execution, training, fresh proxy or quality arbitration was performed in this study; every E42 is a work order, not a result. Protected core docs untouched.
