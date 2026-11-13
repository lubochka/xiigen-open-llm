# Universal Rule Redrive Map

Run: `quality-controls-derived-permissive-source-review-20260618-084753`

Coverage: **12/12 scenarios** and **54/54 pair rows**.

All current rows have common route blocked because no accepted local source/license packet is bound to the row. For common use, each row must be re-derived from an independent MIT/Apache-2.0/open-permissive source with local license text/ref/hash, provenance, redaction, and reviewer acceptance.

| Type | Count | Common allowed | Route now |
|---|---:|---:|---|
| Scenario | 12 | 0 | 7 user-adaptive-only, 5 evidence-only |
| Pair row | 54 | 0 | 21 user-adaptive-only, 33 evidence-only |

Required evidence fields for any later redrive packet: `source_ref`, `doc_id`, `source_sha256`, `source_file_or_upstream_ref`, `source_license_ref`, `license_text_ref`, `license_hash`, `license_class`, `spdx_id_or_reviewed_license_name`, `provenance_ref`, `redaction_ref`, `redaction_status`, `privacy_class`, `universal_candidate_text`, `stack_specific_text_or_quarantine_route`, `common_training_allowed`, `reviewer_id`, `created_utc`, `accepted_packet_ref`, `source_disjoint_group_id`, `train_or_heldout_after_eligibility`.

The complete per-row map is in `universal-rule-redrive-map.json` and `universal-rule-redrive-map.jsonl`.
