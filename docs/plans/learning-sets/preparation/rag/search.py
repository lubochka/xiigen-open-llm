#!/usr/bin/env python3
"""Offline retrieval of curated repository evidence; does not train or run models."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parent


def tokens(text):
    return set(re.findall(r"[\w-]+", text.casefold()))


def load_manifest():
    return json.loads((ROOT / "manifest.json").read_text(encoding="utf-8"))


def verify(manifest):
    errors, seen = [], set()
    for source in manifest["sources"]:
        sid = source["source_id"]
        if sid in seen:
            errors.append(f"duplicate source ID: {sid}")
        seen.add(sid)
        if source["branch"] not in manifest["allowed_source_branches"]:
            errors.append(f"unapproved branch: {sid}")
        if not re.fullmatch(r"[0-9a-f]{40}", source["commit"]):
            errors.append(f"invalid commit: {sid}")
        if not re.fullmatch(r"[0-9a-f]{40}", source["blob_sha"]):
            errors.append(f"invalid Git blob SHA: {sid}")
        file = (ROOT / source["local_excerpt_path"]).resolve()
        if ROOT not in file.parents or not file.is_file():
            errors.append(f"missing or out-of-tree excerpt: {sid}")
            continue
        actual = hashlib.sha256(file.read_bytes()).hexdigest()
        if actual != source["local_excerpt_sha256"]:
            errors.append(f"excerpt SHA256 mismatch: {sid}")
        for span in source["reviewed_line_spans"]:
            if not 1 <= span["start"] <= span["end"] <= source["source_line_count"]:
                errors.append(f"invalid reviewed span: {sid}")
    print(json.dumps({"status": "PASS" if not errors else "FAIL", "sources_checked": len(seen), "errors": errors}, ensure_ascii=False, indent=2))
    return 1 if errors else 0


def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--query", default="")
    parser.add_argument("--id", dest="source_id")
    parser.add_argument("--tag")
    parser.add_argument("--limit", type=int, default=5)
    parser.add_argument("--show", action="store_true", help="Include the saved reviewed excerpt")
    parser.add_argument("--json", action="store_true")
    parser.add_argument("--verify", action="store_true")
    args = parser.parse_args()
    manifest = load_manifest()
    if args.verify:
        return verify(manifest)
    if not 1 <= args.limit <= 20:
        parser.error("--limit must be between 1 and 20")
    if not args.query and not args.source_id and not args.tag:
        parser.error("Supply --query, --id, --tag, or --verify")
    q = tokens(args.query)
    found = []
    for source in manifest["sources"]:
        if args.source_id and source["source_id"] != args.source_id:
            continue
        if args.tag and args.tag.casefold() not in {x.casefold() for x in source["tags"] + source["search_terms"]}:
            continue
        meta = " ".join([source["path"], source["purpose"], source["observed"], source["current_use_and_caveats"], *source["tags"], *source["search_terms"]])
        text = (ROOT / source["local_excerpt_path"]).read_text(encoding="utf-8")
        score = 4 * len(q & tokens(meta)) + len(q & tokens(text))
        if q and score == 0:
            continue
        item = {"source_id": source["source_id"], "score": score, "path": source["path"], "branch": source["branch"], "commit": source["commit"], "source_url": source["source_url"], "reviewed_line_spans": source["reviewed_line_spans"], "observed": source["observed"], "current_use_and_caveats": source["current_use_and_caveats"]}
        if args.show:
            item["excerpt"] = text
        found.append(item)
    found.sort(key=lambda x: (-x["score"], x["source_id"]))
    found = found[:args.limit]
    if args.json:
        print(json.dumps(found, ensure_ascii=False, indent=2))
    else:
        for item in found:
            print(f"{item['source_id']} | {item['branch']} | {item['path']}")
            print(item["source_url"])
            print(item["observed"])
            print("Применение: " + item["current_use_and_caveats"])
            if args.show:
                print(item["excerpt"])
            print()
        if not found:
            print("Совпадений в ограниченной выборке нет. Откройте INDEX.md и coverage.json.")
    return 0 if found else 2


if __name__ == "__main__":
    raise SystemExit(main())
