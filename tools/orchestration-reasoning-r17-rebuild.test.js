"use strict";

const assert = require("node:assert/strict");
const crypto = require("node:crypto");
const fs = require("node:fs");
const os = require("node:os");
const path = require("node:path");
const { execFileSync, spawnSync } = require("node:child_process");
const test = require("node:test");

const repoRoot = path.resolve(__dirname, "..");
const tool = path.join(__dirname, "orchestration-reasoning-r17-rebuild.js");
const sourceCommit = "3a2dc5cb33f43c0b5c80a1834d3a1a88e73b2482";
const outputPaths = Object.freeze([
  "training-data/orchestration-reasoning/or-01u-universalization-candidates.jsonl",
  "training-data/orchestration-reasoning/orchestration-reasoning-common-corpus-manifest.json",
  "training-data/orchestration-reasoning/orchestration-reasoning-common-corpus.jsonl",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-bindings-manifest.json",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-bindings.jsonl",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-continue-delta-payloads-manifest.json",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-continue-delta-payloads.jsonl",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs-manifest.json",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs.jsonl",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-trainer-payloads-manifest.json",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-trainer-payloads.jsonl",
  "training-data/orchestration-reasoning/orchestration-reasoning-held-out-guide-pattern-fixtures-manifest.json",
  "training-data/orchestration-reasoning/orchestration-reasoning-held-out-guide-pattern-fixtures.jsonl",
  "training-data/orchestration-reasoning/quarantine/r17/manifest.json",
  "training-data/orchestration-reasoning/quarantine/r17/source-rows.jsonl",
  "training-data/orchestration-reasoning/source-admission-ledger.json",
  "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/dataset-card.json",
  "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/orchestration-reasoning-expanded-dpo.jsonl",
  "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/schema.json",
  "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/split.json"
]);
const targetOnly = new Set([
  "training-data/orchestration-reasoning/quarantine/r17/manifest.json",
  "training-data/orchestration-reasoning/quarantine/r17/source-rows.jsonl",
  "training-data/orchestration-reasoning/source-admission-ledger.json"
]);
const zeroPaths = new Set([
  outputPaths[0], outputPaths[2], outputPaths[4], outputPaths[8], outputPaths[10]
]);
const targetText = Object.freeze({
  "training-data/orchestration-reasoning/orchestration-reasoning-common-corpus-manifest.json": "{\n  \"schema\": \"xiigen-orchestration-reasoning-common-corpus-manifest-v1\",\n  \"schema_version\": \"1.0\",\n  \"work_order\": \"F35A-2-R17-QUARANTINE-REBUILD\",\n  \"status\": \"REBUILD_PENDING_NO_ADMITTED_ROWS\",\n  \"claim_scope\": \"zero active common corpus rows after R17 quarantine; no source admission, DPO finalization, train-ready data, model mutation, or training\",\n  \"corpus_file_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-common-corpus.jsonl\",\n  \"corpus_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n  \"row_count\": 0,\n  \"included_common_row_ids\": [],\n  \"excluded_adaptive_row_count\": 0,\n  \"excluded_adaptive_refs\": [],\n  \"source_refs\": [],\n  \"model_leg\": \"common\",\n  \"raw_private_text_copied\": false,\n  \"technical_execution_leakage_count\": 0,\n  \"technical_execution_leakage\": false,\n  \"dpo_finalized\": false,\n  \"dpo_train_ready\": false,\n  \"training_started\": false,\n  \"no_hidden_deterministic_shortcut\": true,\n  \"source_admission_ledger_ref\": \"training-data/orchestration-reasoning/source-admission-ledger.json\",\n  \"source_admission_status\": \"pending_no_admitted_rows\",\n  \"computed_training_allowed\": false,\n  \"quarantine_manifest_ref\": \"training-data/orchestration-reasoning/quarantine/r17/manifest.json\"\n}\n",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-bindings-manifest.json": "{\n  \"schema\": \"xiigen-orchestration-reasoning-dpo-bindings-manifest-v1\",\n  \"schema_version\": \"1.0\",\n  \"work_order\": \"F35A-2-R17-QUARANTINE-REBUILD\",\n  \"status\": \"REBUILD_PENDING_NO_ADMITTED_ROWS\",\n  \"claim_scope\": \"zero active DPO bindings after R17 quarantine; no source admission, DPO readiness, model mutation, or training\",\n  \"bindings_file_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-dpo-bindings.jsonl\",\n  \"bindings_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n  \"row_count\": 0,\n  \"source_corpus_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-common-corpus.jsonl\",\n  \"source_corpus_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n  \"source_manifest_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-common-corpus-manifest.json\",\n  \"source_manifest_sha256\": \"770dd9019c628525e3457fc4c7d6ec00c7f36d2685029de293401cf6b2eee809\",\n  \"source_candidate_ref\": \"training-data/orchestration-reasoning/or-01u-universalization-candidates.jsonl\",\n  \"source_candidate_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n  \"included_source_row_ids\": [],\n  \"excluded_adaptive_row_count\": 0,\n  \"excluded_adaptive_refs\": [],\n  \"model_leg\": \"common\",\n  \"raw_private_text_copied\": false,\n  \"technical_execution_leakage\": false,\n  \"dpo_binding_ready\": false,\n  \"dpo_binding_ready_count\": 0,\n  \"dpo_train_ready\": false,\n  \"training_started\": false,\n  \"dpo_finalized\": false,\n  \"model_asset_created\": false,\n  \"checkpoint_created\": false,\n  \"external_ai_used\": false,\n  \"legacy_orchestration_plan_machine_continue_command_used\": false,\n  \"source_admission_ledger_ref\": \"training-data/orchestration-reasoning/source-admission-ledger.json\",\n  \"source_admission_status\": \"pending_no_admitted_rows\",\n  \"computed_training_allowed\": false,\n  \"quarantine_manifest_ref\": \"training-data/orchestration-reasoning/quarantine/r17/manifest.json\"\n}\n",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-continue-delta-payloads-manifest.json": "{\n  \"schema\": \"xiigen-orchestration-reasoning-dpo-continue-delta-payloads-manifest-v1\",\n  \"schema_version\": \"1.0\",\n  \"work_order\": \"F35A-2-R17-QUARANTINE-REBUILD\",\n  \"status\": \"RETAINED_NOT_SOURCE_ADMITTED\",\n  \"claim_scope\": \"one retained ref-only universal OrchestrationReasoning DPO delta; no evidence-bound source admission, train-ready claim, model mutation, checkpoint write, or training\",\n  \"payloads_file_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-dpo-continue-delta-payloads.jsonl\",\n  \"payloads_sha256\": \"6dec44eebe9b46f0d1b34b0508f916393f797017d293c9766bdef447ce586f7f\",\n  \"row_count\": 1,\n  \"source_pairs_ref\": \"artifacts/orchestration-reasoning-lifecycle/OR-04B-continue-training-command-01.json\",\n  \"source_pairs_sha256\": \"392c7ceb7d63675735e3f75cd94405f4747eddc9e4bf0e7a01cfdeed0016ca44\",\n  \"included_pair_ids\": [\n    \"or-dpo-pair:or04b-continue-delta-evidence-boundary-001\"\n  ],\n  \"model_leg\": \"common\",\n  \"abstraction_layer\": \"universal_reasoning\",\n  \"raw_private_text_copied\": false,\n  \"technical_execution_leakage\": false,\n  \"external_ai_used\": false,\n  \"dpo_payload_authored\": true,\n  \"dpo_train_ready\": false,\n  \"training_started\": false,\n  \"dpo_finalized\": false,\n  \"model_asset_created\": false,\n  \"checkpoint_created\": false,\n  \"temp_command_fixture_reused\": false,\n  \"can_feed_next_checkpoint_writer_after_parent_acceptance\": false,\n  \"source_admission_ledger_ref\": \"training-data/orchestration-reasoning/source-admission-ledger.json\",\n  \"source_admission_status\": \"pending_no_admitted_rows\",\n  \"computed_training_allowed\": false,\n  \"quarantine_manifest_ref\": \"training-data/orchestration-reasoning/quarantine/r17/manifest.json\"\n}\n",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs-manifest.json": "{\n  \"schema\": \"xiigen-orchestration-reasoning-dpo-pairs-manifest-v1\",\n  \"schema_version\": \"1.0\",\n  \"work_order\": \"F35A-2-R17-QUARANTINE-REBUILD\",\n  \"status\": \"REBUILD_PENDING_NO_ADMITTED_ROWS\",\n  \"claim_scope\": \"zero active DPO pairs after R17 quarantine; no source admission, materialization, model mutation, or training\",\n  \"pairs_file_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs.jsonl\",\n  \"pairs_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n  \"row_count\": 0,\n  \"source_bindings_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-dpo-bindings.jsonl\",\n  \"source_bindings_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n  \"source_bindings_manifest_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-dpo-bindings-manifest.json\",\n  \"source_bindings_manifest_sha256\": \"6fe618c76dd71f8c5bcf948e5a02827426b191ffb9246129ba25a5bb7aebd01a\",\n  \"source_common_corpus_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-common-corpus.jsonl\",\n  \"source_common_corpus_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n  \"source_common_manifest_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-common-corpus-manifest.json\",\n  \"source_common_manifest_sha256\": \"770dd9019c628525e3457fc4c7d6ec00c7f36d2685029de293401cf6b2eee809\",\n  \"included_source_row_ids\": [],\n  \"excluded_adaptive_row_count\": 0,\n  \"model_leg\": \"common\",\n  \"abstraction_layer\": \"universal_reasoning\",\n  \"raw_private_text_copied\": false,\n  \"technical_execution_leakage\": false,\n  \"dpo_binding_ready\": false,\n  \"dpo_materialized\": false,\n  \"dpo_materialized_count\": 0,\n  \"dpo_train_ready\": false,\n  \"training_started\": false,\n  \"dpo_finalized\": false,\n  \"model_asset_created\": false,\n  \"checkpoint_created\": false,\n  \"external_ai_used\": false,\n  \"source_admission_ledger_ref\": \"training-data/orchestration-reasoning/source-admission-ledger.json\",\n  \"source_admission_status\": \"pending_no_admitted_rows\",\n  \"computed_training_allowed\": false,\n  \"quarantine_manifest_ref\": \"training-data/orchestration-reasoning/quarantine/r17/manifest.json\"\n}\n",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-trainer-payloads-manifest.json": "{\n  \"schema\": \"xiigen-orchestration-reasoning-dpo-trainer-payloads-manifest-v1\",\n  \"schema_version\": \"1.0\",\n  \"work_order\": \"F35A-2-R17-QUARANTINE-REBUILD\",\n  \"status\": \"REBUILD_PENDING_NO_ADMITTED_ROWS\",\n  \"claim_scope\": \"zero active trainer payload rows after R17 quarantine; no source admission, train-ready data, model mutation, or training\",\n  \"payloads_file_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-dpo-trainer-payloads.jsonl\",\n  \"payloads_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n  \"row_count\": 0,\n  \"source_pairs_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs.jsonl\",\n  \"source_pairs_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n  \"source_pairs_manifest_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs-manifest.json\",\n  \"source_pairs_manifest_sha256\": \"008d9b7c1e7f02a4efe401668621713ec44d9ce4e00a72507e1f908dda76a4c2\",\n  \"included_pair_ids\": [],\n  \"model_leg\": \"common\",\n  \"abstraction_layer\": \"universal_reasoning\",\n  \"raw_private_text_copied\": false,\n  \"technical_execution_leakage\": false,\n  \"external_ai_used\": false,\n  \"dpo_payload_authored\": false,\n  \"dpo_train_ready\": false,\n  \"training_started\": false,\n  \"dpo_finalized\": false,\n  \"model_asset_created\": false,\n  \"checkpoint_created\": false,\n  \"source_admission_ledger_ref\": \"training-data/orchestration-reasoning/source-admission-ledger.json\",\n  \"source_admission_status\": \"pending_no_admitted_rows\",\n  \"computed_training_allowed\": false,\n  \"quarantine_manifest_ref\": \"training-data/orchestration-reasoning/quarantine/r17/manifest.json\"\n}\n",
  "training-data/orchestration-reasoning/orchestration-reasoning-held-out-guide-pattern-fixtures-manifest.json": "{\n  \"schema_version\": \"orchestration-reasoning-held-out-guide-pattern-fixtures-v1\",\n  \"fixture_ref\": \"training-data/orchestration-reasoning/orchestration-reasoning-held-out-guide-pattern-fixtures.jsonl\",\n  \"fixture_sha256\": \"0f6c11c2ca5c1ae8cddf1409642a6f5b2b8fe367cb669300cb0d4d7c99e72b1e\",\n  \"row_count\": 1,\n  \"row_novelty_classes\": [\n    \"diagnostic_near_neighbor\"\n  ],\n  \"accepted_ref_ids_excluded\": true,\n  \"source_disjoint_from_active_train_rows\": true,\n  \"raw_private_text_copied\": false,\n  \"technical_execution_leakage\": false,\n  \"external_ai_used\": false,\n  \"training_started\": false,\n  \"checkpoint_mutation_allowed\": false,\n  \"claim_scope\": \"one retained ref-only diagnostic held-out row after R17 quarantine; not admitted training material and no checkpoint mutation\",\n  \"source_admission_ledger_ref\": \"training-data/orchestration-reasoning/source-admission-ledger.json\",\n  \"source_admission_status\": \"pending_no_admitted_rows\",\n  \"computed_training_allowed\": false,\n  \"quarantine_manifest_ref\": \"training-data/orchestration-reasoning/quarantine/r17/manifest.json\"\n}\n",
  "training-data/orchestration-reasoning/quarantine/r17/manifest.json": "{\n  \"schema\": \"xiigen-orchestration-reasoning-r17-quarantine-manifest-v1\",\n  \"schema_version\": 1,\n  \"policy_id\": \"R17\",\n  \"status\": \"QUARANTINED_NOT_TRAINABLE\",\n  \"source_baseline_commit\": \"3a2dc5cb33f43c0b5c80a1834d3a1a88e73b2482\",\n  \"source_admission_parent_commit\": \"ca440dc8637044009f77f8fb73d4c85420f2ed22\",\n  \"source_rows_ref\": \"training-data/orchestration-reasoning/quarantine/r17/source-rows.jsonl\",\n  \"source_rows_sha256\": \"039c150b087d227878ed816316d8f1558707209052608db9368268459eadede3\",\n  \"source_row_count\": 4,\n  \"source_rows\": [\n    {\n      \"row_id\": \"or01u-sonnet-clinerules-001\",\n      \"model_leg\": \"common\",\n      \"source_ref\": \"sonnet-local-analyzes:.clinerules-analysis.md\",\n      \"source_sha256\": \"af174a555ca99292c0c87210665536dec6d520f2bd00532397ccf9318383fb05\"\n    },\n    {\n      \"row_id\": \"or01u-sonnet-gitignore-001\",\n      \"model_leg\": \"common\",\n      \"source_ref\": \"sonnet-local-analyzes:.gitignore-analysis.md\",\n      \"source_sha256\": \"fe16ff2ddcb4eac73110997e47eb3893b6158ae726b37fb20ba3977303f8ae0c\"\n    },\n    {\n      \"row_id\": \"or01u-sonnet-gitattributes-001\",\n      \"model_leg\": \"common\",\n      \"source_ref\": \"sonnet-local-analyzes:.gitattributes-analysis.md\",\n      \"source_sha256\": \"15ee6946aa77f025b11e04ad8f73c801009ebbf56e365a4fe6d2bb43a08d64f5\"\n    },\n    {\n      \"row_id\": \"or01u-sonnet-cursorrules-001\",\n      \"model_leg\": \"adaptive\",\n      \"source_ref\": \"sonnet-local-analyzes:.cursorrules-analysis.md\",\n      \"source_sha256\": \"a106495ccbfc9cbc2651d8070fc0b4c75d2d3b769c97b793d69eb8826299d564\"\n    }\n  ],\n  \"removed_active_material\": [\n    {\n      \"path\": \"training-data/orchestration-reasoning/or-01u-universalization-candidates.jsonl\",\n      \"before_sha256\": \"039c150b087d227878ed816316d8f1558707209052608db9368268459eadede3\",\n      \"before_row_count\": 4,\n      \"after_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n      \"after_row_count\": 0,\n      \"removed_row_ids\": [\n        \"or01u-sonnet-clinerules-001\",\n        \"or01u-sonnet-gitignore-001\",\n        \"or01u-sonnet-gitattributes-001\",\n        \"or01u-sonnet-cursorrules-001\"\n      ]\n    },\n    {\n      \"path\": \"training-data/orchestration-reasoning/orchestration-reasoning-common-corpus.jsonl\",\n      \"before_sha256\": \"1416a55c9f74703b3c5f29db270d054132328e802b6615dbd8566c6214be5c56\",\n      \"before_row_count\": 3,\n      \"after_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n      \"after_row_count\": 0\n    },\n    {\n      \"path\": \"training-data/orchestration-reasoning/orchestration-reasoning-dpo-bindings.jsonl\",\n      \"before_sha256\": \"630cb5cf42bcea0e669be7adb5cfa23496ab87e68ec43ab9baae5b1c4199d5b9\",\n      \"before_row_count\": 3,\n      \"after_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n      \"after_row_count\": 0\n    },\n    {\n      \"path\": \"training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs.jsonl\",\n      \"before_sha256\": \"f9f002a79e2bd7be569f56091b1c4e3bcdb9bc132ac0a020b952c90027fe2243\",\n      \"before_row_count\": 3,\n      \"after_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n      \"after_row_count\": 0\n    },\n    {\n      \"path\": \"training-data/orchestration-reasoning/orchestration-reasoning-dpo-trainer-payloads.jsonl\",\n      \"before_sha256\": \"50edb89ed35cd2613a15cad038445c26cf7f8487f4857a60b9d509f20d0f7c8e\",\n      \"before_row_count\": 3,\n      \"after_sha256\": \"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\",\n      \"after_row_count\": 0\n    },\n    {\n      \"path\": \"training-data/orchestration-reasoning/orchestration-reasoning-held-out-guide-pattern-fixtures.jsonl\",\n      \"before_sha256\": \"11bcbbe047cd22b6cd5a95f79eeb83d5b1ee8d2d7760ed42e8e5db6b30bec961\",\n      \"before_row_count\": 2,\n      \"after_sha256\": \"0f6c11c2ca5c1ae8cddf1409642a6f5b2b8fe367cb669300cb0d4d7c99e72b1e\",\n      \"after_row_count\": 1,\n      \"removed_row_ids\": [\n        \"or-heldout-row:or04b-safety-persistence-diagnostic-001\"\n      ]\n    },\n    {\n      \"path\": \"training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/orchestration-reasoning-expanded-dpo.jsonl\",\n      \"before_sha256\": \"7cc7c94d3d1d160807eebc404a132342479eb74b50300de638c8c80ac24a95f7\",\n      \"before_row_count\": 12,\n      \"after_sha256\": \"8f368b0c05ae8375b12f5d5e17ef96e0278485fa954097b5e2dab50939d0a1ff\",\n      \"after_row_count\": 9,\n      \"removed_row_ids\": [\n        \"or-expanded-v1-pair-001\",\n        \"or-expanded-v1-pair-002\",\n        \"or-expanded-v1-pair-003\"\n      ]\n    }\n  ],\n  \"retained_active_material\": [\n    {\n      \"path\": \"training-data/orchestration-reasoning/orchestration-reasoning-dpo-continue-delta-payloads.jsonl\",\n      \"sha256\": \"6dec44eebe9b46f0d1b34b0508f916393f797017d293c9766bdef447ce586f7f\",\n      \"row_count\": 1,\n      \"computed_training_allowed\": false\n    },\n    {\n      \"path\": \"training-data/orchestration-reasoning/orchestration-reasoning-held-out-guide-pattern-fixtures.jsonl\",\n      \"sha256\": \"0f6c11c2ca5c1ae8cddf1409642a6f5b2b8fe367cb669300cb0d4d7c99e72b1e\",\n      \"row_count\": 1,\n      \"computed_training_allowed\": false\n    },\n    {\n      \"path\": \"training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/orchestration-reasoning-expanded-dpo.jsonl\",\n      \"sha256\": \"8f368b0c05ae8375b12f5d5e17ef96e0278485fa954097b5e2dab50939d0a1ff\",\n      \"row_count\": 9,\n      \"train_count\": 5,\n      \"held_out_count\": 4,\n      \"computed_training_allowed\": false\n    }\n  ],\n  \"forbidden_source_family\": \"sonnet-local-analyzes\",\n  \"computed_training_allowed\": false,\n  \"model_load_allowed\": false,\n  \"checkpoint_export_allowed\": false,\n  \"historical_artifacts_rewritten\": false\n}\n",
  "training-data/orchestration-reasoning/source-admission-ledger.json": "{\n  \"schema\": \"xiigen-orchestration-reasoning-source-admission-ledger-v1\",\n  \"schema_version\": 1,\n  \"policy_id\": \"R17\",\n  \"status\": \"REBUILD_PENDING_NO_ADMITTED_ROWS\",\n  \"computed_training_allowed\": false,\n  \"entries\": []\n}\n",
  "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/dataset-card.json": "{\n  \"schema\": \"orchestration-reasoning-expanded-dataset-card-v1\",\n  \"dataset_id\": \"orchestration-reasoning-expanded-v1\",\n  \"machine_target\": \"OrchestrationReasoning\",\n  \"model_leg\": \"common\",\n  \"abstraction_layer\": \"universal_reasoning\",\n  \"claim_scope\": \"R17-retained ref-only common DPO material; three denied derived train rows removed; no source admission, training, checkpoint, runtime, API, or semantic-quality claim\",\n  \"dataset_file_ref\": \"training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/orchestration-reasoning-expanded-dpo.jsonl\",\n  \"dataset_sha256\": \"8f368b0c05ae8375b12f5d5e17ef96e0278485fa954097b5e2dab50939d0a1ff\",\n  \"row_count\": 9,\n  \"split\": {\n    \"train\": 5,\n    \"held_out\": 4,\n    \"source_disjoint\": true\n  },\n  \"source_quality_counts\": {\n    \"gold\": 5,\n    \"silver\": 4,\n    \"rejected\": 0\n  },\n  \"taxonomy_distribution\": {\n    \"ORCH-01\": 2,\n    \"ORCH-02\": 1,\n    \"ORCH-03\": 1,\n    \"ORCH-04\": 1,\n    \"ORCH-06\": 1,\n    \"ORCH-08\": 1,\n    \"ORCH-09\": 2\n  },\n  \"source_families\": [\n    \"project governance rules as universal orchestration authority\",\n    \"semantic-quality plan gates and deterministic-shortcut firewall\",\n    \"DPO authoring and claim-verification skills\",\n    \"OrchestrationReasoning negative taxonomy\"\n  ],\n  \"privacy\": {\n    \"raw_private_text_copied\": false,\n    \"contains_raw_private_text\": false,\n    \"refs_and_hashes_only\": true\n  },\n  \"forbidden_learning_signals_excluded\": [\n    \"code-bound technical execution\",\n    \"repository-specific patch recipes\",\n    \"file paths as decision signal\",\n    \"filenames as decision signal\",\n    \"guide-title lookup\",\n    \"BM25 surrogate\",\n    \"static route table\",\n    \"generic refusal negatives\",\n    \"absurd negatives\"\n  ],\n  \"training_allowed\": false,\n  \"source_admission_ledger_ref\": \"training-data/orchestration-reasoning/source-admission-ledger.json\",\n  \"source_admission_status\": \"pending_no_admitted_rows\",\n  \"computed_training_allowed\": false,\n  \"semantic_quality_claimed\": false,\n  \"next_gate\": \"evidence-bound R17 source admission and an explicit training-authorization slice are required before runtime evaluation\",\n  \"quarantine_manifest_ref\": \"training-data/orchestration-reasoning/quarantine/r17/manifest.json\"\n}\n",
  "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/split.json": "{\n  \"schema\": \"orchestration-reasoning-expanded-split-v1\",\n  \"split_id\": \"orchestration-reasoning-expanded-v1\",\n  \"held_out_set_id\": \"orchestration-reasoning-expanded-v1-heldout\",\n  \"dataset_file_ref\": \"training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/orchestration-reasoning-expanded-dpo.jsonl\",\n  \"dataset_sha256\": \"8f368b0c05ae8375b12f5d5e17ef96e0278485fa954097b5e2dab50939d0a1ff\",\n  \"row_count\": 9,\n  \"train_count\": 5,\n  \"held_out_count\": 4,\n  \"source_disjoint\": true,\n  \"training_allowed\": false,\n  \"source_admission_ledger_ref\": \"training-data/orchestration-reasoning/source-admission-ledger.json\",\n  \"source_admission_status\": \"pending_no_admitted_rows\",\n  \"computed_training_allowed\": false,\n  \"semantic_quality_claimed\": false,\n  \"train_training_pair_ids\": [\n    \"or-expanded-v1-pair-004\",\n    \"or-expanded-v1-pair-005\",\n    \"or-expanded-v1-pair-006\",\n    \"or-expanded-v1-pair-007\",\n    \"or-expanded-v1-pair-008\"\n  ],\n  \"held_out_training_pair_ids\": [\n    \"or-expanded-v1-pair-009\",\n    \"or-expanded-v1-pair-010\",\n    \"or-expanded-v1-pair-011\",\n    \"or-expanded-v1-pair-012\"\n  ],\n  \"source_manifest_ref_overlap_count\": 0,\n  \"split_method\": \"manual_source_disjoint_by_SourceManifestRef\",\n  \"verification_scope\": \"R17-retained material inventory only; not source admission, model training, or semantic-quality evidence\",\n  \"quarantine_manifest_ref\": \"training-data/orchestration-reasoning/quarantine/r17/manifest.json\"\n}\n"
});
const expected = new Map([
  [
    "training-data/orchestration-reasoning/or-01u-universalization-candidates.jsonl",
    {
      "bytes": 0,
      "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
    }
  ],
  [
    "training-data/orchestration-reasoning/orchestration-reasoning-common-corpus-manifest.json",
    {
      "bytes": 1257,
      "sha256": "770dd9019c628525e3457fc4c7d6ec00c7f36d2685029de293401cf6b2eee809"
    }
  ],
  [
    "training-data/orchestration-reasoning/orchestration-reasoning-common-corpus.jsonl",
    {
      "bytes": 0,
      "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
    }
  ],
  [
    "training-data/orchestration-reasoning/orchestration-reasoning-dpo-bindings-manifest.json",
    {
      "bytes": 1968,
      "sha256": "6fe618c76dd71f8c5bcf948e5a02827426b191ffb9246129ba25a5bb7aebd01a"
    }
  ],
  [
    "training-data/orchestration-reasoning/orchestration-reasoning-dpo-bindings.jsonl",
    {
      "bytes": 0,
      "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
    }
  ],
  [
    "training-data/orchestration-reasoning/orchestration-reasoning-dpo-continue-delta-payloads-manifest.json",
    {
      "bytes": 1678,
      "sha256": "7e2f0604896549b0e4bb4ed829597d77dea97a68c2ceb1b67de2d30e86fb1a59"
    }
  ],
  [
    "training-data/orchestration-reasoning/orchestration-reasoning-dpo-continue-delta-payloads.jsonl",
    {
      "bytes": 2887,
      "sha256": "6dec44eebe9b46f0d1b34b0508f916393f797017d293c9766bdef447ce586f7f"
    }
  ],
  [
    "training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs-manifest.json",
    {
      "bytes": 2188,
      "sha256": "008d9b7c1e7f02a4efe401668621713ec44d9ce4e00a72507e1f908dda76a4c2"
    }
  ],
  [
    "training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs.jsonl",
    {
      "bytes": 0,
      "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
    }
  ],
  [
    "training-data/orchestration-reasoning/orchestration-reasoning-dpo-trainer-payloads-manifest.json",
    {
      "bytes": 1667,
      "sha256": "7690865602e19c2472774a61f6c1b4f03fd69819cc1442f46e3091710e10e142"
    }
  ],
  [
    "training-data/orchestration-reasoning/orchestration-reasoning-dpo-trainer-payloads.jsonl",
    {
      "bytes": 0,
      "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
    }
  ],
  [
    "training-data/orchestration-reasoning/orchestration-reasoning-held-out-guide-pattern-fixtures-manifest.json",
    {
      "bytes": 1078,
      "sha256": "0135848973721d30cca608ef04c117739c69dcb43f59ec749464feb154b193a1"
    }
  ],
  [
    "training-data/orchestration-reasoning/orchestration-reasoning-held-out-guide-pattern-fixtures.jsonl",
    {
      "bytes": 1957,
      "sha256": "0f6c11c2ca5c1ae8cddf1409642a6f5b2b8fe367cb669300cb0d4d7c99e72b1e"
    }
  ],
  [
    "training-data/orchestration-reasoning/quarantine/r17/manifest.json",
    {
      "bytes": 5662,
      "sha256": "f6ab0cecce5f0b5ce76609563e81a0ce2d67fcef1a24c99d78829d26bf081ff7"
    }
  ],
  [
    "training-data/orchestration-reasoning/quarantine/r17/source-rows.jsonl",
    {
      "bytes": 4318,
      "sha256": "039c150b087d227878ed816316d8f1558707209052608db9368268459eadede3"
    }
  ],
  [
    "training-data/orchestration-reasoning/source-admission-ledger.json",
    {
      "bytes": 224,
      "sha256": "4b886701e9823c01de4dd79e686b8ffbc3bdc96236b71b6e419179ee54e52dbc"
    }
  ],
  [
    "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/dataset-card.json",
    {
      "bytes": 2232,
      "sha256": "ea938d3fd24d5c4cf4df0373a2bab9d3362e1624435ba2ad9849c9da0198152b"
    }
  ],
  [
    "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/orchestration-reasoning-expanded-dpo.jsonl",
    {
      "bytes": 14035,
      "sha256": "8f368b0c05ae8375b12f5d5e17ef96e0278485fa954097b5e2dab50939d0a1ff"
    }
  ],
  [
    "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/schema.json",
    {
      "bytes": 1594,
      "sha256": "e7b9b99812f67fea09d7e83ec630c22b7e85fdcf9069fced52fb992c72d1d260"
    }
  ],
  [
    "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/split.json",
    {
      "bytes": 1459,
      "sha256": "73eedac8c9f4025201d9e74ac2204b7da9223974daf0d1ad76f23a890ab113ad"
    }
  ]
]);

function sha256(bytes) {
  return crypto.createHash("sha256").update(bytes).digest("hex");
}

function originalBlob(relativePath) {
  return execFileSync(
    "git",
    ["-C", repoRoot, "cat-file", "blob", sourceCommit + ":" + relativePath],
    { encoding: null, windowsHide: true });
}

function originals() {
  const result = new Map();
  for (const relativePath of outputPaths) {
    if (!targetOnly.has(relativePath)) result.set(relativePath, originalBlob(relativePath));
  }
  return result;
}

function targetBodies() {
  const source = originals();
  const result = new Map();
  for (const relativePath of outputPaths) {
    if (zeroPaths.has(relativePath)) result.set(relativePath, Buffer.alloc(0));
    else if (Object.hasOwn(targetText, relativePath)) result.set(relativePath, Buffer.from(targetText[relativePath]));
  }
  result.set(outputPaths[6], source.get(outputPaths[6]));
  result.set(outputPaths[12], Buffer.from(source.get(outputPaths[12]).toString("utf8").split("\n")[1] + "\n"));
  result.set(outputPaths[14], source.get(outputPaths[0]));
  result.set(outputPaths[17], Buffer.from(source.get(outputPaths[17]).toString("utf8").split("\n").slice(3, 12).join("\n") + "\n"));
  result.set(outputPaths[18], source.get(outputPaths[18]));
  assert.equal(result.size, 20);
  return result;
}

function writeFile(root, relativePath, bytes) {
  const destination = path.join(root, ...relativePath.split("/"));
  fs.mkdirSync(path.dirname(destination), { recursive: true });
  fs.writeFileSync(destination, bytes);
}

function writeSnapshot(root, mode = "original") {
  const bodies = mode === "target" ? targetBodies() : originals();
  for (const [relativePath, bytes] of bodies) writeFile(root, relativePath, bytes);
}

function toolArguments(sourceRoot, outputRoot, gitRoot, commit) {
  return [
    tool,
    "--source-root", sourceRoot,
    "--git-object-root", gitRoot,
    "--output-root", outputRoot,
    "--source-commit", commit
  ];
}

function run(sourceRoot, outputRoot, gitRoot = repoRoot, commit = sourceCommit) {
  return spawnSync(
    process.execPath,
    toolArguments(sourceRoot, outputRoot, gitRoot, commit),
    { cwd: repoRoot, encoding: "utf8" });
}

function runWithPreload(sourceRoot, outputRoot, preloadPath) {
  return spawnSync(process.execPath, [
    "--require",
    preloadPath,
    ...toolArguments(sourceRoot, outputRoot, repoRoot, sourceCommit)
  ], { cwd: repoRoot, encoding: "utf8" });
}

function inventory(root) {
  if (!fs.existsSync(root)) return null;
  const rows = [];
  function walk(current, relative) {
    for (const name of fs.readdirSync(current).sort()) {
      const absolute = path.join(current, name);
      const child = relative ? relative + "/" + name : name;
      const stat = fs.lstatSync(absolute);
      if (stat.isSymbolicLink()) rows.push([child, "link", fs.readlinkSync(absolute)]);
      else if (stat.isDirectory()) {
        rows.push([child, "dir"]);
        walk(absolute, child);
      } else rows.push([child, "file", stat.size, sha256(fs.readFileSync(absolute))]);
    }
  }
  walk(root, "");
  return rows;
}

function fixture(t) {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "or-r17-rebuild-"));
  t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  return {
    root,
    source: path.join(root, "source"),
    output: path.join(root, "output"),
    output2: path.join(root, "output-2"),
    outside: path.join(root, "outside")
  };
}

function createLinkOrSkip(t, target, link, posixType) {
  try {
    fs.symlinkSync(target, link, process.platform === "win32" ? "junction" : posixType);
  } catch (error) {
    if (process.platform === "win32" && (error.code === "EPERM" || error.code === "EACCES")) {
      t.skip("Windows link creation privilege is unavailable: " + error.code);
      return false;
    }
    throw error;
  }
  assert.equal(fs.lstatSync(link).isSymbolicLink(), true);
  return true;
}

function assertRejected(result, sourceBefore, sourceRoot, outputBefore, outputRoot) {
  assert.equal(result.status, 1, result.stderr);
  assert.equal(result.stdout, "");
  const failure = JSON.parse(result.stderr.trim());
  assert.equal(failure.error_code, "ORCHESTRATION_REASONING_R17_REBUILD_REJECTED");
  assert.equal(failure.filesystem_writes_started, false);
  assert.deepEqual(inventory(sourceRoot), sourceBefore);
  assert.deepEqual(inventory(outputRoot), outputBefore);
  return failure;
}

function verifyOutput(root) {
  const actualFiles = inventory(root).filter(row => row[1] === "file").map(row => row[0]);
  assert.deepEqual(actualFiles, outputPaths);
  const bodies = targetBodies();
  for (const relativePath of outputPaths) {
    const bytes = fs.readFileSync(path.join(root, ...relativePath.split("/")));
    const specification = expected.get(relativePath);
    assert.equal(bytes.length, specification.bytes, relativePath);
    assert.equal(sha256(bytes), specification.sha256, relativePath);
    assert.deepEqual(bytes, bodies.get(relativePath), relativePath);
  }
}

test("original snapshot produces exact deterministic output and preserves source", t => {
  const f = fixture(t);
  writeSnapshot(f.source);
  const before = inventory(f.source);
  const first = run(f.source, f.output);
  const second = run(f.source, f.output2);
  assert.equal(first.status, 0, first.stderr);
  assert.equal(second.status, 0, second.stderr);
  assert.equal(JSON.parse(first.stdout).snapshot_mode, "ALL_ORIGINAL");
  verifyOutput(f.output);
  verifyOutput(f.output2);
  assert.deepEqual(inventory(f.output), inventory(f.output2));
  assert.deepEqual(inventory(f.source), before);

  const activePaths = outputPaths.filter(relativePath =>
    !relativePath.startsWith("training-data/orchestration-reasoning/quarantine/"));
  const active = activePaths.map(relativePath =>
    fs.readFileSync(path.join(f.output, ...relativePath.split("/")), "utf8")).join("\n").toLowerCase();
  for (const denied of [
    "sonnet",
    "or01u-sonnet-clinerules-001",
    "or01u-sonnet-cursorrules-001",
    "or01u-sonnet-gitattributes-001",
    "or01u-sonnet-gitignore-001",
    "af174a555ca99292c0c87210665536dec6d520f2bd00532397ccf9318383fb05",
    "a106495ccbfc9cbc2651d8070fc0b4c75d2d3b769c97b793d69eb8826299d564",
    "15ee6946aa77f025b11e04ad8f73c801009ebbf56e365a4fe6d2bb43a08d64f5",
    "fe16ff2ddcb4eac73110997e47eb3893b6158ae726b37fb20ba3977303f8ae0c",
    "or-expanded-v1-pair-001",
    "or-expanded-v1-pair-002",
    "or-expanded-v1-pair-003"
  ]) assert.equal(active.includes(denied), false, denied);
  assert.equal(fs.existsSync(path.join(f.output, "training-data/orchestration-reasoning/source-admission-evidence.json")), false);
  const ledger = JSON.parse(fs.readFileSync(path.join(f.output, ...outputPaths[15].split("/"))));
  assert.equal(ledger.status, "REBUILD_PENDING_NO_ADMITTED_ROWS");
  assert.equal(ledger.computed_training_allowed, false);
  assert.deepEqual(ledger.entries, []);
  assert.equal(fs.existsSync(path.join(f.output, "artifacts")), false);
});

test("all-CRLF original snapshot canonicalizes to exact LF output", t => {
  const f = fixture(t);
  writeSnapshot(f.source);
  for (const relativePath of outputPaths.filter(relativePath => !targetOnly.has(relativePath))) {
    const file = path.join(f.source, ...relativePath.split("/"));
    const bytes = fs.readFileSync(file);
    if (bytes.length) fs.writeFileSync(file, Buffer.from(bytes.toString("utf8").replace(/\n/g, "\r\n")));
  }
  const result = run(f.source, f.output);
  assert.equal(result.status, 0, result.stderr);
  verifyOutput(f.output);
});

test("fully target snapshot is idempotent", t => {
  const f = fixture(t);
  writeSnapshot(f.source, "target");
  const before = inventory(f.source);
  const result = run(f.source, f.output);
  assert.equal(result.status, 0, result.stderr);
  assert.equal(JSON.parse(result.stdout).snapshot_mode, "ALL_TARGET");
  verifyOutput(f.output);
  assert.deepEqual(inventory(f.source), before);
});

for (const mutation of [
  ["mixed LF/CRLF", bytes => Buffer.from(bytes.toString("utf8").replace("\n", "\r\n"))],
  ["bare CR", bytes => Buffer.from(bytes.toString("utf8").replace("\n", "\r"))],
  ["UTF-8 BOM", bytes => Buffer.concat([Buffer.from([0xef, 0xbb, 0xbf]), bytes])],
  ["invalid UTF-8", bytes => Buffer.concat([Buffer.from([0xc3, 0x28]), bytes])]
]) {
  test(mutation[0] + " rejects before any write", t => {
    const f = fixture(t);
    writeSnapshot(f.source);
    const changed = path.join(f.source, ...outputPaths[1].split("/"));
    fs.writeFileSync(changed, mutation[1](fs.readFileSync(changed)));
    const sourceBefore = inventory(f.source);
    const outputBefore = inventory(f.output);
    assertRejected(run(f.source, f.output), sourceBefore, f.source, outputBefore, f.output);
  });
}

test("partial target-only snapshot rejects before write", t => {
  const f = fixture(t);
  writeSnapshot(f.source);
  writeFile(f.source, outputPaths[15], targetBodies().get(outputPaths[15]));
  const sourceBefore = inventory(f.source);
  assertRejected(run(f.source, f.output), sourceBefore, f.source, null, f.output);
});

test("original-target mixture and arbitrary drift reject before write", t => {
  const mixed = fixture(t);
  writeSnapshot(mixed.source, "target");
  writeFile(mixed.source, outputPaths[1], originals().get(outputPaths[1]));
  assertRejected(run(mixed.source, mixed.output), inventory(mixed.source), mixed.source, null, mixed.output);

  const drift = fixture(t);
  writeSnapshot(drift.source);
  const changed = path.join(drift.source, ...outputPaths[1].split("/"));
  fs.appendFileSync(changed, Buffer.from(" "));
  assertRejected(run(drift.source, drift.output), inventory(drift.source), drift.source, null, drift.output);
});

test("nonempty output rejects and preserves sentinel", t => {
  const f = fixture(t);
  writeSnapshot(f.source);
  fs.mkdirSync(f.output);
  fs.writeFileSync(path.join(f.output, "sentinel"), "keep");
  const sourceBefore = inventory(f.source);
  const outputBefore = inventory(f.output);
  assertRejected(run(f.source, f.output), sourceBefore, f.source, outputBefore, f.output);
});

test("source and git-object output overlaps reject in both ancestor directions", t => {
  const f = fixture(t);
  writeSnapshot(f.source);
  const insideSource = path.join(f.source, "nested-output");
  const sourceBefore = inventory(f.source);
  const insideSourceFailure =
    assertRejected(run(f.source, insideSource), sourceBefore, f.source, null, insideSource);
  assert.match(insideSourceFailure.error_message, /disjoint/i);

  const sourceAncestorBefore = inventory(f.root);
  const sourceAncestorFailure =
    assertRejected(run(f.source, f.root), sourceBefore, f.source, sourceAncestorBefore, f.root);
  assert.match(sourceAncestorFailure.error_message, /disjoint/i);

  const insideGit = path.join(repoRoot, ".or-r17-overlap-" + process.pid + "-" + Date.now());
  t.after(() => fs.rmSync(insideGit, { recursive: true, force: true }));
  assert.equal(fs.existsSync(insideGit), false);
  const insideGitFailure =
    assertRejected(run(f.source, insideGit), sourceBefore, f.source, null, insideGit);
  assert.match(insideGitFailure.error_message, /disjoint/i);

  const gitAncestor = path.dirname(repoRoot);
  const gitAncestorEntriesBefore = fs.readdirSync(gitAncestor).sort();
  const gitAncestorResult = run(f.source, gitAncestor);
  assert.equal(gitAncestorResult.status, 1, gitAncestorResult.stderr);
  assert.equal(gitAncestorResult.stdout, "");
  const gitAncestorFailure = JSON.parse(gitAncestorResult.stderr.trim());
  assert.equal(gitAncestorFailure.error_code, "ORCHESTRATION_REASONING_R17_REBUILD_REJECTED");
  assert.equal(gitAncestorFailure.filesystem_writes_started, false);
  assert.match(gitAncestorFailure.error_message, /disjoint/i);
  assert.deepEqual(inventory(f.source), sourceBefore);
  assert.deepEqual(fs.readdirSync(gitAncestor).sort(), gitAncestorEntriesBefore);
});

test("intermediate linked directory is rejected without following it", t => {
  const f = fixture(t);
  writeSnapshot(f.source);
  const linked = path.join(f.source, "training-data", "orchestration-reasoning");
  const outside = path.join(f.outside, "orchestration-reasoning");
  fs.mkdirSync(path.dirname(outside), { recursive: true });
  fs.renameSync(linked, outside);
  if (!createLinkOrSkip(t, outside, linked, "dir")) return;
  const sourceBefore = inventory(f.source);
  const outsideBefore = inventory(f.outside);
  assertRejected(run(f.source, f.output), sourceBefore, f.source, null, f.output);
  assert.deepEqual(inventory(f.outside), outsideBefore);
});

test("linked final controlled path is rejected before write", t => {
  const f = fixture(t);
  writeSnapshot(f.source);
  const controlled = path.join(f.source, ...outputPaths[1].split("/"));
  fs.unlinkSync(controlled);
  if (process.platform === "win32") {
    const outsideDirectory = path.join(f.outside, "linked-final-directory");
    fs.mkdirSync(outsideDirectory, { recursive: true });
    if (!createLinkOrSkip(t, outsideDirectory, controlled, "dir")) return;
  } else {
    const outsideFile = path.join(f.outside, "linked-final.json");
    fs.mkdirSync(path.dirname(outsideFile), { recursive: true });
    fs.writeFileSync(outsideFile, originals().get(outputPaths[1]));
    if (!createLinkOrSkip(t, outsideFile, controlled, "file")) return;
  }
  const sourceBefore = inventory(f.source);
  const outsideBefore = inventory(f.outside);
  assertRejected(run(f.source, f.output), sourceBefore, f.source, null, f.output);
  assert.deepEqual(inventory(f.outside), outsideBefore);
});

test("broken final target-only link is rejected before snapshot classification", t => {
  const f = fixture(t);
  writeSnapshot(f.source);
  const controlled = path.join(f.source, ...outputPaths[15].split("/"));
  const missingTarget = path.join(f.outside, "missing-ledger-target");
  fs.mkdirSync(path.dirname(missingTarget), { recursive: true });
  assert.equal(fs.existsSync(missingTarget), false);
  if (!createLinkOrSkip(t, missingTarget, controlled, "file")) return;
  assert.equal(fs.existsSync(controlled), false);
  const sourceBefore = inventory(f.source);
  const outsideBefore = inventory(f.outside);
  const failure = assertRejected(
    run(f.source, f.output),
    sourceBefore,
    f.source,
    null,
    f.output);
  assert.match(failure.error_message, /symbolic-link|junction|reparse/i);
  assert.deepEqual(inventory(f.outside), outsideBefore);
});

test("broken intermediate link is rejected before snapshot classification", t => {
  const f = fixture(t);
  writeSnapshot(f.source);
  const controlled = path.join(f.source, "training-data", "orchestration-reasoning", "quarantine");
  const missingTarget = path.join(f.outside, "missing-quarantine-target");
  fs.mkdirSync(path.dirname(missingTarget), { recursive: true });
  assert.equal(fs.existsSync(missingTarget), false);
  if (!createLinkOrSkip(t, missingTarget, controlled, "dir")) return;
  assert.equal(fs.existsSync(controlled), false);
  const sourceBefore = inventory(f.source);
  const outsideBefore = inventory(f.outside);
  const failure = assertRejected(
    run(f.source, f.output),
    sourceBefore,
    f.source,
    null,
    f.output);
  assert.match(failure.error_message, /symbolic-link|junction|reparse/i);
  assert.deepEqual(inventory(f.outside), outsideBefore);
});

test("fault after first successful output write reports mutation and preserves partial output", t => {
  const f = fixture(t);
  writeSnapshot(f.source);
  const sourceBefore = inventory(f.source);
  const preload = path.join(f.root, "fail-after-first-write.cjs");
  fs.writeFileSync(preload, [
    "\"use strict\";",
    "const fs = require(\"node:fs\");",
    "const originalWriteFileSync = fs.writeFileSync;",
    "let successfulWrites = 0;",
    "fs.writeFileSync = function (...args) {",
    "  if (successfulWrites >= 1) {",
    "    const error = new Error(\"injected failure after first successful output write\");",
    "    error.code = \"EIO\";",
    "    throw error;",
    "  }",
    "  const result = originalWriteFileSync.apply(this, args);",
    "  successfulWrites += 1;",
    "  return result;",
    "};",
    ""
  ].join("\n"));

  const result = runWithPreload(f.source, f.output, preload);
  assert.equal(result.status, 1, result.stderr);
  assert.equal(result.stdout, "");
  const failure = JSON.parse(result.stderr.trim());
  assert.equal(failure.error_code, "ORCHESTRATION_REASONING_R17_REBUILD_REJECTED");
  assert.equal(failure.filesystem_writes_started, true);
  assert.match(failure.error_message, /injected failure after first successful output write/i);
  assert.deepEqual(inventory(f.source), sourceBefore);
  const partialFiles = inventory(f.output)
    .filter(row => row[1] === "file")
    .map(row => row[0]);
  assert.deepEqual(partialFiles, [outputPaths[0]]);
  assert.equal(fs.readFileSync(path.join(f.output, ...outputPaths[0].split("/"))).length, 0);
  assert.equal(fs.existsSync(path.join(f.output, ...outputPaths[1].split("/"))), false);
});

test("wrong source commit and malformed CLI reject without output", t => {
  const f = fixture(t);
  writeSnapshot(f.source);
  const wrong = run(f.source, f.output, repoRoot, "0000000000000000000000000000000000000000");
  assert.equal(wrong.status, 1);
  assert.equal(inventory(f.output), null);
  const malformed = spawnSync(process.execPath, [tool, "--source-root", f.source], {
    cwd: repoRoot,
    encoding: "utf8"
  });
  assert.equal(malformed.status, 1);
  assert.equal(inventory(f.output), null);
});
