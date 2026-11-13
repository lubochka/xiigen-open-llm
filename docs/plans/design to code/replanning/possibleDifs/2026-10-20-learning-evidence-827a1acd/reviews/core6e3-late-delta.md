# Поздняя дельта core 6e3: завершённый LF-03, исправленные замечания и текущая очередь

**Граница этого review:** core-дельта проверена независимо; расчёт 38/31 ниже относится к пакету до добавления peer-факторов, промежуточное объединение 40/33 — к planningeaed. Финальный объединённый реестр после planningd7 содержит 42 ID, 7 завершённых и 35 будущих; NEXT тот же — N63-INGRESS-LENGTH. См. [текущий реестр](../04-execution-registry.md) и [последнюю сверку](../12-concurrent-planning-d7.md). Исходные выводы review ниже сохраняются как исторический снимок.

Дата: 2026-10-20. Замороженный источник: `llm_mvp_core@6e3e8864933e35fccdb4f21369c9912fa195aaf6`, родитель `827a1acdbef95d220886e54f9d36bce0b405c8cd`. Это дополнение к аудиту 827, а не повторная проверка всего корпуса. Изменения только в `work`; защищённая папка, продукт и старые результаты не изменялись. Эксперименты, сборки, inference и тесты не запускались.

**Действующий итог для planning-пакета: семь завершённых опытов, следующий — №08 `N63-INGRESS-LENGTH`.** Сохранённый `LF-03-01` добавляется как успешный кандидат по отдельным измеренным показателям с ухудшениями и ограничениями рядом. Канонический KN reader observer уже исправлен; добавочный второй observer не требуется. Готовая карта содержит **29 будущих core-пунктов**, без LF-03: [latest6e3-output-path-map.json](../evidence/latest6e3-output-path-map.json). В расширенной программе root из 38 пунктов это означает **7 завершённых и 31 будущий**, при сохранении двух новых гипотез и прежнего порядка.

## 1. Очередь: фактические receipts выше устаревшего NEXT

В [индексе 6e3, строки 5, 29–36][index] по-прежнему написано «№01–06 сохранены; следующий №07 LF-03», а строка LF-03 ещё помечена рецептом. Но [A raw, строки 7–10, 56–57][raw-a] и [B raw, те же строки][raw-b] содержат `COMPLETED`, по одному `issued` и `completed`. В обеих ветках есть `step-000001.issued.json`, `step-000001.completed.json`, начальное состояние, финальные recorded state/readback; [отчёт, строка 5][report] также сообщает завершение. Это достаточное основание считать назначенный опыт выполненным; отсутствие полного качества ответа и пробелы provenance не превращают его в разрешение на повтор.

| Позиция core | ID | Эффективный статус в planning |
|---:|---|---|
| 01 | N-KU-01 | Сохранён, не повторять |
| 02 | NEURAL-FROZEN-TRANSPOSE-01 | Сохранён, не повторять |
| 03 | BPE-NO-MATCH-COPY-01 | Сохранён, не повторять |
| 04 | N62-R1 | Сохранён, не повторять |
| 05 | N62-R2 | Сохранён, не повторять |
| 06 | E-OPT-STEP | Сохранён, учитывать новый erratum |
| 07 | LF-03; имя сохранённого результата LF-03-01 | **Сохранён, завершён; не повторять и не дополнять новым TRAIN readout** |
| 08 | N63-INGRESS-LENGTH | **Следующий документируемый/будущий назначенный пункт; host по индексу ещё не реализован** |

Оставшийся core-порядок №08–36 не менялся. ROPE31 → AST32 → NG0133 → BPE-REALIZED34 → IDENTITY35 → KN36; SKNN остаётся alias AST. В программе planning из 38 пунктов новые ATTN/JSON остаются после core34, до IDENTITY/KN. Никакая из этих записей не запускает программу в текущей документной сессии. Старые слова «следующий LF-03» в протоколе, N62 и NG01 являются устаревшим статусом снимка; они не имеют приоритета над завершёнными receipts.

**Исторический result root LF-03-01:** `docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01`. Его не переносить и не переписывать. Прежний prospective `experiments-v2/RP01-MICRO/LF-03/results` не является свободным назначением второго опыта. Сохранённый фактический runner [Run-Lf03.ps1, строки 4–5, 13–27][run-lf] указывает исторический root; будущий `Run-With-Learning-Signal.ps1` из старой карточки не запускать ради согласования документов.

## 2. LF-03-01: что реально измерено

### Фактор и учебная связь

В текущем сохранённом A/B коде изменён только внешний `ToArray()` в `Tensor.Backward(Tensor)`: [A, строка 99][tensor-a] создаёт `new Tensor(initialGradient.Data.ToArray(), initialGradient.Shape)`, [B, строки 99–103][tensor-b] передаёт `initialGradient.Data`; защитная копия `TensorStorage` остаётся. `Sources.TensorCore.props` [строки 3–15][tensor-props] использует common TensorCore и `$(Variant)/Core/Tensor.cs`. Два DpoTrainer имеют одинаковый Git blob `0a7fd4d088fd37824aa213ddf0c6189675fe7dc2`. Это видимый корректный source-level фактор, а не доказательство всех состояний исторического бинарника.

Измеряется реальный `DpoTrainer.StepVector` с project LoRA/AdamW, один учебный шаг на плечо. Одна и та же fixture `simulation-csharp-doc-f06-008`, без добавленного subtraction-prefix: prompt75, chosen46, rejected43, sequence121, vocab629. SHA256 точных байтов сохранённого `train.jsonl` совпал со study: `5b265faaa2046a49449de0f5a44423a28df65bcf79b2c5fabc55e7d6796ba1f6`. Provenance — simulation, `csharpRuntimeVerified=false`; нельзя переименовать это в проверенные runtime-уроки. Настоящее обучение подтверждают recorded 16/16 изменённых LoRA массивов и 0/39 изменённых base массивов. [Study][study], [A raw, строки 44–79, 140–150][raw-a], [B raw][raw-b].

### Успехи и ухудшения одной сохранённой пары A→B

Все значения ниже пересчитаны из raw, не из округлённой таблицы отчёта. Любое полезное уменьшение учитывается как локальный успех соответствующей оси/домена. Причинность величин wall/CPU/peak и устойчивость выигрыша по одной фиксированной последовательности A→B не установлены.

| Показатель и область | A | B | B−A | Наблюдение |
|---|---:|---:|---:|---|
| Выделения в TRAIN, байт | 1 848 491 176 | 1 847 844 360 | **−646 816 (−0,034992%)** | Полезная экономия |
| TRAIN wall, мс | 15 270,2952 | 17 178,4596 | **+1 908,1644 (+12,495923%)** | Ухудшение |
| TRAIN process CPU, мс | 14 406,25 | 15 015,625 | **+609,375 (+4,229935%)** | Ухудшение |
| GC0, сборок в TRAIN | 93 | 90 | −3 | Полезное снижение |
| GC1, сборок в TRAIN | 8 | 5 | −3 | Полезное снижение; пропущено в исходной таблице отчёта |
| GC2, сборок в TRAIN | 6 | 3 | −3 | Полезное снижение; пропущено в исходной таблице отчёта |
| Peak working set процесса, байт | 830 795 776 | 787 300 352 | **−43 495 424 (−5,235393%)** | Полезное наблюдение; не retained heap и не изолированная память Backward |
| StepVector целиком, мс | 14 042,0480 | 16 063,3838 | +2 021,3358 | Ухудшение |
| Два reference-forward, мс | 2 818,2022 | 3 451,0547 | +632,8525 | Ухудшение |
| Два policy-forward, мс | 2 555,3092 | 3 824,9797 | +1 269,6705 | Ухудшение |
| Два backward, мс | 3 052,7733 | 3 777,7379 | +724,9646 | Ухудшение |
| ApplyOptimizer, мс | 756,4620 | 725,1544 | −31,3076 | Полезное наблюдение; не доказано ускорение именно этим фактором |
| Внешний arm lifecycle, секунд | 30,3123982 | 28,1212902 | −2,1911080 | Другая, более широкая область; не заменяет ухудшившийся TRAIN wall |

Источники: [A raw, строки 6, 60–70, 152–181][raw-a], [B raw, те же поля][raw-b]. Полные точные значения, включая представление double, сохранены в [core6e3-late-delta.json](../evidence/core6e3-late-delta.json).

**Граница TRAIN:** [bridge, строки 196–239][bridge] включает настоящий StepVector, внешние проверки/экспорты optimizer state, issued/completed receipts и публикацию checkpoint в native цепочке. Это цена учебного перехода с его учётом, а не изолированный latency `Tensor.Backward`. Диагностические прямые проходы сделаны вне этого окна. Peak процесса накопительный; нельзя приписать весь его спад удалённой копии. Теоретические 608 872 байта двух полезных payload-копий не равны наблюдаемым 646 816; разница не атрибутирована.

### До/после обучения: только первый токен

| Сохранённая TRAIN-диагностика | A до | A после | B до | B после |
|---|---:|---:|---:|---:|
| NLL первого выбранного токена | 6,814549566480718 | 6,814264388510441 | 6,814549566480718 | 6,814264388510441 |
| Rank gold в полном словаре629 | 221 | 221 | 221 | 221 |
| Первый token exact | false | false | false | false |
| Predicted ID | 589 | 589 | 589 | 589 |

Снижение NLL в каждом плече равно **−0,000285177970277** по записанным десятичным значениям. Оно полезно как небольшой учебный вероятностный сдвиг даже без роста exact. Это одинаковый сдвиг A и B; дополнительного преимущества фактора B по этой диагностике нет. Gold ID73, предсказан ID589, текст ` existing`. [A/B raw, строки 17–43][raw-a], [B raw][raw-b].

`DiagTrainForward` [строки 400–443][bridge] кодирует только prompt, читает последнюю его позицию, выбирает `goldIds[0]` отдельно закодированного chosen и вычисляет один NLL/rank. Это **не** mean NLL/rank всех46 chosen-токенов, не teacher-forced response exact и не качество свободно сгенерированного ответа. В исходном [протоколе24, строки 11, 42–49][protocol] требовались exact target IDs/weights, per-position показатели, полный response и отдельная цена readout. В фактическом LF03 нет этих full-response полей и нет отдельной цены четырёх diagnostic forwards. Записать `NOT_MEASURED`/«частичный TRAIN readout», сохранить уже полученный first-token успех, не доучивать и не перезапускать ради заполнения таблицы.

Единственный `loss=0.866434`, rewards/margin/accuracy0 в обоих `step-000001.completed.json` — метрики того же одного шага, а не независимое before/after ответа. Нельзя подменять ими отсутствующий response readout.

### Что сохранность подтверждает, а что остаётся неизвестным

Recorded before policy/reference SHA256 одинаковы A/B (`2ba6bd6d…`); after policy одинаков (`ef0222ed…`), reference не изменился. `optimizerStep=1`, `optimizerEqualLive=true`, `freshWeightsEqualLive=true`, `randomStateReopened=true` зафиксированы в raw. Это ограниченная сохранность состояния данного опыта. Бинарные Git LFS payloads checkpoint в данном аудите не декодировались; утверждение об их проверке самим аудитором было бы неверным. [A/B raw, строки 71–105][raw-a], [B raw][raw-b].

**Новый доказательный пробел LF03 — неполный паспорт исторических исходников.** Файл `source-manifest.json` имеет SHA256 `598dc9782a848db89cde3d83bc1c3e8c9b3998e294acba9bd41a6f6d42841ff5`, как и записано в study. `dependency-manifest.json` также совпадает со study (`dc6871ec…`). Однако [source-manifest, строки 934–944][lf-manifest]:

- `armSources.Core/Tensor.cs.bSha256` — текстовая заглушка `APPLIED_AFTER_FACTOR_EDIT`, не хеш изменённого B;
- `bridgeSha256` записан как `d00355907f83fd6e9e56daea4ddb998fb90cda24a8d98581e50fd9ad2587f3da`, а проверенные точные байты сохранённого LF03 bridge дают **`eb765f3a95ff50814b31dd702d8d0c013734e514205236b771a6ad2ab545401a`**;
- текущий сохранённый B Tensor SHA256 — `ebd9e3a1ca649a140486716167f78ce7a53a7e8d585d57299602e6bef06bcdcb`; это новый readback нынешнего файла, его нельзя задним числом выдавать за исторический build receipt.

Совпадение SHA256 файла-паспорта со study подтверждает, какой именно неполный паспорт был использован, но не исправляет его содержимое. Текущий A/B source и Variant build chain видны; точная связь post-edit B source/bridge с историческим исполняемым бинарником не закрыта. Сохранённые успехи от этого не удаляются; причинную силу формулировки следует ограничить.

**Правка только добавочного объяснения:** вместо универсального «не меняет обучение совсем» — «в этой сохранённой A/B одинаковы указанные final hashes/readback и first-token diagnostics». Вместо «детерминированно около647КБ» — «в одном сохранённом A→B выделено на646816 байт меньше; source custody ограничена паспортом». Утверждения [отчёта, строки 67–77][report], что рост нетронутой policy-фазы или смена знака времени относительно других экспериментов *доказывают* отсутствие эффекта фактора, слишком сильны: это признаки возможных примесей, а не контролируемое установление причины. Ухудшения времени и CPU остаются в таблице; причина — `NOT_ESTABLISHED`.

## 3. Какие замечания 827 уже исправлены

| Замечание аудита 827 | Состояние на6e3 | Как обновить planning |
|---|---|---|
| KN измерял только неизменённый Train и не проверял стоимость изменённого reader | **Исправлено в канонической карточке:** `Read(stage, profiles)` сохраняет elapsed, process CPU и process-wide allocated bytes | Не накладывать старый09 observer; оставить readback исправления и точную границу окна |
| EOPT raw factor/path/core_commit унаследованы от R1 | **Признано в новом erratum**, старые raw намеренно сохранены | Не писать «ещё не замечено/не исправлено в документации»; ссылаться на erratum |
| EOPT GC1/GC2 не видны в основной таблице | **Дополнено:** `[193,19,13]→[189,14,10]` | Сохранить все три поколения в общей таблице |
| EOPT универсальные утверждения о сохранности/качестве | **Erratum ограничивает** одним recorded A/B, не доказывает качество ответа | Наш08 становится независимым подтверждением/границей, а не новой неучтённой правкой |
| EOPT exact post-edit B custody | **Erratum прямо раскрывает пробел**, все три B SHA — заглушки | Усилить старый08 этой оговоркой; не переписывать исторические SHA |
| EOPT TRAINv2 до/после отсутствует | Историческое ограничение сохранено; повтор явно не назначен | Не запускать ради исправления отчёта |
| Нижние output literals будущих карточек указывали protected results при верхнем новом R | В18 будущих карточках literals приведены к существующему R | Старую effective карту не откатывать; использовать новую29-пунктную карту |

**KN точная граница.** [Карточка, строки 147–166][kn] начинает wall/process-allocation до создания `TrainableRoutingFlow`, затем включает четыре назначенных `RouteAsync` и накопление rows; заканчивает до `return rows.ToArray()`. CPU имеет собственную границу после первого `Process.Refresh`; чтение CPU/счётчиков немного меняет границы измерителей. `before=Read("before", empty)` и `after=Read("after", profiles)` [строки 168–185][kn] использует те же вызовы, raw сохраняет `readerCosts`. Это полный выбранный reader с построением Flow, а не latency только Route или всего `Read` вплоть до `ToArray`. Всего остаётся2Train/16Route; process-wide allocation корректнее через `await`, чем CurrentThread. Статус рецепта по-прежнему не собран/не запущен. Не добавлять вторую панель, новый timer вокруг тех же строк или дополнительные queries. Старый overlay09 нужно явно пометить `SUPERSEDED_BY_CANONICAL_6e3`.

**EOPT источник:** [measurement-erratum, строки 3–17][eopt-erratum]. Проверен и сам [EOPT source-manifest, строки 829–844][eopt-manifest]: три `APPLIED_AFTER_FACTOR_EDIT` относятся к DpoTrainer, AdamW и AdamWPortableState. Старый независимый source review корректно показывал текущий A/B фактор и nonempty RNG callback, но не восстанавливал полную historical binary custody; не усиливать его до такого утверждения. В конце erratum снова сказано о будущем LF03 — этот NEXT также устарел относительно фактического LF03 результата в том же commit.

## 4. Точная дельта путей и entry

У **всех29 оставшихся core-пунктов** верхние effective `R`, report/candidate и назначенные entry остаются такими, как в нашей effective карте827. **Новых effective roots и новых pending entry — ноль.** LF-03 удаляется из future-map; его фактический исторический root фиксируется отдельно. AST `candidate-fix.md` уже был корректным в карте827 и теперь явно указан в верхней строке3 карточки.

В18 pending карточках исправлены именно нижние output-only literals, которые раньше противоречили верхнему `R`. Машиночитаемая дельта с точными новыми строками: [core6e3-path-delta.json](../evidence/core6e3-path-delta.json).

| Pending карточки | Новый/уже effective суффикс внутри `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/` | Что приведено к нему |
|---|---|---|
| N60-E1, LC-REF-CACHE, T-TRAIN-CONTEXT | `N60-E1-VERIFIER-ADMISSION/results`, `N61-REF-CACHE/results`, `T-TRAIN-CONTEXT-01/results` | Runtime/bridge результата |
| SFT-RESPONSE-MASK, SFT-MOMENT, DPO-VALID-LENGTH, DPO-SFT-ORDER | `MS01/results/attempt-0001`, `MC01/results/attempt-0001`, `PD01/results/attempt-0001`, `OR01/results/attempt-0001` | `--prepare`, `--arm A/B`, `--summarize` и body destination |
| KH-COST, KH-EXT, GRADB | `KHC/results`, `KHE/results`, `KGB/results` | Body/runtime destination |
| RMS-COORD, DPO-TOKENIZER, BPE-WEIGHTED | `RMS01/a001/results`, `TS01/results/attempt-0001`, `BW01/results/attempt-0001` | Runner/intent/result literals |
| WIKI-FEATURE-KEYS, WIKI-FEATURES-ONCE | `WK01/results/attempt-001`, `WC01/results/attempt-001` | Arm output, scaffold/runtime и отчёты; `report.ru.md` сохранить |
| N-HEAD, METHOD-KNN-TARGET-SCOPE, KN-ROUTER | `NH01/results/attempt-0001`, `MKSC01/results/attempt-0001`, `KR01/results` | Body destination; METHOD prepare/arms/summarize; KN runtime |

Новая карта проверяет Git blob SHA каждого referenced source card на6e3; одинаковые retained cards взяты из локального827 только после совпадения blob. RETENTION/IDENTITY делят одну карточку, но имеют разные roots; выбирать правильную строку, а не первый `R=` файла. Команды остаются назначенными рецептами, не подтверждёнными собранными host. Права конкретной прежней core-сессии писать в `required-changes.md` не отменяют нынешнее указание пользователя не менять protected: наш candidate публикуется только в разрешённом planning/possibleDifs.

## 5. Объём проверки и материалы

Полный manifest защищённого дерева6e3 содержит1372 blobs и348 Markdown; относительно827 изменены/добавлены63 файла, из них39 Markdown, удалённых0. В этом позднем readback загружены и Git-blob-SHA проверены **52 изменённых protected файла (39MD+13JSON)** и **17 выбранных source/host/manifest файлов**. Это не заявление о новом семантическом прочтении всех348MD, всех114 исходников LF03 или всех LFS payloads. Более ранний bounded аудит827 и crosswalk799 сохраняют свой исходный cutoff; их нельзя автоматически объявить полным аудитом6e3.

Новые результаты: [core6e3-late-delta.json](../evidence/core6e3-late-delta.json), [latest6e3-output-path-map.json](../evidence/latest6e3-output-path-map.json), [core6e3-path-delta.json](../evidence/core6e3-path-delta.json). Private download manifests с подписанными URL остаются служебными и в пакет не входят. До следующего специально порученного аудита cutoff заморожен на6e3; новые HEAD не отслеживаются в этой задаче.

[index]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L5
[report]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/report.md
[raw-a]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/A/arm-result.json
[raw-b]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/B/arm-result.json
[study]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/study.json
[tensor-a]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/A/Core/Tensor.cs#L99
[tensor-b]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/B/Core/Tensor.cs#L99
[tensor-props]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/experiments-v2/RP01-MICRO/LF-03/Sources.TensorCore.props#L15
[run-lf]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/experiments-v2/RP01-MICRO/LF-03/Run-Lf03.ps1
[bridge]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/Common/ContinuousLearning/N62ExperimentBridge.cs
[lf-manifest]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/source-manifest.json#L934
[eopt-manifest]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/source-manifest.json#L829
[eopt-erratum]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/measurement-erratum.md
[kn]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/KN-ROUTER-LEARNED-KEY-01.md#L147
[protocol]: https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/24-neural-learning-signal-protocol.md
