#!/usr/bin/env python3
"""Behavioral coordination checks with separate OS processes and temporary data.

These fixtures verify authoring ownership and preservation only. They do not
generate a production corpus, evaluate semantic DPO quality, or train a learner.
"""
from concurrent.futures import ThreadPoolExecutor
import json
import os
from pathlib import Path
import shutil
import shlex
import subprocess
import sys
import tempfile
import unittest

BASE = Path(__file__).resolve().parents[1]


class ParallelCampaign(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="xiigen-parallel-")
        self.root = Path(self.temporary.name)
        self.prep = self.root / "preparation"
        self.work = self.root / "sets"
        self.tools = self.prep / "tools"
        self.tools.mkdir(parents=True)
        for name in ("campaign.py", "campaign_status.py"):
            shutil.copyfile(BASE / "tools" / name, self.tools / name)
        self.script = self.tools / "campaign.py"
        self.campaign = self.work / "campaigns" / "COORDINATION-FIXTURE"
        (self.prep / "campaign").mkdir()
        (self.prep / "rag" / "sources").mkdir(parents=True)
        (self.prep / "rag" / "sources" / "RAG-TEST.md").write_text("Fixture source: preserve the requested action and its condition.\n")
        names = [("or.alpha", "shared"), ("or.beta", "shared"),
                 ("code.example", "software"), ("text.typo-normalization", "text"),
                 ("or.gamma", "independent")]
        allocations = []
        domains = []
        lessons = []
        for i, (domain, family) in enumerate(names, 1):
            aid = "A" + str(i)
            domains.append({"order": i, "domain_id": domain, "target_pairs": 1000})
            allocations.append({"allocation_id": aid, "order": i, "domain_id": domain,
                                "stream_id": aid + "/base", "topic_id": "topic-" + aid,
                                "objective_id": "objective-" + aid, "profile_id": "base",
                                "target_pairs": 1000, "batch_size": 2, "pairs_per_family": 1,
                                "lesson_id": aid, "scenario_group": family,
                                "source_refs": ["RAG-TEST"],
                                "next_allocation_id": "A" + str(i + 1) if i < len(names) else None})
            lessons.append({"lesson_id": aid, "pairs_per_family": 1,
                            "source_refs": ["RAG-TEST"],
                            "source_excerpts": [{"source_id": "RAG-TEST", "path": "rag/sources/RAG-TEST.md",
                                                 "excerpt": "Preserve action and condition."}],
                            "examples": [{"prompt": "Fixture prompt " + str(n), "chosen": "Correct condition " + str(n),
                                          "rejected": "Reversed condition " + str(n), "rationale": "The condition was reversed."}
                                         for n in (1, 2)]})
        registry = {"mode": "CREATE_ONLY", "campaign_id": "COORDINATION-FIXTURE",
                    "domains": domains, "total_target_pairs": 5000, "allocations": allocations,
                    "canonical_windows_learning_sets_root": "C:/Projects/llm_mvp_core/learning-sets"}
        self.write_json(self.prep / "campaign" / "registry.json", registry)
        self.write_json(self.prep / "campaign" / "lessons.json", {"lessons": lessons})
        self.write_json(self.prep / "campaign" / "quality-status-seed.json",
                        {"schema_version": "xiigen.campaign-quality-seed.v1", "findings": [
                            {"flag_id": "sample-needs-assessment", "domain_id": "code.example",
                             "topic_id": "topic-A3", "profile_id": "base", "status": "needs_assessment",
                             "reason": "Fixture requires source-body assessment.", "evidence": ["fixture:source-unavailable"]}]})

    def tearDown(self):
        self.temporary.cleanup()

    @staticmethod
    def write_json(path, value):
        path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    def run_cli(self, *args, ok=True):
        result = subprocess.run([sys.executable, str(self.script), *map(str, args)],
                                capture_output=True, text=True, encoding="utf-8", timeout=20)
        value = json.loads(result.stdout)
        if ok:
            self.assertEqual(result.returncode, 0, value)
        else:
            self.assertEqual(result.returncode, 2, value)
        return value

    def next(self, owner, pool="all", allocation=None):
        args = ["next", "--work-root", str(self.work), "--agent-id", owner, "--pool", pool]
        if allocation:
            args += ["--allocation", allocation]
        return self.run_cli(*args)

    def request(self, result):
        return json.loads(Path(result["paths"]["request"]).read_text(encoding="utf-8"))

    def args_for(self, result, action):
        identity = result["coordination"]
        return [action, "--request", result["paths"]["request"], "--agent-id", identity["owner_id"],
                "--claim-token", identity["claim_token"]]

    def author_fixture(self, result, count=None):
        request = self.request(result)
        rows = []
        materials = []
        for slot in request["row_slots"][:count]:
            fid = slot["family_id"]
            row = {"schema_version": "xiigen.dpo.v1", "pair_id": slot["pair_id"],
                   "domain_id": request["domain_id"], "topic_id": request["topic_id"],
                   "profile_id": "base", "family_id": fid, "split": "train",
                   "prompt": "Fixture action for " + slot["pair_id"] + ": save only when valid.",
                   "chosen": "Save only when valid.", "rejected": "Save only when invalid.",
                   "error_tag": "reversed-condition", "rationale": "The rejected answer reverses the visible condition.",
                   "source_refs": ["RAG-TEST"], "provenance": {"kind": "test-fixture", "generator_model": "fixture"},
                   "metadata": {"origin_family_id": fid, "learning_objective_id": request["objective_id"],
                                "scenario_group": request["scenario_group"], "review_status": "not_requested",
                                "measured_training_result": False, "leakage_group_ids": [fid]}}
            rows.append(row)
            materials.append({"family_id": fid, "origin_family_id": fid,
                              "scenario_group": request["scenario_group"],
                              "scenario": "Save the described fixture item after validation.",
                              "semantic_facts": ["A valid item is saved; an invalid item is not saved."]})
        Path(request["paths"]["draft"]).write_text("".join(json.dumps(row) + "\n" for row in rows), encoding="utf-8")
        Path(request["paths"]["family_material"]).write_text("".join(json.dumps(row) + "\n" for row in materials), encoding="utf-8")

    def state(self):
        return json.loads((self.campaign / "state.json").read_text(encoding="utf-8"))

    def test_simultaneous_claims_choose_distinct_domains_and_protect_shared_families(self):
        with ThreadPoolExecutor(max_workers=2) as executor:
            results = list(executor.map(lambda who: self.next(who, "orchestration"), ["orch-one", "orch-two"]))
        self.assertEqual({r["allocation_id"] for r in results}, {"A1", "A5"})
        state = self.state()
        self.assertEqual(len(state["claims"]), 2)
        self.assertEqual(len(state["active_requests"]), 2)
        self.assertFalse((self.campaign / "A2").exists())
        self.assertEqual(len({r["coordination"]["claim_token"] for r in results}), 2)

    def test_three_pools_do_not_duplicate_and_status_matches_registry(self):
        tasks = [("translation-one", "translations"), ("orch-one", "orchestration"), ("software-one", "software")]
        with ThreadPoolExecutor(max_workers=3) as executor:
            results = list(executor.map(lambda task: self.next(*task), tasks))
        self.assertEqual({r["domain_id"] for r in results}, {"text.typo-normalization", "or.alpha", "code.example"})
        status = self.run_cli("status", "--work-root", self.work)
        self.assertEqual(status["target_pairs"], 5000)
        self.assertEqual(len(status["active_requests"]), 3)
        software = next(d for d in status["domains"] if d["domain_id"] == "code.example")
        self.assertEqual(software["quality_status"], "needs_assessment")
        self.assertEqual(json.loads((self.campaign / "status.json").read_text())["active_requests"], status["active_requests"])

    def test_non_owner_and_stale_token_cannot_finish_or_change_progress(self):
        result = self.next("owner", "software")
        self.author_fixture(result)
        before = self.state()
        args = self.args_for(result, "finish") + ["--model", "fixture"]
        args[args.index("--agent-id") + 1] = "intruder"
        self.run_cli(*args, ok=False)
        self.assertEqual(self.state(), before)
        args = self.args_for(result, "finish") + ["--model", "fixture"]
        args[args.index("--claim-token") + 1] = "stale"
        self.run_cli(*args, ok=False)
        self.assertEqual(self.state(), before)

    def test_partial_rows_are_visible_but_not_counted_as_completed(self):
        result = self.next("owner", "software")
        request = self.request(result)
        draft = Path(request["paths"]["draft"])
        pair_id = request["row_slots"][0]["pair_id"]
        triple = {"pair_id": pair_id, "prompt": "Save only when valid.",
                  "chosen": "Save a valid item.", "rejected": "Save an invalid item."}
        before = self.state()
        invalid_rows = [{"pair_id": pair_id}, dict(triple, prompt=" "),
                        dict(triple, chosen=7), dict(triple, rejected=""),
                        dict(triple, rejected=triple["chosen"])]
        for row in invalid_rows:
            with self.subTest(row=row):
                draft.write_text(json.dumps(row) + "\n", encoding="utf-8")
                self.run_cli(*self.args_for(result, "progress"), ok=False)
                self.assertEqual(self.state(), before)
        # Progress requires saved triple text, but not complete finish metadata.
        draft.write_text(json.dumps(triple) + "\n", encoding="utf-8")
        progress = self.run_cli(*self.args_for(result, "progress"))
        self.assertEqual(progress["authoring_rows_saved"], 1)
        self.assertEqual(progress["credited_pairs"], 0)
        self.assertEqual(self.state()["allocation_progress"], {})
        self.author_fixture(result, 1)
        self.run_cli(*(self.args_for(result, "finish") + ["--model", "fixture"]), ok=False)
        self.assertEqual(self.state()["allocation_progress"], {})
        self.assertTrue(Path(result["paths"]["draft"]).is_file())

    def test_parallel_finishes_preserve_both_counts_and_family_material(self):
        results = [self.next("software-one", "software"), self.next("text-one", "translations")]
        for result in results:
            self.author_fixture(result)
        with ThreadPoolExecutor(max_workers=2) as executor:
            finished = list(executor.map(lambda result: self.run_cli(*(self.args_for(result, "finish") + ["--model", "fixture"])), results))
        self.assertEqual({r["status"] for r in finished}, {"DRAFT_CREATED"})
        self.assertEqual(sum(self.state()["allocation_progress"].values()), 4)
        self.assertEqual(len(self.state()["completed_requests"]), 2)
        ledger = [json.loads(line) for line in (self.campaign / "family-material.jsonl").read_text().splitlines()]
        self.assertEqual(len(ledger), 4)
        self.assertEqual(len({row["family_id"] for row in ledger}), 4)

    def test_finish_is_idempotent_and_rejects_changed_completed_file(self):
        result = self.next("owner", "software")
        self.author_fixture(result)
        args = self.args_for(result, "finish") + ["--model", "fixture"]
        self.run_cli(*args)
        before = self.state()
        again = self.run_cli(*args)
        self.assertEqual(again["status"], "DRAFT_CREATED")
        self.assertEqual(self.state(), before)
        draft = Path(result["paths"]["draft"])
        draft.write_text(draft.read_text() + "\n")
        self.run_cli(*args, ok=False)
        self.assertEqual(self.state(), before)

    def test_restart_returns_exact_owned_request_and_keeps_original_bytes(self):
        result = self.next("owner", "software")
        self.author_fixture(result, 1)
        originals = {name: Path(result["paths"][name]).read_bytes() for name in ("request", "draft", "family_material")}
        resumed = self.next("owner", "software")
        self.assertEqual(resumed["paths"]["request"], result["paths"]["request"])
        self.assertEqual(resumed["coordination"]["claim_token"], result["coordination"]["claim_token"])
        for name, content in originals.items():
            self.assertEqual(Path(result["paths"][name]).read_bytes(), content)

    def test_repeated_explicit_next_resumes_own_claim_and_blocks_other_owner(self):
        result = self.next("owner", "orchestration", "A1")
        original = Path(result["paths"]["request"]).read_bytes()
        repeated = self.next("owner", "orchestration", "A1")
        self.assertEqual(repeated["status"], "PREPARED")
        self.assertEqual(repeated["action"], "RESUME_OWNED_REQUEST")
        self.assertEqual(repeated["paths"]["request"], result["paths"]["request"])
        self.assertEqual(repeated["coordination"]["claim_token"], result["coordination"]["claim_token"])
        self.assertEqual(Path(result["paths"]["request"]).read_bytes(), original)
        other = self.next("other-owner", "orchestration", "A1")
        self.assertEqual(other["status"], "WAITING_FOR_OWNERS")
        self.assertEqual(self.state()["claims"]["or.alpha"]["owner_id"], "owner")

    def test_explicit_release_then_adoption_rotates_token_without_overwrite(self):
        result = self.next("owner", "software")
        self.author_fixture(result, 1)
        draft = Path(result["paths"]["draft"]).read_bytes()
        self.run_cli(*self.args_for(result, "release"))
        adopted = self.run_cli("resume", "--request", result["paths"]["request"], "--agent-id", "new-owner")
        self.assertEqual(adopted["paths"]["request"], result["paths"]["request"])
        self.assertNotEqual(adopted["coordination"]["claim_token"], result["coordination"]["claim_token"])
        self.assertEqual(Path(result["paths"]["draft"]).read_bytes(), draft)
        self.run_cli(*self.args_for(result, "progress"), ok=False)

    def test_old_timestamp_does_not_steal_live_owner(self):
        result = self.next("owner", "software")
        state = self.state()
        state["claims"]["code.example"]["updated_at"] = "2000-01-01T00:00:00+00:00"
        self.write_json(self.campaign / "state.json", state)
        waiting = self.next("another-owner", "software")
        self.assertEqual(waiting["status"], "WAITING_FOR_OWNERS")
        self.assertEqual(self.state()["claims"]["code.example"]["owner_id"], "owner")
        self.assertEqual(self.state()["claims"]["code.example"]["token"], result["coordination"]["claim_token"])

    def test_os_lock_releases_after_process_exit(self):
        program = "import os,sys; from campaign_status import transaction\nwith transaction(sys.argv[1], 'COORDINATION-FIXTURE'):\n os._exit(17)\n"
        process = subprocess.run([sys.executable, "-c", program, str(self.work)], cwd=self.tools, timeout=10)
        self.assertEqual(process.returncode, 17)
        result = self.next("owner", "software")
        self.assertEqual(result["status"], "PREPARED")

    def test_quality_flags_persist_and_do_not_change_pair_counts(self):
        self.next("owner", "software")
        self.run_cli("flag", "--work-root", self.work, "--agent-id", "assessor", "--flag-id", "sample-needs-assessment",
                     "--domain", "code.example", "--topic", "topic-A3", "--profile", "base",
                     "--quality-status", "needs_rework", "--reason", "Visible counterexample failed.",
                     "--evidence", "fixture:row-3")
        status = self.run_cli("status", "--work-root", self.work)
        software = next(d for d in status["domains"] if d["domain_id"] == "code.example")
        self.assertEqual(software["quality_status"], "needs_rework")
        self.assertEqual(status["draft_created_pairs"], 0)
        self.assertEqual(len(self.state()["quality_flag_history"]), 1)

    def test_report_written_before_state_commit_recovers_without_double_count(self):
        result = self.next("owner", "software")
        self.author_fixture(result)
        before_commit = self.state()
        self.run_cli(*(self.args_for(result, "finish") + ["--model", "fixture"]))
        # Reproduce an interrupted last step: artifacts persisted, state not yet replaced.
        self.write_json(self.campaign / "state.json", before_commit)
        following = self.next("owner", "software")
        self.assertEqual(self.state()["allocation_progress"]["A3"], 2)
        self.assertEqual(len(self.state()["completed_requests"]), 1)
        self.assertNotEqual(following["paths"]["request"], result["paths"]["request"])
        self.assertEqual(following["batch_number"], 2)

    def test_explicit_next_preserves_old_partial_and_invalidates_its_token(self):
        result = self.next("owner", "all", "A1")
        self.author_fixture(result, 1)
        draft = Path(result["paths"]["draft"]).read_bytes()
        following = self.next("owner", "all", "A3")
        self.assertEqual(following["allocation_id"], "A3")
        self.assertEqual(Path(result["paths"]["draft"]).read_bytes(), draft)
        self.assertEqual(self.state()["allocation_progress"], {})
        self.assertNotIn("or.alpha", self.state()["claims"])
        self.run_cli(*self.args_for(result, "progress"), ok=False)

    def test_supplement_is_selected_for_exact_objective_not_wrong_neighbor(self):
        examples = [{"example_id": "extra-right", "domain_id": "code.example", "objective_id": "objective-A3",
                     "profile_id": "base", "prompt": "Additional exact objective prompt", "chosen": "Preserve the condition.",
                     "rejected": "Reverse the condition.", "simple_check_en": "Read the condition in the visible prompt."},
                    {"example_id": "extra-wrong", "domain_id": "code.example", "objective_id": "unrelated",
                     "profile_id": "base", "prompt": "Unrelated prompt", "chosen": "Keep X.", "rejected": "Drop X.",
                     "simple_check_en": "This must not enter another topic."}]
        self.write_json(self.prep / "campaign" / "software-quality-lessons-2026-10-26.json", {"domains": [{"domain_id": "code.example", "examples": examples}]})
        result = self.next("owner", "software")
        selected = self.request(result)["examples"]
        self.assertEqual(len(selected), 3)
        self.assertEqual(selected[0]["prompt"], "Fixture prompt 1")
        self.assertEqual(selected[1]["prompt"], "Fixture prompt 2")
        self.assertEqual(selected[2]["example_id"], "extra-right")
        self.assertEqual(selected[2]["rationale"], examples[0]["simple_check_en"])

    def test_completed_resume_returns_working_caller_next_without_write_commands(self):
        result = self.next("original-owner", "software")
        self.author_fixture(result)
        self.run_cli(*(self.args_for(result, "finish") + ["--model", "fixture"]))
        completed_state = self.state()
        original_request = Path(result["paths"]["request"]).read_bytes()
        resumed = self.run_cli("resume", "--request", result["paths"]["request"], "--agent-id", "new-caller")
        self.assertEqual(set(resumed["commands"]), {"next", "status"})
        self.assertEqual(resumed["creation_owner_id"], "original-owner")
        self.assertEqual(resumed["continuation_owner_id"], "new-caller")
        self.assertEqual(self.state(), completed_state)
        self.assertEqual(Path(result["paths"]["request"]).read_bytes(), original_request)
        # Execute the exact returned POSIX command, not a reconstructed equivalent.
        if os.name != "nt":
            process = subprocess.run(shlex.split(resumed["commands"]["next"]), capture_output=True,
                                     text=True, encoding="utf-8", timeout=20)
            self.assertEqual(process.returncode, 0, process.stdout)
            following = json.loads(process.stdout)
            self.assertEqual(following["coordination"]["owner_id"], "new-caller")
            self.assertEqual(following["batch_number"], 2)
            self.assertEqual(self.state()["completed_requests"][result["paths"]["request"]]["owner_id"], "original-owner")

    def test_resumed_view_gets_current_guidance_and_exact_material_path(self):
        result = self.next("owner", "software")
        original = Path(result["paths"]["request"]).read_bytes()
        example = {"example_id": "added-after-request", "domain_id": "code.example", "objective_id": "objective-A3",
                   "profile_id": "base", "prompt": "Newly supplied exact-objective example", "chosen": "Preserve the condition.",
                   "rejected": "Reverse the condition.", "simple_check_en": "Check the requested condition."}
        self.write_json(self.prep / "campaign" / "software-quality-lessons-2026-10-26.json", {"domains": [{"domain_id": "code.example", "examples": [example]}]})
        resumed = self.next("owner", "software")
        prompt = Path(resumed["paths"]["prompt"]).read_text(encoding="utf-8")
        self.assertIn("Newly supplied exact-objective example", prompt)
        self.assertIn("For missing families, read " + resumed["paths"]["material_required"], prompt)
        self.assertEqual(Path(result["paths"]["request"]).read_bytes(), original)
        self.author_fixture(resumed)
        draft = Path(resumed["paths"]["draft"])
        rows = [json.loads(line) for line in draft.read_text().splitlines()]
        for field in ("prompt", "chosen", "rejected"):
            rows[0][field] = example[field]
        draft.write_text("".join(json.dumps(row) + "\n" for row in rows))
        self.run_cli(*(self.args_for(resumed, "finish") + ["--model", "fixture"]), ok=False)
        self.assertEqual(self.state()["allocation_progress"], {})


if __name__ == "__main__":
    unittest.main(verbosity=2)
