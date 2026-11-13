# Q4 Pre-Commit Review/Fix Cycle 2 of 4

Status: PASS_WITH_REPAIR_ACCEPTED

Reviewed scope: Q4 OrchestrationReasoning changed-file set only: Q4 runner and reports, OrchestrationReasoning checkpoint/manifest/model index, orchestration-reasoning-expanded-v1 training data, material-quality report, cycle-1 review, and state. No staging, commit, push, broad tests, dashboard/API, host/plugin/runtime, Q5, or Q8 work was performed.

## Finding

P1 - Runner replaced the repaired pair-id shortcut with expected-label leakage.

`artifacts/smart-skills-quality-uplift/q4/run-orchestration-reasoning-q4-01.ps1` no longer used a `TrainingPairId` switch, but `New-HeldoutSignals` still placed `$row.ChosenScenarioRef` and `$row.RejectedScenarioRef` into the held-out feature text. Those fields are labels, not inference inputs. Using them to derive signals let held-out prediction see the expected scenario family directly, which violated the cycle-2 requirement to catch replacement deterministic shortcuts after pair-id repair.

## Fix

Removed `ChosenScenarioRef` and `RejectedScenarioRef` from `New-HeldoutSignals`. The runner now derives held-out signals from boundary/provenance text and the near-miss strategy only; `ChosenScenarioRef` remains used only as the expected label for scoring and control-report rows.

Focused Q4 runner rerun was justified because runner logic changed and checkpoint, manifest, model index, fresh-load, continue, held-out, ablation, and training reports depend on the runner output/hash.

## Evidence

- Branch: `codex/chat-gpt-5.5`.
- Focused runner rerun: PASS.
- Held-out result after repair: 4/4, accuracy 1.0.
- Source-disjoint check: 8 train rows, 4 held-out rows, SourceManifestRef overlap 0.
- Current runner SHA-256: `ec80bc32d2d5b26e94d628e1dd2544fbfbad469b4631fb2a6e6c325f24aa9eac`.
- Current checkpoint SHA-256: `49c0c789ca4668e9e1a82c3b06469266fec7ade4928c8ccc94efce9fb081643b`.
- Current manifest SHA-256: `7ac5d43ccd09c45af1ff808a88c18284e66c4654bf2e78122674b1f856369bda`.
- Current model index SHA-256: `9560ddd32c432b1491abca6706f4662d2d559b8940fd2ff4785a8d0f51fb8e80`.
- Current training report SHA-256: `e3687b2fcaa1f824a73ca51740a9bc360b466ba71e4a171c766804ca3d9e07eb`.
- Current held-out report SHA-256: `43e9520fca25278447428bfe02456ccfde48430671dc7b38031ad3512d69aa10`.
- Current ablation report SHA-256: `32c96403c31148c61d9f8e2ae0c5bc958a31afa932a9ff5c8b6e133ac8e72a29`.
- Current fresh-load report SHA-256: `9bc7f565478e75dab2f65bcd50800d2c68b495c89ea5c461310678eede8e55e6`.
- Current continue report SHA-256: `2727024904b20ceaebd6c23f4e967439305e1a4e6f55455002910fce92e4a34b`.
- Training data SHA-256: `7cc7c94d3d1d160807eebc404a132342479eb74b50300de638c8c80ac24a95f7`.
- Split/schema/dataset-card SHA-256: `e9ae5934517b5f0eabf325526e3a2d1b364726ec8ed4598e394e11998aae99ea`, `e7b9b99812f67fea09d7e83ec630c22b7e85fdcf9069fced52fb992c72d1d260`, `126e47576bcaa9c716563ff3c92b639ddefb8789b9d950dd6033aba304cbefe7`.
- Manifest checkpoint hash matches actual checkpoint: PASS.
- Model index checkpoint/manifest hashes match actual files: PASS.
- Training, fresh-load, and continue reports point to the current checkpoint: PASS.
- Deterministic controls: no-trained 0.0, exact guide-title/path/filename 0.0, BM25/title surrogate 0.25, static route table 0.25; all below trained 1.0.
- `New-HeldoutSignals` no longer contains `ChosenScenarioRef` or `RejectedScenarioRef`.
- Static taxonomy/error-code use remains only in explicit control/report generation, not trained held-out feature extraction.
- `semantic_quality_claimed=false` in checkpoint, manifest, model index, training report, held-out report, ablation report, fresh-load report, and continue report.

## Scope/Honesty Notes

This PASS is commit-readiness cycle 2 only. It proves bounded Q4 seed asset/report consistency after removing expected-label leakage from held-out feature extraction. It does not claim full semantic quality, host/plugin/runtime readiness, dashboard/API evidence, Q5, Q8, or final plan completion.

Historical Q4-02/Q4-03 review JSON snapshots may still contain original old hash fields as historical evidence. Commit readiness for current files is superseded by this cycle-2 report and the updated state entry.

## Verification Run

- PowerShell JSON/JSONL/hash checks over Q4 runner outputs, manifest, model index, checkpoint, training data, split, schema, and dataset card: PASS.
- Targeted runner text check: no `switch ($row.TrainingPairId)` held-out prediction authority remains.
- Targeted runner text check: `New-HeldoutSignals` excludes `ChosenScenarioRef` and `RejectedScenarioRef`.
- Targeted runner text check: expected scoring still uses `$row.ChosenScenarioRef` only as the expected label.
- Targeted `rg` over runner/checkpoint/reports/training data found pair/source/path/title/static/overclaim terms only in dataset fields, explicit scoring labels, explicit controls, reports, or no-overclaim evidence; no new final-authority shortcut was accepted.
- `git diff --check`: PASS.

## Verdict

PASS_WITH_REPAIR_ACCEPTED. Cycle 2 found and fixed a real Q4 defect: expected-label leakage replaced the pair-id shortcut. Current Q4 commit candidate remains bounded seed asset/report readiness only with no semantic-quality overclaim.

Exact next action: `Q4_PRE_COMMIT_REVIEW_FIX_CYCLE_3_OF_4`.
