# BPE-WEIGHTED-CHUNKS-01: спецификация изолированной копии исходного C#

**DESIGNED_NOT_RUN.** Этот документ задаёт будущие файлы и поведение; ни один перечисленный C# host/runner пока не создан. Содержимое двух production source files не изменено. Рабочая стадия сейчас — только документирование. Карточка: [BPE-WEIGHTED-CHUNKS-01.md](BPE-WEIGHTED-CHUNKS-01.md).

## Исходный проект и точные границы

Источник: `lubochka/llm_mvp_core@ab583032562d6656de9dd5ca6fa0a325fd4b1f30`, проект `01.Tokenizer/Tokenizer.csproj`, namespace `Tokenizer`.

Пакет будущей реализации — `C:/xg/bw01`. Копируются **только** `01.Tokenizer/BpeTokenizer.cs` и `01.Tokenizer/RegexSplitter.cs` с этого commit; copyright/license headers сохраняются. Исходный checkout `C:/Projects/llm_mvp_core` не редактируется и не переключается. Извлечение — `git show <SHA>:<path>` из существующего checkout в новые файлы короткого пакета; при отсутствии объекта записать `PREPARATION_FAILED_SOURCE_OBJECT_MISSING`, без поиска другой ветки.

| Будущий файл | Содержание |
|---|---|
| `Runner.csproj` | .NET 9 console, имя `BpeWeightedRunner`, implicit usings и nullable; default compile items выключены; только указанные ниже Compile Include. NuGet и ProjectReference на продукт не нужны. |
| `Program.cs` | Только fixture/arm CLI, finite dispatch, native calls и raw measurement. Не реализует второй BPE. |
| `common/RegexSplitter.cs` | Одна неизменённая копия RegexSplitter. |
| `A/BpeTokenizer.cs` | Исходный tokenizer плюс одинаковые в A/B локальные диагностические counters. |
| `B/BpeTokenizer.cs` | Та же копия плюс единственная описанная ниже свёртка повторных чанков. |
| `in/fixture.json` | Байт-в-байт копия `fixtures/bpe-weighted-chunks.json` из этого пакета. |
| `source-copy-manifest.json` | Repo/commit, исходные Git blob IDs, hashes A/B копий, однофакторный diff, версия .NET; формируется один раз при материализации. |
| `r/0001/` | Создаётся эксклюзивно при будущем запуске; `intent.json`, `status.json`, `A-code-doc.json`, `B-code-doc.json`, `A-en-ru.json`, `B-en-ru.json`, stderr/stdout, `comparison.json`, русский `report.md`, при успехе `candidate.md`. |

`Variant=A` выбирает только A/BpeTokenizer.cs, `Variant=B` только B/BpeTokenizer.cs. Нельзя компилировать оба класса вместе, source-link оригинал или подменять namespace. Host задаёт абсолютный `OutputPath=C:/xg/bw01/bin/$(Variant)/Release/net9.0/`, `AppendTargetFrameworkToOutputPath=false` и `AppendRuntimeIdentifierToOutputPath=false`; иначе SDK добавит второй net9.0 к пути из команд. RuntimeIdentifier должен быть пустым, Configuration — Release; другой RID/Variant/Configuration даёт отказ подготовки. Абсолютные `BaseIntermediateOutputPath` и `MSBuildProjectExtensionsPath` равны `C:/xg/bw01/obj/$(Variant)/$(MSBuildProjectName)/` и задаются до SDK imports. Отключить ImportDirectoryBuildProps/ImportDirectoryBuildTargets, чтобы settings родительского каталога не меняли этот изолированный проект. Так A/B не делят generated files.

## Однофакторная patch specification

1. Внутри tokenizer добавить приватный carrier `WeightedChunk`: исходная строка используется только как key при первой свёртке; token IDs, multiplicity и first occurrence ordinal — локальные данные одного Train.
2. В B/Train при исходном проходе `RegexSplitter.Split(text)` вести ordinal. Первый случай строки создаёт запись и получает UTF-8 byte IDs; последующий случай **той же строки по Ordinal equality** увеличивает её multiplicity. Отдельный список представителей сохраняет порядок первых появлений. Dictionary enumeration не использовать для выбора merge.
3. У B/CountPairs заменить вклад +1 на +Multiplicity при **каждой** соседней позиции representative token list. Порядок прохода представителей и позиций исходный. Хранить ту же int arithmetic и Dictionary<(int,int),int>; у этого fixture диапазон мал.
4. У B/MergeAll вызвать **неизменённый** исходный MergeChunk для каждого representative и заменить только его TokenIds; сохранить weight/order. Нельзя объединять разные строки после merge по одному hash.
5. `_merges`, `_vocab`, `nextToken`, `MaxBy`, stop conditions и дальнейшие Encode/Save/Load не изменяются. Не включать lazy allocation при no-match: это уже другой кандидат.
6. TRAIN-only counters одинаково наблюдают total input chunks, distinct chunks, original occurrence sum, count of processed adjacent positions и MergeChunk calls. Счётчики не меняют частоты, порядок, решения, objective или target vocab и выключены до/после основной Train phase.

Аргумент ожидаемой эквивалентности: одинаковые исходные чанки имеют одинаковые byte IDs; после одного и того же merge все их экземпляры остаются одинаковыми. Суммирование одного и того же вклада n раз равняется умножению этого вклада на n в используемом малом диапазоне. Первое появление любой пары приходится на первого представителя какого-либо чанка, поэтому first-occurrence order сохраняет MaxBy tie. Это математическое ожидание, не выполненная native проверка.

Контрпример неправильной реализации: unweighted dedup удалит частотный вес, сортировка представителей сдвинет ties, non-overlap CountPairs изменит число пар в aaaa. Такой B сравнивает другой алгоритм и не признаётся этой оптимизацией.

## Конечный CLI и append-only попытка

Публичный CLI будущего runner:

```text
run-pair --peer <fixed B assembly path> --fixture <fixed fixture path> --attempt 0001
run-arm --arm A|B --domain code-doc|en-ru --fixture <same fixture> --output <one new JSON>
```

Реальный `run-pair` один раз создаёт `r/0001` с FileMode.CreateNew intent, связывает source/factor/fixture/initial profile и четыре arm records. Затем последовательность строго A/code-doc → B/code-doc → A/en-ru → B/en-ru. Каждый обучающий child process запускается один раз; controller не вызывает Train. Отдельный B assembly обязателен; A не переключает treatment после получения данных.

Если `r/0001` либо любой его arm record существует, код возвращает `ATTEMPT_EXISTS_NO_REPLAY`; не удаляет, не изменяет номер и не ищет старые каталоги. Неизвестный эффект прерванного Train сохраняется как `UNKNOWN`. Запрещено автоматическое повторение при timeout, перекомпиляции, исключении или частичном отчёте. Новый attempt — только в отдельном прямом поручении.

Корпус формируется строго из fixture recipe. JSON parsing — до измерения. A0/B0 — исходный byte vocabulary; четыре EXAM before; один Train; снять TRAIN counters; четыре EXAM after. В raw сравнить train corpus bytes/hash, исходный state, vocabulary target, actual state/output. Сравнение JSON не делает новых Encode/Train.

## Точные будущие команды

```powershell
$ErrorActionPreference = 'Stop'
dotnet build C:/xg/bw01/Runner.csproj -c Release -p:Variant=A
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build A; training not started; no retry' }
dotnet build C:/xg/bw01/Runner.csproj -c Release -p:Variant=B
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build B; training not started; no retry' }
dotnet C:/xg/bw01/bin/A/Release/net9.0/BpeWeightedRunner.dll run-pair --peer C:/xg/bw01/bin/B/Release/net9.0/BpeWeightedRunner.dll --fixture C:/xg/bw01/in/fixture.json --attempt 0001
if ($LASTEXITCODE -ne 0) { throw 'ATTEMPT_FAILED: preserve partial artifacts; no retry' }
```

До наличия host это спецификация интерфейса, **не готовность к запуску**. Если .NET 9 SDK либо исходные объекты недоступны, записать реальную ошибку подготовки; не ставить SDK, не искать иной host, не запускать весь solution и не менять рабочую ветку. Команды публикации отсутствуют.

Использование короткого `C:/xg/bw01` устраняет намеренное повторение длинного Windows пути прежнего BPE-NO-MATCH-COPY-01. Это выбор места нового опыта; старый не переносится, не удаляется и не возобновляется.

