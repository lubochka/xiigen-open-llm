#!/usr/bin/env node
"use strict";

const fs = require("node:fs");
const path = require("node:path");
const crypto = require("node:crypto");
const { execFileSync } = require("node:child_process");
const { TextDecoder } = require("node:util");

const SOURCE_COMMIT = "3a2dc5cb33f43c0b5c80a1834d3a1a88e73b2482";
const OUTPUT_PATHS = Object.freeze([
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
const TARGET_ONLY_PATHS = new Set([
  "training-data/orchestration-reasoning/source-admission-ledger.json",
  "training-data/orchestration-reasoning/quarantine/r17/manifest.json",
  "training-data/orchestration-reasoning/quarantine/r17/source-rows.jsonl"
]);
const ZERO_PATHS = new Set([
  "training-data/orchestration-reasoning/or-01u-universalization-candidates.jsonl",
  "training-data/orchestration-reasoning/orchestration-reasoning-common-corpus.jsonl",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-bindings.jsonl",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-pairs.jsonl",
  "training-data/orchestration-reasoning/orchestration-reasoning-dpo-trainer-payloads.jsonl"
]);
const TARGET_TEXT = Object.freeze({
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
const EXPECTED = Object.freeze(new Map([
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
]));
let filesystemWritesStarted = false;

function fail(message) {
  throw new Error(message);
}

function lstatOrNull(lexicalPath) {
  try {
    return fs.lstatSync(lexicalPath);
  } catch (error) {
    if (error && error.code === "ENOENT") return null;
    throw error;
  }
}

function beginFilesystemMutation() {
  filesystemWritesStarted = true;
}

function sha256(bytes) {
  return crypto.createHash("sha256").update(bytes).digest("hex");
}

function normalizeCase(value) {
  const normalized = path.normalize(value);
  return process.platform === "win32" ? normalized.toLowerCase() : normalized;
}

function samePath(left, right) {
  return normalizeCase(left) === normalizeCase(right);
}

function isContained(root, candidate) {
  const relative = path.relative(root, candidate);
  return relative === "" || (!relative.startsWith(".." + path.sep) && relative !== ".." && !path.isAbsolute(relative));
}

function validateRelative(relativePath) {
  if (typeof relativePath !== "string" || relativePath.length === 0 || path.isAbsolute(relativePath) || relativePath.includes("\\")) {
    fail("Controlled path must be a nonempty portable relative path: " + relativePath);
  }
  const parts = relativePath.split("/");
  if (parts.some(part => part === "" || part === "." || part === "..")) {
    fail("Controlled path contains an escaping or empty component: " + relativePath);
  }
  return parts;
}

function inspectExisting(lexicalPath, expectedRoot, knownStat = null) {
  const stat = knownStat ?? fs.lstatSync(lexicalPath);
  if (stat.isSymbolicLink()) {
    fail("Symbolic-link/junction traversal is forbidden: " + lexicalPath);
  }
  const real = fs.realpathSync.native(lexicalPath);
  if (!samePath(real, lexicalPath)) {
    fail("Reparse, junction, mount, or redirected traversal is forbidden: " + lexicalPath);
  }
  if (expectedRoot && !isContained(expectedRoot, real)) {
    fail("Resolved path escapes controlled root: " + lexicalPath);
  }
  return { stat, real };
}

function resolveExistingRoot(rootPath, label) {
  const lexical = path.resolve(rootPath);
  const stat = lstatOrNull(lexical);
  if (stat === null) {
    fail(label + " does not exist: " + lexical);
  }
  const info = inspectExisting(lexical, null, stat);
  if (!info.stat.isDirectory()) {
    fail(label + " is not a directory: " + lexical);
  }
  return { lexical, real: info.real };
}

function resolveCandidateRoot(rootPath, label) {
  const lexical = path.resolve(rootPath);
  const lexicalStat = lstatOrNull(lexical);
  if (lexicalStat !== null) {
    const info = inspectExisting(lexical, null, lexicalStat);
    if (!info.stat.isDirectory()) {
      fail(label + " is not a directory: " + lexical);
    }
    return { lexical, real: info.real, exists: true };
  }
  const missing = [];
  let cursor = lexical;
  let cursorStat = lexicalStat;
  while (cursorStat === null) {
    const parent = path.dirname(cursor);
    if (parent === cursor) fail("No existing ancestor for " + label);
    missing.unshift(path.basename(cursor));
    cursor = parent;
    cursorStat = lstatOrNull(cursor);
  }
  const parentInfo = inspectExisting(cursor, null, cursorStat);
  if (!parentInfo.stat.isDirectory()) fail("Nearest existing ancestor is not a directory: " + cursor);
  let candidate = parentInfo.real;
  for (const part of missing) {
    if (!part || part === "." || part === "..") fail("Unsafe missing root component");
    candidate = path.join(candidate, part);
  }
  return { lexical: candidate, real: candidate, exists: false };
}

function resolveControlled(rootInfo, relativePath, allowMissing) {
  const parts = validateRelative(relativePath);
  let cursor = rootInfo.real;
  let missing = false;
  for (const part of parts) {
    const next = path.join(cursor, part);
    if (!missing) {
      const stat = lstatOrNull(next);
      if (stat !== null) {
        const info = inspectExisting(next, rootInfo.real, stat);
        cursor = info.real;
        continue;
      }
      missing = true;
      cursor = next;
      if (!isContained(rootInfo.real, cursor)) fail("Lexical path escapes root: " + relativePath);
    } else {
      cursor = next;
      if (!isContained(rootInfo.real, cursor)) fail("Lexical path escapes root: " + relativePath);
    }
  }
  if (missing && !allowMissing) fail("Controlled source path is missing: " + relativePath);
  return { absolute: cursor, exists: !missing };
}

function canonicalize(bytes, relativePath) {
  if (bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf) {
    fail("UTF-8 BOM is forbidden: " + relativePath);
  }
  let text;
  try {
    text = new TextDecoder("utf-8", { fatal: true }).decode(bytes);
  } catch {
    fail("Invalid UTF-8: " + relativePath);
  }
  if (!text.includes("\r")) return Buffer.from(text, "utf8");
  if (/(^|[^\r])\n/.test(text) || /\r(?!\n)/.test(text)) {
    fail("Mixed or bare-CR line endings are forbidden: " + relativePath);
  }
  return Buffer.from(text.replace(/\r\n/g, "\n"), "utf8");
}

function readOriginal(gitRoot, relativePath) {
  return execFileSync(
    "git",
    ["-C", gitRoot.real, "cat-file", "blob", SOURCE_COMMIT + ":" + relativePath],
    { encoding: null, windowsHide: true, stdio: ["ignore", "pipe", "pipe"] });
}

function splitLfLines(bytes, relativePath) {
  const canonical = canonicalize(bytes, relativePath).toString("utf8");
  if (!canonical.endsWith("\n")) fail("Expected terminal LF in source blob: " + relativePath);
  return canonical.slice(0, -1).split("\n");
}

function buildTargets(originals) {
  const target = new Map();
  for (const relativePath of OUTPUT_PATHS) {
    if (ZERO_PATHS.has(relativePath)) target.set(relativePath, Buffer.alloc(0));
    else if (Object.hasOwn(TARGET_TEXT, relativePath)) target.set(relativePath, Buffer.from(TARGET_TEXT[relativePath], "utf8"));
  }
  const candidates = originals.get(OUTPUT_PATHS[0]);
  const continuePayload = originals.get(OUTPUT_PATHS[6]);
  const heldoutLines = splitLfLines(originals.get(OUTPUT_PATHS[12]), OUTPUT_PATHS[12]);
  const expandedLines = splitLfLines(originals.get(OUTPUT_PATHS[17]), OUTPUT_PATHS[17]);
  target.set(OUTPUT_PATHS[6], continuePayload);
  target.set(OUTPUT_PATHS[12], Buffer.from(heldoutLines[1] + "\n", "utf8"));
  target.set(OUTPUT_PATHS[14], candidates);
  target.set(OUTPUT_PATHS[17], Buffer.from(expandedLines.slice(3, 12).join("\n") + "\n", "utf8"));
  target.set(OUTPUT_PATHS[18], originals.get(OUTPUT_PATHS[18]));
  if (target.size !== OUTPUT_PATHS.length || OUTPUT_PATHS.some(relativePath => !target.has(relativePath))) {
    fail("Internal target map is incomplete");
  }
  for (const relativePath of OUTPUT_PATHS) {
    const bytes = target.get(relativePath);
    const expected = EXPECTED.get(relativePath);
    if (bytes.length !== expected.bytes || sha256(bytes) !== expected.sha256) {
      fail("Internal target body mismatch: " + relativePath);
    }
  }
  return target;
}

function classifySnapshot(sourceRoot, originals, targets) {
  const presence = [...TARGET_ONLY_PATHS].map(relativePath =>
    resolveControlled(sourceRoot, relativePath, true).exists);
  if (presence.some(Boolean) && !presence.every(Boolean)) {
    fail("Partial target-only snapshot is forbidden");
  }
  const mode = presence.every(Boolean) ? "ALL_TARGET" : "ALL_ORIGINAL";
  for (const relativePath of OUTPUT_PATHS) {
    const resolved = resolveControlled(sourceRoot, relativePath, TARGET_ONLY_PATHS.has(relativePath));
    if (!resolved.exists) {
      if (mode === "ALL_TARGET") fail("Target snapshot path missing: " + relativePath);
      continue;
    }
    const stat = fs.lstatSync(resolved.absolute);
    if (!stat.isFile()) fail("Controlled path is not a file: " + relativePath);
    const actual = canonicalize(fs.readFileSync(resolved.absolute), relativePath);
    const expected = mode === "ALL_TARGET" ? targets.get(relativePath) : originals.get(relativePath);
    if (!expected || !actual.equals(expected)) {
      fail("Snapshot is mixed or drifted at: " + relativePath);
    }
  }
  return mode;
}

function parseArgs(argv) {
  const values = new Map();
  const allowed = new Set(["--source-root", "--git-object-root", "--output-root", "--source-commit"]);
  for (let index = 0; index < argv.length; index += 2) {
    const name = argv[index];
    const value = argv[index + 1];
    if (!allowed.has(name) || value === undefined || values.has(name)) fail("Invalid command arguments");
    values.set(name, value);
  }
  for (const required of ["--source-root", "--output-root", "--source-commit"]) {
    if (!values.has(required)) fail("Missing required argument: " + required);
  }
  if (values.get("--source-commit") !== SOURCE_COMMIT) fail("Only the exact source commit is accepted");
  if (!values.has("--git-object-root")) values.set("--git-object-root", values.get("--source-root"));
  return values;
}

function rootsOverlap(left, right) {
  return isContained(left, right) || isContained(right, left);
}

function ensureOutputDirectory(outputRoot) {
  if (!outputRoot.exists) {
    beginFilesystemMutation();
    fs.mkdirSync(outputRoot.real);
  }
  const verified = resolveExistingRoot(outputRoot.real, "output root");
  if (fs.readdirSync(verified.real).length !== 0) fail("Output root must be empty");
  return verified;
}

function createSafeDirectory(rootInfo, relativeDirectory) {
  let cursor = rootInfo.real;
  for (const part of validateRelative(relativeDirectory)) {
    const next = path.join(cursor, part);
    let stat = lstatOrNull(next);
    if (stat === null) {
      beginFilesystemMutation();
      fs.mkdirSync(next);
      stat = fs.lstatSync(next);
    }
    const info = inspectExisting(next, rootInfo.real, stat);
    if (!info.stat.isDirectory()) fail("Output component is not a directory: " + next);
    cursor = info.real;
  }
}

function writeOutputs(outputRoot, targets) {
  for (const relativePath of OUTPUT_PATHS) {
    const directory = path.posix.dirname(relativePath);
    if (directory !== ".") createSafeDirectory(outputRoot, directory);
    const destination = resolveControlled(outputRoot, relativePath, true);
    if (destination.exists) fail("Output target unexpectedly exists: " + relativePath);
    beginFilesystemMutation();
    fs.writeFileSync(destination.absolute, targets.get(relativePath), { flag: "wx" });
  }
}

function main() {
  const args = parseArgs(process.argv.slice(2));
  const sourceRoot = resolveExistingRoot(args.get("--source-root"), "source root");
  const gitRoot = resolveExistingRoot(args.get("--git-object-root"), "git-object root");
  const outputCandidate = resolveCandidateRoot(args.get("--output-root"), "output root");
  if (rootsOverlap(sourceRoot.real, outputCandidate.real) || rootsOverlap(gitRoot.real, outputCandidate.real)) {
    fail("Output root must be disjoint from source and git-object roots");
  }
  if (outputCandidate.exists && fs.readdirSync(outputCandidate.real).length !== 0) {
    fail("Output root must be empty");
  }

  const originals = new Map();
  for (const relativePath of OUTPUT_PATHS) {
    if (!TARGET_ONLY_PATHS.has(relativePath)) originals.set(relativePath, readOriginal(gitRoot, relativePath));
  }
  const targets = buildTargets(originals);
  const snapshotMode = classifySnapshot(sourceRoot, originals, targets);
  const outputRoot = ensureOutputDirectory(outputCandidate);
  writeOutputs(outputRoot, targets);
  process.stdout.write(JSON.stringify({
    source_commit: SOURCE_COMMIT,
    snapshot_mode: snapshotMode,
    output_count: OUTPUT_PATHS.length,
    output_paths: OUTPUT_PATHS
  }) + "\n");
}

try {
  main();
} catch (error) {
  process.stderr.write(JSON.stringify({
    passed: false,
    error_code: "ORCHESTRATION_REASONING_R17_REBUILD_REJECTED",
    error_message: error instanceof Error ? error.message : String(error),
    filesystem_writes_started: filesystemWritesStarted
  }) + "\n");
  process.exit(1);
}
