#!/usr/bin/env python3
"""Augment a completed internal learning run with Tier 3 governance/intent pairs."""

from __future__ import annotations

import argparse
import datetime as dt
import json
import re
import sys
from pathlib import Path
from typing import Any, Dict, Iterable, List, Optional

SCRIPT_DIR = Path(__file__).resolve().parent
sys.path.insert(0, str(SCRIPT_DIR))

from prepare_internal_learning_sets import (  # noqa: E402
    base_pair,
    extract_b115_candidates,
    infer_flow_id,
    read_source_text,
    source_id_for,
)


def iter_jsonl(path: Path) -> Iterable[Dict[str, Any]]:
    with path.open("r", encoding="utf-8") as handle:
        for line in handle:
            line = line.strip()
            if not line:
                continue
            yield json.loads(line)


def write_jsonl(path: Path, rows: Iterable[Dict[str, Any]], append: bool = True) -> int:
    count = 0
    mode = "a" if append else "w"
    with path.open(mode, encoding="utf-8") as handle:
        for row in rows:
            handle.write(json.dumps(row, ensure_ascii=False, sort_keys=True) + "\n")
            count += 1
    return count


def short_section(text: str, heading_regex: str, max_chars: int = 1800) -> str:
    m = re.search(heading_regex, text, flags=re.I | re.M)
    if not m:
        return ""
    start = m.start()
    next_h = re.search(r"(?m)^#{1,3}\s+", text[start + 1 :])
    end = start + 1 + next_h.start() if next_h else min(len(text), start + max_chars)
    return text[start:end].strip()[:max_chars]


def first_lines(text: str, max_lines: int = 20, max_chars: int = 1800) -> str:
    lines = [ln.rstrip() for ln in text.splitlines() if ln.strip()]
    return "\n".join(lines[:max_lines])[:max_chars]


def make_pair(rec: Dict[str, Any], family: str, seq: int, mechanism: str, a: str, chosen: str, rejected: str, pair_kind: str, quality: str = "train_ready", dto: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
    p = base_pair(rec, family, seq, 3, mechanism)
    p.update({
        "a_side": a,
        "chosen": chosen,
        "rejected": rejected,
        "pair_kind": pair_kind,
        "rejected_type": "plausible_governance_violation",
        "quality_status": quality,
        "dto": dto or {},
        "augmentation": "tier3-governance-intent-v1",
    })
    return p


def extract_b148(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    chosen = short_section(text, r"before you write any code|before you create|4 laws|the 4 laws") or first_lines(text)
    return [
        make_pair(
            rec,
            "B148",
            1,
            "governance_checklist",
            "What should an AI agent check before writing XIIGen/Genie code?",
            chosen,
            "Start by generating a service/controller immediately, then fix architecture problems during review.",
            "ai_agent_quick_start",
            dto={"document": rec.get("name"), "checklist_section_extracted": bool(chosen)},
        )
    ] if chosen else []


def extract_b137(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    chosen = short_section(text, r"one rule|master mapping|module-by-module|anti-pattern|deployment manifest") or first_lines(text)
    return [
        make_pair(
            rec,
            "B137",
            1,
            "composition_governance",
            "How should an AI agent compose generic modules into a site type?",
            chosen,
            "Create a site-specific controller/service for each site type and hardcode the behavior in code.",
            "ai_agent_composition_guide",
            dto={"document": rec.get("name"), "composition_source": True},
        )
    ] if chosen else []


def extract_b146(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    lower = rec.get("path", "").lower()
    pairs: List[Dict[str, Any]] = []
    if "session-protocol" in lower:
        chosen = short_section(text, r"state entry format|recovery protocol|session end checklist") or first_lines(text)
        pairs.append(make_pair(
            rec, "B146", 1, "governance_protocol",
            "How should an architect skill save or restore session state?",
            chosen,
            "Write an informal summary of what happened and continue without Status, Stage, Last decision, Next action, Blocking, Skills consumed, and Files produced.",
            "architect_skill_package",
            dto={"reference_protocol": "session-protocol"},
        ))
    elif "freedom-machine" in lower:
        chosen = short_section(text, r"freedom|machine|one rule|classification") or first_lines(text)
        pairs.append(make_pair(
            rec, "B146", 1, "decision_philosophy_ladder",
            "How should an architect classify a value as MACHINE or FREEDOM?",
            chosen,
            "Treat values as configurable whenever a user might ask to change them, even when changing them breaks behavioral guarantees.",
            "architect_reference_protocol",
            dto={"reference_protocol": "freedom-machine"},
        ))
    elif "build-standards" in lower or rec.get("name", "").lower() == "skill.md":
        chosen = short_section(text, r"iron rules|commands|required files|build standards|microservicebase") or first_lines(text)
        pairs.append(make_pair(
            rec, "B146", 1, "architect_skill_package",
            "What output structure should the architect skill enforce?",
            chosen,
            "Produce a single analysis document with no executable artifacts, no reference protocol files, and no session state.",
            "architect_skill_package",
            dto={"reference_protocol": rec.get("name")},
        ))
    return [p for p in pairs if p.get("chosen")]


def extract_b152(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    chosen = short_section(text, r"before all|who consumes|execution|reference|stop|deliverable") or first_lines(text)
    return [
        make_pair(
            rec,
            "B152",
            1,
            "web_session_handoff_governance",
            "How should a web session package work for Claude Code consumption?",
            chosen,
            "Return one large human-readable analysis document and assume Claude Code will infer and execute the tasks from it.",
            "web_session_handoff_governance",
            dto={"must_split_execution_and_reference": True},
        )
    ] if chosen else []


def split_intent_response(text: str) -> Optional[Dict[str, str]]:
    lines = [ln.strip() for ln in text.splitlines() if ln.strip()]
    if len(lines) < 8:
        return None
    joined = "\n".join(lines)
    # Prefer obvious conversational split.
    split_patterns = [
        r"(?i)\n(?:answer|response|architecture|design|solution|here(?:'s| is))[:\s]",
        r"(?i)\n#+\s*(?:solution|architecture|design|response)",
    ]
    split_at = None
    for pat in split_patterns:
        m = re.search(pat, joined)
        if m and m.start() > 50:
            split_at = m.start()
            break
    if split_at is None:
        split_at = min(len(joined), max(350, len(joined) // 4))
    return {
        "intent": joined[:split_at].strip()[:1800],
        "response": joined[split_at:].strip()[:2400],
    }


def extract_b162(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    split = split_intent_response(text)
    if not split or not split["response"]:
        return []
    flow_id = infer_flow_id(rec.get("path", "")) or "FLOW-UNKNOWN"
    return [
        make_pair(
            rec,
            "B162",
            1,
            "user_intent_to_design_response",
            split["intent"],
            split["response"],
            "Implement the request directly with a narrow feature-specific service and skip the architectural design response.",
            "flow_initiation_intent_response",
            quality="review_required",
            dto={"flow_id": flow_id, "raw_intent_preserved": True, "response_locator": "heuristic_split"},
        )
    ]


def extract_b165(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    chosen = short_section(text, r"business story|mini-rag|no-code explanation|resume anchor|phase") or first_lines(text)
    flow_id = infer_flow_id(rec.get("path", "")) or "FLOW-01"
    return [
        make_pair(
            rec,
            "B165",
            1,
            "early_engine_extension",
            f"What does {flow_id} add to the engine at the early engine-extension stage?",
            chosen,
            "Use the later merge master plan only and ignore the early business story, text DAG, Mini-RAG, and resume anchors.",
            "early_engine_extension",
            quality="review_required",
            dto={"flow_id": flow_id},
        )
    ] if chosen else []


def should_select_b162(rec: Dict[str, Any]) -> bool:
    name = rec.get("name", "")
    lower = name.lower()
    if rec.get("extension") != ".md":
        return False
    if any(x in lower for x in ["deep", "engine_architecture", "task_types", "bfa", "skills_factory", "source_index", "session_state", "merged", "validation", "plan", "report"]):
        return False
    return bool(re.match(r"^\d{1,2}\s*[-_ ]", name))


def classify_for_augmentation(rec: Dict[str, Any]) -> Optional[str]:
    path = rec.get("path", "").lower().replace("\\", "/")
    name = rec.get("name", "").lower()
    if rec.get("source_scope") != "internal":
        return None
    if name == "agent_quickstart.md":
        return "B148"
    if name in {"complete_ai_agent_guide.md", "skill_composition_guide.md", "ai_agent_guide_skill_mapping.md"}:
        return "B137"
    if "xiigen-architect-skill" in path and (name == "skill.md" or "/references/" in path):
        return "B146"
    if "agent-output-format" in path and name in {"skill.md", "agents.md", "skill.yaml"}:
        return "B152"
    if should_select_b162(rec):
        return "B162"
    if name == "flow01_engine_extension.md":
        return "B165"
    return None


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--run", required=True)
    args = ap.parse_args()
    run = Path(args.run)
    inventory = run / "inventory-all-sources.jsonl"
    all_pairs = run / "training-pairs-all.jsonl"
    tier3 = run / "training-pairs-tier-3.jsonl"
    review = run / "training-pairs-review-required.jsonl"
    manifest_path = run / "manifest.json"

    produced: List[Dict[str, Any]] = []
    selected_counts: Dict[str, int] = {}
    for rec in iter_jsonl(inventory):
        fam = classify_for_augmentation(rec)
        if not fam:
            continue
        rec["source_id"] = rec.get("source_id") or source_id_for(rec["path"])
        text = read_source_text(rec)
        if not text:
            continue
        pairs: List[Dict[str, Any]] = []
        if fam == "B148":
            pairs = extract_b148(rec, text)
        elif fam == "B137":
            pairs = extract_b137(rec, text)
        elif fam == "B146":
            pairs = extract_b146(rec, text)
        elif fam == "B152":
            pairs = extract_b152(rec, text)
        elif fam == "B162":
            pairs = extract_b162(rec, text)
        elif fam == "B165":
            pairs = extract_b165(rec, text)
        for p in pairs:
            p["source_family"] = fam
            produced.append(p)
            selected_counts[fam] = selected_counts.get(fam, 0) + 1

    train_ready = [p for p in produced if p.get("quality_status") == "train_ready" and not p.get("eval_only")]
    review_required = [p for p in produced if p.get("quality_status") != "train_ready" and not p.get("eval_only")]
    write_jsonl(all_pairs, produced, append=True)
    write_jsonl(tier3, train_ready, append=True)
    write_jsonl(review, review_required, append=True)

    summary = {
        "generated_at": dt.datetime.now().isoformat(),
        "augmentation": "tier3-governance-intent-v1",
        "produced_pairs": len(produced),
        "train_ready_tier3": len(train_ready),
        "review_required": len(review_required),
        "selected_counts": selected_counts,
    }
    (run / "tier3-augmentation-summary.json").write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")

    if manifest_path.exists():
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        manifest["tier3_augmentation"] = summary
        pair_counts = manifest.setdefault("pair_counts", {})
        for fam, cnt in selected_counts.items():
            pair_counts[fam] = pair_counts.get(fam, 0) + cnt
        manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")

    readme = run / "README.md"
    with readme.open("a", encoding="utf-8") as handle:
        handle.write("\n## Tier 3 Augmentation\n\n")
        handle.write(f"- Produced pairs: {len(produced)}\n")
        handle.write(f"- Train-ready Tier 3 pairs: {len(train_ready)}\n")
        handle.write(f"- Review-required Tier 3 candidates: {len(review_required)}\n")
        for fam, cnt in selected_counts.items():
            handle.write(f"- `{fam}`: {cnt}\n")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
