---
title: "97. Каноническая QK-пара, S–A геометрия и два разных balance index"
source_pdf_pages: [342, 350]
book_pages: [306, 314]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 97. Каноническая QK-пара, S–A геометрия и два разных balance index

[Индекс](../README.md) · [Начало главы 10](../source/0342-0346-ch10-canonical-pair-and-score-splitting.md#chapter-10) · [Theorem 10.3](../source/0342-0346-ch10-canonical-pair-and-score-splitting.md#theorem-10-3) · [Далее: дифференциальный переход](98-first-order-softmax-adjoint-and-signature-response.md)

## Что здесь канонично

При фиксированном входе $X$ билинейные scores зависят от факторов через

$$
M=W_QW_K^\top,\qquad L=XMX^\top.
$$

Преобразование $W_Q\mapsto W_QA$, $W_K\mapsto W_KA^{-\top}$ при обратимой $A$ сохраняет $M$. Это **Proposition 10.2**. Каноничность относится к внутреннему Q/K gauge; она не означает, что из одной наблюдаемой матрицы $L$ можно однозначно восстановить $M$ при произвольном $X$.

В этой части главы scores записаны **без** $1/\sqrt{d_k}$. При переносе формул в обычный scaled attention нужно включить этот множитель либо в score map, либо в соответствующую производную, сохранив единое соглашение.

## Ортогональное разложение до softmax

Для любой вещественной квадратной матрицы:

$$
M_s=\frac{M+M^\top}{2},\qquad M_a=\frac{M-M^\top}{2},
\quad
\langle M_s,M_a\rangle_F=0.
$$

Следовательно, $\lVert M\rVert_F^2=\lVert M_s\rVert_F^2+\lVert M_a\rVert_F^2$. Аналогично

$$
L_s=XM_sX^\top,\quad L_a=XM_aX^\top,\quad
\lVert L\rVert_F^2=\lVert L_s\rVert_F^2+\lVert L_a\rVert_F^2.
$$

[Theorem 10.3 и Proposition 10.4](../source/0342-0346-ch10-canonical-pair-and-score-splitting.md#theorem-10-3) — точные линейно-алгебраические результаты. Они не требуют предположения, что обучение выберет конкретный баланс.

Симметрия scores означает взаимность до row-softmax; после разных строковых нормировок attention matrix может не быть симметричной. Кососимметричная часть описывает ориентированное сравнение, но её наличие само по себе не доказывает кодирование абсолютного порядка или семантическую функцию головы.

## В источнике два разных \(\alpha_{\mathrm{SA}}\)

| Место | Формула | От чего зависит |
|---|---|---|
| PDF 345, §10.4 | $\alpha_{\mathrm{score}}=\Gamma_a/(\Gamma_s+\Gamma_a+\varepsilon)$ | $M$, конкретный $X$, stabiliser; $\Gamma_s=\lVert XM_sX^\top\rVert_F$, $\Gamma_a=\lVert XM_aX^\top\rVert_F$ |
| PDF 350, §10.9.1 | $\alpha_{\mathrm{param}}=\lVert M_a\rVert_F^2/(\lVert M_s\rVert_F^2+\lVert M_a\rVert_F^2)$ | $M$ в текущей системе координат; определена при $M\ne0$ |

Обозначения $\alpha_{\mathrm{score}}$ и $\alpha_{\mathrm{param}}$ введены **редактором** для разведения исходного общего имени. [Первая формула](../source/0342-0346-ch10-canonical-pair-and-score-splitting.md#display-10-score-balance), [вторая формула](../source/0347-0350-ch10-softmax-adjoint-and-two-flow.md#display-10-parameter-balance).

На [PDF 348](../source/0347-0350-ch10-softmax-adjoint-and-two-flow.md#section-10-8) текст говорит, что $\alpha_{\mathrm{SA}}$ вычисляется до действия $X$. Это **несовместимо с определением §10.4**, хотя соответствует более поздней parameter-side идее.

Простой пример различия:

$$
M_s=\begin{pmatrix}3&0\\0&0\end{pmatrix},\quad
M_a=\begin{pmatrix}0&1\\-1&0\end{pmatrix},\quad
X=\operatorname{diag}(t,1),\ t>0.
$$

Тогда $\alpha_{\mathrm{param}}=2/11$, а

$$
\alpha_{\mathrm{score}}
=\frac{\sqrt2\,t}{3t^2+\sqrt2\,t+\varepsilon}
$$

меняется вместе с входом. Эти индексы нельзя складывать в один временной ряд или сравнивать без указания определения.

Изменение самого $X$ при фиксированном $M$ отличается от согласованной смены координат $X'=XR$, $M'=R^{-1}MR^{-\top}$. При последней $L'_s=L_s$ и $L'_a=L_a$, поэтому $\Gamma_s,\Gamma_a$ и $\alpha_{\mathrm{score}}$ сохраняются даже для неортогональной обратимой $R$. Параметрические Frobenius-нормы $M_s,M_a$ и $\alpha_{\mathrm{param}}$ при такой смене могут измениться.

## «Облако на прямой»: линейная и аффинная размерность

**Proposition 10.5** корректно утверждает чётность ранга вещественной кососимметричной матрицы. Если $\operatorname{rank}X\le1$, то $XM_aX^\top=0$: все строки — кратные одному вектору $v$, а $v^\top M_av=0$.

Однако геометрическая **аффинная** прямая не обязана проходить через начало. Для

$$
X=\begin{pmatrix}1&0\\1&1\end{pmatrix},\qquad
M_a=\begin{pmatrix}0&1\\-1&0\end{pmatrix}
$$

центрированное облако одномерно, но $XM_aX^\top=M_a\ne0$. Фразу источника о collapse to a line следует применять к линейному span, а не автоматически к centered SVD.

Иллюстрации главы задают схему интерпретации. Они не устанавливают причинную необходимость S/A-компоненты, измеренный training-time drift или эффект для качества Xiigen.


## Продолжение в проходе 008

[Карточка 100](100-parameter-balance-evolution-and-induced-flow.md) продолжает balance derivative, [102](102-sars-v8-aggregate-trace-and-score-conventions.md) фиксирует отдельные эмпирические score conventions. [Карточка 109](109-layer-observables-and-activation-normalisations.md) различает новые α, γ и Γ главы 11.
