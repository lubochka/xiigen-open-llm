#!/usr/bin/env python3
"""Read-only gold/current OrchestrationReasoning compatibility probe.

Usage:
  python3 52-GOLD-OR-STATIC-PREFLIGHT.py /checkout/orchestrator-extensions /checkout/codex-dpo-implementation

No .NET calls, no training, no writes, no model quality claim. Outputs JSON to stdout.
Supply actual checkouts pinned to the commits in the accompanying audit.
"""
from __future__ import annotations

import hashlib
import json
import re
import sys
from pathlib import Path

REL_ASSET = Path("models/orchestration-reasoning/orchestration-reasoning-v1")
REL_MACHINE = Path("43.SmartSkillsDomain/OrchestrationReasoningMachine.cs")
REL_API = Path("43.SmartSkillsDomain/XiigenExtensionApiService.cs")
REL_INDEX = Path("models/index.json")


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def jread(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8-sig"))


def cs_schema(source: str) -> int | None:
    match = re.search(r"private\s+const\s+int\s+CheckpointSchemaVersion\s*=\s*(\d+)", source)
    return int(match.group(1)) if match else None


def inspect(root: Path) -> dict:
    root = root.resolve(strict=True)
    manifest_path = root / REL_ASSET / "manifest.json"
    manifest = jread(manifest_path)
    named = root / REL_ASSET / manifest.get("checkpointRef", "")
    fallback = root / REL_ASSET / "checkpoint.json"
    model = jread(named)
    fallback_model = jread(fallback)
    source = (root / REL_MACHINE).read_text(encoding="utf-8-sig")
    api = (root / REL_API).read_text(encoding="utf-8-sig")
    index = jread(root / REL_INDEX)
    declared = manifest.get("checkpointSha256")
    actual = sha256(named)
    return {
        "repo_root": str(root),
        "machine_checkpoint_schema": cs_schema(source),
        "committed_manifest_schema": manifest.get("schemaVersion"),
        "named_checkpoint_schema": model.get("schemaVersion"),
        "named_checkpoint_path": str(named.relative_to(root)),
        "named_learned_pattern_count": len(model.get("learnedState", {}).get("learnedPatterns", [])),
        "named_accepted_ref_count": len(model.get("acceptedRefIds", [])),
        "fallback_learned_pattern_count": len(fallback_model.get("learnedState", {}).get("learnedPatterns", [])),
        "fallback_accepted_ref_count": len(fallback_model.get("acceptedRefIds", [])),
        "manifest_declared_pattern_count": manifest.get("learnedPatternCount"),
        "manifest_dpo_pair_count": manifest.get("dpoPairCount"),
        "named_checkpoint_sha256_matches_manifest": (
            actual.lower() == declared.lower() if isinstance(declared, str) else None
        ),
        "index_lists_orchestration_reasoning": any(
            x.get("model_id") == "orchestration-reasoning-v1" for x in index.get("models", [])
        ),
        "current_style_explicit_v1_manifest_rejection": bool(
            re.search(r"ReadSchemaVersion\(document\.RootElement\)\s*==\s*1", source)
            and "ProvenanceRebuildRequiredCode" in source
        ),
        "api_uses_literal_reasoning_manifest_path": (
            '"orchestration-reasoning", "orchestration-reasoning-v1", "manifest.json"' in api
        ),
        "api_warns_on_failed_load": (
            "orchestration_reasoning_asset_loaded" in api
            and "orchestration_reasoning_load_error" in api
        ),
        "source_only": True,
        "native_loaded": None,
        "quality_gain": None,
    }


def main() -> int:
    if len(sys.argv) != 3:
        print(__doc__, file=sys.stderr)
        return 2
    try:
        old = inspect(Path(sys.argv[1]))
        current = inspect(Path(sys.argv[2]))
    except (OSError, ValueError, KeyError, TypeError) as exc:
        print(json.dumps({"status": "INCOMPLETE_INPUT", "error": str(exc)}, ensure_ascii=False))
        return 2

    expected_incompatibility = (
        old["machine_checkpoint_schema"] == 1
        and current["machine_checkpoint_schema"] == 2
        and current["committed_manifest_schema"] == 1
        and current["named_checkpoint_schema"] == 1
        and current["current_style_explicit_v1_manifest_rejection"]
        and current["api_uses_literal_reasoning_manifest_path"]
    )
    status = "STATIC_COMPATIBILITY_GAP_OBSERVED" if expected_incompatibility else "RECHECK_SOURCE_OR_GAP_RESOLVED"
    print(json.dumps({
        "status": status,
        "note": "Static comparison only: does not run LoadCheckpointAsync or score end-user quality",
        "gold": old,
        "current": current,
        "next": "Native A0: gold v1 load, current v1 refusal, lawful v2 admission+12 frozen user cases",
    }, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
