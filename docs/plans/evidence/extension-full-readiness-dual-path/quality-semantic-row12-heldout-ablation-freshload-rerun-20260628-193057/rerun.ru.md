claim_scope: focused post-fix rerun QUALITY-SEMANTIC-ROW12-HELDOUT-ABLATION-FRESHLOAD-RERUN-01 only
evidence_scope: dotnet focused test filter, command logs in this folder, and existing-evidence scan for the P22 claim boundary
count_claimed: 14 tests / 14 passed / 0 failed
independent_evidence_count: 1 test-run log plus context scans and verification logs
subagent_id_or_NONE: NONE
downgrade_needed: yes; full semantic quality is not claimed

# Rerun status

Папка: `docs\plans\evidence\extension-full-readiness-dual-path\quality-semantic-row12-heldout-ablation-freshload-rerun-20260628-193057`

Команда:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -p:IsTestProject=true --filter "FullyQualifiedName~PromptToTaskPhase04UniversalContractTests|FullyQualifiedName~ApiDashboardSourceDisjointGuardTests|FullyQualifiedName~SmartSkillsSemanticQualityP11FreshLoadTests|FullyQualifiedName~SmartSkillsSemanticQualityP12ContinueTrainingTests|FullyQualifiedName~OrchestrationReasoningP9EvalAblationTests|FullyQualifiedName~OrchestrationReasoningP9BEvalAblationRerunTests" --no-restore --logger "console;verbosity=normal"
```

Результат: exit code `0`; restore не запускался, потому что `--no-restore` прошёл.

## Итоги тестов

| Метрика | Значение |
|---|---:|
| Total tests | 14 |
| Passed | 14 |
| Failed | 0 |
| Total time | 2.9850 Seconds |

## P11/P12

P11 fresh-load: green in this focused gate.

P12 continue-training: green in this focused gate.

Узкий вывод: после manifest checkpoint ref fix текущий focused post-fix gate зелёный для P11/P12 и связанных PromptToTask/P9/P9B/API guard families.

## Claim boundary

Full semantic quality нельзя заявлять. Этот rerun не выполнял и не принимал SkillSelection true source-disjoint runtime eval. Existing evidence/state scan всё ещё показывает P22 gap: accepted rows = 0 / `accepted_p22_eval_rows = 0`.

Я не заявляю product readiness, semantic quality, token savings, training success, checkpoint promotion или host runtime readiness.

## Что осталось блокировать

1. SkillSelection source-disjoint runtime eval missing.
2. P22 accepted rows = 0 remains the full semantic quality blocker.
3. Нужны accepted runtime eval rows with source/pair/id disjointness, candidate-order controls, filename/path/title controls, exact prompt replay controls, and no training use.

## Exact next bounded work order

`SKILLSELECTION-TRUE-SOURCE-DISJOINT-RUNTIME-EVAL-01`: create/run accepted SkillSelection runtime eval rows so the P22 accepted rows = 0 blocker is closed; then rerun focused P22 plus the broader semantic precondition gate.

## Report generation note

Первый JSON parse-check поймал неверные нули из-за NUL-separated test log. This file was rewritten from normalized log text; see `06_inspect_test_log_tail.log` and `07_rewrite_rerun_files_after_nul_parse.log`.

## Logs

- `00_create_folder.log`
- `01_dotnet_test_no_restore.log`
- `02_extract_and_context_scan.log`
- `03_p22_accepted_rows_scan.log`
- `04_write_rerun_files.log`
- `05_parse_rerun_json.log`
- `06_inspect_test_log_tail.log`
- `07_rewrite_rerun_files_after_nul_parse.log`
