# Промпт исполнителю: BPE-WEIGHTED-CHUNKS-01

**Текущая стадия — DESIGNED_NOT_RUN / READ_ONLY_RESEARCH.** Сейчас поручены документы. Не создавай runner/source-copy и не запускай build/обучение от одного чтения этого prompt. Следующий текст — точное задание для будущего отдельно назначенного выполнения этой карточки. Host ещё не существует; не называй его готовым.

Используй **модель текущей сессии по умолчанию**, сохрани её режим; другую модель не выбирай и не ищи. Последовательный reviewer использует ту же модель по умолчанию и проверяет уже записанные данные без повторного обучения. Обучаемая модель отдельно: native `Tokenizer.BpeTokenizer`, новый byte vocabulary256, затем vocab320; никаких LLM/LoRA/checkpoints у этого опыта нет.

Навыки заданы из `C:/Projects/llm_mvp_core/docs/skills/`: architect-behavior-classifier-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md, prompt-patch-authoring-SKILL.md, human-readable-architecture-audit-SKILL.md, human-communication-GLOSSARY.md. Уже прочитанное неизменившееся не обходи заново. Дополнительные skills/report templates не ищи.

Прочитай [карточку](../BPE-WEIGHTED-CHUNKS-01.md), [точную source-copy specification](../bpe-source-copy-design.md) и [literal fixture](../fixtures/bpe-weighted-chunks.json). Они уже выбирают путь, алгоритм, данные и бюджет. Источник — `lubochka/llm_mvp_core@ab583032562d6656de9dd5ca6fa0a325fd4b1f30`. Из исходного проекта 01.Tokenizer используй только точные BpeTokenizer.cs и RegexSplitter.cs; оригинал и `docs/plans/replanning-possible-diffs` read-only.

Будущее место работы — **C:/xg/bw01**, host **C:/xg/bw01/Runner.csproj**. В A исходный native tokenizer; в B только одна локальная запись на идентичный regex chunk плюс multiplicity и first-occurrence order. При подсчёте пар прибавляй multiplicity за каждую соседнюю позицию; MergeChunk оставь исходным. Не сортируй ties, не нормализуй строки, не удаляй частотный вес, не добавляй BPE-NO-MATCH-COPY-01 и не меняй Encode. Зафиксируй source-copy manifest и один diff при материализации, без последующего hash-полирования отчёта.

Малый бюджет: 2 темы материалов — документация C# и перевод EN→RU; в каждой A/B. В каждом обучающем процессе `Train("",256)`, четыре EXAM before, один `Train(corpus,320)`, четыре EXAM after. TRAIN corpus — ровно 5 строк fixture, блок повторён8 раз, LF между строками без конечного LF. Всего 4 содержательных Train, 4 initialization, 32 EXAM readout. Controller не учится. Никаких warmup/повторов/KNN/нейросети.

До/после сохрани canonical hash, vocab/merge count, actual IDs и Decode. Отмерь реальные Train allocated bytes текущего потока, CPU с Refresh, elapsed, GC, peak working set с честной областью. Счётчики Train закончи до after EXAM. Два arms должны начинать с одинаковых byte vocabulary и входов; learned merge writer обязан сработать. Если получился лишь regex/serialization benchmark или отказ до Train, это не выполненный опыт.

**Любое измеренное улучшение полезного показателя — успех по этой метрике и теме, рост accuracy не обязателен.** Рядом покажи все ухудшения. Уменьшение вызовов функции само по себе не объявляй полезным выигрышем; измеряй выделения/CPU/время реального Train. Разные A1/B1 state/IDs означают DIVERGED, а не эквивалентную оптимизацию. Не подправляй данные и не запускай дополнительные прогоны, чтобы получить положительный итог.

После будущей реализации этих конкретных файлов команды ровно:

```powershell
$ErrorActionPreference = 'Stop'
dotnet build C:/xg/bw01/Runner.csproj -c Release -p:Variant=A
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build A; training not started; no retry' }
dotnet build C:/xg/bw01/Runner.csproj -c Release -p:Variant=B
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build B; training not started; no retry' }
dotnet C:/xg/bw01/bin/A/Release/net9.0/BpeWeightedRunner.dll run-pair --peer C:/xg/bw01/bin/B/Release/net9.0/BpeWeightedRunner.dll --fixture C:/xg/bw01/in/fixture.json --attempt 0001
if ($LASTEXITCODE -ne 0) { throw 'ATTEMPT_FAILED: preserve partial artifacts; no retry' }
```

Это спецификация будущего CLI, **не существующий проверенный runner**. При missing source/SDK/compile error сохрани точную ошибку подготовки и факт «обучение не начато». Не устанавливай зависимости, не ищи другой project и не запускай full solution.

Результаты только **C:/xg/bw01/r/0001**; первый attempt create-new, четыре arm records. Старые каталоги/markers/логи не удалять и не перезаписывать; существующая попытка — ATTEMPT_EXISTS_NO_REPLAY. Interrupted/unknown effect не перезапускать новым RunId. Новый attempt требует отдельного прямого поручения.

Отчёт **на русском**, обычный Markdown: **C:/xg/bw01/r/0001/report.md**. До350 слов пояснений плюс таблицы raw/readout; разделы «Что меняли», «На чём проверяли», «Что получилось», «Выигрыши и ухудшения», «Граница вывода и кандидат исправления». A0/A1/B0/B1, единицы, A/B/B−A за учебный переход; «не измерено» вместо вымышленных нулей. Если есть useful improvement, одна запись **candidate.md** рядом, до120 слов, source method и ссылка на raw. Только Люба решает продуктовый перенос. Продукт, protected-folder, full CI, tests, commit/push и promotion не менять.

**STOP/NEXT — жёсткое управление текущей задачей.** При прямом STOP немедленно прекрати действия: никакого следующего tool call, оформления, памяти или «последней проверки». Допустимо только прекратить уже работающий собственный процесс, если иначе он продолжит обучение; не делать cleanup/readback и не запускать новый. При NEXT прекрати предыдущий опыт и перейди к явно назначенному следующему. Запрещено продолжать старый report, candidates, tests, hashes, запуски либо оправдывать это необходимостью «завершить красиво». Если ID следующего опыта не указан, сообщи известный статус и спроси ID, не возобновляй старую задачу. При обычном завершении один readback только для фактических ошибок/ссылок; никаких дополнительных циклов полировки.

