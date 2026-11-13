# Translation Training Runbook
# Current operational executor: EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md
# Current source registry: TRANSLATION-SOURCE-REGISTRY-v3.md

## 2026-05-11 Current-Run Correction

This runbook is retained as historical handoff context. For execution, use
`EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md`.

Current semantics:

- read `TRANSLATION-EXECUTOR-HARD-RESET-v1.md` before executing any batch
- stream = direction x topic_domain x translation_level
- batch = 50 new train pairs + 50 held-out rows
- 50 pairs is the batch size, not the total flow/level size
- do not reduce live executor batches to 8/16/24/32 pairs unless explicitly
  running a diagnostic micro-test outside the train-ready stream
- write `LOOK-LIKE-RUN-REPORT.md` for every batch
- use `TRANSLATION-SOURCE-REGISTRY-v3.md`; older registry references are stale
- if the runner/evaluator/flow is missing, write
  `PAIRSET_READY_TRAINING_RUNNER_MISSING` artifacts and `RUNNER-CONTRACT.md`;
  do not end with "awaiting direction"
# Version: 1.0 — 2026-05-11
# Domain: Russian/Hebrew ↔ English Translation

---

## Purpose

Operational handoff for anyone continuing the translation training run.
Covers: what to read first, what state to restore, how to build a batch,
how to measure, and what to do when a phase fails.

---

## Required Reading Order

Read in this order before doing anything:

1. `TRANSLATION-SOURCE-REGISTRY-v1.md` — know what sources you may use
2. `TRANSLATION-DPO-REGISTRY-GUIDE-v1.md` — know which pair families exist
3. `EXAMINATION-METHOD-TRANSLATION-v1.md` — know how to examine a sentence pair
4. `DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md` — know how to write a pair
5. `TRANSLATION-TRAINING-PLAN-v1.md` — know which phase you are in
6. This runbook — operational procedures

Do not skip the registry guide. Decisions about which sources are allowed must
be made before pair files are created, not after.

---

## State Restoration

Before starting a session, locate the current training state:

```
C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\
  run-state.json       ← current phase, last checkpoint, last BLEU scores
  pairs\               ← pair files by phase and direction
  checkpoints\         ← LoRA adapter checkpoints by phase
  eval\                ← held-out evaluation results
```

`run-state.json` must exist before training starts. If it does not exist, this
is the first session — run Phase 0 pre-conditions first.

---

## How to Build a Batch File

### Step 1: Select source sentences

- Open the registered corpus (e.g. OPUS Tatoeba RU–EN sentences).
- Filter: 4–256 source tokens, no mojibake, correct source-language script.
- Draw sentences randomly; do not cherry-pick easy examples.
- Record the sentence id and corpus version in the pair metadata.

### Step 2: Assign pair family

Apply the decision tree from `EXAMINATION-METHOD-TRANSLATION-v1.md` Layer 0-B.

### Step 3: Construct chosen

Use the parallel corpus target sentence verbatim. If quality is uncertain,
flag as `quality_status: review_required` — do not promote to train-ready.

### Step 4: Construct rejected

Select the error type for the family. Construct a plausibly wrong translation.
Append `[WRONG: error_code — specific description]`.

### Step 5: Apply gates

- Token overlap test (script the function from DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md)
- Identity check
- Mojibake check
- [WRONG:] presence
- Language verification
- Licence check

### Step 6: Write pair JSON

Write to `pairs/phase-{N}-{direction}.jsonl`.
One pair per line. UTF-8, no BOM.

### Step 7: Run hard-gate script

```bash
python validate_translation_pairs.py --input pairs/phase-1-ru-en.jsonl
```

Script outputs:
- `train_ready` → proceed
- `repair_required` → fix before proceeding
- `discard` → remove from batch

Do not start training until `repair_required` count is zero.

---

## How to Run a Phase

1. Verify run-state.json shows the correct phase.
2. Verify all pairs in the batch file have `quality_status: train_ready`.
3. Verify no BLOCK or REVIEW_REQUIRED source ids appear in the batch.
4. Start LoRA training with the translation configuration (Rank=4, Alpha=8).
5. After training, run the evaluation script:

```bash
python evaluate_translation.py \
  --checkpoint checkpoints/phase-1.pt \
  --eval-set eval/flores200-ru-en.jsonl \
  --baseline checkpoints/phase-0-baseline.pt
```

6. Record BLEU and chrF in run-state.json.
7. Evaluate Gate B conditions from the training plan.
8. If Gate B passes: update run-state.json, proceed to next phase.
9. If Gate B fails: diagnose, fix, re-run this phase. Do not advance.

---

## Pair Validation Script Template

```python
#!/usr/bin/env python3
"""Validate translation DPO pairs against hard gates."""

import json, sys, hashlib, re
from pathlib import Path

MOJIBAKE_MARKERS = ("U+00C3", "U+00C2", "U+00E2 U+20AC", "U+00C3 U+00A2")
BLOCK_SOURCES = {"culturax", "rnc", "cc100-he-unreviewed"}
REVIEW_REQUIRED_SOURCES = {"cc100-ru", "cc100-he", "mnlp-he-unreviewed"}
QUARANTINE_SOURCES = {"wikimatrix", "hewiki", "flores200", "ntrex128"}

# Rejected sides that are negations of the instruction rather than plausible
# wrong translations produce near-zero reward margin (equivalent of the
# forbidden "Do not generate a capability" template in the orchestration domain).
NEGATION_TEMPLATES = [
    "do not translate",
    "cannot be translated",
    "translation not available",
    "[untranslatable]",
    "n/a",
    "not applicable",
    "no translation",
    "translation impossible",
]

# Note on overlap threshold: the conceptual gate is 0.60 against BPE tokens.
# This script uses whitespace tokenisation as an approximation. Whitespace
# splitting misses subword overlap (e.g. "translate"/"transliterate" share the
# "trans" subword in BPE). The threshold is lowered to 0.50 to compensate —
# some pairs that would pass the BPE gate will be caught here, which is the
# safer direction. Once the BPE tokenizer is callable from Python (or a
# SentencePiece model is available), replace .split() with proper tokenisation
# and restore the threshold to 0.60.
OVERLAP_THRESHOLD = 0.50

def has_mojibake(text):
    return any(m in (text or "") for m in MOJIBAKE_MARKERS)

def has_negation_template(text):
    lower = (text or "").lower()
    return any(pat in lower for pat in NEGATION_TEMPLATES)

def overlap_ratio(a_tokens, c_tokens, r_tokens):
    a_set = set(a_tokens)
    shared = (set(c_tokens) & set(r_tokens)) - a_set
    total_unique = len((set(c_tokens) | set(r_tokens)) - a_set)
    if total_unique == 0:
        return 0.0
    return len(shared) / total_unique

def validate_pair(pair):
    errors = []
    a = pair.get("a_side", "")
    c = pair.get("chosen", "")
    r = pair.get("rejected", "")

    if c.strip() == r.strip():
        errors.append("chosen_equals_rejected")
    for field, val in [("a_side", a), ("chosen", c), ("rejected", r)]:
        if has_mojibake(val):
            errors.append(f"mojibake_in_{field}")
    if "[WRONG:" not in r:
        errors.append("missing_wrong_annotation")
    if has_negation_template(r):
        errors.append("negation_template_as_rejected")
    if not pair.get("direction"):
        errors.append("missing_direction")
    if pair.get("source_id") in BLOCK_SOURCES:
        errors.append("blocked_source")
    if pair.get("source_id") in REVIEW_REQUIRED_SOURCES:
        errors.append("review_required_source")
    if pair.get("source_id") in QUARANTINE_SOURCES and not pair.get("batch_isolation"):
        errors.append("quarantine_source_without_batch_isolation")

    # Whitespace-approximated overlap — see OVERLAP_THRESHOLD comment above.
    a_toks = a.split()
    c_toks = c.split()
    r_toks = r.split()
    if len(c_toks) + len(r_toks) >= 12:
        ratio = overlap_ratio(a_toks, c_toks, r_toks)
        if ratio > OVERLAP_THRESHOLD:
            errors.append(f"overlap_ratio_too_high:{ratio:.3f}")

    return errors

if __name__ == "__main__":
    path = Path(sys.argv[1])
    train_ready, repair_required, discard = [], [], []
    for line in path.read_text(encoding="utf-8").splitlines():
        if not line.strip():
            continue
        pair = json.loads(line)
        errors = validate_pair(pair)
        pair["validation_gate_errors"] = errors
        if not errors:
            pair["quality_status"] = "train_ready"
            train_ready.append(pair)
        elif "blocked_source" in errors or "overlap_ratio_too_high" in " ".join(errors):
            discard.append(pair)
        else:
            pair["quality_status"] = "repair_required"
            repair_required.append(pair)

    stem = path.stem
    (path.parent / f"{stem}-train-ready.jsonl").write_text(
        "\n".join(json.dumps(p, ensure_ascii=False) for p in train_ready) + "\n",
        encoding="utf-8"
    )
    (path.parent / f"{stem}-repair-required.jsonl").write_text(
        "\n".join(json.dumps(p, ensure_ascii=False) for p in repair_required) + "\n",
        encoding="utf-8"
    )
    print(f"train_ready: {len(train_ready)}, repair_required: {len(repair_required)}, discard: {len(discard)}")
```

---

## Measurement Script Template

```python
#!/usr/bin/env python3
"""Evaluate translation checkpoints with BLEU and chrF."""

# Requires: sacrebleu (pip install sacrebleu)
import argparse, json
import sacrebleu

def evaluate(checkpoint_path, eval_path, baseline_path=None):
    # Load eval set (a_side = source, reference = human reference translation)
    references = []
    hypotheses = []

    eval_data = [json.loads(l) for l in open(eval_path, encoding="utf-8") if l.strip()]
    for item in eval_data:
        references.append(item["reference"])
        # hypotheses.append(translate(item["a_side"], checkpoint_path))  # wire to your inference

    bleu = sacrebleu.corpus_bleu(hypotheses, [references])
    chrf = sacrebleu.corpus_chrf(hypotheses, [references])

    print(f"BLEU: {bleu.score:.2f}")
    print(f"chrF: {chrf.score:.2f}")

    if baseline_path:
        # compare with baseline similarly
        pass
```

Wire `translate()` to your existing `09.Inference/TextGenerator.cs` inference
path with the translation LoRA checkpoint loaded.

---

## Failure Recovery

### Phase fails Gate B — DPO reward margin negative

1. Identify which pairs had negative margin (DPO trainer records per-pair margin).
2. Look up error category in DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md Diagnosis Table.
3. Most common cause: rejected side too similar to chosen → increase structural divergence.
4. Replace failing pairs with higher-divergence alternatives.
5. Re-run the phase from scratch (same checkpoint as start of phase).
6. Record the original failure and fix in run-state.json.

### Phase fails Gate B — BLEU regression

1. Identify which direction regressed.
2. Check whether the batch had too many pairs for that direction's opposite direction
   (e.g. too many EN→HE pairs can interfere with HE→EN).
3. Reduce batch size for the regressing direction to 8 pairs and re-test.
4. If regression persists, pause training on that direction and add more T01 core
   pairs to reinforce basic translation before adding morphological complexity.

### Encoding error in Hebrew batch

1. Stop training.
2. Run `validate_translation_pairs.py` — it will flag `mojibake_in_a_side`,
   `mojibake_in_chosen`, or `mojibake_in_rejected`.
3. Do not attempt to repair mojibake automatically. Discard the affected pairs.
4. Re-extract from the source corpus with explicit UTF-8 encoding enforcement.
5. Hebrew: use `codecs.open(path, encoding='utf-8-sig')` to handle BOM if present.

### Source licence issue discovered after pairs are written

1. Move all affected pairs to `pairs/review-required/`.
2. Update the registry entry in TRANSLATION-SOURCE-REGISTRY-v1.md.
3. If the source is now BLOCK: delete all pairs from that source from train-ready files.
4. If the source moves to QUARANTINE: set `batch_isolation: true` on all affected pairs
   and move them to a dedicated quarantine batch file.
5. Re-validate the batch.

---

## Run-State JSON Format

```json
{
  "current_phase": 1,
  "last_checkpoint": "checkpoints/phase-1-ru-en.pt",
  "baseline_checkpoint": "checkpoints/phase-0-baseline.pt",
  "total_pairs_trained": 32,
  "bleu": {
    "ru_en": {"after": null, "delta": null},
    "en_ru": {"after": null, "delta": null},
    "he_en": {"after": null, "delta": null},
    "en_he": {"after": null, "delta": null}
  },
  "chrf": {
    "ru_en": {"after": null, "delta": null},
    "en_ru": {"after": null, "delta": null},
    "he_en": {"after": null, "delta": null},
    "en_he": {"after": null, "delta": null}
  },
  "phases": {
    "1": {"status": "in_progress", "gate_b": null},
    "2": {"status": "pending", "gate_b": null},
    "3": {"status": "pending", "gate_b": null},
    "4": {"status": "pending", "gate_b": null},
    "5": {"status": "pending", "gate_b": null}
  },
  "protected_orchestration_baseline": "RUN-416/V246",
  "notes": []
}
```

Update this file after every phase measurement.
