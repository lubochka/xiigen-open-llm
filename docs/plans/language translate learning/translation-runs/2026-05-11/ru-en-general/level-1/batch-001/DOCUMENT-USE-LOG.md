# Document Use Log

## Step 0 - Prior State
- run directory inspected: translation-runs/ — does not exist prior to this session
- latest stream: none (first run)
- latest accepted batch: none
- decision source file: none

## Step 1 - Rationale
- document: TRANSLATION-SMALL-MODEL-RATIONALE-v1.md
- used for: understanding why 50-pair batches, routing flow, adapters, and level gates are necessary for small model training
- not used for: overriding source gates, DPO gates, or evaluation gates

## Step 2 - Source Registry
- document: TRANSLATION-SOURCE-REGISTRY-v3.md
- source_id: synthetic-seed-tatoeba-style (model-authored seed pairs, Tatoeba-style content)
- source decision: SYNTHETIC_SEED — not in ACCEPT registry; chosen-side authority not verified by parallel corpus
- attribution required: N/A (synthetic)
- chosen-side authority allowed: no (synthetic seed only)

## Step 3 - Routing Architecture
- document: TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md
- stream_id: ru-en-general-l1
- cycle_kind: translator (routing flow not yet trained; R1 gate not yet passed)
- required routing gate: pending — first translator batch run without prior router gate; documented in RUNNER-CONTRACT.md

## Step 4 - Router DPO Guidance
- document: TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md
- used: no (translator batch, not router batch)
- router level: not applicable

## Step 5 - Four-Level Training Plan
- document: TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md
- translation level: 1
- level pair mix: 35 T01 + 5 T08 + 5 T09 + 5 corruption (actual: 27 T01 + 7 T08 + 5 T09 + 5 T04 + 6 T05)
- level promotion rule: DPO margin > 0, acc ≥ 0.60, target-script ≥ 98%, copied-source ≤ 2%

## Step 6 - 50-Pair Runbook
- document: TRANSLATION-50-PAIR-ABLATION-RUNBOOK-v1.md
- output directory: translation-runs/2026-05-11/ru-en-general/level-1/batch-001/
- train count target: 50 (achieved: 50)
- held-out count target: 50 (achieved: 50)

## Step 7 - DPO Registry
- document: TRANSLATION-DPO-REGISTRY-GUIDE-v1.md
- pair families used: T01 (core), T04 (idiom), T05 (false cognate), T08 (completeness), T09 (named entity), CORR (corruption negatives)

## Step 8 - Examination Method
- document: EXAMINATION-METHOD-TRANSLATION-v1.md
- row qualification rules applied: identity gate, mojibake gate, [WRONG:] annotation gate, overlap ratio gate (threshold 0.50), direction gate, source licence gate

## Step 9 - Rejected-Side Prompts
- document: TRANSLATION-DPO-REJECTED-PROMPTS-v1.md
- rejected audit method: all rejected sides constructed with one primary error type and functional consequence in [WRONG:] annotation

## Step 10 - Tokenizer Guide
- document: TRANSLATION-TOKENIZER-GUIDE-v1.md
- tokenizer status: BPE trained on 3000 EN+RU+HE frequency words + 50 pair content. Coverage test against FLORES-200 not run (corpus not available). Cyrillic/Hebrew coverage target (≥95%/≥90%) unverified — noted as pending in RUNNER-CONTRACT.md.

## Step 11 - Look-Like Report Template
- document: TRANSLATION-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md
- report file: LOOK-LIKE-RUN-REPORT.md

## Historical Documents
- documents read: TRANSLATION-SOURCE-REGISTRY-v1.md, TRANSLATION-SOURCE-REGISTRY-v2.md, TRANSLATION-RUNBOOK-v1.md, TRANSLATION-TRAINING-PLAN-v1.md
- conflicts found: none (v3 registry and current executor prompt win over all historical docs)
- current document that wins: TRANSLATION-SOURCE-REGISTRY-v3.md, EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md
