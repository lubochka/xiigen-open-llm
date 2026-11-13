# Кандидаты на исправления: измеренная польза сохранена

**Это семь уже выполненных опытов; новых запусков в этой работе нет.** Числа, первичные ссылки, остальные показатели и ограничения — в [доказательном отчёте](01-results-and-candidates.md). Все записи имеют статус **`CANDIDATE_ONLY_USER_DECISION`**. В этой работе продукт, tests, веса, protected документы и старые raw не менялись. Любое полезное измеренное улучшение признаётся по своей метрике и домену; accuracy расти не обязана.

## C-01 · N-KU-01 — метка do-while

Предлагаемый адрес: `12.SemanticCompiler/Prediction/FileActionKnn.cs::ActionLabelGenerator.Label`; добавить `SyntaxKind.DoStatement` к `hasLoop`. В C# memory-learning опыте лишний ADD_LOOP на трёх задачах с уже существующим циклом 3/3 → 0/3; итоговый rubric 1/4 → 3/4. Регрессия: нужный ADD_LOOP 1/1 → 0/1. Все ответы меняют majority label; структурное обобщение не доказано. Не включать в этот кандидат одновременно новый feature vector. Сохранить полезный label-effect и противоположную ошибку для решения пользователя; full product tests не назначены.

## C-02 · NEURAL-FROZEN-TRANSPOSE-01 — frozen weights

Предлагаемый адрес: `00.TensorCore/Layers/Linear.cs::Forward`; переиспользовать транспонированное представление только неизменяемой frozen матрицы. Четыре SFT/LoRA updates/arm: allocations −1 507 392 байта, около −11,04%; записанные state/optimizer/loss/logits совпали. Retained cache +249 856 payload bytes; wall и CPU не улучшились, exact 0/2 → 0/2. Уменьшение NLL после обучения одинаково в A/B. Кандидат полезен по allocations; lifecycle invalidation при иных способах изменения весов отдельно не проверен. Новая ATTN-гипотеза касается активаций и не включена в этот diff.

## C-03 · BPE-NO-MATCH-COPY-01 — лишние chunks

Предлагаемый адрес: `01.Tokenizer/BpeTokenizer.cs::MergeChunk`; создавать новый список после первого реального merge, без match возвращать исходный chunk. В настоящем BPE Train allocations −71,14% на code-doc и −69,11% на EN→RU. Learned merges/state/token sequences совпали; roundtrip 4/4. В code-doc CPU вырос 31,25→46,875 мс. Общая neural/KNN точность и перенос на все callers не измерены. Результат уже завершён; исторический Windows path failure не назначает повтор. Алгоритмы weighted/realized merge — самостоятельные будущие факторы.

## C-04 · N62-R1 — проверка frozen base

Предлагаемый адрес: `11.DPO/DpoTrainer.cs::DpoProjectLoraRecipient.RequireValid`; сравнение float bit representation через byte spans вместо поэлементного LINQ. На двух DPO updates allocations −90 504 байта (−0,002404%), recorded state/metrics равны. GC0/1/2 выросли на один каждый; target quality не измерено. Сохранённое уменьшение времени — полезное наблюдение, устойчивое причинное ускорение не установлено. Не объединять с softmax, metadata step или удалением исходных проверок получателя.

## C-05 · N62-R2 — scratch softmax

Предлагаемый адрес: `11.DPO/DpoTrainer.cs::BackwardLogProbs`; один локальный expVals на вызов вместо массива на каждую активную позицию. Chosen/rejected сохраняют разные массивы. На двух DPO updates allocations −312 328 байт (−0,008296%); peak +30 474 240 байт и GC0/1/2 +1 — ухудшения. Recorded state/metrics равны; историческая точная B-binary custody неполна. Расчёт 322 048 payload bytes не совпадает с общей измеренной дельтой, разница 9 720 не объяснена. Кандидат успешен по allocations с этой границей доказательств; повтор не назначен.

## C-06 · E-OPT-STEP — чтение шага без копирования moments

Предлагаемые адреса: `11.DPO/DpoTrainer.cs`, `04.TrainingLoop/Optimizers/AdamW.cs` и `AdamWPortableState.cs`; metadata reader сохраняет проверки имён/форм/длин, а для номера шага не копирует m/v. Два DPO updates: allocations −467 944 байта; GC0/1/2 −4/−5/−3. Peak +204 546 048 байт. Записанные state/metrics совпали; это не универсальная проверка guards/error paths. Raw содержит старые R1 labels, actual source задаёт EOPT; см. [addendum](08-eopt-provenance-addendum.md). Кандидат сохранён по полезной ресурсной оси с ограниченной атрибуцией; решение о полном переносе — за пользователем.

## C-07 · LF-03 — начальный градиент

Адрес: `00.TensorCore/Core/Tensor.cs::Backward(Tensor)`. Убрать внешний `initialGradient.Data.ToArray()`, оставив защитную копию внутри TensorStorage. Один настоящий DPO update на плечо: allocations −646 816 байт, GC0/1/2 −3 каждый; whole-process peak −43 495 424 байта. Регрессии: TRAIN wall +1 908,1644 мс, CPU +609,375 мс. Recorded state совпадает, first-token NLL снижается одинаково в A/B; дополнительного качества B не измерено. Historical B provenance неполно, универсальная эквивалентность не доказана. **Кандидат успешен по наблюдённым полезным осям; перенос решает пользователь**, повтор не назначен. [Полный readback](01-results-and-candidates.md#6-поздний-завершённый-lf-03-отдельный-седьмой-кандидат).

## Новые гипотезы пока без измеренного результата

`ATTN-TRANSPOSE-STRIDES-01` и `JSON-OPTIONS-REUSE-01` не добавлены в список **успешных** исправлений: это оформленные отдельные рецепты, **NOT_RUN**. После назначенного исполнения любое полезное измеренное изменение создаст их собственный candidate-файл. Проверка пустого verifier checkpoint-test — доказательное уточнение P25/P56, а не новый успешный runtime опыт.

При команде «следующий» немедленно прекратить дальнейшее оформление предыдущего кандидата и следовать [реестру](04-execution-registry.md). При «остановить всё» прекратить всю работу. Неполнота отчёта не оправдывает продолжение.
