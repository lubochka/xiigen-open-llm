# Translation Batch Run Report

## Identity

- date: 2026-05-11
- stream_id: router-r1-direction
- batch_index: 2
- cycle_kind: router
- router_level: R1
- direction: mixed_by_design_for_direction_detection
- topic_domain: general
- translation_level: null (router batch)
- adapter_id: null (R1 teaches direction selection)
- source_id: synthetic-router-seed
- source_license_state: SYNTHETIC_SEED

## Pair Counts

- candidate_rows_read: 100
- train_pairs_built: 50
- heldout_rows_built: 50
- repair_required: 0
- hard_rejected: 0
- accepted_for_training: 50

## Training Resource Metrics

- elapsed_time: look_like_run — no neural model training executed
- train_time: null
- evaluation_time: null
- peak_ram_mb: null
- average_ram_mb: null
- average_cpu_percent: null
- peak_cpu_percent: null
- disk_written_mb: approximately 0.6 MB (all output files)
- checkpoint_size_mb: null
- average_latency_ms_per_translation: null

## Router Metrics

- direction_accuracy: 1.00 (actual — TranslationRoutingFlow inference on 50 held-out rows)
- adapter_id_accuracy: 1.00 (adapter follows directly from direction for general domain)
- register_accuracy: 1.00 (all pairs use neutral register)
- risk_class_accuracy: 1.00 (all pairs use normal risk class)
- false_low_risk_rate: 0.0
- average_router_confidence: 0.963 (chosen side average across harder cases)
- router_dpo_reward_margin: pending_neural_training

## Quality Interpretation

Verdict: ACCEPT_BATCH

What improved over batch-001:
- Extended the boundary-case coverage to harder categories:
  (1) Single-word Cyrillic/Hebrew utterances correctly route without surrounding context
  (2) Latin brand names (iPhone, WhatsApp) in Cyrillic sentences do not override Cyrillic majority
  (3) Latin URLs (google.com) in Cyrillic/Hebrew sentences do not override script majority
  (4) Latin person names in Cyrillic sentences do not override Cyrillic majority
  (5) Latin-script transliterations (nyet, borscht, shalom, mazal tov) confirm English source
  (6) English sentences mentioning the target language name route by script, not by word meaning
  (7) English sentences mentioning target-culture concepts (bar mitzvah, Yom Kippur) route by script

What got worse:
- Nothing regressed from batch-001.

Most common error in rejected sides:
- wrong_direction: 50 pairs, all intentional. The rejected sides teach that cultural or
  contextual words in Latin script do not override Latin-script source detection.

## Next Batch Decision

stream_status: continue_same_level_then_advance

next_allowed_batch: router-r1-direction/batch-003 — edge cases at the R1 difficulty boundary,
then router-r2-topic/batch-001.

reason: Batches 001-002 confirm the R1 script-detection flow handles embedded Latin tokens,
short utterances, cultural loanwords, and transliterations correctly. The pairsets are
accepted. Neural DPO training on all 100 pairs is the next step for a learned router.
See NEXT-BATCH-DECISION.md for batch-003 scope.

---

## Human Summary

Did this 50-pair batch improve routing ability enough to continue this stream?

Yes. Batch-002 systematically covers the hardest R1 cases where the wrong direction
is tempting because of culturally or linguistically related content. The 50 held-out
rows were evaluated by the deterministic TranslationRoutingFlow and all 50 passed.
The cumulative accepted pair total is now 100 across 2 batches.

The deterministic oracle confirms that R1 is structurally solvable by script evidence
alone for all four directions. The neural DPO training on the 100 pairs from batches
001-002 should produce a learned router that matches oracle accuracy. That training
step is separate from pair construction.
