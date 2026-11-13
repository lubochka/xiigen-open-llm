# Frozen reference corpora

This directory is reserved for frozen reference corpora from a paid closed
model. They are evaluation instruments: we compare a student model against
these references to measure behavior and calibrate matching rules.

Files under this root are repository-owned and Git-visible. If a corpus changes,
write a new version directory instead of overwriting the old one:

```text
training-data/reference-corpora/<domainSlug>/<subjectSlug>/<corpusVersion>/
```

Each sealed corpus version is stored in that exact directory as:

```text
corpus.json
manifest.json
```

`corpus.json` contains the immutable frozen reference payload. `manifest.json`
is written last and binds the corpus address, payload byte length, payload
SHA-256, corpus SHA-256 and manifest self-hash. A restart must reopen the same
bytes through the strict opener; missing files, tampered bytes, mismatched
identity/version, non-canonical repository-relative paths, reparse/symlink-like
path segments, directory/file collisions or payload path escape attempts fail
closed instead of regenerating or overwriting the corpus.

Reference-matching calibration numbers live under the sealed corpus they measure:

```text
training-data/reference-corpora/<domainSlug>/<subjectSlug>/<corpusVersion>/calibrations/<ruleId>/<ruleIdentitySha256Hex>/<calibrationSetSha256Hex>/manifest.json
```

That calibration manifest is written only after the linked corpus reopens through
the strict corpus opener. It stores schema/version, canonical repository-relative
address, linked corpus manifest path/hash, corpus SHA/count, exact rule
domain/id/version, human-readable identity description, identity SHA,
calibration-set SHA, known positive/negative
pair IDs, separate false-miss and false-hit IDs/counts/rates, evaluator result
hash and manifest self-hash. It must not contain raw paid-reference answers, raw
candidate answers, DPO chosen/rejected sides, weights or model output. Repeating
the same manifest is idempotent; a different manifest at the same address is a
conflict.

The evaluator result hash is the membership-bound v2 canonical form: ordered
known-positive and known-negative PairId arrays are hashed alongside the
denominators, false-miss/false-hit IDs, corpus SHA/count, rule identity and
calibration-set SHA. The stored rule description must already be canonical;
reopen reconstructs the rule identity and exact-compares the normalized
description, domain, rule id, version and identity SHA.

Domain code should prefer
`RepositoryOwnedReferenceMatchingCalibrationManifestSealer.EvaluatePersistAndSeal`
when it has a `ReferenceMatchingCalibrationInput` and a matching predicate. That
one call builds the canonical address, reopens the linked sealed corpus before
running the predicate, evaluates the rule and writes the sealed manifest, so the
reusable calibration cannot stay only in memory. Wrong repository roots,
subjects or missing sealed corpora fail before predicate execution or tree
creation.

Paid-reference text from this root must never become `chosen` or `rejected` DPO
content, and it must never be used as training input. The reference bytes may be
used only to evaluate or calibrate later code paths that explicitly open a
sealed corpus.

The repository-owned sealer/opener is implemented for deterministic corpus bytes
and calibration manifests. This storage slice does not create real paid-model
answers, call a teacher/model, generate DPO rows, run training or claim model
quality; it only stores and strictly reopens caller-supplied
`FrozenReferenceCorpus` snapshots and already-computed calibration numbers.

Do not place real paid-model answers, private prompts, credentials, API keys or
other secrets in this README.
