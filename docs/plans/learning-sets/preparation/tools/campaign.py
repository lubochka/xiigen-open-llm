#!/usr/bin/env python3
"""Create-only DPO campaign: prepare work, record real drafts, preserve families.

Standard library only. The tool never calls a model or starts a learner.
Run from preparation/tools; the preparation directory, not cwd, owns the plan.
"""
from __future__ import annotations

import argparse
from collections import Counter
from datetime import datetime, timezone
import json
import os
from pathlib import Path
import re
import shlex
import sys
import tempfile
import uuid

from campaign_status import transaction

BASE = Path(__file__).resolve().parent.parent
REQUEST_SCHEMA = "xiigen.domain-campaign-request.v1"
STATE_SCHEMA = "xiigen.domain-campaign-state.v1"
MODE = "CREATE_ONLY"
ROW_FIELDS = ("schema_version", "pair_id", "domain_id", "topic_id", "profile_id",
              "family_id", "split", "prompt", "chosen", "rejected", "error_tag",
              "rationale", "source_refs", "provenance", "metadata")
MATERIAL_FIELDS = ("family_id", "origin_family_id", "scenario_group", "scenario", "semantic_facts")


def now():
    return datetime.now(timezone.utc).isoformat(timespec="seconds")


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError("Duplicate JSON key: " + key)
        result[key] = value
    return result


def strict_load(text):
    def bad_constant(value):
        raise ValueError("Non-finite JSON number: " + value)
    return json.loads(text, object_pairs_hook=unique_object, parse_constant=bad_constant)


def read(path):
    return strict_load(Path(path).read_text(encoding="utf-8-sig"))


def json_text(value):
    return json.dumps(value, ensure_ascii=False, indent=2, allow_nan=False) + "\n"


def atomic_text(path, text):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    fd, temporary = tempfile.mkstemp(prefix=".write-", dir=str(path.parent))
    try:
        with os.fdopen(fd, "w", encoding="utf-8", newline="\n") as handle:
            handle.write(text)
            handle.flush()
            os.fsync(handle.fileno())
        os.replace(temporary, path)
    finally:
        if os.path.exists(temporary):
            os.unlink(temporary)


def write_new(path, text):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("x", encoding="utf-8", newline="\n") as handle:
        handle.write(text)


def read_rows(path, optional=False):
    path = Path(path)
    if optional and not path.exists():
        return []
    rows = []
    for number, line in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
        if not line.strip():
            continue
        try:
            item = strict_load(line)
        except (ValueError, json.JSONDecodeError) as error:
            raise ValueError(f"{path}:{number}: {error}") from error
        if not isinstance(item, dict):
            raise ValueError(f"{path}:{number}: a JSON object is required")
        rows.append(item)
    return rows


def file_stamp(path):
    path = Path(path)
    if not path.is_file():
        raise ValueError("Saved draft is missing: " + str(path) + ". Preserve the completion record; use an explicitly assigned new revision, not a silent recount.")
    stat = path.stat()
    return {"size": stat.st_size, "mtime_ns": stat.st_mtime_ns}


def verify_completed_input(request, report):
    expected = Path(request["paths"]["draft"]).resolve()
    if Path(report.get("input_path", "")).resolve() != expected:
        raise ValueError("Completed report does not name the exact saved draft path")
    if not isinstance(report.get("input_stamp"), dict):
        raise ValueError("Completion record has no input_stamp; it cannot establish that the current draft is the completed file. Preserve it and use an explicit revision.")
    if file_stamp(expected) != report["input_stamp"]:
        raise ValueError("Saved draft changed after DRAFT_CREATED: " + str(expected) + ". The original completion record is preserved; use an explicitly assigned new revision, not a silent recount.")


def meaningful(value, label):
    if not isinstance(value, str) or not value.strip():
        raise ValueError(label + " must contain real nonempty text")
    stripped = value.strip()
    placeholders = {"...", "…", "TODO", "TBD", "PLACEHOLDER", "unknown", "fill here", "заполнить"}
    bracket_placeholder = re.fullmatch(r"<(?:write|fill|insert|replace|placeholder|TODO|TBD|заполни|вставь)[^>]*>", stripped, re.IGNORECASE)
    if stripped in placeholders or bracket_placeholder:
        raise ValueError(label + " is a placeholder, not authored content")


def literal_id(value, label):
    if not isinstance(value, str) or not value or any(ch in value for ch in "\\/\x00\r\n") or value in {".", ".."}:
        raise ValueError(label + " must be a nonempty single path component")


def config():
    registry_path = BASE / "campaign" / "registry.json"
    lessons_path = BASE / "campaign" / "lessons.json"
    registry = read(registry_path)
    lessons_data = read(lessons_path)
    if registry.get("mode") != MODE:
        raise ValueError("The campaign must explicitly use CREATE_ONLY")
    literal_id(registry.get("campaign_id"), "campaign_id")
    domains = registry.get("domains", [])
    if not domains or len({d["domain_id"] for d in domains}) != len(domains):
        raise ValueError("The campaign requires distinct nonempty domain IDs")
    if any(d.get("target_pairs") != 1000 for d in domains) or registry.get("total_target_pairs") != sum(d["target_pairs"] for d in domains):
        raise ValueError("The current quota is 1000 per domain; the total must equal the registry sum")
    allocations = sorted(registry.get("allocations", []), key=lambda row: row["order"])
    if not allocations or len({a["allocation_id"] for a in allocations}) != len(allocations):
        raise ValueError("Allocation IDs must be unique and nonempty")
    if [a["order"] for a in allocations] != list(range(1, len(allocations) + 1)):
        raise ValueError("Allocation order must be contiguous from 1")
    lessons = {lesson["lesson_id"]: lesson for lesson in lessons_data.get("lessons", [])}
    if len(lessons) != len(lessons_data.get("lessons", [])):
        raise ValueError("Duplicate lesson_id")
    totals = Counter()
    known_domains = {d["domain_id"] for d in domains}
    for i, allocation in enumerate(allocations):
        aid = allocation["allocation_id"]
        literal_id(aid, "allocation_id")
        expected_next = allocations[i + 1]["allocation_id"] if i + 1 < len(allocations) else None
        if allocation.get("next_allocation_id") != expected_next:
            raise ValueError("Broken next_allocation_id at " + aid)
        if allocation.get("domain_id") not in known_domains:
            raise ValueError("Unknown domain in " + aid)
        if allocation["domain_id"].startswith("tr."):
            languages = allocation["profile_id"].split("-")
            if len(languages) != 2 or any(value not in {"en", "ru", "he"} for value in languages) or languages[0] == languages[1] or allocation["domain_id"] != "tr." + languages[1]:
                raise ValueError("Translation profile does not match its target-language domain: " + aid)
        target = allocation.get("target_pairs")
        per_family = allocation.get("pairs_per_family", 1)
        batch = allocation.get("batch_size", 10)
        if type(target) is not int or target < 0 or per_family not in (1, 2) or target % per_family:
            raise ValueError("Invalid target/family allocation in " + aid)
        if type(batch) is not int or not 1 <= batch <= 50 or batch % per_family:
            raise ValueError("Invalid bounded batch_size in " + aid)
        lesson = lessons.get(allocation.get("lesson_id"))
        if lesson is None:
            raise ValueError("Missing lesson for " + aid)
        if lesson.get("pairs_per_family", per_family) != per_family:
            raise ValueError("Lesson/family cardinality mismatch in " + aid)
        if per_family == 2 and len(lesson.get("family_class_values", [])) != 2:
            raise ValueError("Two-row families need two explicit class values: " + aid)
        if per_family == 2 and not lesson.get("family_class_field"):
            raise ValueError("Two-row families need a family_class_field: " + aid)
        schedule = lesson.get("class_schedule", [])
        reserved = {"origin_family_id", "learning_objective_id", "scenario_group", "review_status",
                    "measured_training_result", "leakage_group_ids", "language_pair", "canonical_source", "source_raw"}
        if not isinstance(schedule, list) or any(not isinstance(cell, dict) or not cell or set(cell) & reserved for cell in schedule):
            raise ValueError("class_schedule must contain explicit, nonempty metadata class cells: " + aid)
        if not allocation.get("scenario_group"):
            raise ValueError("A concrete shared scenario_group is required: " + aid)
        labels = lesson.get("route_labels")
        if labels and (len(labels) != len(set(labels)) or len(labels) < 2 or per_family != 1):
            raise ValueError("Router requires unique assigned classes and one row per family")
        totals[allocation["domain_id"]] += target
    if totals != Counter({d["domain_id"]: d["target_pairs"] for d in domains}):
        raise ValueError("Allocation sums must equal each domain's registry quota")
    registry["allocations"] = allocations
    return registry, lessons


def paths_for(registry, work_root=None):
    if work_root is None:
        if os.name != "nt":
            raise ValueError("On non-Windows supply an explicit absolute --work-root for this run. The assigned Windows data root is " + registry["canonical_windows_learning_sets_root"] + "; never create a relative C: directory in cwd.")
        work_root = registry["canonical_windows_learning_sets_root"]
    if os.name != "nt" and re.match(r"^[A-Za-z]:[\\/]", str(work_root)):
        raise ValueError("A Windows root cannot be materialized on this host; supply an explicit absolute local --work-root")
    work_path = Path(work_root).expanduser()
    if not work_path.is_absolute():
        raise ValueError("--work-root must be an explicit absolute path")
    work = work_path.resolve()
    root = work / "campaigns" / registry["campaign_id"]
    return work, root, root / "state.json"


def initial_state(registry, work):
    return {"schema_version": STATE_SCHEMA, "campaign_id": registry["campaign_id"],
            "mode": MODE, "work_root": str(work), "created_at": now(),
            "cursor_order": 1, "active_request": None, "pending_requests": {},
            "allocation_progress": {}, "completed_requests": {},
            "legacy_pairs_counted": 0, "coordination_version": 1,
            "claims": {}, "family_claims": {}, "active_requests": {},
            "agent_cursors": {}, "quality_flags": {}, "legacy_pending_requests": [],
            "legacy_policy": "Old requests are preserved and are not automatically counted or restarted."}


def load_state(registry, work, state_path, create=False):
    if state_path.exists():
        state = read(state_path)
        if state.get("schema_version") != STATE_SCHEMA or state.get("campaign_id") != registry["campaign_id"]:
            raise ValueError("State belongs to another campaign: " + str(state_path))
        if Path(state.get("work_root", "")).resolve() != work:
            raise ValueError("State work_root differs; do not relocate a saved run implicitly")
        normalize_coordination(state)
        seed_quality_flags(registry, state)
        return state
    if state_path.parent.exists() and any(state_path.parent.iterdir()):
        raise ValueError("Campaign files exist but state.json is missing. Restore this exact state; no new run started: " + str(state_path))
    state = initial_state(registry, work)
    seed_quality_flags(registry, state)
    if create:
        atomic_text(state_path, json_text(state))
    return state


def save_state(state_path, state, registry=None):
    state["updated_at"] = now()
    state["revision"] = state.get("revision", 0) + 1
    atomic_text(state_path, json_text(state))
    if registry is not None:
        atomic_text(Path(state_path).with_name("status.json"), json_text(status_result(registry, state, state_path)))


def command(script, *arguments, python=None):
    parts = [str(python or sys.executable), str(script)] + [str(a) for a in arguments]
    if os.name == "nt":
        return "& " + " ".join("'" + value.replace("'", "''") + "'" for value in parts)
    return shlex.join(parts)


def normalize_coordination(state):
    if not state.get("coordination_version"):
        # Old pending attempts remain unclaimed until an explicit exact resume.
        state["legacy_pending_requests"] = list(state.get("pending_requests", {}).values())
        state["legacy_active_request"] = state.get("active_request")
        state["coordination_version"] = 1
    for key in ("claims", "family_claims", "active_requests", "agent_cursors", "quality_flags"):
        state.setdefault(key, {})
    state.setdefault("legacy_pending_requests", [])
    # The previous scalar is retained as a deprecated field, never as authority.
    state["active_request"] = None


def pool_for(domain_id):
    if domain_id.startswith("tr.") or domain_id == "text.typo-normalization":
        return "translations"
    if domain_id.startswith(("or.", "prompt.")):
        return "orchestration"
    if domain_id.startswith(("docs.", "code.")):
        return "software"
    return "unassigned"


def agent_id(args):
    value = getattr(args, "agent_id", None)
    if not isinstance(value, str) or not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9._:-]{0,95}", value):
        raise ValueError("Supply --agent-id with a unique ID for this Kiro session (letters, digits, dot, underscore, colon or hyphen). Reuse it only to resume that same session's saved claim.")
    return value


def seed_quality_flags(registry, state):
    seed = BASE / "campaign" / "quality-status-seed.json"
    if not seed.is_file():
        return
    payload = read(seed)
    if payload.get("schema_version") != "xiigen.campaign-quality-seed.v1":
        raise ValueError("Unknown quality-status seed schema")
    domains = {d["domain_id"] for d in registry["domains"]}
    for source in payload.get("findings", []):
        flag = dict(source)
        key = flag.get("flag_id")
        if not key or flag.get("domain_id") not in domains or flag.get("status") not in {"needs_rework", "needs_assessment", "resolved"}:
            raise ValueError("Invalid domain/status in quality-status seed: " + str(key))
        if key not in state["quality_flags"]:
            flag["recorded_by"] = "preparation-assessment"
            state["quality_flags"][key] = flag


def quality_status(flags):
    values = {f["status"] for f in flags}
    return "needs_rework" if "needs_rework" in values else "needs_assessment" if "needs_assessment" in values else "not_established"


def status_result(registry, state, state_path):
    normalize_coordination(state)
    counts = Counter()
    topics = []
    flags = list(state["quality_flags"].values())
    for allocation in registry["allocations"]:
        aid = allocation["allocation_id"]
        amount = state["allocation_progress"].get(aid, 0)
        if type(amount) is not int or not 0 <= amount <= allocation["target_pairs"]:
            raise ValueError("Progress exceeds assigned quota: " + aid)
        counts[allocation["domain_id"]] += amount
        matching = [f for f in flags if f["domain_id"] == allocation["domain_id"]
                    and (not f.get("topic_id") or f["topic_id"] in {allocation["topic_id"], allocation.get("objective_id")})
                    and (not f.get("profile_id") or f["profile_id"] == allocation["profile_id"])]
        claim = state["claims"].get(allocation["domain_id"])
        claim = claim if claim and claim["allocation_id"] == aid else None
        topics.append({"allocation_id": aid, "domain_id": allocation["domain_id"],
                       "topic_id": allocation["topic_id"], "objective_id": allocation.get("objective_id"),
                       "profile_id": allocation["profile_id"], "target_pairs": allocation["target_pairs"],
                       "draft_created_pairs": amount, "remaining_pairs": allocation["target_pairs"] - amount,
                       "owner_id": claim["owner_id"] if claim else None,
                       "request": claim["request"] if claim else state["pending_requests"].get(aid),
                       "authoring_rows_saved": claim.get("created_rows", 0) if claim else 0,
                       "work_status": "in_progress" if claim else "draft_created" if amount == allocation["target_pairs"] else "pending",
                       "quality_status": quality_status(matching), "quality_flag_ids": [f["flag_id"] for f in matching]})
    total = sum(counts.values())
    target = registry["total_target_pairs"]
    domains = []
    for d in sorted(registry["domains"], key=lambda item: item["order"]):
        domain = d["domain_id"]
        matching = [f for f in flags if f["domain_id"] == domain]
        claim = state["claims"].get(domain)
        domains.append({"domain_id": domain, "pool": pool_for(domain), "target_pairs": d["target_pairs"],
                        "draft_created_pairs": counts[domain], "remaining_pairs": d["target_pairs"] - counts[domain],
                        "domain_complete": counts[domain] == d["target_pairs"],
                        "owner_id": claim["owner_id"] if claim else None,
                        "active_request": claim["request"] if claim else None,
                        "authoring_rows_saved": claim.get("created_rows", 0) if claim else 0,
                        "work_status": "in_progress" if claim else "draft_created" if counts[domain] == d["target_pairs"] else "pending",
                        "quality_status": quality_status(matching), "quality_flag_ids": [f["flag_id"] for f in matching]})
    return {"schema_version": "xiigen.campaign-shared-status.v1",
            "status": "CAMPAIGN_DRAFTS_CREATED" if total == target else "IN_PROGRESS",
            "campaign_id": registry["campaign_id"], "mode": MODE, "revision": state.get("revision", 0),
            "updated_at": state.get("updated_at", state.get("created_at")),
            "target_pairs": target, "draft_created_pairs": total,
            "remaining_pairs": target - total, "active_request": None,
            "active_requests": state["active_requests"], "domains": domains, "topics": topics,
            "quality_flags": flags,
            "state_path": str(state_path), "status_path": str(Path(state_path).with_name("status.json")),
            "authority": "state.json is authoritative; status.json is regenerated from it under the same lock",
            "legacy_pairs_counted": 0, "review_status": "not_requested", "student": "NOT_RUN",
            "learning_metrics": "NOT_MEASURED"}


def require_claim(state, request, args):
    owner = agent_id(args)
    claim = state["claims"].get(request["domain_id"])
    if not claim or claim.get("request") != request["paths"]["request"] or claim.get("owner_id") != owner:
        raise ValueError("This request is not owned by this agent. Read shared status; do not generate or finish another session's work.")
    if getattr(args, "claim_token", None) != claim["token"]:
        raise ValueError("Missing or stale --claim-token. Use the exact saved coordination-commands.md; no takeover or progress change was performed.")
    if state["active_requests"].get(owner) != claim["request"]:
        raise ValueError("This is not the agent's selected request. Direct NEXT has priority; explicitly resume the saved request before editing it.")
    return claim


def release_claim(state, claim):
    domain = claim["domain_id"]
    if state["claims"].get(domain, {}).get("token") != claim["token"]:
        raise ValueError("Claim changed; no ownership was released")
    state["claims"].pop(domain)
    if state["active_requests"].get(claim["owner_id"]) == claim["request"]:
        state["active_requests"].pop(claim["owner_id"])
    for fid, owner in list(state["family_claims"].items()):
        if owner["request"] == claim["request"] and owner["token"] == claim["token"]:
            state["family_claims"].pop(fid)


def acquire_claim(state, request, owner, pool, root):
    domain = request["domain_id"]
    current = state["claims"].get(domain)
    if current:
        if current["owner_id"] == owner and current["request"] == request["paths"]["request"]:
            return current
        raise ValueError("Domain is already claimed by " + current["owner_id"])
    ledger = load_ledger(root / "family-material.jsonl")
    missing = [fid for fid in request["family_ids"] if fid not in ledger]
    for fid in missing:
        other = state["family_claims"].get(fid)
        if other and other["request"] != request["paths"]["request"]:
            raise ValueError("Shared family material is being authored by " + other["owner_id"] + "; choose another free domain until that batch is finished: " + fid)
    claim = {"domain_id": domain, "allocation_id": request["allocation_id"],
             "request": request["paths"]["request"], "owner_id": owner, "pool": pool,
             "token": uuid.uuid4().hex, "claimed_at": now(), "updated_at": now(),
             "created_rows": 0, "status": "in_progress"}
    state["claims"][domain] = claim
    state["active_requests"][owner] = claim["request"]
    for fid in missing:
        state["family_claims"][fid] = {"request": claim["request"], "owner_id": owner, "token": claim["token"]}
    state["legacy_pending_requests"] = [p for p in state["legacy_pending_requests"] if p != claim["request"]]
    return claim


def coordinated_request(request, claim):
    # Saved request/draft/material are never rewritten. These are replaceable
    # views of the current owner and exact commands, with original paths retained.
    result = json.loads(json.dumps(request, ensure_ascii=False))
    paths = result["paths"]
    paths["original_prompt"] = paths["prompt"]
    paths["original_commands"] = paths["commands"]
    paths["original_material_required"] = paths["material_required"]
    directory = Path(paths["request"]).parent
    paths["prompt"] = str(directory / "coordination-prompt.md")
    paths["commands"] = str(directory / "coordination-commands.md")
    paths["material_required"] = str(directory / "coordination-material.required.json")
    paths["shared_status"] = str(Path(paths["state"]).with_name("status.json"))
    result["coordination"] = {"owner_id": claim["owner_id"], "pool": claim["pool"],
                              "claim_token": claim["token"], "shared_status": paths["shared_status"]}
    result["examples"] = selected_examples({"lesson_id": request["lesson_id"], "examples": request["examples"]},
                                           request["profile_id"], request["domain_id"], request["objective_id"])
    result["guidance_scope"] = "Current exact-objective authoring examples; saved row slots, objective, class schedule and original request are unchanged."
    ledger = load_ledger(Path(paths["campaign_root"]) / "family-material.jsonl")
    result["family_material"] = {fid: ledger[fid] for fid in request["family_ids"] if fid in ledger}
    result["missing_family_ids"] = [fid for fid in request["family_ids"] if fid not in ledger]
    script = paths["tools_script"]
    identity = ("--agent-id", claim["owner_id"], "--claim-token", claim["token"])
    result["commands"] = {
        "finish": command(script, "finish", "--request", paths["request"], "--input", paths["draft"], "--family-material", paths["family_material"], "--model", "unknown", *identity),
        "next": command(script, "next", "--work-root", paths["learning_sets_root"], "--agent-id", claim["owner_id"], "--pool", claim["pool"]),
        "resume": command(script, "resume", "--request", paths["request"], *identity),
        "progress": command(script, "progress", "--request", paths["request"], *identity),
        "release": command(script, "release", "--request", paths["request"], *identity),
        "status": command(script, "status", "--work-root", paths["learning_sets_root"])}
    return result


def publish_coordination(request, claim):
    result = coordinated_request(request, claim)
    ensure_artifacts(result, refresh_coordination=True)
    atomic_text(Path(request["paths"]["request"]).with_name("coordination.json"), json_text(result["coordination"]))
    return result


def load_ledger(path):
    records = {}
    for record in read_rows(path, optional=True):
        fid = record.get("family_id")
        if not isinstance(fid, str) or fid in records:
            raise ValueError("Invalid or duplicate family in " + str(path))
        records[fid] = record
    return records


def selected_examples(lesson, profile, domain_id=None, objective_id=None):
    def matches(example):
        value = example.get("profile_id") or example.get("metadata", {}).get("profile_id")
        pair = example.get("metadata", {}).get("language_pair")
        if not value and isinstance(pair, dict) and pair.get("source") and pair.get("target"):
            value = pair["source"] + "-" + pair["target"]
        return value is None or value == profile
    originals = [e for e in lesson.get("examples", []) if matches(e)]
    if len(originals) < 2:
        raise ValueError("Lesson needs at least two examples for the assigned profile: " + lesson["lesson_id"] + "/" + profile)
    supplements = []
    if domain_id and objective_id:
        for name in ("translation-quality-lessons-2026-10-26.json", "software-quality-lessons-2026-10-26.json"):
            path = BASE / "campaign" / name
            if not path.is_file():
                continue
            payload = read(path)
            candidates = list(payload.get("examples", []))
            candidates.extend(example for group in payload.get("domains", []) for example in group.get("examples", []))
            for candidate in candidates:
                if (candidate.get("domain_id"), candidate.get("objective_id"), candidate.get("profile_id")) != (domain_id, objective_id, profile):
                    continue
                example = dict(candidate)
                example["rationale"] = example.get("rationale") or example.get("simple_check") or example.get("simple_check_en")
                example["simple_check"] = example.get("simple_check") or example.get("simple_check_en")
                example["supplement_path"] = "campaign/" + name
                supplements.append(example)
    retained = originals[:2]
    seen = {(example["prompt"], example["chosen"], example["rejected"]) for example in retained}
    distinct = []
    for example in supplements:
        identity = (example["prompt"], example["chosen"], example["rejected"])
        if identity not in seen:
            distinct.append(example)
            seen.add(identity)
    examples = retained + distinct[:2] if distinct else originals[:3]
    for example in examples:
        for field in ("prompt", "chosen", "rejected", "rationale"):
            meaningful(example.get(field), "example." + field)
    return examples


def build_request(registry, lesson, allocation, state, work, root, run_dir):
    aid = allocation["allocation_id"]
    start = state["allocation_progress"].get(aid, 0)
    remaining = allocation["target_pairs"] - start
    count = min(allocation["batch_size"], remaining)
    per_family = allocation.get("pairs_per_family", 1)
    if count <= 0 or count % per_family:
        raise ValueError("No complete family fits the remaining allocation")
    group = allocation["scenario_group"]
    families = [registry["campaign_id"] + ":" + group + ":F" + str(i + 1).zfill(5)
                for i in range(start // per_family, (start + count) // per_family)]
    ledger = load_ledger(root / "family-material.jsonl")
    existing = {fid: ledger[fid] for fid in families if fid in ledger}
    for fid, material in existing.items():
        if material.get("scenario_group") != group or material.get("origin_family_id") != fid:
            raise ValueError("Family material conflicts with its declared scenario group: " + fid)
    labels = lesson.get("route_labels", [])
    route_counts = Counter()
    slots = []
    classes = lesson.get("family_class_values", [])
    schedule = lesson.get("class_schedule", [])
    for offset in range(count):
        ordinal = start + offset
        fid = families[offset // per_family]
        slot = {"pair_id": registry["campaign_id"] + ":" + aid + ":P" + str(ordinal + 1).zfill(5),
                "family_id": fid, "origin_family_id": fid, "leakage_group_ids": [fid]}
        if per_family == 2:
            slot["family_class"] = classes[offset % per_family]
        if labels:
            slot["route_class"] = labels[ordinal % len(labels)]
            route_counts[slot["route_class"]] += 1
        if schedule:
            slot["metadata_assignments"] = schedule[ordinal % len(schedule)]
        slots.append(slot)
    examples = selected_examples(lesson, allocation["profile_id"], allocation["domain_id"], allocation.get("objective_id") or allocation["topic_id"])
    excerpts = lesson.get("source_excerpts", [])[:2]
    if not excerpts:
        raise ValueError("Lesson has no supplied source excerpts: " + lesson["lesson_id"])
    source_paths = {}
    source_refs = allocation.get("source_refs") or lesson["source_refs"]
    for source_id in source_refs:
        source = (BASE / "rag" / "sources" / (source_id + ".md")).resolve()
        if not source.is_file():
            raise ValueError("Missing exact assigned source: " + str(source) + ". Restore that file; do not invent a source or search another checkout.")
        source_paths[source_id] = str(source)
    for excerpt in excerpts:
        meaningful(excerpt.get("excerpt"), "source excerpt")
        source = (BASE / excerpt["path"]).resolve()
        if not source.is_file():
            raise ValueError("Missing exact assigned source: " + str(source) + ". Restore that file; do not invent a source or search another checkout.")
        source_paths.setdefault(excerpt["source_id"], str(source))
    domain_target = next(d["target_pairs"] for d in registry["domains"] if d["domain_id"] == allocation["domain_id"])
    domain_created = sum(state["allocation_progress"].get(a["allocation_id"], 0)
                         for a in registry["allocations"] if a["domain_id"] == allocation["domain_id"])
    paths = {"preparation_root": str(BASE), "learning_sets_root": str(work),
             "campaign_root": str(root), "state": str(root / "state.json"),
             "family_ledger": str(root / "family-material.jsonl"),
             "request": str(run_dir / "request.json"), "prompt": str(run_dir / "prompt.md"),
             "draft": str(run_dir / "draft.jsonl"), "commands": str(run_dir / "commands.md"),
             "material_required": str(run_dir / "material.required.json"),
             "family_material": str(run_dir / "family-material.jsonl"),
             "creation_report": str(run_dir / "creation.json"),
             "tools_script": str(Path(__file__).resolve()), "python_executable": sys.executable,
             "sources": source_paths}
    request = {"schema_version": REQUEST_SCHEMA, "campaign_id": registry["campaign_id"],
               "mode": MODE, "created_at": now(), "allocation_id": aid,
               "allocation_order": allocation["order"], "stream_id": allocation["stream_id"],
               "domain_id": allocation["domain_id"], "topic_id": allocation["topic_id"],
               "objective_id": allocation.get("objective_id") or allocation["topic_id"],
               "profile_id": allocation["profile_id"], "lesson_id": allocation["lesson_id"],
               "split": "train", "start_pair": start, "count": count,
               "domain_target_pairs": domain_target, "domain_created_before_batch": domain_created,
               "domain_remaining_after_batch": domain_target - domain_created - count,
               "allocation_target_pairs": allocation["target_pairs"],
               "allocation_remaining_after_batch": remaining - count,
               "batch_number": start // allocation["batch_size"] + 1,
               "batch_count": (allocation["target_pairs"] + allocation["batch_size"] - 1) // allocation["batch_size"],
               "pairs_per_family": per_family, "scenario_group": group,
               "family_source_languages": sorted({a["profile_id"].split("-")[0] for a in registry["allocations"]
                                                   if a["scenario_group"] == group and a["domain_id"].startswith("tr.")}),
               "family_material_kind": lesson.get("family_material_kind", "canonical_translation" if allocation["domain_id"].startswith("tr.") else "specification"),
               "family_ids": families, "family_material": existing,
               "missing_family_ids": [fid for fid in families if fid not in existing],
               "row_slots": slots, "family_class_field": lesson.get("family_class_field"),
               "family_class_values": classes, "route_labels": labels,
               "class_schedule": schedule,
               "route_class_counts": dict(route_counts),
               "route_total_target_counts": dict(Counter(labels[i % len(labels)] for i in range(allocation["target_pairs"]))) if labels else {},
               "source_refs": source_refs,
               "next_allocation_id": allocation.get("next_allocation_id"),
               "paths": paths,
               "subject": {key: lesson.get(key) for key in ("goal_ru", "instructions", "answer_contract", "hard_constraints", "scenario_axes", "authoring_checks_en")},
               "examples": examples,
               "source_excerpts": excerpts,
               "model_policy": "Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.",
               "language_policy": "Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.",
               "legacy_policy": "Old requests are not counted or restarted. An explicitly selected old request must use resume with its exact path."}
    request["commands"] = {
        "finish": command(paths["tools_script"], "finish", "--request", paths["request"],
                          "--input", paths["draft"], "--family-material", paths["family_material"], "--model", "unknown"),
        "next": command(paths["tools_script"], "next", "--work-root", work),
        "resume": command(paths["tools_script"], "resume", "--request", paths["request"]),
        "status": command(paths["tools_script"], "status", "--work-root", work)}
    return request


def render_prompt(request):
    p = request["paths"]
    visible_slots = [{key: slot[key] for key in ("pair_id", "family_id", "family_class", "route_class", "metadata_assignments") if key in slot}
                     for slot in request["row_slots"]]
    slot_text = "[\n" + ",\n".join(json.dumps(slot, ensure_ascii=False, separators=(",", ":")) for slot in visible_slots) + "\n]"
    blocks = ["# Create the assigned DPO batch\n",
              request["language_policy"] + "\n" + request["model_policy"],
              "You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.",
              "Current task: " + request["stream_id"] + "; domain " + request["domain_id"] + "; profile " + request["profile_id"] + ".",
              f"Write exactly {request['count']} rows, batch {request['batch_number']}/{request['batch_count']}. Domain quota is 1000 total, shared by its allocations. Before this batch: {request['domain_created_before_batch']}; after it, domain remainder: {request['domain_remaining_after_batch']}. Finishing this batch does not by itself finish the domain.",
              "## The lesson\n\n" + json_text(request["subject"]),
              "## Assigned row slots\n\n```json\n" + slot_text + "\n```",
              "Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID."]
    if request.get("coordination"):
        owner = request["coordination"]
        blocks.append("## Shared work ownership\n\nThis request is claimed by agent " + owner["owner_id"] + " in pool " + owner["pool"] + ". Shared matrix: " + owner["shared_status"] + ". Use only the owner commands in " + p["commands"] + "; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.\n\nAfter saving complete JSONL lines, record actual partial progress with:\n```\n" + request["commands"]["progress"] + "\n```\nThe command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.")
    if request["family_class_field"]:
        blocks.append("Set metadata." + request["family_class_field"] + " to the exact slot family_class. Produce each assigned class once for that family; do not invent a class mixture.")
    if request["class_schedule"]:
        blocks.append("Copy each slot's metadata_assignments into that row's metadata exactly, and author the scenario to match those assigned classes. The schedule is fixed by the lesson; do not replace it with your own balancing rule.")
    if request["route_labels"]:
        blocks.append("Set metadata.route_class to the slot route_class. chosen must be the corresponding JSON action/domain_id; rejected must be another permitted label. Assigned class counts: " + json.dumps(request["route_class_counts"], ensure_ascii=False) + ". Final domain class totals: " + json.dumps(request["route_total_target_counts"], ensure_ascii=False) + ". Use exactly request.count rows, including a shorter final batch; never round it to the number of labels.")
    blocks += ["## Concrete family material\n\nExisting facts, reused without changing their meaning:\n```json\n" + json_text(request["family_material"]) + "```",
               "For missing families, read " + p["material_required"] + " and author real records in " + p["family_material"] + ". A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger."]
    if request["domain_id"].startswith("tr."):
        kind = request["family_material_kind"]
        if kind == "layout_counterfactual":
            blocks.append("Material kind layout_counterfactual: write source_text_by_class_language={restore:{en:text,ru:text,he:text},preserve:{en:text,ru:text,he:text}}. Restore contains the decoded ordinary source text; preserve contains the exact RAW identifier source text. These canonical values differ by class. Both learner inputs use the same RAW source within the pair; their visible contexts establish ordinary word versus literal identifier. metadata.canonical_source uses the current class and source language from this material.")
        elif kind == "source_ambiguity":
            blocks.append("Material kind source_ambiguity: source_text_by_language must contain the assigned source language(s) " + json.dumps(request["family_source_languages"]) + ". Also write sense_translations as an object mapping at least two distinct sense IDs to {en:text,ru:text,he:text}. Keep the same ambiguous source input in both classes; the resolved class adds only the visible context. Do not assume the translated words retain the same ambiguity. metadata.canonical_source uses source_text_by_language[source].")
        else:
            blocks.append("For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].")
        blocks.append("The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.")
        if request["pairs_per_family"] == 2:
            blocks.append("Also save learner_visible_context_by_class as {class:{language:context}} for both assigned classes, covering these source languages: " + json.dumps(request["family_source_languages"]) + ". Empty context is allowed when missing context is the assigned condition. Contexts of the two classes must differ. Supply only that class's source-language context to the learner, not the entire author-only facts record. A contrast must be justified in the assigned source language, not inferred from translations.")
    else:
        blocks.append("For a new family supply its concrete facts/specification. Reuse an existing DOC/CODE family only when its assigned scenario_group explicitly joins those tasks. Different unrelated specifications must not share a family ID.")
    blocks += ["## Row format\n\nUse schema_version=xiigen.dpo.v1. Required fields: " + ", ".join(ROW_FIELDS) + ".\n"
               + "Set domain_id=" + request["domain_id"] + ", topic_id=" + request["topic_id"] + ", profile_id=" + request["profile_id"] + ", split=train. metadata.learning_objective_id=" + request["objective_id"] + "; metadata.scenario_group=" + request["scenario_group"] + "; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: " + json.dumps(request["source_refs"], ensure_ascii=False) + ". provenance needs kind and generator_model (actual observed identity or unknown).",
               "chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.",
               "## Worked examples — learn the contrast, do not copy these rows\n\n```json\n" + json_text(request["examples"]) + "```",
               "## Assigned source excerpts\n\nThese are bounded, partial source excerpts; they are not a claim to include the entire original or all its requirements. Use the exact linked local file if a detail needed by this lesson is absent.\n\n" + "\n\n".join(x["source_id"] + " — " + x["path"] + "\n" + x["excerpt"] for x in request["source_excerpts"]),
               "## Create, finish, continue\n\nWrite the exact JSONL rows to " + p["draft"] + ". Then execute:\n\n```\n" + request["commands"]["finish"] + "\n```\n\nAfter successful DRAFT_CREATED execute the supplied next command in the same task:\n\n```\n" + request["commands"]["next"] + "\n```",
               "CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.",
               "Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content."]
    return "\n\n".join(blocks) + "\n"


def ensure_artifacts(request, refresh_coordination=False):
    p = request["paths"]
    required_keys = list(MATERIAL_FIELDS)
    translation_languages = []
    if request["domain_id"].startswith("tr."):
        if request["family_material_kind"] == "layout_counterfactual":
            required_keys.append("source_text_by_class_language")
            translation_languages = ["en", "ru", "he"]
        else:
            required_keys.append("source_text_by_language")
            translation_languages = request["family_source_languages"] if request["family_material_kind"] == "source_ambiguity" else ["en", "ru", "he"]
            if request["family_material_kind"] == "source_ambiguity":
                required_keys.append("sense_translations")
        if request["pairs_per_family"] == 2:
            required_keys.append("learner_visible_context_by_class")
    required = {"status": "AUTHORING_ASSIGNMENT_NOT_FAMILY_FACTS", "scenario_group": request["scenario_group"],
                "missing_family_ids": request["missing_family_ids"],
                "required_keys": required_keys,
                "family_material_kind": request["family_material_kind"],
                "translation_languages": translation_languages,
                "source_context_languages": request["family_source_languages"],
                "learner_visible_context_classes": request["family_class_values"] if request["domain_id"].startswith("tr.") and request["pairs_per_family"] == 2 else [],
                "output_path": p["family_material"],
                "instruction": "Author real scenario facts. This file contains assignments only and cannot be submitted as family material."}
    files = {p["prompt"]: render_prompt(request), p["material_required"]: json_text(required),
             p["commands"]: "# Commands for this saved request\n\n" + "\n\n".join("## " + key + "\n\n```\n" + value + "\n```" for key, value in request["commands"].items()) + "\n"}
    for path, text in files.items():
        if refresh_coordination:
            if not Path(path).name.startswith("coordination-"):
                raise ValueError("Only derived coordination views can be refreshed")
            atomic_text(path, text)
        elif not Path(path).exists():
            write_new(path, text)


def request_result(request, status="PREPARED", creation=None):
    result = {"status": status, "mode": MODE, "allocation_id": request["allocation_id"],
              "stream_id": request["stream_id"], "domain_id": request["domain_id"],
              "count": request["count"], "batch_number": request["batch_number"],
              "batch_count": request["batch_count"], "paths": request["paths"],
              "commands": request["commands"], "generation": "NOT_RUN_BY_THIS_TOOL",
              "candidate_file_exists": Path(request["paths"]["draft"]).is_file(),
              "legacy_policy": request["legacy_policy"]}
    if request.get("coordination"):
        result["coordination"] = request["coordination"]
    if creation:
        result["creation"] = creation
    return result


def completed_result(request, report, caller, pool=None):
    """Read-only completion plus a fresh assignment command for this caller."""
    result = request_result(request, "DRAFT_CREATED", report)
    paths = request["paths"]
    result.pop("coordination", None)
    result["action"] = "COMPLETED_REQUEST_READ_ONLY"
    result["creation_owner_id"] = report.get("owner_id")
    result["continuation_owner_id"] = caller
    result["commands"] = {
        "next": command(paths["tools_script"], "next", "--work-root", paths["learning_sets_root"],
                        "--agent-id", caller, "--pool", pool or pool_for(request["domain_id"])),
        "status": command(paths["tools_script"], "status", "--work-root", paths["learning_sets_root"])}
    result["paths"] = {key: paths[key] for key in ("request", "draft", "creation_report", "state", "campaign_root", "learning_sets_root") if key in paths}
    result["paths"]["shared_status"] = str(Path(paths["state"]).with_name("status.json"))
    return result


def commit_creation(state, state_path, request, report, claim, registry):
    key = request["paths"]["request"]
    aid = request["allocation_id"]
    if key not in state["completed_requests"]:
        current = state["allocation_progress"].get(aid, 0)
        if current != request["start_pair"]:
            raise ValueError("Saved request does not start at the current allocation cursor")
        state["allocation_progress"][aid] = current + request["count"]
        state["completed_requests"][key] = report
    state["pending_requests"].pop(aid, None)
    release_claim(state, claim)
    save_state(state_path, state, registry)


def next_batch(args):
    registry, lessons = config()
    work, root, state_path = paths_for(registry, args.work_root)
    owner = agent_id(args)
    pool = getattr(args, "pool", "all")
    if pool not in {"translations", "orchestration", "software", "all"}:
        raise ValueError("Unknown pool; use translations, orchestration, software or all")
    with transaction(work, registry["campaign_id"]):
        state = load_state(registry, work, state_path, create=True)
        return next_locked(args, registry, lessons, work, root, state_path, state, owner, pool)


def next_locked(args, registry, lessons, work, root, state_path, state, owner, pool):
    allocations = registry["allocations"]
    requested = getattr(args, "allocation", None)
    current_path = state["active_requests"].get(owner)
    if requested:
        selected = next((a for a in allocations if a["allocation_id"] == requested), None)
        if selected is None:
            raise ValueError("Unknown explicit allocation ID: " + requested)
        if pool != "all" and pool_for(selected["domain_id"]) != pool:
            raise ValueError("Explicit allocation is outside the assigned pool")
        if current_path:
            current = read(current_path)
            claim = state["claims"].get(current["domain_id"])
            if claim and claim["allocation_id"] != requested:
                # A direct NEXT replaces this agent's old assignment only.
                release_claim(state, claim)
                state.setdefault("pauses", []).append({"request": current_path, "owner_id": owner,
                                                        "reason": "explicit NEXT", "at": now()})
        state["agent_cursors"][owner] = selected["order"]
        save_state(state_path, state, registry)
        if current_path and state["active_requests"].get(owner) == current_path:
            request = read(current_path)
            claim = state["claims"].get(request["domain_id"])
            if claim and claim["owner_id"] == owner and claim["allocation_id"] == requested:
                marker = Path(request["paths"]["creation_report"])
                if marker.exists():
                    report = read(marker)
                    recovery = argparse.Namespace(request=current_path, input=report.get("input_path"),
                                                  family_material=None, model=report.get("generator_model", "unknown"),
                                                  agent_id=owner, claim_token=claim["token"])
                    finish_locked(recovery, request, registry, work, root, state_path, state)
                else:
                    ensure_artifacts(request)
                    effective = publish_coordination(request, claim)
                    result = request_result(effective)
                    result["action"] = "RESUME_OWNED_REQUEST"
                    return result
    elif current_path:
        request = read(current_path)
        claim = state["claims"].get(request["domain_id"])
        if not claim or claim["owner_id"] != owner or claim["request"] != current_path:
            raise ValueError("Agent active request and shared claim disagree; no new request created")
        if pool != "all" and pool_for(request["domain_id"]) != pool:
            raise ValueError("This agent already owns another pool's request. Resume or explicitly release it first.")
        marker = Path(request["paths"]["creation_report"])
        if marker.exists():
            report = read(marker)
            recovery = argparse.Namespace(request=current_path, input=report.get("input_path"),
                                          family_material=None, model=report.get("generator_model", "unknown"),
                                          agent_id=owner, claim_token=claim["token"])
            finish_locked(recovery, request, registry, work, root, state_path, state)
        else:
            ensure_artifacts(request)
            effective = publish_coordination(request, claim)
            save_state(state_path, state, registry)
            result = request_result(effective)
            result["action"] = "RESUME_OWNED_REQUEST"
            return result
    blocked = []
    for allocation in allocations:
        if allocation["order"] < state["agent_cursors"].get(owner, 1):
            continue
        if pool != "all" and pool_for(allocation["domain_id"]) != pool:
            continue
        if requested and allocation["allocation_id"] != requested:
            continue
        aid = allocation["allocation_id"]
        created = state["allocation_progress"].get(aid, 0)
        if created >= allocation["target_pairs"]:
            continue
        current = state["claims"].get(allocation["domain_id"])
        if current:
            blocked.append({"allocation_id": aid, "domain_id": allocation["domain_id"],
                            "reason": "domain_owned", "owner_id": current["owner_id"], "request": current["request"]})
            continue
        pending = state["pending_requests"].get(aid)
        if pending in state["legacy_pending_requests"]:
            blocked.append({"allocation_id": aid, "domain_id": allocation["domain_id"],
                            "reason": "legacy_request_requires_explicit_resume", "request": pending})
            continue
        if pending:
            request = read(pending)
        else:
            run_dir = root / aid / ("B" + str(created // allocation["batch_size"] + 1).zfill(4))
            request_path = run_dir / "request.json"
            if request_path.exists():
                request = read(request_path)
                if request.get("allocation_id") != aid or request.get("start_pair") != created or request.get("schema_version") != REQUEST_SCHEMA:
                    raise ValueError("Existing run conflicts with the current cursor; preserve it: " + str(run_dir))
            else:
                if run_dir.exists() and any(run_dir.iterdir()):
                    raise ValueError("Existing run has files but no request. Do not overwrite it: " + str(run_dir))
                request = build_request(registry, lessons[allocation["lesson_id"]], allocation, state, work, root, run_dir)
        ledger = load_ledger(root / "family-material.jsonl")
        conflicts = [state["family_claims"][fid] for fid in request["family_ids"]
                     if fid not in ledger and fid in state["family_claims"]
                     and state["family_claims"][fid]["request"] != request["paths"]["request"]]
        if conflicts:
            blocked.append({"allocation_id": aid, "domain_id": allocation["domain_id"],
                            "reason": "shared_family_owned", "owner_id": conflicts[0]["owner_id"],
                            "request": conflicts[0]["request"]})
            continue
        request_path = Path(request["paths"]["request"])
        if not request_path.exists():
            write_new(request_path, json_text(request))
        state["pending_requests"][aid] = str(request_path)
        claim = acquire_claim(state, request, owner, pool, root)
        ensure_artifacts(request)
        effective = publish_coordination(request, claim)
        save_state(state_path, state, registry)
        result = request_result(effective)
        result["action"] = "RESUME_SAVED_REQUEST" if Path(request["paths"]["draft"]).exists() else "AUTHOR_ASSIGNED_BATCH"
        result["coordination"] = effective["coordination"]
        result["skipped_owned_allocations"] = blocked
        return result
    save_state(state_path, state, registry)
    result = status_result(registry, state, state_path)
    eligible = [d for d in result["domains"] if pool == "all" or d["pool"] == pool]
    remaining = sum(d["remaining_pairs"] for d in eligible)
    result["pool"] = pool
    result["pool_remaining_pairs"] = remaining
    if not remaining:
        result["status"] = "CAMPAIGN_DRAFTS_CREATED" if not result["remaining_pairs"] else "POOL_DRAFTS_CREATED"
    elif blocked:
        result["status"] = "WAITING_FOR_OWNERS" if all(x["reason"] != "legacy_request_requires_explicit_resume" for x in blocked) else "PRESERVED_REQUEST_NEEDS_EXPLICIT_RESUME"
        result["blocked_allocations"] = blocked
        result["next_action"] = "Do not duplicate or steal these requests. Follow the owner's actual running tool handle to terminal when available; otherwise continue independent work and rerun next after owner completion/release. Legacy attempts require their exact explicit resume."
    else:
        result["status"] = "FORWARD_QUEUE_COMPLETE_WITH_PRESERVED_EARLIER_WORK"
        result["next_action"] = "A direct NEXT skipped earlier work. Select its exact allocation/request explicitly; do not silently return to the previous task."
    return result


def validate_material(request, submitted, ledger):
    expected = set(request["family_ids"])
    new_records = {}
    for row in submitted:
        fid = row.get("family_id")
        if fid not in expected or fid in new_records:
            raise ValueError("Unassigned or duplicate submitted family material: " + str(fid))
        for field in MATERIAL_FIELDS:
            if field not in row:
                raise ValueError("Missing family-material field: " + field)
        if row["origin_family_id"] != fid or row["scenario_group"] != request["scenario_group"]:
            raise ValueError("Family origin/scenario_group mismatch: " + fid)
        meaningful(row["scenario"], "scenario")
        if row["scenario"].strip() in {fid, row["scenario_group"]}:
            raise ValueError("A family ID is not concrete scenario material")
        if not isinstance(row["semantic_facts"], list) or not row["semantic_facts"]:
            raise ValueError("Concrete semantic_facts are required: " + fid)
        for fact in row["semantic_facts"]:
            meaningful(fact, "semantic fact")
        if request["domain_id"].startswith("tr."):
            kind = request["family_material_kind"]
            if kind == "layout_counterfactual":
                texts_by_class = row.get("source_text_by_class_language")
                if not isinstance(texts_by_class, dict) or set(texts_by_class) != set(request["family_class_values"]):
                    raise ValueError("Layout counterfactual needs canonical texts for both assigned classes: " + fid)
                for class_value, texts in texts_by_class.items():
                    if not isinstance(texts, dict) or set(texts) != {"en", "ru", "he"}:
                        raise ValueError("Each layout class needs en/ru/he canonical texts: " + fid)
                    for language, value in texts.items():
                        meaningful(value, class_value + " canonical " + language)
            else:
                texts = row.get("source_text_by_language")
                required_languages = set(request["family_source_languages"]) if kind == "source_ambiguity" else {"en", "ru", "he"}
                if not isinstance(texts, dict) or not required_languages <= set(texts) or not set(texts) <= {"en", "ru", "he"}:
                    raise ValueError("Translation family lacks its assigned canonical source languages: " + fid)
                for language, value in texts.items():
                    meaningful(value, "canonical " + language)
                if kind == "source_ambiguity":
                    senses = row.get("sense_translations")
                    if not isinstance(senses, dict) or len(senses) < 2:
                        raise ValueError("Source ambiguity needs at least two named sense_translations: " + fid)
                    for sense_id, translations in senses.items():
                        meaningful(sense_id, "sense_id")
                        if not isinstance(translations, dict) or set(translations) != {"en", "ru", "he"}:
                            raise ValueError("Each sense needs en/ru/he translations: " + fid)
                        for language, value in translations.items():
                            meaningful(value, "sense translation " + language)
                    if len({json.dumps(value, ensure_ascii=False, sort_keys=True) for value in senses.values()}) < 2:
                        raise ValueError("Distinct sense IDs cannot merely repeat the same translations: " + fid)
            if request["pairs_per_family"] == 2:
                contexts = row.get("learner_visible_context_by_class")
                if not isinstance(contexts, dict) or set(contexts) != set(request["family_class_values"]):
                    raise ValueError("Two-class translation family needs exact learner_visible_context_by_class: " + fid)
                for class_value, language_map in contexts.items():
                    if not isinstance(language_map, dict) or not set(request["family_source_languages"]) <= set(language_map) or not set(language_map) <= {"en", "ru", "he"} or not all(isinstance(value, str) for value in language_map.values()):
                        raise ValueError("Each class context must supply the assigned source-language strings; empty context is allowed: " + fid)
                for language in request["family_source_languages"]:
                    if len({contexts[value][language] for value in request["family_class_values"]}) != 2:
                        raise ValueError("Two class contexts must differ in each assigned language: " + fid)
        if fid in ledger and ledger[fid] != row:
            raise ValueError("Existing family facts must not change under the same ID: " + fid)
        new_records[fid] = row
    missing = expected - set(ledger) - set(new_records)
    if missing:
        raise ValueError("Missing real family material for: " + ", ".join(sorted(missing)))
    for fid, assigned in request["family_material"].items():
        if ledger.get(fid) != assigned:
            raise ValueError("Saved request and family ledger disagree: " + fid)
    return {fid: row for fid, row in new_records.items() if fid not in ledger}


def decode_route(text, labels):
    value = strict_load(text)
    if not isinstance(value, dict) or set(value) != {"action", "domain_id"}:
        raise ValueError("Router answer must be JSON with exactly action and domain_id")
    if value["action"] == "clarify" and value["domain_id"] is None:
        label = "clarify"
    elif value["action"] == "route" and isinstance(value["domain_id"], str):
        label = value["domain_id"]
    else:
        raise ValueError("Router action/domain_id combination is invalid")
    if label not in labels:
        raise ValueError("Router answer uses a label outside the assigned catalog")
    return label


def validate_rows(request, rows, model, previous_triples, materials):
    if len(rows) != request["count"]:
        raise ValueError(f"Expected {request['count']} rows, found {len(rows)}; incomplete batch is preserved")
    slots = {slot["pair_id"]: slot for slot in request["row_slots"]}
    seen = set()
    triples = set(previous_triples)
    refs = set(request["source_refs"])
    references = {(e["prompt"], e["chosen"], e["rejected"]) for e in request["examples"]}
    shared_source_inputs = {}
    for index, row in enumerate(rows, 1):
        prefix = "row " + str(index) + ": "
        if any(field not in row for field in ROW_FIELDS):
            raise ValueError(prefix + "missing required DPO field")
        slot = slots.get(row["pair_id"])
        if slot is None or row["pair_id"] in seen:
            raise ValueError(prefix + "unassigned or repeated pair_id")
        seen.add(row["pair_id"])
        expected = {"schema_version": "xiigen.dpo.v1", "domain_id": request["domain_id"],
                    "topic_id": request["topic_id"], "profile_id": request["profile_id"],
                    "family_id": slot["family_id"], "split": "train"}
        for field, value in expected.items():
            if row.get(field) != value:
                raise ValueError(prefix + field + " differs from its assigned value")
        for field in ("prompt", "chosen", "rejected", "error_tag", "rationale"):
            meaningful(row[field], prefix + field)
        if len({row["prompt"].strip(), row["chosen"].strip(), row["rejected"].strip()}) != 3:
            raise ValueError(prefix + "prompt/chosen/rejected must be distinct nonempty texts")
        triple = (row["prompt"], row["chosen"], row["rejected"])
        if triple in triples or triple in references:
            raise ValueError(prefix + "exact duplicate of a saved domain row or supplied reference example")
        triples.add(triple)
        if not isinstance(row["source_refs"], list) or not row["source_refs"] or not all(isinstance(x, str) and x in refs for x in row["source_refs"]):
            raise ValueError(prefix + "source_refs must use real assigned IDs")
        provenance = row["provenance"]
        if not isinstance(provenance, dict) or not isinstance(provenance.get("kind"), str) or not provenance["kind"].strip() or provenance.get("generator_model") != model:
            raise ValueError(prefix + "provenance needs kind and the actual supplied model identity")
        if model == "unknown" and provenance.get("model_identity_observed") is True:
            raise ValueError(prefix + "unknown model identity cannot be marked observed")
        metadata = row["metadata"]
        if not isinstance(metadata, dict):
            raise ValueError(prefix + "metadata must be an object")
        wanted = {"origin_family_id": slot["origin_family_id"],
                  "learning_objective_id": request["objective_id"], "scenario_group": request["scenario_group"],
                  "review_status": "not_requested", "measured_training_result": False}
        for field, value in wanted.items():
            if field not in metadata or metadata[field] != value:
                raise ValueError(prefix + "metadata." + field + " differs from the request")
        if type(metadata["measured_training_result"]) is not bool:
            raise ValueError(prefix + "measured_training_result must be the JSON boolean false")
        leakage = metadata.get("leakage_group_ids")
        if not isinstance(leakage, list) or not all(isinstance(x, str) and x for x in leakage) or slot["family_id"] not in leakage:
            raise ValueError(prefix + "leakage_group_ids must retain the assigned origin family")
        if "family_class" in slot and metadata.get(request["family_class_field"]) != slot["family_class"]:
            raise ValueError(prefix + "incorrect family class")
        for field, value in slot.get("metadata_assignments", {}).items():
            if field not in metadata or metadata[field] != value:
                raise ValueError(prefix + "metadata class assignment differs: " + field)
        if request["domain_id"].startswith("tr."):
            source, target = request["profile_id"].split("-")
            if metadata.get("language_pair") != {"source": source, "target": target}:
                raise ValueError(prefix + "language_pair differs from its assigned profile")
            material = materials[slot["family_id"]]
            if request["family_material_kind"] == "layout_counterfactual":
                canonical = material["source_text_by_class_language"][slot["family_class"]][source]
            else:
                canonical = material["source_text_by_language"][source]
            if metadata.get("canonical_source") != canonical:
                raise ValueError(prefix + "canonical_source differs from shared family material")
            meaningful(metadata.get("source_raw"), prefix + "source_raw")
            if request["family_material_kind"] == "layout_counterfactual" and metadata["source_raw"] != material["source_text_by_class_language"]["preserve"][source]:
                raise ValueError(prefix + "layout source_raw must preserve the assigned RAW identifier input")
            if request["pairs_per_family"] == 2:
                previous_input = shared_source_inputs.setdefault(slot["family_id"], metadata["source_raw"])
                if previous_input != metadata["source_raw"]:
                    raise ValueError(prefix + "two context classes must keep the same source input")
        if "route_class" in slot:
            if metadata.get("route_class") != slot["route_class"]:
                raise ValueError(prefix + "incorrect assigned route_class")
            chosen = decode_route(row["chosen"], request["route_labels"])
            rejected = decode_route(row["rejected"], request["route_labels"])
            if chosen != slot["route_class"] or rejected == chosen:
                raise ValueError(prefix + "router answer labels do not match the assigned contrast")


def finish(args):
    request_path = Path(args.request).resolve()
    request = read(request_path)
    if request.get("schema_version") != REQUEST_SCHEMA:
        raise ValueError("This is a legacy request; use resume --request with this exact path")
    if str(request_path) != request["paths"]["request"]:
        raise ValueError("Request was moved without its saved paths; do not infer a new root")
    registry, _ = config()
    work, root, state_path = paths_for(registry, request["paths"]["learning_sets_root"])
    with transaction(work, registry["campaign_id"]):
        state = load_state(registry, work, state_path)
        return finish_locked(args, request, registry, work, root, state_path, state)


def finish_locked(args, request, registry, work, root, state_path, state):
    input_path = Path(args.input or request["paths"]["draft"]).resolve()
    material_path = Path(args.family_material or request["paths"]["family_material"]).resolve()
    if input_path != Path(request["paths"]["draft"]).resolve():
        raise ValueError("Wrong input path; finish requires the exact saved draft: " + request["paths"]["draft"] + ". No progress was written.")
    if material_path != Path(request["paths"]["family_material"]).resolve():
        raise ValueError("Wrong family-material path; finish requires the exact saved family material: " + request["paths"]["family_material"] + ". No progress was written.")
    key = request["paths"]["request"]
    if key in state["completed_requests"]:
        report = state["completed_requests"][key]
        if report.get("owner_id") != agent_id(args) or report.get("claim_token") != getattr(args, "claim_token", None):
            raise ValueError("Completed request belongs to another owner/token. No progress was changed.")
        verify_completed_input(request, report)
        return completed_result(request, report, agent_id(args), report.get("pool"))
    claim = require_claim(state, request, args)
    marker = Path(request["paths"]["creation_report"])
    if state["allocation_progress"].get(request["allocation_id"], 0) != request["start_pair"]:
        raise ValueError("The request cursor no longer matches campaign progress")
    if not args.model or not args.model.strip():
        raise ValueError("Use the observed model identity, or unknown")
    input_stamp = file_stamp(input_path)
    rows = read_rows(input_path)
    previous = set()
    for saved in state["completed_requests"].values():
        if saved["domain_id"] == request["domain_id"]:
            for row in read_rows(saved["input_path"]):
                previous.add((row["prompt"], row["chosen"], row["rejected"]))
    ledger_path = root / "family-material.jsonl"
    ledger = load_ledger(ledger_path)
    additions = validate_material(request, read_rows(material_path, optional=True), ledger)
    validation_request = dict(request, examples=coordinated_request(request, claim)["examples"])
    validate_rows(validation_request, rows, args.model, previous, dict(ledger, **additions))
    if file_stamp(input_path) != input_stamp:
        raise ValueError("Draft changed while finish was reading it; no creation progress was recorded")
    if additions:
        ledger.update(additions)
        atomic_text(ledger_path, "".join(json.dumps(row, ensure_ascii=False, allow_nan=False) + "\n" for row in ledger.values()))
    report = {"status": "DRAFT_CREATED", "request_path": key, "allocation_id": request["allocation_id"],
              "domain_id": request["domain_id"], "stream_id": request["stream_id"],
              "count": len(rows), "family_count": len(request["family_ids"]),
              "new_family_material_records": len(additions), "input_path": str(input_path),
              "input_stamp": input_stamp,
              "generator_model": args.model, "finished_at": now(),
              "owner_id": claim["owner_id"], "claim_token": claim["token"], "pool": claim["pool"],
              "batch_complete": True,
              "allocation_complete": request["allocation_remaining_after_batch"] == 0,
              "review_status": "not_requested", "semantic_quality": "NOT_REVIEWED",
              "student": "NOT_RUN", "learning_metrics": "NOT_MEASURED",
              "next_command": coordinated_request(request, claim)["commands"]["next"]}
    if marker.exists():
        saved_report = read(marker)
        for field in ("status", "request_path", "allocation_id", "domain_id", "stream_id", "count",
                      "family_count", "input_path", "input_stamp", "generator_model", "batch_complete", "allocation_complete",
                      "review_status", "semantic_quality", "student", "learning_metrics"):
            if saved_report.get(field) != report[field]:
                raise ValueError("Existing creation report conflicts with the actual draft: " + field)
        report = dict(saved_report, owner_id=claim["owner_id"], claim_token=claim["token"], pool=claim["pool"],
                      next_command=coordinated_request(request, claim)["commands"]["next"])
    else:
        write_new(marker, json_text(report))
    commit_creation(state, state_path, request, report, claim, registry)
    result = completed_result(request, report, claim["owner_id"], claim["pool"])
    result["campaign_progress"] = status_result(registry, state, state_path)
    return result


def resume(args):
    path = Path(args.request).resolve()
    request = read(path)
    if request.get("schema_version") != REQUEST_SCHEMA:
        if request.get("schema_version") != "xiigen.batch-request.v1":
            raise ValueError("Unknown request schema; no new run was prepared")
        p = request.get("paths", {})
        script = Path(p.get("tools_script", BASE / "tools" / "batches.py"))
        python = p.get("python_executable") or sys.executable
        if not script.is_file():
            raise ValueError("Saved legacy tool is missing: " + str(script) + ". No alternative checkout was searched.")
        return {"status": "LEGACY_REQUEST_PRESERVED", "request": str(path),
                "command": command(script, "resume", "--request", path, python=python),
                "legacy_pairs_counted": 0, "generation": "NOT_RUN",
                "coordination_scope": "LEGACY_NOT_COORDINATED", "shared_claim_created": False,
                "instruction": "This old batch runner does not participate in the current campaign claims. Use the exact legacy resume only in a separately designated sequential legacy session; do not start another session on that legacy path. Preserve the old request and data. Do not create a replacement campaign request or relocate the old data."}
    if str(path) != request["paths"]["request"]:
        raise ValueError("Saved request paths no longer match its location")
    owner = agent_id(args)
    registry, _ = config()
    work, root, state_path = paths_for(registry, request["paths"]["learning_sets_root"])
    with transaction(work, registry["campaign_id"]):
        state = load_state(registry, work, state_path)
        if str(path) in state["completed_requests"]:
            verify_completed_input(request, state["completed_requests"][str(path)])
            return completed_result(request, state["completed_requests"][str(path)], owner)
        if state["pending_requests"].get(request["allocation_id"]) != str(path):
            raise ValueError("Request is not a known pending campaign attempt; no generation started")
        current = state["claims"].get(request["domain_id"])
        if current and (current["owner_id"] != owner or current["request"] != str(path)):
            raise ValueError("Request/domain is owned by another session. It must be explicitly released first; age is not permission to take over.")
        if current and getattr(args, "claim_token", None) != current["token"]:
            raise ValueError("Resume requires the exact current --claim-token from coordination-commands.md")
        previous = state["active_requests"].get(owner)
        if previous and previous != str(path):
            previous_request = read(previous)
            release_claim(state, state["claims"][previous_request["domain_id"]])
        claim = current or acquire_claim(state, request, owner, pool_for(request["domain_id"]), root)
        state["active_requests"][owner] = str(path)
        state["agent_cursors"][owner] = request["allocation_order"]
        claim["updated_at"] = now()
        ensure_artifacts(request)
        effective = publish_coordination(request, claim)
        save_state(state_path, state, registry)
        result = request_result(effective)
        result["action"] = "RESUME_SAVED_REQUEST"
        return result


def progress_or_release(args):
    path = Path(args.request).resolve()
    request = read(path)
    if request.get("schema_version") != REQUEST_SCHEMA or str(path) != request.get("paths", {}).get("request"):
        raise ValueError("Use the exact saved current campaign request path")
    registry, _ = config()
    work, root, state_path = paths_for(registry, request["paths"]["learning_sets_root"])
    with transaction(work, registry["campaign_id"]):
        state = load_state(registry, work, state_path)
        claim = require_claim(state, request, args)
        effective = coordinated_request(request, claim)
        if args.action == "release":
            release_claim(state, claim)
            state.setdefault("pauses", []).append({"request": str(path), "owner_id": claim["owner_id"],
                                                    "reason": "explicit release", "at": now()})
            status = "RELEASED_PRESERVED"
        else:
            rows = read_rows(request["paths"]["draft"], optional=True)
            ids = [row.get("pair_id") for row in rows]
            assigned = {slot["pair_id"] for slot in request["row_slots"]}
            if len(rows) > request["count"] or len(ids) != len(set(ids)) or any(value not in assigned for value in ids):
                raise ValueError("Progress file has excess, duplicate or unassigned rows. Correct the current draft; no count was credited.")
            for row in rows:
                for field in ("prompt", "chosen", "rejected"):
                    if not isinstance(row.get(field), str) or not row[field].strip():
                        raise ValueError("Progress requires nonempty text in " + field + "; no count was credited.")
                if row["chosen"] == row["rejected"]:
                    raise ValueError("Progress requires different chosen and rejected texts; no count was credited.")
            claim["created_rows"] = len(rows)
            claim["updated_at"] = now()
            status = "AUTHORING_IN_PROGRESS"
        save_state(state_path, state, registry)
        return {"status": status, "owner_id": claim["owner_id"], "request": str(path),
                "authoring_rows_saved": claim.get("created_rows", 0), "credited_pairs": 0,
                "state_path": str(state_path), "status_path": str(state_path.with_name("status.json")),
                "next_command": effective["commands"]["next"],
                "instruction": "Progress is file creation only; finish credits a complete structured batch. Semantic quality and training remain unestablished."}


def flag_quality(args):
    owner = agent_id(args)
    registry, _ = config()
    work, root, state_path = paths_for(registry, args.work_root)
    if args.domain not in {d["domain_id"] for d in registry["domains"]}:
        raise ValueError("Unknown quality-flag domain")
    meaningful(args.reason, "quality reason")
    meaningful(args.flag_id, "flag ID")
    if not args.evidence or not all(isinstance(value, str) and value.strip() for value in args.evidence):
        raise ValueError("At least one exact evidence reference is required")
    with transaction(work, registry["campaign_id"]):
        state = load_state(registry, work, state_path, create=True)
        flag = {"flag_id": args.flag_id, "domain_id": args.domain, "topic_id": args.topic,
                "profile_id": args.profile, "status": args.quality_status,
                "reason": args.reason, "evidence": args.evidence,
                "recorded_by": owner, "updated_at": now()}
        previous = state["quality_flags"].get(args.flag_id)
        if previous and (previous.get("domain_id"), previous.get("topic_id"), previous.get("profile_id")) != (args.domain, args.topic, args.profile):
            raise ValueError("Flag ID already belongs to another domain/topic/profile; use a new flag ID")
        if previous:
            state.setdefault("quality_flag_history", []).append(previous)
        state["quality_flags"][args.flag_id] = flag
        save_state(state_path, state, registry)
        return {"status": "QUALITY_FLAG_RECORDED", "flag": flag,
                "status_path": str(state_path.with_name("status.json")), "training": "NOT_RUN"}


def shared_status(args):
    registry, _ = config()
    work, root, state_path = paths_for(registry, args.work_root)
    with transaction(work, registry["campaign_id"]):
        state = load_state(registry, work, state_path, create=True)
        result = status_result(registry, state, state_path)
        atomic_text(state_path.with_name("status.json"), json_text(result))
        return result


def main(argv=None):
    for stream in (sys.stdout, sys.stderr):
        if hasattr(stream, "reconfigure"):
            stream.reconfigure(encoding="utf-8", errors="strict")
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="action", required=True)
    nex = commands.add_parser("next", help="Prepare or resume one assigned batch; does not generate rows")
    nex.add_argument("--work-root")
    nex.add_argument("--allocation", help="Explicit NEXT to an allocation ID; earlier work is preserved")
    nex.add_argument("--agent-id", required=True, help="Unique Kiro session ID; keep it for exact resume")
    nex.add_argument("--pool", choices=("translations", "orchestration", "software", "all"), default="all")
    stat = commands.add_parser("status", help="Read actual creation progress")
    stat.add_argument("--work-root")
    fin = commands.add_parser("finish", help="Record existing candidate rows after structural checks")
    fin.add_argument("--request", required=True)
    fin.add_argument("--input")
    fin.add_argument("--family-material")
    fin.add_argument("--model", default="unknown")
    fin.add_argument("--agent-id", required=True)
    fin.add_argument("--claim-token", required=True)
    res = commands.add_parser("resume", help="Resume an exact current or legacy request without re-generation")
    res.add_argument("--request", required=True)
    res.add_argument("--agent-id", required=True)
    res.add_argument("--claim-token", help="Required when this request already has a current owner")
    for name in ("progress", "release"):
        cmd = commands.add_parser(name, help="Record saved rows or explicitly release a claim while preserving files")
        cmd.add_argument("--request", required=True)
        cmd.add_argument("--agent-id", required=True)
        cmd.add_argument("--claim-token", required=True)
    flag = commands.add_parser("flag", help="Record evidence-backed domain/subtopic needs_rework or needs_assessment")
    flag.add_argument("--work-root")
    flag.add_argument("--agent-id", required=True)
    flag.add_argument("--flag-id", required=True)
    flag.add_argument("--domain", required=True)
    flag.add_argument("--topic")
    flag.add_argument("--profile")
    flag.add_argument("--quality-status", choices=("needs_rework", "needs_assessment", "resolved"), required=True)
    flag.add_argument("--reason", required=True)
    flag.add_argument("--evidence", action="append", required=True)
    args = parser.parse_args(argv)
    try:
        if args.action == "next":
            result = next_batch(args)
        elif args.action == "finish":
            result = finish(args)
        elif args.action == "resume":
            result = resume(args)
        elif args.action in {"progress", "release"}:
            result = progress_or_release(args)
        elif args.action == "flag":
            result = flag_quality(args)
        else:
            result = shared_status(args)
        print(json_text(result), end="")
        return 0
    except (OSError, ValueError, KeyError, TypeError) as error:
        print(json_text({"status": "BLOCKED", "mode": MODE, "error": str(error),
                         "generation": "NOT_RUN_BY_THIS_TOOL", "prior_files_preserved": True}), end="")
        return 2


if __name__ == "__main__":
    raise SystemExit(main())

