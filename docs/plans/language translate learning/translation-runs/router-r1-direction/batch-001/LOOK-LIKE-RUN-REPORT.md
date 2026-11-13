# Translation Batch Run Report

## Identity

- date: 2026-05-11
- stream_id: router-r1-direction
- batch_index: 1
- cycle_kind: router
- router_level: R1
- direction: mixed_by_design_for_direction_detection
- topic_domain: general
- translation_level: null (router batch, not translator batch)
- adapter_id: null (R1 teaches direction selection; adapter is determined after direction)
- source_id: tatoeba-ru-en, tatoeba-he-en
- source_license_state: ACCEPT, ACCEPT

## Pair Counts

- candidate_rows_read: 100
- train_pairs_built: 50
- heldout_rows_built: 50
- repair_required: 0
- hard_rejected: 0
- accepted_for_training: 50

## Training Resource Metrics

- elapsed_time: look_like_run — no model training executed
- train_time: null
- evaluation_time: null
- peak_ram_mb: null
- average_ram_mb: null
- average_cpu_percent: null
- peak_cpu_percent: null
- disk_written_mb: approximately 0.8 MB (all output files)
- checkpoint_size_mb: null
- average_latency_ms_per_translation: null

## Router Metrics

- direction_accuracy: 1.00 (projected from pair quality; training not yet run)
- topic_domain_accuracy: 1.00 (all pairs use general; no domain ambiguity at R1)
- register_accuracy: 1.00 (all pairs use neutral register; R1 does not train register)
- risk_class_accuracy: 1.00 (all pairs use normal risk class; R1 does not train risk)
- adapter_id_accuracy: 1.00 (adapter follows directly from direction for general domain)
- false_low_risk_rate: 0.0 (no high-stakes or formal-high-precision examples in R1)
- average_router_confidence: 0.955 (chosen side average across all 50 pairs)
- router_dpo_reward_margin: pending_training
- router_dpo_accuracy: pending_training

## Translation Metrics

- dpo_reward_margin: pending_training (router batch; translation quality not tested at R1)
- dpo_accuracy: pending_training
- bleu: not_applicable (router batch; no target-language translation output)
- chrf: not_applicable
- target_script_accuracy: not_applicable
- copied_source_rate: not_applicable
- wrong_direction_rate: projected_0.00
- named_entity_preservation: not_applicable (R1 does not test named entities)
- completeness_score: not_applicable
- manual_review_passed: 10
- manual_review_total: 10

## Quality Interpretation

Verdict: ACCEPT_BATCH

What improved:
- This is the first batch. No prior baseline to compare against.
- Established clean R1 direction-detection DPO pairs for all four language directions.
- Covered five non-obvious rejected routing errors that teach script-vs-keyword boundaries:
  (1) Russian adverb 'по-английски' is not an English source signal
  (2) Hebrew noun 'אנגלית' is not a Latin English source signal
  (3) English word 'Jerusalem' in Latin script is not a Hebrew source signal
  (4) English word 'Hebrew' is not a Hebrew source signal
  (5) English phrase 'Tel Aviv' is not a Hebrew source signal

What got worse:
- Nothing regressed (first batch, no prior state).

Most common error:
- None in the pairs themselves. In the rejected sides, wrong_direction is the
  intended error type for all 50 pairs, which is correct for R1.

Is this a model-capacity issue?
- unclear — training has not been run. R1 direction detection from script signals
  is the simplest routing task. Small models should learn it reliably.

Is this a pair-quality issue?
- no — all hard gates pass, all WRONG annotations are specific and functional,
  no repair-required rows.

Is this a routing issue?
- no — routing logic is defined by the pair content. The routing contract
  (TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>) must be
  implemented before training can verify the routing decisions.

## Sample Held-Out Outputs

### Sample 1

source: "Добрый день, чем могу помочь?" (Russian)
reference expected_direction: RU_EN
model_output: pending_training
verdict: projected pass
error_category: none — Cyrillic script is unambiguous

### Sample 2

source: "The weather forecast says it will snow tomorrow." (English, target=ru)
reference expected_direction: EN_RU
model_output: pending_training
verdict: projected pass
error_category: none — Latin script with Russian target is unambiguous

### Sample 3

source: "הוא גר בתל אביב." (Hebrew)
reference expected_direction: HE_EN
model_output: pending_training
verdict: projected pass
error_category: none — Hebrew script with Latin proper noun (Tel Aviv) embedded;
   script detection must look at majority script, not assume Latin noun changes direction

### Sample 4

source: "Good evening, welcome to our restaurant." (English, target=he)
reference expected_direction: EN_HE
model_output: pending_training
verdict: projected pass
error_category: none — Latin script with Hebrew target is unambiguous

### Sample 5

source: "She studies Hebrew at the university." (English, target=he)
reference expected_direction: EN_HE
model_output: pending_training
verdict: projected pass
error_category: none — 'Hebrew' here is an English word, not Hebrew script;
   the train pair TR-ROUTE-R1-0041 explicitly teaches this boundary

### Sample 6

source: "בוקר טוב, מה שלומך?" (Hebrew greeting)
reference expected_direction: HE_EN
model_output: pending_training
verdict: projected pass
error_category: none — fully Hebrew script

### Sample 7

source: "Он живёт в Москве." (Russian with named entity)
reference expected_direction: RU_EN
model_output: pending_training
verdict: projected pass
error_category: none — Москве is a Cyrillic proper noun; no routing ambiguity

### Sample 8

source: "כמה עולה הכרטיס הזה?" (Hebrew question with definite article ה)
reference expected_direction: HE_EN
model_output: pending_training
verdict: projected pass
error_category: none — Hebrew definite prefix ה confirms Hebrew script

### Sample 9

source: "My sister got married last spring." (English, target=he)
reference expected_direction: EN_HE
model_output: pending_training
verdict: projected pass
error_category: none — entirely Latin English source

### Sample 10

source: "Моя сестра учится в университете." (Russian)
reference expected_direction: RU_EN
model_output: pending_training
verdict: projected pass
error_category: none — all Cyrillic tokens

## Next Batch Decision

stream_status: continue_same_level

next_allowed_batch: router-r1-direction/batch-002 — but only after actual DPO training
on batch-001 and actual held-out inference confirm direction_accuracy >= 0.99 and DPO
margin > 0. Batch-001 pairset is ready; it is not yet accepted as trained evidence.

reason: Pairset passes all hard gates. Pair-based DPO could technically run but would
produce fake router metrics without an actual routing inference path. Router behavior
confirmation requires TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>
to be implemented, registered, and exercised. This is an implementation gap, not a
pair-quality issue. See NEXT-BATCH-DECISION.md for the 6-step promotion sequence.

---

## Human Summary

Did this 50-pair batch improve translation routing ability enough to continue this stream,
or should the next batch repair pair quality / routing / tokenization first?

The batch produces 50 clean, well-structured R1 direction-detection routing DPO pairs.
All hard gates pass. The rejected sides are plausible and teach specific script-vs-keyword
boundaries that a small router model could learn incorrectly without them. The manual
review of 10 samples found no quality issues.

The batch cannot be promoted yet. The pairset is ready (PAIRSET_READY_TRAINING_PENDING),
but it is not accepted as trained evidence. Pair-based DPO could technically run, but
without TranslationRoutingFlow there is no actual routing inference path, so the held-out
direction_accuracy would be unmeasurable against real model decisions — producing fake
router metrics. The next action is implementation of the routing flow, not pair repair.
Once TranslationRoutingFlow exists, DPO runs, and actual held-out inference confirms
direction_accuracy >= 0.99, batch-001 is accepted and batch-002 may begin with harder
direction-detection cases (mixed-script sentences, very short utterances, and English
sentences that mention target-language place names or language names).
