#!/usr/bin/env python3
"""Offline integrity check of the 2026-10-25 preparation package.

This checks dataset preparation, not semantic admission or experimental learning.
It never calls CODEX, Kiro, a remote model, a trainer or product code. Optional
prepare checks exercise all 162 effective streams in a temporary folder. COUNT=1
is used where valid; an objective that rejects it uses its declared first count.
No generated candidates, accepted corpus or checkpoint are produced.

Run from any directory:
  python path/to/preparation/tools/verify_preparation.py \
    --rag-root path/to/main/llm-mvp/learning-sets/preparation \
    --out new-report.json --prepare-check

The package root is resolved from this file; the RAG root is an explicit input.
Only the Python standard library is required. Existing reports are not replaced.
"""
from __future__ import annotations

import argparse
from collections import Counter
from datetime import datetime, timezone
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import shlex
import subprocess
import sys
import tempfile
from urllib.parse import unquote, urlsplit


BASE = Path(__file__).resolve().parents[1]
EXPECTED = {
    "parent_topics": 70,
    "parent_profile_streams": 129,
    "objective_cards": 49,
    "objective_profile_streams": 119,
    "covered_parent_profile_streams": 86,
    "remaining_parent_profile_streams": 43,
    "effective_streams": 162,
    "physical_cards": 119,
    "active_reference_rows": 728,
    "target_accepted_train_pairs": 129000
}
EXPECTED_SOURCES = 57
EXPECTED_QUARANTINE_IDS = {'CODE-SQL-GUARD.REF.02', 'CODE-SQL-GUARD.REF.03', 'OR-ROUTE-01-REF-012', 'CODE-SQL-GUARD.REF.04', 'CODE-SQL-GUARD.REF.01', 'TR08-REF-05', 'TR08-REF-02'}
# Frozen names from the pre-extension index; names are evidence, not new topics.
BASELINE_INDEX_SHA256 = "61d3b874e9eb8ae5d96467d0331b47b5d3281d1af6856af45cd2c6334e2cad3c"
BASELINE_TOPIC_IDS = [
    "TR01", "DOC-CS-01", "CODE-CS-01", "OR-ROUTE-01", "TR02", "TR03", "TR04",
    "TR05", "TR06", "TR07", "TR08", "TR09", "TR10", "OR-FUNC-01", "OR-ARCH-01",
    "OR-CODE-01", "OR-DB-01", "OR-CLIENT-01", "OR-SERVER-01", "OR-MOBILE-01",
    "OR-DESIGN-01", "OR-UX-01", "OR-QA-01", "OR-AUTO-01", "DOC-CS-02", "DOC-CS-03",
    "DOC-SQL-01", "DOC-SQL-02", "DOC-SQL-03", "DOC-NG-01", "DOC-NG-02", "DOC-NG-03",
    "CODE-CS-02", "CODE-SQL-01", "CODE-SQL-02", "CODE-NG-01", "CODE-NG-02",
]

# Immutable baseline evidence: the 185 original rows may gain metadata or move
# to quarantine, but their learner text, IDs, rationale and provenance survive.
# These are source snapshot fingerprints, not stale report hashes.
BASELINE_REFERENCE_FIELDS = ('schema_version', 'pair_id', 'domain_id', 'topic_id', 'profile_id', 'family_id', 'split', 'prompt', 'chosen', 'rejected', 'error_tag', 'rationale', 'source_refs', 'provenance')
BASELINE_REFERENCE_IDS = ('CODE-CS-01.REF.001', 'CODE-CS-01.REF.002', 'CODE-CS-01.REF.003', 'CODE-CS-01.REF.004', 'CODE-CS-02.REF.001', 'CODE-CS-02.REF.002', 'CODE-CS-02.REF.003', 'CODE-CS-02.REF.004', 'CODE-NG-01.REF.001', 'CODE-NG-01.REF.002', 'CODE-NG-01.REF.003', 'CODE-NG-01.REF.004', 'CODE-NG-02.REF.001', 'CODE-NG-02.REF.002', 'CODE-NG-02.REF.003', 'CODE-NG-02.REF.004', 'CODE-SQL-01.REF.001', 'CODE-SQL-01.REF.002', 'CODE-SQL-01.REF.003', 'CODE-SQL-01.REF.004', 'CODE-SQL-02.REF.001', 'CODE-SQL-02.REF.002', 'CODE-SQL-02.REF.003', 'CODE-SQL-02.REF.004', 'DOC-CS-01.REF.001', 'DOC-CS-01.REF.002', 'DOC-CS-01.REF.003', 'DOC-CS-01.REF.004', 'DOC-CS-02.REF.001', 'DOC-CS-02.REF.002', 'DOC-CS-02.REF.003', 'DOC-CS-02.REF.004', 'DOC-CS-03.REF.001', 'DOC-CS-03.REF.002', 'DOC-CS-03.REF.003', 'DOC-CS-03.REF.004', 'DOC-NG-01.REF.001', 'DOC-NG-01.REF.002', 'DOC-NG-01.REF.003', 'DOC-NG-01.REF.004', 'DOC-NG-02.REF.001', 'DOC-NG-02.REF.002', 'DOC-NG-02.REF.003', 'DOC-NG-02.REF.004', 'DOC-NG-03.REF.001', 'DOC-NG-03.REF.002', 'DOC-NG-03.REF.003', 'DOC-NG-03.REF.004', 'DOC-SQL-01.REF.001', 'DOC-SQL-01.REF.002', 'DOC-SQL-01.REF.003', 'DOC-SQL-01.REF.004', 'DOC-SQL-02.REF.001', 'DOC-SQL-02.REF.002', 'DOC-SQL-02.REF.003', 'DOC-SQL-02.REF.004', 'DOC-SQL-03.REF.001', 'DOC-SQL-03.REF.002', 'DOC-SQL-03.REF.003', 'DOC-SQL-03.REF.004', 'OR-ARCH-01-REF-001', 'OR-ARCH-01-REF-002', 'OR-ARCH-01-REF-003', 'OR-ARCH-01-REF-004', 'OR-AUTO-01-REF-001', 'OR-AUTO-01-REF-002', 'OR-AUTO-01-REF-003', 'OR-AUTO-01-REF-004', 'OR-CLIENT-01-REF-001', 'OR-CLIENT-01-REF-002', 'OR-CLIENT-01-REF-003', 'OR-CLIENT-01-REF-004', 'OR-CODE-01-REF-001', 'OR-CODE-01-REF-002', 'OR-CODE-01-REF-003', 'OR-CODE-01-REF-004', 'OR-DB-01-REF-001', 'OR-DB-01-REF-002', 'OR-DB-01-REF-003', 'OR-DB-01-REF-004', 'OR-DESIGN-01-REF-001', 'OR-DESIGN-01-REF-002', 'OR-DESIGN-01-REF-003', 'OR-DESIGN-01-REF-004', 'OR-FUNC-01-REF-001', 'OR-FUNC-01-REF-002', 'OR-FUNC-01-REF-003', 'OR-FUNC-01-REF-004', 'OR-MOBILE-01-REF-001', 'OR-MOBILE-01-REF-002', 'OR-MOBILE-01-REF-003', 'OR-MOBILE-01-REF-004', 'OR-QA-01-REF-001', 'OR-QA-01-REF-002', 'OR-QA-01-REF-003', 'OR-QA-01-REF-004', 'OR-ROUTE-01-REF-001', 'OR-ROUTE-01-REF-002', 'OR-ROUTE-01-REF-003', 'OR-ROUTE-01-REF-004', 'OR-ROUTE-01-REF-005', 'OR-ROUTE-01-REF-006', 'OR-ROUTE-01-REF-007', 'OR-ROUTE-01-REF-008', 'OR-ROUTE-01-REF-009', 'OR-ROUTE-01-REF-010', 'OR-ROUTE-01-REF-011', 'OR-ROUTE-01-REF-012', 'OR-ROUTE-01-REF-013', 'OR-ROUTE-01-REF-014', 'OR-ROUTE-01-REF-015', 'OR-ROUTE-01-REF-016', 'OR-ROUTE-01-REF-017', 'OR-ROUTE-01-REF-018', 'OR-ROUTE-01-REF-019', 'OR-ROUTE-01-REF-020', 'OR-ROUTE-01-REF-021', 'OR-SERVER-01-REF-001', 'OR-SERVER-01-REF-002', 'OR-SERVER-01-REF-003', 'OR-SERVER-01-REF-004', 'OR-UX-01-REF-001', 'OR-UX-01-REF-002', 'OR-UX-01-REF-003', 'OR-UX-01-REF-004', 'TR01-REF-01', 'TR01-REF-02', 'TR01-REF-03', 'TR01-REF-04', 'TR01-REF-05', 'TR01-REF-06', 'TR02-REF-01', 'TR02-REF-02', 'TR02-REF-03', 'TR02-REF-04', 'TR02-REF-05', 'TR02-REF-06', 'TR03-REF-01', 'TR03-REF-02', 'TR03-REF-03', 'TR03-REF-04', 'TR03-REF-05', 'TR03-REF-06', 'TR04-REF-01', 'TR04-REF-02', 'TR04-REF-03', 'TR04-REF-04', 'TR04-REF-05', 'TR04-REF-06', 'TR05-REF-01', 'TR05-REF-02', 'TR05-REF-03', 'TR05-REF-04', 'TR05-REF-05', 'TR05-REF-06', 'TR06-REF-01', 'TR06-REF-02', 'TR06-REF-03', 'TR06-REF-04', 'TR06-REF-05', 'TR06-REF-06', 'TR07-REF-01', 'TR07-REF-02', 'TR07-REF-03', 'TR07-REF-04', 'TR07-REF-05', 'TR07-REF-06', 'TR08-REF-01', 'TR08-REF-02', 'TR08-REF-03', 'TR08-REF-04', 'TR08-REF-05', 'TR08-REF-06', 'TR09-REF-01', 'TR09-REF-02', 'TR09-REF-03', 'TR09-REF-04', 'TR09-REF-05', 'TR09-REF-06', 'TR10-REF-01', 'TR10-REF-02', 'TR10-REF-03', 'TR10-REF-04', 'TR10-REF-05', 'TR10-REF-06')
BASELINE_REFERENCE_CORE_SHA256 = '54cc6bfe69b81e9920a55899dd2fd19cdbec794b9edd84246093fd4be332bd99'
BASELINE_REFERENCE_FILE_SHA256 = {'domains/coding/reference-examples.jsonl': '18daa97fec90836ab8d5ea4168a4d588f0c5c9df5c143cb0c941d10113bf7ab2', 'domains/documentation/reference-examples.jsonl': 'd4d9a1f00330330f05cb15febf16c8408503fe9daa88f5a1e41ee28e4ed657f3', 'domains/orchestration/reference-examples.jsonl': '4db4509e213582ae87edf72fd563eab6bae7700b6dbcd95c6733243b4a124067', 'domains/translation/reference-examples.jsonl': '9efc11f8dcd66db59e88b5c0790698a34aae7677c1b50fe64fe3073db9d08a30'}


def read_json(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def sha256(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def relative(path, root=BASE):
    return Path(path).resolve().relative_to(root.resolve()).as_posix()


def within(path, root):
    try:
        Path(path).resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


def load_sets():
    spec = importlib.util.spec_from_file_location("xiigen_sets_package_check", BASE / "tools/sets.py")
    module = importlib.util.module_from_spec(spec)
    old = sys.dont_write_bytecode
    sys.dont_write_bytecode = True
    try:
        spec.loader.exec_module(module)
    finally:
        sys.dont_write_bytecode = old
    return module


def source_url(url):
    return unquote(url.split("#", 1)[0]).rstrip("/")


def visible_markdown(text):
    """Exclude fenced examples from link and heading checks."""
    result = []
    fence = None
    for line in text.splitlines():
        marker = re.match(r"^\s*(`{3,}|~{3,})", line)
        if marker:
            token = marker.group(1)
            if fence is None:
                fence = token[0]
            elif token[0] == fence:
                fence = None
            result.append("")
        elif fence is None:
            result.append(line)
        else:
            result.append("")
    return "\n".join(result)


def markdown_anchors(text):
    seen = Counter()
    anchors = set(re.findall(r'\bid=["\x27]([^"\x27]+)["\x27]', text))
    for line in visible_markdown(text).splitlines():
        match = re.match(r"^#{1,6}\s+(.+?)\s*#*\s*$", line)
        if not match:
            continue
        heading = re.sub(r"\[([^\]]+)\]\([^)]*\)", r"\1", match.group(1))
        heading = re.sub(r"<[^>]*>", "", heading).replace("`", "").lower()
        slug = re.sub(r"[^\w\- ]", "", heading).replace(" ", "-")
        count = seen[slug]
        seen[slug] += 1
        anchors.add(slug if not count else f"{slug}-{count}")
    return anchors


def link_targets(text):
    """Extract inline and explicit reference-definition Markdown destinations."""
    text = visible_markdown(text)
    pattern = r"\]\(\s*(?:<([^>]+)>|([^\s)]+))(?:\s+[\"'][^\n]*?[\"'])?\s*\)"
    targets = [a or b for a, b in re.findall(pattern, text)]
    for match in re.finditer(r"^\s*\[[^\]]+\]:\s*(?:<([^>]+)>|(\S+))", text, re.M):
        targets.append(match.group(1) or match.group(2))
    return targets


def check_links(paths, rag_root, known_sources, planned_output=None):
    counts = Counter()
    errors = []
    unverified = set()
    source_prefix = "/lubochka/xiigen-planning/"
    package_path = "llm mvp/learning-sets/preparation"
    for path in sorted(set(paths)):
        root = rag_root if within(path, rag_root) else BASE
        label = ("rag-root/" if root == rag_root else "") + relative(path, root)
        for target in link_targets(path.read_text(encoding="utf-8")):
            counts["links_seen"] += 1
            if "\\" in target or re.search(r"%(?![0-9A-Fa-f]{2})", target):
                errors.append(f"{label}: MALFORMED_LINK {target}")
                continue
            parsed = urlsplit(target)
            destination = None
            if parsed.scheme in ("http", "https"):
                if not parsed.netloc:
                    errors.append(f"{label}: MISSING_HOST {target}")
                    continue
                if source_url(target) in known_sources:
                    counts["pinned_source_links_matched"] += 1
                    continue
                # Current package links can be checked without a GitHub request.
                parts = unquote(parsed.path).split("/", 5)
                if (parsed.netloc == "github.com" and parsed.path.startswith(source_prefix)
                        and len(parts) == 6 and parts[3] in ("blob", "tree")
                        and parts[4] in ("main", "llm_plan3")
                        and (parts[5] == package_path or parts[5].startswith(package_path + "/"))):
                    checkout = rag_root if parts[4] == "main" else BASE
                    destination = checkout / parts[5][len(package_path):].lstrip("/")
                    if not within(destination, checkout):
                        errors.append(f"{label}: LINK_ESCAPES_PACKAGE {target}")
                        continue
                    counts["github_package_links_checked_locally"] += 1
                else:
                    # A valid URL spelling is not a live reachability guarantee.
                    counts["external_url_syntax_only"] += 1
                    unverified.add(target)
                    continue
            elif parsed.scheme:
                if parsed.scheme == "mailto":
                    counts["mailto_links"] += 1
                    continue
                errors.append(f"{label}: UNSUPPORTED_LINK_SCHEME {target}")
                continue
            else:
                destination = path if not parsed.path else path.parent / unquote(parsed.path)
                if not (within(destination, BASE) or within(destination, rag_root)):
                    errors.append(f"{label}: LINK_OUTSIDE_CHECKED_ROOTS {target}")
                    continue
                counts["local_links_checked"] += 1
            destination = destination.resolve()
            if not destination.exists() and planned_output is not None and destination == Path(planned_output).resolve():
                # This one self-referenced report is created exclusively by main()
                # after run() returns. No unrelated future artifact is exempted.
                counts["self_report_link_pending_exclusive_write"] += 1
            elif not destination.exists():
                errors.append(f"{label}: LINK_TARGET_MISSING {target}")
            elif parsed.fragment and destination.suffix.lower() == ".md":
                if unquote(parsed.fragment) not in markdown_anchors(destination.read_text(encoding="utf-8")):
                    errors.append(f"{label}: ANCHOR_MISSING {target}")
                else:
                    counts["anchors_checked"] += 1
    return {"files_checked": len(set(paths)), **dict(counts), "errors": errors,
            "external_urls_not_fetched": sorted(unverified),
            "source_excerpt_body_links_excluded": True}


def domain_for(topic, profile):
    return "tr." + profile.split("-")[-1] if topic["domain_ids"][0].startswith("tr.") else topic["domain_ids"][0]


def parse_prepare_command(command):
    """Read a declared command without executing shell text."""
    words = shlex.split(command)
    if len(words) < 3 or words[0] not in ("python", "python3") or words[1] != "tools/sets.py" or words[2] != "prepare":
        raise ValueError("NOT_A_DECLARED_SETS_PREPARE_COMMAND")
    allowed = {"topic", "profile", "objective", "run", "count", "split", "start", "work-root", "rag-root", "family-plan"}
    result = {}
    tail = words[3:]
    if len(tail) % 2: raise ValueError("ODD_PREPARE_ARGUMENT_LIST")
    for key, value in zip(tail[::2], tail[1::2]):
        if not key.startswith("--") or key[2:] not in allowed or key[2:] in result:
            raise ValueError("UNKNOWN_OR_DUPLICATE_PREPARE_ARGUMENT " + key)
        result[key[2:]] = value
    required = {"topic", "profile", "run", "count", "split", "work-root", "rag-root"}
    if not required <= set(result): raise ValueError("MISSING_PREPARE_ARGUMENTS")
    result["count"] = int(result["count"])
    result["start"] = int(result.get("start", 0))
    if not 1 <= result["count"] <= 50 or result["start"] < 0:
        raise ValueError("PREPARE_COUNT_OR_START_OUT_OF_BOUNDS")
    return result


def transition_rows(text):
    headings = list(re.finditer(r"^## Переходы (?:единой|полной) очереди\s*$", text, re.M))
    if len(headings) != 1: raise ValueError("FULL_QUEUE_NAVIGATION_SECTION_MISSING_OR_DUPLICATED")
    part = text[headings[0].end():]
    part = re.split(r"^## ", part, maxsplit=1, flags=re.M)[0]
    return [(sid, int(order), nxt) for sid, order, nxt in re.findall(
        r"^\|\s*`([^`]+)`\s*\|\s*(\d+)\s*\|\s*`([^`]+)`\s*\|\s*$", part, re.M)]


def canonical_hash(value):
    data = json.dumps(value, sort_keys=True, ensure_ascii=False, separators=(",", ":"), allow_nan=False).encode("utf-8")
    return hashlib.sha256(data).hexdigest()


def run(args):
    rag_root = Path(args.rag_root).expanduser().resolve()
    m = load_sets()
    index_path = BASE / "index.json"
    index = read_json(index_path)
    topics = index["topics"]
    registry = {t["id"]: t for t in topics}
    parent_streams = [(t, p) for t in topics for p in t["profiles"]]
    curriculum_path = BASE / "curriculum/index.json"
    curriculum = read_json(curriculum_path)
    objective_rows = curriculum["objectives"]
    objective_index = {x["id"]: x for x in objective_rows}
    curriculum_streams = curriculum["stream_order"]
    queue_path = BASE / "generation-queue.json"
    queued = read_json(queue_path)["streams"]
    pilot_path = BASE / "curriculum/pilot-queue.json"
    pilot = read_json(pilot_path)["streams"]
    execution_path = BASE / "execution-queue.json"
    execution = read_json(execution_path)
    effective = execution["streams"]
    errors, checks = [], {}

    # Existing historical reports are deliberately not treated as current receipts.
    input_paths = sorted(p for p in BASE.rglob("*") if p.is_file()
                         and p.suffix in (".py", ".md", ".json", ".jsonl")
                         and "verification" not in p.relative_to(BASE).parts
                         and "__pycache__" not in p.parts)
    input_paths += sorted(p for p in rag_root.rglob("*") if p.is_file() and "__pycache__" not in p.parts)
    inputs_before = {str(p): sha256(p) for p in input_paths}

    def record(name, issues, **detail):
        checks[name] = {"passed": not issues, **detail, "errors": issues}
        errors.extend(f"{name}: {issue}" for issue in issues)

    def check_command(command, expected, issues, label):
        try:
            parsed = parse_prepare_command(command)
            for key, value in expected.items():
                if parsed.get(key) != value: issues.append(f"{label}: COMMAND_{key}_MISMATCH")
            return parsed
        except (ValueError, TypeError) as exc:
            issues.append(f"{label}: {exc}")
            return None

    issues = []
    if len(topics) != EXPECTED["parent_topics"] or len(parent_streams) != EXPECTED["parent_profile_streams"]:
        issues.append("PARENT_COUNTS_MISMATCH")
    if len(registry) != len(topics): issues.append("DUPLICATE_PARENT_ID")
    if index.get("counts", {}).get("topics") != len(topics) or index.get("counts", {}).get("streams") != len(parent_streams):
        issues.append("PARENT_INDEX_DECLARED_COUNTS_MISMATCH")
    for n, topic in enumerate(topics, 1):
        expected_next = topics[n]["id"] if n < len(topics) else None
        if any(topic.get(k) != n for k in ("order", "priority", "queue_priority")):
            issues.append(f"{topic['id']}: ORDER_PRIORITY_MISMATCH")
        if topic.get("next_id") != expected_next: issues.append(f"{topic['id']}: PARENT_NEXT_ID_MISMATCH")
        if len(set(topic["profiles"])) != len(topic["profiles"]): issues.append(f"{topic['id']}: DUPLICATE_PROFILE")
        for dependency in topic.get("prereqs", []):
            if dependency not in registry or registry[dependency].get("order", n) >= n:
                issues.append(f"{topic['id']}: INVALID_OR_LATER_PREREQUISITE {dependency}")
    record("parent_index_counts_and_order", issues, topics=len(topics), streams=len(parent_streams), sha256=sha256(index_path))
    missing = sorted(set(BASELINE_TOPIC_IDS) - set(registry))
    record("baseline_topics_preserved", ["MISSING " + x for x in missing], baseline_count=len(BASELINE_TOPIC_IDS),
           baseline_index_sha256=BASELINE_INDEX_SHA256, baseline_topic_ids=BASELINE_TOPIC_IDS,
           preserved_count=len(BASELINE_TOPIC_IDS) - len(missing))

    issues, local_topics = [], []
    topic_indexes = sorted(BASE.glob("domains/*/topic-index.json"))
    for path in topic_indexes:
        for entry in read_json(path):
            local_topics.append(entry["id"])
            if registry.get(entry["id"]) != entry: issues.append(f"{relative(path)}: ENTRY_MISMATCH {entry['id']}")
            if Path(entry["card"]).parent != path.parent.relative_to(BASE):
                issues.append(f"{relative(path)}: WRONG_DOMAIN_CARD {entry['id']}")
    if Counter(local_topics) != Counter(registry.keys()): issues.append("DOMAIN_INDEX_COVERAGE_MISMATCH")
    record("domain_topic_indexes", issues, files=len(topic_indexes), entries=len(local_topics),
           file_sha256={relative(p): sha256(p) for p in topic_indexes})

    issues, lessons, pack_hashes = [], {}, {}
    for name in curriculum["pack_files"]:
        path = BASE / "curriculum" / name
        if not within(path, BASE / "curriculum") or not path.is_file():
            issues.append("MISSING_OR_OUTSIDE_CURRICULUM_PACK " + name); continue
        pack_hashes[relative(path)] = sha256(path)
        for lesson in read_json(path)["objectives"]:
            oid = lesson["objective_id"]
            if oid in lessons: issues.append("DUPLICATE_OBJECTIVE_IN_PACKS " + oid)
            lessons[oid] = lesson
    if len(objective_rows) != EXPECTED["objective_cards"] or len(objective_index) != len(objective_rows):
        issues.append("OBJECTIVE_COUNT_OR_DUPLICATE_ID")
    if set(objective_index) != set(lessons): issues.append("CURRICULUM_INDEX_PACK_IDS_MISMATCH")
    for oid, entry in objective_index.items():
        lesson = lessons.get(oid)
        parent = registry.get(entry["topic_id"])
        if lesson is None or parent is None:
            issues.append(oid + ": MISSING_LESSON_OR_PARENT"); continue
        for field in ("topic_id", "source_refs"):
            if entry[field] != lesson[field]: issues.append(oid + ": CURRICULUM_PACK_MISMATCH_" + field)
        if Counter(entry["profiles"]) != Counter(lesson["profiles"]) or len(set(entry["profiles"])) != len(entry["profiles"]):
            issues.append(oid + ": CURRICULUM_PACK_PROFILE_MEMBERSHIP_MISMATCH")
        if entry["card"] != "curriculum/cards/" + oid + ".md": issues.append(oid + ": NONCANONICAL_CARD_PATH")
        if not set(entry["profiles"]) <= set(parent["profiles"]): issues.append(oid + ": PROFILE_OUTSIDE_PARENT")
        if not set(lesson["domain_ids"]) <= set(parent["domain_ids"]): issues.append(oid + ": DOMAIN_OUTSIDE_PARENT")
    wanted_objective_streams = Counter((e["id"], p) for e in objective_rows for p in e["profiles"])
    observed_objective_streams = Counter((x["objective_id"], x["profile_id"]) for x in curriculum_streams)
    if observed_objective_streams != wanted_objective_streams or len(curriculum_streams) != EXPECTED["objective_profile_streams"]:
        issues.append("CURRICULUM_STREAM_COVERAGE_MISMATCH")
    for n, item in enumerate(curriculum_streams, 1):
        entry = objective_index.get(item["objective_id"])
        if entry is None: continue
        parent = registry[entry["topic_id"]]
        nxt = curriculum_streams[n]["stream_id"] if n < len(curriculum_streams) else None
        wanted = {"order": n, "stream_id": entry["id"] + "/" + item["profile_id"],
                  "topic_id": parent["id"], "domain_id": domain_for(parent, item["profile_id"]), "next_stream_id": nxt}
        for k, v in wanted.items():
            if item.get(k) != v: issues.append(item["stream_id"] + ": CURRICULUM_" + k + "_MISMATCH")
    if curriculum["counts"].get("objectives") != len(objective_rows) or curriculum["counts"].get("streams") != len(curriculum_streams):
        issues.append("CURRICULUM_DECLARED_COUNTS_MISMATCH")
    record("curriculum_index_and_packs", issues, objectives=len(objective_rows), streams=len(curriculum_streams),
           sha256=sha256(curriculum_path), pack_sha256=pack_hashes)

    issues = []
    wanted_parent_streams = [(t["id"], p) for t, p in parent_streams]
    if [(x["topic_id"], x["profile_id"]) for x in queued] != wanted_parent_streams:
        issues.append("PARENT_QUEUE_ORDER_OR_COVERAGE_MISMATCH")
    for n, item in enumerate(queued, 1):
        topic = registry.get(item["topic_id"])
        if topic is None: continue
        wanted = {"sequence": n, "card_order": topic["order"], "card": topic["card"],
                  "domain_id": domain_for(topic, item["profile_id"]),
                  "target_accepted_train_pairs": topic["target_pairs_per_profile"], "training_status": "NOT_RUN"}
        for k, v in wanted.items():
            if item.get(k) != v: issues.append(item["topic_id"] + ": PARENT_QUEUE_" + k + "_MISMATCH")
        check_command(item["command"], {"topic": topic["id"], "profile": item["profile_id"],
                      "objective": None, "count": item["candidate_count"], "split": "train"}, issues,
                      topic["id"] + "/" + item["profile_id"])
    record("parent_generation_queue", issues, streams=len(queued), sha256=sha256(queue_path),
           role="Command catalogue; covered parent streams are not executed again in the effective queue.")

    issues = []
    if len(pilot) != len(curriculum_streams): issues.append("PILOT_QUEUE_COUNT_MISMATCH")
    for n, item in enumerate(pilot, 1):
        if n > len(curriculum_streams): break
        source = curriculum_streams[n - 1]
        entry = objective_index.get(item["objective_id"])
        if entry is None: issues.append("PILOT_UNKNOWN_OBJECTIVE"); continue
        for k in ("order", "stream_id", "objective_id", "topic_id", "profile_id", "domain_id", "next_stream_id"):
            if item.get(k) != source.get(k): issues.append(item["stream_id"] + ": PILOT_" + k + "_MISMATCH")
        if item["card"] != entry["card"]: issues.append(item["stream_id"] + ": PILOT_CARD_MISMATCH")
        ppf = lessons[entry["id"]].get("pairs_per_family", 1)
        if item.get("pairs_per_family") != ppf or item["batch_pairs"] % ppf:
            issues.append(item["stream_id"] + ": PAIR_FAMILY_COUNT_MISMATCH")
        for split in ("train", "dev", "exam"):
            parsed = check_command(item["prepare_first_" + split], {"topic": item["topic_id"], "profile": item["profile_id"],
                "objective": item["objective_id"], "count": item["batch_pairs"], "split": split, "start": 0}, issues, item["stream_id"] + "/" + split)
            if parsed and split == "train":
                expected_dir = "/".join([parsed["work-root"].rstrip("/"), item["topic_id"], item["profile_id"], parsed["run"]])
                if item["first_output_directory"] != expected_dir or item["first_run_id"] != parsed["run"]:
                    issues.append(item["stream_id"] + ": FIRST_OUTPUT_OR_RUN_MISMATCH")
    budget_fields = {"initial_train_batch_pairs": "batch_pairs", "planned_pilot_train_pairs": "planned_train_pairs",
                     "planned_pilot_dev_pairs": "planned_dev_pairs", "planned_pilot_exam_pairs": "planned_exam_pairs"}
    for count_field, stream_field in budget_fields.items():
        if curriculum["counts"].get(count_field) != sum(x[stream_field] for x in pilot):
            issues.append("CURRICULUM_BUDGET_COUNT_MISMATCH " + count_field)
    record("curriculum_pilot_queue", issues, streams=len(pilot), sha256=sha256(pilot_path),
           local_last_next=pilot[-1].get("next_stream_id"), full_queue_continuation="Checked in execution queue and every card.")

    issues = []
    covered = {(x["topic_id"], x["profile_id"]) for x in pilot}
    remaining = [x for x in queued if (x["topic_id"], x["profile_id"]) not in covered]
    desired = []
    for item in pilot:
        desired.append({"kind": "objective", "stream_id": item["stream_id"], "topic_id": item["topic_id"],
            "objective_id": item["objective_id"], "profile_id": item["profile_id"], "domain_id": item["domain_id"],
            "card": item["card"], "candidate_count": item["batch_pairs"], "command": item["prepare_first_train"],
            "first_output_directory": item["first_output_directory"], "budget_pool": item["topic_id"] + "/" + item["profile_id"],
            "source_queue": "curriculum/pilot-queue.json", "source_order": item["order"], "training_status": "NOT_RUN"})
    for item in remaining:
        parsed = parse_prepare_command(item["command"])
        desired.append({"kind": "topic", "stream_id": item["topic_id"] + "/" + item["profile_id"], "topic_id": item["topic_id"],
            "objective_id": None, "profile_id": item["profile_id"], "domain_id": item["domain_id"], "card": item["card"],
            "candidate_count": item["candidate_count"], "command": item["command"],
            "first_output_directory": "/".join([parsed["work-root"].rstrip("/"), item["topic_id"], item["profile_id"], parsed["run"]]),
            "budget_pool": item["topic_id"] + "/" + item["profile_id"], "source_queue": "generation-queue.json",
            "source_order": item["sequence"], "training_status": "NOT_RUN"})
    for n, item in enumerate(desired, 1):
        item.update(order=n, next_stream_id=desired[n]["stream_id"] if n < len(desired) else "END")
    if effective != desired: issues.append("EFFECTIVE_QUEUE_NOT_EXACT_OBJECTIVE_REPLACEMENT_PLUS_REMAINDER")
    if len({x["stream_id"] for x in effective}) != len(effective): issues.append("DUPLICATE_EFFECTIVE_STREAM_ID")
    computed_counts = {"parent_topics": len(topics), "parent_profile_streams": len(parent_streams),
        "objective_cards": len(objective_rows), "objective_profile_streams": len(pilot),
        "covered_parent_profile_streams": len(covered), "remaining_parent_profile_streams": len(remaining),
        "effective_streams": len(effective), "physical_cards": len(topics) + len(objective_rows),
        "active_reference_rows": EXPECTED["active_reference_rows"],
        "target_accepted_train_pairs": sum(t["target_pairs_per_profile"] for t, _ in parent_streams)}
    if computed_counts != EXPECTED or execution.get("counts") != computed_counts:
        issues.append("EFFECTIVE_QUEUE_COUNTS_MISMATCH")
    if index.get("execution_queue", {}).get("counts") != computed_counts:
        issues.append("PARENT_INDEX_EFFECTIVE_COUNTS_MISMATCH")
    human = (BASE / "EXECUTION-QUEUE.md").read_text(encoding="utf-8")
    human_rows = [(int(n), sid, card, int(count), nxt) for n, sid, card, count, nxt in re.findall(
        r"^\|\s*(\d+)\s*\|\s*`([^`]+)`\s*\|\s*\[[^\]]+\]\(([^)]+)\)\s*\|\s*(\d+)\s*\|\s*`([^`]+)`\s*\|", human, re.M)]
    if human_rows != [(x["order"], x["stream_id"], x["card"], x["candidate_count"], x["next_stream_id"]) for x in effective]:
        issues.append("HUMAN_EXECUTION_QUEUE_MISMATCH")
    record("effective_execution_queue", issues, **computed_counts, sha256=sha256(execution_path),
           first_stream=effective[0]["stream_id"], last_stream=effective[-1]["stream_id"],
           objective_to_parent_boundary={"from": effective[len(pilot)-1]["stream_id"], "to": effective[len(pilot)]["stream_id"]})

    issues = []
    pool_targets = {t["id"] + "/" + profile: t["target_pairs_per_profile"] for t, profile in parent_streams}
    observed_pools = {x["budget_pool"] for x in effective}
    if observed_pools != set(pool_targets): issues.append("BUDGET_POOL_COVERAGE_MISMATCH")
    pilot_by_pool = Counter()
    for x in pilot: pilot_by_pool[x["topic_id"] + "/" + x["profile_id"]] += x["planned_train_pairs"]
    for pool, total in pilot_by_pool.items():
        if total > pool_targets.get(pool, 0): issues.append(pool + ": PILOT_EXCEEDS_PARENT_TRAIN_BUDGET")
    target = sum(pool_targets.values())
    if target != EXPECTED["target_accepted_train_pairs"] or index["counts"].get("target_accepted_pairs") != target:
        issues.append("PARENT_TRAIN_TARGET_MISMATCH")
    if execution["counts"].get("target_accepted_train_pairs") != target: issues.append("DOUBLE_OR_CHANGED_EXECUTION_BUDGET")
    record("shared_budget_no_double_count", issues, parent_budget_pools=len(pool_targets), effective_budget_pools=len(observed_pools),
           accepted_train_target=target, objective_pilot_train_subset=sum(pilot_by_pool.values()),
           objective_train_by_parent_pool=dict(pilot_by_pool),
           rule="Sum each parent/profile budget once; objective pilot counts are subsets, not additional corpus targets.")

    issues, card_paths, cards_digest, navigation_details = [], [], {}, []
    for entry, is_objective in [(t, False) for t in topics] + [(e, True) for e in objective_rows]:
        sid = entry["id"]
        path = (BASE / entry["card"]).resolve()
        if not within(path, BASE) or not path.is_file():
            issues.append(sid + ": MISSING_OR_OUTSIDE_CARD"); continue
        card_paths.append(path); cards_digest[sid] = sha256(path)
        text = path.read_text(encoding="utf-8")
        if not is_objective:
            matches = re.findall(r"Порядок:\s*\*\*(\d+)\*\*\.\s*Устойчивый ID:\s*`([^`]+)`\.\s*Следующий ID:\s*`([^`]+)`", text)
            expected_next = entry.get("next_id") or "нет; следующее задание определяет пользователь"
            if matches != [(str(entry["order"]), sid, expected_next)]: issues.append(sid + ": PARENT_CATALOG_NAVIGATION_MISMATCH")
            old_priorities = re.findall(r"Приоритет\s+`(\d+)`", text)
            if any(int(x) != entry["priority"] for x in old_priorities): issues.append(sid + ": STALE_CARD_PRIORITY")
        for marker in ("CODEX", "astra 6 ultracode", "Kiro", "STOP", "NEXT", "10-CODEX-AND-CHINESE-ROLES.md", "EXECUTION-QUEUE.md"):
            if marker not in text: issues.append(sid + ": MISSING_REQUIRED_MARKER " + marker)
        if "китайск" not in text.lower(): issues.append(sid + ": CHINESE_GENERATOR_ROLE_MISSING")
        wanted = [(x["stream_id"], x["order"], x["next_stream_id"]) for x in effective
                  if (x["objective_id"] == sid if is_objective else x["topic_id"] == sid)]
        try:
            actual = transition_rows(text)
            if actual != wanted: issues.append(sid + ": FULL_QUEUE_NEXT_TABLE_MISMATCH")
        except ValueError as exc:
            actual = []; issues.append(sid + ": " + str(exc))
        navigation_details.append({"card_id": sid, "kind": "objective" if is_objective else "parent", "streams": len(actual),
                                   "transitions": [{"stream_id": a, "order": b, "next_stream_id": c} for a, b, c in actual]})
    if len(card_paths) != EXPECTED["physical_cards"] or len(set(card_paths)) != len(card_paths):
        issues.append("PHYSICAL_CARD_COUNT_OR_DUPLICATED_PATH")
    record("all_cards_roles_stop_next_and_exact_queue", issues, cards_checked=len(card_paths), card_sha256=cards_digest,
           navigation=navigation_details, scope="Explicit contract markers and exact canonical tables; not model-based semantic admission.")

    lock_path = BASE / "sources.lock.json"
    manifest_path = rag_root / "rag/manifest.json"
    lock_doc, manifest_doc = read_json(lock_path), read_json(manifest_path)
    locked, manifest = lock_doc["sources"], manifest_doc["sources"]
    locks, manifests = {s["source_id"]: s for s in locked}, {s["source_id"]: s for s in manifest}
    issues, source_details = [], []
    if len(locked) != EXPECTED_SOURCES or len(manifest) != EXPECTED_SOURCES: issues.append("SOURCE_COUNT_MISMATCH")
    if len(locks) != len(locked) or len(manifests) != len(manifest): issues.append("DUPLICATE_SOURCE_ID")
    if set(locks) != set(manifests): issues.append("LOCK_MANIFEST_IDS_MISMATCH")
    for key in ("source_pins", "additional_source_pins"):
        if lock_doc.get(key) != manifest_doc.get(key): issues.append("LOCK_MANIFEST_PIN_MISMATCH " + key)
    for sid, locked_source in locks.items():
        source = manifests.get(sid)
        if source is None: continue
        for field, value in locked_source.items():
            if source.get(field) != value: issues.append(sid + ": LOCK_MANIFEST_MISMATCH " + field)
        if source.get("branch") not in ("main", "llm_plan3"): issues.append(sid + ": SOURCE_BRANCH_NOT_ALLOWED")
        for field, length in (("commit", 40), ("blob_sha", 40), ("source_content_sha256", 64)):
            if not re.fullmatch(r"[0-9a-f]{" + str(length) + "}", source.get(field, "")): issues.append(sid + ": INVALID_" + field)
        if not source["source_url"].startswith("https://github.com/lubochka/xiigen-planning/blob/" + source["commit"] + "/"):
            issues.append(sid + ": SOURCE_URL_OUTSIDE_PINNED_REPOSITORY")
        slice_path = rag_root / "rag" / source["local_excerpt_path"]
        canonical = rag_root / "rag/sources" / (sid + ".md")
        if not within(slice_path, rag_root) or slice_path.resolve() != canonical.resolve() or not slice_path.is_file():
            issues.append(sid + ": SLICE_PATH_MISMATCH_OR_MISSING"); continue
        digest = sha256(slice_path)
        if digest != locked_source.get("local_excerpt_sha256"): issues.append(sid + ": SLICE_SHA256_MISMATCH")
        for span in source["reviewed_line_spans"]:
            if not 1 <= span["start"] <= span["end"] <= source["source_line_count"]: issues.append(sid + ": INVALID_SPAN")
        source_details.append({"source_id": sid, "branch": source["branch"], "commit": source["commit"],
                               "blob_sha": source["blob_sha"], "slice_sha256": digest, "local_excerpt_path": source["local_excerpt_path"]})
    for entry in topics + objective_rows:
        for ref in entry["source_refs"]:
            if m.source_id(ref) not in manifests: issues.append(entry["id"] + ": UNRESOLVED_SOURCE " + ref)
    record("rag_lock_manifest_and_slices", issues, lock_sha256=sha256(lock_path), manifest_sha256=sha256(manifest_path),
           source_count=len(locks), distinct_source_paths=len({x["path"] for x in manifest}), verified_slices=len(source_details),
           sources=source_details, full_original_blobs_recomputed=False,
           scope="All locked fields and local slice bytes; full original source blobs are not reconstructed from excerpts.")

    reference_files = sorted(BASE.glob("domains/*/reference-examples*.jsonl")) + [BASE / "curriculum/reference-examples.jsonl"]
    reference_rows, inventory = [], []
    for path in reference_files:
        rows = m.rows(path); reference_rows.extend(rows)
        inventory.append({"path": relative(path), "rows": len(rows), "sha256": sha256(path)})
    issues = m.validate_rows(reference_rows, allow_reference=True)
    if len(reference_rows) != EXPECTED["active_reference_rows"]: issues.append("ACTIVE_REFERENCE_COUNT_MISMATCH")
    if any(r.get("split") != "reference" for r in reference_rows): issues.append("NON_REFERENCE_ROW_IN_REFERENCE_FILES")
    if execution["counts"].get("active_reference_rows") != len(reference_rows): issues.append("EXECUTION_REFERENCE_COUNT_MISMATCH")
    curriculum_rows = m.rows(BASE / "curriculum/reference-examples.jsonl")
    if len(curriculum_rows) != curriculum["counts"].get("reference_examples"): issues.append("CURRICULUM_REFERENCE_COUNT_MISMATCH")
    projected = {}
    for r in curriculum_rows:
        key = (r["metadata"].get("learning_objective_id"), r["profile_id"], r["family_id"], r["prompt"])
        if key in projected: issues.append("DUPLICATED_CURRICULUM_EXAMPLE_PROJECTION")
        projected[key] = r
    example_keys = set()
    for oid, lesson in lessons.items():
        for example in lesson["examples"]:
            key = (oid, example["profile_id"], example["family_id"], example["prompt"])
            example_keys.add(key)
            r = projected.get(key)
            if r is None: issues.append(oid + ": MISSING_FLATTENED_REFERENCE"); continue
            for field in ("chosen", "rejected", "error_tag", "rationale"):
                if r.get(field) != example.get(field): issues.append(oid + ": FLATTENED_REFERENCE_MISMATCH " + field)
    if example_keys != set(projected): issues.append("CURRICULUM_FLAT_PACK_REFERENCE_COVERAGE_MISMATCH")
    exclusion = Counter("explicit_false" if r.get("metadata", {}).get("training_allowed") is False else
                        "explicit_true" if r.get("metadata", {}).get("training_allowed") is True else "omitted_or_other" for r in reference_rows)
    record("all_active_reference_structure", issues, rows=len(reference_rows), files=inventory,
           validator="sets.validate_rows(all_728_active_reference_rows, allow_reference=True)",
           training_allowed_metadata_counts=dict(exclusion),
           exclusion_basis="split=reference; training_allowed is optional metadata.", semantic_review="NOT_PERFORMED", training="NOT_RUN")

    issues, quarantine, q_inventory = [], [], []
    for path in sorted(BASE.glob("quarantine/*.jsonl")):
        rows = m.rows(path); quarantine.extend(rows)
        q_inventory.append({"path": relative(path), "rows": len(rows), "sha256": sha256(path)})
    active_by_id = {r["pair_id"]: r for r in reference_rows}
    q_by_id = {r["pair_id"]: r for r in quarantine}
    if set(q_by_id) != EXPECTED_QUARANTINE_IDS or len(quarantine) != len(EXPECTED_QUARANTINE_IDS):
        issues.append("QUARANTINE_ID_COUNT_MISMATCH")
    if set(active_by_id) & set(q_by_id): issues.append("QUARANTINED_ROWS_STILL_ACTIVE")
    core, locations = [], {}
    for pair_id in BASELINE_REFERENCE_IDS:
        row = active_by_id.get(pair_id, q_by_id.get(pair_id))
        if row is None: issues.append("MISSING_BASELINE_REFERENCE " + pair_id); continue
        locations[pair_id] = "active" if pair_id in active_by_id else "quarantine"
        core.append({k: row[k] for k in BASELINE_REFERENCE_FIELDS})
    digest = canonical_hash(core)
    if digest != BASELINE_REFERENCE_CORE_SHA256: issues.append("BASELINE_REFERENCE_CORE_CHANGED")
    record("baseline_185_rows_and_quarantine_preserved", issues, baseline_rows=len(BASELINE_REFERENCE_IDS),
           location_counts=dict(Counter(locations.values())), locations=locations, quarantined_rows=len(quarantine),
           quarantine_files=q_inventory, original_file_sha256=BASELINE_REFERENCE_FILE_SHA256,
           preserved_core_sha256=digest, expected_core_sha256=BASELINE_REFERENCE_CORE_SHA256,
           preserved_fields=BASELINE_REFERENCE_FIELDS,
           scope="Metadata additions and explicit quarantine placement are allowed; learner text, IDs and original rationale/provenance are preserved.")

    # Include all current prose/prompts/cards, not just the original landing pages.
    links_paths = [p for p in BASE.rglob("*.md") if "verification" not in p.relative_to(BASE).parts]
    links_paths += [p for p in rag_root.rglob("*.md") if "sources" not in p.relative_to(rag_root).parts]
    links = check_links(links_paths, rag_root, {source_url(x["source_url"]) for x in locked}, getattr(args, "out", None))
    record("current_docs_cards_and_indexes_links", links.pop("errors"), **links,
           historical_verification_reports_excluded=True)

    if args.prepare_check:
        outcomes, issues = [], []
        with tempfile.TemporaryDirectory(prefix="xiigen-merged-prepare-") as temporary:
            work_root = Path(temporary)
            for n, item in enumerate(effective, 1):
                label = item["stream_id"]
                item_errors, attempts = [], []
                objective = item.get("objective_id")
                lesson = lessons[objective] if objective else None
                ppf = lesson.get("pairs_per_family", 1) if lesson else 1
                declared = parse_prepare_command(item["command"])
                expected_count1_rejection = ppf > 1 or objective == "OR-ROUTE-INTENT"
                count = 1
                run_id = f"VERIFY{n:04d}"
                run_path = work_root / item["topic_id"] / item["profile_id"] / run_id

                def invoke(number):
                    command = [sys.executable, "-B", str(BASE / "tools/sets.py"), "prepare", "--topic", item["topic_id"],
                               "--profile", item["profile_id"], "--run", run_id, "--count", str(number),
                               "--split", "train", "--start", "0", "--work-root", str(work_root), "--rag-root", str(rag_root)]
                    if objective: command.extend(["--objective", objective])
                    result = subprocess.run(command, cwd=BASE, capture_output=True, text=True, encoding="utf-8", timeout=55)
                    attempts.append({"count": number, "exit_code": result.returncode, "stdout": result.stdout.strip(), "stderr": result.stderr.strip()})
                    return result

                try:
                    result = invoke(1)
                    if expected_count1_rejection:
                        if result.returncode == 0 or run_path.exists():
                            item_errors.append("INVALID_COUNT1_ACCEPTED_OR_LEFT_RUN_ARTIFACTS")
                        else:
                            diagnostic = result.stdout + result.stderr
                            known = "count must be divisible by " + str(ppf) if ppf > 1 else "OR-ROUTE-INTENT requires count 21 or 42"
                            if known not in diagnostic: item_errors.append("COUNT1_REJECTED_FOR_UNEXPECTED_REASON")
                            else:
                                count = declared["count"]
                                if count != item["candidate_count"]: item_errors.append("FALLBACK_NOT_DECLARED_FIRST_COUNT")
                                result = invoke(count)
                    if result.returncode != 0: item_errors.append("PREPARE_CLI_FAILED")
                    if not item_errors:
                        request = read_json(run_path / "request.json")
                        selection = read_json(run_path / "selection.template.json")
                        state = read_json(run_path / "state.json")
                        prompt = (run_path / "prompt.md").read_text(encoding="utf-8")
                        topic = registry[item["topic_id"]]
                        refs = lesson["source_refs"] if lesson else topic["source_refs"]
                        expected_files = {"request.json", "prompt.md", "selection.template.json", "selection.prompt.md", "state.json"}
                        if {p.name for p in run_path.iterdir()} != expected_files: item_errors.append("PREPARE_FILE_SET_MISMATCH")
                        expected = {"schema_version": "xiigen.request.v2" if lesson else "xiigen.request.v1",
                            "count": count, "topic_id": item["topic_id"], "profile_id": item["profile_id"], "domain_id": item["domain_id"],
                            "split": "train", "run_id": run_id, "card": item["card"], "card_sha256": sha256(BASE / item["card"]),
                            "sources": refs, "rag_manifest_sha256": sha256(manifest_path), "output_directory": str(run_path),
                            "scope": "DATA_PREPARATION_ONLY", "training_status": "NOT_RUN"}
                        for field, value in expected.items():
                            if request.get(field) != value: item_errors.append("REQUEST_MISMATCH_" + field)
                        if request.get("objective_id") != objective: item_errors.append("REQUEST_OBJECTIVE_MISMATCH")
                        expected_families = count // ppf
                        if len(request.get("family_ids", [])) != expected_families or len(set(request.get("family_ids", []))) != expected_families:
                            item_errors.append("FAMILY_COUNT_OR_UNIQUENESS_MISMATCH")
                        if any(m.family_split(f) != "train" for f in request.get("family_ids", [])):
                            item_errors.append("FAMILY_NOT_ASSIGNED_TO_TRAIN")
                        if set(request.get("family_contracts", {})) != set(request.get("family_ids", [])):
                            item_errors.append("FAMILY_CONTRACT_COVERAGE_MISMATCH")
                        if lesson:
                            for field, value in {"pairs_per_family": ppf, "review_requirements": lesson["review_requirements"],
                                                 "public_reference_families": [r["family_id"] for r in lesson["examples"]]}.items():
                                if request.get(field) != value: item_errors.append("OBJECTIVE_REQUEST_MISMATCH_" + field)
                        if objective == "OR-ROUTE-INTENT":
                            labels = lesson["scenario_axes"][0]["values"]
                            if request.get("route_class_counts") != {label: count // len(labels) for label in labels}:
                                item_errors.append("ROUTE_CLASS_COVERAGE_MISMATCH")
                        for field in ("selection_authority", "quality_authority"):
                            if request.get(field) != {"actor": "CODEX", "required_profile": m.CODEX_PROFILE}:
                                item_errors.append("AUTHORITY_CONTRACT_MISMATCH_" + field)
                        if (selection.get("decision"), selection.get("observed_profile"), selection.get("profile_verified")) != ("PENDING", None, False):
                            item_errors.append("SELECTION_TEMPLATE_NOT_PENDING")
                        if (state.get("status"), state.get("process_handle"), state.get("training_status")) != ("PREPARED", None, "NOT_RUN"):
                            item_errors.append("PREPARE_STATE_MISMATCH")
                        embedded = [BASE / item["card"], BASE / "prompts/MASTER-PROMPT-RU.md"]
                        embedded += [BASE / name for name in ("02-DATA-CONTRACT.md", "07-CONFLICTS-AND-REQUIREMENTS.md", "10-CODEX-AND-CHINESE-ROLES.md")]
                        embedded += [rag_root / "rag/sources" / (m.source_id(ref) + ".md") for ref in refs]
                        if any(path.read_text(encoding="utf-8") not in prompt for path in embedded):
                            item_errors.append("MISSING_EMBEDDED_CONTRACT_CARD_OR_SOURCE")
                        artifacts = {p.name: sha256(p) for p in sorted(run_path.iterdir()) if p.is_file()}
                        outcomes.append({"stream_id": label, "topic_id": item["topic_id"], "objective_id": objective,
                            "profile_id": item["profile_id"], "requested_probe_count": 1, "effective_count": count,
                            "declared_first_count": declared["count"], "count1_expected_rejection": expected_count1_rejection,
                            "fallback_rule": "Use declared first command count after documented COUNT1 rejection" if expected_count1_rejection else None,
                            "family_count": len(request["family_ids"]), "passed": not item_errors, "prompt_chars": len(prompt),
                            "artifact_sha256": artifacts, "attempts": attempts, "errors": item_errors})
                    else:
                        outcomes.append({"stream_id": label, "passed": False, "effective_count": count, "attempts": attempts, "errors": item_errors})
                except Exception as exc:
                    item_errors.append(type(exc).__name__ + ": " + str(exc))
                    outcomes.append({"stream_id": label, "passed": False, "effective_count": count, "attempts": attempts, "errors": item_errors})
                issues.extend(label + ": " + issue for issue in item_errors)
                if n % 20 == 0 or n == len(effective):
                    print(json.dumps({"verification_progress": n, "streams": len(effective), "failed_streams_so_far": sum(not x["passed"] for x in outcomes)}, ensure_ascii=False), flush=True)
        record("prepare_all_effective_streams", issues, requested=len(effective), completed=len(outcomes),
               passed_streams=sum(x["passed"] for x in outcomes), declared_count_fallbacks=sum(x.get("count1_expected_rejection", False) for x in outcomes),
               requests=outcomes, invocation="Python CLI, subprocess.run waits for each terminal result; no shell command execution.",
               temporary_outputs_retained=False, candidates_generated=0, models_called=0, training_runs=0)
    else:
        checks["prepare_all_effective_streams"] = {"status": "NOT_REQUESTED", "instruction": "Use --prepare-check after the integration owner marks the package READY."}

    changed = [str(p) for p in input_paths if not p.is_file() or sha256(p) != inputs_before[str(p)]]
    record("input_snapshot_stable_during_verification", ["INPUT_CHANGED " + p for p in changed], files_checked=len(inputs_before),
           files_sha256={(("rag-root/" + relative(Path(p), rag_root)) if within(Path(p), rag_root) else relative(Path(p))): digest
                         for p, digest in inputs_before.items()},
           historical_report_hashes_not_tested_as_current=True)
    return {"schema_version": "xiigen.preparation.merged-package-check.v1",
            "checked_at_utc": datetime.now(timezone.utc).isoformat(), "scope": "DATA_PREPARATION_ONLY_NOT_EXPERIMENTAL_LEARNING",
            "expected_snapshot": EXPECTED, "tool_sha256": sha256(BASE / "tools/sets.py"), "verifier_sha256": sha256(Path(__file__)),
            "index_sha256": sha256(index_path), "real_codex_calls": 0, "real_chinese_model_calls": 0,
            "training_runs": 0, "provider_identity_authenticated": False, "checks": checks, "errors": errors, "all_passed": not errors,
            "limits": ["No semantic acceptance, generator output or learning measurement is asserted.",
                       "COUNT1 is a request-construction probe; paired conditional and complete routing batches use their declared first count.",
                       "External URLs outside the pinned catalog are syntax-checked, not fetched.",
                       "Historical verification report hashes are retained evidence, not tested as current package hashes.",
                       "RAG source-body links and full source blob reconstruction are outside this bounded check."]}


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--rag-root", required=True, help="main-branch preparation directory containing rag/manifest.json")
    parser.add_argument("--out", required=True, help="New JSON report path; an existing file is never overwritten")
    parser.add_argument("--prepare-check", action="store_true", help="Exercise all effective CLI prepare paths; use declared first count after expected COUNT1 rejection")
    args = parser.parse_args()
    output = Path(args.out).expanduser().resolve()
    if output.exists(): parser.error(f"Report already exists; use a new --out path: {output}")
    try:
        result = run(args)
    except Exception as exc:
        result = {"schema_version": "xiigen.preparation.package-check.v1",
                  "scope": "DATA_PREPARATION_ONLY_NOT_EXPERIMENTAL_LEARNING",
                  "all_passed": False, "errors": [type(exc).__name__ + ": " + str(exc)],
                  "real_codex_calls": 0, "real_chinese_model_calls": 0, "training_runs": 0}
    output.parent.mkdir(parents=True, exist_ok=True)
    with output.open("x", encoding="utf-8", newline="\n") as stream:
        json.dump(result, stream, ensure_ascii=False, indent=2)
        stream.write("\n")
    print(json.dumps({"all_passed": result["all_passed"], "errors": result["errors"], "report": str(output),
                      "scope": result["scope"]}, ensure_ascii=False, indent=2))
    return 0 if result["all_passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
