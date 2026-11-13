#!/usr/bin/env python3
"""
prepare_translation_learning_sets.py
Version: 1.0  —  2026-05-11
Domain:  Russian / Hebrew ↔ English Translation

Reads translation DPO pair JSONL files, applies the hard gates defined in
TRANSLATION-DPO-REGISTRY-GUIDE-v1 and DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1,
and routes each pair to one of three output files:

  training-pairs-train-ready.jsonl         — passed all gates, cleared for training
  training-pairs-repair-required.jsonl     — failed one or more soft gates, needs edits
  training-pairs-blocked.jsonl             — blocked-source pairs; must be discarded

Mirrors the gate logic in the C# TranslationPairValidator.Validate() — any change
to validator logic must be reflected here and vice versa.

Usage:
    python prepare_translation_learning_sets.py [input_jsonl ...] [options]

Options:
    --input     One or more JSONL input files (default: reads from stdin)
    --out-dir   Output directory (default: current directory)
    --verbose   Print per-pair result summary
"""

import argparse
import json
import sys
from pathlib import Path


# ── Licence decision tables ──────────────────────────────────────────────────
# Source ids whose licence decision is BLOCK in TRANSLATION-SOURCE-REGISTRY-v2.
# Any pair referencing these sources must be routed to training-pairs-blocked.jsonl.
BLOCKED_SOURCES = {
    "culturax",
    "rnc",
    "cc100-he-unreviewed",
    "tedtalks",
    "sharedalign-raw",
}

# Source ids whose licence is REVIEW REQUIRED — not cleared for train-ready batches.
# These pairs are repair-required (the reviewer decides whether to accept or discard).
REVIEW_REQUIRED_SOURCES = {
    "cc100-ru",
    "cc100-he",
    "mnlp-he-unreviewed",
    "ccaligned-unreviewed",
}

# Source ids whose licence is QUARANTINE (ShareAlike) — allowed only with batch isolation.
# If batch_isolation: true is set, the pair is train-ready; otherwise repair-required.
QUARANTINE_SOURCES = {
    "wikimatrix",
    "hewiki",
    "flores200",
    "ntrex128",
    "opus-books-bysa",
}

# ── Direction registry ────────────────────────────────────────────────────────
# Valid direction codes — must match TranslationPromptBuilder.KnownDirections.
KNOWN_DIRECTIONS = {"RU_EN", "EN_RU", "HE_EN", "EN_HE"}

# ── Negation template gate ────────────────────────────────────────────────────
# Rejected sides that are refusals produce near-zero reward margin and must be
# discarded or repaired. The check is case-insensitive.
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

# ── Mojibake detection ────────────────────────────────────────────────────────
# UTF-8 bytes decoded as Latin-1 leave these marker strings in the text.
# Any pair containing these markers was mis-encoded and cannot be repaired safely.
MOJIBAKE_MARKERS = ["Ã", "Â", "â€", "Ã¢"]

# ── Token overlap gate ────────────────────────────────────────────────────────
# Overlap ratio threshold using whitespace approximation (not BPE tokens).
# 0.50 instead of 0.60 compensates for the coarser whitespace split vs. BPE.
# Must match TranslationPairValidator.OverlapThreshold.
OVERLAP_THRESHOLD = 0.50

# Minimum combined whitespace-token count to run the overlap gate.
# Pairs below this threshold have trivially high overlap by construction.
MIN_COMBINED_TOKENS = 12


# ── Gate implementations ──────────────────────────────────────────────────────

def contains_mojibake(text: str) -> bool:
    """Return True if the text contains any known mojibake marker."""
    return any(marker in text for marker in MOJIBAKE_MARKERS)


def has_negation_template(text: str) -> bool:
    """Return True if the text (lower-cased) contains a negation-template string."""
    lower = text.lower()
    return any(template in lower for template in NEGATION_TEMPLATES)


def overlap_ratio(a_side_tokens: list[str],
                  chosen_tokens: list[str],
                  rejected_tokens: list[str]) -> float:
    """
    Compute the whitespace-token overlap ratio between chosen and rejected,
    excluding tokens that also appear in the A-side prompt.

    Formula:
        shared = (set(chosen) ∩ set(rejected)) - set(a_side)
        total  = (set(chosen) ∪ set(rejected)) - set(a_side)
        ratio  = len(shared) / len(total)  or  0.0 if total == 0

    Mirrors TranslationPairValidator.ComputeOverlapRatio().
    """
    prompt_set = set(a_side_tokens)
    chosen_set = set(chosen_tokens)
    rejected_set = set(rejected_tokens)

    # Tokens shared by both sides that are not in the prompt.
    shared = (chosen_set & rejected_set) - prompt_set

    # All unique tokens from both sides that are not in the prompt.
    total = (chosen_set | rejected_set) - prompt_set

    # Avoid division by zero when all tokens are prompt tokens (very short pairs).
    if not total:
        return 0.0

    return len(shared) / len(total)


def validate_pair(pair: dict) -> tuple[str, list[str]]:
    """
    Validate a single translation pair against all 9 hard gates.

    Returns:
        ("blocked",        [reason])   — source licence is BLOCK; discard this pair.
        ("repair_required", [errors])  — one or more soft gates failed; needs editing.
        ("train_ready",    [])         — all gates passed.

    Gate numbering matches TranslationPairValidator.Validate() in C#.
    """
    a_side   = pair.get("a_side",   "") or ""
    chosen   = pair.get("chosen",   "") or ""
    rejected = pair.get("rejected", "") or ""
    direction = pair.get("direction", "") or ""
    source_id = (pair.get("source_id", "") or "").lower()

    errors: list[str] = []

    # Gate 1: identity — chosen and rejected must differ after whitespace trimming.
    if chosen.strip() == rejected.strip():
        errors.append("chosen_equals_rejected")

    # Gate 2: mojibake — none of the three text fields may contain encoding artefacts.
    for field_name, value in [("a_side", a_side), ("chosen", chosen), ("rejected", rejected)]:
        if contains_mojibake(value):
            errors.append(f"mojibake_in_{field_name}")

    # Gate 3: [WRONG:] annotation — rejected must carry a specific error label.
    if "[WRONG:" not in rejected:
        errors.append("missing_wrong_annotation")

    # Gate 4: negation template — rejected must not be a refusal or N/A value.
    if has_negation_template(rejected):
        errors.append("negation_template_as_rejected")

    # Gate 5: direction — every pair must declare its translation direction.
    if not direction:
        errors.append("missing_direction")
    elif direction not in KNOWN_DIRECTIONS:
        errors.append(f"unknown_direction:{direction}")

    # Gate 6: source licence — blocked sources are unrecoverable; route to blocked output.
    if source_id in BLOCKED_SOURCES:
        return "blocked", [f"source '{source_id}' is BLOCK — discard this pair"]

    # Gate 7: review-required sources may not appear in train-ready batches.
    if source_id in REVIEW_REQUIRED_SOURCES:
        errors.append("review_required_source")

    # Gate 8: quarantine sources must carry the batch_isolation flag.
    if source_id in QUARANTINE_SOURCES:
        has_isolation = pair.get("batch_isolation") is True
        if not has_isolation:
            errors.append("quarantine_source_without_batch_isolation")

    # Gate 9: whitespace-approximated token overlap — filters near-zero reward pairs.
    a_toks = a_side.split()
    c_toks = chosen.split()
    r_toks = rejected.split()

    if len(c_toks) + len(r_toks) >= MIN_COMBINED_TOKENS:
        ratio = overlap_ratio(a_toks, c_toks, r_toks)
        if ratio > OVERLAP_THRESHOLD:
            errors.append(f"overlap_ratio_too_high:{ratio:.3f}")

    # Route based on accumulated errors.
    if errors:
        return "repair_required", errors

    return "train_ready", []


# ── I/O helpers ───────────────────────────────────────────────────────────────

def iter_pairs(input_files: list[Path]):
    """
    Yield (source_path, line_number, pair_dict) for every non-blank line across
    all input files. Each line must be a valid JSON object.
    """
    for path in input_files:
        with path.open(encoding="utf-8") as fh:
            for line_no, raw in enumerate(fh, start=1):
                raw = raw.strip()
                if not raw:
                    # Skip blank lines between records.
                    continue
                try:
                    pair = json.loads(raw)
                except json.JSONDecodeError as exc:
                    # Malformed JSON is a hard error — print and skip the line.
                    print(f"[SKIP] {path}:{line_no} — JSON decode error: {exc}",
                          file=sys.stderr)
                    continue
                yield path, line_no, pair


# ── Main ──────────────────────────────────────────────────────────────────────

def main() -> None:
    parser = argparse.ArgumentParser(
        description="Route translation DPO pairs to train-ready / repair-required / blocked.")
    parser.add_argument(
        "input", nargs="*", type=Path,
        help="Input JSONL files. Reads from stdin if omitted.")
    parser.add_argument(
        "--out-dir", type=Path, default=Path("."),
        help="Directory for output JSONL files (default: current directory).")
    parser.add_argument(
        "--verbose", action="store_true",
        help="Print a one-line summary for every pair.")
    args = parser.parse_args()

    # Resolve input files — fall back to stdin as a single pseudo-file.
    if args.input:
        input_files = args.input
    else:
        # Wrap stdin in a temporary file object so iter_pairs can iterate it.
        input_files = [sys.stdin]

    out_dir: Path = args.out_dir
    out_dir.mkdir(parents=True, exist_ok=True)

    # Open the three output file handles.
    train_ready_path      = out_dir / "training-pairs-train-ready.jsonl"
    repair_required_path  = out_dir / "training-pairs-repair-required.jsonl"
    blocked_path          = out_dir / "training-pairs-blocked.jsonl"

    # Counters for the summary line printed at the end.
    counts = {"train_ready": 0, "repair_required": 0, "blocked": 0, "skipped": 0}

    with (
        train_ready_path.open("w", encoding="utf-8")     as train_fh,
        repair_required_path.open("w", encoding="utf-8") as repair_fh,
        blocked_path.open("w", encoding="utf-8")         as blocked_fh,
    ):
        for source_path, line_no, pair in iter_pairs(input_files):
            # Run all 9 validation gates.
            outcome, errors = validate_pair(pair)

            if outcome == "blocked":
                # Annotate the pair with the block reason and write to blocked output.
                pair["validation_outcome"] = "blocked"
                pair["validation_errors"]  = errors
                blocked_fh.write(json.dumps(pair, ensure_ascii=False) + "\n")
                counts["blocked"] += 1

            elif outcome == "repair_required":
                # Annotate the pair with the repair errors and write to repair output.
                pair["validation_outcome"] = "repair_required"
                pair["validation_errors"]  = errors
                repair_fh.write(json.dumps(pair, ensure_ascii=False) + "\n")
                counts["repair_required"] += 1

            else:
                # Pair is train-ready — write to train output with clean outcome flag.
                pair["validation_outcome"] = "train_ready"
                pair["validation_errors"]  = []
                train_fh.write(json.dumps(pair, ensure_ascii=False) + "\n")
                counts["train_ready"] += 1

            if args.verbose:
                pair_id = pair.get("id", f"{source_path}:{line_no}")
                if errors:
                    print(f"[{outcome.upper()}] {pair_id} — {', '.join(errors)}")
                else:
                    print(f"[TRAIN_READY] {pair_id}")

    # Print summary to stderr so it does not pollute redirected stdout.
    total = sum(counts.values())
    print(
        f"\nDone. {total} pairs processed.\n"
        f"  train_ready:     {counts['train_ready']}\n"
        f"  repair_required: {counts['repair_required']}\n"
        f"  blocked:         {counts['blocked']}\n"
        f"\nOutput files:\n"
        f"  {train_ready_path}\n"
        f"  {repair_required_path}\n"
        f"  {blocked_path}",
        file=sys.stderr,
    )

    # Exit with code 1 if any pairs were blocked — lets CI detect licence violations.
    if counts["blocked"] > 0:
        sys.exit(1)


if __name__ == "__main__":
    main()
