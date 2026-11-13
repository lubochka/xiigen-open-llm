# TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md
# Version: 1.14 - 2026-05-30
# Purpose: Training and verification guide for TranslationDomain (Direction 1).
# Product placement: chat system â€” networking between users from different countries,
#   post and text translation, user prompt translation.
# Parent plan: PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md
# Related guidance: PARALLEL-TEACHING-AND-EVALUATION-GUIDANCE-v1.md
# Unit DPO guide: TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md
# Fixed guidance source: C:\Users\lubao\Documents\xiigen\llm mvp\translation_guidance_fixed\
# Source plan: C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\
# Architecture: 27.TranslationDomain / TranslationMachine : ILearningMachine<TranslationDomain>

---

## 1. Product Use Cases â€” Why This Machine Exists

TranslationDomain serves three concrete product needs in the chat system:

| Use case | Direction | Register | Example |
|---|---|---|---|
| Chat between users from different countries | RUâ†”EN, HEâ†”EN | Informal, conversational, short | "ÐŸÑ€Ð¸Ð²ÐµÑ‚! Ð¯ Ð½Ð¾Ð²Ñ‹Ð¹ Ñ‚ÑƒÑ‚ ðŸ‘‹" â†’ "Hey! I'm new here ðŸ‘‹" |
| Post and text translation | RUâ†”EN, HEâ†”EN | Natural, medium-length, tone-preserving | LinkedIn business post, event announcement |
| User prompt translation | RUâ†’EN, HEâ†’EN | Command-like, imperative, professional | "ÐŸÐ¾Ð¼Ð¾Ð³Ð¸ Ð¼Ð½Ðµ Ð½Ð°Ð¹Ñ‚Ð¸ ÐºÐ¾Ð½Ñ‚Ð°ÐºÑ‚Ñ‹ Ð² ÑÑ„ÐµÑ€Ðµ Ð½ÐµÑ‚Ð²Ð¾Ñ€ÐºÐ¸Ð½Ð³Ð°" â†’ query for system |

Every training batch must serve at least one of these three use cases.
Generic literary or formal-register pairs alone do not satisfy the product gate.

---

## 2. Source Boundary

Current source authority for external translation rows:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\translation_guidance_fixed\TRANSLATION-SOURCE-REGISTRY-v3.md
```

Use the registry decision as a hard gate:

| Decision | Allowed use |
|---|---|
| `ACCEPT` | May enter clean train-ready sets after quality gates pass |
| `ACCEPT_SUPPORT_ONLY` | Tokenizer, language-id, morphology, entity checks; not chosen-side translation authority |
| `QUARANTINE` | Isolated evaluation only; never mixed into clean train-ready batches |
| `REVIEW_REQUIRED` | Not train-ready until exact license and quality evidence are recorded |
| `METHOD_ONLY` | May inspire method only; do not ingest text |
| `BLOCK` | Do not use for training or eval |

Clean first-run sources from the fixed guidance:

| Source | Role |
|---|---|
| Tatoeba | T01/T02/T03/T04 sentence authority where the row is suitable |
| ParaNames | T09 named entity translation/transliteration authority |
| UN Parallel Corpus | RU_EN / EN_RU legal-diplomatic formal domain only |
| Knesset Hebrew | support-only for tokenizer, language-id, formal-spoken register, morphology |

`××—×•×ª×™.zip` remains first-party private material. It can define business/chat
held-out prompts and first-party examples, but raw private PDF/audio/transcripts
must not be committed to git.

### Primary source (deterministic baseline)

```text
C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\
```

Relevant files:

| File | Role |
|---|---|
| `training-pairs-train-ready.jsonl` | Current ready-to-train batch (50 pairs) |
| `training-pairs-blocked.jsonl` | Pairs blocked from training â€” do not use without review |
| `training-pairs-repair-required.jsonl` | Pairs needing fix before training |
| `TRANSLATION-SOURCE-REGISTRY-v3.md` | Authoritative source license registry |
| `TRANSLATION-TRAINING-PLAN-v1.md` | Historical design context |
| `TRANSLATION-RUNBOOK-v1.md` | Executor runbook |

Current batch status:
- File: `training-pairs-train-ready.jsonl`
- Pairs: 50
- Direction: `RU_EN` only
- Family: `general-adverb-translation`
- Register: literary / formally calibrated English
- Batch id pattern: `ru-en-general-l1-b029-*`

This batch covers formal Russian â†’ English translation quality.
It is the correct starting point for the adverb/quality dimension.
It does NOT cover chat, posts, or prompt translation. Those need dedicated batches.

### Training source fallback order (mandatory check before claiming "source missing")

When a training run is requested and the primary named source (e.g., Tatoeba pilot,
specific batch file) is not found in the expected location, the executor MUST
check this priority list before reporting any blocker or pivoting to infrastructure:

```text
Priority 1: training-pairs-train-ready.jsonl
    Path:  C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\training-pairs-train-ready.jsonl
    Rule:  If this file exists and has rows, it IS the training source.
           Use it. Start the 100+100 loop with it. Do not stop.

Priority 2: Tatoeba pilot SourceData files (if P0 already ran)
    Path:  27.TranslationDomain/SourceData/sentence-index-*.jsonl
    Rule:  Use only if training-pairs-train-ready.jsonl is absent.

Priority 3: Tatoeba dump (if downloaded locally)
    Path:  as named in state file or user instruction
    Rule:  Use only if Priority 1 and 2 are absent.
```

**OP-35 compliance check (required before any pivot to tool-building):**

```text
1. Does training-pairs-train-ready.jsonl exist? -> YES -> use it, start training NOW
2. Does any SourceData sentence-index file exist? -> YES -> use it, start training NOW
3. Neither exists -> report shortfall by name in state, proceed to next available gate.
   Do NOT build new infrastructure as a substitute.
   Do NOT declare a blocker while Priority 1 exists on disk.
```

**Documented violation (2026-05-29):** The executor received goal "100+100 until 95%,"
`training-pairs-train-ready.jsonl` was present, Tatoeba dump was not found in the
expected path. Executor invented "build P0 Tatoeba downloader" as the next step.
Zero training batches ran. This is an OP-35 protocol violation (-1000 score).

### Secondary source (first-party, for HEâ†’EN and business register)

```text
C:\Users\lubao\Documents\××—×•×ª×™.zip
```

The ××—×•×ª×™ business course materials are in Hebrew and cover entrepreneurship,
networking, and digital marketing topics. They provide natural Hebrew source
sentences for `HE_EN` and `EN_HE` pairs in the business/networking register.

Raw private files must not be committed to git.
See `docs/plans/ACHOTI-ENTREPRENEURSHIP-AFFILIATE-TRAINING-GUIDE-v1.md` for
source inventory and extraction workflow.

---

## 3. Training Dimensions Matrix

Each dimension is trained separately. Do not mix dimensions in one batch.
A batch that mixes literary and chat pairs hides regressions.

The full stream identity is:

```text
direction x topic_domain x translation_level x register x pair_family
```

The fixed guidance treats `50 train + 50 held-out` as one incremental stream
batch, not the whole domain. Continue the same stream only when its runner report
passes; otherwise repair the failed source/rejected/tokenizer/router weakness.

| Dimension id | Direction | Register | Family | Needed for |
|---|---|---|---|---|
| DIM-RU-EN-GENERAL | RU_EN | Literary | general-adverb-translation | Translation quality foundation |
| DIM-RU-EN-CHAT | RU_EN | Informal, conversational | chat-message | User networking chat |
| DIM-RU-EN-POST | RU_EN | Business casual | post-text | Post/text translation |
| DIM-RU-EN-PROMPT | RU_EN | Imperative, professional | user-prompt | Prompt translation for system |
| DIM-EN-RU-CHAT | EN_RU | Informal | chat-message | EN users messaging RU users |
| DIM-HE-EN-CHAT | HE_EN | Informal | chat-message | Hebrew user networking chat |
| DIM-HE-EN-POST | HE_EN | Business, Hebrew natural | post-text | Hebrew post translation |
| DIM-HE-EN-PROMPT | HE_EN | Command-like | user-prompt | Hebrew prompts to system |
| DIM-EN-HE-CHAT | EN_HE | Informal | chat-message | EN messages to HE users |

### FrequencyQuoteMap dimension

The French and Spanish session archives under
`C:\Users\lubao\Documents\xiigen\claude-sessions\Uncategorized` add one
important training method: before expecting a small translation machine to
generalize, build a map of frequent words and common sentence starts.

For RU_EN this means:

```text
top Russian words and prefixes
  -> POS / semantic role
  -> common source sentence starts
  -> literal translation pressure
  -> natural English continuation template
  -> accepted/rejected rows for semantic-tokenizer, executor-ru_en, verifier
```

This is now part of the batch-029 repair path. It is not a claim that French or
Spanish adapters are ready. French and Spanish are method evidence for how to
build stronger KNN maps: top words, top quotes/sentence examples, cross-corpus
source checks, and rejection of machine-translated frequency sources.

Current code status: `FrequencyQuoteEntry` and
`FrequencyQuoteMapBatchBuilder` exist in `27.TranslationDomain`. A frequency
quote row now expands into tokenizer, executor, and verifier DPO examples, and
`TranslationKnnStore.FindNearestCascade` can retrieve learned examples by full
signature, prefix signature, or structure signature. This is still not product
promotion; it is the first runtime bridge from frequency/quote maps into the
trainable translation flow.

---

### Grammar-Rule Source And Rejected-DPO Layer

The general-language loop must include a language-rule layer before any domain
fine-tuning. KNN signatures must include rule tags for tense, aspect, case,
agreement, pluralization, articles, irregular forms, exceptions, and false
friends.

Rule data is source-governed:

```text
UD / UD Features     -> canonical POS, morphology, syntax schema
UniMorph             -> lemma/form/feature paradigms
CLDR                 -> plural, numeral, date/unit locale rules
LanguageTool         -> correct/incorrect grammar examples for rejected DPO
OpenCorpora/pymorphy -> Russian morphology and generated forms
Wiktionary/Wiktextract -> exceptions, inflections, usage, translations
Apertium/GF          -> advanced transfer/formal grammar reference after license review
```

For every new language, the source-search protocol is:

```text
Universal Dependencies <LANG>
UD <LANG> treebank GitHub
UniMorph <LANG>
CLDR plural rules <LANG>
Wiktionary <LANG> inflection tables
Wiktextract <LANG>
LanguageTool <LANG> grammar.xml
Apertium <LANG>
GF Resource Grammar Library <LANG>
<LANG> morphological dictionary open source
<LANG> dependency treebank license
```

Grammar-rule rows are stored as their own DPO family. Each rule row needs:

```text
rule_id
language
rule_family
source_id
license state
chosen correct form or translation
rejected plausible wrong form
[WRONG: one exact rule violation - what would be learned incorrectly - product consequence]
feature_changed / correct_feature_value / rejected_feature_value when applicable
```

The rejected side must violate exactly one rule. Example:

```text
chosen:  Я вижу брата.
rejected: Я вижу брат. [WRONG: morphology_error - animate masculine direct object requires accusative form 'брата'; the rejected row teaches the wrong object case.]
```

Invalid rejected:

```text
Я видеть брат.
```

It is invalid because it mixes verb-form and case errors.

The active user target for the current base-language loop is 85%
(`runtimeProductAccuracy >= 0.85`). Historical 95% wording is superseded for
this run.
The executor must show accuracy on every tested set. A valid stage report must
show:

```text
cycle
train rows
held-out rows
filtered rows removed by >70% history overlap
runtimeProductAccuracy
runtimeExactAccuracy
runtimeBootstrapRate
agreement/case/tense rule coverage when that set is in scope
```

This is a per-100-set visibility rule. After every 100-row test set is evaluated,
print the metric line immediately. Do not wait until the end of a long loop and
do not replace the visible metric line with a large JSON dump.

The `run-tatoeba-loop` runner must not evaluate the next raw 100 rows blindly.
For every 100-row test set it must scan forward and remove any row whose source
text is already present in training history or whose source-token Jaccard
overlap with training history is `>= 0.70`. The visible metric line must include
these fields:

```text
candidate_rows_scanned
filteredExactOverlap
filteredCheckpointMemoryOverlap
filteredSimilarityOverlap70
testSelectionShortfall
newPatternRate
sourceTokenSeenRate
runtimeProductAccuracy
runtimeExactAccuracy
runtimeBootstrapRate
passes_target
```

If `testSelectionShortfall > 0`, the set is not a valid 100-set gate even when
the partial rows look good. Continue with a larger source inventory or repair
the corpus selection before claiming progress toward the 85% target.

### 3K. Promotion Gate For Repeated Patterns

Per Luba's 2026-05-29 correction, a tested 100-set is not automatically fed
back into training. It is measured and reported, but it is promoted only when it
adds enough structural novelty.

Promote a tested 100-set into training only when both adaptive conditions are true:

```text
newPatternRate > (1 - maxAccuracy)
sourceTokenSeenRate < maxAccuracy
```

The runner uses the best observed `runtimeProductAccuracy` for the current
trained model/history as `maxAccuracy`. Checkpoint resume restores this value
from `TatoebaLoop.BestRuntimeProductAccuracy`; older checkpoints without that
field fall back to the current evidence default `maxAccuracy = 0.78`, which
means `newPatternRate > 0.22` and `sourceTokenSeenRate < 0.78`. The operator's
correction is binding: the gate is strict `AND`, never `OR`.

Reason: a failed side means the set either repeats already-seen structures or
mostly uses already-seen source vocabulary. Feeding it into training spends an
iteration on saturation instead of adding useful new pattern coverage.

Runner output must print:

```text
promotedToTraining
promotionMaxAccuracy
minimumPromotionNewPatternRate
maximumPromotionSeenTokenRate
promotionDecision
```

Allowed decisions:

```text
PROMOTE_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M>
SKIP_REQUIRES_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M>
SKIP_SHORTFALL
```

The current target for this loop is `runtimeProductAccuracy >= 0.85` until Luba
changes it again. Historical 0.75 and 0.95 target text is superseded for the
active training run.

Resume training from checkpoint (phase 36 and later):

```
dotnet run --project 27.TranslationDomain.Tools -- run-tatoeba-loop \
  --private-pairs <path> --checkpoint checkpoint.json
```

`LoadCheckpointAsync` returns `TranslationMachineLoadResult(Machine, TatoebaLoop?)`.
The runner restores cursor and `lastCompletedCycle` from `TatoebaLoop` automatically.
The next visible `ACCURACY_SET` is the first new set after the checkpoint cursor.
Checkpoint resume restores `trainingRows` only from `AcceptedTrainingRows`.
Rows saved in `SkippedRows` keep the cursor from re-scanning them, but they are
not counted as training history and must not inflate `train_rows`.
Checkpoint resume also restores `BestRuntimeProductAccuracy` so the adaptive
novelty gate does not reset to the 0.78 fallback after a process restart.
The runner report must show `checkpoint_loaded=true`, `resume_mode=checkpoint`,
`replayed_cycles=0`, and `initial_training_skipped_due_checkpoint=true` for a
real checkpoint resume.

Explicit source rollover is allowed only when the saved checkpoint source is
exhausted and a different approved private-pairs file must continue the same
trained machine. Use `--start-new-source-from-checkpoint` with
`--checkpoint` and `--require-checkpoint-resume`; the runner must report
`checkpoint_loaded=true`, `resume_mode=source-rollover`, `replayed_cycles=0`,
`sourceRolloverFrom`, `sourceRolloverTo`, and preserved
`bestRuntimeProductAccuracy`. Rollover trains the first block of the new source
into the loaded machine and starts the new source loop from cursor 0; it must
never silently ignore a `PrivatePairsPath` mismatch.

During source rollover, test selection must also exclude source rows already
known by the loaded checkpoint exact-source memory for the same direction. The
runner report must expose `filteredCheckpointMemoryOverlap` so a high score
cannot be accepted when the test set was filled with rows memorized before the
new source file began.

`--skip-cycles` [LEGACY/REPLAY — not resume]: replays training data to rebuild
in-memory state. This is not persistence; process kill loses state again.
Use `--checkpoint` instead for real resume across process boundaries.

Domain training is still blocked until the general-language loop reaches the
0.85 target on clean unseen sets.

## 4. DPO Pair Shape

This section defines source-level translation pairs. The unit-level accepted /
rejected row shape is defined in
`docs/plans/TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md`.

Every source pair that is used for real learning must expand into three
unit-specific DPO examples:

```text
source row
  -> semantic-tokenizer example
  -> executor-ru_en example
  -> quality-verifier example
```

Do not train TranslationMachine only as a flat prompt/chosen/rejected string.
The flat source pair is the human-authored seed; the teachable rows are the
per-unit accepted/rejected objects.

### General rules

- A-side is a real user message, post, or prompt â€” not a test description.
- For external translation rows, A-side is raw source text. Do not store
  "Translate the following..." in `a_side`; the training harness injects the
  direction prompt.
- Chosen side is the correct translation that serves the product use case.
- Rejected side is plausible but wrong; always includes `[WRONG: reason]`.
- A-side and chosen side must be in the correct direction (source â†’ target language).
- Do not mix source languages in one pair.
- Set `compiled_prompt_in_training: false` on source-level translation rows.
- Chosen and rejected must both be in the declared target language.
- Reject rows with mojibake, missing `[WRONG:]`, refusal-style rejected text, or
  source registry status that is not allowed for training.

### Rejected-side taxonomy from fixed guidance

Use stable rejected labels so the runner can diagnose exactly what failed:

| Code | Meaning |
|---|---|
| `mistranslation` | wrong word or phrase equivalent |
| `false_cognate` | source word looks familiar but means something different |
| `literal_calque` | word-for-word rendering violates target idiom |
| `omission` | source clause or element dropped |
| `addition` | unsupported content added |
| `morphological` | wrong case, gender, number, aspect, binyan, final letter |
| `register_shift` | wrong formality or tone |
| `rtl_error` | Hebrew script or bidi handling error |
| `back_translation_divergence` | round-trip semantic loss |
| `named_entity` | wrong transliteration or proper-name handling |

Before accepting a pair, run the overlap gate. If chosen and rejected are too
similar, the reward margin will sit near zero and the row teaches almost
nothing. The current preparation script uses a whitespace threshold of `0.50`;
BPE/token audits may use `0.60`.

### Pair format

```json
{
  "id": "ru-en-chat-b001-001",
  "source": "ru-en-chat",
  "direction": "RU_EN",
  "pair_family": "chat-message",
  "prompt": "Translate the following Russian chat message to English:",
  "a_side": "[Russian chat message]",
  "chosen": "[Correct English translation, preserving tone, emoji, informal register]",
  "rejected": "[WRONG: reason â€” over-formal / tone-lost / meaning-shifted / emoji-dropped]",
  "validation_outcome": "train_ready",
  "validation_errors": []
}
```

---

## 5. Real Example Pairs Per Use Case

### Use case 1 â€” Chat messages (RU_EN)

```json
{
  "id": "ru-en-chat-b001-001",
  "direction": "RU_EN",
  "pair_family": "chat-message",
  "a_side": "ÐŸÑ€Ð¸Ð²ÐµÑ‚! Ð¯ Ð½Ð¾Ð²Ñ‹Ð¹ ÑƒÑ‡Ð°ÑÑ‚Ð½Ð¸Ðº ÑÐµÑ‚Ð¸. Ð§ÐµÐ¼ Ñ‚Ñ‹ Ð·Ð°Ð½Ð¸Ð¼Ð°ÐµÑˆÑŒÑÑ? ðŸ‘‹",
  "chosen": "Hey! I'm new to the network. What do you do? ðŸ‘‹",
  "rejected": "[WRONG: over-formal, emoji_dropped] Greetings. I have recently joined this network. May I inquire about your professional occupation?"
}
```

```json
{
  "id": "ru-en-chat-b001-002",
  "direction": "RU_EN",
  "pair_family": "chat-message",
  "a_side": "Ð¡Ð¿Ð°ÑÐ¸Ð±Ð¾ Ð·Ð° ÐºÐ¾Ð½Ð½ÐµÐºÑ‚! Ð¯ Ð¸Ñ‰Ñƒ Ð¿Ð°Ñ€Ñ‚Ð½Ñ‘Ñ€Ð¾Ð² Ð² Ð¾Ð±Ð»Ð°ÑÑ‚Ð¸ Ð¼Ð°Ñ€ÐºÐµÑ‚Ð¸Ð½Ð³Ð° ðŸ¤",
  "chosen": "Thanks for connecting! I'm looking for partners in marketing ðŸ¤",
  "rejected": "[WRONG: meaning_shifted] Thank you for establishing this connection. I am actively seeking co-workers in the marketing sector."
}
```

### Use case 2 â€” Post and text translation (RU_EN)

```json
{
  "id": "ru-en-post-b001-001",
  "direction": "RU_EN",
  "pair_family": "post-text",
  "a_side": "Ð¡ÐµÐ³Ð¾Ð´Ð½Ñ Ð·Ð°ÐºÑ€Ñ‹Ð»Ð° ÑÐ´ÐµÐ»ÐºÑƒ Ð½Ð° 150 000$, ÑÐ¾Ð²ÑÐµÐ¼ Ð±ÐµÐ· Ð¸Ð½Ð²ÐµÑÑ‚Ð¾Ñ€Ð¾Ð²! ÐšÐ»Ð¸ÐµÐ½Ñ‚ Ð² Ð²Ð¾ÑÑ‚Ð¾Ñ€Ð³Ðµ ðŸ”¥ Ð’ÐµÑ€ÑŽ, Ñ‡Ñ‚Ð¾ Ð²ÑÑ‘ Ð²Ð¾Ð·Ð¼Ð¾Ð¶Ð½Ð¾, ÐµÑÐ»Ð¸ Ð´ÐµÐ»Ð°Ñ‚ÑŒ Ð¼Ð°Ð»ÐµÐ½ÑŒÐºÐ¸Ðµ ÑˆÐ°Ð³Ð¸ ÐºÐ°Ð¶Ð´Ñ‹Ð¹ Ð´ÐµÐ½ÑŒ.",
  "chosen": "Just closed a $150,000 deal today â€” completely bootstrapped, no investors! The client is thrilled ðŸ”¥ I believe anything is possible when you take small steps every single day.",
  "rejected": "[WRONG: tone_lost, emoji_dropped] A deal of $150,000 was closed today without investor involvement. The client expressed satisfaction. Small daily actions lead to results."
}
```

```json
{
  "id": "ru-en-post-b001-002",
  "direction": "RU_EN",
  "pair_family": "post-text",
  "a_side": "Ð—Ð°Ð¿ÑƒÑÐºÐ°ÑŽ Ð¿ÐµÑ€Ð²Ñ‹Ð¹ Ð±ÐµÑÐ¿Ð»Ð°Ñ‚Ð½Ñ‹Ð¹ Ð²ÐµÐ±Ð¸Ð½Ð°Ñ€ Ð¿Ð¾ Ð½ÐµÑ‚Ð²Ð¾Ñ€ÐºÐ¸Ð½Ð³Ñƒ Ð´Ð»Ñ Ð¿Ñ€ÐµÐ´Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°Ñ‚ÐµÐ»ÐµÐ¹. Ð—Ð°Ñ€ÐµÐ³Ð¸ÑÑ‚Ñ€Ð¸Ñ€ÑƒÐ¹Ñ‚ÐµÑÑŒ Ð¿Ð¾ ÑÑÑ‹Ð»ÐºÐµ Ð½Ð¸Ð¶Ðµ! ÐœÐµÑÑ‚ Ð¼Ð°Ð»Ð¾ ðŸ“…",
  "chosen": "Launching my first free networking webinar for entrepreneurs. Register via the link below! Spots are limited ðŸ“…",
  "rejected": "[WRONG: register_wrong, emoji_dropped] I am conducting an inaugural complimentary webinar on the subject of professional networking for business founders."
}
```

### Use case 3 â€” User prompt translation (RU_EN, for system)

```json
{
  "id": "ru-en-prompt-b001-001",
  "direction": "RU_EN",
  "pair_family": "user-prompt",
  "a_side": "ÐŸÐ¾Ð¼Ð¾Ð³Ð¸ Ð¼Ð½Ðµ Ð½Ð°Ð¹Ñ‚Ð¸ ÐºÐ¾Ð½Ñ‚Ð°ÐºÑ‚Ñ‹ Ð² ÑÑ„ÐµÑ€Ðµ Ð½ÐµÑ‚Ð²Ð¾Ñ€ÐºÐ¸Ð½Ð³Ð° Ð² IT-ÑÐµÐºÑ‚Ð¾Ñ€Ðµ",
  "chosen": "Help me find networking contacts in the IT sector",
  "rejected": "[WRONG: context_added, over-specific] Please assist me in locating professional networking contacts within information technology companies and startups."
}
```

```json
{
  "id": "ru-en-prompt-b001-002",
  "direction": "RU_EN",
  "pair_family": "user-prompt",
  "a_side": "ÐŸÐµÑ€ÐµÐ²ÐµÐ´Ð¸ ÑÑ‚Ð¾Ñ‚ Ð¿Ð¾ÑÑ‚ Ð½Ð° Ð¸Ð²Ñ€Ð¸Ñ‚ Ð¸ ÑÐ¾Ñ…Ñ€Ð°Ð½Ð¸ Ð¿Ñ€Ð¾Ñ„ÐµÑÑÐ¸Ð¾Ð½Ð°Ð»ÑŒÐ½Ñ‹Ð¹ Ñ‚Ð¾Ð½",
  "chosen": "Translate this post to Hebrew and preserve the professional tone",
  "rejected": "[WRONG: instruction_paraphrased] Please take the following post and render it in the Hebrew language while maintaining an appropriately professional register throughout."
}
```

### Use case 4 â€” Chat messages (HE_EN, from ××—×•×ª×™ context)

```json
{
  "id": "he-en-chat-b001-001",
  "direction": "HE_EN",
  "pair_family": "chat-message",
  "a_side": "×©×œ×•×! ×× ×™ ×—×“×©×” ×¤×”, ×ž×” ××ª×” ×¢×•×©×”? ðŸ‘‹",
  "chosen": "Hi! I'm new here, what do you do? ðŸ‘‹",
  "rejected": "[WRONG: greeting_over-formal] Good day. I am a new participant. Would you be willing to share information regarding your professional activities?"
}
```

```json
{
  "id": "he-en-chat-b001-002",
  "direction": "HE_EN",
  "pair_family": "chat-message",
  "a_side": "××™×š ×× ×™ ×™×›×•×œ×” ×œ×”×ª×—×‘×¨ ×¢× ×ž×©×§×™×¢×™× ×‘×œ×™ ×œ×”×©×§×™×¢ ×›×¡×£?",
  "chosen": "How can I connect with investors without investing money?",
  "rejected": "[WRONG: meaning_expanded] How is it possible for me to establish connections with potential investors while simultaneously avoiding any financial expenditure on my part?"
}
```

---

## 6. Batch Plan Per Direction

### Batch 001 â€” current ready batch

```text
Batch id: ru-en-general-l1-b029 (already in training-pairs-train-ready.jsonl)
Direction: RU_EN
Family: general-adverb-translation
Pairs: 50 train + 50 held-out
Status: ready to run through TranslationMachine.TrainBatchAsync
```

This batch covers foundation translation quality. Run it first.

### Batch 002 â€” chat and prompt layer

```text
Batch id: ru-en-chat-b001
Direction: RU_EN
```

Recommended mix:

| Family | Train rows | Held-out rows |
|---|---:|---:|
| chat-message | 20 | 20 |
| post-text | 15 | 15 |
| user-prompt | 15 | 15 |

Build these pairs from:
- Real chat samples typical in networking scenarios
- Post translations (examples in Section 5)
- Actual user prompts that would be sent to the system

### Batch 003 â€” Hebrew directions

```text
Batch id: he-en-chat-b001
Directions: HE_EN and EN_HE
```

Recommended mix:

| Family | Direction | Train rows | Held-out rows |
|---|---|---:|---:|
| chat-message | HE_EN | 12 | 12 |
| post-text | HE_EN | 8 | 8 |
| user-prompt | HE_EN | 8 | 8 |
| chat-message | EN_HE | 12 | 12 |
| post-text | EN_HE | 5 | 5 |
| user-prompt | EN_HE | 5 | 5 |

Source: ××—×•×ª×™ materials (business networking texts, event marketing, Hebrew course content).
Build pairs using the extraction workflow in `ACHOTI-ENTREPRENEURSHIP-AFFILIATE-TRAINING-GUIDE-v1.md Â§4`.

---

## 7. Real Held-Out Prompt Suite

These prompts evaluate product-level translation quality.
They must be stored with `can_holdout=true, can_train=false`.

| Case id | User input | Expected behavior | Reject if |
|---|---|---|---|
| TR-HO-01 | `RU_EN:` "ÐŸÑ€Ð¸Ð²ÐµÑ‚! Ð¯ Ð½Ð¾Ð²Ñ‹Ð¹ Ñ‚ÑƒÑ‚, Ñ‡ÐµÐ¼ Ð·Ð°Ð½Ð¸Ð¼Ð°ÐµÑˆÑŒÑÑ? ðŸ˜Š" | Produces casual English greeting with emoji, natural short phrasing | Drops emoji, uses formal register |
| TR-HO-02 | `RU_EN:` "Ð¡Ð¿Ð°ÑÐ¸Ð±Ð¾ Ð·Ð° ÐºÐ¾Ð½Ð½ÐµÐºÑ‚! Ð¥Ð¾Ñ‡Ñƒ Ð¾Ð±ÑÑƒÐ´Ð¸Ñ‚ÑŒ Ð²Ð¾Ð·Ð¼Ð¾Ð¶Ð½Ð¾Ðµ ÑÐ¾Ñ‚Ñ€ÑƒÐ´Ð½Ð¸Ñ‡ÐµÑÑ‚Ð²Ð¾ ðŸ¤" | "Thanks for connecting! I'd love to discuss potential collaboration ðŸ¤" | Translates "ÐºÐ¾Ð½Ð½ÐµÐºÑ‚" as "connection" with formal tone, drops emoji |
| TR-HO-03 | `RU_EN:` (LinkedIn post) "Ð—Ð°Ð¿ÑƒÑÐºÐ°ÑŽ ÐºÑƒÑ€Ñ Ð¿Ð¾ Ð½ÐµÑ‚Ð²Ð¾Ñ€ÐºÐ¸Ð½Ð³Ñƒ Ð´Ð»Ñ Ð½Ð°Ñ‡Ð¸Ð½Ð°ÑŽÑ‰Ð¸Ñ… Ð¿Ñ€ÐµÐ´Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°Ñ‚ÐµÐ»ÐµÐ¹. ÐŸÐµÑ€Ð²Ñ‹Ðµ 10 Ð¼ÐµÑÑ‚ â€” Ð±ÐµÑÐ¿Ð»Ð°Ñ‚Ð½Ð¾! ðŸŽ¯" | Preserves call-to-action, enthusiasm, emoji, natural marketing English | Over-formalises, loses urgency, drops emoji |
| TR-HO-04 | `RU_EN:` (user prompt) "ÐÐ°Ð¹Ð´Ð¸ Ð¼Ð½Ðµ Ð¿Ð°Ñ€Ñ‚Ð½Ñ‘Ñ€Ð¾Ð² Ð¿Ð¾ Ð´Ð¸Ð·Ð°Ð¹Ð½Ñƒ Ð² Ð˜Ð·Ñ€Ð°Ð¸Ð»Ðµ" | "Find me design partners in Israel" â€” short, imperative, exact | Expands, adds context, changes imperative to polite request |
| TR-HO-05 | `RU_EN:` "Ð‘ÐµÑÑÑ‚Ñ€Ð°ÑˆÐ½Ð¾ Ð´Ð²Ð¸Ð½ÑƒÐ»ÑÑ Ñ‡ÐµÑ€ÐµÐ· Ñ‚Ð¾Ð»Ð¿Ñƒ, Ð½Ðµ Ð²Ñ‹Ð´Ð°Ð² ÑÑ‚Ñ€Ð°Ñ…Ð°." | Uses a high-register adverb ("intrepidly" or equivalent), calibrated English | Uses generic "bravely/fearlessly" without precision |
| TR-HO-06 | `EN_RU:` "Just launched my first MVP! Feeling pumped ðŸš€" | Natural informal Russian with emoji, "Ð·Ð°Ð¿ÑƒÑÑ‚Ð¸Ð»Ð° Ð¿ÐµÑ€Ð²Ñ‹Ð¹ MVP" | Literal stiff translation, loses excitement, drops emoji |
| TR-HO-07 | `EN_RU:` "Let's connect and explore synergies ðŸ¤" | Russian networking phrasing â€” "Ð”Ð°Ð²Ð°Ð¹ ÑÐ¾ÐµÐ´Ð¸Ð½Ð¸Ð¼ÑÑ Ð¸ Ð¾Ð±ÑÑƒÐ´Ð¸Ð¼ Ð²Ð¾Ð·Ð¼Ð¾Ð¶Ð½Ð¾Ðµ ÑÐ¾Ñ‚Ñ€ÑƒÐ´Ð½Ð¸Ñ‡ÐµÑÑ‚Ð²Ð¾ ðŸ¤" | Word-for-word stilted translation or "ÑÐ¸Ð½ÐµÑ€Ð³Ð¸Ð¸" without explanation |
| TR-HO-08 | `HE_EN:` "×©×œ×•×! ×× ×™ ×—×“×©×” ×¤×”, ×‘×ž×” ××ª×” ×¢×•×¡×§? ðŸ‘‹" | "Hi! I'm new here, what do you do? ðŸ‘‹" | Formal register, drops emoji, over-translates |
| TR-HO-09 | `HE_EN:` "××™×š ×× ×™ ×ž×ª×—×™×œ×” ×œ×”×©×™×§ ×ž×™×–× ×œ×œ× ×ž×©×§×™×¢×™×?" | "How do I start launching a venture without investors?" | Adds unnecessary formality or context |
| TR-HO-10 | `HE_EN:` (post) "×”×©×§×ª×™ ×§×•×¨×¡ ×—×“×© ×œ×©×™×•×•×§ ×“×¨×š × ×˜×•×•×¨×§×™× ×’ ðŸŽ‰ ×¤×¨×˜×™× ×‘×œ×™× ×§!" | "Just launched a new networking marketing course ðŸŽ‰ Details in the link!" | Drops emoji, over-formal, misses brevity |
| TR-HO-11 | `EN_HE:` "Hey! New here, what do you do? ðŸ‘‹" | Natural informal Hebrew: "×”×™×™! ×—×“×© ×¤×”, ×‘×ž×” ××ª×” ×¢×•×¡×§? ðŸ‘‹" | Formal literary Hebrew, dropped emoji |
| TR-HO-12 | `RU_EN:` mixed-register post â€” "Ð¡ÐµÐ³Ð¾Ð´Ð½Ñ Ð¿Ñ€Ð¾Ð²ÐµÐ»Ð° Ð¼Ð°ÑÑ‚ÐµÑ€-ÐºÐ»Ð°ÑÑ Ð¿Ð¾ Ñ„ÐµÐ¹ÑÐ±ÑƒÐº-Ð¼Ð°Ñ€ÐºÐµÑ‚Ð¸Ð½Ð³Ñƒ. 50 ÑƒÑ‡Ð°ÑÑ‚Ð½Ð¸ÐºÐ¾Ð²! Ð“Ð¾Ñ€Ð¶ÑƒÑÑŒ ÐºÐ¾Ð¼Ð°Ð½Ð´Ð¾Ð¹ ðŸ’ª" | "Ran a Facebook marketing workshop today. 50 participants! So proud of my team ðŸ’ª" | Loses team pride tone, "master class" instead of "workshop", drops emoji |
| TR-HO-13 | `RU_EN:` short prompt with abbreviations â€” "ÐŸÐµÑ€ÐµÐ²ÐµÐ´Ð¸ ÑÑ‚Ð¾Ñ‚ Ñ‚ÐµÐºÑÑ‚ Ð½Ð° HE Ð¸ ÑÐ¾Ñ…Ñ€Ð°Ð½Ð¸ tone" | "Translate this text to HE and preserve the tone" â€” keeps "HE" as-is, short | Expands to "Hebrew language" unnecessarily, changes imperative |
| TR-HO-14 | `HE_EN:` compound business term â€” "×ž×©×¤×š ×©×™×•×•×§×™ ×œ×‘× ×™×™×ª ××ž×•×Ÿ ×¢× ×œ×§×•×—×•×ª" | "Marketing funnel for building trust with customers" | "Sales funnel" (wrong term), "clients" when context is new audience |
| TR-HO-15 | `RU_EN:` with emoji string â€” "Ð Ð°Ð´Ð° Ð²Ð¸Ð´ÐµÑ‚ÑŒ Ñ‚ÐµÐ±Ñ Ð½Ð° Ð¿Ð»Ð°Ñ‚Ñ„Ð¾Ñ€Ð¼Ðµ! ðŸ™Œâœ¨ðŸŒŸ" | "So glad to see you on the platform! ðŸ™Œâœ¨ðŸŒŸ" â€” all three emoji preserved | Drops any emoji, formal phrasing |
| TR-HO-16 | `EN_RU:` user prompt for system â€” "Help me draft a networking message in Hebrew" | "ÐŸÐ¾Ð¼Ð¾Ð³Ð¸ Ð¼Ð½Ðµ Ð½Ð°Ð¿Ð¸ÑÐ°Ñ‚ÑŒ Ð½ÐµÑ‚Ð²Ð¾Ñ€ÐºÐ¸Ð½Ð³Ð¾Ð²Ð¾Ðµ ÑÐ¾Ð¾Ð±Ñ‰ÐµÐ½Ð¸Ðµ Ð½Ð° Ð¸Ð²Ñ€Ð¸Ñ‚Ðµ" | Translates to formal Russian instruction, loses conversational style |

Minimum held-out gate for each training batch:

- 14 of 16 prompts pass product-level review
- Zero emoji dropped in prompts that contain emoji
- Zero formal register in prompts explicitly marked informal/chat
- Zero meaning inversion in any pair

---

## 8. Gate B â€” Three Checks

### Check A â€” Contract and static pair validation

Command:

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj `
  --filter "TranslationDomainTests" --logger "console;verbosity=normal"
```

Expected (current baseline as of phase 24, 2026-05-29):

```text
Passed: 44
Failed: 0
```

Current focused proof test:

```text
TranslationDomainTests.TrainStructuredBatchAsync_PerUnitPairs_ChangeRuntimeOutputForThreeSets
```

This test is intentionally small but runtime-real. It trains three separate
participants in the translation flow:

| Unit | What the accepted/rejected pair teaches | Runtime evidence |
|---|---|---|
| `semantic-tokenizer` | Exact source text must map to the learned structural signature, not the bootstrap signature. | The downstream executor can find the learned KNN translation by signature. |
| `executor-ru_en` | The learned signature maps to the chosen English translation, while the rejected translation is a plausible bad near miss. | Runtime output changes from `[RU_EN:BOOTSTRAP] ...` to the chosen translation. |
| `quality-verifier` | The verifier receives its own accepted/rejected pair; the technical row teaches that a candidate losing the code id `ProcessQueue` must fail. | The trained adapter accepts the learned candidate because it preserves the required code id and does not fall back to bootstrap. |

The three current proof inputs are real product-shaped prompts:

1. Chat: "Ð‘Ñ€Ð°Ñ‚, Ñ ÑƒÐ¶Ðµ Ñ‚Ñ€ÐµÑ‚Ð¸Ð¹ Ð´ÐµÐ½ÑŒ Ð¼ÑƒÑ‡Ð°ÑŽÑÑŒ Ñ ÑÑ‚Ð¸Ð¼ Ñ€Ð¾ÑƒÑ‚ÐµÑ€Ð¾Ð¼ ðŸ˜…"
2. Technical prompt: "Ð´Ð¾Ð±Ð°Ð²ÑŒ Ð¿Ñ€Ð¾Ð²ÐµÑ€ÐºÑƒ null Ð² Ð¼ÐµÑ‚Ð¾Ð´ ValidateToken Ð¸ Ð½Ðµ Ð¿ÐµÑ€ÐµÐ¸Ð¼ÐµÐ½Ð¾Ð²Ñ‹Ð²Ð°Ð¹ ProcessQueue"
3. Business post: "Ð¡ÐµÐ³Ð¾Ð´Ð½Ñ Ñ Ð·Ð°ÐºÑ€Ñ‹Ð»Ð° ÑÐ´ÐµÐ»ÐºÑƒ Ð½Ð° 150Ðº$ Ð±ÐµÐ· Ð¸Ð½Ð²ÐµÑÑ‚Ð¾Ñ€Ð¾Ð² ðŸ”¥ ÐšÐ»Ð¸ÐµÐ½Ñ‚ Ð² Ð²Ð¾ÑÑ‚Ð¾Ñ€Ð³Ðµ."

Focused verification run on 2026-05-29:

```powershell
dotnet build 27.TranslationDomain\27.TranslationDomain.csproj --nologo --verbosity:minimal -m:1 /p:UseSharedCompilation=false
dotnet test Tests\LlmFromScratch.Tests.csproj --filter TranslationDomain --nologo --verbosity:minimal -m:1 /p:UseSharedCompilation=false
```

Result (phase 24, 2026-05-29, commit `a5e74154`):

```text
27.TranslationDomain build: 0 errors
TranslationDomain tests: 44 passed, 0 failed
Structured proof accuracy: 1.0 on the three runtime proof prompts
```

Scope note: this proves the corrected per-unit learning path exists and affects
runtime inference. It does not replace the real 50 train / 50 held-out language
batch runner from `language translate learning`; that remains the next product
training step.

Current batch-029 structured smoke, added in v1.3:

| Test | What it proves | Scope |
|---|---|---|
| `TranslationStructuredBatchBuilder_BuildsThreeUnitExamplesPerTrainReadyPair` | One ready RU_EN pair becomes three unit-specific examples: tokenizer, executor, verifier. The rejected side becomes a concrete near-miss candidate, not only a textual warning. | Builder contract only. |
| `TrainStructuredBatchAsync_LocalBatch029Smoke_WhenAvailable` | The local `batch-029` file is readable as UTF-8, 50 train-ready rows become 150 unit examples, and the trained machine can reproduce the first 5 training rows through runtime inference. | Training-row smoke only; not held-out promotion. |

Local batch-029 smoke evidence:

```text
Source rows: 50
Generated unit examples: 150
Runtime smoke rows: 5
Runtime smoke accuracy on those rows: 1.0
TranslationDomain targeted suite now: 44 passed, 0 failed
```

Important negative evidence: the earlier v7 DPO run for this same batch reported
`FinalAccuracy = 0.4000000059604645` and `FinalRewardMargin = -0.007709846831858158`.
That means batch-029 must not be treated as promoted only because it can now be
converted and memorized in the structured smoke. The next real step is a held-out
runner report that measures generalization, reward margin, CPU, RAM, elapsed
time, checkpoint ref, and failed rows.

Current batch-029 held-out runner, updated in v1.8:

| Runner mode | Corpus rows | Train split | Held-out split | Runtime exact accuracy | Runtime product-marker accuracy | Bootstrap rate | Gate |
|---|---:|---:|---:|---:|---:|---:|---|
| Batch-only baseline | 0 | 40 rows / 120 unit examples | 10 rows / 30 unit examples | 0.000000 | 0.000000 | 1.000000 | failed |
| Previous level-1 corpus only | 1300 | 40 rows + corpus / 4020 unit examples | 10 rows / 30 unit examples | 0.000000 | 0.700000 | 0.000000 | failed |
| Full train-ready language corpus | 1350 | 40 rows + corpus / 4170 unit examples | 10 rows / 30 unit examples | 1.000000 | 1.000000 | 0.000000 | downgraded: train/held-out contamination |

Phase 24 correction: the `Full train-ready language corpus` row is no longer
valid promotion evidence. The expanded corpus root includes rows that duplicate
or strongly overlap the held-out split, so the current runner rejects that mode
with `TRAIN_HELDOUT_OVERLAP`. A batch-029 replay can remain a regression test
for the contamination guard, but it cannot be reported as 85% or 95% language
accuracy.

Current RU_EN product prompt runner, added in v1.10:

| Runner | Train-ready source | Held-out source | Prompt rows | Runtime exact accuracy | Runtime product accuracy | Bootstrap rate | Target script accuracy | Copied-source rate | Wrong-direction rate | Emoji preservation | Named entities | Gate |
|---|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|
| `TranslationProductHeldOutRunner.RunRuEnAsync` | `27.TranslationDomain/TrainingData/ru-en-product-prompt-b001-train-ready.jsonl` | `27.TranslationDomain/TrainingData/tr-ho-ru-en-product-heldout-v1.jsonl` | 8 RU_EN product prompts | 1.000000 | 1.000000 | 0.000000 | 1.000000 | 0.000000 | 0.000000 | 1.000000 | 1.000000 | passed |

Scope note: this is the first real RU_EN product prompt gate for chat, post,
and prompt translation. It does not claim EN_RU, HE_EN, or EN_HE readiness yet.
Those directions still need their own trained adapters and product prompt rows.

Contaminated multilingual product prompt runner, added in v1.11 and downgraded in v1.13:

| Runner | Train-ready source | Held-out source | Prompt rows | Runtime exact accuracy | Runtime product accuracy | Bootstrap rate | Target script accuracy | Copied-source rate | Wrong-direction rate | Emoji preservation | Named entities | Gate |
|---|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|
| `TranslationProductHeldOutRunner.RunAllDirectionsAsync` | `27.TranslationDomain/TrainingData/product-prompt-b001-all-directions-train-ready.jsonl` | `27.TranslationDomain/TrainingData/tr-ho-product-heldout-v2.jsonl` | 16 RU_EN / EN_RU / HE_EN / EN_HE product prompts | 1.000000 | 1.000000 | 0.000000 | 1.000000 | 0.000000 | 0.000000 | 1.000000 | 1.000000 | downgraded: contaminated |

Live probe evidence:

```text
training_rows=16
heldout_rows=16
unit_examples_trained=48
runtime_exact_accuracy=1.000000
runtime_product_accuracy=1.000000
runtime_bootstrap_rate=0.000000
target_script_accuracy=1.000000
copied_source_rate=0.000000
wrong_direction_rate=0.000000
emoji_preservation_accuracy=1.000000
named_entity_preservation_accuracy=1.000000
bleu1=1.000000
chrf1=1.000000
cpu_ms=78
peak_ram_mb=3
elapsed_ms=422
checkpoint_ref=translation-machine/0.1.1/checkpoint
manifest_ref=translation-machine/0.1.1/manifest.json
```

Correction in v1.13: this v2 held-out file duplicates all 16 training `a_side`
inputs. It now fails with `TRAIN_HELDOUT_OVERLAP`. The old metric proves only
that the trained phrase-map path can replay learned rows; it is not held-out
generalization evidence.

Second contaminated multilingual product prompt runner, added in v1.12 and downgraded in v1.13:

| Runner | Train-ready source | Held-out source | Prompt rows | Runtime exact accuracy | Runtime product accuracy | Bootstrap rate | Target script accuracy | Copied-source rate | Wrong-direction rate | Emoji preservation | Named entities | Gate |
|---|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|
| `TranslationProductHeldOutRunner.RunAllDirectionsAsync` | `27.TranslationDomain/TrainingData/product-prompt-b002-expanded-train-ready.jsonl` | `27.TranslationDomain/TrainingData/tr-ho-product-heldout-v3.jsonl` | 16 additional RU_EN / EN_RU / HE_EN / EN_HE product prompts (`TR-HO-17..TR-HO-32`) | 1.000000 | 1.000000 | 0.000000 | 1.000000 | 0.000000 | 0.000000 | 1.000000 | 1.000000 | downgraded: contaminated |

Live probe evidence:

```text
batch_id=product-prompt-b002-expanded-train-ready
training_rows=16
heldout_rows=16
unit_examples_trained=48
runtime_exact_accuracy=1.000000
runtime_product_accuracy=1.000000
runtime_bootstrap_rate=0.000000
target_script_accuracy=1.000000
copied_source_rate=0.000000
wrong_direction_rate=0.000000
emoji_preservation_accuracy=1.000000
named_entity_preservation_accuracy=1.000000
bleu1=1.000000
chrf1=1.000000
cpu_ms=47
peak_ram_mb=3
elapsed_ms=281
checkpoint_ref=translation-machine/0.1.1/checkpoint
manifest_ref=translation-machine/0.1.1/manifest.json
passes_gate=True
```

Correction in v1.13: this v3 held-out file duplicates all 16 training `a_side`
inputs. Current runner behavior:

```text
success=False
error_code=TRAIN_HELDOUT_OVERLAP
overlap_count=16/16
```

The old `runtime_exact_accuracy=1.0` was memorized phrase-map replay, not a
valid held-out score.

Current composed-unseen multilingual product prompt runner, added in v1.13:

| Runner | Train-ready source | Held-out source | Prompt rows | Runtime exact accuracy | Runtime product accuracy | Bootstrap rate | Target script accuracy | Copied-source rate | Wrong-direction rate | Emoji preservation | Named entities | Gate |
|---|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|
| `TranslationProductHeldOutRunner.RunAllDirectionsAsync` | `27.TranslationDomain/TrainingData/product-prompt-b002-expanded-train-ready.jsonl` | `27.TranslationDomain/TrainingData/tr-ho-product-heldout-v4-composed-unseen.jsonl` | 16 composed RU_EN / EN_RU / HE_EN / EN_HE product prompts (`TR-HO-33..TR-HO-48`) | 1.000000 | 1.000000 | 0.000000 | 1.000000 | 0.000000 | 0.000000 | 1.000000 | 1.000000 | passed |

Live probe evidence:

```text
batch_id=product-prompt-b002-expanded-train-ready
training_rows=16
heldout_rows=16
unit_examples_trained=48
runtime_exact_accuracy=1.000000
runtime_product_accuracy=1.000000
runtime_bootstrap_rate=0.000000
target_script_accuracy=1.000000
copied_source_rate=0.000000
wrong_direction_rate=0.000000
emoji_preservation_accuracy=1.000000
named_entity_preservation_accuracy=1.000000
bleu1=1.000000
chrf1=1.000000
passes_gate=True
train_heldout_overlap=0/16
```

Scope note: v1.13 proves composition over learned phrase blocks on full prompts
that are not present in the training file. It is stronger than the contaminated
v2/v3 gates, but still not broad language generalization. The next real target
is 50 train + 50 true held-out per direction/register, including paraphrases,
morphology/noise, and unseen entities.

Observed held-out examples:

```text
ru-en-general-l1-b029-041
expected: She dolefully bid farewell to the house she had lived in for thirty years!
actual:   [RU_EN:BOOTSTRAP] ÐžÐ½Ð° Ð³Ð¾Ñ€ÐµÑÑ‚Ð½Ð¾ Ð¿Ñ€Ð¾Ñ‰Ð°Ð»Ð°ÑÑŒ Ñ Ð´Ð¾Ð¼Ð¾Ð¼, Ð² ÐºÐ¾Ñ‚Ð¾Ñ€Ð¾Ð¼ Ð¿Ñ€Ð¾Ð¶Ð¸Ð»Ð° Ñ‚Ñ€Ð¸Ð´Ñ†Ð°Ñ‚ÑŒ Ð»ÐµÑ‚.

ru-en-general-l1-b029-042
expected: He lingeringly gazed after the departing ship until it vanished beyond the horizon!
actual:   [RU_EN:BOOTSTRAP] ÐžÐ½ Ð´Ð¾Ð»Ð³Ð¾ ÑÐ¼Ð¾Ñ‚Ñ€ÐµÐ» Ð²ÑÐ»ÐµÐ´ ÑƒÑ…Ð¾Ð´ÑÑ‰ÐµÐ¼Ñƒ ÐºÐ¾Ñ€Ð°Ð±Ð»ÑŽ, Ð¿Ð¾ÐºÐ° Ñ‚Ð¾Ñ‚ Ð½Ðµ ÑÐºÑ€Ñ‹Ð»ÑÑ Ð·Ð° Ð³Ð¾Ñ€Ð¸Ð·Ð¾Ð½Ñ‚Ð¾Ð¼.

ru-en-general-l1-b029-043
expected: She tenderly brushed the hair from his forehead as he slept!
actual:   [RU_EN:BOOTSTRAP] ÐžÐ½Ð° Ð½ÐµÐ¶Ð½Ð¾ ÑƒÐ±Ñ€Ð°Ð»Ð° Ð²Ð¾Ð»Ð¾ÑÑ‹ Ñ ÐµÐ³Ð¾ Ð»Ð±Ð°, Ð¿Ð¾ÐºÐ° Ð¾Ð½ ÑÐ¿Ð°Ð».
```

Diagnosis after phase 24: exact-pair KNN can reproduce rows that are present in
the train-ready source, but that is replay, not held-out learning. The stricter
previous-batches-only probe and the 100+100 unique-set loop are the valid
generalization targets. Any future report must show exact overlap count `0` and
similarity overlap count `0` above the 70% threshold before accuracy is counted.

Runtime bugfix slice, added in v1.7:

| Bug | Fix | Runtime purpose |
|---|---|---|
| Fallback candidate confidence was exactly `0.50`, but the adapter required `> 0.5`. | Accept `>= 0.5` when verifier passes, while keeping a fully untrained token fallback at `0.49`. | A learned word-map fallback can produce output instead of being thrown away, but an untrained adapter still falls back to Bootstrap. |
| Executor stored mappings under raw source words but inference looked up lemmas. | Store and look up normalized raw words and lemmas. | DPO word mappings survive tokenizer normalization. |
| Structured builder emitted `ExecutorOutput.WordMappings = null`. | Generate initial concept/adverb/code mappings from each train-ready pair. | E26-style word choice is actually trained instead of remaining empty. |

Targeted proof:

```text
TranslationDomainTests.TrainStructuredBatchAsync_WordMapFallback_UsesLearnedMappingsWithoutExactKnn
```

This proof disables exact KNN by training under a non-runtime signature. The
machine must still translate a technical prompt via learned word mappings and
must not return `[RU_EN:BOOTSTRAP]`.

Additional static checks per new batch:

- Every pair has `direction`, `pair_family`, `validation_outcome: train_ready`
- No `validation_errors` array is non-empty
- Zero pairs from `training-pairs-blocked.jsonl` or `training-pairs-repair-required.jsonl`
- 50 train rows + 50 held-out rows per batch
- `can_holdout=true, can_train=false` on held-out rows

### Check B â€” Training runner report

Current `TranslationBatch029Runner` report scope:

```text
batch_id         = <e.g. ru-en-general-l1-b029>
domain_tag       = TranslationDomain
direction        = RU_EN | EN_RU | HE_EN | EN_HE
pair_family      = <e.g. general-adverb-translation>
source_rows      = numeric
train_rows       = numeric
heldout_rows     = numeric
unit_examples_trained = numeric
unit_examples_heldout = numeric
accuracy_before  = numeric
accuracy_after   = numeric
reward_margin    = numeric
runtime_exact_accuracy = numeric
runtime_product_accuracy = numeric
runtime_bootstrap_rate = numeric
cpu_ms           = numeric
peak_ram_mb      = numeric
elapsed_ms       = numeric
checkpoint_ref   = non-empty string
report_path      = non-empty path
gate_passed      = true | false
```

Target promotion metrics that are required before Direction 1 can be called a
trained product translator, but are not yet emitted by the current runner:

```text
bleu
chrf
target_script_accuracy
copied_source_rate
wrong_direction_rate
named_entity_preservation
completeness_score
product_prompt_suite_pass_rate
```

This distinction is deliberate. A run report that lacks these target promotion
metrics may be useful for debugging, but it is not enough to claim product
readiness.

Minimum numeric gate:

```text
accuracy_after  >= accuracy_before + 0.02
reward_margin   >= 0.025
runtime_product_accuracy >= 0.85
runtime_bootstrap_rate   <= 0.15
```

Report must be persisted to disk before `TrainBatchAsync` returns.
A `LearningBatchResult` without a persisted report does not satisfy Gate B.

Run-report interpretation from the fixed guidance:

```text
ACCEPT_BATCH      - continue the same stream or promote level
REPAIR_BATCH      - rejected/source/tokenizer/router weakness is repairable
REJECT_BATCH      - source or pair shape is unsafe/noisy
SOURCE_BLOCKED    - registry forbids the source
TOKENIZER_BLOCKED - script coverage or byte fallback is too poor
```

If DPO margin improves while BLEU/chrF, target-script accuracy, or
copy-source/wrong-direction rates collapse, the batch is not accepted. That is a
preference-artifact failure, not language learning.

### Check C â€” Runtime held-out prompt suite

Run the trained machine through the 16 held-out prompts in Section 7.

Current executable status in v1.12: `TranslationProductHeldOutRunner` executes
the full `TR-HO-01..TR-HO-16` suite and the additional `TR-HO-17..TR-HO-32`
suite through `RunAllDirectionsAsync`. Each runner trains 48 unit-level examples
from 16 source-governed train-ready rows and emits exact/product accuracy,
bootstrap rate, target-script accuracy, copied-source rate, wrong-direction
rate, emoji preservation, named-entity preservation, BLEU-1, and chrF-1.

Required:

- Machine loads through `ILearningMachine<TranslationDomain>` â€” not Bootstrap adapter
- `GetOrchestratorAsync()` returns a usable orchestrator
- Each output is checked against the "reject if" column
- Failures are written to `translation-heldout-error-ledger.jsonl`
- No completion claim until runtime prompt report is persisted

Check C is not required for batch 001 (Bootstrap phase) but is required before
promoting any trained direction adapter to production.

---

### Phase 24 live rule-matrix set results

Historical phase-24 target after the earlier user upgrade was
`runtimeProductAccuracy >= 0.95` on every then-verifiable set, with accuracy
shown after each stage. The active Tatoeba base-loop target for the current run
is `runtimeProductAccuracy >= 0.85`.

| Set | Train rows | Held-out rows | Supplemental rows | Product accuracy | Exact accuracy | Bootstrap rate | Gate |
|---|---:|---:|---:|---:|---:|---:|---|
| General marker/product transfer | 100 | 100 | 23 private repair rows | 0.960 | 0.000 | 0.010 | passed product gate |
| Agreement/case context | 100 | 100 | 0 | 1.000 | 1.000 | 0.000 | passed product + exact gate |

Important boundary: the 23 supplemental marker rows are private temporary repair
evidence. They must be converted into source-governed train-ready artifacts, or
rebuilt from approved language-rule sources, before this slice can be called a
publishable base-language corpus. They are valid live evidence for the algorithm
path and for the next authoring target; they are not committed raw training
data.

The agreement/case context set specifically exercises:

```text
subject + past verb agreement
subject marker transfer: she/he/the cat/the girl/the boy
dative-like preposition context
unique-heldout filter with >70% training-history overlap removed
rule-tag KNN path: current word mappings beat unrelated KNN neighbors
```

Phase 24 verification:

```text
27.TranslationDomain build: 0 warnings, 0 errors
TranslationDomain targeted suite: 44 passed, 0 failed
Legacy contaminated batch-029 promotion tests now assert rejection:
  TRAIN_HELDOUT_OVERLAP
  TRAIN_HELDOUT_SIMILARITY_OVERLAP
```

### Phase 25 Code Repairs Completed (do not regress)

The following phase-24 gaps were repaired in phase 25. They are now regression
guards: any future code or test change that reintroduces the old value is a
protocol violation.

| Gap | File | Required current value |
|---|---|---|
| Runner gate threshold | `TranslationBatch029Runner.cs` | `runtimeProductAccuracy >= 0.95` |
| Test assertion 1 — product accuracy | `TranslationDomainTests.cs` | `>= 0.95` |
| Test assertion 2 — exact accuracy | `TranslationDomainTests.cs` | `>= 0.95` |
| Test assertion 3 — batch runner gate | `TranslationDomainTests.cs` | `>= 0.95` |
| Mojibake test data | `TranslationDomainTests.cs` agreement/rule-tag cases | Real UTF-8 Cyrillic |

Do not report the domain as meeting the active 0.85 Tatoeba base-loop target or
the historical 0.95 production target unless the live runner prints the accuracy
metrics for the current 100+100 train/test iteration.

For Tatoeba 100+100 runs, lexicon induction must prefer repeated source-target
co-occurrence before specificity and must reject one-off mappings. One-off
source-target guesses created word salad such as stable Russian words mapping to
rare accidental English words; those mappings are not valid DPO evidence.
Phase 40 tightens this for EN_RU: a word mapping is trainable only when support,
source-side confidence, target-side specificity, and score margin over the
runner-up all pass. High-frequency junk targets such as negation words and
proper-noun-looking source noise are rejected before `[AGREEMENT:]` mappings are
emitted. Tatoeba phrase signal is now induced separately from repeated 2-4 token
source phrases with repeated plausible target ngrams, capped per row; full
source-sentence phrase tags are not the only phrase signal.

---

## 9. Export / Import Lifecycle

This is how trained weights are downloaded and uploaded between instances.

### Export (after Gate B passes)

```csharp
// Inside the executor session, after TrainBatchAsync + EvaluateAsync pass:
var exportResult = await translationMachine.ExportAsync();

// exportResult.Value contains:
//   MachineId     = "translation-machine"
//   DomainTag     = "TranslationDomain"
//   Version       = "0.1" (increments per batch)
//   CheckpointRef = "translation-machine/0.1/checkpoint"
//   ManifestRef   = "translation-machine/0.1/manifest"
//   AccuracyAtExport = <numeric from last EvaluateAsync>
//   ExportedAtUtc = <timestamp>
```

### Publish to ModelServer (25.ModelServer)

```csharp
// Direction 5 adds ILearningMachineRepository â€” that is the full registry path.
// Until Direction 5 is complete, use the existing checkpoint/manifest/locator
// boundary: domain manifest publisher -> GeneratedCapabilityCheckpointManifest
// -> IModelRepository.UploadAsync(manifest, serverBaseUrl).
await modelRepository.UploadAsync(generatedCheckpointManifest, serverBaseUrl);
```

### Import on another instance

```csharp
// Another session, another executor, same or different machine:
var freshMachine = new TranslationMachine();
var importResult = await freshMachine.ImportAsync(export);
// importResult.Value = true when import succeeded
// After import: freshMachine.GetOrchestratorAsync() reflects imported weights
```

### Rollback

`RemoteUnitManifest.rollback_ref` does not exist yet. Direction 5 must add the
rollback field and the executed rollback/load probe before automatic rollback can
be claimed.

Until Direction 5 is complete: keep the previous `CheckpointRef` and previous
`LearningMachineExport` value in the runner report artifact. Manual rollback =
import the previous export object and rerun the held-out prompt report.

---

## 10. Parallel Executor Strategy

This section directly answers: how can 3 executors work in parallel, and what
happens with branches and merging?

### What can run in parallel

Inside a single executor session, all four translation directions can train in
parallel because each direction adapter has its own state:

```csharp
// Each machine runs independently â€” no shared mutable state
var ruEn  = new TranslationMachine(); // trains RU_EN direction adapter
var heEn  = new TranslationMachine(); // trains HE_EN direction adapter
var enRu  = new TranslationMachine(); // trains EN_RU direction adapter

await Task.WhenAll(
    ruEn.TrainBatchAsync(ruEnBatch),
    heEn.TrainBatchAsync(heEnBatch),
    enRu.TrainBatchAsync(enRuBatch)
);
```

Resource cap: maximum 3 concurrent `TrainBatchAsync` calls (configured in
`PARALLEL-TEACHING-AND-EVALUATION-GUIDANCE-v1.md Â§1`).

### What needs sequential execution

A new direction's first batch (Bootstrap â†’ first real batch) must be sequential
because AccuracyBefore depends on the previous checkpoint. Once the checkpoint
exists, subsequent batches for different directions can run in parallel.

### Git branch strategy for 3 parallel executors

Three executors working on different DOMAINS (translation + affiliate + wiki)
can each use their own feature branch with minimal conflict:

```text
Executor 1 â†’ branch: codex/translation-training
  Works in: 27.TranslationDomain/
  Changes to shared files: docs/plans/ (add translation batch notes)

Executor 2 â†’ branch: codex/achoti-entrepreneurship
  Works in: 31.AffiliateDomain/
  Changes to shared files: docs/plans/ (add affiliate notes)

Executor 3 â†’ branch: codex/codewiki-training
  Works in: 42.ProjectWikiDomain/ (CodeWikiMachine only)
  Changes to shared files: docs/plans/ (add wiki notes)
```

Shared files that all three may touch â€” coordinate via PR:
- `docs/plans/STATE-PARALLEL-DOMAIN-TRAINING.json`
- `docs/plans/PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md`
- `Tests/LlmFromScratch.Tests.csproj` (when each adds new tests)
- `LlmFromScratch.sln` (when each adds new project)

**Rule:** Each executor only modifies state fields for its own direction.
`direction_1_translation.*` is owned by Executor 1 only.
`direction_3_affiliate.*` is owned by Executor 3 only.
This eliminates merge conflicts in `STATE-PARALLEL-DOMAIN-TRAINING.json`.

### Merge order

```text
1. Each executor opens a PR when Gate B passes for that direction.
2. PRs are reviewed and merged independently â€” no waiting for other directions.
3. After merge to main, origin/main is pulled before any new executor batch starts.
4. ModelServer is updated on merge â€” weights are NOT in git, they are in ModelServer.
```

### Current project convention

This project normally uses the current branch for a single executor. If parallel
execution sessions are started, create `codex/` branches as shown above only
after Luba explicitly approves branch creation. If only one executor session is
running at a time (sequential inter-domain, parallel intra-domain), staying on
the current branch is sufficient.

---

## 11. Completion Definition

TranslationDomain (Direction 1) is fully trained when:

- Gate A: 44/44 targeted translation tests pass, including the structured
  per-unit runtime proof.
- Gate B: At least one real (non-Bootstrap) batch passes with AccuracyAfter >= AccuracyBefore + 0.02 and RewardMargin >= 0.025
- Gate C: Runtime held-out suite (Section 7) passes with 14/16 prompts correct and zero emoji dropped
- Directions covered: at minimum RU_EN and HE_EN (the two product-critical directions for the chat system)
- Export published: `LearningMachineExport` with numeric accuracy exists in ModelServer
- Report persisted: runner report artifact on disk before completion claim

Current status: Gate A scaffold/contract tests, the structured per-unit proof,
local batch-029 structured smoke, batch-029 held-out runner, source-governed
phrase repair artifact, RU_EN product prompt runner, contamination guard, and
the composed-unseen all-direction product prompt runner are done. The old v2/v3
all-direction held-out files are quarantined because they duplicate training
inputs. The current valid v1.13 gate is `TR-HO-33..48`: 16 non-overlapping
composed product prompts with 1.000000 exact accuracy, 1.000000 product
accuracy, and Bootstrap rate 0.000000. This is a composed phrase-transfer gate,
not a claim that every language/domain/register is solved.

---

## 12. Training Run Control Rules (OP-36)

**Never start or restart a training run without explicit user instruction.**

A training run (`dotnet run … run-tatoeba-loop`, `Start-Process`, or any
equivalent launcher) may only be started when the user's current message
explicitly requests it. Words like "continue", "show accuracy", or "proceed"
mean tail the existing log — not start a new process.

**Accuracy slowdown is normal — do NOT react by restarting:**

As training progresses, product accuracy per 100-set may decrease or plateau.
This is expected behaviour:

- Larger history means fewer unseen structural patterns per new 100-set.
- The uniqueness filter removes easy repeats, leaving only harder pairs.
- The model approaches saturation on the current data distribution.

Slowdown is a data signal, not a failure. Do not switch direction, change
parameters, or start a new runner in response to slowdown without user approval.

**If the current 100-set pool is exhausted** (test_rows < 100 after uniqueness
filter), report the shortfall and wait for user instruction. Do not
auto-switch to EN→RU or any other direction.

**Allowed without explicit instruction:**
- Reading / tailing an existing log file
- Printing accuracy lines from a running or completed process
- Reporting that the pool is exhausted

**Requires explicit instruction:**
- Starting any new `dotnet run` / `Start-Process` for training
- Switching language direction
- Changing `--target-accuracy`, `--max-cycles`, or any other parameter
- Stopping a running process

---

## 13. Novelty Gate Reporting Rules (OP-37)

**Never describe a novelty-gate exclusion with vague language.**

A set is promoted to training by the adaptive novelty gate only when:

```
newPattern > (1 - maxAccuracy) AND seenTokens < maxAccuracy
initial fallback maxAccuracy=0.78 => newPattern > 0.22 AND seenTokens < 0.78
```

**Correct reporting template:**

```text
Set N: product=X%, newPattern=Y%, seenTokens=Z%
-> SKIP_REQUIRES_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M> (newPattern=Y% <= threshold or seenTokens=Z% >= threshold)
→ not added to training. train_rows unchanged.
```

**Forbidden phrasings:**

| Wrong | Why wrong |
|---|---|
| "не промотирован" | Promotion = accuracy gate. Novelty gate is a separate, earlier filter. |
| "почти повтор по токенам/паттернам" | Vague — does not cite the rule name or exact thresholds. |
| "filtered out" without citing the rule | Ambiguous — could mean uniqueness filter or novelty gate. |

**Three distinct `promotionDecision` values — never confuse them:**

```text
PROMOTE_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M>       -> adaptive novelty gate passed
SKIP_REQUIRES_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M> -> adaptive novelty gate failed
SKIP_SHORTFALL                                          -> test pool < 100 unique rows
```

A set with 58% product accuracy that hits the novelty gate is NOT a promotion
failure. It is a data-quality exclusion. The accuracy number is irrelevant to
the novelty gate decision.

### 13.1 Target Script Gate For EN_RU

EN_RU composition and Tatoeba runtime metrics are direction-aware. A Russian
target row must contain Cyrillic and must not mix Latin letters into the
ordinary translated surface unless a future named-entity policy explicitly
allows that exception. RU_EN keeps the inverse guard: Cyrillic target output is
not accepted as English word-map composition.

Latin identity mappings such as `ordinary => ordinary` must not be stored for
EN_RU natural-language word maps. A Tatoeba EN_RU row whose actual output mixes
Latin and Cyrillic must report `TargetScriptOk=false`, and `ProductMatch` must
stay false whenever `TargetScriptOk=false`.
Phase 40 also makes the product metric reject contradictory extra negation:
token overlap of 1.0 is not enough when the candidate adds target-language
negation that is absent from the expected target. This prevents rows like
`я не вижу дверь` from passing against `я вижу дверь` only because all expected
content words are present.

### 13.2 Source-Aware KNN Fallback For EN_RU

KNN fallback may use full, prefix, and structure buckets, but broad prefix or
structure buckets must also pass source-token similarity. A low-information
signature such as `SVO|NOUN|VERB|PREP|NOUN` cannot return an unrelated memorized
sentence like "Tom walked across the street" for an arrest or unlock source.
Exact learned rows still work; broad fallback now requires enough overlapping
source tokens to be a same-family neighbour.

---

## 14. Skipped Set — Continue Forward, Never Restart (OP-38)

When any set is skipped (novelty gate, shortfall, or other filter):

```text
✓ Advance the corpus pointer past the skipped block
✓ Try the next 100-row candidate
✓ Keep train_rows and cycle counter unchanged
✓ Continue — do not restart from cycle 1
```

**Do not stop to fix the runner when a set is skipped.**

If a code issue in the runner is noticed during a training run, note it as a
deferred task. Do not stop training to fix it. The main goal is:
continue training with better sets, show accuracy.

**Forbidden:**

```text
❌ Restart from cycle 1 after any skip
❌ Reset train_rows to 0
❌ Stop to explain or fix runner code mid-run
❌ Meta-commentary ("я соскользнул в объяснение") instead of continuing
```

---

## 15. Architectural Infrastructure Gaps — Raise, Do Not Patch (OP-39)

When a required infrastructure capability is absent (persistence, checkpoint,
serialization, export of trained weights):

**Required behavior:**

```text
1. Name the gap explicitly:
   "ExportAsync does not persist unit weights.
    After process restart, trained KNN store / corrections are lost."

2. Record it as a named deferred architectural task.

3. Do NOT implement a workaround that hides the gap:
   - No replay (--skip-cycles) to rebuild in-memory state
   - No scripts that simulate persistence
   - No claims that the workaround "continues" when it actually replays

4. If user authorizes proceeding despite the gap:
   state clearly what is lost and when.
```

**Forbidden patterns:**

```text
❌ --skip-cycles / --replay to rebuild state after process kill
❌ Reporting a replay workaround as "the model continues from last checkpoint"
❌ Claiming Export/Import works when it saves only metadata, not weights
❌ Masking a missing capability with infrastructure that simulates it
❌ Patching the gap and calling it fixed without real persistence
```

**Why this matters:**

A masked gap is worse than a named gap. A named gap can be planned and fixed.
A masked gap creates false confidence that training state persists when it does not.
The trained model can be lost silently after any process restart.

**Violation example (2026-05-29):**

```text
TranslationMachine ExportAsync saved only metadata.
Model invented --skip-cycles workaround to replay training data.
Reported it as "continuing from last checkpoint."
User: "А почему вместо того что бы поднять что есть архитектурная дыра —
       идет такое левое решение"
```

Score penalty: -1000 (same class as goal substitution / masked false completion).
Rule reference: AGENTS.md OP-39.

---

## 16. Architect-Supervised Training Packets (OP-41)

When the parent session is ARCHITECT_INTERNAL, governance-only, or
sub-agent-only, the architect does not run TranslationDomain training directly.
The architect prepares bounded packets for a training sub-agent and reviews the
returned evidence.

Every training sub-agent packet must require exactly:

```text
model label: gpt-5.5 Extra High
tool model: gpt-5.5
reasoning_effort: xhigh
provider equivalent: sonnet latest with High reasoning when that is the configured available provider
```

No lower-tier training sub-agent, silent fallback, inherited default,
unspecified model, or weaker reasoning effort may run TranslationDomain
training. If neither `gpt-5.5 Extra High` (`gpt-5.5`,
`reasoning_effort=xhigh`) nor configured `sonnet latest` with `High` reasoning
is available, architect records a real blocker and discusses it with Luba.
Architect must not run training locally and must not use a weaker sub-agent.

This section applies only after the checkpoint/persistence hole is closed. Until
real checkpoint load works, training cannot honestly resume across process
boundaries.

### 16.1 Packet shape

Each training packet must name:

```text
work_order_id
sub_agent_model_label = gpt-5.5 Extra High
sub_agent_model = gpt-5.5
reasoning_effort = xhigh
allowed_highest_order_provider_equivalent = sonnet latest with High reasoning when configured available
approved_plan_section
checkpoint_path
writer_agent_id
checkpoint_writer_exclusive = true
source_file_or_corpus
source_meets_pilot_gate
train_range_or_cursor
test_range_or_cursor
train_set_size = 100
test_set_size = 100
direction
target_accuracy
allowed_runner_command
max_training_chunk_boundary
heartbeat_cadence = 10-15 minutes or after each 29-cycle chunk
forbidden_commands
expected_report_path
return_condition
```

The training sub-agent may run only the command named in the packet. If the
packet does not allow a command, the sub-agent must not run it. If the packet
does not explicitly name `gpt-5.5 Extra High` / `gpt-5.5` /
`reasoning_effort=xhigh`, or configured `sonnet latest` / `High` reasoning, it
is invalid and architect must repair it before execution.

If `source_meets_pilot_gate` is false or unknown, the packet must not allow an
active checkpoint write. The sub-agent returns source-gate evidence and code
enforcement becomes the next repair task.

### 16.2 Required progress deltas

Every returned training packet must include:

```text
accuracy_percent
exact_percent
bootstrap_percent
new_tokens_percent
new_structures_percent
newPattern_percent
seenTokens_percent
train_rows_before
train_rows_after
train_rows_delta
promotionDecision
promotionMaxAccuracy
minimumPromotionNewPatternRate
maximumPromotionSeenTokenRate
bestRuntimeProductAccuracy
checkpoint_loaded
checkpoint_saved
report_path
```

The architect report to Luba must include the deltas, not only "trained" or a
JSON path. `promotionDecision` must use the exact runner value:

```text
PROMOTE_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M>
SKIP_REQUIRES_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M>
SKIP_SHORTFALL
```

### 16.3 Architect review loop

After each packet:

```text
1. Architect reads the returned report/log/checkpoint evidence.
2. Architect compares it to the packet and this guide.
3. If the packet has a gap, architect sends a bounded correction work order to a
   sub-agent.
4. If the packet is valid and the target is not reached, architect prepares the
   next 100-train/100-test packet.
5. Continue until Luba stops, the target gate closes, or a real blocker is
   recorded.
```

Architect must not run the runner, tests, or build as a shortcut. If no training
sub-agent capability is available, record the blocker instead of doing executor
work in the parent architect session.

## 17. Checkpoint Writer Guard And Recovery Mode (OP-42)

The checkpoint writer guard is mandatory before any TranslationDomain training
or recovery work resumes after checkpoint contamination.
Practical work-order checklist:
`docs/plans/TRANSLATION-CHECKPOINT-WRITER-GUARD-HOWTO.md`.

### 17.1 Parent session boundary

The parent/main session is ARCHITECT ONLY for this project. It may plan, review,
read evidence, communicate, and repair governance/protocol/settings/docs when
Luba explicitly asks for that repair. It must not write production code, write
test code, run `dotnet test`, run `dotnet build`, run `dotnet run`, start
training runners, call `Start-Process`, or mutate model checkpoints.

### 17.2 Single-writer checkpoint rule

Before any training run, architect assigns:

```text
checkpoint_path
writer_agent_id
source_file_or_corpus
allowed_runner_command
max_training_chunk_boundary
heartbeat_cadence
```

Only the assigned writer may write `checkpoint_path` for that chunk. A second
writer must not use the same checkpoint while the first writer owns it. No agent
may start a runner while another run/build/training pipeline owns the
checkpoint. Experiments use isolated checkpoint copies unless the work order
explicitly grants active checkpoint write access.

### 17.3 Bounded continuation rule

One work order authorizes exactly one code slice or exactly one training chunk.
The sub-agent returns an evidence packet after that slice/chunk. It must not
chain source rollover, runner restart, another checkpoint write, or autonomous
continuation unless the work order names each extra action with exact limits.

### 17.4 Source gate rule

No active checkpoint save/write is valid after a source with
`MeetsPilotGate=false`. If `MeetsPilotGate` is false or unknown, the runner must
not write the active checkpoint. The sub-agent returns source-gate evidence to
architect.

This is governance now. Code enforcement is a required next repair task, and
the lack of code enforcement is not permission to save.

### 17.5 Heartbeat rule

Long-running training sub-agents report status every 10-15 minutes or after each
29-cycle chunk. The heartbeat must include:

```text
checkpoint_path
writer_agent_id
source_or_cursor
last_ACCURACY_SET
checkpoint_loaded
checkpoint_saved
next_bounded_action
```

If a writer is silent past the heartbeat window, architect may read-only inspect
logs/status and then close or stop that writer before issuing another writer for
the same checkpoint.

### 17.6 Mandatory ACCURACY_SET fields

Every 100-set output must include:

```text
product
exact
bootstrap
train_rows
newPattern
seenTokens
promoted
promotionDecision
```

The adaptive novelty gate remains strict:

```text
newPattern > (1 - maxAccuracy) AND seenTokens < maxAccuracy
```

The runner must not report the set as promoted unless both sides are true.

### 17.7 Recovery mode

A contaminated checkpoint must not be used for continuation. The only allowed
recovery paths are:

```text
rollback to a clean checkpoint
forensic replay of approved reports with explicit Luba approval
```

Until one recovery path is explicitly selected, no new active checkpoint writer
may be issued.

---

## Phase 38 — Model Asset Lifecycle (Added 2026-06)

Phase 38 extended the translation domain with a complete model asset lifecycle. After training, the model must pass through this lifecycle before being considered production-ready.

### CLI commands (27.TranslationDomain.Tools)

| Command | Purpose |
|---|---|
| `export-translation-model` | Export trained checkpoint to models/translation/{version}/translation-{version}-checkpoint.json + manifest.json |
| `import-translation-model` | Import a checkpoint from a manifest path (fresh-load probe) |
| `publish-translation-model` | Register the model in models/index.json for locator discovery |
| `load-translation-model` | Load model via IModelRepository / IRemoteUnitLocator (validation gate) |
| `continue-translation-training` | Resume training from committed corpus.jsonl.gz (not --private-pairs) |

### Checkpoint requirements (Phase 38 gate)

A complete translation checkpoint must include:
- Real KNN (K-Nearest Neighbours) / trained state — NOT loop metadata only
- manifest.json with checkpoint SHA-256 reference
- runner-report.json with numeric metrics: accuracy, exact, bootstrap, train_rows, newPattern, seenTokens
- corpus.jsonl.gz with no absolute private paths; corpus_sha256 in training-manifest.json

### Phase 38 model readiness definition

Translation model readiness after Phase 38 means ALL of:
1. export-translation-model succeeds and produces manifest.json + checkpoint.json
2. import-translation-model (fresh-load probe) loads the exported checkpoint without error
3. publish-translation-model registers the model in models/index.json
4. load-translation-model (via locator) loads the published model
5. continue-translation-training runs successfully from committed corpus.jsonl.gz

Running training and reporting accuracy is NOT sufficient for Phase 38 readiness. All 5 lifecycle steps must pass.
