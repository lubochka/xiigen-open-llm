# P19: existing common model vs P17E candidate

## Человеческий вывод

P17E candidate не должен напрямую заменять текущую common-модель. Он меньше, слабее по manifest evidence и сам помечен как no-claim artifact. Прямой promote был бы не улучшением, а перезаписью существующего runtime asset без positive promotion gate.

## Что читает runtime сейчас

- Runtime/common manifest: `models/skill-selection/skill-selection-v1/manifest.json`
- Runtime/common checkpoint: `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json`
- Code ref: `43.SmartSkillsDomain/XiigenExtensionApiService.cs:637-639` строит именно этот path.
- Code ref: `43.SmartSkillsDomain/XiigenExtensionApiService.cs:717-739` читает checkpoint и загружает его в `SkillSelectionMachine.LoadCheckpoint`.

## Где лежит P17E candidate

- Candidate manifest: `artifacts/smart-skills-semantic-quality-completion/p17e-skillselection-lifecycle-import-export/production-export/skill-selection-manifest.json`
- Candidate checkpoint: `artifacts/smart-skills-semantic-quality-completion/p17e-skillselection-lifecycle-import-export/production-export/skill-selection-checkpoint.json`
- Locator: `artifacts/smart-skills-semantic-quality-completion/p17e-skillselection-lifecycle-import-export/p17e-manifest-locator-map.json`

## Сравнение manifest/checkpoint

| Поле | Existing common | P17E candidate | Вывод |
|---|---:|---:|---|
| checkpoint bytes | 1309772 | 17711 | direct overwrite unsafe |
| manifest accuracy | 1 | 0 | candidate не готов к promote |
| semantic quality claimed | false | false | оба no-claim, P19 не поднимает claim |
| trained pair ids | 900 | 19 | candidate намного меньше |
| trained prompt keys | 75 | 17 | candidate покрывает меньше prompt keys |
| candidate-position weights | 5 | 0 | P17E не использует их, но route mode всё ещё scaffold-labelled |

## Секции профилей

| Section | Existing profiles | Candidate profiles | Вывод |
|---|---:|---:|---|
| `prompt_understanding` | 65 | 2 | candidate меньше |
| `task_understanding` | 65 | 3 | candidate меньше |
| `task_type` | 65 | 0 | candidate меньше |
| `routing` | 65 | 0 | candidate меньше |
| `skill_candidates` | 65 | 0 | candidate меньше |
| `skill_selection` | 65 | 5 | candidate меньше |
| `prompt_context` | 81 | 2 | candidate меньше |
| `result_gap` | 65 | 0 | candidate меньше |
| `output_validation` | 65 | 0 | candidate меньше |
| `task_completion` | 65 | 0 | candidate меньше |
| `intermediate_checks` | 65 | 3 | candidate меньше |

## Решение

Direct promotion blocked. P17E остаётся artifact-only candidate до отдельной promotion phase с compatibility/non-regression, rollback/load proof, truthful manifest rewrite и fresh-load через runtime/common loader.
