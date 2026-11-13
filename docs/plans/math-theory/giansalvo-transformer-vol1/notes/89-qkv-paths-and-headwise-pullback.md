---
title: "89. Q/K/V пути, размерности и обратное разбиение по головам"
source_pdf_pages: [324, 326]
book_pages: [288, 290]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 89. Q/K/V пути, размерности и обратное разбиение по головам

[Индекс библиотеки](../README.md) · [§9.9](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#section-9-9) · [Theorem 9.11](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#theorem-9-11) · [Предыдущая карточка](88-convex-values-and-softmax-tangent-motor.md) · [Следующая карточка](90-block-synthesis-depth-recursion-and-residual-bounds.md)

## Фиксируем одну ориентацию

Пусть $Y\in\mathbb R^{n\times d}$, $W_Q,W_K\in\mathbb R^{d\times d_k}$, $W_V\in\mathbb R^{d\times d_v}$. Для query-позиции $m$:

$$
q_m=y_mW_Q\in\mathbb R^{1\times d_k},\quad
K=YW_K\in\mathbb R^{n\times d_k},\quad
V=YW_V\in\mathbb R^{n\times d_v}.
$$

$s_m,a_m$ — строки $1\times n$, $o_m=a_mV$ и входящий motor $g_m=\partial L/\partial o_m$ — строки $1\times d_v$. Именно в этой row-конвенции основной текст даёт согласованные формулы. [PDF 323–324](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#section-9-8).

## Три маршрута к входу

Сначала различаются value- и coefficient-производные:

$$
G_V=a_m^\top g_m\in\mathbb R^{n\times d_v},
\qquad
g_a=g_mV^\top\in\mathbb R^{1\times n}.
$$

Далее

$$
\eta_m=g_aJ_{\mathrm{softmax}}(s_m),
\qquad
g_q=\frac1{\sqrt{d_k}}\eta_mK,
\qquad
G_K=\frac1{\sqrt{d_k}}\eta_m^\top q_m.
$$

Value path распределяет входящий сигнал между values с весами $a_{mj}$. Coefficient path измеряет чувствительность результата к весам. Softmax превращает её в score-motor с нулевой суммой, который разветвляется в Q и K. [Формулы §9.9](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#display-9-qkv-pullbacks).

**Редакторское продолжение цепного правила.** Если $e_m\in\mathbb R^{n\times1}$ — стандартный столбец, то вклад данной query-позиции во входную матрицу имеет вид

$$
G_Y=
e_m\left(\frac{\eta_mK}{\sqrt{d_k}}W_Q^\top\right)
+\frac{\eta_m^\top q_m}{\sqrt{d_k}}W_K^\top
+a_m^\top g_mW_V^\top.
$$

Все три слагаемых имеют размер $n\times d$. Только query-вклад помещается в строку $m$; key и value пути достигают всех используемых токенов. Эта запись показывает, почему компактные формулы внутри большого графа требуют восстановления размерностей и query-selector.

Аналогично,

$$
G_{W_Q}=y_m^\top g_q,\qquad
G_{W_K}=Y^\top G_K,\qquad
G_{W_V}=Y^\top G_V.
$$

При фиксированном одном query-выходе эти выражения имеют ранг не более одного. Суммирование по позициям, примерам или повторным использованиям параметров снимает такую общую rank-one гарантию.

## Как появляется headwise decomposition

Для

$$
\operatorname{MHA}(X)=\operatorname{Concat}(o^{(1)},\ldots,o^{(H)})W_O
$$

сначала вычисляется $g_{\mathrm{concat}}=g_{\mathrm{out}}W_O^\top$, затем его сегмент $(g_{\mathrm{concat}})_h$ идёт в голову $h$. Каждый сегмент проходит собственные Q/K/V операции, после чего вклады суммируются у общего $X$. Это [Theorem 9.11](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#theorem-9-11).

«Независимо проходит по вычислительному пути» означает отдельное применение chain rule при фиксированных значениях промежуточных переменных. Оно не означает статистическую независимость голов, ортогональность градиентов или отсутствие взаимодействий через loss и shared input.

В частности,

$$
\left\|\sum_hG_X^{(h)}\right\|_F^2
=\sum_h\|G_X^{(h)}\|_F^2
+2\sum_{h<k}\langle G_X^{(h)},G_X^{(k)}\rangle_F.
$$

Поэтому [синтетический stacked bar Figure 9.7](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#figure-9-7) полезен как описание долей выбранных норм, но не устанавливает аддитивность энергии полного градиента. Это редакторское следствие, согласующееся с оговоркой после Theorem 9.12.

## Локальный номер головы остаётся координатой реализации

[Remark 9.10](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#remark-9-10) ограничивает Q/K/V decomposition фиксированными слоем и параметризацией. Номер головы удобен для tracing внутри этого блока. Между слоями он не становится инвариантной идентичностью; там сохраняется подход через популяции сигнатур из главы 8.

Для использования в проекте следует хранить вместе путь, переменную, размерность, layer/head index и используемую нормировку. Сумма маршрутов описывает производную, а не автоматически причинную полезность головы: последняя требует отдельного вмешательства и измерения поведения.

