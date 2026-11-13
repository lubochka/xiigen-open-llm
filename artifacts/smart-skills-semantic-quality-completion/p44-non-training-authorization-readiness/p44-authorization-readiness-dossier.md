# P44 Non-Training Authorization / Readiness Dossier

Verdict: PASS_PENDING_PARENT_REVIEW.

Этот dossier не запускает training, eval, checkpoint mutation или host runtime. Его задача проще и строже: собрать точную карту того, что уже есть, чего ещё не хватает, и какие условия должны быть выполнены до любого будущего training/eval work order.

## 1. Что P44 проверяет

P43 оставил два честных хвоста:

- `P37-GAP-05` частично закрыт P10/P11/P12, но common/final checkpoint ещё не переписан новым обучением.
- `P37-GAP-07` всё ещё открыт, потому что training/eval не были авторизованы.

P44 отвечает на вопрос: “можно ли уже запускать training/eval?” Ответ: нет. Но теперь это не абстрактное “нет”, а список конкретных условий, которые должны появиться в следующей фазе.

## 2. Candidate data сейчас

| Source | What it contains | Current role | Count | Can train now? | Why not |
|---|---|---|---:|---|---|
| `p27-skillselection-repair-dataset-candidate` | Checked SkillSelection repair candidates from P24/P25/P26 failures. | Candidate dataset only. | 8 rows | No | P27 ledger says trainingAllowed=false, candidateTrainingAllowed=false, commonTrainingAllowed=false. |
| `p31-materialized-dpo-candidates.jsonl` | Materialized candidate DPO-style rows for SkillSelection repair. | Candidate material, not accepted training set. | 12 rows | No | Needs explicit authorization, source/license gate, source-disjoint split, leakage checks and eval plan. |
| `p31-dashboard-qa-scenarios.jsonl` | Dashboard QA scenarios to display/check candidate behavior. | Dashboard/QA evidence only. | 6 rows | No | QA scenarios are not training rows. |
| `p36-eval-fixtures.jsonl` | Source-disjoint eval fixtures across PromptToTask, OrchestrationReasoning and SkillSelection. | Eval fixture material only. | 36 rows | No | P41 keeps `can_proceed_to_eval_runner=false`; eval runner still needs authorization. |

## 3. License/source gate before training

Before training/eval is allowed, the next phase must create a source/license ledger with:

- exact dataset artifact paths;
- row counts;
- source family for each row;
- license or approval source for each family;
- whether the row is common-training eligible, user-local only, or quarantined;
- checksum for the candidate dataset and held-out fixture set;
- explicit note that private/local user material cannot become common training unless Luba approves that reuse.

Current verdict: missing for training. Existing P27/P31/P36 artifacts are candidate/evidence material, not authorization.

## 4. Source-disjoint readiness

Current evidence:

- P36 has 36 eval fixtures.
- P41 source-disjoint guard reports no duplicate fixture ids, no missing manifest families, no unexpected families, no train/eval family overlap inside the P36 artifact set.
- P41 still keeps `can_proceed_to_eval_runner=false`.

Meaning:

- The guard structure exists.
- Eval is still not authorized.
- A future eval work order must first attach an authorization manifest and prove train/eval split across the exact candidate training set, not only the P36 fixture-only artifact set.

## 5. Target checkpoint readiness

Current evidence:

- P10 created a candidate checkpoint rewrite package/manifest/index/locator.
- P11 checked component fresh-load against candidate checkpoint artifacts.
- P12 checked no-op continue-training lifecycle boundaries.

Current missing condition:

- no new trained checkpoint from the new rows;
- no checkpoint mutation;
- no promotion to common/final model;
- no positive fresh-load result for a newly trained checkpoint;
- no positive continue-training result after a newly trained checkpoint.

Future training/eval cannot claim readiness until the target checkpoint, lineage, dataset checksum, held-out checksum, fresh-load probe and continue-training probe all exist in the same evidence chain.

## 6. Metrics required before any semantic-quality claim

Future eval must report at minimum:

- per-machine metrics for PromptToTask, OrchestrationReasoning and SkillSelection when the phase touches them;
- held-out accuracy or task-specific pass rate by source family;
- ablation deltas against exact prompt lookup, source id/path/filename lookup, title/static route, nearest-only baseline and no-trained-state;
- candidate-order permutation check for SkillSelection;
- false-positive claim scan showing dashboard/API evidence is not treated as model readiness;
- regression check against previous checkpoint behavior;
- explicit failure examples, not only aggregate numbers.

Current verdict: missing. P44 does not invent metrics and does not run eval.

## 7. Rollback plan required before mutation

Before any checkpoint mutation:

1. Record current checkpoint locator, manifest hash and model asset hash.
2. Write new candidate checkpoint under a new lineage id, never overwrite the previous common checkpoint.
3. Keep rollback pointer to previous checkpoint.
4. Promote only after source-disjoint eval, ablations, fresh-load, continue-training and parent review pass.
5. If any gate fails, leave candidate checkpoint quarantined and keep `SemanticQualityClaimed=false`.

Current verdict: rollback design required; mutation not allowed in P44.

## 8. Explicit approval field

Training/eval starts only when a future work order records:

```text
training_authorized_now = true
eval_authorized_now = true
exact_luba_approval_quote = <fresh current quote>
candidate_dataset = <exact artifact path + checksum>
target_checkpoint = <exact checkpoint/manifest/locator>
rollback_checkpoint = <exact previous checkpoint/manifest/locator>
```

Current P44 value:

```text
training_authorized_now = false
eval_authorized_now = false
exact_luba_approval_quote = NONE
```

## 9. Final P44 verdict

P44 closes the readiness-accounting gap, not the model-quality gap.

Next safe action: create the next bounded work order for a training/eval authorization manifest and dry-run validation. That future work order still must not run training/eval until the manifest has explicit approval and all preflight gates are green.
