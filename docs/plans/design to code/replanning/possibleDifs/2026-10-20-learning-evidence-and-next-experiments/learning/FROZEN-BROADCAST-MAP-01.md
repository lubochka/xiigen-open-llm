# FROZEN-BROADCAST-MAP-01 — не хранить backward-индексы замороженного forward

**Статус:** подтверждён участок исходника; малый эксперимент описан, не создан и не запускался. Измеренный успех не заявлен. **Тема:** документация C#. **Обучается:** прежний Student q/v-LoRA. **Полезная метрика:** выделенные байты за один настоящий DPO update; рядом учебные показатели, состояние и остальные расходы.

Это **новое уточнение известной семьи RMS**, а не повтор RMS-COORD-ALLOC-01. Точная модель/host/данные/команды/отчёт назначены [договором исполнения](TENSOR-LEARNING-EXECUTION-CONTRACT.md), который является частью этой карточки. Порядок задаёт [единый реестр](../04-execution-registry.md): исходные №07–15 → №17–31 → ROPE-TRAIN-TABLE-01 → AST-SIB-CACHE-01 → NG01 → BPE-REALIZED-MERGE-COUNT-01 → GRAD-ACCUM-COPY-01 → FROZEN-BROADCAST-MAP-01 → IDENTITY-M1-01 (исходный №16) → KN-ROUTER-LEARNED-KEY-01 (исходный №32) → END. Всего 38 строк: 6 завершены, 32 ожидают. «Дальше» этой карточки — IDENTITY-M1-01 в порученной фазе, без возврата к прежним опытам. Текущий следующий всей очереди — LF-03, исходный №07; завершённые №01–06 не повторяются.

## Непосредственная связь с обучением

[StepVectorCore:879–889][reference] вычисляет chosen/rejected reference log-probs до policy update. [Simulation owner:110–112][freeze] создаёт отдельную reference-модель и замораживает все её параметры. [LlmModel.Forward:219–242][model] проходит Transformer blocks и final RmsNorm; [RmsNorm:29–40][rms] вызывает `MultiplyBroadcast` для нормировки и gamma. В [MultiplyBroadcast:168–185][broadcast-map] `mappedBIndices = new int[data.Length]` выделяется и заполняется всегда. Его последующее чтение существует только в [gradient closure:187–210][broadcast-back], который при обоих `RequiresGrad == false` не создаётся.

**Гипотеза:** не создавать этот массив в полностью замороженном forward уменьшит allocated bytes одинакового DPO update без изменения reference log-probs, policy-gradient, состояния обучения или учебного readout. Это необходимые reference-вычисления настоящего DPO, а не отдельный inference benchmark. Возможные дополнительные вызовы той же no-grad ветви в policy учитываются по фактическому пути; экономия не приписывается только reference по предположению.

## Единственный A/B-фактор

A — полный TensorOps по core `883496bca18113bf37e0afe683c75b05a0075048`. B — только в `MultiplyBroadcast(Tensor,Tensor,CancellationToken)` после существующей проверки размеров и создания `data` добавить no-grad ветвь:

```csharp
if (!a.RequiresGrad && !b.RequiresGrad)
{
    for (int flat = 0; flat < data.Length; flat++)
    {
        if ((flat & 4095) == 0)
            cancellationToken.ThrowIfCancellationRequested();
        var indices = FlatToIndices(flat, a.Shape);
        int bFlat = 0;
        for (int dimension = 0; dimension < b.Shape.Rank; dimension++)
        {
            int bDimension = b.Shape.Dimensions[dimension];
            int bIndex = bDimension == 1 ? 0 : indices[rankOffset + dimension];
            bFlat = bFlat * bDimension + bIndex;
        }
        data[flat] = a.Data[flat] * b.Data[bFlat];
    }
    return new Tensor(data, a.Shape);
}
// Ниже исходная ветвь с mappedBIndices и backward целиком без изменений.
```

`FlatToIndices` и все его allocations **остаются исходными**. Сохранить тот же float multiply, right alignment, scalar/rank shape semantics, cancellation cadence, TensorStorage defensive copy; gradient path не менять. Нельзя добавлять global no-grad mode, cache reference logits, пропуск reference forward, pooling, RMS fusion, изменение gamma или RMS-COORD-ALLOC-01. При этом реальном owner reference flags фиксируются до StepVector; конкурентное изменение `RequiresGrad`/tensor data не входит в договор сравнения.

Удаляемый теоретический payload — сумма `4 * a.Shape.TotalElements` **только по достигнутым MultiplyBroadcast с обоими RequiresGrad=false**. Это не замер peak RAM и не число «параметры × шаги». Не добавлять per-element counters в горячий цикл; actual count/shape при необходимости получать одинаковым входным observer в обоих arms, его расходы объявить. Значения predicted bytes можно оставить расчётом по сохранённым shapes; решение об успехе принимает измеренная стоимость полного StepVector.

## Точное исполнение и before/after

Физический workspace `C:/xg/FBM1`, source `src`, host `host`, результаты `results`; логическая принадлежность существующему экспериментальному проекту `35.ExperimentsV2/Experiments/Rp01Micro/FBM1`. Четыре native source-copy библиотеки и bridge — §4–5 [договора](TENSOR-LEARNING-EXECUTION-CONTRACT.md). Source-copy/runner пока не созданы; это назначенная работа будущего исполнения, не поиск проекта.

Данные — неизменная `simulation-csharp-doc-f06-008`; один B0, independent frozen reference, fresh m/v0; 1 StepVector/arm. Весь A/B: 2 optimizer calls, 4 TRAIN policy forwards, 4 reference forwards, 4 backwards и отдельно диагностический бюджет §6 договора. На одной и той же TRAIN-панели измерить все назначенные показатели A0→A1 и B0→B1. Метрики, возвращённые StepVector, относятся к pre-update и не подменяют A1/B1. Свободная генерация/EXAM не назначены. Все pins, source admission, native lease и final readback — §2–6 договора.

После подготовки ровно одна команда:

```powershell
pwsh -NoProfile -File 'C:/xg/FBM1/host/Run.ps1'
```

Записать полные cumulative counters и segment deltas StepVector, CPU, wall, allocated bytes, GC, retained heap и process peak раздельно. Сравнить final live/saved LoRA/base/reference/optimizer/RNG и назначенные учебные readouts до/после. Не считать исходно frozen reference «не связанной с обучением»: её log-probs прямо входят в DPO objective. Статический ожидаемый parity ещё не является runtime parity.

## Успех и отрицательный результат

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен.** Показать остальные изменения, включая ухудшения. Меньшие allocations при равном TRAIN-сигнале полезны; это не доказывает уменьшение peak RAM или улучшение ответа. Одно A→B без повторов оставляет причинность wall-time/CPU/process peak неустановленной. Нельзя объявить более короткий незавершённый run ускорением одинакового обучения.

Если states/logits разошлись, ресурсное наблюдение сохранить, но гипотеза эквивалентной оптимизации не подтверждена. Если одинаковый update достигнут, а выигрыша нет, записать честный отрицательный результат. Если authority/loader/build отказал раньше learner, это «обучение не началось», а не нулевая обучаемость.

При полезном результате обычного завершения — `C:/xg/FBM1/results/report.md` по §8 договора и `candidate.md` со source target `00.TensorCore/Core/TensorOps.cs::MultiplyBroadcast`, единственной no-grad ветвью и ухудшениями. Кандидат для решения Любы; production и protected required-changes не меняются.

## Немедленный переход/стоп — запрет повторения прежней ошибки

**«Заканчивай этот опыт и переходи к следующему» означает немедленный переход к уже назначенному следующему заданию в разрешённой фазе.** Запрещено сначала дописывать прошлый отчёт, карточку, реестр, паспорт или команды; считать hashes; проверять Git, публикацию, навыки или результаты; выполнять ещё один review, тест, повторный запуск или «последнюю полезную проверку». Нельзя четыре раза объявлять завершение и продолжать прежнюю работу. **«Остановить всё» означает остановить работу полностью.** Для собственного ещё работающего процесса допустимо только необходимое действие остановки. Незавершённые файлы и данные оставить как есть; краткий статус — только по уже известным фактам, без новых инструментов. Это правило выше оформления. Здесь следующий пункт — IDENTITY-M1-01 в порученной фазе: не возвращаться к AGC1, RMS или старым отчётам.

## Готовый промпт исполнителю

> Выполни только назначенную фазу FROZEN-BROADCAST-MAP-01 по этой карточке и TENSOR-LEARNING-EXECUTION-CONTRACT.md. Сейчас статус документальный, обучения ещё нет; при порученном bounded execution подготовь только C:/xg/FBM1, четыре source-copy библиотеки, bridge и Run.ps1. Твоя модель — текущая модель сессии по умолчанию, без override и поиска другой модели; reviewer последовательный и без второго A/B. Навыки и русский report skeleton заданы §8 договора. Единственный B diff — отсутствие mappedBIndices в обоих RequiresGrad=false; FlatToIndices/градиентная ветвь/float arithmetic/cancellation остаются исходными. Не присоединяй RMS-COORD или AGC1. Одна назначенная simulation-пара, один настоящий StepVector на arm, всего 2 updates и отдельно 4 DIAG_TRAIN forwards; точный DIAG_TRAIN_PANEL до/после по §6. Не называй pre-update StepVector return post-update результатом. Измерь A0/A1/B0/B1 и цену TRAIN с ухудшениями; любое полезное изменение — успех по оси, accuracy не обязательна. Theory bytes не замер. Никакого product pipeline/тестов/CI/нового teacher/другого B0. Existing claim/results не удаляй, не подбирай новый RunId; техническую ошибку запиши без подмены опыта. **«Заканчивай этот опыт и переходи к следующему» означает немедленный переход к уже назначенному следующему заданию в разрешённой фазе.** Запрещено сначала дописывать прошлый отчёт, карточку, реестр, паспорт или команды; считать hashes; проверять Git, публикацию, навыки или результаты; выполнять ещё один review, тест, повторный запуск или «последнюю полезную проверку». Нельзя четыре раза объявлять завершение и продолжать прежнюю работу. **«Остановить всё» означает остановить работу полностью.** Для собственного ещё работающего процесса допустимо только необходимое действие остановки. Незавершённые файлы и данные оставить как есть; краткий статус — только по уже известным фактам, без новых инструментов. Это правило выше оформления; следующая карточка IDENTITY-M1-01 в порученной фазе по единому реестру, не возвращайся к старым карточкам. Только при обычном завершении создай русский report.md и при полезном выигрыше candidate.md; перенос в production решает Люба.

## Отличие от прежних карточек

Известный [RMS-COORD-ALLOC-01][prior-rms] оптимизирует per-element координатный массив `FlatToIndices` и прямо сохраняет `mappedBIndices`. Данный опыт сохраняет координатный алгоритм, но не удерживает обратное отображение, когда backward отсутствует. Поэтому это добавочный фактор внутри ранее найденной семьи, не переименование старого опыта и не общее открытие «RMS выделяет память». Фактор LF-03/AccumulateGrad относится к autograd и также сюда не переносится. Поиск 747 planning и 334 protected Markdown, ближайшие совпадения и границы утверждения — [протокол новизны](learning-novelty.json).

**Дополнение по concurrent planning `53cbcabd3926cda714bab428d294eb666301e9e4`:** содержательно сверены ROPE-TRAIN-TABLE-01, AST-SIB-CACHE-01, SKNN-SIBLING-PREP-01, BPE-REALIZED-MERGE-COUNT-01 и `12-source-and-novelty-ledger.md`; root дополнительно выполнил буквальный поиск по всем 53 добавленным или изменённым файлам. Точное вмешательство no-grad `mappedBIndices` не найдено: FROZEN остаётся отдельным уточнением известной RMS-семьи, а не новым открытием всего механизма. Это адресное расширение прежнего аудита, не повторное ручное прочтение всего latest-корпуса; исходный JSON-квитанция 747-файлового снимка не изменён. [Граница и источники сверки](learning-evidence.md#дополнение-после-concurrent-planning-53cb).

[reference]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L879-L889
[freeze]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L110-L112
[model]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/03.TransformerModel/LlmModel.cs#L219-L242
[rms]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/03.TransformerModel/Layers/RmsNorm.cs#L29-L40
[broadcast-map]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorOps.cs#L168-L185
[broadcast-back]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorOps.cs#L187-L210
[prior-rms]: https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-causal-delta-audit/RMS-COORD-ALLOC-01-CARD.md

**Уточнение core669:** N62-R2 уже завершён, повтор запрещён; NEXT общей программы LF-03. Source-copy этих двух факторов остаётся на883: [оба Tensor-файла идентичны669, R2 меняет другой механизм](CORE669-DELTA.md).

**Финальный статус827:** №01–06 completed, NEXT=LF-03; [реестр](../04-execution-registry.md). TensorOps/TensorStorage на827 имеют те же blobs, что883/669; заданный source-copy pin883 не меняется.
