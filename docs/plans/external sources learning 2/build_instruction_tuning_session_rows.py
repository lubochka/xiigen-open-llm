#!/usr/bin/env python3
"""
Build SessionLearningRow JSONL pilot sets from accepted instruction datasets.

The XIIGen batch comparison runner does not train from raw DPO pair rows. It
expects SessionLearningRow records: user prompt surfaces plus target lifecycle
sub-phases. This builder creates experiment-only rows for the runner from
external instruction datasets while keeping source answers out of training.
"""

from __future__ import annotations

import argparse
import json
import re
import ssl
import sys
import time
import urllib.parse
import urllib.error
import urllib.request
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Dict, Iterable, List, Optional, Tuple


DATASET_API = "https://datasets-server.huggingface.co/rows"

DATASETS = {
    "oasst1": {
        "dataset": "OpenAssistant/oasst1",
        "config": "default",
        "split": "train",
        "license": "Apache-2.0",
    },
    "oasst2": {
        "dataset": "OpenAssistant/oasst2",
        "config": "default",
        "split": "train",
        "license": "Apache-2.0",
    },
}

MOJIBAKE_MARKERS = ("Ã", "Â", "Ã¢")


@dataclass(frozen=True)
class SubPhase:
    id: str
    phase_id: str
    name: str
    task_type: str
    predicates: Tuple[str, ...]


SUB_PHASES: Dict[str, SubPhase] = {
    "1.1": SubPhase("1.1", "1", "Design System and Tokens", "design-system", ("UI",)),
    "1.2": SubPhase("1.2", "1", "Layout and Responsive Structure", "layout-responsive", ("UI",)),
    "1.3": SubPhase("1.3", "1", "Interaction and Navigation Model", "interaction-navigation", ("UI",)),
    "1.4": SubPhase("1.4", "1", "Accessibility and Usability Review", "accessibility-usability", ("UI", "QA")),
    "2.1": SubPhase("2.1", "2", "Business Capability Definition", "business-capability", ("DOCUMENT",)),
    "2.2": SubPhase("2.2", "2", "User Stories and Acceptance Criteria", "acceptance-criteria", ("DOCUMENT", "QA")),
    "2.3": SubPhase("2.3", "2", "Data and Validation Rules", "data-validation", ("API_CONTRACT", "DOCUMENT")),
    "2.4": SubPhase("2.4", "2", "Edge Cases and Boundary Conditions", "edge-cases", ("DOCUMENT",)),
    "3.1": SubPhase("3.1", "3", "Architecture and Component Topology", "architecture-topology", ("ARCHITECTURE",)),
    "3.2": SubPhase("3.2", "3", "Data Model and Persistence Design", "data-persistence", ("ARCHITECTURE",)),
    "3.3": SubPhase("3.3", "3", "API Contract Definition", "api-contract", ("API_CONTRACT",)),
    "3.4": SubPhase("3.4", "3", "Security and Authorization Model", "security-verification", ("SECURITY", "AUTHORIZATION")),
    "3.5": SubPhase("3.5", "3", "Observability and Operational Design", "observability-design", ("ARCHITECTURE",)),
    "4.1": SubPhase("4.1", "4", "Backend Services", "backend-services", ("IMPLEMENT",)),
    "4.2": SubPhase("4.2", "4", "Frontend and Client Integration", "frontend-client-integration", ("UI", "IMPLEMENT")),
    "4.3": SubPhase("4.3", "4", "Infrastructure and Deployment Code", "infrastructure-deployment", ("IMPLEMENT",)),
    "4.4": SubPhase("4.4", "4", "Refactoring and Code Quality", "refactoring-code-quality", ("IMPLEMENT",)),
    "4.5": SubPhase("4.5", "4", "Integration and Provider Connectors", "provider-connectors", ("IMPLEMENT", "API_CONTRACT")),
    "5.1": SubPhase("5.1", "5", "Unit Test Design", "unit-tests", ("TEST",)),
    "5.2": SubPhase("5.2", "5", "Integration Test Design", "integration-tests", ("TEST", "API_CONTRACT")),
    "5.3": SubPhase("5.3", "5", "End-to-End Scenario Testing", "end-to-end-tests", ("TEST", "QA")),
    "5.4": SubPhase("5.4", "5", "Regression and Compatibility Testing", "regression-compatibility", ("TEST",)),
    "6.1": SubPhase("6.1", "6", "Manual QA and Acceptance Review", "manual-qa", ("QA",)),
    "6.2": SubPhase("6.2", "6", "Release Readiness and Risk Review", "release-risk", ("QA",)),
    "6.3": SubPhase("6.3", "6", "Security and Compliance Review", "security-compliance-review", ("QA",)),
    "6.4": SubPhase("6.4", "6", "Performance and Reliability Review", "performance-reliability", ("QA",)),
    "7.1": SubPhase("7.1", "7", "Automated Test Harness", "automation-harness", ("TEST", "AUTOMATION")),
    "7.2": SubPhase("7.2", "7", "CI Pipeline and Quality Gates", "ci-quality-gates", ("AUTOMATION",)),
    "7.3": SubPhase("7.3", "7", "Monitoring and Feedback Loop", "monitoring-feedback", ("AUTOMATION",)),
    "7.4": SubPhase("7.4", "7", "Coverage and Reporting", "coverage-reporting", ("TEST", "DOCUMENT")),
    "7.5": SubPhase("7.5", "7", "Learning Set and Model Feedback", "learning-feedback", ("LEARNING",)),
}

PREDICATE_TERMS: Dict[str, Tuple[str, ...]] = {
    "SECURITY": ("security", "authorization", "authentication", "auth", "jwt", "oauth", "session", "mfa", "rbac"),
    "AUTHORIZATION": ("authorization", "permission", "rbac", "scope", "tenant", "ownership"),
    "API_CONTRACT": ("api", "endpoint", "contract", "schema", "request", "response", "openapi"),
    "ARCHITECTURE": ("architecture", "architect", "component", "topology", "boundary", "dependency", "project", "codebase", "system design"),
    "IMPLEMENT": ("implement", "implementation", "build", "create", "code", "fix", "bug", "connector", "provider", "integration"),
    "TEST": ("test", "testing", "verify", "validation", "coverage", "fixture", "regression", "failure", "failed"),
    "QA": ("qa", "quality", "release", "acceptance", "verification", "review"),
    "UI": ("ui", "ux", "screen", "layout", "figma", "interaction", "accessibility", "frontend"),
    "DOCUMENT": ("documentation", "docs", "readme", "document", "spec", "specification", "requirements", "plan", "steps"),
    "AUTOMATION": ("automation", "automate", "automated", "ci", "pipeline", "quality gate", "monitoring", "dashboard"),
    "LEARNING": ("skill", "skills", "learning", "teach", "training", "reusable guidance", "knowledge", "model feedback"),
}

ORCHESTRATION_TERMS = re.compile(
    r"\b(plan|steps|workflow|pipeline|architecture|system|project|build|implement|create|design|"
    r"software|app|website|api|test|validation|automation|skill|training|learning|model|agent|"
    r"code|debug|fix|requirements|specification|documentation|orchestrate|task)\b",
    re.IGNORECASE,
)

STRICT_ORCHESTRATION_TERMS = re.compile(
    r"\b(plan|steps|workflow|pipeline|architecture|system design|project plan|requirements|"
    r"specification|documentation|skill|skills|training|learning|model feedback|agent|"
    r"orchestrate|orchestration|task decomposition|quality gate|quality gates|review process)\b",
    re.IGNORECASE,
)

CODE_HEAVY_TERMS = re.compile(
    r"\b(write code|give me code|python|javascript|typescript|java|c#|csharp|function|class|"
    r"script|sql|regex|leetcode|programming problem|compile error|syntax error)\b",
    re.IGNORECASE,
)

CODE_ALLOWED_WITH_ORCHESTRATION_TERMS = re.compile(
    r"\b(architecture|architect|plan|workflow|requirements|specification|documentation|"
    r"quality gate|orchestrate|orchestration|skill|training|learning|test plan)\b",
    re.IGNORECASE,
)


def fetch_page(dataset_key: str, offset: int, length: int, allow_insecure_tls: bool, retry_count: int, retry_seconds: float) -> Dict[str, Any]:
    source = DATASETS[dataset_key]
    query = urllib.parse.urlencode({
        "dataset": source["dataset"],
        "config": source["config"],
        "split": source["split"],
        "offset": offset,
        "length": length,
    })
    request = urllib.request.Request(f"{DATASET_API}?{query}", headers={"User-Agent": "xiigen-learning-set-builder/1.0"})
    context = ssl._create_unverified_context() if allow_insecure_tls else None
    for attempt in range(retry_count + 1):
        try:
            with urllib.request.urlopen(request, timeout=90, context=context) as response:
                return json.loads(response.read().decode("utf-8"))
        except urllib.error.HTTPError as ex:
            if ex.code != 429 or attempt >= retry_count:
                raise
            sleep_for = retry_seconds * (attempt + 1)
            print(f"rate-limited by datasets-server; sleeping {sleep_for:.1f}s before retry {attempt + 1}/{retry_count}", file=sys.stderr)
            time.sleep(sleep_for)
    raise RuntimeError("unreachable retry loop exit")


def has_mojibake(text: str) -> bool:
    return any(marker in text for marker in MOJIBAKE_MARKERS)


def clean_text(text: str) -> str:
    return re.sub(r"\s+", " ", text).strip()


def row_text(row: Dict[str, Any], dataset_key: str) -> str:
    if dataset_key.startswith("oasst"):
        return str(row.get("text") or "")
    return str(row.get("instruction") or "")


def is_accepted_user_row(row: Dict[str, Any], dataset_key: str, strict_orchestration: bool) -> bool:
    if dataset_key.startswith("oasst"):
        if str(row.get("role") or "").lower() != "prompter":
            return False
        if str(row.get("lang") or "").lower() not in {"", "en", "eng"}:
            return False
        if row.get("deleted") is True:
            return False
        if row.get("review_result") is False:
            return False
    text = clean_text(row_text(row, dataset_key))
    if len(text) < 40 or len(text) > 1600:
        return False
    if has_mojibake(text):
        return False
    if strict_orchestration:
        if not STRICT_ORCHESTRATION_TERMS.search(text):
            return False
        if CODE_HEAVY_TERMS.search(text) and not CODE_ALLOWED_WITH_ORCHESTRATION_TERMS.search(text):
            return False
        return True
    return bool(ORCHESTRATION_TERMS.search(text))


def extract_predicates(text: str) -> Tuple[List[str], List[str]]:
    lower = text.lower()
    predicates: List[str] = []
    evidence: List[str] = []
    for predicate, terms in PREDICATE_TERMS.items():
        for term in terms:
            if term in lower:
                predicates.append(predicate)
                evidence.append(f"{predicate}:{term}")
                break
    if not predicates:
        predicates = ["DOCUMENT"]
        evidence = ["DOCUMENT:fallback"]
    return sorted(set(predicates)), evidence


def infer_domain(predicates: List[str], text: str) -> str:
    lower = text.lower()
    if "bug" in lower or "fix" in lower or "error" in lower or "failed" in lower:
        return "BUG"
    if "refactor" in lower or "cleanup" in lower:
        return "REFACTOR"
    if "architecture" in lower or "design" in lower or "topology" in lower:
        return "ARCH"
    if "code" in lower or "implement" in lower or "api" in lower:
        return "CODE_Q"
    return "PLAN"


def infer_subphases(predicates: List[str], text: str) -> List[SubPhase]:
    lower = text.lower()
    ids: List[str] = []
    if "UI" in predicates:
        ids.extend(["1.2", "4.2"])
    if "DOCUMENT" in predicates:
        ids.extend(["2.1", "2.2"])
    if "API_CONTRACT" in predicates:
        ids.extend(["2.3", "3.3", "5.2"])
    if "ARCHITECTURE" in predicates:
        ids.extend(["3.1", "3.2"])
    if "SECURITY" in predicates or "AUTHORIZATION" in predicates:
        ids.extend(["3.4", "6.3"])
    if "IMPLEMENT" in predicates:
        ids.extend(["4.1"])
    if "TEST" in predicates:
        ids.extend(["5.1", "5.2"])
    if "QA" in predicates:
        ids.extend(["6.1"])
    if "AUTOMATION" in predicates:
        ids.extend(["7.1", "7.2", "7.4"])
    if "LEARNING" in predicates:
        ids.extend(["7.5"])
    if "end-to-end" in lower or "e2e" in lower or "scenario" in lower:
        ids.append("5.3")
    if "regression" in lower or "compatibility" in lower:
        ids.append("5.4")
    if "monitoring" in lower or "feedback loop" in lower:
        ids.append("7.3")
    if not ids:
        ids = ["2.1", "3.1"]
    unique: List[str] = []
    for sub_phase_id in ids:
        if sub_phase_id not in unique:
            unique.append(sub_phase_id)
    return [SUB_PHASES[sub_phase_id] for sub_phase_id in unique[:5]]


def transition_type(predicates: List[str], text: str) -> str:
    lower = text.lower()
    if "LEARNING" in predicates:
        return "learning_feedback"
    if "TEST" in predicates or "QA" in predicates:
        return "validation"
    if "ARCHITECTURE" in predicates:
        return "architecture_planning"
    if "IMPLEMENT" in predicates:
        return "feature_addition"
    return "planning"


def subphase_json(sub_phase: SubPhase) -> Dict[str, Any]:
    return {
        "Id": sub_phase.id,
        "PhaseId": sub_phase.phase_id,
        "Name": sub_phase.name,
        "TaskType": sub_phase.task_type,
        "Predicates": list(sub_phase.predicates),
    }


def build_session_row(dataset_key: str, api_row: Dict[str, Any], row_number: int, row_idx: int) -> Dict[str, Any]:
    text = clean_text(row_text(api_row, dataset_key))
    predicates, evidence = extract_predicates(text)
    sub_phases = infer_subphases(predicates, text)
    primary = sub_phases[0]
    message_id = str(api_row.get("message_id") or f"{dataset_key}-{row_idx}")
    tree_id = str(api_row.get("message_tree_id") or message_id)
    created_at = str(api_row.get("created_date") or "")
    source = DATASETS[dataset_key]
    return {
        "RowId": f"{dataset_key.upper()}-INSTRUCTION-ORCH-{row_number:04d}",
        "RowNumber": row_number,
        "ConversationUuid": tree_id,
        "ConversationName": f"{dataset_key.upper()} instruction-tuning prompt surface",
        "ConversationCreatedAt": created_at,
        "ConversationUpdatedAt": created_at,
        "MessageUuid": message_id,
        "MessageIndex": row_idx,
        "MessageCreatedAt": created_at,
        "UserRequest": text,
        "Domain": infer_domain(predicates, text),
        "TransitionType": transition_type(predicates, text),
        "Predicates": predicates,
        "TargetSubPhases": [subphase_json(item) for item in sub_phases],
        "PrimarySubPhaseId": primary.id,
        "PrimaryPhaseId": primary.phase_id,
        "PrimaryTaskType": primary.task_type,
        "PrimarySubPhaseName": primary.name,
        "SourcePath": f"hf://{source['dataset']}/{source['split']}/{row_idx}/{message_id}",
        "EvidenceTerms": evidence,
    }


def collect_rows(
    dataset_key: str,
    limit: int,
    page_size: int,
    max_pages: int,
    pause_seconds: float,
    allow_insecure_tls: bool,
    retry_count: int,
    retry_seconds: float,
    strict_orchestration: bool) -> Tuple[List[Dict[str, Any]], Dict[str, int]]:
    accepted: List[Dict[str, Any]] = []
    seen: set[str] = set()
    stats = {"scanned": 0, "accepted": 0, "deduped": 0, "filtered": 0}
    offset = 0
    for _ in range(max_pages):
        page = fetch_page(dataset_key, offset, page_size, allow_insecure_tls, retry_count, retry_seconds)
        rows = page.get("rows") or []
        if not rows:
            break
        for wrapped in rows:
            row_idx = int(wrapped.get("row_idx") or offset)
            raw = wrapped.get("row") or {}
            stats["scanned"] += 1
            if not isinstance(raw, dict) or not is_accepted_user_row(raw, dataset_key, strict_orchestration):
                stats["filtered"] += 1
                continue
            text = clean_text(row_text(raw, dataset_key)).lower()
            if text in seen:
                stats["deduped"] += 1
                continue
            seen.add(text)
            accepted.append(build_session_row(dataset_key, raw, len(accepted) + 1, row_idx))
            stats["accepted"] = len(accepted)
            if len(accepted) >= limit:
                return accepted, stats
        offset += page_size
        time.sleep(pause_seconds)
    return accepted, stats


def write_jsonl(path: Path, rows: Iterable[Dict[str, Any]]) -> None:
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        for row in rows:
            handle.write(json.dumps(row, ensure_ascii=False, sort_keys=False) + "\n")


def write_summary(
    path: Path,
    dataset_key: str,
    rows: List[Dict[str, Any]],
    stats: Dict[str, int],
    allow_insecure_tls: bool,
    strict_orchestration: bool) -> None:
    by_domain: Dict[str, int] = {}
    by_primary: Dict[str, int] = {}
    for row in rows:
        by_domain[row["Domain"]] = by_domain.get(row["Domain"], 0) + 1
        by_primary[row["PrimarySubPhaseId"]] = by_primary.get(row["PrimarySubPhaseId"], 0) + 1
    lines = [
        f"# {dataset_key.upper()} Instruction-Tuning Session Rows",
        "",
        f"- Rows scanned: {stats['scanned']}",
        f"- Rows accepted: {len(rows)}",
        f"- Rows filtered: {stats['filtered']}",
        f"- Rows deduped: {stats['deduped']}",
        f"- Source license state: {DATASETS[dataset_key]['license']}",
        f"- TLS verification bypassed: {str(allow_insecure_tls).lower()}",
        f"- Strict orchestration focus: {str(strict_orchestration).lower()}",
        "- Assistant answers used: no",
        "- Output authority: experiment-only prompt-surface rows for batch runner",
        "",
        "## Domain Distribution",
        "",
    ]
    for key, value in sorted(by_domain.items()):
        lines.append(f"- {key}: {value}")
    lines.extend(["", "## Primary Sub-Phase Distribution", ""])
    for key, value in sorted(by_primary.items()):
        sub_phase = SUB_PHASES[key]
        lines.append(f"- {key} {sub_phase.name}: {value}")
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dataset", choices=sorted(DATASETS), required=True)
    parser.add_argument("--limit", type=int, default=200)
    parser.add_argument("--page-size", type=int, default=100)
    parser.add_argument("--max-pages", type=int, default=120)
    parser.add_argument("--pause-seconds", type=float, default=0.05)
    parser.add_argument("--retry-count", type=int, default=8)
    parser.add_argument("--retry-seconds", type=float, default=15.0)
    parser.add_argument("--allow-insecure-tls", action="store_true", help="Bypass TLS verification when local Python CA store rejects datasets-server.")
    parser.add_argument("--strict-orchestration", action="store_true", help="Require plan/architecture/skill/orchestration wording and reject code-only prompts.")
    parser.add_argument("--output-dir", default="external sources learning 2/instruction-tuning-pilot/session-rows")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)
    rows, stats = collect_rows(
        args.dataset,
        args.limit,
        args.page_size,
        args.max_pages,
        args.pause_seconds,
        args.allow_insecure_tls,
        args.retry_count,
        args.retry_seconds,
        args.strict_orchestration)
    suffix = "strict-session-rows" if args.strict_orchestration else "session-rows"
    jsonl_path = output_dir / f"{args.dataset}-{suffix}-{len(rows)}.jsonl"
    summary_path = output_dir / f"{args.dataset}-{suffix}-summary.md"
    write_jsonl(jsonl_path, rows)
    write_summary(summary_path, args.dataset, rows, stats, args.allow_insecure_tls, args.strict_orchestration)
    print(json.dumps({
        "dataset": args.dataset,
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
