---
title: "110. Поток в so(d), нормальная форма и ориентированные площади"
source_pdf_pages: [374, 377]
book_pages: [338, 341]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 110. Поток в so(d), нормальная форма и ориентированные площади

[Индекс библиотеки](../README.md) · [§§11.5–11.5.1](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#section-11-5) · [Предыдущая карточка](109-layer-observables-and-activation-normalisations.md) · [Следующая карточка](111-fixed-point-classes-and-rank-gap-consistency.md)

## Что следует из антисимметричной проекции

Для вещественной квадратной матрицы определим

$$
P_a(Z)=\frac{Z-Z^\top}{2},\qquad
P_s(Z)=\frac{Z+Z^\top}{2}.
$$

Если **независимая матрица $M$** обучается евклидовым gradient flow $\dot M=-\eta G_M$, то линейность проекции даёт

$$
\dot M_a=P_a(\dot M)=-\eta P_a(G_M)=-\eta G_a.
$$

Это корректная алгебраическая часть Proposition 11.7. $\mathfrak{so}(d)=\{A:A^\top=-A\}$ — векторное пространство размерности $d(d-1)/2$. Принадлежность $M_a$ этому пространству сохраняется. [Proposition 11.7, (11.3)](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#proposition-11-7).

Но проекции градиента могут зависеть одновременно от $M_s,M_a$, полного residual stream, softmax и задачи. Ортогональность подпространств не даёт независимости двух динамических систем.

Для фактической факторизации $M=W_QW_K^\top$ обычный gradient flow по двум факторам имеет вид

$$
\dot W_Q=-\eta G_MW_K,\qquad
\dot W_K=-\eta G_M^\top W_Q,
$$

откуда

$$
\dot M
=-\eta\left(G_MW_KW_K^\top+W_QW_Q^\top G_M\right),
\qquad
\dot M_a=P_a(\dot M).
$$

В общем случае это не $-\eta G_a$. Формулу (11.3) можно использовать как flow по независимому $M$ либо после явного переопределения эффективного motor; подмена двух смыслов требует отдельного обоснования. [Исходная формула](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#equation-11-3), [подробный разбор факторизованного обучения](99-two-flow-gradient-and-factorised-training.md).

## Инвариантность пространства не сохраняет ненулевую направленность

Remark 11.8 утверждает, что directional и reciprocal heads не смешиваются. Из $M_a(t)\in\mathfrak{so}(d)$ этого не следует: **нулевая матрица тоже принадлежит $\mathfrak{so}(d)$**. Например, для $\mathcal L(M)=\frac12\lVert M_a\rVert_F^2$ прямой flow даёт

$$
M_a(t)=e^{-\eta t}M_a(0).
$$

При фиксированном ненулевом $M_s$ полная матрица стремится к чисто симметричной. Один дискретный шаг с размером шага 1 по этому квадратичному loss вообще обнуляет $M_a$. Сохранение антисимметрии не означает сохранения ранга, нормы, спектрального разрыва или класса головы. [Remark 11.8 на PDF 374–375](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#remark-11-8).

## Точная нормальная форма

Содержательное алгебраическое ядро Proposition 11.9:

$$
M_a=\sum_{k=1}^{K}\lambda_k
(q_kp_k^\top-p_kq_k^\top),
\qquad
\lambda_k>0,\qquad
K=\frac{\operatorname{rank}M_a}{2}.
$$

Пары $(q_k,p_k)$ ортонормальны и взаимно ортогональны. Каждая плоскость несёт блок $\left[\begin{smallmatrix}0&\lambda_k\\-\lambda_k&0\end{smallmatrix}\right]$, собственные значения $\pm i\lambda_k$ и **две одинаковые сингулярные величины** $\lambda_k,\lambda_k$. Ядро составляет оставшиеся $d-2K$ направлений. [Proposition 11.9, (11.4)](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#proposition-11-9), [Figure 11.10](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#figure-11-10).

Матрица имеет чётный ранг $2K\le2\lfloor d/2\rfloor$; это не размерность пространства $\mathfrak{so}(d)$. Для QK-факторизации дополнительно

$$
\operatorname{rank}M\le d_K,\qquad
\operatorname{rank}M_a\le
\min\!\left(2d_K,\,2\lfloor d/2\rfloor\right).
$$

При этом ранг $M_a$ может превосходить ранг $M$: $M=e_1e_2^\top$ имеет ранг 1, а $(M-M^\top)/2$ — ранг 2. Эти редакторские следствия помогают не смешивать head width, matrix rank и число eigenplanes.

Уточнение к заявленной уникальности: при совпадающих $\lambda_k$ можно смешивать плоскости внутри соответствующего вырожденного подпространства. Неоднозначность шире отдельных вращений каждой уже выбранной плоскости. При конвенции $\lambda_k>0$ смена знака связана со сменой ориентации базиса, а не является независимой свободой положительного коэффициента. [Формулировка об уникальности](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#proposition-11-9).

## Симплектическая форма и её область

На каждой активной плоскости форма $\omega_k(u,v)=u^\top M_{a,k}v$ невырождена. На полном $\mathbb R^d$ она имеет ядро, если $2K<d$, поэтому всю форму в этом случае нельзя называть невырожденной симплектической формой. Источник явно выписывает $\omega_k(q_k,p_k)=\lambda_k$; для стандартной Darboux-нормировки со значением 1 нужны дополнительно рескалированные координаты, например $q_k/\sqrt{\lambda_k},p_k/\sqrt{\lambda_k}$. Ортонормальный спектральный базис и нормированный Darboux-базис выполняют разные задачи. [Remark 11.10](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#remark-11-10).

## Scores как сумма взвешенных площадей

Формула (11.5) даёт

$$
(XM_aX^\top)_{ij}
=\sum_k\lambda_k
\det\begin{pmatrix}
x_i\cdot q_k&x_i\cdot p_k\\
x_j\cdot q_k&x_j\cdot p_k
\end{pmatrix}.
$$

Каждый определитель — ориентированная площадь; итоговый score — их **взвешенная сумма**. Знак описывает ориентацию в выбранной плоскости. Он не задаёт автоматически временной или транзитивный порядок токенов: три вектора с углами $0^\circ,120^\circ,240^\circ$ дают циклические положительные ориентированные сравнения. Вклады разных плоскостей могут компенсироваться. [Remark 11.11 и Figure 11.9](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#remark-11-11).

Поскольку $\mathcal D=XM_aX^\top$ антисимметрична, её ранг также чётен:

$$
\operatorname{rank}\mathcal D
\le\min(\operatorname{rank}M_a,\operatorname{rank}X)
\le\min(d,n).
$$

С учётом чётности получается (11.6). Переход $M_a\mapsto\mathcal D$ может уменьшить число активных плоскостей, вплоть до нуля; он не обязан переносить все $K$ плоскостей с прежними амплитудами. Это критично для следующей классификации. [Corollary 11.12](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#corollary-11-12).

