# Translation Batch Run Report

## Identity

- date: 2026-05-11
- stream_id: router-r2-topic
- batch_index: 1
- cycle_kind: router
- router_level: R2
- direction: mixed_by_design_for_topic_detection
- topic_domain: all_six (medical, legal, technical, cultural, personal, general)
- translation_level: null (router batch)
- adapter_id: null (R2 teaches topic domain selection)
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
- disk_written_mb: approximately 0.1 MB (all output files)
- checkpoint_size_mb: null
- average_latency_ms_per_translation: null

## Router Metrics

- topic_accuracy: 1.00 (actual — TranslationTopicRoutingFlowTests.R2Batch001_HeldOut50_TopicAccuracyMeetsGate on 50 held-out rows)
- adapter_id_accuracy: 1.00 (adapter follows directly from direction+topic)
- average_router_confidence: 0.92 (mix of 1-match=0.85, 2-match=0.92, 3+-match=0.97, general=0.90)
- router_dpo_reward_margin: pending_neural_training

## Quality Interpretation

Verdict: ACCEPT_BATCH

What this batch established:
1. Baseline keyword detection for all 6 topic domains across all 4 directions
2. Medical domain: врач/больниц/лекарств (RU), physician/patient/surgery (EN), רופא/תרופ/טיפול (HE)
3. Legal domain: суд/адвокат/договор (RU), defendant/court/contract (EN), משפט/נאשם/חוק (HE)
4. Technical domain: сервер/функци/программ (RU), api/server/algorithm (EN), קוד/מערכת/שרת (HE)
5. Cultural domain: традиц/музык/народ (RU), museum/ancient/heritage (EN), מסורת/תרבות/מוזיאון (HE)
6. Personal domain: дорогой/скуча/семья (RU), dear/love/family (EN), לב/גירושים (HE)
7. General domain: no-keyword default confirmed for all 4 directions

What keyword issues were fixed in TranslationTopicRoutingFlow before this batch:
- "heart" removed from MedicalKeywordsLatin (caused false medical wins over personal)
- "сердце" removed from MedicalKeywordsCyrillic (same tie-break issue)
- "скучать" changed to "скуча" stem (catches скучаю, скучать, скучает inflections)
- "мама" changed to "мам" stem (catches маме, маму, мамы inflections)
- "art" changed to "artwork" in CulturalKeywordsLatin (was matching "heart" and "departs")

## Human Summary

Batch-001 opens the R2 topic detection stream. The deterministic TranslationTopicRoutingFlow
correctly classified all 50 held-out rows (1.00 accuracy). The keyword fix pass preceding
this batch resolved 4 cross-domain contamination issues: "heart"/"сердце" in both medical
and personal arrays, "скучать" not matching inflected форм "скучаю", and "art" appearing
as a false substring in "heart" and "departs".

R2 batch-001 is complete. Proceed to batch-002 to cover harder boundary cases.
