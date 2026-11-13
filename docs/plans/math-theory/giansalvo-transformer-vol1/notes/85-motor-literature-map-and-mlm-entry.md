---
title: "85. Литературная карта motor formalism и вход в MLM head"
source_pdf_pages: [314, 317]
book_pages: [278, 281]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 85. Литературная карта motor formalism и вход в MLM head

[Индекс библиотеки](../README.md) · [§9.2](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#section-9-2) · [Предыдущая карточка](84-motor-taxonomy-and-chain-rule.md) · [Следующая карточка](86-mlm-primary-motor-and-computational-graph.md)

## Каков статус литературного обзора

§9.2 и Table 9.1 позиционируют motor vocabulary среди соседних подходов. Это **изложение авторской классификации**. В рукописи многие ссылки напечатаны как `?`, а некоторые фамилии повторены: например, “Devlin et al. Devlin et al. (2019b)” и “Elhage et al. Elhage et al. (2021b)”. Эти дефекты сохранены. По данному фрагменту нельзя выдавать список за самостоятельно проверенную полную библиографию или утверждать установленный приоритет motor formalism. [PDF 314–316](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#pdf-0314).

Автор проводит следующие связи:

| Соседняя область | Угол зрения motor formalism в этой главе |
|---|---|
| Backpropagation/autodiff | Отделить incoming signal, локальный оператор и получившийся pullback |
| Residual-stream circuits | Описать local differential primitives внутри компонентов |
| BERTology/probing | Исследовать, как вариация достигает representations и attention maps |
| Attention explanation/interventions | Проверять функциональный эффект attention, не читать weights как готовое объяснение |
| Attribution/influence | Использовать differential observables как кандидатов для дальнейшей проверки |
| Information geometry | Связать tangent-space softmax operations с геометрией distributions |
| FFN memory interpretations | Описать expansion, gating и recombination signal |
| Low-rank adaptation | Различать локальные outer products и ограничение пространства parameter updates |
| Pruning/redundancy | Дополнить signature crowding differential marginality |
| Training dynamics | Проследить цепь parameter variation → scores → attention → signatures |

[Table 9.1, все десять строк](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#table-9-1).

## Softmax motor имеет точную tangent-space структуру

§9.2.6 даёт

$$
J_{\mathrm{softmax}}(s)=\operatorname{diag}(a)-aa^\top,
\qquad a=\operatorname{softmax}(s).
$$

Редакторская проверка: $J\mathbf1=0$, поскольку $\sum_i a_i=1$. Для любого вектора $v$:

$$
v^\top Jv=\sum_i a_iv_i^2-\left(\sum_i a_iv_i\right)^2\ge0.
$$

Это weighted variance. При всех положительных $a_i$ kernel состоит из постоянных векторов; при маске нужно работать на допустимой поддержке и учитывать дополнительные нулевые координаты. Constant shift logits не меняет probabilities. [Формула и пояснение автора](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#section-9-2-6).

Такой локальный Jacobian не является самим Hellinger distance между двумя сигнатурами. Он описывает differential map logits→probabilities; Hellinger/Fisher language сравнивает distributions и задаёт геометрию. Чтобы перейти далее к signature derivative, нужны свойства projection и выбранного coefficient map, особенно у границ faces. [Карточки 69](69-hellinger-wasserstein-layer-geometry.md) и [70](70-training-paths-and-coupling-observables.md).

## Outer product не гарантирует low-rank всего обучения

Источник подчёркивает single-token outer-product gradients до aggregation. **Редакторское уточнение:** $uv^\top$ имеет rank **не больше** 1; он равен нулю при нулевом факторе. Сумма таких вкладов может иметь большой rank: $\sum_{j=1}^d e_je_j^\top=I_d$. Поэтому локальная форма gradient не доказывает, что полный batch gradient, trajectory или оптимальная update ограничены rank 1. [§9.2.8](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#section-9-2-8).

Там же автор различает близость сигнатур и motor energies. Это полезное дополнение: одинаковая attention geometry не задаёт одинаковый loss sensitivity, а необычная сигнатура может иметь малый local motor на данной задаче. Но ни gradient norm, ни crowding отдельно не гарантируют безопасного конечного ablation; возможны насыщение и эффекты высших порядков. [§§9.2.4–9.2.5](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#section-9-2-4).

## С чего начинается MLM head

На PDF 317 автор фиксирует masked position $m$ и final hidden row $A_m\in\mathbb R^{1\times d}$:

$$
\nu_m=A_mW_D+b_D,
\qquad
z_m=\operatorname{softmax}(\nu_m),
\qquad
L_m=-\sum_c t_{mc}\log z_{mc}.
$$

$t_m$ — one-hot target. При vocabulary size $V$ размерностная проверка требует $W_D\in\mathbb R^{d\times V}$ и $b_D,z_m,t_m\in\mathbb R^{1\times V}$; эти размеры — редакторское раскрытие напечатанной формулы. Описана affine-softmax head; более сложная head конкретной реализации потребует дополнительных Jacobian factors. [§9.3 и исходные формулы](../source/0312-0317-ch09-motor-taxonomy-and-related-work.md#section-9-3).

PDF 317 заканчивается завершённой cross-entropy формулой. Производные MLM head продолжаются на [PDF 318](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#pdf-0318) и рассматриваются в [карточке 86](86-mlm-primary-motor-and-computational-graph.md). Этот фрагмент не заявляет нового обучения модели: он задаёт локальную математическую нотацию для последующего анализа.
