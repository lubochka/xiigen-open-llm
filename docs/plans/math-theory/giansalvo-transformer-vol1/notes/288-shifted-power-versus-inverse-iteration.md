---
title: "288. Spectral shift: степенная и обратная итерации выбирают разные endpoints"
source_pdf_pages: [692, 696]
book_pages: [656, 660]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 288. Spectral shift: степенная и обратная итерации выбирают разные endpoints

## Вопрос

Ускоряет ли выбор $\mu\approx\lambda_2$ поиск верхнего eigenvector в (17.16)?

**Только при контроле всех остальных shifted eigenvalues.** Умножение на $A-\mu I$ выбирает наибольшее расстояние $|\lambda_j-\mu|$, а inverse iteration — наименьшее. Уничтожение одной компоненты около $\lambda_2$ не обеспечивает правильный target. [Shifted power (17.16–17.17)](../source/0696-0698-ch17-shifted-power-and-rayleigh-quotient-iteration.md#equation-17-16), [inverse strategy B1](../source/0689-0692-ch17-dwell-time-and-spectral-degeneracy.md#pdf-0692).

## Полный коэффициент сжатия

Для $u_0=\sum_j c_jv_j$ после $t$ ненормированных степенных шагов

$$
(A-\mu I)^tu_0
=\sum_j c_j(\lambda_j-\mu)^tv_j.
$$

Пусть целевой индекс $k$ удовлетворяет $c_k\ne0$ и $|\lambda_k-\mu|>\max_{j\ne k}|\lambda_j-\mu|$. Тогда projective direction сходится к $v_k$ с коэффициентом

$$
\rho_k(\mu)=
\frac{\max_{j\ne k}|\lambda_j-\mu|}{|\lambda_k-\mu|}<1.
$$

Возможная смена знака нормированного вектора не меняет projective convergence. При равенстве максимальных модулей единственный target не выделяется. При нулевом исходном overlap нужная компонента не возникает.

Исходная (17.17) содержит правильное указание на второго конкурента **по абсолютному расстоянию**, но следующий абзац подменяет его исходным $\lambda_2$. Контрпример:

$$
A=\operatorname{diag}(3,2,0),\qquad
\mu=\lambda_2=2,\qquad
A-\mu I=\operatorname{diag}(1,0,-2).
$$

При ненулевой третьей компоненте метод сходится по направлению к eigenvector исходного eigenvalue $0$, а не $3$. Shift точно известен, поэтому ошибка вызвана не неточностью estimate. Уничтоженная middle component перестала быть основным конкурентом; им стал нижний endpoint. [Применение shift к gate](../source/0696-0698-ch17-shifted-power-and-rayleigh-quotient-iteration.md#pdf-0696).

## Что меняется при обращении

Inverse iteration с фиксированным $\mu\notin\sigma(A)$ использует собственные значения $(\lambda_j-\mu)^{-1}$. Поэтому target должен быть единственным **ближайшим** к shift, а коэффициент равен

$$
\frac{|\lambda_k-\mu|}
{\min_{j\ne k}|\lambda_j-\mu|}.
$$

Это другой оператор и другая численная задача: надо решать линейную систему. RQI (17.18) адаптирует именно inverse shift; полезность Rayleigh estimate не означает, что она исправляет степенное умножение простым обновлением $\mu$. [RQI (17.18)](../source/0696-0698-ch17-shifted-power-and-rayleigh-quotient-iteration.md#equation-17-18).

## Ограничения авторской applicability

Для Type A условие $\mu\ne\lambda_1$ недостаточно: shifted magnitude кратного блока должен превосходить весь остальной спектр. Например, у $\operatorname{diag}(1,1,0)$ при $\mu=0.9$ метод выбирает нулевой eigenvalue, поскольку $0.9>0.1$. Даже при правильном выборе один начальный вектор даёт одно направление внутри кратного eigenspace, а не весь projector.

Фраза, что shift около kernel eigenvalue делает **leading singular values** малыми, также не следует из близости к нулю одной eigenvalue. У $\operatorname{diag}(3,2,0)$ при $\mu=0$ крупнейшее singular value равно $3$. Для степенного шага опасно малое $\|(A-\mu I)u\|$ конкретного текущего вектора; для inverse solve — малое минимальное singular value матрицы. Эти две причины нельзя объединять в одну универсальную «kernel pathology».

Полезный контракт для реализации — оценивать весь доступный spectral interval, проверять целевой endpoint и residual, обрабатывать ties/zero vectors и отделять multiplication cost от solve cost. Из одной точности $\widehat\lambda_2$ гарантированное ускорение gate не следует.
