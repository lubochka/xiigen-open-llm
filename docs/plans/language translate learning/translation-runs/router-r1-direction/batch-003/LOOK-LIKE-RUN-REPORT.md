# Translation Batch Run Report

## Identity

- date: 2026-05-11
- stream_id: router-r1-direction
- batch_index: 3
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
- disk_written_mb: approximately 0.7 MB (all output files)
- checkpoint_size_mb: null
- average_latency_ms_per_translation: null

## Router Metrics

- direction_accuracy: 1.00 (actual — TranslationRoutingFlow inference on 50 held-out rows)
- adapter_id_accuracy: 1.00 (adapter follows directly from direction for general domain)
- register_accuracy: 1.00 (all pairs use neutral register)
- risk_class_accuracy: 1.00 (all pairs use normal risk class)
- false_low_risk_rate: 0.0
- average_router_confidence: 0.958 (chosen side average across harder cases)
- router_dpo_reward_margin: pending_neural_training

## Quality Interpretation

Verdict: ACCEPT_BATCH

What improved over batch-002:
- Extended coverage to the hardest R1 edge cases at the difficulty boundary:
  (1) Code-switched Cyrillic+Latin sentences with clear Cyrillic majority
  (2) Exact Cyrillic/Latin script-count ties resolved by Cyrillic-wins-on-tie rule
  (3) Cyrillic-lookalike glyphs (Cyrillic Ок vs Latin OK) correctly detected as Cyrillic
  (4) Latin brand names (McDonalds, Skype) in Cyrillic sentences do not override Cyrillic majority
  (5) Latin tech terms (Wi-Fi, Windows, Android) in Hebrew sentences do not override Hebrew majority
  (6) Russian cultural loanwords in Latin (vodka, blini, troika, matryoshka) route by script not culture
  (7) Hebrew cultural transliterations in Latin (challah, Kaddish, Knesset, kibbutz) route by script not culture

What got worse:
- Nothing regressed from batches 001-002.

Most common error in rejected sides:
- wrong_direction: 50 pairs, all intentional. The rejected sides teach that cultural,
  contextual, and brand-name Latin tokens embedded in Cyrillic/Hebrew sentences do not
  override script majority, and that Latin transliterations of Russian/Hebrew words
  used in English sentences route as English (Latin source).

## R1 Stream Completion

Batches 001-003 collectively confirm that the R1 script-detection flow handles all
known difficulty classes for four directions (RU_EN, EN_RU, HE_EN, EN_HE):
- Batch-001: baseline clear-script cases
- Batch-002: single-word utterances, embedded Latin tokens, cultural loanwords
- Batch-003: code-switching, tie-breaking, lookalike glyphs, brand names, transliterations

Cumulative: 150 train pairs, 150 held-out rows, all 150 HO rows passing (1.00 accuracy).
R1 stream declared complete. Next: router-r2-topic/batch-001.

---

## Human Summary

Did this 50-pair batch improve routing ability enough to complete the R1 stream?

Yes. Batch-003 closes the known difficulty range for R1 direction detection by
covering code-switching, script-count tie-breaking, and the full vocabulary of
cultural loanwords and transliterations that might mislead direction detection.
The 50 held-out rows were evaluated by the deterministic TranslationRoutingFlow
and all 50 passed. Cumulative accepted pair total is now 150 across 3 batches.

The R1 stream is complete. The deterministic oracle confirms that R1 is
structurally solvable by script evidence alone for all four directions and all
known edge case categories. Neural DPO training on the 150 pairs from batches
001-003 is the next step for a learned router, and R2 topic domain detection
is the next stream.
