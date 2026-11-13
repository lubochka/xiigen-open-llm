# Сохранённые измерения и кандидаты в исправления

> **Граница источников:** основной текст ниже относится к core `fb9175cc`. Перед публикацией отдельно проверен новый commit `d962f958`: [позднее уточнение](10-late-core-delta-d962f958.md), [актуальный реестр](04-registry.md). Исторические номера/статусы и счётчики 163 cards / 8 prompts не являются повторной аттестацией нового среза. NEXT сейчас — №10 PARENT-ADOPTION-01; старые raw и попытки сохраняются.

**Метод:** заново прочитаны сохранённые JSON на pinned SHA, извлечены значения и выполнена арифметическая сверка. Ни обучение, ни inference, ни повтор scorer, ни сборка здесь не выполнялись. Машинная сверка со ссылками на raw — [saved-metric-reconciliation.json](data/saved-metric-reconciliation.json). Это независимая проверка документа/чисел, не независимая репликация экспериментов.

Любое измеренное улучшение полезного показателя — успех этого показателя и домена. Ниже оно сохраняется кандидатом даже без роста accuracy. Ограничения причинности и ухудшения не скрываются. Ни один кандидат в этом отчёте не переносится в production автоматически.

## 1. Replay: исходный №09 и отдельная N63R-PROBE

[Исходный consumed №09](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/docs/plans/replanning-possible-diffs/experimets/cards/N63-replay-capacity.md) выполнил 9 updates и получил 0 правильных response-токенов и 0 целых ответов до/после. Он сохраняет механическое наблюдение: A07 оставлен и реально включён в учебный sample. Этого недостаточно для retention ранее приобретённого навыка. **Повтор запрещён; статус consumed сохраняется.**

Отдельная [N63R-PROBE raw](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/N63R-PROBE/results/n63r-probe.json) имеет другой бюджет: common1000CODE_Q+400BUG updates; A800, B1200, C1200, всего 4600;40 scoringforwards. Её нельзя называть результатом исходных 9 updates. H32/qv-rank8, тот же начальный WA в A/B/C; после общего обучения 45/92 правильных старых TRAIN-токенов,0/2 целых TRAIN-ответов. Указанные ниже response exact — teacher-forced, не свободная генерация.

| Строка | Response знаменатель | WA до нового материала | A: FIFO | B: reserved replay | C:1200 без replay |
|---|---:|---:|---:|---:|---:|
| A07 | 49 | 22 | 2 | 15 | 4 |
| EO02 | 49 | 15 | 1 | 15 | 3 |
| A01 | 43 | 23 | 2 | 1 | 2 |
| EO01 | 45 | 6 | 1 | 0 | 1 |
| N01 | 26 | 0 | 22 | 18 | 24 |
| N02 | 31 | 0 | 28 | 25 | 28 |
| EN01 | 35 | 1 | 4 | 1 | 2 |
| EN02 | 29 | 0 | 3 | 1 | 1 |

Положительный эффект B виден по старому A07 и близкому EXAM EO02: относительно A сохраняются дополнительные 13/49 и 14/49 правильных токенов. На A01/EO01 защиты нет, новые N01/N02 и их EXAM EN01/EN02 хуже A по этой метрике. **Ни в одном из восьми ответов этих состояний не получен полный teacher-forced exact.** Поэтому корректный кандидат — частичное удержание конкретных выученных токенов с ухудшениями, а не общий успех сохранения навыка или всех доменов.

| Стоимость continuation | A | B | C |
|---|---:|---:|---:|
| Native updates |800|1200|1200|
| Входные tokens |63600|118800|95400|
| Shifted next-token targets |62800|117600|94200|
| Allocated bytes |5863987776|12124886728|8795992464|
| Wall ms |12217.5506|28327.0492|17791.1449|
| CPU ms |10406.25|22125|16484.375|

B/A: updates+50%; inputtokens+86,79%; allocations+106,77%; wall+131,86%. Не «всё+50%» и не «двойное число updates». C уравнивает только updates сB, но не число токенов, состав и порядок материала. Фиксированный A→B→C порядок и отсутствие равного token budget ограничивают причинное сравнение стоимости. Сохраняются фактические наблюдения по каждой оси. Конкретный будущий участок — `16.AutonomousLearning/Training/ReplayBuffer.cs:Add` (строки 32–38) и `Sample` (40–68), сбор фактически передаваемых в native Train replay lessons; исследуемое reserved-slot изменение остаётся экспериментальным до решения пользователя.

## 2. Restore действительно позволяет продолжить обучение; предел результата уже виден

[RESTORE-MINIMAL-SET raw](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json) сохраняет H16 native модель после 1000 SFT updates, загружает её в новый объект и делает 200 дополнительных updates. Модельный tensor hash после load совпадает; optimizerImported=true, step1000→1000→1200. В этом аудите все сохранённые объекты измеренной панели до/после load сравнены целиком и совпали. Это сильнее узкого boolean исходного probe, но не новый runtime restore.

| Показатель TRAIN | После restore, до continuation | После 200 updates | Оценка |
|---|---:|---:|---|
| Response NLL mean |4.766696420187372|4.711598363502413|Успех: −0.055098056684959|
| Mean gold rank |51.825|50.05|Успех: −1.775|
| Правильные response-токены |3/40|4/40|Успех:7,5%→10%|
| Строки хотя бы с одним верным токеном |2/2|2/2|Не полные ответы|
| Полный teacher-forced TRAIN exact |0/2|0/2|Освоение задания не доказано|
| EXAM правильные response-токены |0/43|0/43|Положительного token результата на EXAM нет|

CONTROL response NLL ухудшается 5.8079633717111525→5.857194149707198; EXAM NLL/rank меняются по своим осям и полностью сохранены в JSON сверки. Успех TRAIN NLL/rank/tokens остаётся документным кандидатом. Нельзя называть 2 строки с любым совпавшим токеном двумя правильными ответами или считать этот результат полностью выполненным пользовательским требованием ненулевых **целых** ответов.

Проверены факты import и шага; отдельный post-import export всех m/v с побайтовым сравнением не выполнен. Доступные saved checkpoint pointers указывают на step1000, не 1200. Файлы private weights/optimizer сами по себе не доказывают штатный production parent contract. [Source restore/continuation](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs) и [разделение raw restore/P02.PARENT](03-card-contract-audit.md) задают конкретную границу. Недостающая production provenance не запрещает private native experiment, BPE или learned-memory restore.

## 3. SiLU donor: известный частичный сигнал и точная цена подготовки

[LEARN-SANITY-PROBE raw](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/probe-result.json) использует H64,V256,L1,q/v-rank4 и 400native SFT updates на двух коротких знаковых ответах. TRAIN NLL7.960885496→1.887944600; EXAM8.015334124→2.798859528. TRAIN/EXAM0/2→1/2. Однако конечные ответы TRAIN `+,+`, EXAM `−,−` равны constant baseline1/2: условное правило вычисления знака не усвоено. Это ограниченная ненулевая траектория и реальное обучение, а не достаточное доказательство общего умения.

Program сохраняет JSON, готовый H64 checkpoint этой пробы не найден. Новая [SiLU карточка](cards/SILU-SIGMOID-REUSE-01.md) потому честно назначает собственные 400 подготовительных updates, сохранение S0 внутри нового host, затем 4+4 updates. Нет «бесплатного» восстановленного donor, нет переноса H16/H32 checkpoint в другую topology и нет нового запуска старой пробы. В текущей документационной фазе эти 408 updates не выполняются.

## 4. Настоящее обучение BPE и native SFT: полезные ресурсные кандидаты

| Сохранённый A/B | Полезное изменение | Ухудшения и границы |
|---|---|---|
| BPE-NO-MATCH-COPY, код/документация | Allocations3904664→1126744B, −71,144%; в обоих arms64 learned merges | CPU31,25→46,875ms; скорость/качество downstream learner не измерены |
| BPE-NO-MATCH-COPY, EN/RU материал | Allocations2892824→893656B, −69,108%;64 learned merges | CPU46,875→46,875ms; отдельный domain corpus, не свободный перевод |
| NEURAL-FROZEN-TRANSPOSE,4 SFT updates на arm | Allocations13647840→12140448B, −1507392B/−11,045%; сохранённые losses, logits, before/after, model/optimizer hashes A/B совпадают | Cached payload+249856B; wall84,3133→84,57ms; CPU62,5→78,125ms; полные ответы 0/2→0/2 |

Для BPE заново сравнены `state_after_sha256`, все saved before и after outputs: A/B совпадают на обоих корпусах. Это действительный успех стоимости **обучения tokenizer**. Не нужно объявлять его провалом из-за отсутствия downstream neural accuracy; одновременно downstream accuracy не приписывается.

Первичные A/B JSON и точные поля доступны из [машинной сверки](data/saved-metric-reconciliation.json). Сохранённые N62-R1/R2/E-OPT-STEP/LF-03 resource gains также не отбрасываются: их [предыдущий адресный аудит](https://github.com/lubochka/xiigen-planning/blob/fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-audit-dd150aa5/04-verified-results-and-candidates.md) содержит −90504/−312328/−467944/−646816B allocations с trade-offs. В этом проходе они не пересняты целиком и не названы новым независимым raw review. Для части B binaries прежний аудит установил hash placeholders/несовпадение measured-path; это ограничивает атрибуцию конкретному factor, не стирает сохранённую дельту.

## 5. Обученная переводческая память и зависимый ingress/restore

P125 source scores показывают: A без уроков 0/24→0/24; B с корректными уроками 0/24→19/24, acceptedWrong0→3; corrupted C0/24→13/24, acceptedWrong0→9. Результат зависит от материала. Это настоящее обучение памяти, а не подтверждение neural DPO. В `P25` после холодного восстановления совпадают 24/24outputs, statehash, те же 19 правильных и 3 принятые ошибки; `newTrainingCalls=0`. Здесь отсутствие нового Train допустимо как зависимое восстановление ранее выученного результата; источник такого знания — P125, не пустая fixture.

Ingress `phrase_mappings` даёт 11/24→19/24; composition0/8→8/8; acceptedWrong1→3. Полный corpus all содержит 24 случая в обоих arms. Такой ingress участвует в записи обучаемой фразовой памяти, поэтому относится к обучению по пользовательскому правилу. Данные подтверждают полезный gain и конкретную цену ошибочных принятий; одинаковый итог 19/24 у P125/ingress не является двумя независимыми доказательствами neural способности.

Primary summaries с source links полностью включены в [сверку](data/saved-metric-reconciliation.json); source writer/reader и existing owners — [08](08-knn-history-and-source-audit.md). Успехи E09/E25/E43 и полезный, но необучаемый E26 также сохранены [там](08-knn-history-and-source-audit.md), с разными датами runs, timings и доменной границей. Материалы одного results tree в нескольких ветках не подсчитываются как независимые повторы.

## 6. Документные кандидаты: точный участок будущего исправления

| Кандидат | Что менять в оригинале только после отдельного выбора пользователя | Сохраняемый смысл и риск |
|---|---|---|
| BPE no-match copies | `01.Tokenizer/BpeTokenizer.cs`, `MergeChunk`: вернуть существующий chunk только если pair replacement не произошёл; сохранить ownership и результат Train | Доказанный allocation gain на двух корпусах; CPU code corpus хуже |
| Frozen transpose | `00.TensorCore/Layers/Linear.cs`, `Forward`: кешировать transpose только неизменяемой frozen base, с корректным lifetime/invalidation | Не кешировать изменяемые LoRA A/B; retained cache стоит памяти |
| N63 ingress | Native admission/материализация длины в существующем N63I path: допускать короткие пригодные уроки, deferred-long фиксировать отдельно до mutation | B действительно обучает вместо общего отказа; не обещать full-answer gain |
| Reserved replay | `ReplayBuffer.Add` и actual sampling к native Train: сохранить требуемое старое lesson/domain в буфере и передать его learner | Лучше A07/EO02 tokens; больше работы, хуже новые N01/N02 и отсутствует защита A01 |
| Restore continuation | `LlmModel.SaveCheckpoint/LoadCheckpoint`, `AdamW.ExportPortableState/TryImportPortableState`; отдельно truthful SFT lineage в native parent path | Реальная continuation имеет полезные метрики. Нельзя фальсифицировать DPO provenance ради допуска SFT parent |
| Translation ingress/restore — сохранить уже внесённое | В current `27.TranslationDomain.Tools/Program.cs` reader/row/builder уже передают phrase mappings; не чинить это повторно. P92/P106/P25 — владельцы дальнейших отдельных вопросов |19/24 и композиция полезны; acceptedWrong3 остаются; точные текущие строки в разделе 8 |
| N-KU-01, ранее измеренный кандидат | `12.SemanticCompiler/Prediction/FileActionKnn.cs`, `ActionLabelGenerator.Label`: учесть `DoStatement` в hasLoop, не менять одновременно признаки/k | EXAM1/4→3/4, но потерян единственный нужный ADD_LOOP; сохранённый кандидат не безусловный product fix |

Для N63/translation здесь дан summary исправления; точный актуальный caller и привязка к существующей экспериментальной карточке обязательны в 01/03/07/08. Две новые SiLU/HNSW идеи остаются **гипотезами**, а не строками уже измеренных улучшений. До успешного отдельного опыта изменения production/tests/CI не назначены. Отчёт не разрешает заново запускать завершённые исследования ради недостающей красивой таблицы или более удобного результата.


## 7. Дополненная сверка №08: вход действительно разблокирует обучение

Первичные [arms.json](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/N63I/results/arms.json) и [per-row.json](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/N63I/results/per-row.json) включены в машинную сверку целиком. A передаёт 3 строки; одна имеет 221 token при допустимом 65 и прерывает batch до updates. B заранее принимает только 2≤Encode(prompt+LF+source).Count≤ContextLen+1, передаёт 2 короткие строки и сохраняет train-long как deferred; native guard не ослаблен.

| Измерение | A-full | B-bounded |
|---|---:|---:|
| Actual optimizer updates |0|2|
| Shifted training targets |0|70|
| Native elapsed ms |279.9091|111.3761|
| CPU ms |62.5|93.75|
| Allocated bytes |81992|3330408|
| Изменились веса |Нет|Да|

TRAIN response NLL mean6.595570459138105→6.592119856314251; правильных response-токенов 0/40→0/40, полных ответов 0/2→0/2. По отдельным TRAIN/EXAM/CONTROL rows NLL ниже, но все 5scored rows остаются с 0correct tokens; over-context train-long имеет NOT_SCORED, а не NLL0. Суммарно 20 scoring forwards уже были выполнены исходным опытом. Это сохранённый успех admission/updates и NLL с ростом CPU/allocations; elapsed сравнивает отказ и настоящее обучение, не равную работу. Исходный S0 общий и случайный, не выученный donor. №08 consumed, не повторять.

**Точный оригинальный участок для будущего выбранного исправления:** [IncrementalTrainer.cs:384–420](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/14.ContinuousLearning/Training/IncrementalTrainer.cs#L384-L420), `TrainAsync`: текущий loop токенизирует весь examples и возвращает TRAINING_EXAMPLE_CONTEXT_EXCEEDED при одной длинной строке. Исследованный ремонт — отдельный admission plan до передачи выбранных examples/привязки provenance: принимать короткие пригодные строки, deferred хранить с причиной; сохранять unchanged length guard и связывать source/material hashes только с фактически переданным набором. **Не просто continue внутри loop после привязки provenance.** [Точный экспериментальный caller](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/N63I/Program.cs) уже проверил этот ограниченный путь. Пользовательское решение о production переносе остаётся отдельным.

## 8. Уже исправленная доставка phrase mappings: сохранить, не чинить повторно

На текущем core исправление действительно присутствует: [ReadTranslationCorpusRowsAsync:4442–4471](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/27.TranslationDomain.Tools/Program.cs#L4442-L4471) читает optional phrase_mappings; [BuildTranslationContinueExample:4682–4700](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/27.TranslationDomain.Tools/Program.cs#L4682-L4700) передаёт row.PhraseMappings в ExecutorOutput; поле row имеет этот nullable type [5146–5147](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/27.TranslationDomain.Tools/Program.cs#L5146-L5147). [TranslationMachine:107–112](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/27.TranslationDomain/TranslationMachine.cs#L107-L112) вызывает настоящий TrainOnExample, [Executor writer:453–455](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/27.TranslationDomain/TranslationTrainedUnits.cs#L453-L455) сохраняет части, [consumer:798](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/27.TranslationDomain/TranslationTrainedUnits.cs#L798) использует их для композиции.

Статус этой адресной доставки — **ALREADY_IMPLEMENTED_PRESERVE**. Сохранённые 11/24→19/24 и composition0/8→8/8 объясняют, почему её нельзя потерять при переносе: в обоих входах 16 частей, старый ingress их не доводил, новый доводит. В этом аудите код не меняется и новый опыт не запускается. Предшествующий report уже фиксирует пользовательское принятие trade-off; этот аудит не отменяет его. Потери отрицания/вопроса и acceptedWrong1→3 остаются рядом как факты. Общий RP01 neural adapter использует другой путь; перенос этого успеха на neural DPO не заявляется.
