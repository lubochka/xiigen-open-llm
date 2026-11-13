---
title: "104. S–A interventions: локальный эффект и критерий ориентации"
source_pdf_pages: [358, 360]
book_pages: [322, 324]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 104. S–A interventions: локальный эффект и критерий ориентации

[Индекс библиотеки](../README.md) · [Предыдущая карточка](103-headwise-polarisation-and-attention-asymmetry.md) · [Следующая карточка](105-enriched-crowding-and-empirical-scope.md)

## Какие вмешательства заданы

**Definition 10.19** сравнивает полный motor $M_s+M_a$ с $M_s$, $M_a$, $M_s-M_a$ и $M_s+\beta M_a$. Можно измерять attention, signature, hidden-state, loss или task-metric changes; выбор observable составляет часть определения профиля. Для причинного сравнения фиксируются input batch, checkpoint и остальные компоненты вычисления. [PDF 358, §10.13 и Definition 10.19](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#section-10-13).

В published Table 10.3 для layer 2/head 7 сообщены:

| Intervention | $\beta$ | Attention drift | Output drift | Attention asymmetry |
|---|---:|---:|---:|---:|
| Full | 1.0 | 0.000 | 0.000 | 0.320 |
| $M_s$ only | 0.0 | 0.272 | 0.103 | 0.091 |
| $M_a$ only | — | 0.177 | 0.052 | 0.391 |
| Sign flip $M_s-M_a$ | -1.0 | 0.558 | 0.219 | 0.468 |
| $M_s+0.5M_a$ | 0.5 | 0.133 | 0.049 | 0.140 |
| $M_s+1.5M_a$ | 1.5 | 0.127 | 0.045 | 0.481 |

Это **авторские значения**, а не наш запуск. Ось Figure 10.10 обозначена как relative change versus full; точная норма и знаменатель нормировки на этих страницах не специфицированы. Figure дополнительно повторяет эквивалентные условия под именами `beta_0`, `beta_1`, `beta_-1`; это не дополнительные независимые interventions. [Table 10.3 и Figure 10.10](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#table-10-3).

## Что доказывает Proposition 10.20

Proposition прямо называет своё доказательство **definition-level operational criterion**. Если sign-flip attention drift существенно больше обоих removal drifts, то на этом batch и для этой нормы автор называет голову operationally dependent on orientation. В таблице $0.558>0.272$ и $0.558>0.177$, поэтому reported values удовлетворяют этому критерию. Измерен также output effect 0.219. [Proposition 10.20](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#proposition-10-20); [продолжение proof](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#pdf-0360).

Условие само не доказывает глобальную необходимость головы для classifier, значимость на других inputs или улучшение task metric. Автор отдельно различает цепочку: score intervention → attention effect → value/output effect → downstream/task effect. Отсутствие одного из последних эффектов может оборвать функциональную значимость, несмотря на большой balance. [PDF 360, три failure modes](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#pdf-0360).

## Размер вмешательства — отдельный фактор

Редакторское ограничение сильного прочтения proof: removal меняет scores на $-L_a$, а flip — на $-2L_a$. Поэтому больший flip drift может частично объясняться **вдвое большей длиной perturbation**, даже в почти линейном режиме. Для гладкого $A(\beta)=\operatorname{softmax}(L_s+\beta L_a)$ около $\beta=1$:

$$
A(1+\delta)-A(1)=\delta\,D\operatorname{softmax}_{L_s+L_a}[L_a]+O(\delta^2).
$$

Norm этого изменения имеет V-shaped leading term $|\delta|\,\lVert D\operatorname{softmax}[L_a]\rVert$. Такая форма возникает у любой ненулевой гладкой чувствительности к скалярному параметру; сама V-форма не выделяет особый learned directional mechanism. [Source beta interpretation](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#pdf-0359).

Например, при $M_s=0$ и достаточно малом ненулевом $M_a$ удаление $M_s$ вообще не меняет full map, removal $M_a$ даёт первый порядок $\lVert D A[M_a]\rVert$, а flip — приблизительно удвоенный. Operational criterion может выполняться без дополнительных выводов о semantics или task necessity. Это не опровержение локального эффекта знака: $M_a$ и $-M_a$ имеют одинаковую norm, но разные maps. Это ограничение вывода о причине **сравнительной величины** эффекта; для него полезны controls одинаковой perturbation norm. [Proposition 10.20](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#proposition-10-20).

## Matrix intervention и архитектура факторов

В canonical coordinates

$$
M_s-M_a=M^\top,\qquad
M_s+\beta M_a=\frac{1+\beta}{2}M+\frac{1-\beta}{2}M^\top.
$$

Sign flip можно реализовать обменом Q/K factors в простой unbiased factorization. Но $M_s$ или $M_a$ могут иметь rank до $2d_k$ при исходном rank $M\le d_k$; произвольный $\beta$ также не гарантирует прежний factor width. Поэтому прямой score hook и замена обучаемых $W_Q,W_K$ заданной ширины — разные реализации. Книга здесь задаёт matrix interventions, а не доказанную процедуру компрессии architecture. [Intervention family](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#display-10-intervention-family); [factorization caveat](99-two-flow-gradient-and-factorised-training.md).
