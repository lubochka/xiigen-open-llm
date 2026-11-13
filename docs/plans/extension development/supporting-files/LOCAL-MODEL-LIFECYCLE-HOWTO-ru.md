# Local model lifecycle HOWTO

Этот документ объясняет two-leg lifecycle: common repo assets and user-local adaptive assets. Это docs-only guidance. Сейчас trainable DPO rows 0/9 and actual lifecycle rows 0/9; no training, continue-training, checkpoint promotion, locator update or target mutation is claimed here.

## Всего / Сделано / Сейчас / Осталось

| Поле | Статус |
|---|---|
| Всего | Для 9 host/method rows нужны accepted DPO candidates and lifecycle evidence: backup/export/import/share/restore/load-without-retrain/continue-training/fresh-load/held-out/ablation/rollback. |
| Сделано | Docs now explain the common leg, user-local leg and required lifecycle checklist. |
| Сейчас | Full UI/chat readiness не закрыта. Trainable DPO rows 0/9. Actual lifecycle rows 0/9. Token pairs 0/9, quality rows 0/9, final reviews 0/10. Claim locks false. |
| Осталось | Future bounded work orders must create approved concrete refs, not schema-only rows, and must keep docs/status evidence separate from training or lifecycle proof. |

## Common vs user-local leg

| Leg | What belongs there | What it proves only after evidence |
|---|---|---|
| Common repo assets | Universal, source/license/privacy-approved material, manifests, import/export path, public machine boundary, repo-owned checkpoint refs when approved. | Common loadability only after manifest, checksum, fresh-load and controls. |
| User-local adaptive assets | User-approved private/local corrections, adaptive root, rollback root, private manifests and local checkpoint refs. | User-local lifecycle only for the approved local scope. It does not prove common readiness. |

Mixing user-local private material into common repo training is forbidden unless a later source/license/privacy packet explicitly approves the exact material and route.

## DPO chosen/rejected/provenance approval

Every DPO row must include:

- chosen response ref;
- rejected near-miss ref;
- provenance ref;
- source/license/privacy/redaction classification;
- route: common, user-local, evidence-only or quarantine;
- reviewer/adjudication ref;
- material quality decision;
- exact approval for authoring/training if any.

Candidate rows alone do not prove training. Schema-only rows are not trainable rows.

## Backup/export/import/restore/load-without-retrain/continue-training/fresh-load

For each approved local or common target, future packets must show:

1. backup before mutation;
2. export from existing state;
3. import into clean target root;
4. share rules and private/public boundary;
5. restore from backup;
6. load without retrain;
7. continue-training from existing checkpoint, with base checkpoint sha;
8. fresh-load after training;
9. held-out evaluation and ablation/no-memorization controls;
10. rollback to named rollback checkpoint.

Saved checkpoint only is explicitly insufficient. A checkpoint file without manifest, data refs, import/export path, fresh-load, continue-training proof and rollback proof stays narrow artifact evidence.

## No-code algorithm

1. Select exact host/method row.
2. Identify whether material is common or user-local.
3. Verify source/license/privacy/redaction route.
4. Build DPO candidate with chosen/rejected/provenance refs.
5. Obtain exact approval before any training or checkpoint mutation.
6. Run lifecycle only in a future authorized work order.
7. Record all paths, hashes, target locator, rollback locator and reports.
8. Keep claim locks false until every required evidence packet passes.

## Current boundary

No training was performed by this docs update or docs QA review. No continue-training was performed. No checkpoint was promoted. No target locator was updated. No final readiness review passed. The current status remains: trainable DPO rows 0/9, lifecycle rows 0/9, token pairs 0/9, quality rows 0/9 and final readiness reviews 0/10.
