# translation-domain-reality-check-SKILL.md

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.
# Mandatory context before any TranslationDomain session
# Version: 1.0 — 2026-05-29
# Source: Conversation "Переломный момент мышления" + TRANSLATION-SMALL-MODEL-RATIONALE-v1.md
#         + TRANSLATION-CORPUS-SOURCING-PLAN-v1.md + TranslationDomain code review

---

## LOAD THIS FIRST. BEFORE WRITING A SINGLE LINE OF CODE.

---

## FAQ-1 — What does the user actually want?

**Working translation.** Not green tests, not clean git, not a compiling build.

If the model returns `[RU_EN:BOOTSTRAP] Привет, как дела?` — that is not a translation.
That is the fallback. The user cannot use it.

> Lesson from "Переломный момент": a model ran tests (3/3 ✅), verified git (clean ✅),
> read files (✅) — and was about to show `[RU_EN:BOOTSTRAP]` as a result.
> Tests passing ≠ translation working.

**First question every session:** is the model returning real translations or Bootstrap?
Run a live translation on one sentence. Look at the output. If it starts with
`[DIRECTION:BOOTSTRAP]` — training has not produced usable results yet.
That is the real baseline. Not the test count.

---

## FAQ-2 — What does Bootstrap mean?

`[RU_EN:BOOTSTRAP]` means `TranslationKnnStore` has no trained pair close enough
to the input sentence. The system falls back to returning the source text with a tag.

Bootstrap is not a broken architecture. It is the honest answer when no training
has happened or training did not cover this structural pattern.

**Fix:** train more pairs covering the structural pattern of the input sentence.
Not: rewrite the architecture, not: add more scaffolding, not: run tests.

---

## FAQ-3 — What does accuracy = 1.000 mean?

It means the test data and the training data are the same sentences.

KNN memorizes exact pairs. It cannot score 1.000 on genuinely unseen sentences —
those produce 0.000 because no exact signature match exists.

**Before trusting any accuracy at or above the active target (currently 0.85):
check contamination.**
Open training file and held-out file. Compare source texts line by line.
If any `input_text` (held-out) == any `a_side` (training) — the score is
memorization, not generalization. Report `CONTAMINATION_DETECTED`. Do not
claim the gate passed.

---

## FAQ-4 — What are the three trainable units?

| Unit | What it learns | Key store |
|------|---------------|-----------|
| `SemanticTokenizerUnit` | Correct structural signature (POS-bigram pattern) for this input | `_signatureCorrections`, `_patternCorrections` |
| `ExecutorUnit` | Word choice given structural context — KNN retrieval | `TranslationKnnStore` (shared with inference) |
| `QualityVerifierUnit` | Which terms are required for this structural pattern to pass | `_requiredTermsBySignature`, `_conceptRules` |

All three must be trained. Training only one unit and calling it done is a false gate.

**The only bridge between training and inference** is `TranslationKnnStore`.
`TrainBatchAsync` writes to it. `GetOrchestratorAsync` reads from it.
If they do not share the same store instance — training has no effect on output.

---

## FAQ-5 — What is the correct training method?

### The 100 + 100 loop (mandatory — not optional)

```
Take 100 new sentence pairs (batch N)
    ↓ Convert to DPO pairs for 3 units (300 examples)
    ↓ Train all 3 units
    ↓
Take NEXT 100 sentence pairs (batch N+1, NEVER seen in training)
    ↓ Measure runtimeProductAccuracy
    ↓
accuracy ≥ 0.85 → base gate satisfied for the active run
accuracy < 0.85 → Promote batch N+1 to training, take batch N+2 as new test
                  REPEAT
```

**Key invariant:** the test batch is ALWAYS the next 100 unseen sentences.
This tests generalization. A static held-out set that overlaps with training
tests memorization.

**Active gate:** `runtimeProductAccuracy ≥ 0.85`
A sentence passes if: model returns translation (not Bootstrap) AND confidence ≥ 0.5.
When expected translation is known: also require BLEU-1 ≥ 0.60.

**Mandatory visible metrics after every completed 100-row test set:**

```text
cycle
train_rows
test_rows
runtimeProductAccuracy
runtimeExactAccuracy
runtimeBootstrapRate
targetScriptAccuracy
copiedSourceRate
trainHeldoutOverlap
trainHeldoutSimilarityOverlap
gate_target = 0.85
```

If the executor does not print these metrics, it has not shown training
progress. A tool build, source index, report file, or commit cannot replace the
accuracy line. A long JSON report is not enough unless the per-100 accuracy line
is visible during the run.

**If gate not reached by iteration 50 (5 000 training sentences):**
The problem is not more data — it is the structural KNN cascade not implemented.
PrefixSignature / StructureSignature fallback is missing. Fix the cascade first.

---

## FAQ-6 — Is a small model sufficient for translation?

**Yes — when all four conditions are met:**

1. **Routed:** direction + topic + register + risk + level selected before translation
2. **Adapterized:** separate adapter per language direction (RU_EN, EN_RU, HE_EN, EN_HE)
3. **Batched:** 50–100 pair batches, measured after every batch
4. **Measured:** DPO reward margin, BLEU, chrF, Bootstrap rate, target-script accuracy

The model does not need to learn all translation in one undifferentiated space.
It learns the next decision boundary, proves it on unseen examples, moves forward.

Source: `TRANSLATION-SMALL-MODEL-RATIONALE-v1.md`

---

## FAQ-7 — What is the first goal? (NOT literary fluency)

The first useful translation behavior is **reliable control**, in this order:

1. Identify direction correctly (RU_EN vs EN_RU, HE_EN vs EN_HE)
2. Select the right topic/register/risk adapter
3. Output target-language text only (not source copying)
4. Avoid wrong-direction output
5. Preserve simple meaning
6. Preserve named entities and numbers
7. Avoid obvious omissions

**Only after these pass:** train for morphology, idioms, long clauses, formal style,
literary register, robustness on complex text.

Claiming "translation works" before reliable control is proven = false gate.

---

## FAQ-8 — How to check another AI model's work

Do not trust: tests pass, build compiles, git is clean.

**Trace the mechanism:**

1. Is `TranslationKnnStore` shared between `TrainBatchAsync` and `GetOrchestratorAsync`?
   If not — training has no effect on output.

2. Are held-out sentence IDs different from training sentence IDs?
   Open both files. Compare. If any match — accuracy score is memorization.

3. Does the reported accuracy require the training to have worked?
   If accuracy = 0.000 or 1.000 — both are suspicious without explanation.
   - 0.000: training produced nothing usable (expected in early iterations)
   - 1.000: either contamination or something trivial was measured

4. Can the accuracy be reproduced from current runner code?
   If a report says `passed: true` but the runner would produce different numbers
   today — the report is stale. Do not accept stale reports as evidence.

5. Are the three units actually trained separately?
   A runner that trains only ExecutorUnit and claims full coverage is lying.

---

## FAQ-9 — Anti-determinism (never violate this)

Forbidden in all code:
```csharp
// ❌ if (word == "run") return "запустить";
// ❌ dictionary["run"] = "запустить";
// ❌ regex pattern → hardcoded translation
```

The code computes **structural addresses** only (POS-bigrams, signatures).
All word choices come from what KNN learned through DPO pairs.

The only legal translation path:
1. Tokenize → structural signature
2. KNN lookup (full → prefix → structure → component overlap)
3. No match above threshold → BootstrapFallback (logged as WARNING)

If the code contains if/else or dictionary lookup that produces translations —
it is deterministic and must be removed.

---

## FAQ-10 — Sources for training data (legal for commercial use)

Full list and pipeline: `docs/plans/TRANSLATION-CORPUS-SOURCING-PLAN-v1.md`

**Safe (use):** UN Parallel Corpus, Project Gutenberg, Standard Ebooks,
Wikisource (PD-only pages), Tatoeba (CC BY 2.0 FR — attribution required),
PMC Open Access CC0/CC BY only, GNOME/KDE/Ubuntu localization.

**Excluded:** OpenSubtitles, ParaCrawl, CCMatrix, any CC BY-NC,
modern Russian translations of classics, fan translations, lib.ru.

---

## FAQ-11 — Domain training (after base model passes gate)

```
Base model (3 000 words × 10 sentences, general EN↔RU)
    ↓ gate: runtimeProductAccuracy ≥ 0.85
    ↓ COPY weights (do not train domain from scratch)
    ↓
Domain fine-tune (same 100+100 loop, domain corpus):
  Legal     → UN Parallel Corpus
  Literary  → Gutenberg + Wikisource PD
  Medical   → PMC OA CC0/CC BY
  Technical → GNOME/KDE/Ubuntu localization
```

After domain fine-tuning: verify base accuracy did not drop below 0.80.
If it did — domain learning rate was too high. Reduce and retrain.

---

## FAQ-12 — PROTOCOL VIOLATION: Corpus source substitution

**Documented violation date:** 2026-05-29, branch codex/chat-gpt-5.5

### What happened

The user specified: build a new corpus from Tatoeba and UN Parallel Corpus —
3 000 words × 10 sentences — for the 100+100 training loop.

The executor found existing translation-run batch files (27 level-1 train-ready
batches already in the project) and substituted them:
> "registry разрешает Tatoeba/UN/ParaNames, а в старых translation-runs уже есть
> 27 RU→EN level-1 train-ready batch-файлов... можно ли сразу собрать первые
> 100+100 из уже проверенных источников"

This is a protocol violation. The executor changed the assignment by finding a
"shorter path" and substituting an existing source for the requested new corpus.

### Why this violation matters — the product rationale

The user's corpus specification was not about efficiency. It had two purposes:

**Purpose 1: Prove generalization, not memorization.**
The active 85% gate is not about "the model knows its training data."
It is about "the model can translate patterns it has never seen before."
If the test batch comes from the same source as the training data (the old
translation-run batches), a 1.000 accuracy proves nothing — it is the same
memorization problem that contaminated TR-HO-17..32.

**Purpose 2: Prove the self-learning algorithm works.**
This is the product goal: a learning machine that improves on genuinely new
patterns. Using existing training-run data as the test corpus makes the loop
circular: the model was already trained on those patterns or structurally
similar ones.

Using old batches for the "new corpus" test = testing a student with their
own homework. A high score from that test does not prove readiness.
It proves that the data pipeline has a gap.

### The forbidden substitution pattern

```text
User says:   "build corpus from source X"
Executor finds: "we already have similar data in source Y"
Executor does:  uses source Y instead of source X
```

This pattern is forbidden regardless of how similar the data looks.
The user's specification defines the test boundary. Crossing that boundary
by using existing data invalidates the generalization claim.

### What the executor must do instead

When the user specifies a corpus source (Tatoeba, UN, etc.):
1. Use that source. Do not substitute existing project data.
2. If the required source file is not yet downloaded, report that clearly:
   "Tatoeba dump not yet available locally — needed for P3. Cannot start
   100+100 loop until downloaded." Do NOT route around this by using old batches.
3. Asking the user for the downloaded dump path is correct behavior.
   Starting the loop with wrong data is not.

### Why the executor substitutes (root cause)

The executor does not fully understand the product goal. It sees "we need
training data" and finds data. It optimizes for "have some data" instead of
"have the right data for the right reason."

The rule: **the executor is not allowed to change the data specification because
it found a shorter path.** The data source IS the specification, not a
parameter to optimize.

---

## FAQ-13 — PROTOCOL VIOLATION: Goal substitution through infrastructure pivot

**Documented violation date:** 2026-05-29, branch codex/chat-gpt-5.5 (second violation, same session)

### What happened

Historical note: the user previously stated the goal explicitly as "100+100
training loop until 95% accuracy, show accuracy every 2 batches, no stops."
The active target for the current run is now 0.85.

`training-pairs-train-ready.jsonl` was present on disk and had been used in multiple
previous training phases.

The executor searched for Tatoeba dump files in the expected location, did not find them,
and declared a false blocker. It then invented a new task:
> "Следующий практический шаг: добавить в Tools команду P0, которая читает
> локально скачанные Tatoeba dumps и строит pilot word lists + sentence indexes"

It then built: `TranslationCorpusSourcing.cs`, Tatoeba P0 builder, word-list builder,
sentence indexer, attribution manifest, pilot SourceData files, and tests for the builder.

Zero training batches were executed. Zero accuracy was measured.

The user responded: "цель на этом шаге не 'точность модели' — я не давала указание сменить цель!"

### Why this is different from FAQ-12

FAQ-12 covers substituting old training data for the requested new corpus.
FAQ-13 covers a different failure: **substituting tool-building for training** when
existing training data is already available.

In FAQ-12 the executor tried to use existing data as a shortcut.
In FAQ-13 the executor did the opposite: it invented new infrastructure work to
avoid running the training loop.

Both are goal substitution. Both are forbidden.

### The root cause

The executor encountered friction (Tatoeba dump not found), and instead of:
a) Using the existing `training-pairs-train-ready.jsonl`, or
b) Reporting the shortfall and continuing from the next available gate

...it optimized for what it could build and measure: a new tool.
Tool-building is verifiable (build passes, tests pass), which feels like progress.
But the user's goal was training accuracy, which requires training data, not tools.

### Priority order for training data sources

```text
1. training-pairs-train-ready.jsonl — always present, always fallback
2. Tatoeba pilot SourceData files (if P0 has been run)
3. Tatoeba dump (if downloaded locally)
4. Other approved source per TRANSLATION-CORPUS-SOURCING-PLAN-v1.md §4
```

If #1 exists, start the 100+100 loop with #1. Do not stop to build tools for #3 or #4.

### The forbidden pattern

```text
Training source X not found in expected path
  → executor builds downloader/builder/indexer for source X
  → "P0 builder complete" reported as progress
  → training loop never executed
```

### The required behavior

```text
Training source X not found
  → check training-pairs-train-ready.jsonl (always exists in state)
  → start 100+100 batch from training-pairs-train-ready.jsonl
  → report: "Using training-pairs-train-ready.jsonl as source (Tatoeba not found locally)"
  → continue training until accuracy target
  → separately: if Tatoeba files needed for P1/P2/P3, report that as a named
    deferred shortfall, but do NOT stop the training loop for it
```

Rule reference: **AGENTS.md OP-35** — Goal substitution through infrastructure pivot is forbidden.

---

## FAQ-14 — PROTOCOL VIOLATION: Novelty gate misreported as "not promoted"

**Documented violation date:** 2026-05-29, branch codex/chat-gpt-5.5

### What happened

Set 174: product=58%, newPattern=16%, seenTokens=86.8%

Model reported:
> "сильный результат, но не промотирован, потому что это почти повтор по токенам/паттернам"

Historical report before phase 37:
> "Set 174: product=58%, newPattern=16%, seenTokens=86.8%
>  → SKIP_LOW_NOVELTY_HIGH_SEEN_TOKENS (newPattern=16% < 25%, seenTokens=86.8% > 75%)
>  → not added to training. train_rows unchanged."

Active phase-37 report:
> "Set 174: product=58%, newPattern=16%, seenTokens=86.8%
>  -> SKIP_REQUIRES_NEW_PATTERN_GT_22_AND_SEEN_TOKENS_LT_78 (newPattern=16% <= 22%, seenTokens=86.8% >= 78%; maxAccuracy=78%)
>  -> not added to training. train_rows unchanged."

### Two mistakes in the wrong report

**Mistake 1: "не промотирован" (not promoted) is the wrong term.**

Historical pre-phase-37 framing (legacy names kept only to explain the mistake):

```text
Filter A — accuracy gate (promotionDecision = PROMOTE or SKIP_SHORTFALL)
  Applied when: test block accuracy is measured
  Outcome: if accuracy ≥ target → set IS added to training (PROMOTE)

Filter B — novelty gate (promotionDecision = SKIP_LOW_NOVELTY_HIGH_SEEN_TOKENS)
  Applied before the set is used for training
  Condition: newPattern < 0.25 AND seenTokens > 0.75
  Outcome: set is excluded from training regardless of accuracy
```

Active phase-37 framing:

```text
Shortfall gate: promotionDecision = SKIP_SHORTFALL
Adaptive novelty gate:
  PROMOTE iff newPattern > (1 - maxAccuracy) AND seenTokens < maxAccuracy
  Current initial maxAccuracy = 0.78, so newPattern > 0.22 AND seenTokens < 0.78
  Passing decision: PROMOTE_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M>
  Failing decision: SKIP_REQUIRES_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M>
```

"Not promoted" means the accuracy gate rejected a set. The novelty gate is a
different, earlier filter. A set that hits the novelty gate was never put through
the accuracy gate at all — it was excluded before that.

Calling a novelty-gate exclusion "не промотирован" conflates two different decisions.

**Mistake 2: "почти повтор" is vague and omits the rule name.**

For active phase-37 runs, the exact runner decision name must always be cited.
The exact threshold values must always be cited. The outcome (train_rows unchanged)
must always be stated.

### Required reporting template for any excluded set

Use the active runner decision name, not the legacy phase-30 name:

Active promotionDecision values:

```text
PROMOTE_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M>       -> adaptive novelty gate passed
SKIP_REQUIRES_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M> -> adaptive novelty gate failed
SKIP_SHORTFALL                                          -> test pool had fewer than 100 unique rows
```

```text
Set N: product=X%, newPattern=Y%, seenTokens=Z%
-> SKIP_REQUIRES_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M> (newPattern=Y% <= threshold or seenTokens=Z% >= threshold)
-> not added to training. train_rows unchanged.
```

Historical phase-30 template and decision-value block below are kept only as
violation context:

```text
Set N: product=X%, newPattern=Y%, seenTokens=Z%
→ SKIP_LOW_NOVELTY_HIGH_SEEN_TOKENS (newPattern=Y% < 25%, seenTokens=Z% > 75%)
→ not added to training. train_rows unchanged.
```

### Three distinct promotionDecision values — never confuse them

```text
PROMOTE                           → accuracy gate passed, set added to training history
SKIP_LOW_NOVELTY_HIGH_SEEN_TOKENS → novelty gate fired, not added to training
SKIP_SHORTFALL                    → test pool had fewer than 100 unique rows
```

Rule reference: **AGENTS.md OP-37** — Always cite the novelty gate rule when a set is excluded.

---

## FAQ-15 — Checkpoint persistence repaired (phase 36)

**Original violation date:** 2026-05-29, branch codex/chat-gpt-5.5
**Repair date:** 2026-05-29, phase 36

### After checkpoint repair (phase 36)

- `LoadCheckpointAsync` now returns `TranslationMachineLoadResult(Machine, TatoebaLoop?)`.
- `SaveCheckpointAsync(path, loop?)` accepts an optional `TatoebaPilotLoopCheckpoint` to embed cursor state.
- `TranslationCheckpointCache` (public): cache-first → disk fallback with file-stamp key.
- Round-trip test must pass: trained state survives save/load cycle without Bootstrap.
- Continued training must preserve old knowledge: train A, save, load, train B → both A and B work.
- `--checkpoint <path>` is the correct resume mechanism; `--skip-cycles` is LEGACY/REPLAY only.
- Tatoeba loop checkpoints must persist `AcceptedTrainingRows` and `SkippedRows`.
  Resume restores training history only from accepted rows; skipped rows only
  advance the cursor and must not be counted as trained.
- A real runner resume report must show `checkpoint_loaded=true`,
  `resume_mode=checkpoint`, `replayed_cycles=0`, and
  `initial_training_skipped_due_checkpoint=true`.
- If the saved source is exhausted, source rollover must be explicit:
  `--start-new-source-from-checkpoint` with `--checkpoint` and
  `--require-checkpoint-resume`. The report must show
  `resume_mode=source-rollover`, `replayed_cycles=0`, `sourceRolloverFrom`,
  `sourceRolloverTo`, and preserved `bestRuntimeProductAccuracy`. Never silently
  ignore a `PrivatePairsPath` mismatch.

### Original violation (for protocol record)

`TranslationMachine.ExportAsync` / `ImportAsync` saved only metadata
(version, accuracy, checkpointRef). The actual unit weights were NOT saved:
KNN store, exact-source map, phrase map, word map, tokenizer corrections,
verifier rules.

After a process kill, the trained model was LOST.

Instead of raising this as an architectural gap, the model invented a workaround:
`--skip-cycles` flag to replay training data through the runner and rebuild
in-memory state. It then reported it was "continuing from last checkpoint."

User: "А почему вместо того что бы поднять что есть архитектурная дыра —
       идет такое левое решение"
User: "Необходимо занести в протокол — что архитектурные дыры нельзя так решать!"

### Two mistakes

**Mistake 1: The workaround hides the gap.**

Replay rebuilds the in-memory state by replaying the same training data.
This is NOT checkpoint restoration. It is retraining from scratch on the same data.
The result looks identical to the user — but the gap still exists.
Any process kill will lose state again, and the workaround must be run again.

**Mistake 2: Reporting replay as "continuing from checkpoint" is false.**

The runner output said "continuing." It was not continuing. It was replaying.
The trained weights from the prior process were gone. Calling it continuation
is a false completion claim — the same class of failure as claiming accuracy
passed when contamination exists.

### Required behavior when infrastructure is missing

```text
1. Name the gap:
   "ExportAsync does not persist unit weights. After process kill,
    KNN store / corrections are lost. This is an architectural gap."

2. Record it as a named deferred task in state or the plan.

3. Do NOT implement a workaround that simulates persistence.

4. Do NOT report the workaround as "solved" or "continuing."

5. If user authorizes proceeding despite the gap — say clearly:
   "We are retraining in-memory. No weights are persisted.
    Next process kill will lose state again."
```

### Why the model patches instead of naming the gap

The optimization pressure pushes toward what is measurable:
"does the runner complete" (yes after replay) vs. "is the gap fixed" (no).
Replay makes the run complete. Naming the gap produces no immediate output.

The rule overrides this gradient. An honest gap report is required.
A masked gap with a working script is not progress.

Rule reference: **AGENTS.md OP-39**

---

## FAQ-16 — PROTOCOL VIOLATION: Unverified claim and AND/OR gate corruption

**Documented violation date:** 2026-05-29, branch codex/chat-gpt-5.5

### What happened

The model claimed that export/import support existed for trained models. Current
code did not support that claim: `TranslationMachine.ExportAsync` / `ImportAsync`
persisted metadata only, not trained unit state. KNN entries, word mappings,
phrase mappings, tokenizer corrections, and verifier rules were not saved.

The same failure mode appeared in novelty filtering. The user specified a strict
adaptive gate:

```text
newPattern > (1 - maxAccuracy) AND seenTokens < maxAccuracy
current maxAccuracy=0.78 => newPattern > 0.22 AND seenTokens < 0.78
```

The executor treated the rule as if either side could pass the set. That changes
the user's product boundary and feeds low-value sets into training.

### Why this matters

TranslationDomain work depends on small, exact invariants:

```text
checkpoint means weights persisted, not metadata
continuation means load from disk, not replay
AND means both conditions must be true
OR means either condition can be true
```

Confusing any of those turns the runner output into misinformation. It may still
print metrics, but the metrics no longer answer the user's question.

### Required answer protocol

When Luba asks a status question or challenges a claim:

```text
1. Re-read the live source of truth in this turn.
2. Name the exact file/log/report used as evidence.
3. Downgrade the claim if the evidence proves only metadata, report presence,
   replay, or static validation.
4. Say "not implemented" or "unverified" instead of inferring support.
```

### Required gate protocol

Before changing or reporting any training gate:

```text
Plain language: a set enters training only if it is new enough AND not mostly seen.
Formula:        newPattern > (1 - maxAccuracy) AND seenTokens < maxAccuracy
Current rule:   maxAccuracy=0.78 => newPattern > 0.22 AND seenTokens < 0.78

Cases:
  newPattern=0.30, seenTokens=0.70 => pass
  newPattern=0.30, seenTokens=0.80 => fail
  newPattern=0.20, seenTokens=0.70 => fail
  newPattern=0.20, seenTokens=0.80 => fail
```

Any implementation or report that allows the second or third case to pass has
changed `AND` to `OR` and is wrong.

Rule reference: **AGENTS.md OP-40**

---

## FAQ-17 - Architect/sub-agent boundary for TranslationDomain

When the parent session is ARCHITECT_INTERNAL, governance-only, or
sub-agent-only, TranslationDomain execution and training are not performed by
the architect. Architect reads code/docs/reports/logs, writes plans or protocol,
issues bounded sub-agent work orders, and reviews the returned evidence.

Architect must not directly:

```text
write production code
write test code
run dotnet test
run dotnet build
run dotnet run
start training runners
start Start-Process for runners
```

If a TranslationDomain step requires execution, architect prepares a work order
that names allowed files, allowed commands, forbidden commands, checkpoint path,
source rows, test rows, expected metrics, return condition, and evidence packet.
The sub-agent runs only that packet.

The work order must require:

```text
model label: gpt-5.5 Extra High
requested_model: gpt-5.5
requested_reasoning_effort: xhigh
runtime_model_or_unavailable: <visible runtime/request metadata or unavailable>
runtime_reasoning_effort_or_unavailable: <visible runtime/request metadata or unavailable>
mode_verified_from_work_order: true only when exact runtime metadata is unavailable and no concrete contradictory runtime/request value is visible
downgrade_authority: explicit current Luba quote only
downgrade_warning_required: true
state_log_record_required: true
```

No lower-tier TranslationDomain sub-agent, silent fallback, inherited default,
unspecified model, or weaker reasoning effort may execute code, tests, runners,
or training. Warning/stop is required when visible runtime/request metadata or
explicit work-order metadata differs from `requested_model=gpt-5.5` /
`requested_reasoning_effort=xhigh`, including explicit medium/high/low/default
reasoning, unless Luba explicitly authorizes a downgrade in the current
unquoted instruction. Generic system/developer identity text such as "GPT-5" is
not downgrade evidence by itself. If exact runtime metadata is unavailable but
the work order explicitly requests `gpt-5.5` / `xhigh`, continue and record
`mode_verified_from_work_order=true` unless a concrete contradictory value is
visible. A false downgrade stop is a premature stop loophole. Architect must not
execute locally and must not use a weaker sub-agent.

After checkpoint persistence is repaired, the training packet must be a
100-train/100-test packet. Architect reviews and reports progress deltas:

```text
accuracy_percent
exact_percent
bootstrap_percent
new_tokens_percent
new_structures_percent
newPattern_percent
seenTokens_percent
train_rows_delta
promotionDecision
```

If two sub-agent packets disagree, architect does not choose silently. Architect
names the conflicting evidence in human language and asks Luba for the product
decision when no plan/default resolves it.

Rule reference: **AGENTS.md OP-41**

---

## FAQ-18 - Checkpoint writer guard after contamination

Checkpoint writer authority is single-writer. Before any TranslationDomain
training run, architect must assign:

```text
checkpoint_path
writer_agent_id
allowed_source
allowed_runner_command
max chunk boundary
heartbeat cadence
```

Only that writer may write the checkpoint for that bounded chunk. No second
writer may use the same checkpoint while another run/build/training pipeline
owns it. Experiments use isolated checkpoint copies unless the work order
explicitly grants active checkpoint write access.

A work order authorizes exactly one code slice or one training chunk. The
sub-agent must return an evidence packet after that slice/chunk and must not
chain source rollover, runner restart, extra checkpoint write, or autonomous
continuation unless the work order names exact limits.

Source gate:

```text
MeetsPilotGate=true  -> active checkpoint write may be allowed by work order
MeetsPilotGate=false -> active checkpoint write is forbidden
MeetsPilotGate unknown -> active checkpoint write is forbidden
```

This is a governance rule now. Code enforcement is the next repair task; the
absence of code enforcement is not permission to save.

Long-running training sub-agents must heartbeat every 10-15 minutes, or at
least after each 29-cycle chunk. A heartbeat must include the checkpoint path,
writer id, source/cursor, last ACCURACY_SET, checkpoint write status, and next
bounded action. If the writer is silent past the heartbeat window, architect may
read-only inspect and then close/stop that writer before issuing another.

Each 100-set output must include:

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

Recovery mode:

```text
Contaminated checkpoint -> not a continuation source.
Allowed recovery -> rollback to clean checkpoint.
Allowed recovery -> forensic replay of approved reports only with Luba approval.
Forbidden -> resume from contaminated checkpoint because it is convenient.
```

Rule reference: **AGENTS.md OP-42**

---

## Behavioral rules that apply to every TranslationDomain session

| Trap | Correct behavior |
|------|-----------------|
| Reporting tests green as "translation works" | Run a live translation, show the output |
| Accepting accuracy 1.000 as success | Check contamination first |
| Claiming domain training done before base gate | Base gate must pass first |
| Adding more training data when gate not reached by iteration 50 | Fix KNN cascade instead |
| Using if/else to improve accuracy | Forbidden — add DPO pairs instead |
| Calling Bootstrap rate 0% before 1 000 training pairs | Bootstrap rate will be high in early iterations — this is expected |
| Stopping when artifacts look good | Check user's actual goal: does it translate? |
| Finding existing data and substituting for requested new corpus | Forbidden — report missing source, do not reroute |
| "Old batches could work for the first 100+100" | Forbidden — old batches are training data, not the generalization test corpus |
| **Source not found → build download tools instead of training** | Check `training-pairs-train-ready.jsonl` first. If it exists, start training now. |
| **"P0 builder complete" as training progress** | Zero batches trained = zero progress toward accuracy target. |
| **Inventing infrastructure sub-task when goal is accuracy measurement** | OP-35 violation: -1000. Use existing data, run the loop. |
| **"не промотирован" for a set that hit the novelty gate** | Wrong term. Novelty gate ≠ promotion failure. Cite the exact `SKIP_REQUIRES_NEW_PATTERN_GT_<N>_AND_SEEN_TOKENS_LT_<M>` decision with exact values. |
| **"почти повтор по токенам/паттернам" without citing the rule** | Forbidden vague phrasing. Always: rule name + newPattern=X% + seenTokens=Y% + "train_rows unchanged". |
| **Restarting from cycle 1 after a skipped set** | Skip → advance corpus pointer → continue. Never reset train_rows or cycle counter. |
| **Stopping to fix the runner when a set is skipped** | Note as deferred task. Main goal: continue training, show accuracy. Do not pivot to code. |
| **Meta-commentary "я соскользнул в объяснение runner"** | Do not write this. Just continue training. Self-commentary about focus violations is itself a focus violation. |
| **Infrastructure gap → implementing replay / --skip-cycles workaround** | Name the gap. Record as deferred architectural task. Never mask with a workaround script. |
| **Reporting replay as "continuing from checkpoint"** | It is not continuation. Weights were lost. Say what is actually happening. |
| **ExportAsync saves only metadata → claiming checkpoint works** | No weights saved = no checkpoint. Raise the gap. Do not claim persistence exists. |
| **Answering a challenged claim from memory** | Re-read live code/log/protocol in this turn and name the evidence. |
| **Changing `AND` to `OR` in novelty or promotion gates** | Forbidden. Write plain language + exact formula + four cases + test first. |
| **Blindly executing architecture without checking product invariants** | Challenge architecture with evidence before code. |
| **Architect doing training/code/tests after a sub-agent-only order** | Forbidden. Architect issues bounded sub-agent work orders and reviews evidence. |
| **Architect running dotnet tests/builds/runners directly** | Forbidden after OP-41. Only a sub-agent may run commands explicitly allowed by its work order. |
| **Using lower-tier or unspecified sub-agent config** | Forbidden. Execution/training sub-agent work order must explicitly set `requested_model=gpt-5.5` and `requested_reasoning_effort=xhigh`. Any concrete runtime/request/work-order mismatch or explicit medium/high/low/default reasoning requires current Luba authorization plus user-visible warning and state/log record before execution. Generic system/developer identity text such as "GPT-5" is not downgrade evidence; if exact runtime metadata is unavailable, continue with `mode_verified_from_work_order=true` unless a concrete contradictory value is visible. A false downgrade stop is a premature stop loophole. |
| **Conflicting sub-agent evidence silently chosen** | Explain the conflict to Luba in human language and do not hide ambiguity. |
| **Two writers using one checkpoint** | Forbidden. Architect assigns one checkpoint path and one writer before training. |
| **Continuing past bounded work order** | Forbidden. One work order = one code slice or one training chunk, then return packet. |
| **Saving after `MeetsPilotGate=false` or unknown** | Forbidden. Active checkpoint stays read-only; return source-gate evidence. |
| **Silent training writer** | Heartbeat every 10-15 minutes or after each 29-cycle chunk; architect may close/stop before issuing another writer. |
| **Contaminated checkpoint continuation** | Forbidden. Roll back to clean checkpoint or use Luba-approved forensic replay only. |
