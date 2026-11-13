# N37-02 — Новая узкая гипотеза: stochastic sampler может вернуть запрещённый последний токен при float32 mass < 1

**16.10.2026 · C=CODE BRANCH VERIFIED · P=EXECUTED JS SOURCE-LOGIC PROXY · NATIVE_DOTNET_NOT_RUN · RP01 CAUSALITY NOT PROVEN.** Актуальный `llm_mvp_core/codex/dpo-implementation` прочитан при HEAD `793d7b755b23aac90445da3ac6661b1bff7e6672`. Инцидент пока **не наблюдался в реальном ответе**. В отличие от N37-01 — другой recipient: декодер нейронного Student, а не KNN Unit.

## Что происходит

В top-K/top-P отборе есть запрещённые токены с вероятностью ноль. Мультиномиальный sampler суммирует сохранённые вероятности в `double`, но сами вероятности вычислены в `float`; из-за округления их суммарная масса иногда меньше 1. Если псевдослучайное число попадает в этот маленький незаполненный хвост, код **не выбирает последний допустимый токен**, а жёстко возвращает последний ID во всём словаре, даже если его вероятность нулевая. При совпадении этого ID с EOS генерация может остановиться раньше.

Это не объясняет ранее известную тему «K и P не пересекаются» и не эквивалентно ошибке распознавания EOS. Это отдельная ветка **masked-to-zero candidate → random residual → unconditional last vocab id**.

## Прямые исходники

1. [`Sampler.Sample` L19–39](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/09.Inference/Sampler.cs#L19): при `temperature<=0` вызывается greedy `Argmax`; при `temperature>0` строится probability tensor через `TopKFilter`, `TopPFilter` или `Softmax`.
2. [`TopKFilter` L42–57](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/09.Inference/Sampler.cs#L42): всё ниже top-K threshold получает `float.NegativeInfinity`; [`TopPFilter` L59–82](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/09.Inference/Sampler.cs#L59) аналогично выключает nucleus-tail.
3. [`TensorOps.Softmax` L785–852](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/00.TensorCore/Core/TensorOps.cs#L785): `resultData` имеет тип `float[]`, `expSum` — `float`; каждую вероятность записывает обратно как `float`.
4. [`MultinomialSample` L84–97](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/09.Inference/Sampler.cs#L84): `double r = random.NextDouble(); double acc = 0;`, цикл `acc += probs.Data[i]`, проверка `if (r <= acc) return i;`, **при отсутствии выбора — `return probs.Shape.TotalElements - 1;`**, независимо от support/mask.
5. [`TextGenerator.GenerateCore` L215–249](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/09.Inference/TextGenerator.cs#L215): `nextToken` берётся из sampler, добавляется в generated и при `nextToken == eosToken` завершает генерацию.

**Source-confirmed predicate:** если finite normalised `probs` фактически имеет double-sum ниже возвращённого `r`, будет выдан последний ID независимо от `probs[last]`. Вероятность/частота настоящего инцидента и его продуктивный эффект не измерялись.

## Один исполненный source-only математический proxy

Для 62 позиций предположим logit=0 у первых 61 и `-Infinity` у последнего: topK=61 допустил первые 61. В `float32` каждое из 61 допустимых значений получается `0.016393441706895828`. Суммирование их как `double`, как в `MultinomialSample`, даёт `0.9999999441206455`, а не 1.0. Контролируемый draw `r=0.99999999` не проходит ни одну итерацию и попадает в `return lastIndex` (61), где probability = 0.

**Это не native .NET опыт:** JS `Float32Array` + воспроизведённый цикл C# подтвердили условную арифметику. [Воспроизводимый proxy](37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY.mjs) и [receipt](37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY-RESULT.json) записаны отдельно. Proxy не прогоняет `TensorOps.Softmax`, не отражает реальный seeded Student и не даёт эмпирической частоты качества. Конкретная ширина 61 нужна лишь для опровержимого синтетического примера.

## История веток и приоритет

В `language_translate`, `DesignToCode`, `external_domains` обнаружен общий старый `Sampler.cs` blob `2b007f59524ba50a0ac5a3334d92f38267c8ebf5` с тем же `if(r<=acc)... return lastId` (старый RNG был глобальным). Текущий blob `4533c158b0dea02ff8e07816c02d19055ca80cb1`. **Следовательно, это унаследованный потенциальный сбой, не новая RP01 регрессия.**

**P3 для stochastic translation/code generation. Для текущих C#500/ENRU500, где сохранённые RP01 первые-токены оценивались на greedy, этот эффект НЕПРИМЕНИМ как объяснение 0/3 и 0/4.** Потенциальное влияние на редкие stochastic outputs не оправдывает большой DPO запуск и не должно оттеснять P0 «B0 random без SFT», KNN/Unit consumer path и teacher pair-law validity.

| Домен | Когда релевантен | Что проверять |
| --- | --- | --- |
| EN→RU / HE→EN | stochastic `temperature>0`, явное ограничение топ-кандидатов | premature EOS, пустая/обрезанная фраза |
| C# / SQL generation | stochastic code/doc answer с выведенным запрещённым `last token` | illegal character/token или неверный конец ответа |
| КNN / deterministic Unit | НЕ применим без нейронного sampler на final path | не записывать как причину KNN ranking |
| RP01 greedy | НЕ применим при `temperature<=0` | отрицательный контроль `Argmax` |

## Мини-эксперимент: N37-TAIL-01

**Baseline A:** прямой native вызов действительного `Sampler.Sample` с `Tensor` logits длины 62: первые 61 равны 0, последний -100; `temperature=1`, `topK=61`, `topP=1`. Подставить test-controlled `Random.NextDouble`, дающий `r=0.99999999`; если overload не принимает `Random` с переопределяемым `NextDouble`, test-only выделить чистую `MultinomialSample` под internal seam, **не менять production behaviour**. Потребуются фактические разрешённые internal access и сборка проекта. Сравнить selected index с probability tensor и masked support.

**Enhanced B:** только test-only выбор последнего положительно-весового допустимого индекса при остатке; не менять softmax, logits, EOS, LoRA, teacher, tokenizer, seed law. Native `SupportViolationCount` A=1, B=0 для данного deterministic fixture — доказательство механизма, **не обучение**. Контроли: `r=0.25`, все logits finite/greedy, последняя позиция разрешена; результат по B совпадает с baseline в штатных случаях.

**Проверка качества отдельно:** только если обычный ProductHost реально использует stochastic sampler и было наблюдаемое `maskedTokenSelected`, провести 12 source-family-disjoint задач соответствующего домена A/B с одинаковыми seed/Student/checkpoint, primary = `law-correct answers`, secondary = `prematureStopRate`, `invalidTokens`, `normalCoverage`. Без этой ступени статус — `SOURCE-PREDICATE + PROXY`, не `QUALITY_GAIN`.

**Фальсификаторы:** production нормализует распределение иначе перед этим вызовом; last token всегда разрешён на реальных конфигурациях; native `float` суммируется до 1 на реально используемых vocab/topK; отсутствует stochastic serving consumer; наблюдаемые semantic failures не связаны с tail branch.

**Dedup:** [N26 stop-target/topK-topP](26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md) уже описывает отсутствие совместного пересечения K/P и tie-overflow, но не conditional `MultinomialSample` residual tail. [Core/31 answer boundary](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/31-answer-boundary-and-decoding-scope.md) — другой EOS/trainability вопрос.

## Review / граница изменения

Изменений product `llm_mvp_core` не было. Публикация — только исследовательская карточка, код маленького proxy и его выход. План-review: проверить точные current lines, воспроизвести source-logic proxy, не вводить случайный фальшивый EOS в рабочую модель и не провозглашать ухудшение RP01 на основании достижимого синтетического условного пути.
