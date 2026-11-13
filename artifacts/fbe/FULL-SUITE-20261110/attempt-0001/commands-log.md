# Журнал команд с кодами возврата

Все команды выполнены в рабочем корне `C:\Projects\llm_mvp_core`, ветка
`codex/dpo-implementation`. Записи идут в порядке исполнения. Коды выхода
сборки и прогона снимались **отдельной командой** сразу после простой
команды с перенаправлением, а не через `$?` после конвейера.

## Чтение состояния (шаг 0 и шаг 1)

| № | Команда | Код | Что дала |
|---|---|---|---|
| 1 | `git rev-parse HEAD` | 0 | `cd7268a17721afc7d3df6a3a388b3ff9d2533ee7` |
| 2 | `git rev-parse --abbrev-ref HEAD` | 0 | `codex/dpo-implementation` |
| 3 | `git rev-parse --show-toplevel` | 0 | `C:/Projects/llm_mvp_core` |
| 4 | `git status --porcelain` | 0 | одна строка: ` M Tests/ExperimentFixes/TranslationPhraseCoverageLearningTests.cs` |
| 5 | `git log -1 --format=... babe5cba7db` | 0 | коммит починки существует, 9 ноября 15:46 |
| 6 | `grep -n "1488" CLAUDE.md` | 0 | строка 1098, объявленный уровень |
| 7 | `dotnet --version` | 0 | `10.0.302` |
| 8 | `git diff --numstat` | 0 | `10 0 Tests/ExperimentFixes/TranslationPhraseCoverageLearningTests.cs` |
| 9 | `git diff -- Tests/.../TranslationPhraseCoverageLearningTests.cs` | 0 | все 10 добавленных строк — комментарии |
| 10 | `git ls-files --others --exclude-standard` | 0 | 4 неотслеживаемых: 1 чужой + 3 моих улики |
| 11 | `git diff --cached --name-only` | 0 | пусто (ноль проиндексированных) |
| 12 | `grep -i "Project(" LlmFromScratch.sln \| grep -iE test` | 0 | три тестовых проекта + один инструмент |
| 13 | `git grep -l --text "TranslationDpoTrainEvaluateRunnerTests" -- 'Tests/**'` | 0 | `Tests/TranslationDpoTrainEvaluateRunnerTests.cs` |
| 14 | `git grep -l --text "TranslationDpoTrainerBridgeApiBoundaryTests" -- 'Tests/**'` | 0 | `Tests/TranslationDpoTrainerBridgeApiBoundaryTests.cs` |

Примечание к 13–14: применён `git grep --text`, а не `git grep -I`, —
последний слеп к путям со снятым текстовым атрибутом.

## Сборка (шаг 2)

| № | Команда | Код | Журнал |
|---|---|---|---|
| 15 | `dotnet build Tests/LlmFromScratch.Tests.csproj -c Release > build-release.log 2>&1` | **0** | `build-release.log` |

Код выхода снят отдельной командой и записан в `build-exit.txt`:

```
ts_start=2026-11-10T09:45:28Z
BUILD_EXIT=0
ts_end=2026-11-10T09:49:49Z
```

Итог сборки из журнала: `251 Warning(s)`, `0 Error(s)`,
`Time Elapsed 00:04:18.91`. Предупреждения — анализаторы xUnit
(`xUnit2031`, `xUnit2029`, `xUnit2012`, `xUnit2009`, `xUnit2000`);
ошибок нет, сборка не падала, скрывать нечего.

## Проверка приборов на эталоне

| № | Команда | Код | Что дала |
|---|---|---|---|
| 16 | `python -I <scratchpad>/count_trx.py artifacts/fbe/FIX-124-S05/attempt-0001/red-run-decisive-tests.trx` | 0 | два способа подсчёта сошлись (10/10, 1 зелёный, 9 красных); ловушка `grep` доказана: наивный счёт 10 против верных 9 |

Подробности — `probe-verification-on-reference.md`.

## Проверка свежести библиотеки (ловушка прежней библиотеки)

| № | Команда | Код | Что дала |
|---|---|---|---|
| 17 | `stat -c %Y Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll` + `find . -name '*.cs' ... -printf '%T@'` | 0 | библиотека Release `1794304188` (11:49:48) новее новейшего исходника `1794303018` (11:30:18) |

Записано в `freshness-proof.txt`, вердикт
`RELEASE_LIBRARY_IS_NEWER_THAN_SOURCES_OK`.

## Независимый подсчёт по исходникам

| № | Команда | Код | Что дала |
|---|---|---|---|
| 18 | разбор атрибутов xUnit по 643 файлам основного набора | 0 | `[Fact]` 8 651, `[Theory]` 1 247, `[InlineData]` 5 669, `[MemberData]` 78 |

## Полный прогон (шаг 3)

| № | Команда | Код | Журнал |
|---|---|---|---|
| 19 | `dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --logger "trx;LogFileName=full-suite.trx" --results-directory <каталог улик>` | см. `run-exit.txt` | `full-run.log`, `full-suite.trx` |

Фильтра (`--filter`) в команде **нет** — это обязательное условие наряда.
Путь файла результатов задан явно, чтобы улика легла в хранилище проекта,
а не в каталог раннера по умолчанию.

Код выхода и метки времени — в `run-exit.txt`. Числа и разбор — в
`report.md`.

## Прочие прогоны

Состояние двух малых наборов (`DesignConversion.ModelActivation.Tests`,
`Tests.ApiDashboardWpf`) — в `report.md`. Параллельно с основным прогоном
они **не** запускались: одновременный прогон мог бы исказить основное
измерение через общие временные файлы и блокировки реестра.
