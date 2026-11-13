# Независимый статический review BPE-REALIZED-MERGE-COUNT-01

20 октября 2026. Проверены новая карточка, отдельный prompt, полные native `BpeTokenizer.cs` / `RegexSplitter.cs` и названный downstream caller на core `883496bca18113bf37e0afe683c75b05a0075048`. Проверяющий — отдельный агент experiment_review, default-модель сессии. Только чтение и проверка документации: host не материализован, C#/PowerShell/.NET не запускались, protected/output не менялись.

**Вердикт:** реальная связь с обучением подтверждена; один фактор, входы и before/after бюджет согласованы. Адресные исправления stream position, CLI build isolation и отказа повторного attempt внесены автором и проверены финальным readback. Оставшихся блокирующих замечаний к этому документальному рецепту не найдено. Сборка и runtime остаются **UNVERIFIED / NOT_RUN**. Измеренного улучшения у этой новой гипотезы ещё нет.

## 1. Native обучение и зависимости

- [Train:42–68](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/01.Tokenizer/BpeTokenizer.cs#L42-L68) действительно выбирает CountPairs→MaxBy и пишет новые `_merges/_vocab`. [Encode:73–82](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/01.Tokenizer/BpeTokenizer.cs#L73-L82) применяет это выученное состояние. Это самостоятельное обучение tokenizer, не внешний scorer.
- [KnnLogicalUnit:136–153](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L136-L153) обучает такой BPE с vocab320 перед записью индекса; [Vectorize:266–279](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L266-L279) использует Encode. Рецепт честно ограничен vocab257/одним merge и не обещает доказать весь downstream KNN или нейронный RP01.
- Внешняя native source dependency BpeTokenizer — RegexSplitter; прочие helpers, `SavedModel`, `MergeEntry`, cancellation stream и `CanonicalTokenizerArtifactLimits` находятся в том же BpeTokenizer.cs. Остальные используемые типы — .NET BCL/System.Text.Json/Regex. Двух полных native файлов достаточно для указанного source closure; продуктовый csproj и NuGet reference не требуются. Полный файл важно не обрезать до Train/Encode.
- Отдельные Variant A/B assemblies с одним выбранным tokenizer source не смешивают factor runtime toggle. Parent props/targets должны отключаться через CLI до SDK imports, а не только поздней property group.

## 2. SelfEdges меняет именно заданный критерий

У одинаковой пары два её экземпляра могут перекрываться только в серии одного token ID. В серии длины L цикл B учитывает self-edges с номерами1,3,5…, то есть floor(L/2), и сбрасывает parity на иной паре и новом chunk. У пары разных IDs перекрытие двух экземпляров самой этой пары невозможно; прежний count сохраняется. Первый экземпляр каждого ключа не пропускается, поэтому insertion order/tie policy не меняются. [Исходный CountPairs:526–538](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/01.Tokenizer/BpeTokenizer.cs#L526-L538) и [MergeChunk:548–565](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/01.Tokenizer/BpeTokenizer.cs#L548-L565) подтверждают этот контраст.

Это предложенный другой objective, не доказанный bug стандартного BPE. Карточка правильно признаёт overlap уже известным из weighted-chunks lineage; новым называется отдельный causal experiment, а не повторное открытие исходного счётчика.

## 3. Fixture, показатели и бюджет

[RegexSplitter:16–21](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/01.Tokenizer/RegexSplitter.cs#L16-L21) отделяет буквы, punctuation и whitespace. Для заданного ASCII fixture source-derived расчёт карточки согласован:

| Величина | До | После A, прогноз | После B, прогноз |
|---|---:|---:|---:|
| TRAIN | 27 | 24 | 23 |
| Три lambda EXAM | 12 | 12 | 9 |
| Comment EXAM | 9 | 5 | 9 |
| Neutral EXAM | 3 | 3 | 3 |
| Вся EXAM | 24 | 20 | 21 |

Это **расчёт из исходника, не native измерение**. Прогноз намеренно содержит общий EXAM regression при локальном lambda gain; его нельзя скрывать или требовать общего выигрыша для признания полезной оси.

Бюджет задан точно: **четыре native Train API calls суммарно = два `Train("",256)` для initialization + два содержательных `Train(corpus,257)`**. Инициализации не создают merges, потому что vocabulary уже256 и условие while false. Содержательное обучение ограничено одним merge на arm. У C нет Train. EXAM: 2arms×2moments×2states×5rows = **40 Encode/Decode пар**; отдельно **4 Encode TRAIN**. Дополнительного KNN fit, DPO/SFT, модели LLM или повторного учебного цикла нет.

Before/after читают те же строки у того же обучаемого объекта и его отдельного no-update control. Для outputs корректно показывать ΔA, ΔB и ΔB−ΔA. Стоимость измеряется вокруг целого содержательного native Train, который сам заново создаёт byte vocabulary; нет выдуманной baseline «стоимости до обучения». Initial byte setup/readouts/Save/hash/JSON находятся вне TRAIN timer и одинаково оплачены в плане. Одна A→B пара не доказывает причинный стабильный speedup.

## 4. Clone и сохранение результата

[Save(Stream):107–120](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/01.Tokenizer/BpeTokenizer.cs#L107-L120) оставляет Position в конце. [Load(Stream):163–183](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/01.Tokenizer/BpeTokenizer.cs#L163-L183) читает с текущей позиции и сам её не перематывает. Правильный literal порядок: `tokenizer.Save(stream); stream.Position=0; var control=BpeTokenizer.Load(stream);`. Это обязательная правка исходного рецепта, иначе clone может упасть до содержательного обучения.

Load создаёт новый tokenizer и самостоятельные dictionaries/decoded byte arrays. Для C не нужен дополнительный Train; его state/IDs должны быть одинаковыми до/после treatment. A0=B0 проверяется по canonical state, inputs/fixture hashes; A1 и B1 имеют право и по прогнозу должны различаться. Здесь нельзя применять правило parity конечных моделей из эквивалентных resource-only оптимизаций.

Actual IDs+Decode и exact round-trip обязательны: нельзя объявлять меньше tokens полезным результатом при утрате текста. Canonical hashes подтверждают состояние, а не полезность сами по себе. No-update condition связан с реальным учебным изменением A/B и потому не является запрещённым посторонним экспериментом.

## 5. Handoff и оставшиеся условия

Точное место `C:/xg/br01`, source pin, source filenames, build variants, result root0001, модель, skills, русский отчёт/candidate, limits и STOP/NEXT назначены. Ничего не направлено на изменение protected. Короткий новый host не повторяет старый длинный scaffold path. Program/Runner.csproj пока представлены контрактом будущей реализации, **не** существующим проверенным executable.

Для ясного handling конфликтов существующий подготовленный package с совпавшим manifest и существующий attempt — разные случаи: первое может быть входом подготовки, второй всегда завершает replay с `ATTEMPT_EXISTS_NO_REPLAY`. Этот guard нужен также **до build** в обеих command fences; controller guard остаётся перед Train. При неизвестном исходе процесса нет нового запуска и нового номера attempt.

Фраза `FACTOR_NOT_DISTINGUISHED` должна означать отсутствие подтверждения другого выбора merge. Она не должна отменять запись любой реально полезной измеренной оси. При INVALID_COMPARISON положительное raw число сохраняется с ограничением, без причинного утверждения.

## 6. Статус адресных исправлений

Все первоначальные замечания **FIXED** в карточке и отдельном prompt; проверено чтением конечного текста:

- Card L109 / prompt L23: `Save(stream); stream.Position = 0; ... Load(stream)` создаёт no-update clone без чтения с конца потока.
- Card L153, L162/L164 / prompt L32/L34: обе команды build передают `-p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false` как global CLI properties до SDK imports.
- Card L155/L161 / prompt L19/L31: любой существующий `r/0001` запрещает replay независимо от manifest и наличия completed; guard стоит до build, controller повторяет его до Train.
- Card L117 / prompt L44: `FACTOR_NOT_DISTINGUISHED` ограничен выбором merge и не отменяет документирование полезного ресурсного наблюдения и остальных ухудшений.
- Card L115 / prompt L25: четыре native Train API calls явно разделены на две byte initialization и две содержательные операции обучения.

Review не запускал команды и не менял protected или готовый output. Статус: **DOCUMENT_REVIEWED / BUILD_AND_RUNTIME_UNVERIFIED**.
