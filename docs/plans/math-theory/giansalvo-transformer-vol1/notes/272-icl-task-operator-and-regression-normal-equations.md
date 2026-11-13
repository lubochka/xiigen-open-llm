---
title: "272. ICL task operator: signed labels, нулевой третий момент и normal equations"
source_pdf_pages: [669, 670]
book_pages: [633, 634]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 272. ICL task operator: signed labels, нулевой третий момент и normal equations

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Представляет ли mean(y x xᵀ) задачу обычной линейной регрессии и является ли этот оператор PSD?

## Определение сохраняется буквально

В [Definition 16.77](../source/0668-0670-ch16-deployment-frontier-and-implicit-task-operator.md#definition-16-77) записано

$$
\widehat B=\frac1n\sum_i y_i x_ix_i^\top,\qquad
B=\mathbb E[y(x)xx^\top].
$$

Для скалярного $y$ матрица симметрична. Положительная полуопределённость следует, например, из $y\ge0$, но не из самого определения: $v^\top Bv=\mathbb E[y(v^\top x)^2]$ может быть отрицательным. При $x=1,y=-1$ получается $B=-1$. Даже если population operator PSD, его конечная выборочная оценка при signed labels не обязана быть PSD. Следующая Theorem 16.78 поэтому требует самостоятельного ограничения класса задач.

## Сопоставление с least squares

Для указанного в книге objective

$$
L(w)=\frac1n\sum_i(y_i-w^\top x_i)^2
$$

точные производные равны

$$
\nabla L(w)=\frac2n\sum_i x_i(x_i^\top w-y_i),\qquad
\nabla^2L(w)=\frac2n\sum_i x_ix_i^\top.
$$

При $w_0=0$ один шаг даёт $w_1=(2\eta/n)\sum_i y_ix_i$, а предсказание в запросе — $(2\eta/n)\sum_i y_i x_i^\top x_q$. Таким образом, sufficient statistics для normal equations — Gram matrix $\sum xx^\top$ и cross moment $\sum xy$. Они не совпадают с label-weighted matrix $\sum yxx^\top$. [Исходная связь attention с gradient step](../source/0668-0670-ch16-deployment-frontier-and-implicit-task-operator.md#equation-16-117) требует заданных размерностей augmented tokens, проекций и чтения scalar output; сама следующая формула не выводит такую эквивалентность.

Это различие существенно даже без шума. Пусть распределение $x$ центрально симметрично, $y=w_*^\top x$ и существует третий момент. Замена $x\mapsto-x$ меняет знак $yxx^\top$, поэтому

$$
\mathbb E[(w_*^\top x)xx^\top]=0
$$

для любого $w_*$. При этом $\mathbb E[xy]=\mathbb E[xx^\top]w_*$ обычно ненулевой. Оператор книги теряет информацию о целевом векторе именно в типичном центрированном линейном примере. Это не опровержение ICL как gradient descent; это ограничение предложенного здесь идентификатора задачи.

## Концентрация и индекс сложности

При i.i.d. $Z_i=y_ix_ix_i^\top$ и конечном втором моменте

$$
\mathbb E\|\widehat B-B\|_F^2
=\frac1n\mathbb E\|Z-B\|_F^2.
$$

Константа зависит от моментов $y^2\|x\|^4$, а не только от спектра среднего $B$. Условий независимости и хвостов нельзя заменить словами «by construction». Для зависимого prompt нужен отдельный эффективный sample size. [Population definition и утверждение о концентрации](../source/0668-0670-ch16-deployment-frontier-and-implicit-task-operator.md#equation-16-119).

В [16.120](../source/0668-0670-ch16-deployment-frontier-and-implicit-task-operator.md#equation-16-120) напечатано $\kappa_{A,\rm ICL}^2=(\Gamma_0/\delta_0)(\sigma_{\rm noise}^2/\sigma_y^2)$ с $\delta_0=\lambda_1(B)$. Если прежний $\kappa_A=\Gamma_0/\delta_0$, правая часть содержит **$\kappa_A$, а не $\kappa_A^2$**. В noiseless случае она равна нулю, а не сохраняет спектральную сложность. Для ненулевого PSD $B$ отношение $\|B\|_F/\lambda_{\max}(B)$ лежит между $1$ и $\sqrt{\operatorname{rank}B}$; его нельзя без объявления заменить отношением к минимальному ненулевому eigenvalue.

Для Xiigen сначала следует выбрать оператор, соответствующий loss и доступной статистике prompt, зафиксировать пространство и знак, затем определять residual и его uncertainty. Формулы этого раздела пока не дают готовый универсальный estimator для контекстного выбора голов.
