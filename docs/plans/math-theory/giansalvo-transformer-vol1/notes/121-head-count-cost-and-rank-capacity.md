---
title: "121. Стоимость числа голов, skew-ёмкость и пределы сравнения many versus one"
source_pdf_pages: [396, 398]
book_pages: [360, 362]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 121. Стоимость числа голов, skew-ёмкость и пределы сравнения many versus one

[Индекс](../README.md) · [Proposition 11.55](../source/0396-0400-ch11-head-size-cost-and-empirical-predictions.md#proposition-11-55) · [§11.10.2](../source/0396-0400-ch11-head-size-cost-and-empirical-predictions.md#section-11-10-2) · [(11.28)](../source/0396-0400-ch11-head-size-cost-and-empirical-predictions.md#equation-11-28)

## Что даёт фиксированная суммарная ширина

Пусть у каждой головы $d_v=d_k$, а $D=\sum_h d_k^{(h)}$ фиксировано. Тогда ведущий счёт матричных multiply-accumulate operations (MAC) имеет вид

$$
C_{\mathrm{MAC}}=4nDd+2n^2D.
$$

Первая часть включает Q, K, V и выходную проекцию; вторая — оба произведения $QK^\top$ и $AV$. При таком соглашении разбиение $D$ на головы не меняет эту ведущую матричную стоимость.

Если умножение и сложение считать отдельными FLOPs, ведущий член удваивается. В доказательстве книги $2n^2d_k$ названо стоимостью score computation: при таком буквальном FLOP-счёте остаётся неучтённым $AV$, а проекции используют другое соглашение. Поэтому при сравнении с профилировщиком сначала надо зафиксировать единицу счёта.

Кроме того, (11.27) не содержит softmax, хотя утверждение перечисляет его среди учтённых операций. Softmax и связанные с ним операции по attention scores дают работу порядка $Hn^2$. Размер промежуточных attention matrices также зависит от $H$. Реальные память и время зависят от materialization, используемых kernels, masking и аппаратуры. Равенство ведущего матричного члена не доказывает одинаковое полное время.

Если $D_k=\sum_h d_k^{(h)}$ и $D_v=\sum_h d_v^{(h)}$ различны, соответствующий ведущий MAC-счёт:

$$
C_{\mathrm{MAC}}
=2nd(D_k+D_v)+n^2(D_k+D_v).
$$

Он возвращает формулу выше при $D_k=D_v=D$.

## Skew-часть имеет другую границу ранга

Для $M=W_QW_K^\top$ с $W_Q,W_K\in\mathbb R^{d\times d_k}$:

$$
\operatorname{rank}M\le d_k,
\qquad
\operatorname{rank}M_a
\le\min\!\left(2d_k,\;2\left\lfloor\frac d2\right\rfloor\right).
$$

Вторая оценка следует из $M_a=(M-M^\top)/2$, субаддитивности ранга и его чётности. Оценка $\operatorname{rank}M_a\le d_k$ в §11.10.2 для произвольных Q/K-факторов неверна. Уже голова с $d_k=1$ может иметь skew-ранг 2: [карточка 120](120-even-skew-rank-and-head-growth-assumptions.md).

Верхняя граница числа плоскостей одной головы равна $\min(d_k,\lfloor d/2\rfloor)$, а не $d_k/2$. При фиксированном $D$ сумма индивидуальных границ не превышает $D$, до учёта ambient dimensions и перекрытия плоскостей. Число независимых плоскостей суммы операторов и сумма per-head capacities — разные величины.

Даже совпадение какой-либо алгебраической границы не означает одинаковый класс функций. Раздельные головы имеют отдельные softmax, поэтому их нелинейные attention maps не сводятся автоматически к softmax одной большой головы.

## Почему (11.28) не задаёт общий effective skew-operator

В обычных размерах $W_V\in\mathbb R^{d\times d_v}$, $M_a\in\mathbb R^{d\times d}$ и $W_K^\top\in\mathbb R^{d_k\times d}$. Напечатанное произведение $W_VM_aW_K^\top$ обычно не определено.

Если искусственно сделать все матрицы квадратными, skew-симметрия всё равно не сохраняется при разных левом и правом множителях. Например,

$$
W_V=\operatorname{diag}(1,2),\quad W_K=I,\quad
M_a=\begin{pmatrix}0&1\\-1&0\end{pmatrix}
\quad\Longrightarrow\quad
W_VM_aW_K^\top=
\begin{pmatrix}0&1\\-2&0\end{pmatrix}.
$$

Последняя матрица не является skew. Согласованная congruence-преобразованием skew-форма требует множителей вида $B M_a B^\top$.

Для обычного attention без дополнительных bias/masks фактический суммарный выход имеет вид

$$
Y=\sum_h
\operatorname{softmax}_{\mathrm{row}}
\left(\frac{XM_hX^\top}{\sqrt{d_k^{(h)}}}\right)
XW_V^{(h)}W_O^{(h)}.
$$

Здесь важны полные $M_h$, нормировки, отдельные softmax и выходные карты. Утверждения о «потере ёмкости в $H$ раз» из совпадения skew-плоскостей требуют отдельного доказательства для этого отображения. Установленный leading-cost факт сам по себе такого доказательства не даёт.
