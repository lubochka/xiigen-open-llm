---
title: "357. Lie algebra и softmax: генератор вращения, Jacobian и две simplex convention"
source_pdf_pages: [794, 795]
book_pages: [758, 759]
batch: "016"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 357. Lie algebra и softmax: генератор вращения, Jacobian и две simplex convention

## Вопрос

Где краткие формулировки Appendix A требуют уточнения, чтобы не изменить математический смысл?

**Кососимметричная матрица задаёт генератор вращения; сама матрица не равна вращению на свой spectral parameter. Softmax увеличивает собственную вероятность при росте собственного logit, но уменьшает остальные.** [§A.2](../source/0792-0796-appendix-a-mathematical-background.md#section-a-2), [§A.3](../source/0792-0796-appendix-a-mathematical-background.md#section-a-3).

## Генератор и экспонента

На invariant plane skew matrix имеет форму

$$
A=\begin{pmatrix}0&-\theta\\\theta&0\end{pmatrix}.
$$

Тогда $Av$ — поворот на прямой угол с масштабом $|\theta|$ и ориентацией по знаку, а

$$
e^A=\begin{pmatrix}\cos\theta&-\sin\theta\\
\sin\theta&\cos\theta\end{pmatrix}
$$

— вращение на угол $\theta$. Поэтому фраза источника “$A$ acts as a 2D rotation by $\theta_k$” смешивает $A$ и $\exp(A)$. Для $\theta=0$ это особенно явно: $A=0$, тогда как $\exp(A)=I$. Формулы (A.5)–(A.7) и утверждение об экспоненте сохраняются как напечатаны; editor correction не подменяет source.

## Что следует из Jacobian softmax

При конечных logits и $n\ge2$

$$
\frac{\partial p_i}{\partial z_i}=p_i(1-p_i)>0,\qquad
\frac{\partial p_i}{\partial z_j}=-p_ip_j<0\quad(j\ne i).
$$

Фраза “strictly increasing in each $z_i$” корректна для соответствующей компоненты $p_i$, а не для всех вероятностей одновременно. При $n=1$ softmax константен.

Для любого $v$,

$$
v^\top Jv=\sum_ip_iv_i^2-\left(\sum_ip_iv_i\right)^2\ge0.
$$

Это variance под распределением $p$. При всех $p_i>0$ она нулевая ровно на константных $v$, что объясняет null space $\operatorname{span}\mathbf1$ и rank $n-1$. Row sums абсолютных значений равны $2p_i(1-p_i)\le1/2$, поэтому для симметричного $J$ имеем $\|J\|_2\le1/2$. Это даёт global Euclidean Lipschitz bound softmax; оно не доказывает устойчивость полного deep block, включающего другие операторы.

## Открытый и закрытый simplex

В §A.3 $\Delta^{n-1}$ обозначает положительные вероятности; в (A.12) тот же символ обозначает замкнутый simplex с $p_i\ge0$. При конечных logits image softmax — **relative interior** последнего. Нули достигаются лишь как предел либо после отдельной masking convention. При переносе формул в код следует сохранить различие, особенно для masked softmax и logarithms.

