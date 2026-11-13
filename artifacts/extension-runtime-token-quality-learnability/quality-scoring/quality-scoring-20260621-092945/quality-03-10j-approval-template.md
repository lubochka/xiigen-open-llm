# QUALITY-03 10J approval template

This is a template artifact only. It is not current approval and it is not a prompt to Luba.

A future approval packet would need to bind each exact source_ref/doc_id/source_hash to:

- source_license_ref
- license_hash
- license_class
- consent_or_provenance
- privacy_class
- route: common_training_allowed=true with reason, or explicit non-common route
- training_use_approval_ref, or explicit statement that training is forbidden

For raw-private replay, a future packet would also need:

- approved_raw_private_corpus_ref
- file path/hash ledger
- row-level replay-only approval
- excluded_from_training_targets=true
- exact replay/no-leakage/no-memorization scope
