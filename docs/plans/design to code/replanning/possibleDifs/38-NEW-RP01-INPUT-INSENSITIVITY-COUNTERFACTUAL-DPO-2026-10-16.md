# N38-A — Новый проверяемый механизм: DPO мог выучить приоритет ответа, не связь ответа с входным законом

**16.10.2026 · SOURCE-CONFIRMED DESIGN / NEW CAUSAL HYPOTHESIS / NATIVE PAIRWISE PROBE NOT_RUN / QUALITY EFFECT NOT_MEASURED.** Папка-назначение — только planning/possibleDifs. Защищённые core/plans/replanning-possible-diffs и frozen RP01 checkpoints/оценки не изменяются.

## Короткий смысл

RP01 может повысить вероятность формулировки «It writes … because …», сохраняя почти одинаковые предпочтения для **двух программ, у которых правильные ответы противоположны**. Это не идентично давно найденному общему префиксу ответов: здесь вопрос — научился ли Student зависеть от *изменения кода*, а не просто начинать текст, заканчивать предложение или повышать логит выбранного шаблона.

Гипотеза **H38-A / prompt-insensitive preference prior**: после 50 DPO updates изменение signed preference margin на тренировочных парах существенно больше, чем увеличение **контрфактической дискриминации** между предметно противоположными C# входами. В таком случае тренировочный loss может снижаться, а новые программы останутся неразличимыми для Student. **Нулевая гипотеза:** conditional preference действительно меняет знак между противоположными исходниками, даже если greedy/free decoder пока этого не показывает.

## Source evidence: что подтверждено

1. [Исторический C#500 materializer, semantic_case и make_record](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/.kiro/xiigen-slices/work/results/RP01-astra-synthetic-dev-csharp-500-20261015/materialize_dataset.py#L22-L255) детерминированно генерирует **25 семейств ×20 вариантов**. Примеры противоположных причин: приоритет умножения / скобки, postfix / prefix increment, оператор && / ||, строковая конкатенация и вычисление знаковых чисел. Все chosen имеют форму «It writes {good} because {yes}.», все rejected — «It writes {bad} because {no}.». Разметка взята из **simulation**, не из независимого преподавателя.
2. [DpoExactPairMaterial](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/Models/DpoExactPairMaterial.cs#L58-L92) раздельно кодирует prompt и response и строит response-only masks; [DpoTrainer.StepVectorCore](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L866-L914) оптимизирует **отношение вероятностей выбранного и отвергнутого response при конкретном prompt**. Из этого не следует, что learned response *различается на двух новых prompt*: такой контрфакт тренер не измеряет.
3. [Путь генерации Student](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L61-L76) использует свободный decoder. Сохранённый [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) фиксирует изменение LoRA/preference margin при free C#0/3→0/3. **Это не прямое доказательство H38-A**: требуются conditional log-probs на согласованной паре исходников.
4. Прежний [N27-PREFIX](27-RP01-DPO-COMMON-PREFIX-GENERATION-GAP-2026-10-16.md) уже проверил общий символьный ответный префикс 500/500 и предложил принудительно начать ответ; [DIF-206](02-DPO-DATA-OBJECTIVE-GRADIENT.md) перечисляет риски сегментации. **Новое здесь — cross-prompt counterfactual signed-margin interaction без принудительного начала генерации и без повторного DPO.** Это самостоятельная проверка learned input→law зависимости. Абсолютная уникальность среди всех 330 отчётов не заявлена.

## Одна причинная метрика, не подменяющая экзамен

Берём два исходника q0 и q1 с **одинаковым framing**, но с разными законными ответами y0 и y1. Для замороженной модели M определить:

- d(M,q) = logP_M(y0 | q) − logP_M(y1 | q), используя тот же неизменный tokenizer, response-only next-token alignment и одинаковый ответный бюджет; логарифмы суммировать в одном заранее выбранном objective.
- **Контрфактический разрыв:** C(M) = d(M,q0) − d(M,q1).
- **Эффект обучения:** G = C(C50) − C(B0), и отдельно G_ENRU = C(C100) − C(C50) только если используется реальная ENRU-compatible модель и source corpus.
- Признак предметной дискриминации: **d(M,q0)>0 и d(M,q1)<0**, а не один увеличившийся DPO reward margin. C>0 без двойного правильного знака — лишь слабый сигнал.
- Контроль априорного предпочтения: d(M,empty/framing-only) вне invalid пустого tokenizer prompt, то есть на том же корректном instruction prefix, но без предметного различителя; если оба знака одинаковы, модель может предпочитать текст y0 независимо от кода.

Строгое ограничение: response logprobs при разных длинах необходимо дополнительно показывать как **per-token mean** только для диагностики length bias; не изменять исходную формулу DPO и не смешивать mean и sum в главном G.

## Принадлежность доменам

| Домен | Осмысленный контрфакт | Нужные признаки | Что нельзя переносить |
|---|---|---|---|
| **C#/.NET — P0** | a+b*c ↔ (a+b)*c, x++ ↔ ++x, b&&f() ↔ b||f() с правильным исходным состоянием | AST, типы, порядок исполнения, оператор, результат | «похожий текст» не гарантирует эквивалентный закон; нужен проверенный stdout |
| **EN→RU — P1** | «the payment is approved» ↔ «the payment is not approved», he ↔ she в корректном контексте | отрицание, лицо/род, направление и Russian agreement | не судить перевод по C# шаблону и не использовать literal английский ответ |
| **SQL — переносимая гипотеза** | WHERE x IS NULL ↔ IS NOT NULL, AND ↔ OR | SQL AST и null semantics | NOT_RUN; отдельный разрешённый корпус |
| **HE↔EN — переносимая гипотеза** | отрицание / смена субъекта при явном target language | морфология и направленность | нельзя предполагать, что RP01 обучал этот домен |

Это **нейронный** тест способности отличить условия. Успех learned KNN/Unit в таких же задачах сравнивать отдельно, поскольку его input→nearest→Output реализует другой recipient.

## Приоритет и условие остановки

**P0 (read-only, без CI):** [MICRO-38-A](38-MICRO-COUNTERFACTUAL-CONDITIONING-EXPERIMENT-2026-10-16.md) — использовать сохранённые B0/C50, 6–12 открытых пар, никаких новых optimizer steps. Сначала native score identity, затем семантическая оценка; без раскрытия sealed exam. Если C50 не меняет знак ни в одной предметно контрастной паре и G≈0, не увеличивать число DPO пар на прежнем случайном B0 до изоляции warm-start/input conditioning. Если C50 меняет sign на source-disjoint парах, проблема может быть в decoder/начале ответа или доступе к checkpoint, а не отсутствии условного сигнала. В обоих случаях нужен предметно правильный ответ обычного consumer, не только margin.

**Статус:** код и экспериментальный генератор прочитаны; фактические C(B0), C(C50), G и native/semantic quality **ещё не измерены**.
