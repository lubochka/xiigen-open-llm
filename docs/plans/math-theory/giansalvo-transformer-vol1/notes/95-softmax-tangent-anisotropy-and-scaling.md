---
title: "95. Softmax-tangent anisotropy: ориентация векторов, масштаб и насыщение"
source_pdf_pages: [337, 338]
book_pages: [301, 302]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 95. Softmax-tangent anisotropy: ориентация векторов, масштаб и насыщение

[Индекс](../README.md) · [§9.22.2](../source/0335-0341-ch09-motor-rank-anisotropy-and-bounds.md#section-9-22-2) · [Proposition 9.29](../source/0335-0341-ch09-motor-rank-anisotropy-and-bounds.md#proposition-9-29) · [Карточка 88](88-convex-values-and-softmax-tangent-motor.md)

## Какой переход измеряется

После coefficient motor $u=gV^\top$ автор рассматривает действие

$$
B(a)=\operatorname{diag}(a)-aa^\top,\qquad
J_{\mathrm{SA}}(a)=\frac{B(a)}{\sqrt{d_k}}.
$$

Индекс $\chi_{\mathrm{sa}}$ — отношение нормы motor после этого линейного преобразования к норме исходного $u$. Он показывает, какая часть данной чувствительности проходит через нормировку вероятностей.

В источнике $u$ определён как **строка**, но затем написано $J_{\mathrm{SA}}^\top u$. Размерности этой записи несогласованы. Корректные эквивалентные соглашения:

$$
\chi_{\mathrm{sa}}
=\frac{\lVert uJ_{\mathrm{SA}}\rVert_2}{\lVert u\rVert_2}
=\frac{\lVert J_{\mathrm{SA}}^\top u^\top\rVert_2}{\lVert u^\top\rVert_2}.
$$

При $u=0$ значение по соглашению равно 0. Английский source сохраняет исходную запись; эта карточка явно исправляет ориентацию вектора.

## Когда нужен множитель \(1/\sqrt{d_k}\)

Если attention probabilities равны $a=\operatorname{softmax}(qK^\top/\sqrt{d_k})$, то производная по **немасштабированным** scores $qK^\top$ включает $1/\sqrt{d_k}$. Производная по уже масштабированным logits $s=qK^\top/\sqrt{d_k}$ равна просто $B(a)$.

Таким образом, нельзя одновременно включить scaling в $s$ и ещё раз умножить его Jacobian на $1/\sqrt{d_k}$. Сравнивая разные головы, нужно сообщать размер $d_k$, температуру и переменную дифференцирования.

## Ядро, норма и насыщение

Так как $B(a)\mathbf1=0$, постоянный coefficient motor уничтожается. Для строго положительных softmax probabilities ядро состоит ровно из постоянных векторов: квадратичная форма $z^\top B(a)z$ — дисперсия значений $z_i$ под распределением $a$. При нулевых вероятностях или маске ядро может быть больше; сравнение должно использовать согласованную допустимую поддержку.

**Proposition 9.29** даёт

$$
0\le\chi_{\mathrm{sa}}
\le\frac{\lVert B(a)\rVert_{\mathrm{op}}}{\sqrt{d_k}}.
$$

Дополнительная редакторская граница $\lVert B(a)\rVert_{\mathrm{op}}\le1/2$ следует, например, из кругов Гершгорина: диагональ $a_i(1-a_i)$ равна сумме абсолютных внедиагональных элементов строки, а $2a_i(1-a_i)\le1/2$. Поэтому

$$
\chi_{\mathrm{sa}}\le\frac1{2\sqrt{d_k}}.
$$

Это не индекс, который обязательно достигает 1. При uniform attention на $n$ позициях

$$
B(a)=\frac1nP,\qquad P=I-\frac{\mathbf1\mathbf1^\top}{n},
\quad
\chi_{\mathrm{sa}}=
\frac{\lVert uP\rVert_2}{n\sqrt{d_k}\lVert u\rVert_2}.
$$

При почти one-hot attention весь Jacobian становится малым. Низкая $\chi_{\mathrm{sa}}$ может отражать насыщение или направление $u$, а не слабую голову.

## Один входящий motor не контролирует весь оператор

При $a=(1/2,1/2)$ и $u=(1,1)$ имеем $\chi_{\mathrm{sa}}=0$, но $B(a)\ne0$ и $\lVert B(a)\rVert_{\mathrm{op}}=1/2$. Поэтому измерение $\chi_{\mathrm{sa}}$ вдоль фактического градиента не даёт верхней границы на operator norm для всех возмущений. Это существенно при чтении условной Theorem 9.31.

Величина зависит от $g$, а значит от task loss и backward pass. Её нельзя объявить доступной только из forward-геометрии представлений. [Условные оценки и их предпосылки](96-conditional-cone-bounds-and-local-causality.md).
