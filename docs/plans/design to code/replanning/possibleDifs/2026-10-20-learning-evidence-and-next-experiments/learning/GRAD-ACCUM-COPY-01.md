# GRAD-ACCUM-COPY-01 — убрать лишнюю копию первого вклада градиента

**Статус:** подтверждена строка исходника; малый эксперимент описан, не создан и не запускался. Измеренный успех не заявлен. **Тема:** документация C#. **Обучается:** прежний Student q/v-LoRA. **Полезная метрика:** выделенные байты за один настоящий DPO update; рядом TRAIN-сигнал, состояния и другие расходы.

Это дополнение к [единому реестру](../04-execution-registry.md), не продолжение LF-03 и не повтор завершённого опыта. Точная модель/host/данные/команды/отчёт назначены [договором исполнения](TENSOR-LEARNING-EXECUTION-CONTRACT.md), который является частью этой карточки. Единая очередь: исходные №07–15 → №17–31 → ROPE-TRAIN-TABLE-01 → AST-SIB-CACHE-01 → NG01 → BPE-REALIZED-MERGE-COUNT-01 → GRAD-ACCUM-COPY-01 → FROZEN-BROADCAST-MAP-01 → IDENTITY-M1-01 (исходный №16) → KN-ROUTER-LEARNED-KEY-01 (исходный №32) → END. Всего 38 строк: 6 завершены, 32 ожидают. «Дальше» этой карточки — FROZEN-BROADCAST-MAP-01 в порученной фазе. Текущий следующий всей очереди — LF-03, исходный №07; завершённые №01–06 не повторяются.

## Непосредственная связь с обучением

[DpoTrainer.StepVectorCore][step] делает policy forward обеих сторон, [BackwardLogProbs][backward] запускает `logits.Backward`, backward-функции слоя вызывают [TensorOps.AccumulateGrad][accum], затем один AdamW update. При первом поступлении градиента текущий метод создаёт внешний `ToArray`; [Tensor constructor][ctor] передаёт массив в [TensorStorage][storage], который делает ещё один `ToArray`. Поэтому это затраты каждого достигнутого первого вклада в autograd, включая промежуточные узлы двух policy-графов; не только две начальные logits-матрицы.

**Гипотеза:** удаление внешней копии в этом месте уменьшит allocated bytes одинакового учебного перехода, сохранив независимое владение массивом градиента, порядок его накопления, значения weights и learning readout. Выигрыш по wall-time или peak RAM не обещается.

## Единственный A/B-фактор

A — полный исходник TensorOps по core `883496bca18113bf37e0afe683c75b05a0075048`. B — только следующий diff в экспериментальной копии:

```diff
 if (target.Grad == null)
 {
-    target.Grad = new Tensor(gradToAdd.Data.ToArray(), gradToAdd.Shape);
+    target.Grad = new Tensor(gradToAdd.Data, gradToAdd.Shape);
 }
```

Сохранить `if(!target.RequiresGrad) return`, ветвь `+=`, исходные Tensor/TensorStorage, формы, граф/порядок обхода, masks и optimizer. Нельзя `target.Grad=gradToAdd`, no-copy storage, pooled/shared массив, менять ZeroGrad, присоединять LF-03 initial-gradient изменение или другие оптимизации. В B остаётся defensive copy конструктора; прежний внешний alias не удерживается. Для конкурентной мутации `gradToAdd.Data` исходный код не предоставлял snapshot-транзакцию; данный опыт использует существующий последовательный policy lease без concurrent tensor writes.

Ожидаемый удаляемый payload равен сумме `4 * gradToAdd.Data.Length` **по фактически достигнутым вызовам с `target.Grad == null`**. Это расчёт, не measured RAM. Параметры после ZeroGrad могут иметь уже существующий carrier, а новые промежуточные узлы — нет; не умножать размеры всех параметров на число уроков без трассы. Никакой отдельный copy microbenchmark не засчитывается.

## Точное исполнение и before/after

Физический workspace `C:/xg/AGC1`, source `src`, host `host`, результаты `results`; логическая принадлежность существующему экспериментальному проекту `35.ExperimentsV2/Experiments/Rp01Micro/AGC1`. Четыре native source-copy библиотеки и bridge — §4–5 [договора](TENSOR-LEARNING-EXECUTION-CONTRACT.md). Source-copy/runner пока не созданы; это конкретная инженерная работа будущего исполнения, не поиск проекта.

Данные — неизменная `simulation-csharp-doc-f06-008`; один B0, independent frozen reference, fresh m/v0; 1 StepVector/arm. Весь A/B: 2 optimizer calls, 4 TRAIN policy forwards, 4 reference forwards, 4 backwards и отдельно 4 DIAG_TRAIN policy forwards. **DIAG_TRAIN_PANEL** по pinned protocol24: chosen TRAIN NLL/full gold rank/token accuracy/teacher-forced exact снимаются до и после по одной и той же response-mask из тех же logits; точные формулы/raw-поля — §6 договора. Возвращённые StepVector loss/margin — `TRAIN_STEP_PRE_UPDATE`, не A1/B1; дополнительный native DPO post-update loss/margin не измеряется. Свободная генерация/EXAM не назначены. Все детали выбранной модели, pins, source admission, native lease и final readback — §2–6 договора; заменять их нельзя.

После подготовки ровно одна команда:

```powershell
pwsh -NoProfile -File 'C:/xg/AGC1/host/Run.ps1'
```

Сравниваются A0→A1 и B0→B1, а стоимость — цена этих двух одинаковых реальных переходов. Записать накопленные counters и deltas полного StepVector, CPU, wall, allocated bytes, GC, retained heap и process peak раздельно; final live/saved state, pre/post readout. Source expectation — прежние значения; runtime parity ещё не проверялась. Ресурсный эффект при несовпавших states показывается, но чистота эквивалентной оптимизации тогда не подтверждена.

## Успех и отрицательный результат

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен.** Рядом обязательны остальные дельты, включая ухудшения. Меньшие allocations при неизменном TRAIN NLL полезны. Меньшие allocations не означают меньше peak RAM; более короткий незавершённый run не означает ускорение. Нулевая экономия при достигнутом одинаковом update — честный результат; ошибка сборки/authority/loader до learner — не нулевая эффективность обучения.

При полезном результате обычного завершения — `C:/xg/AGC1/results/report.md` по §8 договора и `candidate.md` со source target `00.TensorCore/Core/TensorOps.cs::AccumulateGrad`, точным diff и ухудшениями. Кандидат для решения Любы; production и protected required-changes не меняются.

## Немедленный переход/стоп — запрет повторения прежней ошибки

**«Заканчивай этот опыт и переходи к следующему» означает немедленный переход к уже назначенному следующему заданию в разрешённой фазе.** Запрещено сначала дописывать прошлый отчёт, карточку, реестр, паспорт или команды; считать hashes; проверять Git, публикацию, навыки или результаты; выполнять ещё один review, тест, повторный запуск или «последнюю полезную проверку». Нельзя четыре раза объявлять завершение и продолжать прежнюю работу. **«Остановить всё» означает остановить работу полностью.** Для собственного ещё работающего процесса допустимо только необходимое действие остановки. Незавершённые файлы и данные оставить как есть; краткий статус — только по уже известным фактам, без новых инструментов. Это правило выше оформления. Здесь следующая карточка — FROZEN-BROADCAST-MAP-01, в порученной фазе; документное поручение не разрешает запуск обучения.

## Готовый промпт исполнителю

> Выполни только назначенную фазу GRAD-ACCUM-COPY-01 по этой карточке и TENSOR-LEARNING-EXECUTION-CONTRACT.md. Сейчас статус документальный, обучения ещё нет; при порученном bounded execution подготовь только C:/xg/AGC1 и назначенные четыре source-copy библиотеки, bridge и Run.ps1. Твоя модель — текущая модель сессии по умолчанию, без override и поиска другой модели; reviewer последовательный и без повторного A/B. Навыки и русский report skeleton заданы §8 договора. Единственный B diff — внешний ToArray первого AccumulateGrad; TensorStorage defensive copy и Tensor.Backward(initialGradient) сохраняются. Одна назначенная simulation-пара, один настоящий StepVector на arm, общий бюджет 2 updates и 4 DIAG_TRAIN forwards. Для DIAG_TRAIN_PANEL все назначенные метрики из тех же logits/masks до/после по §6; StepVector return — PRE_UPDATE, не after. Измерь A0/A1/B0/B1, фактическую цену TRAIN и все ухудшения; любое полезное изменение — успех по оси, accuracy не обязательна. Не путай theory bytes с измерением. Никакого product pipeline/тестов/CI/нового teacher/другого B0. Existing claim/results не удаляй, не подбирай новый RunId; техническую ошибку запиши без подмены опыта. **«Заканчивай этот опыт и переходи к следующему» означает немедленный переход к уже назначенному следующему заданию в разрешённой фазе.** Запрещено сначала дописывать прошлый отчёт, карточку, реестр, паспорт или команды; считать hashes; проверять Git, публикацию, навыки или результаты; выполнять ещё один review, тест, повторный запуск или «последнюю полезную проверку». Нельзя четыре раза объявлять завершение и продолжать прежнюю работу. **«Остановить всё» означает остановить работу полностью.** Для собственного ещё работающего процесса допустимо только необходимое действие остановки. Незавершённые файлы и данные оставить как есть; краткий статус — только по уже известным фактам, без новых инструментов. Это правило выше оформления; следующая карточка FROZEN-BROADCAST-MAP-01 в порученной фазе. Только при обычном завершении создай русский report.md и при полезном выигрыше candidate.md; затем конец данного ID, перенос в production решает Люба.

## Отличие от прежних карточек

Известный [LF-03][lf] меняет `Tensor.Backward(Tensor)` — копирование начального logits-gradient перед графом. Данный фактор меняет первое принятие gradient в каждом достигнутом producer. Его не объединять с LF-03 и не засчитывать дважды. Поиск `gradToAdd` и сочетаний AccumulateGrad/copy/alloc/ToArray в 747 planning Markdown дал0 точных обсуждений фактора; protected 334 Markdown содержат correctness/gradB mentions, без этого A/B. [Протокол новизны](learning-novelty.json).

**Дополнение по concurrent planning `53cbcabd3926cda714bab428d294eb666301e9e4`:** содержательно сверены ROPE-TRAIN-TABLE-01, AST-SIB-CACHE-01, SKNN-SIBLING-PREP-01, BPE-REALIZED-MERGE-COUNT-01 и `12-source-and-novelty-ledger.md`; root дополнительно выполнил буквальный поиск по всем 53 добавленным или изменённым файлам. Точное вмешательство первого `AccumulateGrad` не найдено: GRAD остаётся отдельным новым фактором в проверенном корпусе. Это адресное расширение прежнего аудита, не повторное ручное прочтение всего latest-корпуса; исходный JSON-квитанция 747-файлового снимка не изменён. [Граница и источники сверки](learning-evidence.md#дополнение-после-concurrent-planning-53cb).

[step]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L832-L943
[backward]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L1072-L1121
[accum]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorOps.cs#L984-L997
[ctor]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/Tensor.cs#L39-L46
[storage]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorStorage.cs#L21-L24
[lf]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#queue-lf03

**Уточнение core669:** N62-R2 уже завершён, повтор запрещён; NEXT общей программы LF-03. Source-copy этих двух факторов остаётся на883: [оба Tensor-файла идентичны669, R2 меняет другой механизм](CORE669-DELTA.md).

**Финальный статус827:** №01–06 completed, NEXT=LF-03; [реестр](../04-execution-registry.md). TensorOps/TensorStorage на827 имеют те же blobs, что883/669; заданный source-copy pin883 не меняется.
