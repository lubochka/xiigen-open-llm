#!/usr/bin/env python3
"""
Prepare XIIGen internal DPO learning sets from local roots.

The script is intentionally conservative:
- every filesystem file and nested ZIP member is inventoried;
- external-source paths are retained in inventory but excluded from internal pairs;
- pair extraction is automatic only for high-structure document families;
- ambiguous or unknown files are routed to model-led undefined-type candidate reports;
- root prompt, user-intent, genesis, and flow-initiation documents must not be
  demoted by filename-only logic. Scripts inventory and validate; model-led
  registry analysis decides authority and canonicality.
"""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import io
import json
import os
import re
import sys
import traceback
import zipfile
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any, Dict, Iterable, Iterator, List, Optional, Tuple


TEXT_EXTS = {
    ".md", ".txt", ".json", ".jsonl", ".yaml", ".yml", ".xml", ".html",
    ".cs", ".ts", ".tsx", ".js", ".jsx", ".py", ".mjs", ".sql", ".csv",
    ".toml", ".ini", ".config", ".csproj", ".sln", ".props", ".targets",
    ".dockerfile", ".yml", ".env", ".example",
}

ARCHIVE_EXTS = {".zip"}

EXTERNAL_MARKERS = [
    "external sources",
    "external-sources",
    "external sources learning",
    "external-sources-training",
    "external learning",
]

GENERATED_OUTPUT_MARKERS = [
    "internal learnin 2",
]

MOJIBAKE_MARKERS = ("Ã", "Â", "Ã¢")

WRONG_ANNOTATION_EXPLANATIONS = {
    "renumber_draft_id": "the original pre-renumber ID is no longer authoritative after the standalone renumber map is published. Using it numbers artifacts from the wrong base and can collide with other flows.",
    "stale_numbering": "the rejected side uses stale numbering instead of the latest canonical state. It cannot teach the caller which artifact range is currently safe.",
    "false_pass": "the rejected side claims success while the evidence shows failures or missing checks. It teaches the model to suppress verification evidence.",
    "evidence_free_claim": "the rejected side asserts a conclusion without citing the source evidence. The caller cannot learn which file, metric, or checkpoint proves the claim.",
    "pre_renumber_range": "the rejected side uses the draft pre-renumber range. That range is not safe after merge and can create duplicate IDs.",
    "contract_violation": "the rejected side violates the documented contract shape. The caller cannot learn the required fields or routing boundary from it.",
    "false_or_evidence_free_verdict": "the rejected side gives a verdict without the evidence needed to justify it. It trains unsupported review decisions.",
    "unsupported_training_claim": "the rejected side claims training readiness without checkpoint metrics, pair gates, or source authority. It can move unsafe rows into training.",
    "inside_existing_range": "the rejected side places new artifacts inside a range already owned by prior flows. That creates registry collisions.",
    "existing_range_number": "the rejected side reuses an existing artifact number instead of the next available canonical number.",
    "eval_wrong_b": "the rejected side is the intentionally wrong evaluation answer. It must remain visibly wrong so held-out evaluation does not become training authority.",
    "documented_violation": "the rejected side contradicts the cited guide or source document. It cannot teach the active rule.",
}

ROOT_PROMPT_NAME_PATTERNS = [
    r"basic[_ -]?prompt",
    r"session[-_ ]?start[-_ ]?prompt",
    r"start[-_ ]?prompt",
    r"flow[-_ ]?\d+.*(?:description|request|input)",
    r"^\d{1,2}\s*[-_ ].*\.md$",
]

PRIORITY_BY_FAMILY = {
    "B89": 1,
    "B140": 1,
    "B144": 1,
    "B160": 1,
    "B115": 2,
    "B157": 2,
    "B161": 2,
    "B97": 2,
    "B153": 2,
    "B146": 3,
    "B148": 3,
    "B152": 3,
    "B137": 3,
    "B162": 3,
    "B158": 4,
    "B131": 4,
    "B101": 4,
    "B85": 4,
    "B159": 4,
    "B96": 5,
    "B163": 5,
    "B165": 5,
    "B99": 5,
}


def now_stamp() -> str:
    return dt.datetime.now().strftime("%Y%m%d-%H%M%S")


def norm_path(path: str) -> str:
    return path.replace("\\", "/")


def sha1_text(text: str) -> str:
    return hashlib.sha1(text.encode("utf-8", errors="replace")).hexdigest()


def sha1_bytes(data: bytes) -> str:
    return hashlib.sha1(data).hexdigest()


def safe_read_text_from_bytes(data: bytes, limit: int = 8_000_000) -> Optional[str]:
    if len(data) > limit:
        return None
    if b"\x00" in data[:4096]:
        return None
    for enc in ("utf-8-sig", "utf-8", "cp1251", "latin-1"):
        try:
            return data.decode(enc)
        except UnicodeDecodeError:
            continue
    return data.decode("utf-8", errors="replace")


def safe_read_text_file(path: Path, limit: int = 8_000_000) -> Optional[str]:
    try:
        if path.stat().st_size > limit:
            return None
        data = path.read_bytes()
        return safe_read_text_from_bytes(data, limit=limit)
    except Exception:
        return None


def json_dump_line(handle, obj: Dict[str, Any]) -> None:
    handle.write(json.dumps(obj, ensure_ascii=False, sort_keys=True) + "\n")


def source_id_for(locator: str) -> str:
    return "src-" + hashlib.sha1(locator.encode("utf-8", errors="replace")).hexdigest()[:16]


def pair_id_for(prefix: str, locator: str, seq: int) -> str:
    return f"{prefix}-{hashlib.sha1(locator.encode('utf-8', errors='replace')).hexdigest()[:10]}-{seq:04d}"


def has_mojibake(value: Any) -> bool:
    text = "" if value is None else str(value)
    return any(marker in text for marker in MOJIBAKE_MARKERS)


def has_wrong_annotation(value: Any) -> bool:
    return "[WRONG:" in ("" if value is None else str(value))


def wrong_annotation_for(pair: Dict[str, Any]) -> str:
    rejected_type = str(pair.get("rejected_type") or "violates chosen authority").strip()
    explanation = WRONG_ANNOTATION_EXPLANATIONS.get(
        rejected_type,
        "the rejected side does not follow the chosen source authority. It withholds the evidence or consequence that the caller needs to make the correct decision.",
    )
    return f"[WRONG: {rejected_type} - {explanation}]"


def is_root_prompt_authority_candidate(name: str, locator: str, text_hint: Optional[str] = None) -> bool:
    probe = f"{name}\n{locator}\n{text_hint or ''}".lower().replace("\\", "/")
    if "deep research" in probe or "validation" in probe or "report" in probe:
        return False
    return any(re.search(pattern, probe, flags=re.I) for pattern in ROOT_PROMPT_NAME_PATTERNS)


def pair_validation_errors(pair: Dict[str, Any]) -> List[str]:
    errors: List[str] = []
    a = str(pair.get("a_side") or pair.get("prompt") or "")
    chosen = str(pair.get("chosen") or "")
    rejected = str(pair.get("rejected") or "")
    if not a.strip():
        errors.append("missing_a_side")
    if not chosen.strip():
        errors.append("missing_chosen")
    if not rejected.strip():
        errors.append("missing_rejected")
    if chosen.strip() and rejected.strip() and chosen.strip() == rejected.strip():
        errors.append("chosen_equals_rejected")
    for field_name, value in (("a_side", a), ("chosen", chosen), ("rejected", rejected)):
        if has_mojibake(value):
            errors.append(f"mojibake_in_{field_name}")
    if pair.get("quality_status") == "train_ready":
        if not has_wrong_annotation(rejected):
            errors.append("missing_wrong_annotation")
    return errors


def normalize_pair_for_current_run(pair: Dict[str, Any]) -> Dict[str, Any]:
    """Apply 2026-05-11 train-ready gates without doing semantic type adjudication."""
    rejected = str(pair.get("rejected") or "")
    if pair.get("quality_status") == "train_ready" and rejected.strip() and not has_wrong_annotation(rejected):
        pair["rejected"] = rejected.rstrip() + "\n" + wrong_annotation_for(pair)
    errors = pair_validation_errors(pair)
    pair["validation_gate_errors"] = errors
    if errors and pair.get("quality_status") == "train_ready":
        pair["quality_status"] = "repair_required"
    return pair


def is_external_path(path: str) -> bool:
    lower = norm_path(path).lower()
    return any(marker in lower for marker in EXTERNAL_MARKERS)


def in_current_output(path: str, output_root: Path) -> bool:
    try:
        Path(path).resolve().relative_to(output_root.resolve())
        return True
    except Exception:
        return False


def ext_of(name: str) -> str:
    lower = name.lower()
    if lower.endswith(".dockerfile"):
        return ".dockerfile"
    return Path(name).suffix.lower()


def classify_source(name: str, locator: str, text_hint: Optional[str], is_archive: bool) -> Dict[str, Any]:
    """Heuristic classification into known registry/additional families."""
    lower_name = name.lower()
    lower_loc = locator.lower().replace("\\", "/")
    ext = ext_of(name)

    if is_archive or ext in ARCHIVE_EXTS:
        return {
            "family": "archive_container",
            "family_name": "Archive container",
            "registry_status": "archive_container",
            "target_registry_type": "archive_container",
            "priority_tier": None,
        }

    if is_root_prompt_authority_candidate(name, locator, text_hint):
        return family("ROOT_PROMPT_AUTHORITY_CANDIDATE", "Root Prompt / User Intent Authority Candidate", "model_led_adjudication_required", "B08 / B162 / Type 21")

    # Highest priority guide types.
    if re.search(r"flow[-_ ]?\d+.*renumber[_-]?map\.json$", lower_name) or lower_name.endswith("renumber_map.json"):
        return family("B160", "Standalone Renumber Map JSON", "full_type", "Type 22 / AG01")
    if lower_name == "phase_state_final.json":
        return family("B144", "Translation Completion Declaration", "full_type", "Type 22")
    if lower_name == "batch-summary.jsonl":
        return family("B85", "Training Run Batch-Summary Logs", "full_type", "Type 4")
    if lower_name.endswith("dpo_step_000001.json") or re.search(r"dpo_step_\d+\.json$", lower_name):
        return family("B86", "DPO Training Step Checkpoint", "full_type", "Type 4")
    if "held-out" in lower_loc and lower_name.endswith("corruption.yaml"):
        return family("B90", "Held-Out Corruption Test Case", "full_type", "Type 8 eval")
    if "held-out" in lower_loc and lower_name.endswith("rephrase.yaml"):
        return family("B91", "Held-Out Rephrase Test Case", "full_type", "Type 8 eval")
    if re.search(r"flow[-_]\d+[-_]examination[-_]result\.json$", lower_name) or lower_name.endswith("examination-result.json"):
        return family("B97", "Flow Examination Result JSON", "full_type", "Type 8")
    if re.search(r"flow[-_]\d+[-_]examination[-_]result\.md$", lower_name) or lower_name.endswith("examination-result.md"):
        return family("B97", "Flow Examination Result MD", "full_type", "Type 8")
    if lower_name == "flow-examination-progress.json":
        return family("B98", "Flow Examination Progress JSON", "full_type", "Type 22")
    if lower_name.endswith("step-8-handoff-contract.md"):
        return family("B115", "STEP-8 Handoff Contract", "full_type", "Type 10 / Type 26")
    if "merge_master_plan" in lower_name or "merge-master-plan" in lower_name:
        return family("B157", "Flow Merge Master Plan", "full_type", "Type 29")
    if "merge_completion_report" in lower_name or "merge-completion-report" in lower_name:
        return family("B161", "Per-Flow Merge Completion Report", "full_type", "Type 32")
    if "validation_report" in lower_name or "validation-report" in lower_name:
        return family("B158", "Canonical/Flow Validation Report", "full_type", "Type 32")
    if re.search(r"phase[-_].*evidence\.json$", lower_name):
        return family("B131", "Phase Evidence JSON", "full_type", "Type 32")
    if "live-run-results.json" in lower_name or "execution-log.json" in lower_name:
        return family("B101", "Flow Live Run Evidence", "full_type", "Type 4 / Type 22")
    if lower_name.endswith("flow21_state.json") or re.search(r"flow\d+_state\.json$", lower_name):
        return family("B153", "Per-Flow Implementation Completion State", "full_type", "Type 22")
    if "engine_extension" in lower_name or "engine-extension" in lower_name:
        return family("B165", "Early/Per-Flow Engine Extension", "full_type", "Type 29 / AC11 / AG06")
    if "deep-research" in lower_name or "deep research" in lower_name:
        return family("B163", "Flow Deep Research With Citations", "limited_authority", "Type 21")
    if "prompt improvements" in lower_name or "prompt_improvements" in lower_name:
        return family("B164", "Prompt Improvement Session Document", "full_type", "AG05")
    if "planning-simulation" in lower_name:
        return family("AF06", "Planning Simulation Document", "full_type", "Type 35")
    if "design-simulation" in lower_name:
        return family("AF07", "Deep Design Simulation Document", "full_type", "Type 35")
    if "teach-qa-r0" in lower_name:
        return family("AF08", "TEACH-QA-R0 Bootstrap Document", "full_type", "Type 35")
    if "teach-qa-r1" in lower_name:
        return family("AF09", "TEACH-QA-R1 Correction Document", "full_type", "Type 36")
    if "1day-implementation-plan" in lower_name:
        return family("AF10", "Autonomous Claude Code Execution Plan", "full_type", "Type 29")
    if lower_name.endswith(".drawio.xml") or "drawio" in lower_name and ext == ".xml":
        return family("K07", "DrawIO XML Diagram", "full_type", "Type 27")
    if lower_name.endswith(".html") and "verdict" in lower_name:
        return family("AF12", "Design Review Verdict HTML Artifact", "preprocess_then_reclassify", "Type 8 / Type 32")
    if lower_name == "skill.md" or lower_name.endswith("-skill.md") or lower_name.endswith("_skill.md"):
        return family("Type20", "Skill File", "full_type", "Type 20")
    if lower_name in {"readme.md", "readme-unified.md"}:
        return family("B11", "README / Entry Point", "limited_authority", "Type 11")
    if "architecture" in lower_name and ext == ".md":
        return family("Type2", "Architecture Document", "full_type", "Type 2")
    if "decision" in lower_name and ext in {".md", ".json"}:
        return family("Type9", "Decision Document", "full_type", "Type 9")
    if ext in {".cs", ".ts", ".tsx", ".js", ".py", ".mjs"}:
        return family("Type26", "Source Code Contract / Implementation", "full_type", "Type 26")
    if ext in {".schema.json"} or lower_name.endswith(".schema.json"):
        return family("Type25", "Configuration Schema", "full_type", "Type 25")
    if ext in {".png", ".jpg", ".jpeg", ".gif", ".webp", ".ico", ".pdf", ".pptx", ".docx", ".xlsx", ".dll", ".exe", ".pdb"}:
        return family("binary_or_visual", "Binary/Visual Asset", "excluded_direct", "A10/A29/A17")
    if ext == ".json":
        return family("Type22", "Execution State / JSON Artifact", "full_type_candidate", "Type 22")
    if ext in {".md", ".txt"}:
        return family("text_unclassified", "Text Document Pending Classification", "full_type_candidate", "undefined_candidate")
    return family("unclassified", "Unclassified File", "preprocess_then_reclassify", "undefined_candidate")


def family(fid: str, name: str, status: str, target: str) -> Dict[str, Any]:
    return {
        "family": fid,
        "family_name": name,
        "registry_status": status,
        "target_registry_type": target,
        "priority_tier": PRIORITY_BY_FAMILY.get(fid),
    }


def iter_filesystem_sources(roots: List[Path], output_root: Path) -> Iterator[Dict[str, Any]]:
    for root in roots:
        root = root.resolve()
        if not root.exists():
            yield {"kind": "missing_root", "root": str(root)}
            continue
        for dirpath, dirnames, filenames in os.walk(root, topdown=True):
            # Prevent this run from ingesting its own outputs.
            if in_current_output(dirpath, output_root):
                dirnames[:] = []
                continue
            for filename in filenames:
                path = Path(dirpath) / filename
                try:
                    st = path.stat()
                except OSError as exc:
                    yield {"kind": "fs_error", "path": str(path), "error": str(exc)}
                    continue
                yield {
                    "kind": "filesystem",
                    "root": str(root),
                    "path": str(path),
                    "name": filename,
                    "size": st.st_size,
                    "mtime": dt.datetime.fromtimestamp(st.st_mtime).isoformat(),
                    "extension": ext_of(filename),
                }


def iter_zip_entries_from_file(path: Path, max_depth: int = 5) -> Iterator[Dict[str, Any]]:
    try:
        with zipfile.ZipFile(path, "r") as zf:
            yield from iter_zip_entries(zf, str(path), [], max_depth)
    except Exception as exc:
        yield {
            "kind": "zip_error",
            "archive_path": str(path),
            "archive_chain": [str(path)],
            "error": repr(exc),
        }


def iter_zip_entries(zf: zipfile.ZipFile, archive_path: str, chain: List[str], max_depth: int) -> Iterator[Dict[str, Any]]:
    current_chain = [archive_path] + chain
    for info in zf.infolist():
        if info.is_dir():
            continue
        locator = "::".join(current_chain + [info.filename])
        rec = {
            "kind": "zip_member",
            "archive_path": archive_path,
            "archive_chain": current_chain,
            "member_path": info.filename,
            "path": locator,
            "name": Path(info.filename).name,
            "size": info.file_size,
            "compressed_size": info.compress_size,
            "extension": ext_of(info.filename),
            "mtime": zip_dt(info),
        }
        yield rec
        if rec["extension"] == ".zip" and len(current_chain) < max_depth:
            try:
                data = zf.read(info)
                with zipfile.ZipFile(io.BytesIO(data), "r") as nested:
                    yield from iter_zip_entries(nested, archive_path, chain + [info.filename], max_depth)
            except Exception as exc:
                yield {
                    "kind": "zip_error",
                    "archive_path": archive_path,
                    "archive_chain": current_chain + [info.filename],
                    "member_path": info.filename,
                    "path": locator,
                    "error": repr(exc),
                }


def zip_dt(info: zipfile.ZipInfo) -> str:
    try:
        return dt.datetime(*info.date_time).isoformat()
    except Exception:
        return ""


def read_source_text(rec: Dict[str, Any]) -> Optional[str]:
    ext = rec.get("extension", "")
    if ext not in TEXT_EXTS:
        return None
    if rec["kind"] == "filesystem":
        return safe_read_text_file(Path(rec["path"]))
    if rec["kind"] == "zip_member":
        # Only used for selected candidates; open archive chain on demand.
        try:
            archive_path = rec["archive_path"]
            members = rec["archive_chain"][1:] + [rec["member_path"]]
            data = read_nested_zip_member(Path(archive_path), members)
            return safe_read_text_from_bytes(data)
        except Exception:
            return None
    return None


def read_nested_zip_member(archive_path: Path, members: List[str]) -> bytes:
    data: Optional[bytes] = None
    with zipfile.ZipFile(archive_path, "r") as zf:
        data = zf.read(members[0])
    for member in members[1:]:
        with zipfile.ZipFile(io.BytesIO(data), "r") as zf:
            data = zf.read(member)
    return data


def extract_pairs(rec: Dict[str, Any], classification: Dict[str, Any], text: Optional[str]) -> List[Dict[str, Any]]:
    if rec.get("source_scope") != "internal":
        return []
    fid = classification["family"]
    if text is None and rec.get("extension") in TEXT_EXTS:
        text = read_source_text(rec)
    pairs: List[Dict[str, Any]] = []
    try:
        if fid == "B160" and text:
            pairs.extend(extract_b160(rec, text))
        elif fid == "B144" and text:
            pairs.extend(extract_b144(rec, text))
        elif fid == "B85" and text:
            pairs.extend(extract_b85(rec, text))
        elif fid == "B97" and text and rec.get("extension") == ".json":
            pairs.extend(extract_b97_json(rec, text))
        elif fid == "B161" and text:
            pairs.extend(extract_b161(rec, text))
        elif fid == "B157" and text:
            pairs.extend(extract_b157(rec, text))
        elif fid == "B158" and text:
            pairs.extend(extract_b158(rec, text))
        elif fid == "B131" and text:
            pairs.extend(extract_b131(rec, text))
        elif fid == "B86" and text:
            pairs.extend(extract_b86(rec, text))
        elif fid in {"B90", "B91"} and text:
            pairs.extend(extract_heldout_yaml(rec, text, fid))
        elif fid == "B115" and text:
            pairs.extend(extract_b115_candidates(rec, text))
        elif fid == "B140" and text:
            pairs.extend(extract_wrong_correct_blocks(rec, text, "B140"))
    except Exception as exc:
        pairs.append(make_pair_error(rec, fid, exc))
    return pairs


def base_pair(rec: Dict[str, Any], family_id: str, seq: int, tier: Optional[int], mechanism: str) -> Dict[str, Any]:
    locator = rec["path"]
    return {
        "pair_id": pair_id_for(family_id, locator, seq),
        "source_family": family_id,
        "priority_tier": tier,
        "mechanism": mechanism,
        "source_path": locator,
        "source_locator": {"path": locator, "archive_member_path": rec.get("member_path")},
        "source_canonicality": infer_canonicality(locator),
        "canonical_confirmed_via": canonical_confirmation_for(locator, rec),
        "source_scope": rec.get("source_scope", "internal"),
        "quality_status": "candidate",
        "compiled_prompt_in_training": True,
    }


def infer_canonicality(locator: str) -> str:
    lower = locator.lower()
    if any(x in lower for x in ["stale", "do-not-use", "archive/", "/old/", "\\old\\"]):
        return "stale"
    if "final" in lower or "canonical" in lower or "complete" in lower:
        return "canonical"
    return "canonical"


def canonical_confirmation_for(locator: str, rec: Dict[str, Any]) -> str:
    lower = locator.lower().replace("\\", "/")
    if any(x in lower for x in ["stale", "do-not-use", "archive/", "/old/"]):
        return "path marker indicates stale or archived source"
    if any(x in lower for x in ["final", "canonical", "complete", "merged", "state-final"]):
        return "filename or path contains canonical/final/complete marker"
    if rec.get("member_path"):
        return "archive member path recorded in recursive source inventory"
    return "source inventory path and registry-family classifier"


def extract_json(text: str) -> Optional[Any]:
    try:
        return json.loads(text)
    except Exception:
        return None


def extract_b160(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    obj = extract_json(text)
    if not isinstance(obj, dict):
        return []
    pairs: List[Dict[str, Any]] = []
    flow_id = obj.get("flow") or obj.get("flow_id") or infer_flow_id(rec["path"])
    seq = 1
    for map_key in ("factory_map", "task_type_map", "family_map", "skill_map"):
        mp = obj.get(map_key)
        if isinstance(mp, dict):
            for new_id, value in list(mp.items())[:250]:
                label = value.get("name") if isinstance(value, dict) else str(value)
                original = value.get("original_id") if isinstance(value, dict) else None
                p = base_pair(rec, "B160", seq, 1, "renumber_authority")
                p.update({
                    "a_side": f"What is the correct {map_key.replace('_map','')} ID for {label} in {flow_id or 'this flow'} context?",
                    "chosen": f"{new_id} — from {Path(rec['name']).name}, machine-readable renumber map.",
                    "rejected": f"{original or 'the pre-renumber draft ID from input documents'} — rejected because the standalone renumber map is the post-merge authority.",
                    "pair_kind": "standalone_renumber_map",
                    "rejected_type": "renumber_draft_id",
                    "quality_status": "train_ready" if original else "review_required",
                    "dto": {"flow_id": flow_id, "map_key": map_key, "new_id": new_id, "label": label, "original_id": original},
                })
                pairs.append(p)
                seq += 1
    next_av = obj.get("next_available")
    if isinstance(next_av, dict):
        for k, v in next_av.items():
            p = base_pair(rec, "B160", seq, 1, "state_vs_evidence")
            p.update({
                "a_side": f"What is the next available {k} after {flow_id or 'this flow'} completes?",
                "chosen": f"{v} — from next_available.{k} in {Path(rec['name']).name}.",
                "rejected": "Use a number inside the just-added flow range.",
                "pair_kind": "standalone_renumber_map",
                "rejected_type": "inside_existing_range",
                "quality_status": "train_ready",
                "dto": {"flow_id": flow_id, "next_available_key": k, "next_available_value": v},
            })
            pairs.append(p)
            seq += 1
    return pairs


def extract_b144(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    obj = extract_json(text)
    if not isinstance(obj, dict):
        return []
    pairs: List[Dict[str, Any]] = []
    seq = 1
    numbering = obj.get("numbering_correction") or obj.get("numberingCorrections")
    if isinstance(numbering, (list, dict)):
        items = numbering if isinstance(numbering, list) else [numbering]
        for item in items:
            p = base_pair(rec, "B144", seq, 1, "canonicality")
            p.update({
                "a_side": "What numbering correction must be used after the translation completion declaration?",
                "chosen": json.dumps(item, ensure_ascii=False),
                "rejected": "Use the original pre-correction numbering from the earlier phase document.",
                "pair_kind": "translation_completion_declaration",
                "rejected_type": "stale_numbering",
                "quality_status": "train_ready",
                "dto": {"numbering_correction": item},
            })
            pairs.append(p)
            seq += 1
    next_av = obj.get("next_available") or obj.get("nextAvailable")
    if isinstance(next_av, dict):
        for k, v in next_av.items():
            p = base_pair(rec, "B144", seq, 1, "state_vs_evidence")
            p.update({
                "a_side": f"What is the next available {k} after the translation completion declaration?",
                "chosen": f"{v} — from PHASE_STATE_FINAL.json next_available.{k}.",
                "rejected": "Use a number already assigned inside the translated artifact ranges.",
                "pair_kind": "translation_completion_declaration",
                "rejected_type": "existing_range_number",
                "quality_status": "train_ready",
                "dto": {"next_available_key": k, "next_available_value": v},
            })
            pairs.append(p)
            seq += 1
    return pairs


def extract_b85(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    pairs: List[Dict[str, Any]] = []
    seq = 1
    for line_no, line in enumerate(text.splitlines(), 1):
        line = line.strip()
        if not line:
            continue
        try:
            obj = json.loads(line)
        except Exception:
            continue
        passed = obj.get("VerificationPassed")
        failures = obj.get("VerificationFailures") or []
        if passed is False or failures:
            batch = obj.get("BatchIndex", line_no)
            p = base_pair(rec, "B85", seq, 4, "state_vs_evidence")
            p.update({
                "a_side": f"What verification failures occurred in batch {batch}?",
                "chosen": json.dumps(failures, ensure_ascii=False),
                "rejected": "No verification failures — batch passed.",
                "pair_kind": "training_run_telemetry",
                "rejected_type": "false_pass",
                "quality_status": "train_ready",
                "dto": obj,
            })
            pairs.append(p)
            seq += 1
    return pairs


def extract_b86(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    obj = extract_json(text)
    if not isinstance(obj, dict):
        return []
    if not {"Loss", "ChosenReward", "RejectedReward", "RewardMargin", "Accuracy"} & set(obj):
        return []
    p = base_pair(rec, "B86", 1, 4, "state_vs_evidence")
    p.update({
        "a_side": "What DPO checkpoint metrics were recorded for this training step?",
        "chosen": json.dumps({k: obj.get(k) for k in ("Step", "Loss", "ChosenReward", "RejectedReward", "RewardMargin", "Accuracy") if k in obj}, ensure_ascii=False),
        "rejected": "Training progress cannot be claimed without checkpoint metrics.",
        "pair_kind": "dpo_checkpoint_metric",
        "rejected_type": "unsupported_training_claim",
        "quality_status": "train_ready",
        "dto": obj,
    })
    return [p]


def extract_b97_json(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    obj = extract_json(text)
    if not isinstance(obj, dict):
        return []
    flow_id = obj.get("FLOW_ID") or obj.get("flow_id") or infer_flow_id(rec["path"])
    verdicts = obj.get("H_VERDICTS") or obj.get("h_verdicts") or obj.get("SCENARIO_STEP_VERDICTS")
    if not isinstance(verdicts, dict):
        return []
    pairs: List[Dict[str, Any]] = []
    seq = 1
    for hid, val in verdicts.items():
        verdict = val.get("verdict") if isinstance(val, dict) else val
        evidence = val.get("evidence") if isinstance(val, dict) else None
        if verdict:
            p = base_pair(rec, "B97", seq, 2, "state_vs_evidence")
            p.update({
                "a_side": f"What is the {hid} verdict for {flow_id} and what evidence supports it?",
                "chosen": f"{verdict}" + (f" — evidence: {evidence}" if evidence else ""),
                "rejected": "PASS / CONSISTENT without citing the examination evidence.",
                "pair_kind": "flow_examination_result",
                "rejected_type": "false_or_evidence_free_verdict",
                "quality_status": "train_ready" if evidence else "review_required",
                "dto": {"flow_id": flow_id, "hypothesis": hid, "verdict": verdict, "evidence": evidence},
            })
            pairs.append(p)
            seq += 1
    return pairs


def extract_b161(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    flow_id = infer_flow_id(rec["path"])
    pairs: List[Dict[str, Any]] = []
    ranges = re.findall(r"(?im)(Factories|Families|Task Types|TaskTypes|BFA Rules|Skills|Templates)[^\n\r]*?([A-Z]+-?\d+\s*(?:[-–]\s*[A-Z]+-?\d+)?)", text)
    seq = 1
    for artifact_type, rng in ranges[:80]:
        p = base_pair(rec, "B161", seq, 2, "renumber_authority")
        p.update({
            "a_side": f"What artifact range did {flow_id or 'this flow'} add for {artifact_type}?",
            "chosen": f"{rng} — verified in {Path(rec['name']).name}.",
            "rejected": "Use the original pre-renumber range from the input documents.",
            "pair_kind": "merge_completion_evidence",
            "rejected_type": "pre_renumber_range",
            "quality_status": "review_required",
            "dto": {"flow_id": flow_id, "artifact_type": artifact_type, "range": rng},
        })
        pairs.append(p)
        seq += 1
    return pairs


def extract_b157(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    flow_id = infer_flow_id(rec["path"])
    pairs: List[Dict[str, Any]] = []
    # Look for explicit offset / original -> renumbered rows.
    rows = re.findall(r"(?im)(F\d+\s*[-–]\s*F\d+|T\d+\s*[-–]\s*T\d+|CF-\d+\s*[-–]\s*CF-\d+).*?(?:→|->|to|renumbered).*?((?:F|T)\d+\s*[-–]\s*(?:F|T)\d+|CF-\d+\s*[-–]\s*CF-\d+)", text)
    seq = 1
    for original, new in rows[:80]:
        p = base_pair(rec, "B157", seq, 2, "renumber_authority")
        p.update({
            "a_side": f"What is the correct renumbered artifact range for {flow_id or 'this flow'}?",
            "chosen": f"{new} — renumbered range from merge master plan.",
            "rejected": f"{original} — original pre-renumber range from input documents.",
            "pair_kind": "flow_merge_master_plan",
            "rejected_type": "pre_renumber_range",
            "quality_status": "train_ready",
            "dto": {"flow_id": flow_id, "original_range": original, "renumbered_range": new},
        })
        pairs.append(p)
        seq += 1
    return pairs


def extract_b158(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    flow_id = infer_flow_id(rec["path"])
    if not re.search(r"(?i)(validation matrix|checks passed|all pass|verified|status)", text):
        return []
    summary = first_matching_lines(text, [r"(?i)checks passed", r"(?i)all pass", r"(?i)validation matrix", r"(?i)verified"], 8)
    if not summary:
        return []
    p = base_pair(rec, "B158", 1, 4, "state_vs_evidence")
    p.update({
        "a_side": f"Has {flow_id or 'this artifact'} passed validation, and what evidence supports it?",
        "chosen": "\n".join(summary),
        "rejected": "Yes, validation passed. (No validation matrix or evidence cited.)",
        "pair_kind": "canonical_validation_report",
        "rejected_type": "evidence_free_claim",
        "quality_status": "review_required",
        "dto": {"flow_id": flow_id, "evidence_lines": summary},
    })
    return [p]


def extract_b131(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    obj = extract_json(text)
    if not isinstance(obj, dict):
        return []
    status = obj.get("status") or obj.get("Status") or obj.get("result")
    if status is None:
        return []
    p = base_pair(rec, "B131", 1, 4, "state_vs_evidence")
    p.update({
        "a_side": "What is the portability/test phase evidence status in this JSON file?",
        "chosen": json.dumps({k: obj.get(k) for k in obj.keys() if k.lower() in {"flowid", "flowslug", "phase", "status", "generatedat", "verification", "phaseclosescan"}}, ensure_ascii=False),
        "rejected": "The phase passed without checking exit codes or phase-close scan fields.",
        "pair_kind": "live_portability_evidence",
        "rejected_type": "false_pass",
        "quality_status": "train_ready",
        "dto": obj,
    })
    return [p]


def extract_heldout_yaml(rec: Dict[str, Any], text: str, fid: str) -> List[Dict[str, Any]]:
    wrong = extract_yaml_scalar_block(text, "wrong_B")
    correct = extract_yaml_scalar_block(text, "correct_B")
    desc = extract_yaml_scalar_block(text, "corruption") or extract_yaml_scalar_block(text, "description")
    if not (wrong and correct):
        return []
    p = base_pair(rec, fid, 1, None, "heldout_eval")
    p.update({
        "a_side": f"Evaluate this held-out case: {desc[:500] if desc else Path(rec['name']).name}",
        "chosen": correct,
        "rejected": wrong,
        "pair_kind": "holdout_corruption_eval" if fid == "B90" else "holdout_rephrase_eval",
        "rejected_type": "eval_wrong_b",
        "eval_only": True,
        "compiled_prompt_in_training": False,
        "quality_status": "eval_ready",
        "dto": {"description": desc},
    })
    return [p]


def extract_yaml_scalar_block(text: str, key: str) -> Optional[str]:
    m = re.search(rf"(?ms)^\s*{re.escape(key)}\s*:\s*(?:\|)?\s*\n(.*?)(?=^\S[^:\n]*\s*:|\Z)", text)
    if m:
        return m.group(1).strip()
    m = re.search(rf"(?m)^\s*{re.escape(key)}\s*:\s*(.+)$", text)
    if m:
        return m.group(1).strip().strip('"')
    return None


def extract_b115_candidates(rec: Dict[str, Any], text: str) -> List[Dict[str, Any]]:
    rules = re.findall(r"(?im)^\s*(?:[-*]|\d+[.)])\s*((?:DNA|IRON|Rule|RULE)[^:\n]*[:\-].{10,220})", text)
    pairs: List[Dict[str, Any]] = []
    for seq, rule in enumerate(rules[:40], 1):
        p = base_pair(rec, "B115", seq, 2, "dna_compliance")
        p.update({
            "a_side": "Write an implementation that satisfies the STEP-8 handoff contract.",
            "chosen": rule.strip(),
            "rejected": "Implementation ignores this handoff rule or changes the I/O contract shape.",
            "pair_kind": "step_teaching_contract",
            "rejected_type": "contract_violation",
            "quality_status": "review_required",
            "dto": {"rule_text": rule.strip()},
        })
        pairs.append(p)
    return pairs


def extract_wrong_correct_blocks(rec: Dict[str, Any], text: str, fid: str) -> List[Dict[str, Any]]:
    pairs: List[Dict[str, Any]] = []
    # Conservative: only pair adjacent WRONG/CORRECT fenced blocks.
    pattern = re.compile(r"(?is)(?:WRONG|BAD|Rejected)[^\n]*\n```[^\n]*\n(.*?)```\s*(?:CORRECT|GOOD|Chosen)[^\n]*\n```[^\n]*\n(.*?)```")
    for seq, m in enumerate(pattern.finditer(text), 1):
        rejected = m.group(1).strip()
        chosen = m.group(2).strip()
        if not chosen or not rejected:
            continue
        p = base_pair(rec, fid, seq, PRIORITY_BY_FAMILY.get(fid), "dna_compliance")
        p.update({
            "a_side": f"Apply the correct implementation pattern from {Path(rec['name']).name}.",
            "chosen": chosen,
            "rejected": rejected,
            "pair_kind": "stack_translation_execution" if fid == "B140" else "explicit_wrong_correct",
            "rejected_type": "documented_violation",
            "quality_status": "train_ready",
            "dto": {"pattern_source": Path(rec["name"]).name},
        })
        pairs.append(p)
    return pairs


def make_pair_error(rec: Dict[str, Any], fid: str, exc: Exception) -> Dict[str, Any]:
    p = base_pair(rec, fid, 0, PRIORITY_BY_FAMILY.get(fid), "extraction_error")
    p.update({
        "quality_status": "extract_error",
        "error": repr(exc),
        "trace": traceback.format_exc(limit=2),
    })
    return p


def infer_flow_id(locator: str) -> Optional[str]:
    m = re.search(r"(?i)FLOW[-_ ]?(\d{1,3})", locator)
    if m:
        return f"FLOW-{int(m.group(1)):02d}"
    m = re.search(r"(?i)(?:^|/|\\)(\d{1,2})[-_ ]", locator)
    if m:
        return f"FLOW-{int(m.group(1)):02d}"
    return None


def first_matching_lines(text: str, patterns: List[str], limit: int) -> List[str]:
    out: List[str] = []
    for line in text.splitlines():
        if any(re.search(p, line) for p in patterns):
            out.append(line.strip())
            if len(out) >= limit:
                break
    return out


def undefined_bucket(rec: Dict[str, Any], classification: Dict[str, Any]) -> str:
    if classification["family"] not in {"unclassified", "text_unclassified"}:
        return ""
    ext = rec.get("extension") or "<none>"
    name = rec.get("name", "")
    lower = name.lower()
    if ext in {".md", ".txt"}:
        if "plan" in lower:
            return "text_plan_unmapped"
        if "report" in lower or "review" in lower:
            return "text_report_unmapped"
        if "state" in lower:
            return "text_state_unmapped"
        return "text_general_unmapped"
    return f"{ext}_unmapped"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True)
    parser.add_argument("--guide", required=True)
    parser.add_argument("--additional-guide", required=True)
    parser.add_argument("--root", action="append", required=True)
    parser.add_argument("--max-zip-depth", type=int, default=5)
    args = parser.parse_args()

    output_root = Path(args.output).resolve()
    output_root.mkdir(parents=True, exist_ok=True)

    roots = [Path(p).resolve() for p in args.root]

    files = {
        "inventory": output_root / "inventory-all-sources.jsonl",
        "classification": output_root / "classification.jsonl",
        "pairs_all": output_root / "training-pairs-all.jsonl",
        "pairs_tier": {i: output_root / f"training-pairs-tier-{i}.jsonl" for i in range(1, 6)},
        "pairs_review": output_root / "training-pairs-review-required.jsonl",
        "eval": output_root / "eval-heldout.jsonl",
        "rag_only": output_root / "rag-only-sources.jsonl",
        "undefined": output_root / "undefined-type-candidates.jsonl",
        "model_led_authority": output_root / "model-led-authority-candidates.jsonl",
        "repair_required": output_root / "training-pairs-repair-required.jsonl",
        "zip_errors": output_root / "zip-errors.jsonl",
        "excluded": output_root / "excluded-sources.jsonl",
    }

    counters: Counter[str] = Counter()
    family_counts: Counter[str] = Counter()
    undefined_counts: Counter[str] = Counter()
    examples_by_undefined: Dict[str, List[str]] = defaultdict(list)
    pair_counts: Counter[str] = Counter()

    handles = {}
    try:
        handles["inventory"] = files["inventory"].open("w", encoding="utf-8")
        handles["classification"] = files["classification"].open("w", encoding="utf-8")
        handles["pairs_all"] = files["pairs_all"].open("w", encoding="utf-8")
        handles["pairs_review"] = files["pairs_review"].open("w", encoding="utf-8")
        handles["eval"] = files["eval"].open("w", encoding="utf-8")
        handles["rag_only"] = files["rag_only"].open("w", encoding="utf-8")
        handles["undefined"] = files["undefined"].open("w", encoding="utf-8")
        handles["model_led_authority"] = files["model_led_authority"].open("w", encoding="utf-8")
        handles["repair_required"] = files["repair_required"].open("w", encoding="utf-8")
        handles["zip_errors"] = files["zip_errors"].open("w", encoding="utf-8")
        handles["excluded"] = files["excluded"].open("w", encoding="utf-8")
        tier_handles = {i: files["pairs_tier"][i].open("w", encoding="utf-8") for i in range(1, 6)}

        def process_rec(rec: Dict[str, Any]) -> None:
            kind = rec.get("kind")
            if kind in {"fs_error", "missing_root", "zip_error"}:
                json_dump_line(handles["zip_errors"] if kind == "zip_error" else handles["excluded"], rec)
                counters[kind] += 1
                return

            locator = rec["path"]
            rec["source_id"] = source_id_for(locator)
            rec["source_scope"] = "external_excluded" if is_external_path(locator) else "internal"
            rec["is_current_output_excluded"] = in_current_output(locator, output_root)
            json_dump_line(handles["inventory"], rec)
            counters[f"{kind}_sources"] += 1
            counters["total_sources"] += 1

            is_archive = rec.get("extension") == ".zip"
            text_hint = None
            classification = classify_source(rec.get("name", ""), locator, text_hint, is_archive)
            classification_record = {
                **{k: rec.get(k) for k in ("source_id", "kind", "path", "archive_path", "member_path", "size", "extension", "source_scope")},
                **classification,
            }
            json_dump_line(handles["classification"], classification_record)
            family_counts[classification["family"]] += 1

            if rec["source_scope"] != "internal":
                json_dump_line(handles["excluded"], {**classification_record, "exclude_reason": "external source path excluded from internal training"})
                counters["external_excluded"] += 1
                return

            if classification["registry_status"] == "limited_authority" or classification["family"] in {"B163"}:
                json_dump_line(handles["rag_only"], classification_record)

            bucket = undefined_bucket(rec, classification)
            if bucket:
                undefined_counts[bucket] += 1
                if len(examples_by_undefined[bucket]) < 10:
                    examples_by_undefined[bucket].append(locator)
                json_dump_line(handles["undefined"], {**classification_record, "undefined_bucket": bucket})

            if classification["registry_status"] == "model_led_adjudication_required":
                json_dump_line(handles["model_led_authority"], {
                    **classification_record,
                    "handling": "model_led_type_and_authority_adjudication_required",
                    "do_not_auto_demote": True,
                    "reason": "root prompt/user-intent/session-start documents may be primary prompt authority",
                })

            # Avoid text reads for broad source-code families; extract only known priority families.
            if classification["family"] in PRIORITY_BY_FAMILY or classification["family"] in {"B86", "B90", "B91"}:
                text = read_source_text(rec)
                pairs = extract_pairs(rec, classification, text)
                for pair in pairs:
                    pair = normalize_pair_for_current_run(pair)
                    pair_counts[pair.get("source_family", "unknown")] += 1
                    json_dump_line(handles["pairs_all"], pair)
                    if pair.get("eval_only"):
                        json_dump_line(handles["eval"], pair)
                    elif pair.get("quality_status") == "train_ready":
                        tier = pair.get("priority_tier")
                        if tier in tier_handles:
                            json_dump_line(tier_handles[tier], pair)
                    elif pair.get("quality_status") == "repair_required":
                        json_dump_line(handles["repair_required"], pair)
                    else:
                        json_dump_line(handles["pairs_review"], pair)

        for rec in iter_filesystem_sources(roots, output_root):
            process_rec(rec)
            if rec.get("kind") == "filesystem" and rec.get("extension") == ".zip":
                for zrec in iter_zip_entries_from_file(Path(rec["path"]), args.max_zip_depth):
                    process_rec(zrec)

    finally:
        for h in handles.values():
            h.close()
        if "tier_handles" in locals():
            for h in tier_handles.values():
                h.close()

    manifest = {
        "generated_at": dt.datetime.now().isoformat(),
        "roots": [str(r) for r in roots],
        "output_root": str(output_root),
        "guide": args.guide,
        "additional_guide": args.additional_guide,
        "counters": dict(counters),
        "family_counts": dict(family_counts.most_common()),
        "pair_counts": dict(pair_counts.most_common()),
        "undefined_counts": dict(undefined_counts.most_common()),
        "files": {k: str(v) for k, v in files.items() if not isinstance(v, dict)},
        "tier_files": {str(k): str(v) for k, v in files["pairs_tier"].items()},
    }
    (output_root / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")

    write_reports(output_root, manifest, undefined_counts, examples_by_undefined)
    return 0


def write_reports(output_root: Path, manifest: Dict[str, Any], undefined_counts: Counter[str], examples: Dict[str, List[str]]) -> None:
    lines = []
    lines.append("# Internal Learning Sets Run\n")
    lines.append(f"Generated: `{manifest['generated_at']}`\n")
    lines.append("## Roots\n")
    for root in manifest["roots"]:
        lines.append(f"- `{root}`")
    lines.append("\n## Counts\n")
    for k, v in manifest["counters"].items():
        lines.append(f"- `{k}`: {v}")
    lines.append("\n## Pair Counts\n")
    for k, v in manifest["pair_counts"].items():
        lines.append(f"- `{k}`: {v}")
    lines.append("\n## Key Outputs\n")
    lines.append("- `inventory-all-sources.jsonl` — every filesystem file and nested ZIP member")
    lines.append("- `classification.jsonl` — registry/additional-family classification")
    lines.append("- `training-pairs-tier-*.jsonl` — train-ready pairs by DPO guide tier")
    lines.append("- `training-pairs-review-required.jsonl` — candidate pairs needing human review")
    lines.append("- `eval-heldout.jsonl` — held-out/eval rows excluded from training")
    lines.append("- `rag-only-sources.jsonl` — limited-authority internal sources")
    lines.append("- `undefined-type-candidates.*` — additional guide for unmapped types")
    lines.append("- `model-led-authority-candidates.jsonl` — root prompt/user-intent/session-start candidates that require model-led registry adjudication, not filename-only demotion")
    lines.append("- `training-pairs-repair-required.jsonl` — rows blocked by hard DPO gates such as identity pairs, mojibake, or missing `[WRONG: ...]`")
    lines.append("\n## Safety Notes\n")
    lines.append("- Paths containing external-source markers are inventoried but excluded from internal training pairs.")
    lines.append("- Current output run directory is excluded from scanning to prevent self-ingestion.")
    lines.append("- `review_required` pairs are not placed in tier train-ready files.")
    lines.append("- User-intent/root-prompt documents are routed to model-led authority candidates; they are not low-authority by default.")
    lines.append("- Train-ready pairs must satisfy the 2026-05-11 gates: chosen != rejected, no mojibake in A/chosen/rejected, and rejected carries `[WRONG: ...]`.")
    (output_root / "README.md").write_text("\n".join(lines) + "\n", encoding="utf-8")

    u = []
    u.append("# Undefined Type Candidates Guide\n")
    u.append("These buckets are files that did not match an explicit registry family. Use the three gateway questions from `XIIGEN-DPO-REGISTRY-GUIDE-v1.md` before creating any new type.\n")
    u.append("| Bucket | Count | Suggested handling | Examples |")
    u.append("|---|---:|---|---|")
    for bucket, count in undefined_counts.most_common():
        ex = "<br>".join(f"`{e}`" for e in examples.get(bucket, [])[:5])
        suggested = "compare structure/consumer/A-side; likely subtype unless all three differ"
        u.append(f"| `{bucket}` | {count} | {suggested} | {ex} |")
    (output_root / "undefined-type-candidates.md").write_text("\n".join(u) + "\n", encoding="utf-8")

    c = []
    c.append("# Coverage Report\n")
    c.append("## Top Families\n")
    for k, v in list(manifest["family_counts"].items())[:80]:
        c.append(f"- `{k}`: {v}")
    c.append("\n## Undefined Buckets\n")
    for k, v in manifest["undefined_counts"].items():
        c.append(f"- `{k}`: {v}")
    c.append("\n## Pair Families\n")
    for k, v in manifest["pair_counts"].items():
        c.append(f"- `{k}`: {v}")
    (output_root / "coverage-report.md").write_text("\n".join(c) + "\n", encoding="utf-8")


if __name__ == "__main__":
    raise SystemExit(main())
