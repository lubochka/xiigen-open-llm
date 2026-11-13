---
title: "158. Log-centred softmax: точное тождество и необходимая инъективность values"
source_pdf_pages: [474, 475]
book_pages: [438, 439]
batch: "010"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 158. Log-centred softmax: точное тождество и необходимая инъективность values

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Точное линейное тождество

Для положительного row-stochastic $A$ определён $\widetilde A=(\log A)C_n$, где $C_n=I-\mathbf1\mathbf1^\top/n$. При $A=\operatorname{softmax}(L)$ построчные нормализаторы уничтожаются:

$$
\widetilde{\operatorname{softmax}}(L)=LC_n.
$$

Поэтому сложение logits даёт точное сложение log-centred координат. При $L=L_s+L_a$ и обозначениях source TrA означает сохранение $L_a$, TrB — сохранение $L_s$. В этих координатах действительно

$$
\Delta\widetilde A_{\mathrm{both}}
=\Delta\widetilde A_{\mathrm{TrA}}+\Delta\widetilde A_{\mathrm{TrB}}.
$$

Это линейность **после взятия log и удаления row gauge**. Она не устанавливает $D_{\mathrm{both}}=D_{\mathrm{TrA}}+D_{\mathrm{TrB}}$ для attention-output differences. [Формулы 13.33–13.37](../source/0472-0475-ch13-activation-and-intervention-bounds.md#equation-13-33).

## Forward Lipschitz bound

Для $p=\operatorname{softmax}(l)$ Jacobian равен $J=\operatorname{diag}(p)-pp^\top$, и $\|J\|_{\mathrm{op}}\le1/2$. Например, абсолютная сумма элементов строки равна $2p_i(1-p_i)\le1/2$, а матрица симметрична. Это даёт единый Euclidean/Frobenius bound независимо от минимального attention weight. При $V=XW_VW_O$,

$$
\|D_X\|_F\le\tfrac12\|V\|_{\mathrm{op}}\|\Delta\widetilde A_X\|_F.
$$

Нижняя граница $\epsilon>0$ нужна для Lipschitz map $A\mapsto\log A$, а не для forward softmax. [Формулы 13.38–13.40](../source/0472-0475-ch13-activation-and-intervention-bounds.md#equation-13-38).

## Где обратная оценка источника требует исправления

Из full **column** rank $V\in\mathbb R^{n\times d}$ не следует возможность восстановить произвольное левое возмущение по $DV$. Нужна инъективность right-multiplication на используемом пространстве. Full **row** rank достаточен. Для разностей row-stochastic матриц более точное условие:

$$
m_V:=\inf_{z\perp\mathbf1,\ \|z\|_2=1}\|z^\top V\|_2>0.
$$

Если $z\perp\mathbf1$ и $z^\top V=0$, ненулевые attention differences вдоль $z$ могут стать невидимыми на выходе. У обычной головы $\operatorname{rank}V\le d_k$; при $d_k<n-1$ требуемой инъективности на всём $\mathbf1^\perp$ быть не может. [Ошибочная обратная оценка 13.41](../source/0472-0475-ch13-activation-and-intervention-bounds.md#equation-13-41).

При всех attention entries обеих сравниваемых матриц не меньше $\epsilon$ и $m_V>0$ корректно

$$
\|\Delta\widetilde A_X\|_F\le\frac{\|D_X\|_F}{\epsilon m_V}.
$$

Подстановка в parallelogram inequality даёт coefficient $\|V\|_{\mathrm{op}}^2/(2\epsilon^2m_V^2)$ для квадратичных output deviations. Он зависит от геометрии attention и values; заменить его универсальным 2 нельзя. Проверенный контрпример к такой замене приведён в [159](159-joint-intervention-counterexample-and-curvature.md).
