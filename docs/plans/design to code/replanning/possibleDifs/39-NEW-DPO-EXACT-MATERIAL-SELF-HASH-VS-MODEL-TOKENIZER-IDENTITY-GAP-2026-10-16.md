# N39 — DPO exact-v2: самопроверка tokenizer SHA не равна привязке пары к tokenizer загруженного Student

<!-- CODEX-POST128-PLANNING:START -->
## Действующее уточнение после P128 — Проверенный материал должен дойти своему получателю

Проверенный core: `f8784df037b59cf878889eef4d0956f5d0c985e7`; planning: `5df24354aa803f7ed5dd3bac48146466e2feb189`. Это адресное усиление приёмки соответствующего класса, а не новая аттестация всех исторических опытов карточки.

Actual content hash, tokenizer identity, family и владелец изменяемых весов — отдельные оси. Проверить оба входа конструктора DpoTrainer, область ledger и реальный Issue. Контроль разных объектов одного checkpoint не подменять разными строками ref-alpha/ref-beta. Честная пара owner A/recipient A проходит; подстановка B и повторная capability чужого owner над A отвергаются до optimizer step; явное sharing разрешается отдельно. Для материала одинаковое содержание одной семьи не стирает IDs через rows[0]; role берётся у верифицированной доверенной строки. Новая продуктовая нативная приёмка не равна повторению P121 или расширению научного бюджета.

**Передать исполнителю вместе с собственным паспортом:** проследить названный actual consumer, сначала принять полный honest пакет, затем отвергнуть одну адресную порчу тем же checker; результат включить в общий verdict и внешний exit. Указать источник/сборку, фактический расход каждой попытки и предел утверждения. Ремонт expected опирается на независимый договор, не на actual сломанного кода. Сохранённый PRIMARY не повторять; gold, исходные бюджеты и прошлые результаты не менять. Активная попытка сохраняет ранее выданный ей pin карточки. Native source/static counterexample и исполненный тест обозначать отдельно.

[Сводка нового аудита](codex-2026-11-03-POST128-TRANSFER-AND-EXPERIMENT-AUDIT.md) · [действующая core-карточка](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P121-project-adapter-selection.md#current-executor-task). Историческое содержание ниже сохранено.
<!-- CODEX-POST128-PLANNING:END -->

**16.10.2026 · SOURCE_CONFIRMED CONDITIONAL CONTRACT GAP / QUALITY_CAUSALITY_NOT_RUN / .NET_NOT_RUN.**
Core source pin: [llm_mvp_core@d1862939bfe24c3bbeb12a49af917c7ca569a10f](https://github.com/lubochka/llm_mvp_core/tree/d1862939bfe24c3bbeb12a49af917c7ca569a10f). Изменения внесены только в planning `possibleDifs`. Protected `core/docs/plans/replanning-possible-diffs` READ ONLY. Это **другой seam**, чем известный N21 BPE retrain/reindex generic KNN, N28 SemanticTokenizer→Executor key drift, N16 legacy-hash Tokenizer64 и N38 replay fusion. Полный независимый novelty review сотен документов не выполнялся.

## Почему обучение может портиться, даже когда «точный материал» валиден

Токен ID, например 257, означает разные байтовые последовательности, если два BPE словаря независимо обучены с одинаковым `VocabSize`, но разным порядком merge. Модель связывает token ID со своим embedding. Если DPO-тензоры изготовлены по словарю B, а загруженная Student/reference модель должна интерпретировать IDs по словарю A, вычисление градиента математически корректно **по индексам** и при этом может учить предпочтение **не тех** фрагментов языка/кода. Хеш и round-trip pair tokenizer B могут быть идеально валидны — вопрос в соответствии A↔B.

## Проверенное source evidence: чей SHA и где он сравнивается

| Узел | Source confirmed C | Что НЕ проверено |
| --- | --- | --- |
| `BpeTokenizer.Train` | словарь из 256 byte IDs расширяется merge IDs; `ComputeStateSha256()` хеширует canonical state; `VocabSize` только количество token IDs | одно число `VocabSize` не гарантирует одинаковые словари | 
| `DpoExactPairMaterial.CreateBatch` | доверенный producer делает snapshot собственного `BpeTokenizer`, кодирует prompt/chosen/rejected и сохраняет `TokenizerSha256` и `TokenizerVocabSize` | в этом методе нет объекта loaded Student tokenizer для взаимного сравнения |
| `DpoExactPairMaterial.CaptureAndValidate` | принимает optional `expectedTokenizerSha256 = null` и сравнивает SHA **только если он передан**; иначе валидирует собственные ID/mask/digest, total vocab bounds | не требует default-проверки совместимости с loaded model |
| `DpoTrainer.CaptureBatch` | вызывает `CaptureAndValidate(batch, Min(policy.VocabSize,ref.VocabSize), Min(policy.ContextLen,ref.ContextLen))` **тремя аргументами** | ожидаемый SHA токенизатора Student не подставлен |
| `DpoExecutionProfile` | profile хранит material/framing/objective versions и allowed parameter map, `Matches` сравнивает их | SHA tokenizer/material конкретной pair и owner model tokenizer в самом profile отсутствует |

**Точные ссылки на код:**
- [BpeTokenizer Train/merge, L34–71, and ComputeStateSha256 L120–127](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/01.Tokenizer/BpeTokenizer.cs#L34-L127).
- [DpoExactPairMaterial L27–89 (снимок BPE + self digest)](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/11.DPO/Models/DpoExactPairMaterial.cs#L27-L89).
- [DpoExactPairMaterial L91–145 (optional expectedTokenizerSha256 and same-size ID bounds)](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/11.DPO/Models/DpoExactPairMaterial.cs#L91-L145).
- [DpoTrainer.CaptureBatch L991–1000 — не передан expected SHA](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/11.DPO/DpoTrainer.cs#L991-L1000).
- [DpoExecutionProfile L120–151 — в identity нет tokenizer hash](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/11.DPO/DpoTrainer.cs#L120-L151).

**Строгая граница:** это **отсутствие invariant на данном уровне abstraction**, а НЕ доказательство, что реальный RP01 использовал второй словарь. В RP01 producer использует один pinned tokenizer, receipt мог проверить SHA **на другом уровне**. Надо прочесть actual config/artifacts и runner; если они гарантируют идентичный A==B, N39 **не причина** frozen C#0/3 / ENRU0/4. Иное отсутствие проверки в локальном trainer нельзя без caller trace обобщить на весь ProductHost. Project server admission и continuation proof — отдельные safety guards, и нельзя заявлять их обойденными: гипотеза о семантической совместимости при *законно допущенном* материале.

## Три различающие гипотезы

- **N39-H1 (математический контракт):** при одинаковых `VocabSize/ContextLen` A и B, но различных BPE merges/digest, `CaptureAndValidate(batchB,modelA.VocabSize,modelA.ContextLen)` проходит, а идентичный вызов с четвёртым аргументом `expectedTokenizerSha256: A.ComputeStateSha256()` отклоняется. Нативный тест **пока не запускался**.
- **N39-H2 (quality impact conditional):** если такая pair случайно попала в допускаемый training owner, модель может предпочитать неверные tokenizer A targets; gradient/nonzero LoRA и self-validity остаются, но `domain_law_correct` падает. Если actual owner уже валидирует A/B до ConstructBatch/MutationPermit, импакт = `NOT_REACHABLE`, N39 остаётся defense-in-depth.
- **N39-H3 (restore variant):** при cold restore checkpoint модели + tokenizer артефакта другого происхождения с тем же `VocabSize` source pair и continuation profile могут оставаться self-consistent, но actual free decoding меняется. Не путать с историческим generic KNN re-train vocab drift: здесь относится **непосредственно к exact DPO Student**.

## Почему аналогичность размера особенно опасна

Формальный допустимый контрпример: создать BPE A и B по разным корпусам так, чтобы оба словаря имели одинаковую мощность `V` (например 258 tokens), но их `ComputeStateSha256` отличались и `Encode("ab")` возвращал разный набор token IDs. В обоих случаях roundtrip `Decode(Encode("ab")) == "ab"`. `DpoExactPairMaterial` B сохраняет собственный SHA, защищает его digest и ID-range, поэтому не обязан заметить, что модель интерпретирует этот ID по merge A. Это **условие достижимости**: конкретные корпуса/token IDs необходимо подобрать проверенным тестом, а не выдавать приведённые слова за уже выполненный native run.

## Доменность и применимость

| Домен | Почему риск | Что проверять |
| --- | --- | --- |
| C# / TS code-doc Neural Student | merge для namespace/identifiers/спецсимволов сильно зависит от корпуса | ID→UTF8 token bytes on original BPE vs loaded Student BPE |
| RU/EN/HE переводы Neural Student | разный UTF-8 merge над кириллицей/ивритом, размер может совпасть | unicode-normalized source vs actual bytes and corpus |
| KNN embedding recall | лишь когда embeddings этих token IDs участвуют в индексации/запросах | KNN — **другой** индекс и причина; не считать самим N39 |
| Trained Translation `ExecutorUnit` без neural DPO | не проходит `DpoTrainer.CaptureBatch` | `NOT_APPLICABLE` |
| Frozen RP01 exact-v2 | только если producer/loader допустил A/B mismatch, чего пока **нет в evidence** | actual token SHA receipt + caller path, before/after |

## Минимальное native proof и проверка реального ущерба

[Полностью адресный микроэксперимент](39-MICRO-TOKENIZER-IDENTITY-DPO-PAIR-TO-STUDENT-EXPERIMENT-2026-10-16.md): сначала два разных BPE с одним V и 1 exact-v2 batch; проверка `CaptureAndValidate` 3 args vs 4 args. Это занимает только test-only tensors, **без DPO training**. Затем отдельно single-pair semantic effect на disposable legally admitted learner только после установления реального owner-path и permission. Никаких Windows CI и повторов 500 пар.

**Предпочтительный repair только после проверки:** связать accepted exact-v2 token SHA с immutable model/tokenizer binding на границе DPO project recipient/trainer; сохранить обратную совместимость migration и fail-before-mutation. Нельзя просто менять `TokenizerSha256` строковым полем без проверки durable tokenizer bytes и согласованности config/embedding table. В RP01 test result метку не переписывать.
