# Состояние прежних пробелов после core 6694778

**Текущий документный срез:** core `66947785d1318f7cbbc88118f3156cdc75001e16`. Это ограниченное дополнение к аудиту core `883496bca18113bf37e0afe683c75b05a0075048`; прежние документы сохраняют свой исторический pin. Прочитаны только P54, P78, P89, точная карточка NG01 и относящиеся к результатам/кандидатам фрагменты `required-changes.md`. Для нахождения NG01 прочитан listing каталога cards. Production, исходные protected-файлы и старые результаты не изменялись; сборка, обучение и новые измерения не выполнялись.

## 1. Какие выводы 883 уже нельзя выдавать за текущую проблему

| Вывод на 883 | Состояние на 669 | Доказательство и оставшаяся граница |
| --- | --- | --- |
| P54 одновременно допускает learned reader и запрещает все семь подслучаев без нового update | **Конфликт устранён в protected P54** | Новая заменяющая формулировка разрешает сравнение reader на одном подтверждённом learned checkpoint без нового Train. Все P54.1–.7 теперь `NEEDS_INPUT_FOR_RECIPE`; старый нулевой `OUT_OF_LEARNING_SCOPE` отменён отдельно в каждой строке.[p54-top][p54-subcases] |
| NG01 уже есть в planning, но точный int-relational подслучай отсутствует в protected | **Пробел присутствия exact-карточки устранён** | Protected `NG01-null-guard-label.md` существует и задаёт именно этот фактор с nullable-int/type/null controls. Статус `DESIGNED_NOT_RUN`; это известный N48-01 subcase, без присвоения новой семьи. Повторно создавать или переносить второй NG01 не надо.[ng-status][ng-factor] |
| NG01 использует старое planning-размещение `C:/xg` | **Активное назначение пути заменено внутренней карточкой** | Явно назначены `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01`, один `Run.ps1`, ранние claims и `attempt-0001`. Старый путь запрещён именно этой версией. Старую mapping-строку сохранить как историю, активную заменить явным supersede из раздела2 ниже.[ng-path][ng-run] |
| N62-R1 ещё надо добавить как успешного кандидата с оговорками | **Требуемая запись уже есть в current required-changes** | R1 содержит полезную allocation-дельту, GC-регрессии, отсутствие final TRAIN quality и запрет повтора. Там же уже записан следующий R2-кандидат с ограничением historical binary custody.[req-r1][req-r2] |

**Что не исчезло:** снятие ошибочного запрета P54 не создаёт отсутствующие runtime inputs. Карточка прямо не назначает host/project/commands и требует конкретные checkpoint contents, tokenizer contract, запросы без exact-map перехвата, а для соответствующих подслучаев — разрешённые mandatory/semantic contracts. Новое уточнение отдельно предупреждает: `ExecutorUnit.MeasureAccuracy` создаёт новый tokenizer, поэтому batch metric не доказывает применение импортированных tokenizer corrections или полный restore/Flow. Это named input dependencies, а не запрет исследовать результаты обучения.[p54-top][p54-inputs]

**E-METRIC-PHASE не появился как exact контраст в проверенных P78/P89.** P78.1 по-прежнему сравнивает `window1→5` при `max5`; P89.2 — reported weighted loss и normalized unscaled loss при прежних gradients. Это не сравнение pre-update и post-update значения одной функции при одинаковых window/threshold. Общие действующие правила этих карточек разрешают learning-dependent readout и успех любой полезной метрики, но не добавляют отсутствующий temporal-phase фактор.[p78-factor][p89-factor][p-policy] Проверка ограничена указанными файлами: наличие нового отдельного phase-рецепта где-либо ещё в core 669 здесь не аттестуется. Нельзя на основании этого прохода повторять прежнее глобальное утверждение «весь protected 669 не покрывает phase».

P89.1 сохраняет запрет отдельного frozen-loss replay: без настоящего learner это только аналитический контроль, не обязательный runtime gate для P89.2. Его не следует смешивать с уже отменённым blanket запретом P54 learned-state reader.[p89-factor]

## 2. NG01: точное действующее назначение из карточки 669

Все необходимые поля находятся **в самой карточке**. Ссылка на `../prompts/NG01-executor.md` приведена на строке 5; содержимое prompt дополнительно не понадобилось для этого извлечения. Полная машинная запись, включая все 16 literal samples, — [ng01-core669-output-mapping.json](evidence/ng01-core669-output-mapping.json). Это документирование будущего назначения, не материализация host и не команда начать запуск сейчас.

| Поле | Точное значение | Строки карточки |
| --- | --- | --- |
| SourceCore / read-only source checkout | `C:/Projects/llm_mvp_core`; в literal script имя переменной — `$Repo`, не `SourceCore` | [55,87–100][ng-source] |
| Source pin / path / blob | `0b76ab950d82534f5904005dda096545505ae307`; `12.SemanticCompiler/Prediction/FileActionKnn.cs`; `ef7a88a3c4c2540569b9ce6f204d8e4750875501` | [55 и85–100][ng-source] |
| Host | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01` | [53][ng-path] |
| Project | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/Runner.csproj`; net9.0, Roslyn4.12.0, compile только Program/SourceA/SourceB | [53,61–79][ng-project] |
| Entry | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/Run.ps1` | [406–412][ng-command] |
| Единственная внешняя команда | `pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/Run.ps1` | [409][ng-command] |
| Results | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001` | [53 и362–363][ng-path] |
| Report | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001/report.md` | [438][ng-report] |
| Candidate | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001/candidate.md` | [438,486][ng-report] |
| Внешний claim | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/execution.claim`, `CreateNew` до Prepare/build | [362–370][ng-run] |
| Claims отдельных плеч | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001/training-A.claim` и `training-B.claim` | [195–225][ng-arm]; полный второй путь — `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001/training-B.claim` |
| Raw / status | В том же result directory: `arm-A.json`, `arm-B.json`, `failure-A.json`/`failure-B.json` при ошибке, `events.jsonl`, `status.json`, stage stdout/stderr | [202–218][ng-arm], [372–400][ng-run] |
| Model | Модель текущей сессии по умолчанию, без override; фактическое имя в отчёте. Ученик — native `FileActionKnnPredictor(k=3)`. Внешняя LLM, LoRA, neural optimizer и GPU не участвуют | [13–21][ng-rules] |

Внутренняя версия сохраняет **source pin 0b76**, хотя сама карточка прочитана на 669; заменять его HEAD нельзя. Ровно четыре авторских файла: `Runner.csproj`, `Prepare.ps1`, `Program.cs`, `Run.ps1`; Prepare создаёт SourceA/SourceB и provenance. Run выполняет Prepare→один build→один процесс A→один процесс B. Существующие claim/source/build/result артефакты не перезаписываются, claims сохраняются при любом исходе; Prepare отдельно не запускается.[ng-path][ng-source][ng-run]

### Данные и полный малый бюджет

`Program.cs` содержит literal `Lessons` T01–T12 и `Queries` Q1–Q4. В каждом плече одни и те же 12 TRAIN source: по три материала четырёх семейств; четыре новых текста EXAM принадлежат тем же семействам. Внешнего corpus path нет, подмена поиском другого набора не назначена. Полные строки извлечены в JSON выше; исходные literal arrays — [165–186][ng-data].

| TRAIN IDs | Семейство / EXAM | Gold |
| --- | --- | --- |
| T01–T03 | `unrelated-int`; Q1 `class Q1{int Read(string? item,int limit){if(limit is >3)return item.Length;return 3;}}` | `ADD_NULL_CHECK` |
| T04–T06 | `null-literal`; Q2 `class Q2{int Read(string? item){if(item==null)return 3;return item.Length;}}` | `ADD_METHOD` по действующей policy |
| T07–T09 | `type-pattern`; Q3 `class Q3{int Read(object? item){if(item is string content)return content.Length;return 3;}}` | `ADD_METHOD` по действующей policy |
| T10–T12 | `nullable-int`; Q4 `class Q4{int Read(int? item){if(item is >3)return item.Value;return 3;}}` | `ADD_METHOD` по действующей policy |

Каждое плечо: 12 Label, 12 FeatureVector TRAIN, 4 FeatureVector EXAM с повторным использованием; 1 Train; 4 logical queries до и 4 после, до обучения 4 закрыты guard, после 4 доходят до native Predict. Всего A+B: 24 предъявления TRAIN, 24 Label, 24 TRAIN features, 8 EXAM features, 2 Train, 16 logical queries, 8 guard-blocked, 8 native Predict. Optimizer/Generate/Compile программ/warmup/retry — 0. Единственный build host учитывается отдельно. Лимиты: 120 с Prepare, 120 с build, 30 с на плечо; исполнитель наблюдает эти лимиты, новая параллельная копия не допускается.[ng-budget]

Отчёт — русский Markdown, до 350 слов пояснений плюс таблицы; candidate до 120 слов. Любое измеренное полезное улучшение — успех своего показателя/домена с ухудшениями рядом. Требование улучшить accuracy отсутствует. В источнике также предписана запись в protected `required-changes.md`. **В рамках нашего задания это предписание не действует:** пользователь запретил менять папку; допустимы только experiment outputs и последующее разрешённое документирование в possibleDifs по root05. Это обнаруженный конфликт области записи, разрешённый прямым правилом пользователя. STOP/NEXT имеет приоритет над report/review/hash/retry/Git.[ng-rules][ng-report]

Карточка на строках 7–9 сохраняет завершённые №01–05, называет текущим следующим №06 E-OPT-STEP, а NG01 размещает №32 с последующим №33 KN-ROUTER-LEARNED-KEY-01. Это извлечение её явного назначения; весь новый execution-index здесь отдельно не проверялся. Прямой назначенный ID имеет приоритет; одиночный NG01 не разрешает самостоятельно начинать следующий. Новый host/длина путей, сборка и качество **не проверены runtime**; статус остаётся `DESIGNED_NOT_RUN`.[ng-status][ng-notrun]

## 3. Точность текущей записи кандидатов

Current `required-changes.md` уже сохраняет R1: allocation −90 504 байт, GC0/1/2 по +1, отсутствие измеренной TRAIN target quality и причинного вывода о времени. R2 добавлен как кандидат с allocation −312 328 байт, peak +30 474 240 байт, GC по +1 и неполной historical binary custody. Это здесь **проверка содержимого реестра**, без повторного чтения raw/бинарников или собственной проверки новых измерений. Нельзя поручать их повтор ради добавления уже существующей записи.[req-r1][req-r2]

Исторические 8 уроков translation memory и cold restore в начале того же реестра сохраняются. Сами ссылки на эти successes ещё не удостоверяют специфические map contents и no-exact-interception условия каждого из семи P54; карточка правильно оставляет их точными зависимостями.[req-memory][p54-inputs]

## 4. Pinned blobs и предел readback

Пять файлов получены через `github_fetch_file`; вычисленные Git blob SHA совпали 5/5. [Compact receipt](evidence/core669-gap-source-receipt.json) сохраняет identities полученных файлов.

| Файл | Git blob SHA |
| --- | --- |
| P54-phrase-composition.md | `79ed487a570ddb5ccdd64d4eca8d0d401285f8f5` |
| P78-loss-plateau.md | `f66386fd86cbb168e4fea3e2622d43437b7a2ae6` |
| P89-density-stop.md | `9f64d31f480453e341110bddf3e9dc4bce4c1c87` |
| NG01-null-guard-label.md | `772a0202a7bbb61d54021058e82b55b611f53f3c` |
| required-changes.md | `d60fadbd207194e80e4fa66671bf984538ef4f75` |

Для текущего описания пакета: **«На 883 были P54 admission-конфликт и отсутствие protected NG01; на 669 оба документных пробела устранены. P54 остаётся NEEDS_INPUT_FOR_RECIPE, NG01 — DESIGNED_NOT_RUN. Exact temporal-phase contrast не добавлен в проверенные P78/P89».** Это обновление статуса, не повтор всего аудита и не изменение исторических выводов на прежнем pin.

[p54-top]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L19-L25
[p54-subcases]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L264-L310
[p54-inputs]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md#L31-L105
[p78-factor]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/P78-loss-plateau.md#L131-L144
[p89-factor]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/P89-density-stop.md#L131-L153
[p-policy]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/P89-density-stop.md#L3-L7
[ng-status]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L3-L9
[ng-factor]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L25-L49
[ng-path]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L51-L59
[ng-source]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L55-L100
[ng-project]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L53-L79
[ng-rules]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L13-L21
[ng-data]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L165-L186
[ng-arm]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L195-L225
[ng-run]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L358-L412
[ng-command]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L406-L412
[ng-budget]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L414-L434
[ng-report]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L436-L486
[ng-notrun]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/NG01-null-guard-label.md#L488-L492
[req-r1]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/required-changes.md#L85-L94
[req-r2]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/required-changes.md#L96-L104
[req-memory]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/required-changes.md#L5-L19
