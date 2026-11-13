# 01 — Baseline, архив экспериментов и реальный предмет сравнения
**Режим:** source-only reconciliation; числа извлечены из сохранённых результатов 01.05 и RP01 15–16.10; эксперименты здесь заново не запускались.

## DIF-101. Неверно переносить «успех эксперимента» на RP01
| Исторический эксперимент | Сохранённый показатель baseline → experimental | Истинный target | Какое заключение допустимо |
|---|---|---|---|
| E09 structural KNN | top1 0.2413 → 0.3507 (+45.33% относительно) | Предсказать следующий AST-kind-код по Roslyn structural vector | Есть положительный *узкий* результат структурной nearest-neighbour модели; не языковая генерация и не LoRA |
| E13 method KNN | 0.3333 → 0.3333 | Тип следующего метода | **Нет измеренного выигрыша** |
| E14 context-action KNN | 0.8824 → 0.8676 | Класс действия по SyntaxKind | **Ухудшение**; серьёзный дисбаланс классов |
| E15 semantic tokenizer | perplexity 229.8119 → 207.5766 | Bigram на level0 vs stopword/stem level1 | Выигрыш на изменённом пространстве токенов; не доказательство downstream смыслового качества |
| E16 synonym collapse | perplexity 207.5766 → 190.1389 | Level1 vs collapsed level2 | Кандидат на полезное смысловое сжатие, нужна обратная проверка значений |
| E17 logical predicates | perplexity 190.1389 → 5.5964 | Level2 (2319 токенов) vs level3 (583 токена, vocab=10) | **Сравниваются разные цели/алфавиты**; огромный спад perplexity не доказывает качество исходных задач |
| E29 “DPO pair value” | pred_accuracy 0.3347 → 0.3347 | Bigram с дополнительным 2-кратным повтором выбранных файлов | Не DPO optimizer и **нет выигрыша** |
| E31 continuous loop | slope 0.0369 → 0.0341 (-7.69%) | Стратегия взвешивания циклов | **Ухудшение** |

Проверенные JSON:
[E09](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) ·
[E13](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E13-METHOD-KNN/results/run-2026-05-01-001.json) ·
[E14](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-002.json) ·
[E15](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E15-SEMANTIC-TOKENIZER/results/run-2026-05-01-001.json) ·
[E16](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E16-SYNONYM-COLLAPSE/results/run-2026-05-01-001.json) ·
[E17](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E17-LOGICAL-PREDICATES/results/run-2026-05-01-001.json) ·
[E29](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E29-DPO-PAIR-VALUE/results/run-2026-05-01-001.json) ·
[E31](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E31-CONTINUOUS-LOOP/results/run-2026-05-01-001.json).

## DIF-102. E09 сравнивает baseline с KNN на НЕ одной и той же validation sample
**Прямой код:** [E09 Run lines 30–78](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L30-L78).
Baseline оценивает val.Count−1 переходов. KNN берёт только val.Take(500), оценивает максимум 499 переходов. Поэтому **+45.33% нельзя пока интерпретировать как чистый treatment effect** без повторного единого panel и одинакового target set. Также [BuildTokenVectors](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L102-L124) создаёт Roslyn SyntaxKind и StructuralVectorExtractor, а не готовые semantic embeddings языка.
**Фальсификатор:** на сохранённом corpus, train rows без изменения, оценить обе системы на одинаковых val indices; если улучшение исчезает, исторический эффект был частично измерительным.
**Статус:** CODE доказан; новая сопоставимая численная оценка NOT_RUN.

## DIF-103. E15/E16/E17 — compressibility != семантический learning
[E15](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E15SemanticTokenizer.cs#L27-L70), [E16](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E16SynonymCollapse.cs#L27-L68), [E17](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E17LogicalPredicates.cs#L28-L73).
E17 меняет и длину, и словарь, и саму целевую случайную величину; уровни perplexity напрямую не нормированы на одно и то же исходное событие. Её успех как компрессии предикатной последовательности не тождественен способности сохранить SQL NULL, отрицание, AST relation, RU/HE морфологию или user answer.
**Проверка:** semantic round-trip/feature-law probes с фиксацией source, version, downstream solver; paired accuracy, а не только entropy; не требовать полного буквального восстановления от lossy compression.
**Статус:** CODE + REPORTED_RUN; влияние на текущий RP01 OPEN.

## DIF-104. Исторические «DPO» и «continuous training» не подтверждали loss/LoRA
[E29 implementation](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E29DpoPairValue.cs#L21-L71) строит bigram, выбирает топ-файлы по числу предикатов и дополнительно повторяет пары. У него нет DpoTrainer, policy/reference, optimizer или LoRA mutation. E29 не является доказательством DPO-эффекта.
[E31 result](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E31-CONTINUOUS-LOOP/results/run-2026-05-01-001.json) сообщает отрицательный relative gain. Их нельзя объединить с успешным E09 в blanket statement «вся система уже обучалась».

## DIF-105. «Обученный discriminator» P3 был сконструирован детерминированно
[P3LearningFromFailureProof.BuildLearningCycle](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/P3LearningFromFailureProof.cs#L329-L365) создаёт объект LearnedDiscriminator со строковыми правилами, явным BaseService и итоговым verdict. Это допустимый пример *добавления дискретного правила из feedback*, но не тренировка нейронных весов. Требуются отдельно evidence изменения состояния, generalization, routing/consumer и измеренное качество.
[T35](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/T35LogicalUnitReplacementProof.cs#L75-L122) проверяет конкретный подменённый unit относительно oracle packets и ставит синтетическое время 0.1ms: это functional proof, не measured runtime latency или LLM training proof.

## DIF-106. RP01 research результаты пока отрицательны по смыслу
[Current RP01 mechanism review](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/RP01-LEARNING-MECHANISM-REVIEW-2026-10-16.md#L10-L31) и [RP01 recovery audit](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md#L12-L29) фиксируют C# 0/3→0/3, EN→RU 0/4→0/4 (узкие free-generation panels), изменение preference margins/адаптеров и отсутствие подтверждённого useful answer gain. Dataset C# 500 строк = 25 шаблонов×20; ENRU 500 = ограниченные семейства. Нельзя считать 500 независимых задачами.
**Исследовательская оговорка:** исходная B0 случайна; нулевой прирост на малой free-panel не локализует root cause DPO и не является строгим доказательством невозможности обучения после другого baseline.

## Матрица сопоставимых задач
1. AST-next-symbol: E09 original baseline vs original structural KNN vs production hashed KNN; одинаковые train/val indices.
2. Meaning preservation: E15–E17 vs BPE raw vs domain-IR; domain-law counterexamples с одинаковым downstream consumer.
3. Preference formation: DPO chosen preference and held-out semantic answer — **разные метрики и разные claims**.
4. Unit learning: old discrete rule formation vs current per-unit learned state; frozen upstream и same exact inputs.
5. Ordinary consumer: research result после fresh load vs ordinary app path при одинаковых state IDs.

**Запрещённый вывод:** «E17−97% perplexity + E09+45% accuracy гарантировали успешный EN→RU DPO». Это логически разные эксперименты.
