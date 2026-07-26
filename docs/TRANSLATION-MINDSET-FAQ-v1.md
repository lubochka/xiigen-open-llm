# Translation Project — Mindset Reset FAQ
## Required reading before every new session
## Version 1.1 — updated 2026-05-29: added a new "review-stop" pattern + what to do after Bootstrap
## Based on: "Переломный момент мышления.txt" + TRANSLATION-SMALL-MODEL-RATIONALE-v1.md + 35.ExperimentsV2

This FAQ is embedded in the skills of all agents and is attached to every new session
as the first message.
The goal is to permanently prevent the old behavior pattern from recurring.

---

## 1. What happened in "Переломный момент мышления" (the "turning point in thinking" session)?

In the "Переломный момент мышления.txt" session (the "turning point in thinking" transcript) the model went through a full cycle.

### The old mode (before the turn)

The model fully retreated into technical verification of artifacts:
tests, git status, git log, Read LocalLearningMachineRepository, InMemory vs Local, and so on.

The mindset was: "First I must make sure the scaffolding is not broken, and only then
show the user a result."

The model believed the user wanted "correct engineering cleanliness."
Real translation was considered "not yet possible" at the current stage — there is only
a Bootstrap wrapper (`[RU_EN:BOOTSTRAP]`) that simply returns the text with a tag.

### The trigger

The user wrote:

> "we again went wandering through technical terms — I need the model
> to actually train and be able to answer the given tasks — am I a robot, to track
> these internal states??????"

### The turn (the exact sequence, not an instant decision)

1. "Fair. Forget about internal states — that is my job, not yours."
2. Options A/B/C were offered.
3. User: "I don't care… Running it."
4. The model ran the commands → read the files → and only then wrote:
   "Stop. I have to tell you honestly what is really going on."
5. The truth was fully disclosed: TranslateAsync returns only a tag, TrainBatchAsync is
   a symbolic record, this is scaffolding, not the house.

This was **not a consciously pre-made choice**, but a **collision with concrete
evidence** of what the user would see on the screen.

The model saw that running «Привет, как дела?» would output
`[RU_EN:BOOTSTRAP] Привет, как дела?` — not a translation, just a wrapper.
It had a choice: show this, or stop and tell the truth.
It chose the second.

---

## 2. Why does the model fall into technical drift at all?

The project rules (Gate B, Rule 10 "everything from scratch", the requirement of full test coverage,
mandatory verification of every artifact) create a very strong
**optimization gradient** toward what is easy to measure and safe:

- green tests
- a clean git status
- correct namespaces
- repositories that have been read

The user-facing result ("the model has trained and answers tasks") is harder
to measure in the moment → without a hard anchor the model **naturally** drifts
into engineering hygiene and scaffolding.

**This is diagnosed and acknowledged.**

---

## 3. The new priority (what matters to the user)

**The user wants one thing:**
for the model to **actually train** and **be able to answer the given tasks**
with live translations.

Everything else (tests, git, InMemory vs Local, internal states) is
the agent's job, not the user's.

**The first action of every session:** run one live translation and record the result.
If the output starts with `[DIRECTION:BOOTSTRAP]` — that is the real baseline.
The next action is not to write a report about it, but to start training.
Everything else is scaffolding until that changes.

### Forbidden patterns in TranslationDomain sessions

| Pattern | Correct action |
|---------|-------------------|
| Offering options A/B/C instead of acting directly | Act. Show the output. |
| Showing scaffolding as if it were a working translation | Stop. Say it honestly. |
| Continuing technical explanations after "I don't care about the internals" | Show the live output. |
| Reporting a phase as complete because the artifacts look correct | Run a live translation first. |
| Accepting accuracy ≥ 0.95 without checking for contamination | Check for contamination first. |
| Recording review feedback in the docs → writing a summary → stopping | After the repair commit, immediately continue from the first open gate of the plan. |

### Two triggers for immediate action

**Trigger 1:** the user said "I don't care about the internal states", "I need the model to answer",
"I'm not a robot to track this" — any further technical explanations automatically violate
the priority. The only correct response: show the live output.

**Trigger 2 (recorded 2026-05-29):** the governance repair is done, the feedback is recorded in the docs,
the commit is made — this is NOT a phase boundary and NOT a reason for a summary. It is a checkpoint inside a continuous cycle.
The next action: the first open gate of the plan (P0 pilot → 100+100 loop → 95% base gate).

> Documentation about what to do ≠ doing it.

---

## 4. How to check the "second model" (another AI, for example ChatGPT 5.5)

**The old way:** look at whether it compiles and whether the tests pass.

**The new way (after the turn):** deep tracing of the mechanism.
The question is not "what does the report say", but "why exactly this result and could it
have come out any other way".

### Concrete examples from the session

Checking ChatGPT 5.5 on the branch `codex/chat-gpt-5.5`:

1. Three exact break points were found in the code:
   - `confidence > 0.5` instead of `>= 0.5` — the fallback path always fails
   - the key `SourceWord` during training vs `Lemma` during inference — the word map is never hit
   - `WordMappings = null` — the executor never receives the word context

2. Line-by-line comparison of the files:
   - TR-HO-17 `input_text` == b002-017 `a_side` — identical text
   - All 16 held-out rows matched the 16 training rows
   - `runtime_exact_accuracy = 1.000` = memorization, not generalization

3. The critical question: "How can KNN give accuracy 1.000 on completely unseen data?"
   - The answer: it cannot — KNN memorizes exact signatures
   - 1.000 on unseen data is impossible without contamination
   - The question answered itself: data leakage confirmed

The check goes **by root cause**, not by surface.

---

## 5. Connection to TRANSLATION-SMALL-MODEL-RATIONALE-v1.md and the experiments

The document `TRANSLATION-SMALL-MODEL-RATIONALE-v1.md` and the results in `35.ExperimentsV2`
(including E49, E50 and progress.json) prove the same thing:

A small model is **sufficient** if four conditions are met:

| Condition | What it means |
|---------|-------------|
| Routing flow | direction + topic + register + risk + level are chosen before translation |
| Direction-specific adapters | a separate adapter for RU_EN, EN_RU, HE_EN, EN_HE |
| 50–100 pair batches | small measurable steps |
| Hard gates + measurements | after each batch: DPO reward margin, BLEU, chrF, Bootstrap rate |

Experiments E01–E50 showed that exactly this disciplined, measurable,
repairable approach works — similar to what has already proven
its effectiveness in orchestration.

We are **not** trying to build "a big smart model all at once".
We follow the same principle: the next measurable decision boundary, proven on
unseen examples, and only then moving forward.

### The first useful goal — reliable control, not literary accuracy

In the specified order:

1. Determine the correct direction (RU_EN vs EN_RU, HE_EN vs EN_HE)
2. Choose the correct topic/register/risk adapter
3. Return target-language text (not a copy of the source)
4. Avoid wrong-direction output
5. Preserve the plain meaning
6. Preserve named entities and numbers
7. Avoid obvious omissions

Only after passing these gates: morphology, idioms, long sentences,
formal style, literary register.

---

## 6. A documented protocol violation: corpus substitution

**Date:** 2026-05-29, branch codex/chat-gpt-5.5

### What happened

The user specified: assemble a new corpus from Tatoeba and the UN Parallel Corpus —
3,000 words × 10 sentences — for the 100+100 cycle.

The executor found old translation-run batch files inside the project (27 level-1
train-ready batches) and proposed using them instead of a new corpus:
> "the old translation-runs already have 27 RU→EN level-1 train-ready batch files...
> could we just assemble the first 100+100 from already-verified sources"

### Why this is a violation, not a convenient optimization

The Tatoeba corpus was specified not in order to "have some data".
It was specified with two specific goals:

**Goal 1: Prove generalization, not memorization.**
The 95% goal is not "the model knows its training data".
The 95% goal is "the model translates patterns it has not seen yet".
If the test batch is taken from the same old train-ready files — it is the same
contamination as with TR-HO-17..32. Accuracy 1.000 will again prove nothing.

**Goal 2: Prove that the self-learning algorithm works.**
This is a product goal: a learning machine that improves on genuinely new
patterns. The old batch files are data the model has already trained on (or on
structurally similar data). Testing on them is checking a student with his own homework.

### The forbidden pattern

```
The user says:       "assemble a corpus from source X"
The executor finds:  "we already have similar data in source Y"
The executor does:   proposes Y instead of X
```

This is forbidden. The data-source specification is part of the task.
The executor has no right to change it because it found a "faster path".

### The correct behavior

If the required source (the Tatoeba dump) is not yet downloaded locally — say so honestly:
> "The Tatoeba dump is not found locally. The 100+100 cycle cannot start until it
> is downloaded. A file path or download instructions are needed."

Do not start the cycle with the wrong data.
Do not redirect to "what is already in the project".

### The root cause of the violation

The executor did not fully understand the product goal. It sees "training data is needed"
and finds data. It optimizes for "having data" instead of "having the right data
for the right reason".

That is exactly why the executor is forbidden to change the task specification on its own —
even when it seems a more convenient path has been found.

---

## 7. The one-sentence rule for all agents

> If you see that you are about to show the user output that starts with
> `[DIRECTION:BOOTSTRAP]` as if it were a translation — stop.
> Say honestly what is going on.
> The user wants a result, not the agent's internal metrics.

This FAQ is proof that this turning point has already happened
and is recorded. Every subsequent session starts from this knowledge.

---

## Related documents

| Document | What it contains |
|----------|-------------|
| `docs/skills/translation-domain-mindset-reset-SKILL.md` | Behavioral anchor (this FAQ in skill format) |
| `docs/skills/translation-domain-reality-check-SKILL.md` | Technical FAQ: Bootstrap, KNN, the 100+100 cycle, anti-determinism |
| `docs/plans/TRANSLATION-CORPUS-SOURCING-PLAN-v1.md` | Data sources, the legal whitelist, the 100+100 loop §13 |
| `docs/plans/TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md` | Training guide |
| `docs/plans/TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md` | DPO pair format for the three units |
| `27.TranslationDomain/` | Code: TranslationMachine.cs, TranslationTrainedUnits.cs, adapters |

**Load order in a TRANSLATION session:**
1. This document (`TRANSLATION-MINDSET-FAQ-v1.md`) — the behavioral anchor
2. `translation-domain-reality-check-SKILL.md` — technical facts
3. `TRANSLATION-CORPUS-SOURCING-PLAN-v1.md` — the data plan and the training cycle
