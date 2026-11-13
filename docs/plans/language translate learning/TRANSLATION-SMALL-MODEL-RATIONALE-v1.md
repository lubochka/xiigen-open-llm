# Why A Small Model Is Sufficient For This Translation Curriculum
# Version: 1.0 - 2026-05-11
# Domain: Russian/Hebrew <-> English Translation
# Depends on: TranslationRoutingFlow + 50-pair batch streams

---

## Position

A small model can be sufficient for this language-translation curriculum because
the system is not asking one model to learn all translation behavior in one
undifferentiated space.

The architecture splits the task into:

1. a learnable routing flow
2. direction-specific adapters
3. topic-specific streams
4. level-specific difficulty gates
5. small measured 50-pair batches
6. held-out translation evaluation after every batch

This is the same principle used in the orchestration work: the model does not
need to become generally omniscient. It needs to learn the next executable
decision boundary, prove it on held-out examples, and only then move forward.

---

## Why This Works

### 1. Routing Reduces The Search Space

The model does not decide translation, topic, register, risk, and style inside
one generation step.

`TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>` first selects:

- direction
- topic domain
- register
- risk class
- translation level
- adapter id

The translator adapter then solves a narrower problem.

This turns:

```text
translate any Russian/Hebrew/English text in any topic and style
```

into:

```text
translate RU_EN x general x Level 1
translate EN_HE x named_entity x Level 3
translate RU_EN x legal_diplomatic x Level 2
```

A small model is much more likely to learn these narrower decision surfaces.

### 2. Adapters Make Learning Reversible

The four language directions are not forced into one shared update:

- RU_EN
- EN_RU
- HE_EN
- EN_HE

Russian and Hebrew have different scripts, morphology, and generation risks.
Separate adapters allow one direction to improve without damaging another.

If `EN_HE` regresses on Hebrew script quality, the system repairs that stream
instead of rolling back all translation learning.

### 3. 50-Pair Batches Create Measurable Increments

Fifty pairs is not the whole level. It is the batch size.

Each stream accumulates many accepted 50-pair batches. After every batch, the
runner measures:

- DPO reward margin
- DPO accuracy
- BLEU
- chrF
- target-script accuracy
- copied-source rate
- wrong-direction rate
- named-entity preservation
- completeness score
- CPU, RAM, disk, elapsed time

This prevents broad unmeasured training. If a batch hurts translation quality,
the system knows which batch, source slice, family, or rejected-side pattern
caused the regression.

### 4. DPO Teaches Preference Boundaries, Not Full Language From Scratch

The model is not learning Russian, Hebrew, and English from zero. The DPO task is
more focused:

```text
prefer the better translation over a plausible wrong translation
```

Good rejected sides teach boundaries such as:

- this is the wrong direction
- this dropped a clause
- this changed formal register
- this corrupted a named entity
- this used the wrong Hebrew script behavior
- this routed legal text to the general adapter

These are compact preference boundaries. A small model can learn many such
boundaries when the pairs are clean and narrowly scoped.

### 5. The Orchestration Experiments Support The Method

The orchestration work showed that small incremental batches are useful only
when they are:

- scoped to one capability
- measured immediately
- protected by hard gates
- rejected when weighted accuracy or reward margin regresses
- repaired when rejected sides are weak
- separated by domain instead of merged blindly

The accepted orchestration baseline style matters here: the translation
curriculum reuses the same discipline rather than assuming translation is
special enough to skip gates.

The lesson from the weak runs is also important. When batches were broad,
source-mixed, or had poor rejected sides, weighted accuracy dropped. Translation
training must therefore stay narrow, measurable, and repairable.

---

## What The Small Model Should Learn First

The first useful translation behavior is not literary fluency.

The first useful behavior is reliable control:

1. identify direction
2. select topic/register/risk adapter
3. output target-language text only
4. avoid source copying
5. avoid wrong-direction output
6. preserve simple meaning
7. preserve entities
8. avoid obvious omissions

Only after these pass should the system train morphology, idioms, long clauses,
formal style, and robustness.

---

## What This Does Not Claim

This rationale does not claim that a small model is enough for every translation
task.

It does not claim first-pass success for:

- legal production translation without review
- medical or financial high-stakes text
- literary style transfer
- unrestricted conversational translation
- broad web-mined corpora

Those require either later levels, human review, stronger sources, larger
adapters, or a larger model.

The claim is narrower:

```text
A small model is sufficient to learn controlled translation behavior when the
problem is routed, adapterized, trained in 50-pair batches, and measured after
every batch.
```

---

## Required Evidence Before Trusting The Claim

Each stream must prove itself with:

- accepted routing-flow batches
- accepted translator batches
- positive DPO reward margin
- non-regressive BLEU and chrF
- high target-script accuracy
- low copied-source and wrong-direction rates
- readable `LOOK-LIKE-RUN-REPORT.md`
- cumulative `STREAM-STATE.json`

If these reports do not improve, the claim fails for that stream. The next
action is repair, not faith.
