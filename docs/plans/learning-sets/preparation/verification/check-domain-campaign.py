#!/usr/bin/env python3
"""Temporary synthetic fixtures only; no Teacher, corpus generation or learning."""
from collections import Counter
import copy
import importlib.util
import json
from pathlib import Path
import shutil
import sys
from types import SimpleNamespace as NS
import tempfile


def put_json(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding="utf-8")


def put_rows(path, rows):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("".join(json.dumps(row, ensure_ascii=False) + "\n" for row in rows), encoding="utf-8")


def fixtures(base):
    domain_ids = ["tr.en", "tr.ru", "tr.he", "docs.csharp", "docs.sql", "docs.angular",
                  "code.csharp", "code.sql", "code.angular", "or.functional", "or.architecture",
                  "or.code", "or.database", "or.client", "or.server", "or.mobile", "or.design",
                  "or.ux", "or.qa", "or.automation", "or.routing"]
    domains = [{"domain_id": did, "order": i + 1, "title_ru": "Synthetic fixture " + did,
                "target_pairs": 1000, "card": "fixture.md",
                "next_domain_id": domain_ids[i + 1] if i + 1 < len(domain_ids) else None}
               for i, did in enumerate(domain_ids)]
    source = base / "rag" / "sources" / "FIXTURE-SOURCE.md"
    source.parent.mkdir(parents=True)
    source.write_text("Synthetic fixture source. Not a training resource.\n", encoding="utf-8")
    excerpts = [{"source_id": "FIXTURE-SOURCE", "path": "rag/sources/FIXTURE-SOURCE.md",
                 "excerpt": "Synthetic fixture source. Not a training resource."}]
    profiles = ["ru-en", "he-en", "en-ru", "he-ru", "ru-he", "en-he", "base"]
    examples = [{"profile_id": profile, "prompt": f"Synthetic fixture example {profile} {i}",
                 "chosen": f"Synthetic correct fixture answer {i}",
                 "rejected": f"Synthetic wrong fixture answer {i}",
                 "rationale": "This is a structural fixture only, not a useful teaching pair.",
                 "metadata": {"synthetic_fixture": True}}
                for profile in profiles for i in (1, 2)]
    common = {"goal_ru": "Синтетическая проверка утилиты; не обучающий материал.",
              "instructions": "Use synthetic fixtures only.", "answer_contract": "Fixture text.",
              "hard_constraints": ["No real generation or training."], "scenario_axes": [],
              "examples": examples, "source_refs": ["FIXTURE-SOURCE"], "source_excerpts": excerpts}
    cells = [{"fixture_gender": gender, "fixture_reference": reference}
             for gender in ("feminine", "masculine") for reference in ("first", "second")]
    common["class_schedule"] = [cells[i] for i in [0, 1, 2, 3, 0, 1, 2, 3, 0, 3, 1, 2, 0, 3, 1, 2, 0, 3, 1, 2]]
    lessons = [dict(common, lesson_id="TR-FIXTURE", pairs_per_family=2,
                    family_class_field="fixture_class", family_class_values=["ambiguous", "resolved"]),
               dict(common, lesson_id="BASE-FIXTURE", pairs_per_family=1),
               dict(common, lesson_id="ROUTE-FIXTURE", pairs_per_family=1,
                    route_labels=domain_ids[:-1] + ["clarify"])]
    specifications = [("tr.en", 20, "ru-en", "TR-FIXTURE", "translation-shared"),
                      ("tr.en", 980, "he-en", "TR-FIXTURE", "translation-en-rest"),
                      ("tr.ru", 20, "en-ru", "TR-FIXTURE", "translation-shared"),
                      ("tr.ru", 980, "he-ru", "TR-FIXTURE", "translation-ru-rest"),
                      ("tr.he", 1000, "ru-he", "TR-FIXTURE", "translation-he-rest")]
    specifications += [(did, 1000, "base", "ROUTE-FIXTURE" if did == "or.routing" else "BASE-FIXTURE", did)
                       for did in domain_ids[3:]]
    allocations = []
    for i, (domain, target, profile, lesson, group) in enumerate(specifications):
        allocations.append({"allocation_id": f"A{i + 1:03d}", "order": i + 1, "domain_id": domain,
                            "stream_id": "FIXTURE/" + profile, "topic_id": "FIXTURE-TOPIC",
                            "objective_id": lesson, "profile_id": profile, "target_pairs": target,
                            "batch_size": 21 if domain == "or.routing" else 10,
                            "pairs_per_family": 2 if domain.startswith("tr.") else 1,
                            "lesson_id": lesson, "scenario_group": group,
                            "next_allocation_id": f"A{i + 2:03d}" if i + 1 < len(specifications) else None,
                            "source_refs": ["FIXTURE-SOURCE"], "reference_card": "fixture.md"})
    registry = {"campaign_id": "D1000-20261025", "mode": "CREATE_ONLY", "total_target_pairs": 21000,
                "canonical_windows_learning_sets_root": "C:/Projects/llm_mvp_core/learning-sets",
                "domains": domains, "allocations": allocations}
    put_json(base / "campaign" / "registry.json", registry)
    put_json(base / "campaign" / "lessons.json", {"lessons": lessons})
    return allocations[-1]["allocation_id"]


def load_module(path):
    spec = importlib.util.spec_from_file_location("campaign_fixture_module", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def make_draft(module, request, material=True):
    rows = []
    for slot in request["row_slots"]:
        metadata = {"origin_family_id": slot["family_id"], "leakage_group_ids": [slot["family_id"]],
                    "learning_objective_id": request["objective_id"], "scenario_group": request["scenario_group"],
                    "review_status": "not_requested", "measured_training_result": False,
                    "synthetic_fixture": True}
        if "family_class" in slot:
            metadata[request["family_class_field"]] = slot["family_class"]
        metadata.update(slot.get("metadata_assignments", {}))
        if request["domain_id"].startswith("tr."):
            source, target = request["profile_id"].split("-")
            canonical = {"en": "Synthetic fixture " + slot["family_id"],
                         "ru": "Синтетическая проверка " + slot["family_id"],
                         "he": "בדיקה סינתטית " + slot["family_id"]}[source]
            metadata.update(language_pair={"source": source, "target": target},
                            canonical_source=canonical, source_raw=canonical)
        chosen = "Synthetic fixture chosen " + slot["pair_id"]
        rejected = "Synthetic fixture rejected " + slot["pair_id"]
        if "route_class" in slot:
            label = slot["route_class"]
            metadata["route_class"] = label
            other = next(label2 for label2 in request["route_labels"] if label2 != label)
            encode = lambda value: json.dumps({"action": "clarify" if value == "clarify" else "route",
                                                "domain_id": None if value == "clarify" else value})
            chosen, rejected = encode(label), encode(other)
        rows.append({"schema_version": "xiigen.dpo.v1", "pair_id": slot["pair_id"],
                     "domain_id": request["domain_id"], "topic_id": request["topic_id"],
                     "profile_id": request["profile_id"], "family_id": slot["family_id"], "split": "train",
                     "prompt": "Synthetic fixture prompt " + slot["pair_id"], "chosen": chosen, "rejected": rejected,
                     "error_tag": "synthetic_fixture", "rationale": "Synthetic structural fixture only.",
                     "source_refs": ["FIXTURE-SOURCE"],
                     "provenance": {"kind": "synthetic_test_fixture", "generator_model": "unknown"},
                     "metadata": metadata})
    put_rows(Path(request["paths"]["draft"]), rows)
    if material:
        facts = []
        for fid in request["missing_family_ids"]:
            row = {"family_id": fid, "origin_family_id": fid, "scenario_group": request["scenario_group"],
                   "scenario": "Synthetic fixture scenario for " + fid,
                   "semantic_facts": ["This is a synthetic fixture, not a generated teaching scenario."]}
            if request["domain_id"].startswith("tr."):
                row["source_text_by_language"] = {"en": "Synthetic fixture " + fid,
                                                  "ru": "Синтетическая проверка " + fid,
                                                  "he": "בדיקה סינתטית " + fid}
                if request["pairs_per_family"] == 2:
                    row["learner_visible_context_by_class"] = {
                        value: {language: ("" if index == 0 else "Synthetic resolved fixture context")
                                for language in ("en", "ru", "he")}
                        for index, value in enumerate(request["family_class_values"])}
            facts.append(row)
        if facts:
            put_rows(Path(request["paths"]["family_material"]), facts)
    return rows


def finish_args(request, **overrides):
    result = dict(request=request["paths"]["request"], input=None, family_material=None, model="unknown")
    result.update(overrides)
    return NS(**result)


def run():
    checks = []
    with tempfile.TemporaryDirectory(prefix="xiigen-campaign-fixtures-") as temp:
        base = Path(temp) / "learning-sets" / "preparation"
        work = Path(temp) / "core" / "learning-sets"
        tools = base / "tools"
        tools.mkdir(parents=True)
        packaged_runner = Path(__file__).with_name("campaign.py")
        if not packaged_runner.is_file():
            packaged_runner = Path(__file__).resolve().parent.parent / "tools" / "campaign.py"
        shutil.copyfile(packaged_runner, tools / "campaign.py")
        route_id = fixtures(base)
        module = load_module(tools / "campaign.py")
        if module.os.name != "nt":
            try:
                module.next_batch(NS(work_root=None, allocation=None))
                raise AssertionError("implicit Windows path accepted on non-Windows")
            except ValueError as error:
                assert "explicit absolute --work-root" in str(error)
        first = module.next_batch(NS(work_root=str(work), allocation=None))
        request = module.read(first["paths"]["request"])
        assert Path(first["paths"]["learning_sets_root"]) == work
        assert not Path(first["paths"]["draft"]).exists()
        assert not Path(first["paths"]["family_material"]).exists()
        assert module.read(first["paths"]["material_required"])["status"] == "AUTHORING_ASSIGNMENT_NOT_FAMILY_FACTS"
        assert request["count"] == 10 and len(request["family_ids"]) == 5
        checks.append("next only prepares exact paths/slots; no fake draft or family material")

        rows = make_draft(module, request)
        state_before_wrong_path = Path(request["paths"]["state"]).read_bytes()
        outside_draft = Path(temp) / "another-directory" / "draft.jsonl"
        put_rows(outside_draft, rows)
        for override in ({"input": str(outside_draft)}, {"family_material": str(outside_draft)}):
            try:
                module.finish(finish_args(request, **override))
                raise AssertionError("finish accepted an input from another directory")
            except ValueError as error:
                assert "exact saved" in str(error)
            assert Path(request["paths"]["state"]).read_bytes() == state_before_wrong_path
        checks.append("finish rejects alternate draft/family-material paths before writing state")
        put_rows(Path(request["paths"]["draft"]), rows[:5])
        before = Path(request["paths"]["draft"]).read_bytes()
        try:
            module.finish(finish_args(request))
            raise AssertionError("partial draft accepted")
        except ValueError as error:
            assert "Expected 10 rows" in str(error)
        again = module.next_batch(NS(work_root=str(work), allocation=None))
        assert again["paths"]["request"] == request["paths"]["request"]
        assert Path(request["paths"]["draft"]).read_bytes() == before
        assert module.read(request["paths"]["state"])["allocation_progress"] == {}
        checks.append("partial draft is retained and uncounted; next resumes the same request")

        put_rows(Path(request["paths"]["draft"]), rows)
        finished = module.finish(finish_args(request))
        assert finished["creation"]["count"] == 10
        module.finish(finish_args(request))
        assert module.read(request["paths"]["state"])["allocation_progress"]["A001"] == 10
        second = module.next_batch(NS(work_root=str(work), allocation=None))
        second_request = module.read(second["paths"]["request"])
        assert second_request["allocation_id"] == "A001" and second_request["start_pair"] == 10
        assert not set(request["family_ids"]) & set(second_request["family_ids"])
        first_classes = Counter(tuple(sorted(s["metadata_assignments"].items())) for s in request["row_slots"])
        second_classes = Counter(tuple(sorted(s["metadata_assignments"].items())) for s in second_request["row_slots"])
        assert sorted(first_classes.values()) == [2, 2, 3, 3]
        assert all(first_classes[cell] + second_classes[cell] == 5 for cell in first_classes)
        make_draft(module, second_request)
        done = module.finish(finish_args(second_request))
        domain = next(d for d in done["campaign_progress"]["domains"] if d["domain_id"] == "tr.en")
        assert domain["draft_created_pairs"] == 20 and domain["remaining_pairs"] == 980
        assert not domain["domain_complete"]
        assert module.next_batch(NS(work_root=str(work), allocation=None))["allocation_id"] == "A002"
        checks.append("finish is idempotent; next fills allocation, then advances; batch completion is not domain completion")
        checks.append("fixed class schedule assigns 3/2/2/3 then complementary 2/3/3/2; rows retain exact assigned metadata")

        # These are isolated structural fixtures; no campaign progress is changed.
        for kind in ("layout_counterfactual", "source_ambiguity"):
            special = copy.deepcopy(request)
            special["paths"]["draft"] = str(Path(temp) / "isolated-fixtures" / kind / "draft.jsonl")
            special["family_material_kind"] = kind
            special["family_source_languages"] = ["ru"]
            special["family_class_values"] = ["restore", "preserve"] if kind == "layout_counterfactual" else ["ambiguous", "resolved"]
            for index, slot in enumerate(special["row_slots"]):
                slot["family_class"] = special["family_class_values"][index % 2]
            special_rows = make_draft(module, special, material=False)
            materials = []
            for fid in special["family_ids"]:
                item = {"family_id": fid, "origin_family_id": fid, "scenario_group": special["scenario_group"],
                        "scenario": "Synthetic class-dependent fixture scenario for " + fid,
                        "semantic_facts": ["Synthetic structural fixture only."],
                        "learner_visible_context_by_class": {special["family_class_values"][0]: {"ru": ""},
                                                              special["family_class_values"][1]: {"ru": "Synthetic distinguishing context"}}}
                if kind == "layout_counterfactual":
                    item["source_text_by_class_language"] = {
                        "restore": {language: "Decoded ordinary fixture " + fid for language in ("en", "ru", "he")},
                        "preserve": {language: "RAW-IDENTIFIER-" + fid for language in ("en", "ru", "he")}}
                else:
                    item["source_text_by_language"] = {"ru": "Ambiguous fixture word " + fid}
                    item["sense_translations"] = {"sense_a": {language: "Fixture sense A" for language in ("en", "ru", "he")},
                                                  "sense_b": {language: "Fixture sense B" for language in ("en", "ru", "he")}}
                materials.append(item)
            material_map = {item["family_id"]: item for item in materials}
            for row, slot in zip(special_rows, special["row_slots"]):
                item = material_map[row["family_id"]]
                if kind == "layout_counterfactual":
                    row["metadata"]["canonical_source"] = item["source_text_by_class_language"][slot["family_class"]]["ru"]
                    row["metadata"]["source_raw"] = item["source_text_by_class_language"]["preserve"]["ru"]
                else:
                    row["metadata"]["canonical_source"] = item["source_text_by_language"]["ru"]
                    row["metadata"]["source_raw"] = item["source_text_by_language"]["ru"]
            module.validate_material(special, materials, {})
            module.validate_rows(special, special_rows, "unknown", set(), material_map)
            if kind == "layout_counterfactual":
                special_rows[1]["metadata"]["canonical_source"] = special_rows[0]["metadata"]["canonical_source"]
                try:
                    module.validate_rows(special, special_rows, "unknown", set(), material_map)
                    raise AssertionError("layout classes accepted a single wrong canonical source")
                except ValueError as error:
                    assert "canonical_source" in str(error)
        checks.append("layout uses class-specific canonicals with one RAW input; ambiguity supports only its assigned source language and two multilingual senses")

        related = module.next_batch(NS(work_root=str(work), allocation="A003"))
        related_request = module.read(related["paths"]["request"])
        assert related_request["family_ids"] == request["family_ids"]
        assert related_request["missing_family_ids"] == []
        assert len(related_request["family_material"]) == 5
        make_draft(module, related_request)
        material = next(iter(related_request["family_material"].values())).copy()
        material["scenario"] = "A different synthetic scenario under the same ID"
        put_rows(Path(related_request["paths"]["family_material"]), [material])
        try:
            module.finish(finish_args(related_request))
            raise AssertionError("conflicting family material accepted")
        except ValueError as error:
            assert "must not change" in str(error)
        rejected_material = Path(related_request["paths"]["family_material"])
        shutil.copyfile(rejected_material, rejected_material.with_name("family-material.rejected-fixture.jsonl"))
        put_rows(rejected_material, list(related_request["family_material"].values()))
        module.finish(finish_args(related_request))
        checks.append("translation directions reuse concrete en/ru/he material; changing facts under an existing ID is rejected")

        pending = module.next_batch(NS(work_root=str(work), allocation=None))
        pending_request = module.read(pending["paths"]["request"])
        assert pending_request["allocation_id"] == "A003"
        make_draft(module, pending_request)
        module.next_batch(NS(work_root=str(work), allocation=route_id))
        try:
            module.finish(finish_args(pending_request))
            raise AssertionError("old pending request finished after NEXT")
        except ValueError as error:
            assert "not the currently selected" in str(error)
        assert Path(pending_request["paths"]["draft"]).exists()
        checks.append("explicit NEXT preserves but does not continue the old pending task")

        counts, route_counts = [], Counter()
        for number in range(48):
            current = module.next_batch(NS(work_root=str(work), allocation=None))
            current_request = module.read(current["paths"]["request"])
            assert current_request["allocation_id"] == route_id
            assert current_request["batch_number"] == number + 1
            assert current_request["batch_count"] == 48
            counts.append(current_request["count"])
            route_counts.update(current_request["route_class_counts"])
            make_draft(module, current_request)
            result = module.finish(finish_args(current_request))
        assert counts == [21] * 47 + [13]
        assert sum(route_counts.values()) == 1000
        assert sorted(route_counts.values()) == [47] * 8 + [48] * 13
        route_domain = next(d for d in result["campaign_progress"]["domains"] if d["domain_id"] == "or.routing")
        assert route_domain["domain_complete"] and route_domain["draft_created_pairs"] == 1000
        tail = module.next_batch(NS(work_root=str(work), allocation=None))
        assert tail["status"] == "FORWARD_QUEUE_COMPLETE_WITH_PRESERVED_EARLIER_WORK"
        checks.append("router completes exactly 1000 as 47 batches of 21 plus 13; label totals 47/48 and no quota overflow")

        completed_draft = Path(current_request["paths"]["draft"])
        unchanged_state = Path(current_request["paths"]["state"]).read_bytes()
        completed_draft.write_text(completed_draft.read_text(encoding="utf-8") + "\n", encoding="utf-8")
        for action in (lambda: module.finish(finish_args(current_request)),
                       lambda: module.resume(NS(request=current_request["paths"]["request"]))):
            try:
                action()
                raise AssertionError("changed completed draft reported as DRAFT_CREATED")
            except ValueError as error:
                assert "changed after DRAFT_CREATED" in str(error)
            assert Path(current_request["paths"]["state"]).read_bytes() == unchanged_state
        completed_draft.unlink()  # Temporary synthetic fixture only.
        for action in (lambda: module.finish(finish_args(current_request)),
                       lambda: module.resume(NS(request=current_request["paths"]["request"]))):
            try:
                action()
                raise AssertionError("missing completed draft reported as DRAFT_CREATED")
            except ValueError as error:
                assert "Saved draft is missing" in str(error)
        assert Path(current_request["paths"]["state"]).read_bytes() == unchanged_state
        checks.append("finish/resume block changed or missing completed drafts using size+mtime_ns, preserving recorded progress")

        legacy_script = tools / "batches.py"
        legacy_script.write_text("# Synthetic legacy-tool fixture, never executed.\n", encoding="utf-8")
        legacy_path = Path(temp) / "old run" / "request.json"
        put_json(legacy_path, {"schema_version": "xiigen.batch-request.v1",
                               "paths": {"tools_script": str(legacy_script), "python_executable": module.sys.executable}})
        legacy = module.resume(NS(request=str(legacy_path)))
        assert legacy["status"] == "LEGACY_REQUEST_PRESERVED" and legacy["legacy_pairs_counted"] == 0
        assert "batches.py" in legacy["command"] and "resume" in legacy["command"]
        checks.append("legacy resume returns the exact saved-tool command without generating or adopting rows")

    return {"status": "PASS", "scope": "TEMPORARY_SYNTHETIC_UTILITY_FIXTURES_ONLY",
            "checks": checks, "teacher_calls": 0, "student_runs": 0,
            "real_campaign_generated_pairs": 0, "fixture_storage": "temporary directory removed on exit"}


def actual_config_result():
    runner = Path(__file__).with_name("campaign.py")
    if not runner.is_file():
        runner = Path(__file__).resolve().parent.parent / "tools" / "campaign.py"
    module = load_module(runner)
    registry, _ = module.config()
    results, failures = [], []
    with tempfile.TemporaryDirectory(prefix="xiigen-all-request-") as temp:
        for allocation in registry["allocations"]:
            try:
                result = module.next_batch(NS(work_root=temp, allocation=allocation["allocation_id"]))
                request = module.read(result["paths"]["request"])
                prompt = Path(result["paths"]["prompt"]).read_text(encoding="utf-8")
                assert request["count"] == min(allocation["target_pairs"], allocation["batch_size"])
                assert not Path(result["paths"]["draft"]).exists()
                assert not Path(result["paths"]["family_material"]).exists()
                for field in ("goal_ru", "instructions", "answer_contract", "authoring_checks_en"):
                    assert request["subject"].get(field), "Missing subject field " + field
                results.append({"allocation_id": allocation["allocation_id"], "stream_id": allocation["stream_id"],
                                "prompt_chars": len(prompt), "examples": len(request["examples"]),
                                "source_excerpts": len(request["source_excerpts"])})
            except (AssertionError, OSError, ValueError, KeyError, TypeError) as error:
                failures.append({"allocation_id": allocation["allocation_id"], "error": str(error)})
    return {"status": "PASS" if not failures else "FAIL",
            "scope": "REQUEST_PREPARATION_ONLY_NO_MODEL_OR_DATA_GENERATION",
            "allocations_checked": len(results), "failures": failures,
            "prompt_chars": {"min": min((x["prompt_chars"] for x in results), default=0),
                             "max": max((x["prompt_chars"] for x in results), default=0),
                             "mean": round(sum(x["prompt_chars"] for x in results) / len(results)) if results else 0},
            "largest_prompts": sorted(results, key=lambda x: x["prompt_chars"], reverse=True)[:5],
            "teacher_calls": 0, "generated_dpo_rows": 0, "student_runs": 0}


if __name__ == "__main__":
    result = actual_config_result() if "--actual" in sys.argv[1:] else run()
    print(json.dumps(result, ensure_ascii=False, indent=2))
    raise SystemExit(0 if result["status"] == "PASS" else 1)
