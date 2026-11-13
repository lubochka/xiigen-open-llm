# P48–P58: точность измерителей и отказов. Дополнение к possibleDifs от 11.11.2026

Основание: read-only audit core `009bf226174441fa0fbf0336e74ad2f2b7174d92`, planning `04ae31e9232d7c8763bd393a36fc163ac2fd0149`. Перечисленные ниже исходные исследовательские документы прочитаны целиком и сохраняются как датированная история. Дополнение не объявляет их исполненными, не разрешает повторять старые Train и не переносит выводы Probe/Unit на RP01 Student.

## Crosswalk: какой документ усиливается и в какой части

| Исходный документ possibleDifs | Точный участок | Новое требование |
|---|---|---|
| `33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md` | «Сверка P46/P48/P49 —30.10.2026»; «Не исправлять научную историю задним числом» | Source-closure, attempt identity и фактор перечисляются отдельно. Current repaired backward и finite gate не остаются в очереди как открытые из-за исторического текста |
| `27-NEW-E48-MISREAD-WARM-START-AND-MISSING-CHECKPOINT-2026-10-16.md` | «Граница последующих выводов о тёплом старте —9 ноября2026» | Overlap имеет три категории, а не бинарное replay/unseen: новый target при знакомом input не является новым input |
| `27-MICRO-LOW-BUDGET-TRANSFER-AND-GRAPH-CLOSURE-2026-10-16.md` | «Measurements and stop»; «Minimal implementer contract» | Независимые guards управляют serializer и external terminal. Собственный статус healthy helper не удостоверяет whole run |
| `41-REJECTED-SIDE-ANNOTATION-CONTROLS-POSITIVE-EXECUTOR-MEMORY-2026-10-18.md` | §2 critical negative; §3 measures and quality causality | Cold score, constant maximum, marker provenance и actual consumer response являются разными наблюдениями |
| `60-01-NEW-CURRICULUM-FEEDBACK-AND-UNIT-MEASUREMENT-SEAMS-2026-10-19.md` | H60-A/B; S60-M; ожидаемая трасса в конце | Полная история физического обучения и actual feedback provenance включаются в расход и вывод; успешное изменение урока не заменяет ordinary learning outcome |

## P48 / source-history: что закрыто, что измерялось

Фразы старого N33 о необходимых repairs ProbeNN остаются верными для указанного исторического snapshot. На нынешнем core pre-step backward и finite-verdict guard уже исправлены. Нельзя переносить их в новый список открытых product defects без проверки source. P48 attempt2 и attempt3 также нельзя сводить в одно A/B: weighted A/B attempt2 имели12/16 и0 различающихся actual; attempt3 содержит equal-weight и repeated-row mechanism control с12/16 и6 различающимися actual. В следующем плане указать mapping claim→attempt→factor→learner source→consumer; одинаковый score не считается побайтовой parity. Repeated-row контроль не реализует gradient weighting автоматически. Сохранённый historical receipt остаётся, а correction добавляется отдельным документом с provenance.

## P49 / warm transfer: независимость определяется входом модели

К уже имеющемуся требованию таблицы model-visible overlap добавить mutually exclusive counts: seen exact pair, seen input/new target, unseen input. В сохранённом P49 attempt3 cold EXAM распределён2/1/1, warm4/0/0. e-t1 = 0→1 не новая входная задача: cold видел 0→3. e-t2=2→3 — действительно новый input. Существующий source test 1→5, требующий слово unseen, ошибочен: TRAIN содержит 1→0. Negative control меняет толькоgold при том жеinput и обязан сохранить input-novelty count.

Выигрыш нового материала7/8 против6/8 и утрата старого TRAIN6/6→3/6 сохраняются. Warm стоит600+60updates, cold60. Одинendpoint не превращать в экономию времени/скорость по дозам. В будущем E48-SMALL source/family partition дополняется фактическим model-visible partition; разныеsource IDs сами по себе независимость не доказывают. Это уточнение не меняет его заранее назначенные бюджеты и не разрешает новый запуск.

## P50 / graph result: валидность предшествует научной поддержке

Наблюдение P50 A 2/4, B 4/4 допускается только в пределах исполненного toy graph consumer. При обнаруженном unknown node, неверной дозе или неполном stage outcome builder сначала записывает invalid/open и причину, затем завершает тот же процесс ненулевым exit. Проверка last>first — endpoint gain, не доказательство монотонности всего sweep. Принятие критерия отдельно проверяется на [2,4,1,4]. Для нормального отрицательного исхода healthy эксперимента сохраняется successful execution без научной поддержки.

Отдельный процесс с восстановленнымиBPE/HNSW/records остаётся экспериментальным reader. Называть его native Unit restore можно лишь после actual Import/Load нового Unit и ordinary Execute. Приёмку того же whole host проверять на invalid observation после уже оплаченногоTrain, чтобы report correction не перезапускала обучение. Все физические main/probe/control исполнения учитываются отдельно, даже когда hashes их состояний совпадают.

## P52 / annotation: независимый смысл каждого числа

Для будущих annotation A/B/C/D сохранить правильную цепь rejected marker→chosen typed payload→native state→ordinary output, но не оценивать её одним итоговымscore. Current P52 attempt2 подтверждает address1/4→3/4 при answer4/4→4/4. Обе обученные панели используют exact-source memory. Этот факт не доказывает преимущество переводческого качества или unseen composition и не отменяет потенциальный harmful-marker риск N41-T2.

Добавить аналитический baseline: maximum score единственного постоянного ответа на всей панели по тому же equality/rubric. Для P52это2/4;cold 0/4 — отдельное наблюдение. Independent review строитmaximum изgold histogram и обязан заметить подмену cold вместоmaximum. Две копии одинакового неверного числа не независимы. Перед следующей реализацией каждой метрики фиксировать semantic definition, numerator, denominator, comparison rule и независимый negative. Тестирование cold!=constantMaximum не требует ни teacher generation, ни native Train.

## P58 / feedback: изменение урока, приобретение навыка и terminal различаются

Для correction сохранять rowID, исходный request, observed wrong answer, teacher chosen, source state и созданный lesson identity. ВP58 attempt2 B действительно содержит actual wrong answer e1; A/B state и последующие ответы совпадают, C control state отличается. Значит пригодность контрпримера улучшилась0→1, но влияние rejected на answer не найдено. Current source уже вычисляет constant ceiling 1/8 и подтверждает setup learning отдельно по TRAIN 0/8→8/8; EXAM 1/8 не вышеconstant baseline.

Сохранённый receipt обязан существовать и при no TRAIN gain, unequal start, instrument failure. Earlyassert, который выбрасывает управление до записи INVALID, нарушает договор даже при правильном DecideReceipt/Serialize. Для no errors наinitial panel требуетсяNO_CONTRAST и 0 corrective calls. Проверять весьmeasurement→receipt→sink→externalexit, не только сериализациюsynthetic invalid object.

Фактическую дозу не сводить к числу новых исходных пар. У P58 attempt2 при прежних24 setup /3 corrective Unit records на плечо отдельно был24-lesson setup probe. Derived total=24+3×24+3×3=105 Unit lesson calls одного успешного main Fact; семь сохранённых панелей по8 строк дают56 наблюдений прямой цепочки `SemanticTokenizerUnit.Tokenize → ExecutorUnit.Execute`. Это не56 полных запусков TranslationMachine/TranslationRoutingFlow, не lifetime total и не105 уникальных пар. Общая сумма дополняет per-arm числа, не заменяет их. History counter должен видеть каждый физический fit и исключать повторный подсчёт внутренних Tokenize как отдельных наблюдений. Для H60-A/B отдельно сохранять исторический score, latest score, выбранные lesson IDs, фактические Train calls и ordinary gain: выбор уроков или запись статуса сами по себе не доказывают их освоение.

## Исполнение настоящего дополнения

Стоимость основного P58 Fact не считается итогом всего test class: в том же source есть другие обучающие Facts, например строки1183 и1377.105 вызовов основной проверки — нижняя граница общего расхода, если эта проверка входила в процесс. Для полного итога нужны selector, discovery/TRX и физические run IDs. Это уточнение не меняет24/3 per-arm и не увеличивает56 наблюдений основной проверки за счёт неизвестных запусков.

В аудите выполнены толькоsource inspection и Python-пересчёт сохранённых строк. Native build/discovery/tests/mutations/learning UNRUN. Исправленияmeasurement code, полныйterminal QA и новые научныеcomparison recipes имеют разные будущиеwork items. Старыеraw сохраняются; текстовые correction и crosswalk не запускают ученик повторно и не объявляются новый quality result.
