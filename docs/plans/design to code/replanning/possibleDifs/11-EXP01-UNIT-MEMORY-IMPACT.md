# EXP01 — влияет ли потеря обучаемой памяти Units на качество?
**XIIGEN RP01 · быстрый причинный эксперимент · план без выполненных измерений · 16 октября 2026**

## Вопрос и одна гипотеза

Исторический переводчик сохраняет выбранные ответы в ExecutorUnit: exact-source lookup, kNN, word/phrase maps и связанные с ними предметные ограничения. В текущем RP01 основной Student эксперимент обучает LoRA отдельного Transformer, а этот маршрут Unit memory не показан. **Гипотеза:** обучение старого ExecutorUnit действительно даёт дополнительные **правильные переводы на ранее не предъявленных вариантах фраз** при тех же правилах и тех же входах. Проверяем именно влияние записанной памяти — не DPO математику и не качество будущего production Student.

Нужны три самостоятельных вывода: (1) сработало ли прямое запоминание, (2) появился ли перенос на иную формулировку, (3) не привёл ли поиск к принятию ошибочного ответа на фразу противоположного смысла. Нулевой или отрицательный результат допустим и фиксируется как наблюдение, без подгонки проверочных строк.

## Исходный код, который уже существует

- [TranslationMachine.TrainStructuredBatchAsync:89–145](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L89-L145) действительно вызывает ITranslationUnitTrainer.TrainOnExample для каждой подходящей записи.
- [TranslationMachine.TrainBatchAsync:51–86](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L51-L86) — legacy scaffold: может повысить AccuracyAfter на 0.01, НЕ обучая Units.
- [ExecutorUnit.TrainOnExample:403–445](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L445) записывает chosen в exact memory, kNN и word maps.
- [ExecutorUnit.Execute:263–331](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L263-L331) читает обученную память на обычном запросе.
- [TranslationMachine.CreateRuntime:714–790](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L714-L790) связывает те же обучаемые Units с обычным TranslationRoutingFlow.
- [Работающий образец инициализации/восстановления/обычного запроса](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/tools/Rp01TranslationDomainLawContrast/Program.cs#L46-L150). Повторно использовать API, не создавать второй тренер.

## Сравнение четырёх состояний на одной версии кода

| Состояние | Начальное состояние | Изменение до оценки | Что проверяет |
| --- | --- | --- | --- |
| A — без обучения | Новый TranslationMachine | Никакого обучения | Baseline того же runtime/правил |
| B — legacy-псевдообучение | Отдельный новый TranslationMachine | Передать 4 публичные пары в TrainBatchAsync | Отрицательный контроль: внутренний accuracy +0.01 не равен навыку |
| C — обучаемая память | Третий новый TranslationMachine | Четыре TranslationLearningExample **только** для TranslationUnitIds.ExecutorEnRu; один TrainStructuredBatchAsync | Эффект записанной памяти при тех же tokenizer/verifier/law коде |
| D — холодное восстановление C | Новый instance/process | SaveCheckpointAsync(C), затем LoadCheckpointAsync; **без нового обучения** | Полноту реального сохранённого состояния |

В третьем экземпляре сначала запросить все 14 cases и сохранить C_before **до** обучения; затем однократно обучить и запросить те же 14 cases как C_after. Это добавляет реальную внутримодельную пару до/после, а не только сравнение двух новых экземпляров. Пять наблюдаемых состояний A/B/C_before/C_after/D ×14 = **70 ordinary inference outputs**. D загружать в отдельном новом процессе, а не просто новым instance в уже обучавшем процессе.

Важно: если near-template результат получен по канонизированному exact key (а не kNN или word map), маркировать **NORMALIZED_LOOKUP**, не приписывать его общему переносу нового закона; adapter ID должен показать actual path.

Четыре исходные пары синтетические и публичные: не имеют статуса китайского лицензированного автора DPO, никогда не подменяют frozen RP01 наборы. В этом опыте нет нейронных обновлений: A–D используют один механизм обычного TranslationMachine.GetOrchestratorAsync().RunAsync с source_language=en, target_language=ru, topic_domain=general.

Нельзя менять между arms SourceLaw, tokenizer, confidence floor, memory selection, grader, код модели, библиотечный commit или данные. В C обучается только ExecutorEnRu: SemanticTokenizerUnit и QualityVerifierUnit используют неизменённую обычную реализацию. Если C выигрывает, эффект относится к **обученной памяти Executor как механизму в целом**, а не автоматически отдельно к kNN.

## Исходные данные и контроль качества

[11-EXP01-CASES.json](11-EXP01-CASES.json) содержит 4 TRAIN и 14 открытых запросов: 4 exact memory controls, 4 near-template holdout (другой артикль, те же предмет/свойство/полярность), 4 opposite-polarity guard controls и 2 исследовательских композиции, которых не было в обучении. Near-template — **не новая независимая семантическая семья**; результат подтверждает максимум перенос внутри одной предметной закономерности. Открытый опыт не является будущим blind confirmation.

**Обязательная защита от утечки:** только массив train попадает в тренер; массив cases и expected ответы до окончания всех arms нельзя использовать при построении памяти. Один и тот же case id/исходный input запрашивается на A, B, C_before, C_after, D. Сохранить raw output, adapter ID, verdict, предупреждения, stop/fallback и SHA checkpoint. Нельзя использовать внутреннюю AccuracyAfter из TrainBatchAsync или измеренную unit coverage как внешнюю оценку качества.

**Основное измерение** — число дополнительных правильных near-template переводов, отсутствовавших у C_before и обоих A/B: количество случаев, в которых C_after_correct=true, C_before_correct=false, A_correct=false, B_correct=false. Отдельно показать число новых ошибочно принятых противоположных переводов (из 4), exact memorization (из 4), coverage/abstention и C_after↔D output equality на всех 14. Для публичных фраз использовать фиксированный семантический смысловой критерий (сохранение отрицания, действующего лица, свойства) плюс exact-match как диагностическую метрику. Подготовить per-case paired таблицу с ожидаемым и фактическим ответом.

Нулевой по содержанию ответ Bootstrap — **не** правильный перевод, даже если pipeline пометил его как технически успешную операцию. Для negative cases expected=null значит «в имеющейся тренировочной памяти нет допустимого подтверждённого ответа», а не «английскую фразу невозможно перевести вообще». Refusal/abstention безопаснее ошибочно принятого перевода; не превращать 100% coverage в самоцель.

## Предварительно объявленные вердикты

- **Подтверждён локальный полезный эффект памяти:** C даёт минимум один новый правильный ответ среди 4 near-template (лучше два и более), A/B его не дают; не возникает новых ошибочно принятых ответов из противоположных случаев; D на тех же входах воспроизводит C_after из cold checkpoint. Даже это НЕ означает, что LoRA плоха или RP01 провалился именно по этой причине.
- **Только запоминание:** улучшились exact, но не near-template. Memory write работает, перенос качества не доказан.
- **Вред retrieval:** выросло число правильных near, но добавились ложные accepted opposite-polarity — нельзя принимать как безопасное улучшение.
- **Нет наблюдаемого влияния:** near и exact не изменились; сначала проверить реальный training→state→serving consumer, не переписывать гипотезу задним числом.
- **Проблема восстановления:** C до save отличается от D после cold load; исследовать точный full checkpoint, не смешивать его с metadata-only ExportAsync.
- **Некорректная проверка:** разные inputs/правила, подмешивание gold в обучение, изменённые веса/пороги, повторно использованный output directory, не совпавшие исходные SHA — результат не засчитывается.

Никакой статистической значимости на 4 near examples не утверждать. Это быстрый diagnostic proof/falsifier конкретной функции, а не release gate.

## Как исполнитель запускает

Одна задача в [11-EXP01-EXECUTOR-WORK-ORDER.md](11-EXP01-EXECUTOR-WORK-ORDER.md). В core создать только небольшой tools/Rp01UnitMemoryLearningProbe (net9.0 + project reference на 27.TranslationDomain); использовать Program.cs существующего Rp01TranslationDomainLawContrast как пример. Не изменять действующие продукционные Units или прежние Student datasets/checkpoints. Команды **проектные; до написания runner НЕ существуют**:

    dotnet build tools/Rp01UnitMemoryLearningProbe/Rp01UnitMemoryLearningProbe.csproj -c Release
    dotnet run --project tools/Rp01UnitMemoryLearningProbe/Rp01UnitMemoryLearningProbe.csproj -c Release -- --fixture <cases.json> --out <new-unique-results-dir> --phase train-evaluate
    dotnet run --project tools/Rp01UnitMemoryLearningProbe/Rp01UnitMemoryLearningProbe.csproj -c Release -- --fixture <cases.json> --out <same-results-dir> --phase restore-evaluate

Результат должен включать: input/source SHA; 70 actual raw answers (14×5); learning mutation receipt только для C; checkpoint bytes SHA для D; per-case матрицу A/B/C_before/C_after/D; primary near correct, negative false accepted, exact sanity, abstain; отдельные timings/memory для этой локальной программы; понятный человеческий вывод. External model calls 0, native DPO training steps 0. Не запускать full Windows CI/всю Solution.

## Что следует после положительного результата

Для утверждения, что это **причина отсутствия качества RP01**, нужен второй, отдельный paired опыт: те же новые запросы и тот же frozen RP01 C100 Student, только обычный EN→RU consumer с выключенной/включённой веткой обученной memory. Никакой новый baseline model/checkpoint не подмешивать. Если выигрыш даёт только deterministic guard + memory, классифицировать как улучшение **гибридного продукта**, а не нейронного DPO-learning. До такого опыта причина RP01 остаётся гипотезой.

Другие короткие испытания: [11-OTHER-CAUSAL-MICROPROBES.md](11-OTHER-CAUSAL-MICROPROBES.md).