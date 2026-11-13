
# N34 — новое исходно подтверждённое различие: база обучается на другом потоке токенов, чем предполагает контракт границ и документов

**Дата:** 2026-10-16. **Статус:** SOURCE_CONFIRMED mechanism / SOURCE-ONLY proxy executed / CAUSAL_LEARNING_NOT_RUN / NATIVE_NOT_RUN.  
**Core:** `llm_mvp_core@216a1f3d6af3f40ceef081dc168b07e4ce3934f9`. **Planning:** `xiigen-planning/llm_plan3`.  
**Зона изменений:** только `replanning/possibleDifs`. Исследования `llm_mvp_core/docs/plans/replanning-possible-diffs` — READ ONLY.

## Суть для архитектора

RP01 обучал *DPO/LoRA Student* с рандомной B0; это не `DatasetBuilder`/`DataLoader`. Нельзя объявлять перечисленные ниже дефекты причиной состоявшегося RP01 C#500/ENRU500. Однако переход от *случайной B0* к обещанной пригодной для DPO базовой модели с помощью собственного pretraining/SFT потребует проверки другого реального пути: 
`DatasetBuilder.BuildAsync → *.npy → DataLoader.NextBatch → Trainer.TrainAsync/RunPretrainingCoreAsync → LlmModel.Forward`.
Этот путь отличается от exact response-masked DPO; ошибка в нём способна скрыть полезность новых DPO пар ещё до эксперимента.

**Новый материал N34:** этот класс конкретных границ `DatasetBuilder/DataLoader` не представлен в тематическом индексе `possibleDifs/README.md` на момент чтения; существующие N33 embedding/warmup, N29-N33 KNN и E07/E09 — отдельные доказательства. Не утверждается абсолютная новизна относительно каждого из 259 файлов или всех ранее запущенных агентов.

## N34-A · Граница документа кодируется ID=1, но EOS токенизатора — последний ID

### Что доказано кодом
- [`DatasetBuilder.cs:19-20`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/02.DatasetPipeline/DatasetBuilder.cs#L19-L20): `private const int EndOfFileToken = 1`.
- [`DatasetBuilder.cs:42-49`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/02.DatasetPipeline/DatasetBuilder.cs#L42-L49): после каждого файла добавляется ровно `1` в поток токенов, не вызывая tokenizer для специального маркера.
- [`BpeTokenizer.cs:37-38`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/01.Tokenizer/BpeTokenizer.cs#L37-L38): `EosToken => _vocab.Count - 1`. При `VocabSize=629` это `628`, а не `1`.
- [`BpeTokenizer.cs:42-55`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/01.Tokenizer/BpeTokenizer.cs#L42-L55): базовые 256 IDs — байты `0..255`; ID=1 соответствует обычному байту `0x01` (SOH), не зарезервированному модельному end-of-document. Текущий `Decode` также не удаляет этот ID как special.
- [`DataLoader.cs:43-56`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/02.DatasetPipeline/DataLoader.cs#L43-L56) передаёт `1` как обычный target/input в последовательно нарезанных next-token окнах.

### Гипотеза качества
Нейронная база может учить переходы `...предыдущий файл → byte(1) → следующий файл` вместо явной, согласованной и отдельно контролируемой границы документа. Это потенциально вредит разделению независимых кодовых файлов и предложений. **Это не доказательство ухудшения RP01 и не утверждение, что все обучающие данные содержали такой переход.**

**Область:** pretraining кодовой базы C#/TS/Python и смешанных текстовых корпусов; перевод — если исходники передаются как несколько файлов. **Не относится напрямую:** exact DPO StepVector, HNSW, E09 Structural KNN.

**Falsifier:** при `VocabSize > 256` вывести IDs фактического `EOF` и `EOS`; проверить, сохраняется ли отдельный boundary bit/attention reset; после одного токенизатора и равного training budget провести A (`1`) против B (явный, реально выделенный и корректно сериализованный EOD с валидной политикой loss-mask). Не вставлять `EosToken` как суррогат EOD без анализа: текущий EOS сам определяется последним *обычным* merge.

## N34-B · Шаг окна равен одному токену, а не длине последовательности

- [`DataLoader.NextBatch:43-57`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/02.DatasetPipeline/DataLoader.cs#L43-L57): после выдачи каждого training row вызывается `_pos++`. Для `seqLen=2048` соседние rows используют 2047 одинаковых входных токенов из 2048, а targets также сильно перекрываются.
- [`Trainer.TrainAsync:150-165`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/04.TrainingLoop/Trainer.cs#L150-L165): реальный pretraining consumer получает данные из `loader.NextBatch`. Повторяется на каждом accumulation в [`RunPretrainingCoreAsync`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/04.TrainingLoop/Trainer.cs#L191-L226).

**Квалификация:** stride=1 — допустимый осознанный sliding-window режим, **не математическая ошибка сам по себе**. Но при малом числе optimizer updates и большом контексте он может давать крайне низкое число уникальных токенов на один batch, высокую корреляцию градиентов и расход бюджета без охвата новых синтаксических конструкций. Напротив, перекрытие может помогать на коротких контекстах. Требуется опыт, а не безусловная замена на `_pos += seqLen`.

**Домен:** широкие next-token C# корпуса, редкие грамматические явления RU/EN, длинные документы; не KNN и не DPO preference batches.

## N34-C · Без предупреждения склеиваются разные IDs при сериализации uint16

- [`DatasetBuilder.cs:46-49`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/02.DatasetPipeline/DatasetBuilder.cs#L46-L49): `(ushort)Math.Min(t, ushort.MaxValue)`.
- [`NpyWriter.cs:10-35`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/02.DatasetPipeline/NpyWriter.cs#L10-L35) пишет `<u2`; [`NpyReader.cs`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/02.DatasetPipeline/NpyReader.cs) читает `ushort`.
- Для `t=65536` и `t=70000` записывается один и тот же `65535`: разные BPE/семантические IDs становятся неразличимы. При текущем RP01 `vocab=629` данный путь **не может** вызвать такое насыщение для корректных IDs; риск условен — для будущих tokenizer/vocab больше 65536 или неисправных входных IDs.

**Область:** high-vocab pretraining и долгоживущая миграция словаря; не текущий RP01 Student, не E26 dictionary, не LoRA сохранение. **Минимальный falsifier:** на `[65535,65536,70000]` сделать serialize/readback; затем сравнить `uint32`/range-checked transport при том же corpus. Защитная проверка с точной ошибкой вместо silent clamp — самостоятельная малая кодовая правка, её не следует путать с доказанным приростом генерации.

## N34-D · Потеря межшардовых контекстов и нефискированный порядок исходных файлов

- [`DatasetBuilder.cs:35-37`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/02.DatasetPipeline/DatasetBuilder.cs#L35-L37) берёт `Directory.GetFiles` без явной сортировки; [`42-65`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/02.DatasetPipeline/DatasetBuilder.cs#L42-L65) сериализует конкатенацию в шарды по `_shardSize`. Поле `_seqLen` сохраняется, но в `BuildAsync` не управляет контекстной границей.
- [`DataLoader.EnsureAvailable:60-76`](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/02.DatasetPipeline/DataLoader.cs#L60-L76): если в текущем шарде осталось меньше `seqLen+1`, loader переходит к следующему шарду и сбрасывает `_pos=0`; контекст из конца предыдущего не продолжается. При шардах короче `seqLen+1` нельзя получить ни одного окна.
- Следовательно, split одного файла на границе шардов и начало нового файла внутри одного шарда получают *разную* policy: в первом случае переход теряется для next-token примера, во втором учится через `ID=1`. Без pinned file list/sha/shard manifest сравнивать одинаковость обучения между запусками нельзя только по количеству файлов.

**Статус:** механизм подтверждён; влияние на качество зависит от фактических размеров файлов/шардов и домена. Не объявлять произвольный порядок `GetFiles` гарантированно различным в каждом запуске.

## Историческая проверка против соседних веток

Считаны версии `02.DatasetPipeline/DataLoader.cs` и `DatasetBuilder.cs` в `DesignToCode`, а также blob SHA для `language_translate` и `external_domains`. В `DesignToCode` существуют те же `_pos++`, `EndOfFileToken = 1` и `Math.Min(t, ushort.MaxValue)`. Blob SHA старого DataLoader `85927cc9db7e1ed53d6a0035a8e85ccfb30be96b`, старого DatasetBuilder `5e76bcbc992b240973d90b695d1664bbfd8ade22`. Для `language_translate` и `external_domains` получены те же старые blob SHA. **Это унаследованные алгоритмы, а НЕ обнаруженная потеря при переносе из этих трёх веток.** Положительные E09/E26 нельзя приписывать их исправлению.

## Причинность, приоритет и ограничения доказательств

| Вопрос | Вердикт |
|---|---|
| Существует ли несовпадение EOF/EOS, overlap, clamp, разрыв межшардовой последовательности? | **Да, SOURCE_CONFIRMED** в указанной ветке |
| Исполнен ли исходный .NET pipeline для сравнения A/B? | **Нет, NATIVE_NOT_RUN** |
| Есть ли выигрыш на независимых доменных задачах? | **Не измерен, CAUSAL_HYPOTHESIS** |
| Объясняет ли это уже опубликованные RP01 0/3→0/3 и 0/4→0/4? | **Нет доказательств; pipeline другой** |
| Что исправлять раньше? | Для самого RP01 — сначала приемлемая warmed B0/gradient/teacher/free-answer из существующего P0. N34 — pretraining gateway, только если выбран такой bootstrap |

**Source-only receipt:** [исполняемый proxy](34-SOURCE-ONLY-DATA-PIPELINE-PROXY.mjs) и [JSON](34-SOURCE-ONLY-DATA-PIPELINE-PROXY-RESULT.json) воспроизводят лишь арифметику индексов, они НЕ являются исполнением C# / обучением / blind quality judge.

**Связанные старые evidence owners:** [N33 autograd/warmup](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md), [N33 master](33-MASTER-RP01-QUALITY-DIVERGENCE-AND-NOVELTY-2026-10-16.md), [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).
