# Независимая проверка RMS-ROW-REDUCE-01

Дата: 20.10.2026. Reviewer: `/root/historical_knn`, отдельно от автора `/root/learning_code_audit`. Проверены полные актуальные `new-rms-row-card.md`, `new-rms-row-prompt.md`,36-file manifest, исходный pinned Program scaffold и выполняемые native методы core `66947785d1318f7cbbc88118f3156cdc75001e16`.

**Вердикт: DESIGN_VALID / RUNTIME_UNVERIFIED.** Открытых замечаний к публикации как гипотезы с точным будущим рецептом нет. Runtime host ещё не создан/не собран; улучшение allocations, качество обучения и численная идентичность после исполнения не измерены. Проверка не запускала learner, build, tests, surrogate или численный benchmark.

## 1. Реальное участие в обучении

Проверен путь [`Trainer.TrainSupervisedNextTokenStep`,102–147][trainer] → [`LlmModel.Forward`,219–242][model] → [`TransformerBlock.Forward`,46–64][block] и final norm → [`RmsNorm.Forward`,29–40][rms] → [`Mean`,530–586][mean] → [`SumAlongAxis`,1020–1047][sum]. `RmsNorm` использует axis−1/keepDims=true; оптимизируемая ветвь достигается обычным neural forward до backward и actual AdamW update. [`AdamW.Step`,57–108][adam] изменяет веса и моменты; карточка требует trainable hashes и optimizer steps, не удовлетворяется голым таймером арифметики.

Таким образом, это измерение затрат действительного обучения на предметной задаче. Тема «язык программирования C#», задача «знак целочисленного вычитания», механизм SFT/q-v-LoRA и низкоуровневое изменение Mean разделены. Runtime LogicalUnit/Flow здесь нет; raw receipt прямо исправлен на native_learner_instances=1/logical_unit_flow_scope=not used.

## 2. Математика единственного изменения

В [`TensorShape`,25–57][shape] row-major strides, последняя stride=1. Private `SumAlongAxis` вызывается из Mean, где остальные размерности результата сохраняются, а последняя равна 1. Поэтому для этой ветви old `FlatIndex(dstIdx с последним k)` точно равен `dstFlat*axisSize+k`.

Новая ветвь выполняет те же скалярные `sum +=` в порядке k0…axisSize−1, тот же assignment dst, прежние точки cancellation и прежнее деление на axisSize за пределами helper. Она устраняет `FlatToIndices` и `.ToArray` на каждое слагаемое. Другие axis/keepDims формы остаются на исходном пути; autograd children/backward/gradient broadcast не меняются. Это математическое обоснование, **не** заявление о выполненном битовом parity тесте. Карточка верно требует сравнить A1/B1 state/logits уже после объявленного Train и показать любое расхождение.

Это отдельный фактор от `RMS-COORD-ALLOC-01`: protected исходная карточка [82][old-rms] прямо исключает изменение Mean и меняет MultiplyBroadcast. Новый опыт не совмещает эту оптимизацию с прежней. Известный keepDims:false backward defect здесь не достигается и не используется как оправдание дополнительного ремонта.

## 3. Сопоставимость и счётчики

Данные до/после в обоих плечах фиксированы. TRAIN T1=`9-4=+`,T2=`4-9=-` в полном literal prompt; порядок T1,T2,T1,T2. EXAM отдельные 8−3/3−8. Исходный scaffold получает logits из настоящего model.Forward; ответ не вычисляется арифметическим правилом. Gold используется для scoring и не заменяет output.

| Вызов | На arm | A+B |
|---|---:|---:|
| TrainSupervisedNextTokenStep |4|8|
| EXAM readout до+после |2+2|8|
| TRAIN diagnostic prompt readout до+после |2+2|8|
| Все readout forward вне TRAIN |8|16|
| Neural DPO/внешний teacher/restore/repeat |0|0|

Каждый Train вызывает свой обычный forward/backward/optimizer; эти 8 training forward не являются ещё 8 EXAM. A0/B0 получают identical initialization/seed/config и выключенную экспериментальную ветвь. B переключается непосредственно перед TRAIN и сохраняет branch при B1. Два диагностических TRAIN prompts оцениваются до/после, поэтому четыре online losses до отдельных updates не подменяют matched before/after. Для каждого readout сохранены 256 logits, native argmax, targetNLL, goldrank и exact. Для однобайтного ответа `+`/`−` exact одного next-token соответствует этой узкой проверке; свободное владение задачей не заявляется.

Примечание о буквальном corpus: output minus в карточке/scaffold — ASCII `-`, U+002 D. Символ `−` в пояснительной прозе этого review не меняет fixture. Source byte-tokenizer прямо обозначен `Encoding.UTF8`, без claims о вызове native BPE.

## 4. Timer, память и state

Primary allocation/time boundaries включают ровно 4 реальных Train calls/arm: forward/backward/clipping/AdamW. Preparation, Observe на EXAM/Train, hashes, serialization и build находятся снаружи. `GC.GetAllocatedBytesForCurrentThread` обозначает выделенные bytes, не peakRAM. CPU считывается с Refresh, process peak — отдельный показатель. Cold JIT и один A→B порядок остаются ограничением causal wall-speed вывода.

Хеш trainable state обязан измениться; frozen state — сохраниться; optimizer step4 и moments hash отдельно. Статическое сокращение координатных массивов не объявлено measured improvement. Правило пользователя any-useful-metric success присутствует в обоих документах: полезный allocation gain можно признать при неизменной accuracy, а NLL/exact/CPU/RAM ухудшения публикуются рядом.

## 5. Источники и воспроизводимость рецепта

Все 36 manifest blobs независимо сверены с actual source bytes.34 найдены в shared current snapshot; два `00.Core` файла дополнительно прочитаны через GitHub на том же pin. [Квитанция 36/36](../evidence/review-rms-source-hashes.json). Это не означает, что новый host собран.

Blueprint задаёт один короткий root `C:/xg/RD01/a001`, fixed no-argument run.py, один flat net9.0 host без ProjectReference и один результат на A/B. Source originals читаются через pinned git-show argv без shell/CRLF, hash проверяется **до** copy/patch; prepared SHA256 проверяет уже подготовленный host. Existing attempt/results/bin/obj сохраняются; autoa002/retry отсутствуют. Build deadline120s, A/B по 60 s, total native120s. Timeout без durable receipt даёт null/«не измерено», не выдуманные 4 updates.

Explicit commands и места report/candidate/logs заданы, старые глубокие ArtifactRoot/параметры прежнего launcher не наследуются. Pinned Program используется как исходный текст, его frozen-transpose switch/counters удаляются; оригинальный Linear.cs остаётся в 36 файлах. Protected folder и production read-only. Doc-only статус и no-runnable-claim сохраняются.

## 6. Замечания, исправленные до принятия

| Замечание reviewer | Внесённое автором исправление | Readback |
|---|---|---|
| Старый scaffold оставлял `domain=args[1]`, где `code-sign` — профиль, а не предмет обучения | Явно заменить на `profile=args[1]`, `domain=programming-language-csharp`, сохранить subject/task | Проверены card§5.3 и prompt п.5 |
| Checksum уже подготовленных copies мог проверять только самосогласованность, без проверки pinned originals | Проверять actual Git blob каждого 36 originals до копирования/patch; exact hash formula и no-shell argv указаны | Проверены card§5.2 и prompt п.2 |

Оба исправления прочитаны после записи. Reviewer не внёс алгоритм в production и не превратил blueprint в запуск.

## 7. Управление исполнением

Немедленные STOP и NEXT запрещают даже дальнейшее оформление прежнего опыта; они есть в card и prompt. Есть current-default executor model, exact skills, русский report, запрет удаления прежних результатов, no-auto-production-fix и explicit NEXT=IDENTITY-M1-01 после RMS. Новый Registry имеет право определять общую очередь; NEXT не разрешает автоматически запустить второй опыт. Подтверждён только план малого эксперимента, не его успех или пригодность production patch.

[trainer]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/04.TrainingLoop/Trainer.cs#L102-L147
[model]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/03.TransformerModel/LlmModel.cs#L219-L242
[block]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/03.TransformerModel/TransformerBlock.cs#L46-L64
[rms]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/03.TransformerModel/Layers/RmsNorm.cs#L29-L40
[mean]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorOps.cs#L530-L586
[sum]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorOps.cs#L1020-L1047
[shape]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorShape.cs#L25-L57
[adam]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/04.TrainingLoop/Optimizers/AdamW.cs#L57-L108
[old-rms]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/RMS-COORD-ALLOC-01.md#L82
