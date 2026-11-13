# Запись хешей и прогонов — попытка 0002, конфигурация Release

Ветка `codex/dpo-implementation`, вершина `67767526165`.
Рабочий корень `C:\Projects\llm_mvp_core`, других копий и песочниц не создавалось.

## Что это за попытка

Повторный прогон на текущих байтах в конфигурации Release с сохранением
журналов прогонов файлами в репозиторий. Ни одной строки продукта и ни одной
строки логики проверок в этой попытке не менялось, кроме временной мутации в
шаге четвёртом, которая восстанавливается дословно в том же ходе.

Предыдущая попытка (каталог `artifacts/fbe/FIX-SUBMIT-DPO-PINS/attempt-0001`)
создана по указанию, которое затем было отменено. Её файлы оставлены на месте
и не удалялись и не перемещались; судьбу того каталога решает согласующая
сторона.

## Исходное состояние до прогонов

| Что | Значение |
|---|---|
| хеш продуктового согласователя цикла улучшений | `68f8270cfdc6f9312cd6e428e6c71aa778dca1cc` |
| хеш файла проверок | `0402c26cc332a2a834d2b7d014c7baca09c9d9af` |
| `git diff --check` по файлу проверок | код возврата 0, замечаний нет |
| маркеров конфликта в файле проверок | 0 |
| продуктовый файл в `git status` | отсутствует, то есть не изменён |

Файл продукта: `44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs`.
Файл проверок: `Tests/DesignConversionModelServerApiTests.cs`.

## Личность проверяемого двоичного файла

| Что | Значение |
|---|---|
| путь | `Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll` |
| хеш содержимого | `bbece33c73516308cbc046f5325a57fc923af359` |
| размер | 28 000 768 байт |
| время изменения | 2026-11-09 23:14:08 |
| время последней правки исходника проверок | 2026-11-09 23:13:05 |

Двоичный файл датирован позже последней правки исходника, поэтому прогоны идут
на текущих байтах файла проверок.

## Честная оговорка о сборке Release

Моя сборка Release нашла все проекты готовыми: ноль предупреждений,
перекомпиляции не было, время 32 секунды. Это значит, что сам двоичный файл
собрал параллельный прогон второго исполнителя в 23:14:08, а не моя команда в
23:35. Для моих двух семей это не меняет вывода, потому что двоичный файл
новее моего исходника, но названо это прямо: изолированной собственной сборки
«с нуля» в этой попытке не было, и утверждать обратное я не могу.

Из того же следует вторая оговорка: этот двоичный файл собран из дерева, в
котором одновременно лежат незавершённые изменения второго исполнителя
(`27.TranslationDomain/TranslationMachine.cs` и новый файл проверок в
`Tests/ExperimentFixes/`). К моим двум семьям они отношения не имеют, но
чистым «только моим» дерево в момент сборки не было.

## Таблица «команда → код возврата → прошло/упало»

| Шаг | Команда | Код возврата | Прошло | Упало | Журнал |
|---|---|---|---|---|---|
| 1 | `dotnet build Tests/LlmFromScratch.Tests.csproj -c Release` | 0 | ошибок 0, предупреждений 0 | — | `step-1-build-release.log` |
| 2 | `dotnet test ... -c Release --results-directory .../family-model-server-api --logger "trx;LogFileName=family-model-server-api.trx" --filter "FullyQualifiedName~DesignConversionModelServerApiTests"` | 0 | 317 прошло, 4 пропущено, всего 321 | 0 | `step-2-family-model-server-api.log` + `family-model-server-api/family-model-server-api.trx` |
| 3 | `dotnet test ... -c Release --results-directory .../family-teacher-production-path --logger "trx;LogFileName=family-teacher-production-path.trx" --filter "FullyQualifiedName~DesignConversionDpoTeacherProductionPathTests"` | 0 | 94 прошло, 0 пропущено, всего 94 | 0 | `step-3-family-teacher-production-path.log` + `family-teacher-production-path/family-teacher-production-path.trx` |
| 4 | цикл мутации, выполняется только после промежуточного доклада | не выполнялся | — | — | — |

Время прогонов: шаг 2 — 2 минуты 59 секунд, шаг 3 — 45 секунд.

## Адресное подтверждение изменённых проверок в машинном результате

В файле `family-model-server-api/family-model-server-api.trx` три проверки
договора репозиторного набора присутствуют по именам с исходом `Passed`:

- `SubmitAndEvaluateAsync_PublicRepositoryOwnedDpoContextIsHonouredBySealedDatasetReuseStop`
  (проверка повторного использования, где закреплены четыре различающие величины);
- `SubmitAndEvaluateAsync_PublicRepositoryOwnedDpoContextWithoutSealedDatasetRefuses_ObservedBehaviourPin`
  (закрепление наблюдения об отказе, где закреплена полная строка предупреждения);
- `SubmitAndEvaluateAsync_InvalidClientRepositoryOwnedDpoContextStaysRefused`
  (отказ по негодному клиентскому договору, не менялась).

Это проверяемо по файлу, а не по моему слову.

## Каталоги результатов проверки качества, появившиеся от моих прогонов

Созданы самими прогонами, не удалялись и не перемещались:

- от остановленного прогона в конфигурации Debug, время 23:32:32-23:32:39:
  `qa-results/q01-concurrency-290e44406017414eb280844c5cce8ab9`,
  `qa-results/q01-concurrency-9f9d58d3ef4b4e308ea02cef47dee24f`,
  `qa-results/q01-concurrency-a0d1f53d3590402ca7278a8a531af3d2`,
  `qa-results/q01-output-policy-b3a0ec66c08a40a69f19b23b4e1a0bed`;
- от шага 2, время 23:39:18-23:39:22:
  `qa-results/q01-concurrency-5fbf071294d54c50b8b6cd0fd729ed38`,
  `qa-results/q01-concurrency-c62c3751450247f492b175488e2261fc`,
  `qa-results/q01-concurrency-bd8ba5e21089450fb0fe165659269455`,
  `qa-results/q01-output-policy-42c517b78e0e410b9053fa0c0c13fd2a`.

Остальные каталоги в `qa-results` старше и моим прогонам не принадлежат.

## Состояние после шага 3

| Что | Значение |
|---|---|
| хеш продуктового согласователя | `68f8270cfdc6f9312cd6e428e6c71aa778dca1cc`, не изменён |
| хеш файла проверок | `0402c26cc332a2a834d2b7d014c7baca09c9d9af`, тот же, что до прогонов |
| `git diff --check` по файлу проверок | код возврата 0 |
| маркеров конфликта | 0 в файле проверок и 0 во всех файлах этого каталога |
| метка порядка байтов в моих файлах | отсутствует |

Флаг пропуска сборки не применялся ни в одной команде этой попытки.
Повторов сборки из-за блокировок промежуточных каталогов: 0.
Команд записи Git не выполнялось.
