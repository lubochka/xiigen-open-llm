#!/usr/bin/env python3
"""
Build SessionLearningRow JSONL from license-clean architecture sources.

Unlike instruction-chat prompts, these rows are grounded in architecture source
material: system design trade-offs, patterns, reliability, decomposition, and
planning guidance. The text is shaped as an architecture/planning task so the
existing llm_mvp_core runner can test whether these sources improve ARCH/PLAN
orchestration without pulling in generic code-question noise.
"""

from __future__ import annotations

import argparse
import hashlib
import html
import json
import re
import ssl
import sys
import time
import urllib.parse
import urllib.request
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Dict, Iterable, Iterator, List, Tuple


MOJIBAKE_MARKERS = ("\u00c3", "\u00c2", "\u00e2\u20ac", "\u00c3\u00a2")

AOSA_INDEX = "https://aosabook.org/en/"

# These are intentionally documented in code because the builder is a gate, not
# just a row formatter. If a future source adapter is added, it should be put in
# one of these buckets before it can emit rows.
BLOCKED_SOURCE_CLASSES = {
    "mit-ocw": "CC BY-NC-SA blocks commercial training use.",
    "stack-exchange-dump": "Download terms are disputed for LLM training; block practically.",
}

QUARANTINE_SOURCE_CLASSES = {
    "wikipedia": "CC BY-SA share-alike source; only isolated quarantine batches.",
}

LOCAL_SOURCES = {
    "system-design-primer": {
        "license": "CC-BY-4.0",
        "path": "external sources learning 2/architecture-source-cache/system-design-primer/README.md",
        "source_url": "https://github.com/donnemartin/system-design-primer",
    },
    "mehdihadeli-awesome-architecture": {
        "license": "CC0-1.0",
        "path": "external sources learning 2/architecture-source-cache/mehdihadeli-awesome-software-architecture",
        "source_url": "https://github.com/mehdihadeli/awesome-software-architecture",
    },
    "dovamir-awesome-patterns": {
        "license": "CC0-1.0",
        "path": "external sources learning 2/architecture-source-cache/DovAmir-awesome-design-patterns/README.md",
        "source_url": "https://github.com/DovAmir/awesome-design-patterns",
    },
    "simskij-awesome-architecture": {
        "license": "CC0-1.0",
        "path": "external sources learning 2/architecture-source-cache/simskij-awesome-software-architecture/readme.md",
        "source_url": "https://github.com/simskij/awesome-software-architecture",
    },
}

SOURCE_ALIASES = {
    "aosa": "aosa",
    **{key: key for key in LOCAL_SOURCES},
}


@dataclass(frozen=True)
class SubPhase:
    id: str
    phase_id: str
    name: str
    task_type: str
    predicates: Tuple[str, ...]


SUB_PHASES: Dict[str, SubPhase] = {
    "2.1": SubPhase("2.1", "2", "Business Capability Definition", "business-capability", ("DOCUMENT",)),
    "2.2": SubPhase("2.2", "2", "User Stories and Acceptance Criteria", "acceptance-criteria", ("DOCUMENT", "QA")),
    "2.3": SubPhase("2.3", "2", "Data and Validation Rules", "data-validation", ("API_CONTRACT", "DOCUMENT")),
    "2.4": SubPhase("2.4", "2", "Edge Cases and Boundary Conditions", "edge-cases", ("DOCUMENT",)),
    "3.1": SubPhase("3.1", "3", "Architecture and Component Topology", "architecture-topology", ("ARCHITECTURE",)),
    "3.2": SubPhase("3.2", "3", "Data Model and Persistence Design", "data-persistence", ("ARCHITECTURE",)),
    "3.3": SubPhase("3.3", "3", "API Contract Definition", "api-contract", ("API_CONTRACT",)),
    "3.4": SubPhase("3.4", "3", "Security and Authorization Model", "security-verification", ("SECURITY", "AUTHORIZATION")),
    "3.5": SubPhase("3.5", "3", "Observability and Operational Design", "observability-design", ("ARCHITECTURE",)),
    "4.5": SubPhase("4.5", "4", "Integration and Provider Connectors", "provider-connectors", ("IMPLEMENT", "API_CONTRACT")),
    "5.2": SubPhase("5.2", "5", "Integration Test Design", "integration-tests", ("TEST", "API_CONTRACT")),
    "5.3": SubPhase("5.3", "5", "End-to-End Scenario Testing", "end-to-end-tests", ("TEST", "QA")),
    "5.4": SubPhase("5.4", "5", "Regression and Compatibility Testing", "regression-compatibility", ("TEST",)),
    "6.2": SubPhase("6.2", "6", "Release Readiness and Risk Review", "release-risk", ("QA",)),
    "6.4": SubPhase("6.4", "6", "Performance and Reliability Review", "performance-reliability", ("QA",)),
    # Current DevelopmentPromptCompiler emits both AUTOMATION and LEARNING.
    "7.1": SubPhase("7.1", "7", "Automated Test Harness", "automation-harness", ("TEST", "AUTOMATION")),
    "7.2": SubPhase("7.2", "7", "CI Pipeline and Quality Gates", "ci-quality-gates", ("AUTOMATION",)),
    "7.3": SubPhase("7.3", "7", "Monitoring and Feedback Loop", "monitoring-feedback", ("AUTOMATION",)),
    "7.4": SubPhase("7.4", "7", "Coverage and Reporting", "coverage-reporting", ("TEST", "DOCUMENT")),
    "7.5": SubPhase("7.5", "7", "Learning Set and Model Feedback", "learning-feedback", ("LEARNING",)),
}


TOPIC_RULES: List[Tuple[str, re.Pattern[str], Tuple[str, ...], str]] = [
    ("security", re.compile(r"\b(security|auth|permission|privacy|threat|attack|encrypt|trust)\b", re.I), ("3.4", "6.3", "2.3"), "ARCH"),
    ("api-contract", re.compile(r"\b(api|interface|contract|schema|request|response|protocol|rpc|rest)\b", re.I), ("3.3", "2.3", "5.2"), "ARCH"),
    ("data", re.compile(r"\b(database|storage|data model|cache|consistency|replication|partition|index|persistence|transaction)\b", re.I), ("3.2", "2.3", "6.4"), "ARCH"),
    ("reliability", re.compile(r"\b(reliability|availability|latency|throughput|load|performance|failover|fault|resilience)\b", re.I), ("6.4", "5.4", "7.3"), "ARCH"),
    ("observability", re.compile(r"\b(logging|metrics|monitoring|trace|observability|feedback|dashboard|alert)\b", re.I), ("3.5", "7.3", "7.4"), "ARCH"),
    ("integration", re.compile(r"\b(integration|service|connector|queue|message|event|distributed|microservice|component)\b", re.I), ("3.1", "4.5", "5.2"), "ARCH"),
    ("test-gate", re.compile(r"\b(test|testing|validation|verify|quality gate|regression|coverage)\b", re.I), ("5.2", "5.3", "7.1", "7.4"), "PLAN"),
    ("learning", re.compile(r"\b(learn|learning|training|feedback|knowledge|model|skill|guidance)\b", re.I), ("7.5", "7.3", "2.1"), "PLAN"),
    ("planning", re.compile(r"\b(plan|trade-off|tradeoff|decision|requirement|constraint|decomposition|design)\b", re.I), ("2.1", "2.2", "2.4", "3.1"), "PLAN"),
]


def sha1_short(value: str) -> str:
    return hashlib.sha1(value.encode("utf-8", errors="replace")).hexdigest()[:12]


def has_mojibake(text: str) -> bool:
    return any(marker in text for marker in MOJIBAKE_MARKERS)


def clean_markdown(text: str) -> str:
    text = re.sub(r"```.*?```", " ", text, flags=re.S)
    text = re.sub(r"!\[[^\]]*\]\([^)]*\)", " ", text)
    text = re.sub(r"\[([^\]]+)\]\([^)]*\)", r"\1", text)
    text = re.sub(r"<[^>]+>", " ", text)
    text = re.sub(r"`([^`]+)`", r"\1", text)
    text = text.replace("\u00a0", " ")
    return text


def markdown_blocks(path: Path) -> Iterator[Tuple[str, str]]:
    raw = path.read_text(encoding="utf-8", errors="replace")
    raw = clean_markdown(raw)
    current_heading = path.stem
    buffer: List[str] = []
    for line in raw.splitlines():
        stripped = line.strip()
        if not stripped:
            if buffer:
                yield current_heading, " ".join(buffer)
                buffer = []
            continue
        heading = re.match(r"^#{1,4}\s+(.+)$", stripped)
        if heading:
            if buffer:
                yield current_heading, " ".join(buffer)
                buffer = []
            current_heading = heading.group(1).strip()
            continue
        if stripped.startswith("|") and stripped.endswith("|"):
            continue
        stripped = re.sub(r"^[-*+]\s+", "", stripped)
        stripped = re.sub(r"^\d+\.\s+", "", stripped)
        if stripped:
            buffer.append(stripped)
    if buffer:
        yield current_heading, " ".join(buffer)


def fetch_text(url: str, allow_insecure_tls: bool) -> str:
    context = ssl._create_unverified_context() if allow_insecure_tls else None
    request = urllib.request.Request(url, headers={"User-Agent": "xiigen-architecture-source-builder/1.0"})
    with urllib.request.urlopen(request, timeout=90, context=context) as response:
        return response.read().decode("utf-8", errors="replace")


def html_to_text(value: str) -> str:
    value = re.sub(r"(?is)<script.*?</script>|<style.*?</style>|<nav.*?</nav>|<footer.*?</footer>", " ", value)
    value = re.sub(r"(?i)</(p|li|h1|h2|h3|h4|blockquote)>", "\n\n", value)
    value = re.sub(r"(?s)<[^>]+>", " ", value)
    value = html.unescape(value)
    return re.sub(r"[ \t]+", " ", value)


def aosa_urls(allow_insecure_tls: bool, limit_pages: int) -> List[str]:
    index = fetch_text(AOSA_INDEX, allow_insecure_tls)
    urls: List[str] = []
    for match in re.finditer(r'href="([^"]+\.html)"', index):
        href = match.group(1)
        if "license" in href or "faq" in href or "intro" in href:
            continue
        if not (href.startswith("v1/") or href.startswith("v2/") or href.startswith("v3/") or href.startswith("v4/")):
            continue
        urls.append(urllib.parse.urljoin(AOSA_INDEX, href))
    return list(dict.fromkeys(urls))[:limit_pages]


def aosa_blocks(allow_insecure_tls: bool, limit_pages: int) -> Iterator[Tuple[str, str, str]]:
    for url in aosa_urls(allow_insecure_tls, limit_pages):
        try:
            text = html_to_text(fetch_text(url, allow_insecure_tls))
        except Exception as ex:
            print(f"warning: failed to fetch {url}: {ex}", file=sys.stderr)
            continue
        parts = [part.strip() for part in re.split(r"\n\s*\n", text) if part.strip()]
        title = parts[0][:120] if parts else url
        for part in parts[1:]:
            yield title, part, url
        time.sleep(0.1)


def source_blocks(source_key: str, allow_insecure_tls: bool, aosa_page_limit: int) -> Iterator[Tuple[str, str, str]]:
    if source_key == "aosa":
        yield from aosa_blocks(allow_insecure_tls, aosa_page_limit)
        return

    source = LOCAL_SOURCES[source_key]
    root = Path(source["path"])
    if root.is_file():
        paths = [root]
    else:
        paths = [p for p in root.rglob("*.md") if ".git" not in p.parts]
    for path in paths:
        for heading, block in markdown_blocks(path):
            if re.search(r"\b(resources?|articles?|videos?|books?|courses?|read more)\b", heading, re.I):
                continue
            yield heading, block, str(path)


def is_good_block(text: str) -> bool:
    words = text.split()
    if len(words) < 24 or len(words) > 180:
        return False
    if has_mojibake(text):
        return False
    if re.search(
        r"\b(sponsor|badge|contributing|pull request|star this|newsletter|"
        r"curated list|awesome articles|awesome list|repository to share|"
        r"table of contents|contents software architecture)\b",
        text,
        re.I,
    ):
        return False
    return any(rule.search(text) for _, rule, _, _ in TOPIC_RULES)


def choose_targets(text: str) -> Tuple[str, List[SubPhase], List[str], List[str]]:
    ids: List[str] = []
    domains: List[str] = []
    evidence: List[str] = []
    for topic, pattern, sub_phase_ids, domain in TOPIC_RULES:
        if pattern.search(text):
            ids.extend(sub_phase_ids)
            domains.append(domain)
            evidence.append(f"{topic}:matched")
    if not ids:
        ids = ["2.1", "3.1"]
        domains = ["PLAN"]
        evidence = ["fallback:architecture-planning"]
    unique_ids: List[str] = []
    for item in ids:
        if item not in unique_ids and item in SUB_PHASES:
            unique_ids.append(item)
    selected_ids = unique_ids[:1]
    domain = "ARCH" if domains.count("ARCH") >= domains.count("PLAN") else "PLAN"
    return domain, [SUB_PHASES[item] for item in selected_ids], evidence, sorted({predicate for sub in selected_ids for predicate in SUB_PHASES[sub].predicates})


def sanitize_prompt_excerpt(value: str) -> str:
    replacements = {
        r"\bcode\b": "delivery artifact",
        r"\bcodebase\b": "system surface",
        r"\bbug(s)?\b": "defect risk",
        r"\berror(s)?\b": "operational risk",
        r"\bfailure(s)?\b": "reliability risk",
        r"\bfailed\b": "did not satisfy the gate",
        r"\bfix(es|ed|ing)?\b": "remediate",
        r"\brefactor(ing)?\b": "reshape boundaries",
        r"\bnull\b": "empty value",
        r"\blog(s|ging)?\b": "telemetry",
    }
    text = value
    for pattern, replacement in replacements.items():
        text = re.sub(pattern, replacement, text, flags=re.I)
    return text


def make_user_request(source_key: str, heading: str, block: str) -> str:
    excerpt = re.sub(r"\s+", " ", block).strip()
    excerpt = sanitize_prompt_excerpt(excerpt)
    if len(excerpt) > 900:
        excerpt = excerpt[:900].rsplit(" ", 1)[0] + "."
    return (
        "Architecture planning task. Choose orchestration sub-phase order, evidence gates, "
        "system boundaries, risk review, and trade-off review. Treat the source note as "
        f"architecture evidence only. Source={source_key}; Topic={heading}. Evidence summary: {excerpt}"
    )


def subphase_json(sub_phase: SubPhase) -> Dict[str, Any]:
    return {
        "Id": sub_phase.id,
        "PhaseId": sub_phase.phase_id,
        "Name": sub_phase.name,
        "TaskType": sub_phase.task_type,
        "Predicates": list(sub_phase.predicates),
    }


def build_rows(source_key: str, limit: int, allow_insecure_tls: bool, aosa_page_limit: int) -> Tuple[List[Dict[str, Any]], Dict[str, int]]:
    rows: List[Dict[str, Any]] = []
    candidates_by_locator: Dict[str, List[Tuple[str, str, str]]] = {}
    seen: set[str] = set()
    stats = {"scanned": 0, "accepted": 0, "filtered": 0, "deduped": 0}
    source_meta = {"license": "CC-BY-3.0", "source_url": AOSA_INDEX} if source_key == "aosa" else LOCAL_SOURCES[source_key]
    for heading, block, locator in source_blocks(source_key, allow_insecure_tls, aosa_page_limit):
        stats["scanned"] += 1
        text = re.sub(r"\s+", " ", block).strip()
        if not is_good_block(text):
            stats["filtered"] += 1
            continue
        key = text.lower()
        if key in seen:
            stats["deduped"] += 1
            continue
        seen.add(key)
        candidates_by_locator.setdefault(locator, []).append((heading, text, locator))

    selected: List[Tuple[str, str, str]] = []
    active = {locator: list(items) for locator, items in candidates_by_locator.items() if items}
    while active and len(selected) < limit:
        for locator in sorted(list(active)):
            group = active.get(locator)
            if not group:
                active.pop(locator, None)
                continue
            selected.append(group.pop(0))
            if not group:
                active.pop(locator, None)
            if len(selected) >= limit:
                break

    for heading, text, locator in selected:
        row_number = len(rows) + 1
        domain, sub_phases, evidence, predicates = choose_targets(text)
        primary = sub_phases[0]
        source_hash = sha1_short(f"{source_key}|{locator}|{heading}|{text}")
        rows.append({
            "RowId": f"{source_key.upper().replace('-', '_')}-ARCHSRC-{row_number:04d}",
            "RowNumber": row_number,
            "ConversationUuid": f"{source_key}-{source_hash}",
            "ConversationName": f"{source_key} architecture source row",
            "ConversationCreatedAt": "2026-05-11T00:00:00Z",
            "ConversationUpdatedAt": "2026-05-11T00:00:00Z",
            "MessageUuid": source_hash,
            "MessageIndex": row_number,
            "MessageCreatedAt": "2026-05-11T00:00:00Z",
            "UserRequest": make_user_request(source_key, heading, text),
            "Domain": domain,
            "TransitionType": "architecture_planning",
            "Predicates": predicates,
            "TargetSubPhases": [subphase_json(item) for item in sub_phases],
            "PrimarySubPhaseId": primary.id,
            "PrimaryPhaseId": primary.phase_id,
            "PrimaryTaskType": primary.task_type,
            "PrimarySubPhaseName": primary.name,
            "SourcePath": locator,
            "EvidenceTerms": evidence + [f"license:{source_meta['license']}"],
        })
        stats["accepted"] = len(rows)
    return rows, stats


def write_jsonl(path: Path, rows: Iterable[Dict[str, Any]]) -> None:
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        for row in rows:
            handle.write(json.dumps(row, ensure_ascii=False) + "\n")


def write_summary(path: Path, source_key: str, rows: List[Dict[str, Any]], stats: Dict[str, int], allow_insecure_tls: bool) -> None:
    by_domain: Dict[str, int] = {}
    by_primary: Dict[str, int] = {}
    for row in rows:
        by_domain[row["Domain"]] = by_domain.get(row["Domain"], 0) + 1
        by_primary[row["PrimarySubPhaseId"]] = by_primary.get(row["PrimarySubPhaseId"], 0) + 1
    source_meta = {"license": "CC-BY-3.0", "source_url": AOSA_INDEX} if source_key == "aosa" else LOCAL_SOURCES[source_key]
    lines = [
        f"# {source_key} Architecture Source Session Rows",
        "",
        f"- Rows scanned: {stats['scanned']}",
        f"- Rows accepted: {len(rows)}",
        f"- Rows filtered: {stats['filtered']}",
        f"- Rows deduped: {stats['deduped']}",
        f"- Source license state: {source_meta['license']}",
        f"- Source URL: {source_meta['source_url']}",
        f"- TLS verification bypassed: {str(allow_insecure_tls).lower()}",
        "- Source role: authority/source-note surface for architecture and planning rows",
        "",
        "## Domain Distribution",
        "",
    ]
    for key, value in sorted(by_domain.items()):
        lines.append(f"- {key}: {value}")
    lines.extend(["", "## Primary Sub-Phase Distribution", ""])
    for key, value in sorted(by_primary.items()):
        lines.append(f"- {key} {SUB_PHASES[key].name}: {value}")
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", choices=sorted(SOURCE_ALIASES), required=True)
    parser.add_argument("--limit", type=int, default=200)
    parser.add_argument("--output-dir", default="external sources learning 2/architecture-source-pilot/session-rows")
    parser.add_argument("--allow-insecure-tls", action="store_true")
    parser.add_argument("--aosa-page-limit", type=int, default=80)
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    source_key = SOURCE_ALIASES[args.source]
    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    rows, stats = build_rows(source_key, args.limit, args.allow_insecure_tls, args.aosa_page_limit)
    jsonl_path = output_dir / f"{source_key}-architecture-session-rows-{len(rows)}.jsonl"
    summary_path = output_dir / f"{source_key}-architecture-session-rows-summary.md"
    write_jsonl(jsonl_path, rows)
    write_summary(summary_path, source_key, rows, stats, args.allow_insecure_tls)
    print(json.dumps({
        "source": source_key,
        "rows": len(rows),
        "stats": stats,
        "jsonl": str(jsonl_path),
        "summary": str(summary_path),
    }, indent=2))
    if len(rows) < args.limit:
        print(f"warning: requested {args.limit} rows but produced {len(rows)}", file=sys.stderr)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
