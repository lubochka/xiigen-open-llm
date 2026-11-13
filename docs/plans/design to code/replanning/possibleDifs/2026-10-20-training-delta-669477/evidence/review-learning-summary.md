# Независимый смысловой review итоговых 01/02

Проверены только `01-learning-differences.md` и `02-measured-candidates.md` итогового пакета. Source и старые receipts использованы адресно; runtime, обучение и полный повторный аудит не выполнялись. Три конкретных уточнения ниже относятся к 01; в 02 нового фактического замечания в проверенном scope нет.

## 1. Embedding autograd описан без обязательного условия

**Место:** `01-learning-differences.md`, строка 71; согласовать также формулировку в строке 49.

Сейчас «Embedding output связан с Weight через graph» звучит безусловно. В [Embedding.cs:44–62](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Layers/Embedding.cs#L44-L62) и `requiresGrad`, и добавление `Children`, и `BackwardFunc` зависят от **`Weight.Value.RequiresGrad`**. Для frozen base отсутствие градиента embedding штатно; эту проверку нельзя прочесть как доказательство, что RP01 учил сами embeddings.

**Точная замена строки 71:** «При `Weight.Value.RequiresGrad=true` Embedding связывает output с Weight; repeated IDs суммируют градиент. При frozen Weight этот backward намеренно не строится».

**Уточнение строки 49:** «В текущем исходнике low-rank forward подключён; fractional mask участвует в backward, а autograd embedding реализован для обучаемого Weight». Это описывает увиденный source и не расширяет вывод до всех режимов или истории исправления.

## 2. Порядок вызовов verifier слегка перевёрнут

**Место:** `01-learning-differences.md`, строка 88: «Эта строка действительно поступает в TrainOnExample».

В native `TrainOnExample` поступает `TranslationLearningExample` с полным issue. Сам `TrainOnExample` вызывает `TryParseRequiredTerm`, получает уже обрезанный `req` и записывает его в marker map: [TranslationTrainedUnits.cs:1100–1143](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationTrainedUnits.cs#L1100-L1143).

**Точная замена:** «`TryParseRequiredTerm`, вызываемый внутри `TrainOnExample`, возвращает `brother`; затем этот же `TrainOnExample` записывает обрезанный `req` в marker rules».

Причинный вывод остаётся прежним; исправляется только точность producer/caller, чтобы читатель не искал отдельный предпроцессор перед Train.

## 3. RMS-бюджет в кратком описании требует “на плечо”

**Место:** `01-learning-differences.md`, строка 94: «с реальными четырьмя SFT/LoRA updates».

Карточка задаёт 4 updates **на плечо**, всего 8. Сейчас краткую фразу можно прочитать как общий бюджет либо уже исполненное обучение.

**Точная замена:** «Будущий опыт предусматривает по 4 настоящих SFT/LoRA updates на плечо, всего 8; точный состав 36 файлов, A0/A1/B0/B1 TRAIN/EXAM и единственный короткий путь заданы в карточке». Статус остаётся NOT_RUN.

## Адресно проверенное без замечаний

R1 в 02 исправлен верно: исходный [DpoTrainer.cs:260–261](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/11.DPO/DpoTrainer.cs#L260-L261) содержит ленивые `Select(BitConverter.SingleToInt32Bits).SequenceEqual(...)`, **не `ToArray`**. Описание `MemoryMarshal.AsBytes(...).SequenceEqual(...)`, побитовой семантики, малой allocation-разницы и неопределённости причинного ускорения соответствует source и сохранённому отчёту. Метод `DpoProjectLoraRecipient.RequireValid` действительно находится внутри `11.DPO/DpoTrainer.cs`.

Claims LoRA forward, fractional-mask multiplication и SFT→backward→AdamW подтверждаются названными текущими методами. Нулевой free-answer RP01 не объявлен отсутствием обучения или доказанной единственной причиной; полезные margin/resource gains сохраняются без accuracy-порога. Профили BPE названы профилями материала, не доказательством улучшения итогового перевода. Дополнительного обучения/переписывания raw для этих трёх редакционных уточнений не требуется.
