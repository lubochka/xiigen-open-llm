#!/usr/bin/env python3
"""
Build XIIGen orchestration prompt-surface candidates from instruction datasets.

This script deliberately does not treat external assistant answers as authority.
It reads accepted instruction/chat datasets, keeps only user/prompter messages,
and writes model-review candidates whose chosen side must be adjudicated against
XIIGen contracts before training.
"""

from __future__ import annotations

import argparse
import gzip
import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any, Dict, Iterable, Iterator, List, Optional


OASST_SOURCES = {
    "oasst1": {
        "license": "apache-2.0",
        "url": "https://huggingface.co/datasets/OpenAssistant/oasst1/resolve/main/2023-04-12_oasst_all.messages.jsonl.gz",
    },
    "oasst2": {
        "license": "apache-2.0",
        "url": "https://huggingface.co/datasets/OpenAssistant/oasst2/resolve/main/2023-11-05_oasst2_all.messages.jsonl.gz",
    },
}

MOJIBAKE_MARKERS = ("Ã", "Â", "Ã¢")

PAIR_FAMILIES = {
    "IT-ORCH-01": {
        "name": "Root intent to routing compiler",
        "contract": "PromptRoutingInputCompilerUnit + DevelopmentPromptCompiler predicate lexicon",
        "chosen": "Compile the user request into SubPhaseRouterSourceDocument fields, emit code-backed predicate ids, set confidence 0.92 when predicates match or 0.70 with a knowledge-edge warning when none match.",
        "rejected": "Answer the user directly as a generic assistant and skip prompt compilation, predicate evidence, and routing metadata.\n[WRONG: generic chat answer bypasses the root routing compiler, so the orchestrator cannot learn which predicates and sub-phases the request activates.]",
    },
    "IT-ORCH-02": {
        "name": "Ambiguous request to multi-sub-phase routing",
        "contract": "MultiSubPhaseRouterUnit all-matching-sub-phases contract",
        "chosen": "Route every supported sub-phase instead of choosing only one branch; preserve evidence predicates for each route and surface knowledge-edge metrics for missing flow coverage.",
        "rejected": "Route only the strongest-looking branch and suppress the other matched sub-phase.\n[WRONG: when multiple predicates support multiple sub-phases, one-route output hides valid orchestration work and creates a false clean pass.]",
    },
    "IT-ORCH-03": {
        "name": "Clarification before execution",
        "contract": "Knowledge-edge detection + minimum clarification gate",
        "chosen": "Record missing constraints as knowledge-edge evidence and ask the minimum clarifying question before execution; do not fabricate scope, authority, or acceptance criteria.",
        "rejected": "Proceed with implementation using assumed requirements and no missing-constraint record.\n[WRONG: executing before constraint capture trains the model to fabricate missing scope and makes downstream validation impossible.]",
    },
    "IT-ORCH-04": {
        "name": "Complex task decomposition",
        "contract": "ARCH/PLAN orchestration guidance, sub-phase ordering, and skill-load rules",
        "chosen": "Decompose the request into goal, sub-phases, dependency order, required skills, evidence artifacts, and validation gates before code work begins.",
        "rejected": "Return a flat checklist or immediate coding answer without sub-phase order, skill activation, or evidence gates.\n[WRONG: flat planning loses orchestration dependencies and cannot teach the model how to manage a complex multi-step task.]",
    },
    "IT-ORCH-05": {
        "name": "Skill activation and load order",
        "contract": "HOW-TO-USE-SKILLS + XIIGen skill load-order constraints",
        "chosen": "Select governance/planning skills before implementation skills, state the load order, and explain which missing authority blocks execution.",
        "rejected": "Load a late implementation skill first and skip governance/planning skills.\n[WRONG: late skill activation before planning authority causes execution drift and weakens the orchestration signal.]",
    },
}

KEYWORDS = {
    "IT-ORCH-05": re.compile(r"\b(skill|skills|teach|training|guidance|model feedback|learning set)\b", re.I),
    "IT-ORCH-03": re.compile(r"\b(clarify|unclear|not sure|ambiguous|requirements?|constraints?|before|should i|which)\b", re.I),
    "IT-ORCH-02": re.compile(r"\b(and|also|while|both|multiple|pipeline|workflow|end[- ]to[- ]end|test.*document|plan.*implement)\b", re.I),
    "IT-ORCH-04": re.compile(r"\b(build|design|create|make|plan|architecture|system|project|workflow|orchestrate|agent)\b", re.I),
}


def sha1_short(value: str) -> str:
    return hashlib.sha1(value.encode("utf-8", errors="replace")).hexdigest()[:12]


def has_mojibake(text: str) -> bool:
    return any(marker in text for marker in MOJIBAKE_MARKERS)


def read_jsonl(path: Path) -> Iterator[Dict[str, Any]]:
    opener = gzip.open if path.suffix.lower() == ".gz" else open
    mode = "rt"
    with opener(path, mode, encoding="utf-8-sig", errors="replace") as handle:
        for line in handle:
            line = line.strip()
            if not line:
                continue
            try:
                obj = json.loads(line)
            except json.JSONDecodeError:
                continue
            if isinstance(obj, dict):
                yield obj


def message_text(row: Dict[str, Any]) -> str:
    for key in ("text", "content", "message"):
        value = row.get(key)
        if isinstance(value, str) and value.strip():
            return value.strip()
    return ""


def is_prompter(row: Dict[str, Any]) -> bool:
    role = str(row.get("role") or row.get("speaker") or "").lower()
    return role in {"prompter", "user", "human"}


def source_lang(row: Dict[str, Any]) -> str:
    return str(row.get("lang") or row.get("language") or "").lower()


def select_family(text: str) -> Optional[str]:
    for family, pattern in KEYWORDS.items():
        if pattern.search(text):
            return family
    return "IT-ORCH-01" if len(text.split()) >= 8 else None


def build_candidate(row: Dict[str, Any], dataset: str, source_file: str, seq: int, family: str) -> Dict[str, Any]:
    text = message_text(row)
    family_def = PAIR_FAMILIES[family]
    message_id = str(row.get("message_id") or row.get("id") or sha1_short(text))
    tree_id = str(row.get("message_tree_id") or row.get("tree_id") or "")
    pair_id = f"{dataset.upper()}-{family}-{seq:04d}-{sha1_short(message_id + text)}"
    return {
        "pair_id": pair_id,
        "source_dataset": dataset,
        "source_file": source_file,
        "source_license_state": OASST_SOURCES.get(dataset, {}).get("license", "unknown"),
        "source_id": f"{dataset}:{message_id}",
        "source_message_id": message_id,
        "source_tree_id": tree_id,
        "external_role": "a_side_style_only",
        "quality_status": "model_review_required",
        "pair_kind": family,
        "pair_family_name": family_def["name"],
        "internal_contract_reference": family_def["contract"],
        "a_side": text,
        "chosen_candidate": family_def["chosen"],
        "rejected_candidate": family_def["rejected"],
        "review_instruction": "Model-led adjudication must rewrite/confirm chosen side against XIIGen code-backed orchestration contracts before this row can become train_ready.",
        "filters": {
            "prompter_only": True,
            "assistant_answer_used": False,
            "mojibake_free": not has_mojibake(text),
        },
    }


def iter_candidates(paths: List[Path], limit: int) -> Iterator[Dict[str, Any]]:
    seq = 0
    seen_text: set[str] = set()
    for path in paths:
        dataset = "oasst2" if "oasst2" in path.name.lower() else "oasst1"
        for row in read_jsonl(path):
            if not is_prompter(row):
                continue
            lang = source_lang(row)
            if lang and lang not in {"en", "eng"}:
                continue
            text = message_text(row)
            if len(text) < 40 or len(text) > 1600:
                continue
            if has_mojibake(text):
                continue
            key = re.sub(r"\s+", " ", text).strip().lower()
            if key in seen_text:
                continue
            family = select_family(text)
            if not family:
                continue
            seen_text.add(key)
            seq += 1
            yield build_candidate(row, dataset, str(path), seq, family)
            if seq >= limit:
                return


def write_jsonl(path: Path, rows: Iterable[Dict[str, Any]]) -> int:
    count = 0
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        for row in rows:
            handle.write(json.dumps(row, ensure_ascii=False, sort_keys=True) + "\n")
            count += 1
    return count


def write_summary(path: Path, rows: List[Dict[str, Any]], input_paths: List[Path]) -> None:
    by_family = Counter(row["pair_kind"] for row in rows)
    by_dataset = Counter(row["source_dataset"] for row in rows)
    lines = [
        "# Instruction-Tuning Orchestration Pilot Summary",
        "",
        f"Rows produced: {len(rows)}",
        "",
        "## Inputs",
        "",
    ]
    for source_path in input_paths:
        lines.append(f"- `{source_path}`")
    lines.extend(["", "## Dataset Distribution", ""])
    for key, value in sorted(by_dataset.items()):
        lines.append(f"- {key}: {value}")
    lines.extend(["", "## Pair Family Distribution", ""])
    for key, value in sorted(by_family.items()):
        lines.append(f"- {key}: {value}")
    lines.extend([
        "",
        "## Gate State",
        "",
        "- All rows are `model_review_required`, not train-ready.",
        "- External rows are A-side prompt-surface only.",
        "- Assistant answers are not used.",
        "- Chosen candidates must be rewritten/confirmed against XIIGen contracts before training.",
    ])
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", action="append", required=True, help="Path to OASST .jsonl or .jsonl.gz file. Repeatable.")
    parser.add_argument("--output-dir", default="external sources learning 2/instruction-tuning-pilot")
    parser.add_argument("--limit", type=int, default=64)
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    input_paths = [Path(p) for p in args.input]
    missing = [str(p) for p in input_paths if not p.exists()]
    if missing:
        raise SystemExit(f"Missing input files: {missing}")
    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    rows = list(iter_candidates(input_paths, args.limit))
    jsonl_path = output_dir / "oasst-orchestration-pilot-candidates.jsonl"
    summary_path = output_dir / "oasst-orchestration-pilot-summary.md"
    write_jsonl(jsonl_path, rows)
    write_summary(summary_path, rows, input_paths)
    print(json.dumps({
        "rows": len(rows),
        "jsonl": str(jsonl_path),
        "summary": str(summary_path),
        "quality_status": "model_review_required",
    }, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
