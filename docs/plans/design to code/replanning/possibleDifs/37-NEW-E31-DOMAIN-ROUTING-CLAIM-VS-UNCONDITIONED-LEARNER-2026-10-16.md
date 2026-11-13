# N37-C — E31v2: метка домена влияет только на порядок уроков, не на вход ученика

**Дата:** 2026-10-16. **Статус:** SOURCE_CONFIRMED; HISTORICAL_RUN_READBACK; ANALYTICAL_COUNTEREXAMPLE; REAL_QUALITY_TEST_NOT_RUN. Snapshot: `llm_mvp_core/codex/dpo-implementation@793d7b755b23aac90445da3ac6661b1bff7e6672`. Создано лишь в planning/possibleDifs; core/docs/plans/replanning-possible-diffs сохранён read only.

## Диагноз в одном абзаце

Исторический E31v2 объявлял сравнение «random vs domain-routed feedback», но исполнял **одну и ту же нейросеть** на одних и тех же переходах; домен извлекался из **имени файла** и использовался **только для сортировки последовательности обновлений**, затем выбрасывался при вызове `TrainStep`. Это не доказательство, что domain-conditioned learning или KNN-модели для отдельных доменов бесполезны. В отличие от очередной ошибки параметров LoRA, здесь **отсутствует сам сигнал домена внутри обучаемого предиктора**.

## Доказательства

- [E31ContinuousLoopV2.cs:8–12](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E31ContinuousLoopV2.cs#L8-L12): заявляется `domain-routed feedback` и «coherent gradients».
- [E31v2:26–38](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E31ContinuousLoopV2.cs#L26-L38): `BuildLabeledPairs`, затем `start/feedback/val` и round chunks.
- [E31v2:58–83](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E31ContinuousLoopV2.cs#L58-L83): две global `ProbeNeuralNetwork(vocabSize,32,0.02f,42)`; разница — `round.OrderBy(_ => rng.Next())` vs `round.OrderBy(p => p.Domain)`.
- [E31v2:112–130](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E31ContinuousLoopV2.cs#L112-L130): `foreach (var (preds, _) in items)` явно **отбрасывает метку домена**, `net.TrainStep(preds[i],preds[i+1])`; domain зависит от `DomainClassifier.AutoLabel(Path.GetFileNameWithoutExtension(f.Path))`.
- [E31v2:40–56](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/Experiments/E31ContinuousLoopV2.cs#L40-L56): единая aggregate next-predicate accuracy; нет conditional metrics для контрастных доменов.
- [Сохранённый E31v2 Tier3 от 2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/35.ExperimentsV2/results/E31V2-CONTINUOUS-LOOP/results/run-2026-05-01-002.json): baseline=enhanced=**0.0025**, `passed=false`; в notes R0:43% и последующие почти нулевые round accuracies. **Это отрицательный результат curriculum ORDER**, не отрицательный результат domain-conditioned KNN.

## Аналитический контрпример

Даны два равновероятных домена: для одного и того же входного предиката `x=1` в C# правильный переход `y=2`, а в SQL `y=3`. Предиктор `f(x)` без domain/context входа детерминированно может выбрать только 2 или 3; `max macro top1 accuracy=50%`. Условный `f(x,domain)` способен выразить обе функции. Сортировка тех же уроков не добавляет `domain` к сигнатуре `TrainStep`. Это **математический expressivity bound**, не исполненный .NET inference и не измерение реального RP01.

## Гипотезы

- **H37-C1:** часть «неудачных» доменных экспериментов — на самом деле неудачные order-only методы, а conditional memory / typed KNN / адаптер по домену ещё не проверялись на этих конфликтах.
- **H37-C2:** auto-label по basename ошибочно приписывает source в чужой домен, создавая мнимый эффект от curriculum сортировки. Даже корректная сортировка не лечит loss of domain identity в learner.
- **H0:** после истинного domain partition качество не растёт из-за недостатка контрастных примеров, неправильных labels или глобальных полезных закономерностей. Следовательно, domain gate должен оставаться opt-in и с fallback.

## Мини-эксперимент N37-E31: один фактор — наличие domain/context в предикторе

**Corpus:** 16–24 source-family-disjoint переходов с явным ground-truth доменом, включая 8–12 конфликтных одинаковых `x`, разные `y` по домену. Домены: (a) C# AST/nullability vs SQL LEFT JOIN/NULL (только при codec разных типизированных предикатов), (b) RU→EN vs EN→RU с собственными direction ID, (c) DTC design rule vs orchestrator transition при typed input. Нельзя сравнивать сырые непереводимые токены как если бы это один общий словарь.

**Arms:** A=unconditioned `f(x)`, random order; B=unconditioned `f(x)`, grouped order (копия E31v2, отдельно проверка curriculum); C=`f(x,domain,role)` с теми же TRAIN наблюдениями и fixed budget; D=domain-conditioned KNN с запретом доступа чужого scope и versioned global fallback; negative control E=shuffle domain labels только на TRAIN.

**Primary:** paired macro correct lawful next-step@1 по реальным доменам, отдельно conflicted and non-conflicted families. Secondary: unknown domain abstain, false routing, cross-domain interference, old-law retention after new lesson, performance; cold restore и реальный ProductHost/Unit recipient tracing. **Не использовать** loss slope E31 для утверждения о consumer correctness. Native .NET/Student, independent CODEX/Astra 6 UltraCode semantic review — NOT_RUN; это work order.

**Интеграция:** E31 не является прямым codepath `RP01 C#500/ENRU500`; решение conditional Unit/KNN сравнивают на **его собственном** ordinary consumer. Старые numerical statuses не редактировать, а пометить `not a test of domain-conditioned representation`.

## Ограничение новизны

Существующие [E30 domain routing](26-NEW-E30-PREDICATE-CACHE-STATE-ALIAS-AND-MISREAD-2026-10-16.md), [E09 KNN](11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS-2026-10-16.md) и [E31 generic mention](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md) известны. Новый аспект — **конкретное source-confirmed выбрасывание domain из `TrainStep` + basename proxy**, отличающий недостаточный input representation от отрицательной метрики. Абсолютная глобальная уникальность не утверждается.