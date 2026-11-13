---
title: "98. Первый порядок softmax, сопряжённый motor и скорость сигнатуры"
source_pdf_pages: [346, 349]
book_pages: [310, 313]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 98. Первый порядок softmax, сопряжённый motor и скорость сигнатуры

[Индекс](../README.md) · [Theorems 10.6–10.7](../source/0347-0350-ch10-softmax-adjoint-and-two-flow.md#theorem-10-6) · [Proposition 10.8](../source/0347-0350-ch10-softmax-adjoint-and-two-flow.md#proposition-10-8) · [Далее: обучение факторов](99-two-flow-gradient-and-factorised-training.md)

## Линейность производной при общем базовом состоянии

Пусть $A=\operatorname{softmax}_{\mathrm{row}}(L)$ и $dL=dL_s+dL_a$. При одном и том же фиксированном $L$:

$$
dA_s=D\operatorname{softmax}_L[dL_s],\qquad
dA_a=D\operatorname{softmax}_L[dL_a],\qquad
dA=dA_s+dA_a.
$$

Это **Theorem 10.6**. Row sums каждого $dA$ равны нулю, поскольку суммарная вероятность строки остаётся 1 в первом порядке.

Из этого не следует $\operatorname{softmax}(L_s+L_a)=\operatorname{softmax}(L_s)+\operatorname{softmax}(L_a)$. Нельзя также вычислить один производный вклад в $L_s$, другой в $L_a$ и ожидать их суммы в качестве производной при $L_s+L_a$: изменился бы базовый Jacobian.

Обозначения symmetric-driven и antisymmetric-driven описывают **источник возмущения**. После row-softmax $dA_s$ не обязан быть симметричным, $dA_a$ — кососимметричным, а два образа не обязаны оставаться Frobenius-ортогональными. Линейный map сохраняет сложение, но не автоматически углы или исходные матричные подпространства.

## Обратный переход к канонической паре

При фиксированном $X$ и $L=XMX^\top$:

$$
dL=XdMX^\top,\qquad
\langle G_L,XdMX^\top\rangle_F
=\langle X^\top G_LX,dM\rangle_F.
$$

Поэтому $G_M=X^\top G_LX$. Его проекции

$$
G_s=\frac{G_M+G_M^\top}{2},\qquad
G_a=\frac{G_M-G_M^\top}{2}
$$

дают **Theorem 10.7**:

$$
\langle G_M,dM_s+dM_a\rangle_F
=\langle G_s,dM_s\rangle_F+\langle G_a,dM_a\rangle_F.
$$

Это равенство первого изменения loss в согласованной Frobenius-метрике. При scaled scores $L=XMX^\top/\sqrt{d_k}$ сопряжённая формула получает тот же $1/\sqrt{d_k}$.

Если меняются одновременно $M$ и представление $X$, полный score differential включает ещё

$$
dL=(dX)MX^\top+X(dM)X^\top+XM(dX)^\top.
$$

Формула для одного parameter-side вклада не описывает сама по себе все пути полного обучения.

## Производная сигнатуры требует гладкости всего выбранного отображения

Если **само** отображение $\Sigma$ дифференцируемо в $A$, то

$$
d\Sigma_s=D\Sigma_A[D\operatorname{softmax}_L[dL_s]],\qquad
d\Sigma_a=D\Sigma_A[D\operatorname{softmax}_L[dL_a]]
$$

и $d\Sigma=d\Sigma_s+d\Sigma_a$. Это смысл **Proposition 10.8**.

Существование производной ближайшей матрицы в выпуклой оболочке ещё не гарантирует гладкость всех координат сигнатуры. Нужны фиксированный словарь, устойчивое правило выбора неоднозначных barycentric coefficients и обработка переходов активной грани. Если residual coordinate использует норму ошибки, она может быть недифференцируема при нулевом остатке даже при гладкой проекции. Например, расстояние до фиксированной точки имеет вид $\lVert h\rVert$ и не обладает линейной производной при $h=0$.

Поэтому фразу «на страте дифференцируемости» следует относить ко **всей используемой сигнатуре**, а в точках излома применять направленные производные или явно заданное piecewise/hybrid описание. Это редакторское уточнение области Proposition 10.8.

## Что известно об эмпирическом подтверждении

На [PDF 349](../source/0347-0350-ch10-softmax-adjoint-and-two-flow.md#pdf-0349) сам автор отделяет score/attention traces от полного измерения $\partial_t\mu_\ell$ и canonical residuals. Упоминание последующих опытов — анонс; они не прочитаны в этом пакете.

Для проекта полезно сохранять отдельно исходное возмущение, полный $dL$, tangent response, выбранную сигнатуру и активную страту. Чувствительность первого порядка не заменяет измерение ошибки конечной замены и качества задачи.
