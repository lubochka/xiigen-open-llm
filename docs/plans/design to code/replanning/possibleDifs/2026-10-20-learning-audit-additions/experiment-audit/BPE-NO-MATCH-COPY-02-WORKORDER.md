# BPE-NO-MATCH-COPY-02 — новая короткая попытка существующего опыта BPE

**Приоритет позднего источника:** core `65d03a73` уже назначил `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BPE01`. Настоящий документ сохранён как запасной рецепт отдельной попытки bp02; он не заменяет BPE01 и не поручает новый запуск. Использовать его можно только когда пользователь явно назначил именно новую попытку bp02. При отсутствии такого назначения применять [позднее дополнение](LATE-CORE-65d03a73.md), без копирования, подготовки или обучения здесь.

**Статус: документальный рецепт; запуск не выполнен.** Это новая техническая попытка существующей гипотезы, не новая научная гипотеза и не продолжение старого result-каталога. Native learner, данные, фактор A/B и бюджет сохраняются. Внутренний ID неизменного пакета — `BPE-NO-MATCH-COPY-01`; ID новой попытки/каталога — `bp02`. В отчёте указать оба.

Пользователь сообщил, что прежний запуск упал до обучения из-за Windows-пути и оставил `build.stdout`/`status.json`. Это пользовательское свидетельство: сырые файлы того сбоя здесь не читались, причина и 0.6 секунды не выдаются за воспроизведённое измерение. Результат обучения по этому запуску отсутствует.

## Что именно разблокируется

[run.py:15–18,29–40](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold/run.py#L15) определяет package-root по собственному местоположению, пишет bin/obj внутрь него и принимает отдельный `--results-root`. Поэтому подготовленный пакет можно побитово скопировать в короткий новый корень. Исследуемый код, Program и manifests не меняются.

| Назначение | Заданный путь |
|---|---|
| Подготовленный источник | `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold` |
| Новая копия пакета | `C:/xg/bp02` |
| Новый результат | `C:/xg/r/bp02` |
| Скрипт ниже | `C:/xg/Run-bp02.ps1` |
| Отчёт | `C:/xg/r/bp02/report.md` |

По прописанным путям вычислена длина примера `experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/obj/Release/net9.0/.NETCoreApp,Version=v9.0.AssemblyAttributes.cs`: 276 символов из исходного helper-root, 200 из root прежней [core-карточки ab583032](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/BPE-NO-MATCH-COPY-01.md#L93), 123 из `C:/xg/bp02`. Строка [ArtifactRoot](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/BPE-NO-MATCH-COPY-01/Runner.csproj#L5) до нормализации включает `hostdir/../../../../`: соответствующие длины 348, 272, 195. Это арифметика строк, **не доказательство конкретного пути из упавшего Windows-процесса**. Windows-копирование и сборка в данном аудите не проверены.

## Гипотеза и связь с обучением

В оригинальном `01.Tokenizer/BpeTokenizer.cs::MergeChunk` создаётся новый список даже для chunk без выбранной пары. В экспериментальной ветви B список создаётся при первом merge; при отсутствии совпадений возвращается исходный локальный список. A сохраняет прежнее тело. Гипотеза: такая замена снижает стоимость настоящего `BpeTokenizer.Train` при том же выученном словаре/merge table и тех же Encode/Decode.

Производственный путь указан в [исходной карточке:9–15](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/cards/BPE-NO-MATCH-COPY-01.md#L9): `KnnLogicalUnit.Train → BpeTokenizer.Train → Encode учебных примеров → HnswIndex.Add`. Локальный опыт измеряет настоящий BPE TRAIN и чтение его результата. Он не измеряет полный KNN training time, качество переводчика, смысловую правильность C# объяснений или нейронное обобщение.

Не меняются UTF-8, порядок корпуса, частоты, tie-break, merge-rank, vocabulary320, RegexSplitter, EOS и downstream KNN. Происхождение готовой копии — core `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`; это не обещание совпадения произвольного будущего HEAD. Будущий target при принятии кандидата — оригинальный `MergeChunk`, перенос lazy-allocation ветви с сохранением левого неперекрывающегося прохода.

## Точные данные и начальное состояние

[Program.cs:85–105](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold/experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/Program.cs#L85) уже содержит следующий материал. Пять TRAIN строк повторены восемь раз в порядке T1…T5; разделитель LF, заключительного LF нет. Это один корпус, а не восемь запусков.

**Документация C#, TRAIN:**

```text
int Add(int a, int b) => a + b; // return the sum
int Subtract(int a, int b) => a - b; // return the difference
int Multiply(int a, int b) => a * b; // return the product
string Echo(string value) => value; // return the same text
// identifiers: alpha beta gamma delta aaaa aaaaa aaaaaa
```

**Документация C#, EXAM:**

```text
int Add(int left, int right) => left + right; // return the sum
int Subtract(int left, int right) => left - right; // return the difference
aaaa aaaaa aaaaaa
// value: Ω + ё
```

**Материалы EN→RU, TRAIN:**

```text
Dana opened the door. => Дана открыла дверь.
Dana did not open the door. => Дана не открыла дверь.
Mira closed the window. => Мира закрыла окно.
Mira did not close the window. => Мира не закрыла окно.
book books door doors окно окна аааа ааааа
```

**Материалы EN→RU, EXAM:**

```text
Nora opened the window. => Нора открыла окно.
Nora did not open the window. => Нора не открыла окно.
аааа ааааа aaaa aaaaa
Ёж увидел окно; Ω.
```

На домен — два свежих процесса A/B. В каждом `Train("",256)` создаёт byte-vocabulary; четыре EXAM читаются до обучения; затем один `Train(corpus,320)`; после него те же четыре EXAM. Модель агента — текущая по умолчанию. Обучаемый объект — native BPE; внешнего teacher, нейронного Student и checkpoint-файла нет. Начальный state задаёт указанный вызов, его hash сохраняет Program.

## Фиксированная подготовка и команда

Сохранить следующий текст как `C:/xg/Run-bp02.ps1`. Этот документ сам скрипт не создает и не выполняет. Используются уже назначенные в исходной карточке `C:/Python314/python.exe` и `C:/Program Files/dotnet`. Нужна только новая копия **девяти перечисленных файлов**; bin/obj, результаты и prepare.py не копируются. Готовые source-копии уже присутствуют.

```powershell
$ErrorActionPreference = 'Stop'
$bpSource = 'C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold'
$bpPackage = 'C:/xg/bp02'
$bpResults = 'C:/xg/r/bp02'
$bpFiles = @(
    'run.py'
    'compare.py'
    'experiment.json'
    '35.ExperimentsV2/Experiments/Rp01Micro/BPE-NO-MATCH-COPY-01/Runner.csproj'
    'experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/Program.cs'
    'experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/source-copy-manifest.json'
    'experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/one-factor.diff'
    'experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/src/01.Tokenizer/BpeTokenizer.cs'
    'experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/src/01.Tokenizer/RegexSplitter.cs'
)
if (Test-Path -LiteralPath $bpPackage) {
    throw "Attempt bp02 package exists: $bpPackage. Preserve it; no automatic retry."
}
if (Test-Path -LiteralPath $bpResults) {
    throw "Attempt bp02 results exist: $bpResults. Preserve them; no automatic retry."
}
foreach ($bpRelative in $bpFiles) {
    $bpInput = Join-Path $bpSource $bpRelative
    if (-not (Test-Path -LiteralPath $bpInput -PathType Leaf)) {
        throw "Required prepared input unavailable: $bpInput. Training not started."
    }
}
foreach ($bpRelative in $bpFiles) {
    $bpInput = Join-Path $bpSource $bpRelative
    $bpTarget = Join-Path $bpPackage $bpRelative
    New-Item -ItemType Directory -Path (Split-Path -Parent $bpTarget) -Force | Out-Null
    Copy-Item -LiteralPath $bpInput -Destination $bpTarget
    if ((Get-FileHash -LiteralPath $bpInput -Algorithm SHA256).Hash -ne
        (Get-FileHash -LiteralPath $bpTarget -Algorithm SHA256).Hash) {
        throw "Prepared copy differs: $bpRelative. Preserve attempt; do not launch."
    }
}
$env:PATH = 'C:/Program Files/dotnet;' + $env:PATH
& 'C:/Python314/python.exe' (Join-Path $bpPackage 'run.py') --results-root $bpResults
if ($LASTEXITCODE -ne 0) {
    throw "Attempt bp02 incomplete, exit $LASTEXITCODE. Preserve logs and status; no retry."
}
& 'C:/Python314/python.exe' (Join-Path $bpPackage 'compare.py') --results-root $bpResults
if ($LASTEXITCODE -ne 0) {
    throw "Comparison incomplete, exit $LASTEXITCODE. Preserve raw; no new training."
}
```

Единственная команда вызова:

```powershell
powershell.exe -NoProfile -File C:/xg/Run-bp02.ps1
```

Один раз сверить копии необходимо для сохранения одного фактора; повторная сверка hashes ради отчёта не нужна. Автоматические manifest-проверки runner не обходить. Существующие bp02 package/result либо недоступный исходный файл означают точный технический остаток этой попытки, без удаления, автоматического bp03, другого SDK или имитации. Если чтение длинного **исходного** пути недоступно оболочке, сохранить имя файла/этап; не менять алгоритм. При build/run failure завершить эту попытку с имеющимися logs/status, без выдуманной accuracy и без нового обучения ради отчёта.

## Бюджет и наблюдения

| Операция | На процесс | Всего |
|---|---:|---:|
| Fresh OS-процесс | 1 | 4: A/code-doc, B/code-doc, A/en-ru, B/en-ru |
| Инициализация byte vocabulary | 1 | 4 |
| Содержательный Train | 1 | 4 |
| EXAM Encode/Decode до/после | 4 + 4 | 32 |
| Максимум learned merges | 64 | 256 |
| Warmup, дополнительные A/B, reviewer rerun | 0 | 0 |

Одна сборка малого Runner.csproj, timeout120с; вся очередь процессов до120с, один процесс до60с — как в неизменённом [run.py](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold/run.py#L35). Нет сборки всего проекта, тестов, CI или дополнительного второго порядка B→A.

Скрытого warmup/повторов в прочитанном [Program:21–46](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold/experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/Program.cs#L21) нет. Инициализация и before-readout исполняются **до** измеряемого Train и входят в объявленный бюджет. Они исключены из TRAIN wall/allocation, но входят в whole-process peak; такое включение уже подписано в [compare.py:34–37](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold/compare.py#L34). Это вывод из кода, не измеренные расходы.

| Показатель | Сравнение | Граница |
|---|---|---|
| Allocated bytes текущего потока, elapsed, CPU | B−A за одинаковый содержательный Train | Один замер, без заявления устойчивого speedup. |
| Process peak working set | B−A по свежим процессам | Startup и before-readout включены; не чистый пик BPE. |
| Состояние обучения | Vocabulary256→фактическое≤320, learned merges и state hash | Подтверждение сработавшего writer. |
| Представление EXAM | A0/A1/B0/B1, IDs/count/Decode, round-trip | Это tokenizer-показатели, не точность переводчика. |
| Эквивалентность оптимизации | Сопоставить state и outputs A1/B1 | Расхождение показать рядом с ресурсным эффектом. |

Для сопоставимости нужны одинаковые corpus, EXAM, vocabulary target и исходный state. Любое измеренное улучшение полезного показателя — успех по показателю/домену; рост точности не обязателен. Все ухудшения рядом. Неизмеренное обозначается словами. Если учебный writer не достигнут или плечи выполнили разную работу, время не объявляется ускорением того же обучения.

## Отчёт и кандидат — без нового прогона

Прочитать только сохранённые `A/code-doc.json`, `B/code-doc.json`, `A/en-ru.json`, `B/en-ru.json`, `execution.json`, `comparison.json`. Создать `C:/xg/r/bp02/report.md`: до350 слов пояснений плюс таблицы и ссылки.

1. Исходный ID и попытка bp02; домены; фактическая модель агента и reviewer либо «не выполнен».
2. Единственный фактор, original MergeChunk, одинаковые данные и исходные состояния.
3. Фактические стадии и показатели A0/A1/B0/B1 обоих доменов. Ожидаемые 8 Train и32 readouts не выдавать за измеренные счетчики, если они выведены только из кода.
4. Успехи по осям, ухудшения, state/output identity, ограничение одного наблюдения.
5. Кандидат: перенести lazy-allocation no-match ветвь в оригинальный `01.Tokenizer/BpeTokenizer.cs::MergeChunk`. Основной файл остается неизменным; внедрение решает Люба.

При текущем документном поручении core `required-changes.md` защищён. Фактический результат регистрируется кандидатом в разрешённом possibleDifs со ссылкой на report, когда такой результат существует. Заранее выдумывать выигрыш нельзя. Предыдущий failed-run и все его receipts сохраняются.

## Готовый промпт и обязательный переход

> Выполни только явно назначенную попытку bp02 существующей гипотезы BPE-NO-MATCH-COPY-01. Модель — текущая по умолчанию, без поиска Astra/другой модели и смены reasoning; ученик здесь native BPE, teacher нет. Используй только перечисленные девять файлов prepared scaffold, C:/xg/bp02, C:/xg/r/bp02, данный PowerShell и неизменные run.py/compare.py. Не вызывай prepare.py, не копируй bin/obj и не очищай старые результаты. Один build, четыре свежих процесса; ни warmup, ни B→A, ни reviewer rerun. На русском запиши один report из raw. Любая измеренная полезная ось — успех без обязательной accuracy, все ухудшения рядом. Основной код не меняй; результат — кандидат для решения Любы.
>
> **Запрещено продолжать предыдущий эксперимент после команды пользователя перейти к следующему.** Отчёт, commit, поиск формата, исправление старого опыта, память агента и «последняя проверка» не оправдывают задержку. **По команде остановить/закончить немедленно прекрати работу**: допускается прекращение работающего процесса и краткое сообщение фактического состояния, без новых проверок/записей. После compaction действует последняя команда пользователя. Существующий незавершенный опыт не перезаписывай и не повторяй автоматически.

Скилы ограниченного задания: `C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`; термины — `human-communication-GLOSSARY.md` в той же папке. Применять их к механизму, одному фактору, конкретному рецепту, проверке копии и читаемому результату. Старые требования full CI, fixed model, автоматического внедрения или повторного обхода ради оформления не действуют.

## Первичные источники

Полностью прочитаны [run.py](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold/run.py#L1), [compare.py](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold/compare.py#L1), [Program.cs](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold/experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/Program.cs#L1), [Runner.csproj](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/BPE-NO-MATCH-COPY-01/Runner.csproj#L1) и исходная core-карточка. Адресно прочитана [экспериментальная ветвь tokenizer](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/bpe-copy/scaffold/experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/src/01.Tokenizer/BpeTokenizer.cs#L23) и native Train/MergeChunk.

Ни одна команда этого документа в ходе аудита не запускалась.

