# BPE-NO-MATCH-COPY-01 — лишние копии chunk в настоящем обучении BPE

## Паспорт и граница результата

**Цель:** проверить, уменьшает ли ленивое создание списка в `MergeChunk` расходы native обучения токенизатора при неизменном выученном словаре и представлении текста. Два заранее заданных предметных профиля: C# документация и EN→RU материалы. Это отдельная P1-гипотеза стоимости обучения токенизатора после устранения P0 блокировок TRAIN; она не выдаётся за уже найденную причину падения accuracy RP01.

Ученика зовут `XIIGEN-native-BPE-320`. Исполняется настоящий `BpeTokenizer.Train`. Neural optimizer steps = 0, KNN index training = 0; связь с существующим обучением Unit доказана ниже. В этой карточке не измеряется качество ответа KNN/нейросети, перевода или C# documentation generation.

Core commit: `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`. Статус: **`SCAFFOLD_MATERIALIZED_NOT_COMPILED_NOT_RUN`**. Подготовлены два canonical C# файла и Program. SDK dotnet в документной сессии отсутствует; сборка/обучение не выполнялись. Старые experiments/results сохраняются; защищённый `core/docs/plans/replanning-possible-diffs` не изменяется.

Правила пользователя: эксперимент должен измерять обучение или доказанную зависимость его результата; до успешного опыта код выносится в эксперимент, а production-изменения решает Люба; никаких продуктовых tests/CI/full chain. **Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. В отчёте рядом показываются изменения остальных показателей, включая ухудшения.**

## Почему это участвует в обучении, а не отдельный string benchmark

[`KnnLogicalUnit.Train`, 132–153](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L132-L153) создаёт tokenizer, учит его на текстах уроков с `vocabSize:320`, затем через тот же tokenizer строит векторы примеров перед `HnswIndex.Add`. Его native TRAIN manifest включает время/allocations этого участка. Следовательно, оптимизируемая функция реально вызывается в обучении существующего logical Unit.

[`BpeTokenizer.Train`, 42–68](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/01.Tokenizer/BpeTokenizer.cs#L42-L68) учит merge table и vocabulary. [`MergeAll/MergeChunk`, 540–565](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/01.Tokenizer/BpeTokenizer.cs#L540-L565) вызываются внутри этого writer. [`Encode/ApplyMerges`, 73–83 и 568–574](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/01.Tokenizer/BpeTokenizer.cs#L568-L574) используют результат обучения. Это реальные state transitions словаря, не имитация Python и не попытка назвать unrelated validation обучением.

Малый опыт выделяет ровно этот native learner из более длинного KNN pipeline. Он достаточен для локального вывода о BPE TRAIN allocations. Чтобы заявить экономию полного KNN training time или изменение KNN accuracy, понадобится отдельное измерение с реальным `KnnLogicalUnit.Train`; данная карточка такого вывода не делает.

## A/B: одна разница

A использует исходное тело `MergeChunk`, которое всегда создаёт `List<int>(ids.Count)` и копирует весь chunk. B ищет пары тем же левосторонним проходом, создаёт список лишь на первом фактическом merge, копирует предшествующий префикс и продолжает исходную логику. Если пары нет, B возвращает исходный список, который callers не меняют. Возврат общей ссылки не превращает её в новую изменяемую глобальную память: chunk остаётся локальным входом последующих чистых merge.

Изменена только экспериментальная копия `01.Tokenizer/BpeTokenizer.cs`; общая ветвь A/B и счётчики находятся в одной сборке, A проходит исходное body. Не меняются RegexSplitter, список/порядок уроков, UTF-8, tie resolution, pair frequencies, merge rank, target vocab, EOS semantics, словарь/IDs и алгоритм KNN. `one-factor.diff` и hashes сохранены.

Возвращённые token IDs и канонический tokenizer hash должны быть одинаковы у A1/B1. Это проверяемое ожидание, а не заранее проставленный pass. Если после обучения state/output отличается, отчёт показывает расхождение и ресурсный эффект рядом; нельзя объявлять его эквивалентной оптимизацией.

## Данные буквально

Каждый из следующих пяти TRAIN рядов повторяется **восемь раз в составе одного корпуса**. Точный порядок — T1,T2,T3,T4,T5,T1…T5; разделитель LF (`\n`) без заключительного LF. Это один вызов `Train(corpus,320)` на домен/плечо, а не восемь запусков/эпох. В host эта конструкция зафиксирована и raw report содержит SHA256 UTF-8 корпуса.

### C# documentation (`code-doc`)

```text
int Add(int a, int b) => a + b; // return the sum
int Subtract(int a, int b) => a - b; // return the difference
int Multiply(int a, int b) => a * b; // return the product
string Echo(string value) => value; // return the same text
// identifiers: alpha beta gamma delta aaaa aaaaa aaaaaa
```

EXAM, ровно четыре строки:

```text
int Add(int left, int right) => left + right; // return the sum
int Subtract(int left, int right) => left - right; // return the difference
aaaa aaaaa aaaaaa
// value: Ω + ё
```

### EN→RU materials (`en-ru`)

```text
Dana opened the door. => Дана открыла дверь.
Dana did not open the door. => Дана не открыла дверь.
Mira closed the window. => Мира закрыла окно.
Mira did not close the window. => Мира не закрыла окно.
book books door doors окно окна аааа ааааа
```

EXAM, ровно четыре строки:

```text
Nora opened the window. => Нора открыла окно.
Nora did not open the window. => Нора не открыла окно.
аааа ааааа aaaa aaaaa
Ёж увидел окно; Ω.
```

Повторяющиеся `aaaa`/`аааа` и нечётные длины затрагивают перекрывающиеся пары; разнотипные чанки создают no-match случаи; `ё`, `Ω` и кириллица выявляют изменение байтов. Это наблюдения через настоящий обученный tokenizer, не дополнительные учебные циклы. Тема EN→RU не делает token round-trip оценкой переводчика.

## Сценарий A0/A1/B0/B1

Для каждого домена запускаются отдельные A и B процессы. В каждом:

1. `new BpeTokenizer(); Train("",256)` создаёт исходный byte vocabulary; ни одной learned merge. Сохранить state hash и EXAM Encode/Decode — A0 или B0. До TRAIN фактор B выключен, общая инициализация одинакова.
2. Включить фактор только для B. Измерить ровно один native `Train(corpus,320)` — фактическое обучение merge table. Копии, пропущенные из-за no-match, посчитать внутри этого вызова.
3. Зафиксировать vocabulary, learned merge count, state hash и такие же EXAM Encode/Decode — A1 или B1. No-match/copy counters за TRAIN зафиксировать **до** послеучебного readout, чтобы Encode не попадал в TRAIN счётчик.

Всего четыре содержательных обучающих перехода, четыре начальных byte-vocab инициализации; не более 256 выученных merges суммарно (64 на переход), четыре процесса и 32 EXAM Encode readouts суммарно. Никаких warmup-обучений, двадцати повторов, локального подбора словаря или запуска нейросети ради доказательства расходов BPE.

## Метрики и интерпретация

| Группа | Метрика и смысл |
|---|---|
| Основная TRAIN cost | allocated bytes текущего native потока за Train; native elapsed; process CPU с Refresh; GC counts |
| Память | peak working set отдельного процесса до конца TRAIN; не выдавать его за только память токенизатора |
| Фактическое обучение | Vocab256 → фактический итог ≤320; learned merges >0; state hash before/after |
| Выученное представление | A0,A1,B0,B1: actual IDs/count и Decode каждого EXAM; exact round-trip; послеобучения canonical hash A1/B1 |
| Контроль одного фактора | Один corpus hash, EXAM strings, target vocabulary и исходный tokenizer state для A/B; native state/output divergence отдельно |

Стоимость B−A сравнивает одинаковый участок фактического обучения. Изменение EXAM token_count/round-trip показывается через `ΔA`, `ΔB`, `ΔB−ΔA`. Данные до обучения не подменяются нулями. EXAM token count — характеристика выученного представления; accuracy документации/перевода и экономия полного Unit training time здесь **не измерены**.

Если same inputs/initial state не подтверждены или learned merge writer не сработал, `compare.py` выдаёт `INVALID_CAUSAL_COMPARISON_DIAGNOSTIC_ONLY`; наблюдённые числа остаются в отчёте без заявления о причинном улучшении. При одинаковых исходных условиях любое полезное измеренное снижение TRAIN allocations/CPU/времени — локальный успех, независимо от accuracy. Все ухудшения, включая рост памяти или altered tokenization, показываются рядом. Маленький wall-time может быть шумным: сообщить предел одного наблюдения, не добавлять автоматически повторные циклы и не отбрасывать измеренную полезную метрику.

## Место и запуск назначены

Рабочая директория: `xiigen-planning/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit`.

Host: `experiments/bpe-copy/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/BPE-NO-MATCH-COPY-01/Runner.csproj`.

Native копии: `experiments/bpe-copy/scaffold/experiments-v2/RP01-MICRO/BPE-NO-MATCH-COPY-01/src/01.Tokenizer/` — только `BpeTokenizer.cs` и `RegexSplitter.cs`. Program рядом с src. Полных ProjectReference и общего dispatcher нет. Источники уже подготовлены.

При поручении на исполнение:

```bash
python3 experiments/bpe-copy/scaffold/run.py --results-root results/BPE-NO-MATCH-COPY-01
python3 experiments/bpe-copy/scaffold/compare.py --results-root results/BPE-NO-MATCH-COPY-01
```

Первая команда собирает ровно этот маленький host, затем вызывает DLL в фиксированном порядке: `A code-doc`, `B code-doc`, `A en-ru`, `B en-ru`. Raw: `results/BPE-NO-MATCH-COPY-01/A/code-doc.json`, `.../B/code-doc.json`, `.../A/en-ru.json`, `.../B/en-ru.json`; рядом execution/build/stdout/stderr и comparison. После сравнения исполнитель добавляет человеческий `report.md` в тот же result root.

При отсутствующем prepared payload единственная команда восстановления копий:

```bash
python3 experiments/bpe-copy/scaffold/prepare.py --core-root C:/Projects/llm_mvp_core
```

Её не выполнять для существующего подготовленного payload. Она читает core без записи и проверяет pinned Git blob SHA, выводит копии только в planning scaffold. При другой ОС заменяется лишь фактический путь read-only core. Куда ставить эксперимент и какие файлы подключать, уже определено; новую библиотеку/проект не искать.

## Бюджет и конфликты

Один targeted build ≤120 секунд. Native execution ≤120 секунд суммарно, ≤60 секунд на процесс. После failure/timeout сохраняется partial evidence, автоматических повторов нет. Ошибка SDK или сборки не разрешает решать весь solution, чинить production tests или выполнять чужую карточку.

При уже существующем result root старые данные не перезаписывать: читать их и назвать конфликт, без удаления каталога или самовольного нового ID. При существующем prepared source manifest использовать его, не запускать prepare снова. При изменившемся фактически подключённом source выдать точное имя mismatch; чужие Markdown changes не создают новый per-HEAD approval gate.

Указания исторических карточек писать в защищённую core-папку здесь заменены прямым user restriction. Существующие P17/P98/N21/N50 изучены как контекст; не переносить в этот опыт tokenizer refit/capacity/order changes. Если отсутствует native learner, нельзя заменить его Python-моделью и назвать её временем C#.

## Кандидат после результата

Точное место возможного исправления — `01.Tokenizer/BpeTokenizer.cs::MergeChunk`, сохранение native merge order и отсутствие allocation при no-match. Отчёт с actual state hashes и показателями направляется Любе для решения. Успех по allocations не требует роста accuracy; он также не разрешает автоматический production перенос, tests/CI или удаление старого опыта.

## Поздняя сверка соседних обновлений

Пакет учитывает [адресную проверку root, 06-CONCURRENT-UPDATES](../../06-CONCURRENT-UPDATES.md): core `62bcc8f → 7c5718` изменил защищённую документацию, а native source данного фактора сохранился. Новые OCT20-R1, OCT20-R2 и E-OPT-STEP относятся к другим операциям; здесь они не присваиваются и не смешиваются с единственным фактором.

Точный исполнительский промпт: [EXECUTOR-PROMPT.md](EXECUTOR-PROMPT.md).

`run.py` отключает parent `Directory.Build.props/targets` через MSBuild flags, отказывается собирать поверх существующих `bin`/`obj` и записывает отсутствующий SDK/таймаут сборки в `status.json`. Это изолированная сборка текущего host, без очистки прошлых artifacts.

Настройки `bin`, `obj` и NuGet project-extension path задаются в csproj **до** SDK imports; все они находятся в указанном artifact-каталоге, который проверяет preflight. Это уточнение оболочки сборки, не второго учебного фактора.
