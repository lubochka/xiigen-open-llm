# Extension development status package - README

## What to open first

Open in this order:

1. CURRENT-EXTENSION-DEVELOPMENT-REPORT-2026-07-08.md - main status: what this project is, what is done/partial/not started/needs fix, and what is not proven.
2. EXTENSIONS-IMPLEMENTATION-STATUS-BOARD.md - фазовая доска с human counts.
3. REQUIREMENTS-COVERAGE-MATRIX.md - Luba requirements -> evidence/status in this package.
4. PLANS-AND-SUPPORTING-FILES-INVENTORY.md - which plans and supporting files were copied.
5. 00-PATTERN-AND-SOURCE-INVENTORY.md - как использованы pattern folders.
6. copy-manifest.json - machine-readable source -> destination manifest.
7. 50-PASS-SUBAGENT-REVIEW-LEDGER.md - prepared ledger for the next review stage; it is 0/50 pending, not completed passes.

## Как читать статус

This package does not claim product readiness. It states the extension project stage by evidence-package state. For extension/runtime evidence, it uses dual-path same-scenario coverage: the same scenario needs automated boundary proof and local desktop proof. Only one path, or different scenarios, gives partial evidence, not completion.

## Текущие counts

| Status | Count |
|---|---:|
| Total phases | 15 |
| Done | 2 |
| In progress | 2 |
| Partial | 8 |
| Not started | 1 |
| Needs fix | 2 |
| Remaining / not fully closed | 13 |

The phrase "remaining 13" means: 13 phases are not fully closed as product work. This is not a package refresh error; it is an honest evidence boundary.

## Где лежат файлы

| Папка | Назначение |
|---|---|
| copied-plans | Копии planning/status/state файлов из C:\Projects\llm_mvp_core\docs\plans. |
| supporting-files | HOWTO, project docs, selected skills, and selected evidence copies. |
| supporting-files\evidence-extension-full-readiness-dual-path | Selected evidence/status packets for extension dual-path work. |

## Что не делать по этому пакету

Do not read this package as a product-ready claim. Do not count the 50-pass ledger as completed. Do not count old ledgers or previous summaries as fresh verification. Do not infer git/branch/readiness claims from copied files existing.
