# Row14.1 Backup/Export Preflight Evidence

Status: ROW14_1_BACKUP_EXPORT_PREFLIGHT_PASS

## Boundaries

- trainingPerformed=false
- checkpointMutation=false
- locatorUpdate=false
- promotion=false
- uiRun=false
- readinessClaimed=false
- No training, no mutation
- Rollback: restore the copied checkpoint and manifest from backup-export to the expected asset-root paths only after an explicit rollback work order; do not update locators or promote as part of rollback.

## Files

- Role: assetRootCheckpoint
  - Source: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-targetlocator-non-ui-proof-20260626T094740Z\adaptive-asset-root\skill-selection\skill-selection-v1\skill-selection-v1-checkpoint.json
  - Destination: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-learnability-training-continue\row14-1-backup-export-preflight-no-training-20260626T102311Z\backup-export\skill-selection-v1-checkpoint.json
  - Source SHA256 before: 6b711271fd3f1c792fdaecf6610bed2a7791adfc4a2f4d04eb4a69dc6783419f
  - Source SHA256 after: 6b711271fd3f1c792fdaecf6610bed2a7791adfc4a2f4d04eb4a69dc6783419f
  - Destination SHA256: 6b711271fd3f1c792fdaecf6610bed2a7791adfc4a2f4d04eb4a69dc6783419f
  - Source size bytes: 29225
  - Destination size bytes: 29225
  - Source unchanged after copy: True
  - Copy matches source: True
  - Staged peer matches asset-root source: True
- Role: assetRootManifest
  - Source: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-semantic-quality-heldout\row13-targetlocator-non-ui-proof-20260626T094740Z\adaptive-asset-root\skill-selection\skill-selection-v1\manifest.json
  - Destination: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-learnability-training-continue\row14-1-backup-export-preflight-no-training-20260626T102311Z\backup-export\manifest.json
  - Source SHA256 before: 4dc0b69deecd73645c681fe193fb58fb3febdf106dc3de5f664df850d01f5c7d
  - Source SHA256 after: 4dc0b69deecd73645c681fe193fb58fb3febdf106dc3de5f664df850d01f5c7d
  - Destination SHA256: 4dc0b69deecd73645c681fe193fb58fb3febdf106dc3de5f664df850d01f5c7d
  - Source size bytes: 503
  - Destination size bytes: 503
  - Source unchanged after copy: True
  - Copy matches source: True
  - Staged peer matches asset-root source: False

## Rollback

Rollback: restore the copied checkpoint and manifest from backup-export to the expected asset-root paths only after an explicit rollback work order; do not update locators or promote as part of rollback.

## Next

Row14.2 DPO rows only
