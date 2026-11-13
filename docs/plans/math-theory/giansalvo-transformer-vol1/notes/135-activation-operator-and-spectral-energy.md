---
title: "135. Оператор активации: геометрия подпространства, Gram-факторы и cancellation"
source_pdf_pages: [425, 427]
book_pages: [389, 391]
batch: "009"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 135. Оператор активации: геометрия подпространства, Gram-факторы и cancellation

[Индекс](../README.md) · [Definitions 12.8–12.9](../source/0425-0428-ch12-activation-operator-and-random-initialisation.md#definition-12-8) · [Remark 12.10](../source/0425-0428-ch12-activation-operator-and-random-initialisation.md#remark-12-10) · [Theorem 12.11](../source/0425-0428-ch12-activation-operator-and-random-initialisation.md#theorem-12-11)

## Что измеряют три наблюдаемые

Для строкового пространства $U=\operatorname{rowspan}(X)$ и его ортопроектора $P$:

$$
\mathcal A=PM_aP,\qquad
\mathcal E=\lVert PM_aP\rVert_F^2,\qquad
\Gamma=\lVert XM_aX^\top\rVert_F.
$$

Оператор $\mathcal A$ сохраняет ту часть кососимметричной формы, которую можно прочитать **обоими аргументами внутри** $U$. Он зависит от подпространства, но не от размеров отдельных токенов. При этом он сохраняет масштаб самого $M_a$: $M_a\mapsto cM_a$ даёт $\mathcal A\mapsto c\mathcal A$, $\mathcal E\mapsto c^2\mathcal E$. Поэтому это не безразмерная и не чисто alignment-величина, вопреки вводной формулировке источника. Для нормированной доли можно отдельно определить $\mathcal E/\lVert M_a\rVert_F^2$ при $M_a\ne0$.

Указанная в книге $\widehat\gamma=\Gamma/(\lVert X\rVert_{\mathrm{op}}^2\lVert M_a\rVert_F)$ устраняет общий масштаб, но сохраняет относительную анизотропию singular values $X$; она тоже не определяется одним только $U$. Эта нормированная величина определена при $X\ne0$ и $M_a\ne0$; сами $\Gamma$ и $\mathcal E$ определены и равны нулю в соответствующих нулевых случаях.

Проектор не зависит от выбора базиса $U$, включая смену ориентации. Ранг $\mathcal A$ чётный и не превосходит $\min(2\lfloor r/2\rfloor,\operatorname{rank}M_a)$. При фиксированном $M_a$ и **вложенных** пространствах $U\subseteq V$ энергия не уменьшается; произвольное вращение или обучение $M_a$ под это утверждение не подпадает.

## Исправленная связь с Gamma

В Remark 12.10 напечатана неверная trace-формула. Из определения нормы напрямую, при $G=X^\top X$, следует

$$
\Gamma^2=\operatorname{tr}(M_a^\top G M_aG).
$$

В книге вместо второго Gram между матрицами стоит выражение, эквивалентное $\operatorname{tr}(M_a^\top M_aG^2)$; эти произведения в общем случае различаются. Если строки $X$ ортонормальны, $G=P$ и действительно $\Gamma^2=\mathcal E$. Более общо, для ненулевого $X$, с наименьшим ненулевым singular value $s_{\min}^+$ и $s_{\max}$:

$$
(s_{\min}^+)^4\mathcal E\le\Gamma^2\le s_{\max}^4\mathcal E.
$$

Это редакторская оценка из SVD: на $U$ матрица $M_a$ с двух сторон умножается на diagonal singular values. Она показывает, почему маленькая raw $\Gamma$ может означать маленький масштаб представлений, а не приближение подпространства к silence.

## Спектральная формула: нужно суммировать до возведения в квадрат

Для $M_a=\sum_k\sigma_k(e_kf_k^\top-f_ke_k^\top)$ точное тождество (12.28):

$$
\mathcal E=2\left\lVert\sum_k\sigma_k(Pe_k\wedge Pf_k)\right\rVert^2.
$$

Коэффициент 2 связывает Frobenius-норму skew-матрицы и норму её bivector. При раскрытии квадрата появляются cross terms между проекциями разных плоскостей. Исходная ортогональность плоскостей до проекции не уничтожает эти слагаемые после проекции.

Редакторский пример в четырёхмерном пространстве: две плоскости с одинаковой $\sigma=1$ и

$$
u=(e_1+e_2)/\sqrt2,\qquad v=(f_1-f_2)/\sqrt2,
\qquad U=\operatorname{span}(u,v).
$$

Тогда $Pe_1\wedge Pf_1=(u\wedge v)/2$, $Pe_2\wedge Pf_2=-(u\wedge v)/2$. Каждая проекция имеет ненулевую площадь, но их взвешенная сумма нулевая, поэтому $\mathcal A=0$ и $\mathcal E=0$. Это показывает возможность полного cancellation и пригодится при чтении active-subspace claims следующего раздела.

Одноплоскостная формула (12.29) верна при явно указанном условии $Pe_\ell\wedge Pf_\ell=0$ для всех $\ell\ne k$. Однако пояснение в доказательстве, заменяющее его произвольным Lagrangian complement, слишком широкое: в Lagrangian пространстве могут взаимно уничтожаться несколько ненулевых projected bivectors. И наоборот, diagonal-only energy верна не только в одноплоскостном случае, но и при попарно ортогональных ненулевых projected bivectors. Эти достаточные условия нужно различать.

Для диагностики вычисляйте весь $PM_aP$, его norm и spectrum. Отдельные projected areas полезны, но их положительная сумма без cross terms может объявить активной полностью silent-конфигурацию.
