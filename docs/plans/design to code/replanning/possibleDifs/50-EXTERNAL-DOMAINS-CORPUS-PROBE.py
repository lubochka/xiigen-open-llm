#!/usr/bin/env python3
"""Read-only, reproducible source audit of external_domains CODE_TO_DOC JSONL.

Usage:
 python 50-EXTERNAL-DOMAINS-CORPUS-PROBE.py --data-root "C:/.../llm_mvp_core/external domains/documentation/learning sets" --runs 3

The audit DOES NOT train, alter models, or certify semantic quality.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path

DOMAINS = (
    "dotnet-csharp", "sql-tsql", "angular-typescript",
    "mongodb-csharp", "elasticsearch-csharp",
)


def inspect(root: Path, runs: int) -> dict:
    if not root.is_dir():
        raise FileNotFoundError(f"dataset directory missing: {root}")

    all_prompts: set[str] = set()
    totals = Counter()
    domains = []
    overlap_examples: list[dict] = []

    for domain in DOMAINS:
        stats = Counter()
        prompt_set: set[str] = set()
        exact_triples: dict[tuple[str, str, str], list[tuple[str, object]]] = defaultdict(list)
        files = []

        for run in range(1, runs + 1):
            path = root / domain / f"{domain}-run-{run:03}.jsonl"
            if not path.is_file():
                raise FileNotFoundError(f"input absent: {path}")
            data = path.read_bytes()
            sha = hashlib.sha256(data).hexdigest()
            per_file = Counter()

            try:
                lines = data.decode("utf-8-sig").splitlines()
            except UnicodeDecodeError as exc:
                raise ValueError(f"non-UTF8 source: {path}") from exc

            for lineno, line in enumerate(lines, start=1):
                if not line.strip():
                    raise ValueError(f"empty record at {path}:{lineno}")
                try:
                    row = json.loads(line)
                except json.JSONDecodeError as exc:
                    raise ValueError(f"invalid json at {path}:{lineno}") from exc

                if not isinstance(row, dict):
                    raise ValueError(f"non-object row at {path}:{lineno}")
                for key in ("Prompt", "Chosen", "Rejected"):
                    if not isinstance(row.get(key), str):
                        raise ValueError(f"missing string {key} at {path}:{lineno}")

                prompt, chosen, rejected = row["Prompt"], row["Chosen"], row["Rejected"]
                pair_id = str(row.get("pair_id", f"{domain}:{run}:{lineno}"))
                prompt_set.add(prompt)
                all_prompts.add(prompt)
                stats["rows"] += 1
                per_file["rows"] += 1

                if chosen == rejected:
                    stats["identical_pair"] += 1
                    per_file["identical_pair"] += 1
                if row.get("curriculum_tier") == 3 and row.get("rejected_type") == "missing-edge-case":
                    stats["potentially_reversed_by_tier"] += 1
                    per_file["potentially_reversed_by_tier"] += 1
                if row.get("curriculum_tier") == 2 and row.get("rejected_type") == "missing-edge-case":
                    stats["tier2_missing_edge"] += 1
                if row.get("compiled_prompt_in_training") is False:
                    stats["compiled_prompt_false"] += 1
                if isinstance(row.get("annotation"), str) and row["annotation"] not in rejected:
                    stats["annotation_stored_separately"] += 1

                triple = (prompt, chosen, rejected)
                exact_triples[triple].append((pair_id, row.get("overlap_ratio")))

            files.append({
                "name": path.name, "sha256": sha,
                "rows": per_file["rows"], "identical_pair": per_file["identical_pair"],
                "potentially_reversed_by_tier": per_file["potentially_reversed_by_tier"],
            })

        stats["unique_prompts"] = len(prompt_set)
        stats["unique_triples"] = len(exact_triples)
        for repeated in exact_triples.values():
            scores = {repr(score) for _, score in repeated}
            if len(scores) > 1:
                stats["same_text_different_stored_overlap"] += 1
                if len(overlap_examples) < 5:
                    overlap_examples.append({
                        "domain": domain,
                        "pair_ids_and_stored_ratios": [
                            {"pair_id": pair_id, "overlap_ratio": score}
                            for pair_id, score in repeated[:4]
                        ],
                    })

        totals.update(stats)
        domains.append({
            "domain": domain,
            **dict(stats),
            "files": files,
            "share_identical": round(stats["identical_pair"] / stats["rows"], 4) if stats["rows"] else None,
        })

    # Unique prompts cannot be summed across domains: compute global cardinality separately.
    totals["unique_prompts"] = len(all_prompts)
    return {
        "audit": "external-domains-dpo-source-only",
        "scope": "first N JSONL runs per domain, not all source files and NOT RP01 input",
        "data_root": str(root),
        "runs_per_domain": runs,
        "domains": domains,
        "totals": dict(totals),
        "overlap_drift_examples": overlap_examples,
        "semantic_label_inversion_proven": False,
        "model_training_executed": False,
        "training_consumer_lineage_proven": False,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--data-root", type=Path, required=True)
    parser.add_argument("--runs", type=int, default=3)
    parser.add_argument("--output", type=Path, default=None,
                        help="Optional path for JSON report; originals are never changed")
    args = parser.parse_args()

    if not 1 <= args.runs <= 50:
        parser.error("--runs must be between 1 and 50")
    try:
        report = inspect(args.data_root, args.runs)
    except (OSError, ValueError) as exc:
        print(json.dumps({"error": str(exc), "status": "SOURCE_AUDIT_FAILED"}, ensure_ascii=False),
              file=sys.stderr)
        return 2

    rendered = json.dumps(report, ensure_ascii=False, indent=2, sort_keys=True)
    if args.output is not None:
        output = args.output.expanduser().resolve()
        if output.exists():
            print(json.dumps({"error": f"output already exists: {output}"}, ensure_ascii=False), file=sys.stderr)
            return 2
        # Explicit opt-in only. Never write reports into input source tree.
        if args.data_root.resolve() == output or args.data_root.resolve() in output.parents:
            print(json.dumps({"error": "output cannot be within the dataset directory"}), file=sys.stderr)
            return 2
        output.write_text(rendered + "\n", encoding="utf-8")
    print(rendered)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
