# S17 human handoff and no-overclaim record

**Prepared 2026-10-13. Scope:** one new source-bound plan packet under `replanning`; not a code merge, model run, learned Student state, hardware measurement, independent Astra review, or authorized runtime release.

## Present observations versus hypotheses
- ACTUAL SOURCE OBSERVATION on K `3c7f5f8c`: `KnnLogicalUnit.ExecuteAsync` uses `hits[0]` for predicted output after Search(k); therefore multiple hits in evidence are not, by themselves, a multi-neighbor result. RP02 fixes general multi-hit behavior; S17 must not duplicate that owner.
- ACTUAL SOURCE OBSERVATION: D-04 `PromptDomainClassifier` task labels are not subject-law version labels; `TrainingVectorBuilder` 60-position vector is different from KNN's local BPE/predicate representation.
- **DESIGN HYPOTHESIS:** a versioned domain-law compatibility guard and state migration can reduce stale memory errors, improve accuracy under changed laws and maintain old-law retention. No empirical results currently prove it.
- USER-REQUIRED ROLES: CODEX `gpt-6-astra/ultra` selects real teaching case **and separately** judges the exact DPO pair; approved external open Chinese model alone authors both sides; Student uses existing trainer; independent post-update judge separately examines competence.
- GOVERNANCE: existing native/security/release evidence remains mandatory *when applicable*; no new coverage or Windows-first gate for this learning experiment.

## What is next
Launch `S17.00B` old/new source-family exam, then `S17.02A` minimal current kNN eligibility-consumer seam if explicit implementation authorization exists; run `S17.05A` C0 vs parent before any conditional DPO. Assign domain taxonomy drift corrections to RP02; Unit/Flow to RP03; Orchestrator/Graph to RP04; trainer to RP01; full state/journal to RP05; evaluation runner to RP09; product host to RP06. An absent port becomes owned `INTEGRATION_OPEN`, not another scope expansion.

## Truth table for a final report
`PACK_PUBLISHED` requires verified remote SHA and all 14 task files; `SOURCE_TRACED` requires actual primary code span; `GUARD_USED` requires real ordinary call; `STUDENT_UPDATED` requires actual weights/journal; `MEASURED_IMPROVE` requires frozen independent quality/time/PC old/new comparison; `COLD_ADOPTED` needs real consumer read; `INDEPENDENT_ASTRA_PASS` needs a distinct actual model invocation and exact-SHA receipt. Do not conflate them.

**Design review issue to investigate rather than assert:** RP02 may concurrently change the exact multi-neighbor consumer. S17 must negotiate the smallest version-eligibility port or work with a frozen eligible-pool adapter, never write over RP02 files based on a stale snapshot.
