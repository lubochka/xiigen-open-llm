---
title: "61. Остаточный поток, якобиан внимания и двумерная активация"
source_pdf_pages: [252, 256]
book_pages: [216, 220]
batch: "006"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 61. Остаточный поток, якобиан внимания и двумерная активация

[Индекс библиотеки](../README.md) · [Исходные §§7.11–7.13](../source/0251-0257-ch07-signature-transport-and-closed-locus.md#section-7-11) · [Предыдущая карточка](60-cone-margin-entropy-and-coherence-profiles.md) · [Следующая карточка](62-uniform-criticality-and-first-signature-direction.md)

## Остаточная добавка меняет билинейную форму четырьмя слагаемыми

При $X=X_0+\Delta$ и $\Gamma(X;M)=\|XM_aX^\top\|_F$ разложение имеет вид

$$
XM_aX^\top=X_0M_aX_0^\top+X_0M_a\Delta^\top
+\Delta M_aX_0^\top+\Delta M_a\Delta^\top.
$$

Два смешанных слагаемых линейны по $\Delta$, последнее квадратично. Triangle inequality и смешанные operator/Frobenius bounds дают

$$
\Gamma(X;M)\le\Gamma(X_0;M)
+2\|X_0\|_F\|\Delta\|_F\|M_a\|_{\mathrm{op}}
+\|\Delta\|_F^2\|M_a\|_{\mathrm{op}}.
$$

Это [Proposition 7.19](../source/0251-0257-ch07-signature-transport-and-closed-locus.md#proposition-7-19), доказательство которой продолжается на PDF 254. Если дрейф большой, квадратичное слагаемое нельзя отбрасывать. Если линейные вклады обращаются в нуль, квадратичное может быть ведущим и при малом дрейфе.

Название «amplification bound» не означает обязательного усиления. Верхняя граница допускает и уменьшение, и полное обнуление энергии; например, $\Delta=-X_0$ даёт X=0. Редакторское усиление той же оценки по reverse triangle inequality:

$$
|\Gamma(X;M)-\Gamma(X_0;M)|
\le(2\|X_0\|_F\|\Delta\|_F+\|\Delta\|_F^2)\|M_a\|_{\mathrm{op}}.
$$

Здесь M фиксирован. Одновременное изменение M при обучении требует отдельного члена, которого в данной формуле нет.

## Нормировка отделяет масштабы, но имеет особые случаи

В §§7.10.2 и 7.13 автор вводит

$$
\widehat\gamma(X;M)=
\frac{\|XM_aX^\top\|_F}{\|X\|_{\mathrm{op}}^2\|M_a\|_F}.
$$

При **X≠0 и $M_a\ne0$** submultiplicativity даёт $0\le\widehat\gamma\le1$. Общие ненулевые скалярные множители X и $M_a$ сокращаются, поэтому это показатель согласования представления с формой, а не просто размера весов. [Proposition 7.17](../source/0251-0257-ch07-signature-transport-and-closed-locus.md#proposition-7-17), [Definition 7.21](../source/0251-0257-ch07-signature-transport-and-closed-locus.md#definition-7-21).

В оригинале отдельно оговорено только $M_a=0$. При X=0 и ненулевом $M_a$ остаётся 0/0: фраза «for every X» требует исправления области определения или явного соглашения. Даже если условиться возвращать нуль, непрерывности в X=0 нет: для $X=\varepsilon I$ и фиксированного ненулевого skew $M_a$ отношение равно 1 при любом $\varepsilon\ne0$. Добавка epsilon в знаменатель меняет измеряемую величину и должна быть указана как практическая модификация.

## Чувствительность внимания зависит от конкретных данных

Для

$$
F_h(X)=P_h(X)XW_V^{(h)},\qquad
P_h(X)=\operatorname{softmax}_{\mathrm{row}}(XM_hX^\top/\sqrt{d_k})
$$

производная содержит прямую value-ветвь и изменение самой карты внимания:

$$
DF_h(X)[E]=P_h(X)EW_V^{(h)}+DP_h(X)[E]XW_V^{(h)}.
$$

Используя $\|J_{\mathrm{softmax}}\|_{2\to2}\le1/2$, автор получает, для Frobenius-нормы входной и выходной матриц,

$$
\|DF_h(X)\|
\le\|P_h(X)\|_{\mathrm{op}}\|W_V^{(h)}\|_{\mathrm{op}}
+\frac{2c_{\mathrm{sm}}}{\sqrt{d_k}}\|XW_V^{(h)}\|_F
\|M_h\|_{\mathrm{op}}\|X\|_{\mathrm{op}}.
$$

Это [Proposition 7.20](../source/0251-0257-ch07-signature-transport-and-closed-locus.md#proposition-7-20). В первой, более точной версии sup следует читать по E≠0. Для целого multi-head block сумма head bounds, умноженная на норму $W_O$, является допустимой, хотя грубой, верхней границей.

Row-stochastic матрица не обязана иметь operator norm ≤1: матрица, все строки которой равны одному one-hot вектору, имеет норму $\sqrt n$; finite-logit softmax может сколь угодно близко её аппроксимировать. Поэтому подмена $\|P_h\|_{\mathrm{op}}$ единицей без дополнительных условий некорректна. Оценка относится к указанному attention block; residual addition, LayerNorm и MLP добавляют свои производные.

## Направленная активация требует площади, а не одной линии

**Proposition 7.22** рассматривает ортонормальную каноническую eigenplane E=span(u,v) кососимметричной матрицы с $M_au=\sigma v$, $M_av=-\sigma u$. При проекциях x,y на эту плоскость существенна ориентированная площадь

$$
a=\det\begin{pmatrix}
\langle x,u\rangle&\langle x,v\rangle\\
\langle y,u\rangle&\langle y,v\rangle
\end{pmatrix}.
$$

Если другие eigenplanes не вносят вклада, то $|x^\top M_ay|=\sigma|a|$. Если вклады есть, reverse triangle inequality даёт нижнюю границу с вычитанием **модуля** их суммы. Величина $\varepsilon_E$ должна быть неотрицательной оценкой остаточного вклада; исходное слово «contribution» без модуля недостаточно. [Утверждение](../source/0251-0257-ch07-signature-transport-and-closed-locus.md#proposition-7-22) и [доказательство](../source/0251-0257-ch07-signature-transport-and-closed-locus.md#pdf-0256).

Есть и знаковая оговорка: при напечатанном действии $M_a$ на u,v форма $x^\top M_ay$ равна **−σa**, тогда как доказательство печатает +σa. Итоговое равенство с модулем сохраняется. Ненулевая площадь пары в row span показывает возможность ненулевой активации, но сама по себе не задаёт количественной нижней границы для нормированной $\widehat\gamma$ без контроля геометрии и масштаба строк X.
