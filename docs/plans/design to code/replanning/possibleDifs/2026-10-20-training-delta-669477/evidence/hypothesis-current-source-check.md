# Точечная проверка новых гипотез после core827

Проверка документа и source без build, обучения или изменения репозитория. Аудируемый pin новых карточек остаётся `66947785d1318f7cbbc88118f3156cdc75001e16`; обнаруженный следующий commit — `827a1acdbef95d220886e54f9d36bce0b405c8cd`, непосредственный parent —669. Изменение HEAD не превращает выполненный source-аудит в runtime-эксперимент и не требует перезапуска уже измеренных опытов.

## 1. Исходные учебные пути не изменились

У последнего commit список changed files содержит ровно 300 entries; сам по себе такой список не использован как доказательство отсутствия иных изменений. Вместо него сравнили дочерние Git tree SHA двух **полных корневых деревьев с `truncated=false`**. Для девяти production проектов SHA совпали: `00.Core`, `00.TensorCore`, `01.Tokenizer`, `02.DatasetPipeline`, `03.TransformerModel`, `04.TrainingLoop`, `10.LoRA`, `11.DPO`, `27.TranslationDomain`. Поскольку совпало всё дерево каждого проекта, совпали и все 36 native файлов RMS host, и все 7 файлов verifier host.

| Гипотеза | Что сохранилось на 827 | Вывод |
|---|---|---|
| RMS-ROW-REDUCE-01 | `TensorOps.SumAlongAxis` и вся указанная цепь Trainer → Transformer → RmsNorm → Mean | Source allocation-factor не исправлен и не исчез; влияние на реальные TRAIN allocations пока не измерено |
| VT-APOSTROPHE-01 | Producer `BuildGeneralBatch`, parser `TryParseRequiredTerm`, native Train и Verify | Source truncation-factor не исправлен и не исчез; прогноз false accepts/rejects не является результатом |

Квитанция с обоими root tree и девятью парами subtree SHA: [source-delta](latest-core-learning-source-delta.json). Исходные API trees: [core669](https://api.github.com/repos/lubochka/llm_mvp_core/git/trees/66947785d1318f7cbbc88118f3156cdc75001e16), [core827 tree](https://api.github.com/repos/lubochka/llm_mvp_core/git/trees/5897d466db4bf707fafac7ce551d8dc6fdd546b0). Это сравнение source bytes, не проверка компиляции.

## 2. Три добавленные в protected cards не повторяют новые факторы

Прочитаны разделы гипотезы, A/B-фактора и участия в обучении трёх добавленных карточек на 827; все их bytes сверены по Git blob SHA. По полному тексту дополнительно проверены `SumAlongAxis`, `RMS-ROW-REDUCE`, `TryParseRequiredTerm`, `VT-APOSTROPHE`, `brother.s`, `апостроф`: совпадений нет. Одна только непохожесть имён не считается доказательством; ниже сопоставлены сами вмешательства.

| Добавленная карточка | Её единственный фактор | Почему не повторяет RMS/VT |
|---|---|---|
| [ROPE-TRAIN-TABLE-01](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/ROPE-TRAIN-TABLE-01.md#L23-L32) | Однопозиционный training-only cache cos/sin в RotaryEmbedding и caller CausalSelfAttention | Не меняет Mean/SumAlongAxis; не относится к записи verifier terms. RMS явно исключён из совместного изменения |
| [AST-SIB-CACHE-01](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/AST-SIB-CACHE-01.md#L55-L91) | Scoped sibling count/index cache в StructuralKnn.TrainFromSource/Extract | Другой learner, представление и writer; не RMS reduction и не переводческий parser |
| [BPE-REALIZED-MERGE-COUNT-01](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/BPE-REALIZED-MERGE-COUNT-01.md#L19-L45) | Выбор merge по числу реально исполнимых неперекрывающихся замен вместо частоты соседних пар | Меняет цель обучения BPE; не scalar reduction Transformer и не delimiter терминов verifier |

Квитанция scope/queries/blob SHA — [overlap](latest-core-three-card-overlap.json). Эти три дизайна уже входили в первоначально прочитанный planning-корпус 799. Новый перенос в protected каталог не создаёт дубликат двух предложенных здесь факторов. Полный повторный смысловой review всех остальных изменённых protected карточек в этой точечной проверке не заявляется.

При чтении ROPE повторно обнаружена важная оговорка наследованного SFT host: фактически используется `Encoding.UTF8`, а не вызов BPE; LogicalUnit/Flow отсутствуют. В RMS-рецепте явно исправлены только подписи receipt (`native_learner_instances`, `logical_unit_flow_scope`, `byte_tokenizer`), без изменения A/B-фактора, данных или бюджета.

**Итог:** оба новых дизайна остаются обоснованными кандидатами для будущей изолированной проверки. Статус — NOT_MATERIALIZED / NOT_RUN; measured benefit, runtime readiness и перенос в production не заявляются.
