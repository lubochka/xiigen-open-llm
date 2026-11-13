# 66. E22: корпус положительного запуска и граница восстановленной истории

16 октября 2026. **READ_ONLY_RESEARCH**. Worker `/root/rp01_code_trace_astra`, Astra ultra; без вложенных агентов. Создан только этот Markdown. Читались сохранённые JSON, тексты, ZIP и Git history; loader, C# parser, обучение, inference, tests/build и probes не запускались. ZIP не распаковывался в рабочее дерево, DLL не загружались. Изменений продукта, корпуса, весов и Git нет.

**Положительный E22 подтверждается более полным архивным контекстом: +19,89% относительного macro-F1 при 360/90 примерах, tier 3, явно указанном корне `C:\Projects\llm_mvp_core`.** Найден общий aggregate, связывающий этот результат с предыдущей попыткой tier 2. Однако точный список 450 принятых файлов, их порядок, хеши, индивидуальные метки и команда запуска в ограниченно проверенных источниках не найдены. Это ограничение переноса и воспроизводимости результата; оно не превращает положительный результат в неуспех.

Core при начале чтения: `216a1f3d6af3f40ceef081dc168b07e4ce3934f9`; перед возвратом HEAD стал `793d7b755b23aac90445da3ac6661b1bff7e6672`. Scoped diff между ними и рабочим деревом пуст для E22, PredicateDatasetRouter, ExperimentsV2.csproj, всего рассматриваемого run-архива и ZIP. Пакет продолжает [65](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md:69). Повторный разбор DatasetLoader, ExperimentRunner/Dispatch и ResultWriter не выполнялся: их уже прочитанные parent свойства — численно отсортированные каталоги проектов для tier≥3, несортированный Directory.GetFiles внутри, фильтр obj/bin и отсутствие сохранения file manifest — приняты как переданный контекст, а не заявлены новым независимым чтением.

## Что теперь известно о конкретном запуске

[Общий aggregate:1–7](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/e01-e53-tier4-aggregate.json:1) содержит run_id `exp35-e01-e53-tier4-20260507-190227`, project_root `C:\Projects\llm_mvp_core`, output_root этого архива, max_tier=4 и UTC-интервал всей серии `19:02:27.7379319Z–19:02:47.3363953Z`. [Companion всей серии:3](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/e01-e53-tier4-summary.md:3) повторяет корень, а строка45 — переход E22 с tier 2 на tier 3 за две попытки. Корень восстановлен **из сохранённого отчёта**, не из догадки по сегодняшней директории.

| Попытка E22 внутри этой серии | Сохранённый результат | Связь с составом |
|---|---|---|
| [Tier 2:1049](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/e01-e53-tier4-aggregate.json:1049), `19:02:43.0832876Z–19:02:43.1262049Z` | macro-F1 0,4761904762 → 0,4761904762; passed=false; `SCALE_UP_DATA_TOO_SMALL` | notes: examples=17, train=13, val=4. |
| [Tier 3:1075](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/e01-e53-tier4-aggregate.json:1075), `19:02:43.1263061Z–19:02:44.211869Z` | **0,47093037316501 → 0,5645912342677316; +19,88847320958498%; passed=true** | notes: examples=450, train=360, val=90; baseline=top3freq. |

У обеих попыток `success=true` означает успешное исполнение попытки без записанной ошибки, а качество отдельно задаёт `passed`. У обеих `evidence_path=null` ([1073](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/e01-e53-tier4-aggregate.json:1073), [1099](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/e01-e53-tier4-aggregate.json:1099)): aggregate не указывает отдельный E22 manifest или per-file receipt.

Отдельный [run-2026-05-07-002.json:3–17](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json:3) округляет метрики до 0,4709/0,5646/19,89 и имеет run_at `19:02:44.1971218Z`, внутри указанного интервала попытки. [runs-flat-format:615](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/runs-flat-format.json:615) использует время её завершения `19:02:44.211869Z`. Эти разные timestamp-поля не свидетельствуют о независимых положительных запусках.

Прочитаны схемы и E22-участки runs-flat-format, [progress-final-format:212](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/progress-final-format.json:212), [progress-by-tier-format:374](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/progress-by-tier-format.json:374) и [progress/progress.json:212](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/progress/progress.json:212). Они сохраняют статус, метрики, tier и время, но не дополняют E22 последовательностью файлов или labels. Это несколько представлений свидетельства, не несколько независимых подтверждений улучшения.

**450 — число принятых examples, не доказанное число всех прочитанных loader файлов.** В `88a880abf4:35.ExperimentsV2/Experiments/E22DatasetRouter.cs:22–30` цикл отбрасывает неуспешное/пустое извлечение предикатов и сохраняет только строки с непустой меткой; path из входной пары отбрасывается через `foreach (var (_, text) in files)`. Первые 80% уже отфильтрованной последовательности идут в train на исторических строках36–38. Нынешний эквивалент: [E22:27–43](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E22DatasetRouter.cs:27). Из счётчиков 360/90 нельзя восстановить ни отброшенные пути, ни состав получившегося split.

## Source history и изменение label function

Aggregate и результаты добавлены `88a880abf473f74981e32740dbe0ed4b5065b106`, `latest runs`, 7 мая 23:19:15+03:00. Diff этого коммита содержит только пути результатов; для исследуемого архива последующий diff `88a880abf4..HEAD` пуст. Это подтверждает сохранность записанного результата, но commit добавления в 20:19 UTC не является SHA процесса, завершившегося около19:02 UTC.

Есть адресное уточнение к SHA переноса в65. `git merge-base --is-ancestor 10364aef4016ceeb66877f0f62dac8cf6fecd43d 88a880abf4` вернул1. В ancestry архивного коммита найден **`cb88fee44619ed00b59ebd6e4830676146e6bf4b`**, с теми же датой6мая22:00:53+03:00 и сообщением `wire canonical semantic sources through orchestration`; ancestor-check для cb88 вернул0. Причина существования двух SHA здесь не исследовалась.

При этом содержательная проверка65 сохраняется: `git rev-parse <rev>:<path>` дал одинаковые blob IDs во всех трёх refs — 10364, cb88 и88:

| Файл | Один и тот же Git blob |
|---|---|
| `35.ExperimentsV2/Experiments/E22DatasetRouter.cs` | `36cc6f6fe1373673882f5651d7b4d6dcc53d0dd2` |
| `22.AgentAlgorithms/Context/PredicateDatasetRouter.cs` | `b191e3deed01fc83433f46f959db369764935554` |

В source, сохранённом вместе с receipt, E22 вызывает `AgentAlgorithms.PredicateDatasetRouter.AutoLabel` на исторической строке27, а `PredicateDatasetRouter.cs:71–76` использует строковую NeedsNullCheck: наличие точки и отсутствие ` null`/`is null` во всём файле. Это та же новая label function, которую65 сравнил со старой AST-эвристикой; нынешний якорь — [NeedsNullCheck:76](C:/Projects/llm_mvp_core/22.AgentAlgorithms/Context/PredicateDatasetRouter.cs:76). **Связь с поздним положительным receipt теперь точнее на уровне Git-содержимого и chronology. Связь с фактически загруженной DLL и индивидуальными 450 labels всё ещё не аттестована.** Нельзя приписать весь прирост переносу: одновременно изменился входной набор, а парного опыта на фиксированном corpus/labels в этом архиве нет.

## Что добавил ZIP и чего он не доказывает

В корне найден [35.ExperimentsV2.zip](C:/Projects/llm_mvp_core/35.ExperimentsV2.zip). Проверено его оглавление: 1613 entries, 86 имён DLL/EXE/PDB. В именах entries не обнаружены Program.cs, manifest, `.log`, `.ps1`, `.sh`, `.cmd`. Прочитаны только относящиеся к E22 source/JSON и названные ниже текстовые build metadata; бинарники не анализировались и не исполнялись.

Копии aggregate и положительного E22 JSON внутри ZIP побайтно совпадают с текущими файлами архива. SHA256: aggregate `0c59105aaf526e5dde9974d8c4af06983f72a614a7acba1372179ab8d52a7052`; E22 JSON `5f5318a96717918508d890d21fb423ea96ac3d57e942a1f04bbe6b9497ab146a`. ZIP-копия E22DatasetRouter совпадает с source88 после нормализации CRLF→LF; это сравнение сохранённых текстов, не повторное вычисление labels.

Сам нынешний ZIP **не заморожен на7мая**. Его Git history показывает обновления24мая (`5f948431a2872cd17a346636d01eb56e7c05fb77`) и4июня (`6bb3f21bb0a513be613d5825fd12009c5d2a5d6b`); размер отличается от ZIP при88. Два внутренних `obj/{Debug,Release}/net9.0/ExperimentsV2.csproj.FileListAbsolute.txt` прочитаны целиком: 52 и50 строк build outputs, references и generated metadata; это не список450 входных C# примеров.

В ZIP есть точные, но разные SourceLink refs:

| Entry внутри `35.ExperimentsV2.zip!/35.ExperimentsV2/` | Сохранённая ссылка на исходники |
|---|---|
| `obj/Debug/net9.0/ExperimentsV2.sourcelink.json:1` и `ExperimentsV2.AssemblyInfo.cs:16` | `66860d15d7dc4370979c2951c96b324f7eddcd6e`, commit29мая07:25:05+03:00. Это позднее положительного запуска. |
| `obj/Release/net9.0/ExperimentsV2.sourcelink.json:1` и `ExperimentsV2.AssemblyInfo.cs:16` | `d97fe37ed74c6e895e967b2eb2afcab94dfedfbc`, commit6мая23:45:22+03:00. Временная совместимость есть, привязки к запуску7мая нет. |

В обоих этих refs blobs E22/router снова равны двум значениям выше. Release metadata даёт конкретного кандидата для версии сборки, **но не доказывает выбор Release, отсутствие dirty changes или совпадение корпуса**. Debug показывает, почему нельзя считать всё содержимое ZIP единым runtime snapshot7мая. Даже подтверждение DLL не восстановит порядок Directory.GetFiles или незафиксированные входные файлы.

## Команда, граница поиска и полезный механизм

При88 `35.ExperimentsV2` имеет ExperimentRunner, dispatcher и [проект ExperimentsV2.csproj:3–9](C:/Projects/llm_mvp_core/35.ExperimentsV2/ExperimentsV2.csproj:3), но отдельного Program.cs в этом модуле нет. В tracked C#/PowerShell/sh/cmd этого исторического ref поиск `ExperimentRunner`, `e01-e53-tier4` и `190227` не нашёл внешний entry point, запускающий данную серию. Найденные одноимённые тестовые callers относятся также к34.Experiments; они не объявлены командой запуска35. Текстовый поиск не исключает временный/untracked runner, reflection или внешнюю сессию.

Граница поиска: верхний уровень run-архива и E22-каталога, сводные JSON/Markdown и отдельный E22 receipt, historical tree и адресные refs88/cb88/10364, оглавление и узкие entry ZIP, непосредственные имена детей `.tmp`, `temp`, `tools`. В этих границах **точный command receipt, ordered corpus manifest и per-file labels не найдены**. Весь диск, shell history, внешние журналы чатов и все ветки не обходились. Один широкий HEAD grep оказался переполнен поздними index-артефактами и не использован как доказательство отсутствия; вывод основан на ограниченном историческом поиске и фактически прочитанных файлах.

Положительный смысл результата остаётся прежним: kNN использовал память учебных пар «предикаты → шесть dataset labels» и на сохранённом контрольном split лучше baseline top3freq воспроизвёл эту разметку. Это полезный механизм предметного выбора по урокам. Недостающий corpus manifest ограничивает ответ на другой вопрос — переносится ли выигрыш на независимые исходники/семейства. Отсутствие manifest не доказывает leakage; отсутствие явного group-disjoint split также не доказывает независимость. Это не neural free generation RP01, и consumer-предел65 данным пакетом не снимается.

## Один будущий контроль, только проект

При отдельной авторизации заморозить один source snapshot с ordered manifest, SHA256 каждого текста, extracted predicates, labels и стабильным source-family/group ID. Если старые 450 примеров не восстановлены, назвать это новым контролем механизма, а не воспроизведением 7 мая. Зафиксировать одну label function, extractor, k=3 и top3freq baseline; отделить группы/семейства исходников целиком между train и heldout, не допуская точных копий и производных одного исходника по обе стороны. Для каждого split baseline заново считается только по его train; сравнивать прирост kNN над baseline и per-bucket support/F1, а не только сырой F1 при разной трудности heldout. Обычный prefix split того же frozen corpus можно оставить парным контролем. Сейчас этот протокол не реализован и не исполнялся.

Конкретный дальнейший первичный след: внешний invocation/console receipt с `run_id=exp35-e01-e53-tier4-20260507-190227` и интервалом `2026-05-07T19:02:27Z–19:02:47Z`, который мог бы связать `project_root`, загруженную DLL/конфигурацию и manifest. Путь к такому журналу пока не найден; нельзя выдумать его по имени ZIP. Сохранённый Release SourceLink `d97fe37…` — узкий кандидат для последующего сопоставления, не замена receipt. Общий поиск исторически успешных механизмов продолжается по решению parent.

Адресно перечитаны local AGENTS1–17, core AGENTS1590–1610/CLAUDE1029–1042 и фрагменты skills: architect340–346, planning305–313, plan-review674–684, generated-code-review75–84, prompt26–34, readability26–35, human-readable-architecture100–113, conversation-intent1–35. Все эти источники доступны; это **targeted чтение, не полный reload**. Прочитан65; его разбор loader/runner/writer и companion002 не заявляется повторно проведённым. Запусков эксперимента или проверки исторического корпуса не было.
