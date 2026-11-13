# PromptToTask manifest checkpoint ref fix

## Что было не так

P11 fresh-load и P12 no-op continue-training брали PromptToTask candidate refs из `artifacts/smart-skills-semantic-quality-completion/p10-checkpoint-rewrite/smart-skills-p10-component-manifests.json`.

P10 указывал checkpoint и manifest в текущей рабочей копии как repo-relative paths, но сам `prompt-to-task-p7f-candidate-manifest.json` содержал абсолютные refs старой рабочей копии:

- `C:\Projects\llm_mvp_core\...\prompt-to-task-p7f-candidate-checkpoint.json`
- `C:\Projects\llm_mvp_core\...\prompt-to-task-p7f-candidate-manifest.json`

`PromptToTaskMachine.LoadCheckpointAsync` строго проверяет, что `manifest.checkpointRef` и `manifest.manifestRef` после разрешения указывают на фактически переданные файлы. В workspace `C:\Projects\llm_mvp_core_codex_sync_20260628` эти absolute refs разрешались в другую директорию, поэтому validator возвращал `Manifest checkpoint ref mismatch`.

## Исправление

В `artifacts/smart-skills-semantic-quality-completion/p7f-prompt-to-task-missing-coverage-repair/prompt-to-task-p7f-candidate-manifest.json` заменены только manifest refs на portable refs рядом с manifest:

- `checkpointRef`: `prompt-to-task-p7f-candidate-checkpoint.json`
- `manifestRef`: `prompt-to-task-p7f-candidate-manifest.json`
- `sourceManifestRef`: `prompt-to-task-p7f-source-manifest.json`

Checkpoint JSON не изменялся, поэтому `candidate_checkpoint_sha256` из P10 остается тем же. Product loader не ослаблялся.

## Проверки

- P11/P12: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~SmartSkillsSemanticQualityP11FreshLoadTests|FullyQualifiedName~SmartSkillsSemanticQualityP12ContinueTrainingTests" --logger "trx;LogFileName=prompt-to-task-p11-p12.trx"` - Passed 2, Failed 0.
- Adjacent lifecycle: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~PromptToTaskMachineAssetLifecycleTests" --logger "trx;LogFileName=prompt-to-task-asset-lifecycle.trx"` - Passed 5, Failed 0.

## Что не доказано

Эта правка доказывает только, что PromptToTask P7F candidate manifest/checkpoint refs теперь согласованы для P11/P12 lifecycle gates и что соседний PromptToTask lifecycle class не регрессировал. Она не доказывает full semantic quality, product readiness, token savings, checkpoint promotion, locator update, unified Row12/P01 gate или SkillSelection source-disjoint runtime eval.
