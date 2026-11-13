---
title: "90. Синтез блока, порядок pullback и границы residual-приближения"
source_pdf_pages: [325, 328]
book_pages: [289, 292]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 90. Синтез блока, порядок pullback и границы residual-приближения

[Индекс библиотеки](../README.md) · [Theorem 9.12](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#theorem-9-12) · [Theorem 9.13](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#theorem-9-13) · [Предыдущая карточка](89-qkv-paths-and-headwise-pullback.md) · [Следующая карточка](91-signature-differential-chain-and-sa-motors.md)

## Схема маршрутов не заменяет композицию реального блока

Автор собирает residual, FFN и headwise Q/K/V вклады:

$$
g_m^-=g_m^++\mathcal M_{\mathrm{FFN}}(g_m^+)
+\sum_h[\mathcal Q_h(g_m^+)+\mathcal K_h(g_m^+)+\mathcal V_h(g_m^+)].
$$

В [Theorem 9.12](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#theorem-9-12) это прямо названо **schematic decomposition**. После доказательства также сказано, что attention-вклады не обязаны быть ортогональными или независимыми.

Для точной производной необходим порядок forward-операций. Рассмотрим редакторский пример без нормировок:

$$
Y=X+A(X),\qquad Z=Y+F(Y).
$$

При обычных forward Jacobian для векторизованных входов и row-covector $g^+$ получаем

$$
g^-=g^+(I+J_F)(I+J_A)
=g^++g^+J_F+g^+J_A+g^+J_FJ_A.
$$

Attention получает сигнал, уже прошедший через FFN residual-блок. Последнее смешанное слагаемое нельзя удалить лишь потому, что в схеме перечислены отдельные ветви. Авторские $\mathcal Q_h,\mathcal K_h,\mathcal V_h$ должны включать полный предшествующий pullback, если схема используется как точное равенство.

При post-norm варианте $Y=N_1(X+A(X))$, $Z=N_2(Y+F(Y))$ порядок становится

$$
g^-=g^+J_{N_2}(I+J_F)J_{N_1}(I+J_A).
$$

Здесь прямой маршрут тоже проходит через нормировки. Нельзя считать его тождественным отображением без дополнительных условий. Сравните [алгебру блока §8.3](../source/0285-0292-ch08-reextraction-and-residual-algebra.md#section-8-3) и [карточку 87](87-feedforward-gates-and-layernorm-motors.md).

## Для глубины нужна производная по всему residual stream

Источник пишет $g_m^{(\ell)}=\partial L_m/\partial X_m^{(\ell)}$ и

$$
g_m^{(\ell)}
=g_m^{(L+1)}J_LJ_{L-1}\cdotsJ_\ell.
$$

Порядок справа существенен: матрицы, вообще говоря, не коммутируют. Компактное $\prod_{r=\ell}^{L}J_r$ в Theorem 9.13 нужно читать в явно раскрытом порядке $L,L-1,\ldots,\ell$, который приведён непосредственно перед теоремой. [PDF 327](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#display-9-ordered-depth-pullback).

**Оговорка об индексе $m$.** Loss может быть связан с одной masked position, но его gradient после attention распространяется на другие позиции через K и V. Для точной рекурсии состояние должно быть $G^{(\ell)}=\partial L_m/\partial\operatorname{vec}X^{(\ell)}$ по всем $n d$ координатам, а $J_\ell$ — полным Jacobian блока. Рекурсия только по строке $X_m$ не замкнута. Индекс $m$ можно сохранять как метку loss-вклада, не ограничивая носитель градиента одной строкой.

## Что даёт малость residual-возмущений

Если для согласованной операторной нормы

$$
J_r=I+E_r,\qquad \|E_r\|\le\varepsilon_r,
$$

то [Proposition 9.14](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#proposition-9-14) даёт

$$
\|g^{(\ell)}\|
\le\|g^{(L+1)}\|\prod_{r=\ell}^{L}(1+\varepsilon_r)
\le\|g^{(L+1)}\|\exp\!\left(\sum_{r=\ell}^{L}\varepsilon_r\right).
$$

Первое неравенство — submultiplicativity, второе — $1+x\le e^x$. Верхняя оценка нормы сама по себе не доказывает сохранение направления или отсутствие затухания.

Более явное редакторское следствие для **близости к identity**:

$$
\left\|J_L\cdotsJ_\ell-I\right\|
\le\prod_r(1+\varepsilon_r)-1
\le e^{\sum_r\varepsilon_r}-1.
$$

Оно получается раскрытием упорядоченного произведения и оценкой каждого произведения возмущений; коммутативность не требуется. Если правая часть мала, итоговый motor близок к исходному, что обосновывает смысл residual-path approximation.

Запись $E_r=J_r-I$ всегда формально возможна, но малость $\|E_r\|$ из архитектуры не следует. [Conjecture 9.15](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#conjecture-9-15) о профиле этих величин по глубине остаётся проверяемой гипотезой. Норму $\|J_r\|$ нельзя подменять нормой $\|J_r-I\|$, а отклик на одну пробную вариацию не удостоверяет полный операторный bound.

Таким образом, источник предоставляет условную оценку распространения производных, а не доказательство того, что attention/FFN можно удалить или что любое residual-обучение устойчиво.

