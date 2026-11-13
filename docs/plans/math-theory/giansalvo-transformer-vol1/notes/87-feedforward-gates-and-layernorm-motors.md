---
title: "87. FFN gates и LayerNorm как преобразования covector"
source_pdf_pages: [320, 323]
book_pages: [284, 287]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 87. FFN gates и LayerNorm как преобразования covector

[Индекс библиотеки](../README.md) · [§9.6](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#section-9-6) · [§9.7](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#section-9-7) · [Предыдущая карточка](86-mlm-primary-motor-and-computational-graph.md) · [Следующая карточка](88-convex-values-and-softmax-tangent-motor.md)

## FFN передаёт сигнал через локальную производную активации

Для строки $y_m\in\mathbb R^{1\times d}$ источник задаёт

$$
c_m=y_mW_1+b_1,\qquad
r_m=\phi(c_m),\qquad
\operatorname{FFN}(y_m)=r_mW_2+b_2,
$$

где $W_1\in\mathbb R^{d\times h}$, $W_2\in\mathbb R^{h\times d}$. Если входящий covector $g_m=\partial L/\partial\operatorname{FFN}(y_m)$ — строка $1\times d$, то для ReLU

$$
D_m=\operatorname{diag}(\mathbf1_{c_m>0}),\qquad
g_{y_m}=g_mW_2^\top D_mW_1^\top.
$$

Маска отсекает неактивные каналы **этой ветви**. Она не отключает residual-путь и не доказывает бесполезность всей головы или блока. [Proposition 9.6](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#proposition-9-6).

Параметрические вклады также имеют согласованные размерности:

$$
G_{W_2,m}=r_m^\top g_m\in\mathbb R^{h\times d},
\qquad
G_{W_1,m}=y_m^\top\bigl((g_mW_2^\top)\odot\mathbf1_{c_m>0}\bigr)
\in\mathbb R^{d\times h}.
$$

Их ранг не превосходит одного; нулевые активации или сигналы дают нулевой ранг. При другой дифференцируемой активации $D_m$ заменяется на $\operatorname{diag}(\phi'(c_m))$, которая обычно не является бинарной маской. В точке $c_{mj}=0$ ReLU не имеет классической производной: необходим выбор subgradient или явная договорённость реализации. [Формулы PDF 320–321](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#display-9-feedforward-pullback).

## Идеализированная LayerNorm

В источнике

$$
\mu=\frac1d\sum_i x_i,\qquad
\sigma^2=\frac1d\sum_i(x_i-\mu)^2,\qquad
\widehat x=(x-\mu\mathbf1)/\sigma.
$$

Здесь стабилизирующее $\epsilon$ не включено; требуется $\sigma>0$. При $\gamma=\mathbf1$, $\sigma=1$ матрица

$$
P=I-\frac1d\mathbf1\mathbf1^\top-\frac1d\widehat x\widehat x^\top
$$

удаляет направления $\mathbf1$ и $\widehat x$, поскольку $\mathbf1^\top\widehat x=0$ и $\|\widehat x\|^2=d$. Отсюда корректная идеальная формула

$$
\|Pg\|^2=\|g_\perp\|^2-\frac1d(\widehat x^\top g)^2,
\qquad
g_\perp=g-\frac1d(\mathbf1^\top g)\mathbf1.
$$

Это содержание [Proposition 9.7](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#proposition-9-7) в точном базовом случае.

**Уточнение доказательства.** $g_\perp$ и его проекция на $\widehat x$ вообще не ортогональны: вторая является частью первого. Ортогональны направления $\mathbf1$ и $\widehat x$, а также остаток $Pg$ и удалённая радиальная компонента. Исходная фраза об ортогональности сохранена, но формулу нормы следует обосновывать этим разложением.

## Согласование транспонирования и практического epsilon

Ниже — редакторская запись с **векторами-столбцами**. Для

$$
s=\sqrt{\|u\|^2/d+\epsilon},\qquad
u=x-\mu\mathbf1,\qquad
\widehat x=u/s,\qquad
P_\epsilon=I-\frac1d\mathbf1\mathbf1^\top-\frac1d\widehat x\widehat x^\top
$$

forward Jacobian и обратный covector равны

$$
J_{\mathrm{forward}}=\frac1s\operatorname{diag}(\gamma)P_\epsilon,
\qquad
g_x=J_{\mathrm{forward}}^\top g_y
=\frac1sP_\epsilon\operatorname{diag}(\gamma)g_y.
$$

При row-конвенции соответствующие матрицы транспонируются. Напечатанная в книге матрица имеет порядок $P\operatorname{diag}(\gamma)$; поэтому её нельзя одновременно трактовать как column-forward Jacobian и ещё раз бездумно транспонировать. При $\gamma=\mathbf1$ это различие исчезает, что и делает идеальную нормовую формулу верной. [Исходная формула](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#display-9-layernorm-jacobian).

При $\epsilon>0$ радиальное направление подавляется, но не зануляется точно:

$$
D(u/s)[u]=\frac{\epsilon}{s^3}u.
$$

Направление общего сдвига по-прежнему зануляется. Поэтому формулировка «удаляет одно направление, как softmax» слишком груба: идеальная LN без $\epsilon$ удаляет два ортогональных направления; с $\epsilon>0$ радиальная компонента остаётся ослабленной.

## Нормовый фильтр не является безусловным сжатием

Для $\gamma$ и $s$, близких к единице, получается приближённая контрактность. Но большой $|\gamma_i|$ или малое $s$ способны усилить covector. В квадратичной оценке остаток должен масштабироваться как

$$
O\!\left((|1-s|+\|\gamma-\mathbf1\|)\,\|g\|^2\right)
$$

при фиксированной области допустимых масштабов; напечатанный $O(\cdot)$ не может быть равномерным по произвольному $g$ без такого нормирования.

[Figures 9.3–9.4](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#figure-9-3) показывают маршруты, а [Figure 9.5](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#figure-9-5) — синтетическую gate-гистограмму. Эти изображения сами по себе не измеряют контрактность конкретного checkpoint.

