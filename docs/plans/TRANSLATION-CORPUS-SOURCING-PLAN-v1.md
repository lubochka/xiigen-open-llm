# TRANSLATION-CORPUS-SOURCING-PLAN-v1.md
# TranslationDomain — Training Corpus Sourcing and Base-Block Architecture

Version: 1.0
Date: 2026-05-29
Status: ACTIVE

---

## 1. Goal

Build a legally-cleared, commercially-usable parallel corpus for EN↔RU translation
training that:

1. Covers the **3 000 most frequent words** in each language (English and Russian).
2. Provides **10 representative sentences** per word — each sentence using the word
   in a structurally distinct grammatical role.
3. Produces a **Base Trained Model** (general EN↔RU) that all professional domains
   inherit through fine-tuning, not from scratch.
4. Feeds the **DPO (Direct Preference Optimization) pipeline** for the three
   trainable units: `SemanticTokenizerUnit`, `ExecutorUnit`, `QualityVerifierUnit`.

---

## 2. Domain Architecture: Base Block → Copy → Fine-Tune

```
Source Whitelist (§4)
        ↓
Word Frequency Lists   (§5)   — 3 000 words × 2 languages
        ↓
Sentence Selection     (§6)   — 10 sentences per word, structural diversity
        ↓
DPO Pair Construction  (§7)   — 3 unit examples per sentence pair
        ↓
┌─────────────────────────────────────┐
│  BASE TRAINED MODEL (general EN↔RU) │  ← trained on 60 000 base pairs
└─────────────┬───────────────────────┘
              │  copy weights + fine-tune on domain shard
     ┌────────┼──────────┬──────────────┐
     ▼        ▼          ▼              ▼
  Legal    Literary   Medical       Technical
  (UN)   (Gutenberg)  (PMC OA)    (GNOME/KDE)
```

**Key rule:** A new professional domain is never trained from scratch.
It starts from Base Model weights and adds a domain-specific DPO shard.
The base shard is the same across all domains.

---

## 3. Volume Estimate

| Component                        | Count      |
|----------------------------------|------------|
| Words per language               | 3 000      |
| Sentences per word               | 10         |
| Sentence pairs — EN→RU           | 30 000     |
| Sentence pairs — RU→EN           | 30 000     |
| Base sentence pairs total        | 60 000     |
| DPO examples per pair (3 units)  | × 3        |
| **Total DPO training examples**  | **180 000** |
| Held-out per direction (20 %)    | 6 000      |
| Training per direction (80 %)    | 24 000     |

---

## 4. Source Whitelist — Legal-Cleared for Commercial Use

All sources below are usable for commercial LLM training without known legal
risk. Sources with NC (Non-Commercial) licenses or unclear rights are excluded.

### 4A. UN Parallel Corpus — EN↔RU
**License:** freely available under a liberal license (ACL Anthology L16-1561).
**Domain:** law, international relations, official correspondence, economics.
**Register:** formal.
**Use:** formal English ↔ formal Russian; diplomatic and legal style.
**Priority:** first.

### 4B. Project Gutenberg — English Public Domain
**License:** public domain in the US.
**Domain:** literature, science, philosophy, economics (pre-1928).
**Register:** literary / classical.
**Use:** English originals. Russian translation paired from Wikisource PD only.
**URL:** gutenberg.org

### 4C. Standard Ebooks — Clean English Editions
**License:** CC0 1.0 Universal Public Domain Dedication (standardebooks.org/about).
**Domain:** literature, essays.
**Use:** structurally clean English text (less OCR noise than Gutenberg).

### 4D. Wikisource / Викитека — Russian Public Domain
**License:** public domain only (check translator death year per jurisdiction).
**Domain:** literature, history, law, old translations.
**Rule:** include only pages with explicit PD status.
         Verify translator death year before including any translation.
**Paired with:** 4B (Gutenberg EN) for literary EN↔RU alignment.

### 4E. Tatoeba — Short EN↔RU Sentence Pairs
**License:** CC BY 2.0 FR.
**Attribution:** required (attribution manifest must be stored).
**Domain:** general language, everyday usage, grammar patterns.
**Use:** primary source for 10 sentences per word in the base block.
        Short (5–15 words), human-translated, structurally varied.
**Download:** downloads.tatoeba.org/exports/

### 4F. PMC Open Access Subset — Biomedical English
**License:** CC0 / CC BY only. Exclude: CC BY-NC, CC BY-NC-SA, Other.
**Domain:** medicine, biology, life sciences.
**Use:** English-only biomedical domain shard (medical professional domain).
**Note:** not a parallel EN↔RU source; use for English medical terminology.
**Rule:** filter to "Commercial Use Allowed" bucket only.

### 4G. GNOME / KDE / Ubuntu Localization
**License:** project-specific open licenses (LGPL, GPL, CC BY-SA — verify per project).
**Domain:** UI strings, commands, error messages, system notifications.
**Use:** technical EN↔RU translation (software/technical professional domain).
**Through:** OPUS (opus.nlpl.eu) whitelist subsets only.

### 4H. OPUS Whitelist Subsets
**Permitted subsets:** UNPC, Tatoeba, GNOME, KDE (after license check),
                       Ubuntu/Debian localization (after license check),
                       Wikimedia-related (CC BY-SA — separate shard).
**Prohibited subsets:** OpenSubtitles, ParaCrawl, CCMatrix, CCAligned,
                        noisy web-crawl corpora, fan translations, subtitles.

---

## 5. Word Frequency Pipeline

### 5A. English — Top 3 000

| Source | Notes |
|--------|-------|
| COCA / wordfrequency.info | REVIEW_REQUIRED: paid/proprietary source; do not use for commercial ML training until license clearance explicitly allows it |
| Oxford 3 000 | REVIEW_REQUIRED: proprietary Oxford source; method reference only unless licensed for commercial ML training |
| General Service List (West 1953) | ACCEPT: public-domain core seed list |

Filter: keep nouns, verbs, adjectives, adverbs.
Remove: articles, conjunctions, prepositions, numerals, proper nouns.
Output lemma form only.

Only sources with explicit commercial-training clearance may feed `build-word-list`. REVIEW_REQUIRED rows are allowed as human planning references only; they must not enter `word-list-*-pilot-100.jsonl` or `word-list-*-3000.jsonl` until cleared.

### 5B. Russian — Top 3 000

| Source | Notes |
|--------|-------|
| Ляшевская & Шаров, 2009 (ruscorpora.ru) | lemmatized frequency dict |
| OpenCorpora (opencorpora.org) | ACCEPT_SUPPORT_ONLY / separate shard: morphology evidence with attribution/share-alike review |
| НКРЯ (Russian National Corpus) top lists | real-language frequency |

Filter: same as 5A.
Output lemma form only.

### 5C. Storage

```
27.TranslationDomain/SourceData/word-list-en-3000.jsonl
27.TranslationDomain/SourceData/word-list-ru-3000.jsonl
```

Format:
```json
{ "word": "run", "lemma": "run", "pos": "VERB", "rank": 42 }
{ "word": "идти", "lemma": "идти", "pos": "VERB", "rank": 17 }
```

These files are stored in git (no private content — just word lists).

---

## 6. Sentence Selection Pipeline

### 6A. Primary Source: Tatoeba

**Input:**
- `sentences_en.tsv` — sentence_id, lang, text
- `sentences_ru.tsv` — sentence_id, lang, text
- `links.csv` — alignment between EN and RU sentences

**Algorithm:**
1. Lemmatize all English and Russian sentences (simple rule-based or dictionary).
2. For each word in the top-3 000 list, find all sentences containing that lemma.
3. Compute the POS-bigram structural signature for each sentence.
4. Select 10 sentences per word with **maximum structural diversity**:
   - Sort candidates by how different their POS-bigram pattern is from already-selected sentences.
   - Prefer sentences where the target word appears in different grammatical roles
     (subject, object, predicate, modifier).
5. Record Tatoeba sentence IDs (not the text) in the source index.

**Minimum diversity rule:** among 10 selected sentences for one word,
at least 5 must have different POS-bigram patterns.

### 6B. Fallback: UN Parallel Corpus and OPUS whitelist

Used when Tatoeba has fewer than 10 sentences for a word:
1. Search UN Parallel Corpus (formal register) for sentences containing the word.
2. Apply the same structural diversity selection.
3. Merge with Tatoeba results, deduplicate by structural signature.

### 6C. Quality Gates per Sentence

A sentence is included only if:

| Criterion | Rule |
|-----------|------|
| Length | 4–18 source words |
| Target word role | semantically load-bearing (not just mentioned) |
| Translation quality | human-translated (Tatoeba, UN — not MT) |
| No proper nouns in key position | proper nouns do not teach structural patterns |
| Neutral register | no offensive or highly informal content |
| Source ID available | can be attributed |

### 6D. Source Index Storage (git)

```
27.TranslationDomain/SourceData/sentence-index-en-ru.jsonl
27.TranslationDomain/SourceData/sentence-index-ru-en.jsonl
```

Format:
```json
{
  "word": "run",
  "pos": "VERB",
  "source_id": "tatoeba_123456",
  "source_url": "https://tatoeba.org/en/sentences/show/123456",
  "license": "cc_by_2fr",
  "pattern": "SUBJECT|VERB:execute|OBJECT",
  "direction": "en_ru"
}
```

The actual sentence text is a **private artifact** (see §9).

---

## 7. DPO Pair Construction

Each sentence pair `(source_text, chosen_translation)` produces
3 unit-specific DPO training examples.

### 7A. SemanticTokenizerUnit

Teaches: correct structural signature for the input pattern.

```json
{
  "unit": "SemanticTokenizerUnit",
  "input": "I need to run this script",
  "chosen_signature": "SUBJECT|MODAL|VERB:execute|OBJECT:script",
  "rejected_signature": "SUBJECT|VERB:move|ADVERB:fast",
  "wrong_annotation": "[WRONG: treats 'run' as physical movement - misses technical context - loses execution semantics]"
}
```

### 7B. ExecutorUnit

Teaches: correct word choice and word order given the structural context.

```json
{
  "unit": "ExecutorUnit",
  "input": "I need to run this script",
  "chosen": "Мне нужно запустить этот скрипт",
  "rejected": "Мне нужно бежать этот скрипт",
  "word_mappings": [
    { "source": "run", "target": "запустить", "concept": "EXECUTE" }
  ],
  "wrong_annotation": "[WRONG: 'run' → 'бежать' ignores EXECUTE context - physical verb in technical phrase - wrong register]"
}
```

### 7C. QualityVerifierUnit

Teaches: what terms are required for a translation of this structural pattern
to pass quality check.

```json
{
  "unit": "QualityVerifierUnit",
  "candidate": "Мне нужно запустить этот скрипт",
  "signature": "SUBJECT|MODAL|VERB:execute|OBJECT:script",
  "chosen_verdict": "PASS",
  "rejected_candidate": "Мне нужно бежать этот скрипт",
  "rejected_verdict": "FAIL",
  "required_terms": ["запустить", "скрипт"],
  "wrong_annotation": "[WRONG: 'бежать' not in required terms for EXECUTE concept - fails term check]"
}
```

### 7D. Rejected Variant Construction Rules

The rejected variant must differ from chosen in **exactly one conceptual decision**:
- Wrong word sense (physical vs technical, formal vs informal)
- Wrong word order (SOV instead of SVO, or VALIDATE_CHECK+NULL_LITERAL → "check null" instead of "null check")
- Wrong register (literal → idiomatic or vice versa)
- Missing required term (drops a domain-specific word)

Do not construct rejected variants by random word substitution.
Every `[WRONG: weakness - gap - consequence]` annotation must name all three parts.

---

## 8. Domain Expansion Pattern

### 8A. Base Block

- Trained on general 60 000 pairs (3 000 words × 10 sentences × 2 directions).
- Checkpoint exported through `IModelRepository` after base training.
- Manifest stored in `27.TranslationDomain/Manifests/base-general-en-ru-v1.json`.

### 8B. Professional Domain Shards

Each domain shard is built from domain-specific sources and uses the same
DPO format as the base block. Fine-tuning starts from base checkpoint.

| Domain | Primary Source | Sentences per word | Additional words |
|--------|---------------|-------------------|-----------------|
| Legal | UN Parallel Corpus | 10 | +500 legal terms |
| Literary | Gutenberg + Wikisource PD | 10 | +300 literary terms |
| Medical | PMC OA CC0/CC BY + UN health docs | 10 | +500 medical terms |
| Technical | GNOME/KDE/Ubuntu + OPUS whitelist | 10 | +500 tech terms |

### 8C. Copy-and-Fine-Tune Procedure

```
1. Load base checkpoint from IModelRepository using base manifest.
2. Construct domain shard DPO pairs (same 3-unit format).
3. Run TrainBatchAsync with domain shard pairs (starting from base weights).
4. Export domain checkpoint with domain manifest.
5. Runner validates: domain-specific held-out accuracy ≥ 0.95.
6. Register domain checkpoint through IRemoteUnitLocator.
```

---

## 9. Storage: Git vs Private Artifact

| Artifact | Location | Why |
|----------|----------|-----|
| `word-list-en-3000.jsonl` | git | just word lists, no private content |
| `word-list-ru-3000.jsonl` | git | same |
| `sentence-index-en-ru.jsonl` | git | source IDs only, no actual text |
| `sentence-index-ru-en.jsonl` | git | same |
| Source metadata schema | git | provenance records |
| Raw sentence text + translations | **private artifact** | actual corpus text |
| DPO training JSONL batches | **private artifact** | chosen/rejected pairs |
| Held-out JSONL (prompts only, no answers) | git | only `input_text`, no `expected_translation` |
| Runner reports (numeric metrics only) | git | accuracy, BLEU scores, counts |
| Manifests + checkpoint references | git | no actual weights |
| Model checkpoint weights | **private artifact** | binary, not git-suitable |

Attribution manifest for CC BY sources (Tatoeba, etc.) must be stored
alongside the private artifact with the actual sentence text.

---

## 10. Source Metadata Schema

Every raw sentence file must record its source:

```json
{
  "source_id": "tatoeba",
  "source_url": "https://tatoeba.org",
  "title": "Tatoeba parallel corpus",
  "author": "Tatoeba contributors",
  "translator": null,
  "author_death_year": null,
  "translator_death_year": null,
  "language": "en",
  "paired_language": "ru",
  "domain": "general",
  "license": "cc_by_2fr",
  "commercial_allowed": true,
  "attribution_required": true,
  "share_alike": false,
  "no_derivatives": false,
  "source_quality": "high",
  "alignment_level": "sentence",
  "allowed_for_training": true,
  "allowed_for_distribution": false,
  "notes": "Attribution required. Text is derived work — private artifact only."
}
```

---

## 11. Excluded Sources

| Source | Reason |
|--------|--------|
| OpenSubtitles | rights unclear, fan/auto translations |
| ParaCrawl, CCMatrix, CCAligned | noisy web-crawl, no provenance |
| Nabokov | died 1977, rights not expired |
| Orwell | died 1950, jurisdiction-dependent |
| Hemingway | died 1961, rights not expired |
| Bulgakov | died 1940, modern editions risky |
| Modern Russian translations of classics | separate copyright work |
| Fan translations | unknown rights |
| CC BY-NC anything | non-commercial only |
| CC BY-NC-SA anything | non-commercial only |
| CC BY-NC-ND anything | non-commercial only |
| "Educational use only" | not commercial |
| No license / unknown license | excluded by default |
| lib.ru and similar pirate libraries | excluded |

---

## 12. Implementation Phases

| Phase | Task | Output | Gate |
|-------|------|--------|------|
| P1 | Download and lemmatize EN top-3000 frequency list | `word-list-en-3000.jsonl` | 3 000 lemma entries |
| P2 | Download and lemmatize RU top-3000 frequency list | `word-list-ru-3000.jsonl` | 3 000 lemma entries |
| P3 | Download Tatoeba EN↔RU dump, build search index | local index | search returns results for ≥ 90% of words |
| P4 | For each word: select 10 sentences, diversity-scored | `sentence-index-*.jsonl` | 10 per word, ≥ 5 distinct POS-bigram patterns |
| P5 | Construct rejected variants per DPO guide | DPO raw JSONL | every pair has `[WRONG:]` annotation |
| P6 | Split 80/20 train/held-out (different sentences) | train JSONL + held-out prompts only | no overlap between train and held-out |
| P7 | Batch into 50-pair JSONL batches | `dpo-train-*-batch-NNN.jsonl` | batch count = ceil(24000/50) per direction |
| P8 | Start incremental training loop (§13) on base corpus | loop state | first 100-pair test batch accuracy measured |
| P9 | Continue loop until `runtimeProductAccuracy ≥ 0.95` | base checkpoint manifest | base gate passed |
| P10 | Domain shard: Legal — start domain loop (§13.C) | legal-en-ru checkpoint | domain gate ≥ 0.95 |
| P11 | Domain shard: Literary — domain loop | literary-en-ru checkpoint | same gate |
| P12 | Domain shard: Medical — domain loop | medical-en-ru checkpoint | same gate |
| P13 | Domain shard: Technical — domain loop | technical-en-ru checkpoint | same gate |

---

## 13. Incremental Training Loop (100 + 100 pattern)

This section defines the core training method for both the base model and
all domain models. The loop ensures the model is tested on genuinely unseen
sentences at every step — not on sentences it was trained on.

### 13A. Non-Stop Execution Law

The base language domain is not considered ready until the general EN<->RU
loop reaches `runtimeProductAccuracy >= 0.95` on a never-seen 100-sentence
test batch. Until that happens:

1. Do not stop the TranslationDomain work because a helper, source builder,
   artifact, report, or checkpoint was created.
2. Do not start professional domain fine-tuning as a replacement for the base
   gate. Domain training starts only after the general-language base passes.
3. Do not report "translation ready" from memorized rows, contaminated held-out
   rows, or any test batch that was already converted into training examples.
4. Continue the next 100+100 iteration whenever an approved next corpus batch
   exists.

Here, "100 sets" means 100 source/target sentence-pair sets. Each set is
converted into the three unit-level DPO examples required by the current
TranslationFlow:

```
1 source/target sentence pair
  -> SemanticTokenizerUnit accepted/rejected example
  -> ExecutorUnit accepted/rejected example
  -> QualityVerifierUnit accepted/rejected example

100 sentence-pair sets
  -> up to 300 unit-level DPO examples
```

If the loop is below 0.95, the next action is not a status report. The next
action is: train on the failed test batch, take the following 100 never-seen
sets as the new test batch, and run the runner again.

### 13A.1. Evaluation Set Authority (No Substitute Sets)

The evaluation set is part of the learning task. If Luba names a concrete
pair family, corpus shard, source folder, or prepared set sequence, the runner
must use that exact family for the 100+100 loop.

An unrelated or easier corpus is not a valid substitute for the current gate.
It may be inspected only as a future source candidate, not as evidence that the
current TranslationDomain curriculum reached 95%.

The meaning of "95%" here is: at least 95% of the patterns in the selected
learning curriculum are covered by the trained units and pass on the next
never-seen 100 sets from that same curriculum. It is not a generic score on a
different, convenient dataset.

If the selected curriculum does not yet contain enough approved rows for the
next 100+100 iteration, the correct output is a shortfall report and the next
action is to add approved rows to that same curriculum. Do not replace it with
a synthetic, old, or unrelated batch to make the metric look better.

### 13B. Loop Structure

```
CORPUS (sentence pairs, ordered by structural diversity)
│
├─ Batch N   (100 pairs)  ──► CONVERT to DPO pairs ──► TRAIN 3 units
│                                                          │
│                                                          ▼
├─ Batch N+1 (100 pairs)  ──► TEST: measure accuracy on these 100
│                                          │
│                          ┌──────────────┴──────────────┐
│                          ▼                             ▼
│               accuracy ≥ 0.95                  accuracy < 0.95
│               LOOP EXITS                        │
│               (proceed to domain)               ▼
│                                     ADD Batch N+1 to training
│                                     TAKE Batch N+2 as new test
│                                     REPEAT
└─ (continues until gate or corpus exhausted)
```

**Key invariant:** the test batch is always the *next* 100 sentences —
sentences the model has never seen during training. This measures
generalization, not memorization.

### 13C. Step-by-Step

**Step 1 — Select training batch N**
- Take the next 100 sentence pairs not yet used for training.
- Each pair must satisfy quality gates from §6C.
- Pairs are ordered so structural diversity increases across batches
  (simplest POS patterns first, complex nested patterns later).

**Step 2 — Convert to DPO pairs**
- Apply §7 algorithm: each sentence pair → 3 unit examples.
- Total: 100 sentence pairs → up to 300 DPO examples (100 per unit).
- Construct rejected variants per TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE.
- Every rejected variant must have a complete `[WRONG: weakness - gap - consequence]` annotation.

**Step 3 — Train all three units**
- Call `TrainBatchAsync` with the 300 DPO examples.
- Units trained: `SemanticTokenizerUnit`, `ExecutorUnit`, `QualityVerifierUnit`.
- Export checkpoint after training (overwrite or version-stamp).

**Step 4 — Select test batch N+1**
- Take the next 100 sentence pairs NOT yet seen in training.
- These are the evaluation sentences for this loop iteration.
- Their `expected_translation` is kept private (not stored in git).

**Step 5 — Measure accuracy**
- Run `TranslationProductHeldOutRunner` (or equivalent) on test batch N+1.
- Compute `runtimeProductAccuracy`:
  - A sentence passes if: model returns translation (not Bootstrap) AND confidence ≥ 0.5.
  - When expected translation is available: also require BLEU-1 ≥ 0.60 for that sentence.
- `runtimeProductAccuracy` = (passing sentences) / 100.
- Also record: `runtimeBootstrapRate`, `runtimeExactAccuracy`, iteration number.

**Step 6 — Gate check**

| Result | Action |
|--------|--------|
| `runtimeProductAccuracy ≥ 0.95` | Base loop exits. Export final base checkpoint and only then allow domain training. |
| `runtimeProductAccuracy < 0.95` | Promote test batch N+1 to training. Take batch N+2 as new test batch. Return to Step 2. Do not stop. |

**Step 7 — Corpus exhausted without reaching gate**
- If all corpus batches are consumed and gate not reached, report:
  `CORPUS_EXHAUSTED_GATE_NOT_REACHED` with last accuracy and iteration count.
- Next action: add more approved corpus sentences (new Tatoeba download,
  additional UN Corpus sentences, or another whitelist source).
- Do not claim gate passed. Do not synthesize artificial test data.
- Do not pad the corpus with generated translations. Synthetic rows may be
  used only as rejected examples when their source/chosen side comes from an
  approved human/source-governed pair.

### 13D. Loop Invariants (must hold every iteration)

1. Test batch sentences must not exist in training set.
2. Test batch Tatoeba IDs must not match any training Tatoeba ID.
3. Runner reads test batch from file; test file is written before runner starts.
4. Runner report is persisted before gate check — gate is never claimed from memory.
5. Accuracy is computed over exactly 100 test sentences — not more, not less.
6. The failed test batch becomes training only after its metric has been
   persisted as the previous iteration's result.
7. A professional domain shard cannot begin while the base loop is below 0.95.

### 13E. Domain Fine-Tuning Loop

After the base gate passes, each professional domain runs the same loop with
two changes. This is a hard dependency: domain training is not a substitute for
the general-language 0.95 base gate.

| Parameter | Base loop | Domain loop |
|-----------|-----------|-------------|
| Starting weights | cold (zero or random init) | copy of base checkpoint |
| Training corpus | general 3 000-word corpus | domain shard (UN / Gutenberg / PMC / GNOME) |
| Batch size | 100 pairs | 100 pairs |
| Gate | `runtimeProductAccuracy ≥ 0.95` | same gate |
| Test sentences | next 100 from general corpus | next 100 from domain corpus |

**Domain training must not degrade base accuracy.**
After domain fine-tuning, run the base held-out test. If base accuracy
drops below 0.80, the domain fine-tuning overwrote too much and the domain
learning rate must be reduced.

### 13F. Loop State Tracking

Per-iteration state must be persisted as a JSON file:

```json
{
  "loop_id": "base-en-ru-loop-001",
  "direction": "EN_RU",
  "corpus": "general-3000-word",
  "iteration": 7,
  "training_batches_used": [1, 2, 3, 4, 5, 6, 7],
  "current_test_batch": 8,
  "accuracy_history": [0.12, 0.24, 0.38, 0.51, 0.63, 0.74, 0.81],
  "last_runtime_product_accuracy": 0.81,
  "last_bootstrap_rate": 0.19,
  "gate_reached": false,
  "gate_target": 0.95,
  "next_action": "train_batch_8_test_batch_9"
}
```

This file is updated after every iteration. It is a private artifact (not git)
because it references batch file paths that contain private sentence text.
A summary (iteration count, accuracy curve, gate status) is included in the
runner report — which is stored in git.

### 13G. Expected Convergence

Based on the corpus structure (3 000 words × 10 sentences = 30 000 pairs):

| Iteration | Training sentences | Expected accuracy |
|-----------|-------------------|-------------------|
| 1 | 100 | ~0.10–0.20 (almost all Bootstrap) |
| 5 | 500 | ~0.40–0.55 (frequent words learned) |
| 10 | 1 000 | ~0.60–0.70 (common patterns stable) |
| 20 | 2 000 | ~0.75–0.82 (structural cascade helping) |
| 30–40 | 3 000–4 000 | ≥ 0.95 (gate target) |

If gate is not reached by iteration 50 (5 000 training sentences), the
structural KNN cascade (PrefixSignature / StructureSignature fallback) is
likely not yet implemented or not being used by inference. This is still not
a stop point. The next action is to fix the trainable inference path, rerun the
last failed 100-sentence test batch, and continue the loop.

### 13H. Completion Boundary

The general language base is complete only when a runner report proves all of
the following at the same time:

1. The test batch contains exactly 100 never-seen sentence-pair sets.
2. Those 100 sets were not used in training before the measurement.
3. Each training set was converted into the three unit-level DPO examples
   (`SemanticTokenizerUnit`, `ExecutorUnit`, `QualityVerifierUnit`).
4. `runtimeProductAccuracy >= 0.95`.
5. The runner report includes bootstrap rate and contamination check results.

Anything less than this is progress, not completion. In particular, creating a
builder, source list, DPO file, helper test, checkpoint, or plan section does
not close the base-language goal by itself.

Only after this base completion boundary is crossed may the same 100+100 loop
begin for professional domains such as legal, medical, technical, philosophy,
or current-events translation.

---

## 14. Held-Out Integrity Rules

These rules enforce that held-out test data is genuinely unseen:

1. Held-out sentences must **not** appear anywhere in the training set.
2. Held-out source text must differ from training source text character-for-character.
3. Held-out structural signature (first 3 POS-bigrams) must differ from
   all training signatures for the same word.
4. Before any runner gate claim, compare held-out IDs against training IDs —
   if any match, report contamination and block the gate.
5. Held-out files in git contain only `input_text` — never `expected_translation`.

---

## 15. Anti-Determinism Constraint

This corpus must not produce deterministic mappings in the code.
The code computes structural addresses (signatures, POS-bigrams) only.
All actual word choices come from what the model learned through DPO pairs.

The following patterns are **forbidden** in implementation:
- `if (word == "run") return "запустить"` — hardcoded mapping
- `dictionary["run"] = "запустить"` — lookup table as primary path
- Regex that produces translations by pattern-matching source text

The only legal path from source text to translation:
1. Tokenize → structural signature
2. KNN lookup in trained store (closest trained pair by signature)
3. If no match above threshold → BootstrapFallback (logged as warning)

---

## 16. Next Action

### Mandatory gate order — do not skip

**P0 — Pilot (mandatory first gate before any 3 000-word scale-up):**

Run the builder with `--limit 100` on an approved source.
Produce `word-list-en-pilot-100.jsonl` + `word-list-ru-pilot-100.jsonl`.
Build 10 sentences per word from Tatoeba.
Run one complete 100+100 loop (100 training, 100 unseen test).

Gate B: the test batch shows zero contamination (no `input_text` == any
training `a_side`), and the runner report is present with
`runtimeProductAccuracy` recorded (even 0.00 is an honest baseline).
Do not proceed to P1 until this gate B passes.

**Why P0 exists:** pilot proves the unseen batch is genuinely unseen —
that the contamination guard works in practice, not just in documentation.
Claiming 95% on 3 000 words before contamination is proven on 100 words
is the same false gate that produced `accuracy = 1.000` on TR-HO-17..32.

---

**P1:** Create `word-list-en-3000.jsonl` from an approved EN frequency source
        (see §5A: COCA and Oxford are REVIEW_REQUIRED — do not use until license
        clearance; use General Service List or another §4-cleared source)
        filtered to nouns/verbs/adjectives/adverbs, first 3000 by rank.
**P2:** Create `word-list-ru-3000.jsonl` from Ляшевская & Шаров list.
**P3:** Download Tatoeba dump and build local EN↔RU search index.

P1 + P2 + P3 only after P0 gate B passes.
These three phases produce no private artifacts — output is word lists
and source ID indexes only.

---

### After any review, feedback, or governance repair

After a review finding is incorporated (documentation updated, state fixed,
protocol violation recorded), the next action is NOT a summary or pause.

The next action is always: return to the first open executable gate in this
sequence (P0 → P1 → P2 → P3 → 100+100 loop → 95% base gate → domain).

If P0 is not yet complete, the next action is to run P0.
If P0 is complete but P1 is not, the next action is to run P1.
And so on.

"Review incorporated" is never a stopping point. It is a bounded repair step
inside the continuous execution loop. The plan continues from the first
incomplete gate immediately after the repair is committed.

---

## 17. P1/P2 Implementation Status

### 17A. What is implemented in code

`27.TranslationDomain/TranslationCorpusSourcing.cs` now provides the first
reproducible P1/P2 mechanism:

1. Parse local frequency source files with columns:
   `word`, `lemma`, `pos`, `rank`, `source_id`, `license`, `commercial_allowed`.
2. Keep only core POS rows: `NOUN`, `VERB`, `ADJ`, `ADV`.
3. Reject rows that are not commercial-use allowed.
4. Reject rows whose license is not in the approved source whitelist.
5. Deduplicate by lemma, keeping the best rank.
6. Emit git-safe JSONL rows with only:
   `word`, `lemma`, `pos`, `rank`, `language`, `source_id`, `license`.

This is intentionally not a translator and not a word-meaning table. It is only
the source-cleaning gate before DPO construction.

`27.TranslationDomain.Tools` adds the executable wrapper.

Pilot mode is the first executable use:

```powershell
dotnet run --project 27.TranslationDomain.Tools -- build-word-list `
  --language en `
  --input C:\path\approved-frequency-source.tsv `
  --output 27.TranslationDomain\SourceData\word-list-en-pilot-100.jsonl `
  --limit 100 `
  --delimiter tab
```

Scale-up mode is allowed only after P0 passes:

```powershell
dotnet run --project 27.TranslationDomain.Tools -- build-word-list `
  --language en `
  --input C:\path\approved-frequency-source.tsv `
  --output 27.TranslationDomain\SourceData\word-list-en-3000.jsonl `
  --limit 3000 `
  --delimiter tab
```

The CLI prints only counts and a rejection histogram. It does not download
sources and does not write raw sentence text.

### 17B. P0 Tatoeba Pilot Builder Status

The full `word-list-en-3000.jsonl` and `word-list-ru-3000.jsonl` are not yet
generated because the exact source files must be supplied or downloaded from an
approved source. The project now has the builder and tests, but it does not
claim the 3 000-word lists exist.

P0 pilot is now executable through:

```powershell
dotnet run --project 27.TranslationDomain.Tools -- build-tatoeba-pilot `
  --rus-sentences <private>\rus_sentences.tsv `
  --eng-sentences <private>\eng_sentences.tsv `
  --rus-eng-links <private>\rus-eng_links.tsv `
  --output-dir 27.TranslationDomain\SourceData `
  --private-output-dir <private>\pilot-private `
  --word-limit 100 `
  --sentences-per-word 10
```

Live P0 result on locally downloaded Tatoeba exports:

| Metric | Value |
|---|---:|
| candidate sentence pairs | 512477 |
| EN pilot words | 100 |
| RU pilot words | 100 |
| EN→RU sentence-index rows | 1000 |
| RU→EN sentence-index rows | 1000 |
| P0 source gate | passed |

Git-safe outputs:

```text
27.TranslationDomain/SourceData/word-list-en-pilot-100.jsonl
27.TranslationDomain/SourceData/word-list-ru-pilot-100.jsonl
27.TranslationDomain/SourceData/sentence-index-en-ru-pilot-100.jsonl
27.TranslationDomain/SourceData/sentence-index-ru-en-pilot-100.jsonl
27.TranslationDomain/SourceData/tatoeba-pilot-attribution-manifest.json
```

Private raw sentence-pair outputs stay outside git under the local private
corpus folder and are the only source allowed for the next train/test DPO
conversion step.

### 17C. Current verification

Targeted tests prove:

1. non-core POS rows are rejected;
2. duplicate lemmas are removed;
3. non-commercial / unapproved licenses are rejected;
4. emitted JSONL does not contain `chosen` or `expected_translation`;
5. P0 Tatoeba pilot indexes contain sentence IDs and patterns, not raw text;
6. private pilot pair files contain raw source/target text outside git.

### 17D. Next executable step

P0 source pass is complete. The next executable step is not the 3 000-word
scale-up. It is DPO conversion for the pilot private pairs:

```
C:\Users\lubao\Documents\xiigen\private-corpora\tatoeba-2026-05-23\pilot-private\sentence-pairs-en-ru-pilot-100.jsonl
C:\Users\lubao\Documents\xiigen\private-corpora\tatoeba-2026-05-23\pilot-private\sentence-pairs-ru-en-pilot-100.jsonl
```

Convert the first 100 private sentence-pair sets into three unit-level DPO
examples each, train, then test on the next 100 never-seen private sentence-pair
sets from the same pilot curriculum. Report product accuracy, exact accuracy,
bootstrap rate, script accuracy, copied-source rate, and train/test similarity
before any promotion claim.

If fewer than 100 accepted pilot rows or fewer than 200 approved sentence-pair
sets are produced, the runner must report the shortfall and the rejected-row
histogram instead of padding the list or jumping to an unrelated corpus.

The 3 000-word files remain the P1/P2 scale-up output, not the current next
action:

```
27.TranslationDomain/SourceData/word-list-en-3000.jsonl
27.TranslationDomain/SourceData/word-list-ru-3000.jsonl
```
