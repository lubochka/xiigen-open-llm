---
title: "295. Regularised cluster: правильное ядро Davis–Kahan и роль выбора basis"
source_pdf_pages: [704, 706]
book_pages: [668, 670]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 295. Regularised cluster: правильное ядро Davis–Kahan и роль выбора basis

## Какой bias контролирует Lemma 17.10

Для симметричного $A$ и верхнего кластера размера $m$ source различает внутреннюю ширину $\delta_{\mathrm{in}}=\lambda_1-\lambda_m$ и внешний gap $\delta_{\mathrm{out}}=\lambda_m-\lambda_{m+1}>0$. При perturbation $E=\varepsilon_{\mathrm{reg}}R$ и $\|E\|_2\le\delta_{\mathrm{out}}/2$ корректное spectral ядро даёт

$$
\|P_{\widetilde V_m}-P_{V_m}\|_2
=\sin\theta_{\max}
\le\frac{\|E\|_2}{\delta_{\mathrm{out}}-\|E\|_2}
\le\frac{2\|E\|_2}{\delta_{\mathrm{out}}}.
$$

Здесь $\widetilde V_m$ — соответствующее верхнее spectral subspace именно $A+E$. Редакторская запись через projector уточняет смысл «principal angles»: bound контролирует максимальный угол. [Lemma 17.10](../source/0703-0705-ch17-unified-gate-algorithm-and-regularised-block-flow.md#lemma-17-10), [(17.39)–(17.40)](../source/0706-0709-ch17-subspace-bias-and-conditional-convergence.md#equation-17-39).

В доказательстве есть локальная неточность: расстояние **между двумя возмущёнными** кластерами по Weyl оценивается снизу через $\delta_{\mathrm{out}}-2\|E\|_2$, поскольку могут сдвинуться обе границы. Для $A=\operatorname{diag}(2,1)$ и $E=\operatorname{diag}(-0.4,0.4)$ новый gap равен $1.6-1.4=0.2$, а не как минимум $1-0.4=0.6$. Это не опровергает итоговый sin-angle bound: его знаменатель $\delta_{\mathrm{out}}-\|E\|_2$ можно обосновать **перекрёстным** расстоянием между исходным complement и возмущённым cluster. Важно исправлять конкретный шаг proof, сохраняя корректное условное ядро lemma.

Внутренняя кратность не мешает устойчивости **всего** отделённого кластера. Но lemma не доказывает, что произвольная реализация RegBlockMCA найдёт нужный верхний subspace: source прямо предполагает соответствующую сходимость. Для trace cost $f(W)=\operatorname{tr}(W^\top(A+E)W)$ максимизация выбирает верхний cluster, минимизация — нижний. Знак обновления, retraction и stopping criterion должны быть заданы отдельно. [Cost (17.37)](../source/0703-0705-ch17-unified-gate-algorithm-and-regularised-block-flow.md#equation-17-37).

## Identity shift не вращает eigenvectors

В абзаце “Why epsilon R and not epsilon I” утверждается, что $A+\varepsilon I$ вносит uniform bias во все eigenvectors. В действительности

$$
Av_j=\lambda_jv_j
\quad\Longrightarrow\quad
(A+\varepsilon I)v_j=(\lambda_j+\varepsilon)v_j.
$$

Все eigenspaces и gaps сохраняются. Поэтому identity shift действительно **не расщепляет кратность**, но и не вносит angular bias собственного базиса. Это разные свойства. [Исходный абзац](../source/0703-0705-ch17-unified-gate-algorithm-and-regularised-block-flow.md#pdf-0705).

Projected random perturbation $R=P_Q^\perp R'P_Q^\perp$ оставляет старое пространство $Q$ без действия perturbation при корректном ортогональном проекторе. Случайность может выбирать basis внутри кратного cluster, но точный recovered basis зависит от realization. Для воспроизводимости нужен зафиксированный generator/seed либо сравнение projectors.

## Нужно ли расщеплять Type A ради сходимости

В source сказано, что при $\delta_{\mathrm{in}}=0$ regularisation необходима для convergence. Это слишком широко. При цели восстановления **projector** на верхнее $m$-мерное пространство внешний gap уже выделяет уникальный target. Отсутствие единственного basis внутри него не означает отсутствия subspace convergence. Например, $A=\operatorname{diag}(2,2,0)$ имеет однозначный projector $\operatorname{diag}(1,1,0)$ при любой ортонормированной базе верхнего пространства.

Perturbation оправдана, если нужны конкретные отдельные направления или она улучшает заданный solver при данном бюджете. Она не является обязательным условием определения кластерного projector. Аналогично $\delta_{\mathrm{in}}>0$ ещё не означает, что все внутренние eigenvalues различны: $(2,2,1.9)$ имеет положительную ширину и сохраняет кратность.

Наконец, Type-B условие малости каждого соседнего relative gap не автоматически ограничивает всю ширину кластера одной $\gamma_{\mathrm{near}}\lambda_1$ при произвольном $m$. Для применения Lemma 17.10 существенно проверить именно внешний gap и норму фактического perturbation; индивидуальная идентифицируемость требует дополнительных внутренних separations.
