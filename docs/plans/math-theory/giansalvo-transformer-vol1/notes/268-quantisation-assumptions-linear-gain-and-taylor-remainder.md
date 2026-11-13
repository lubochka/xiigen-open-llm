---
title: "268. Квантование: white-noise модель, chain-rule gain и остаток Taylor"
source_pdf_pages: [664, 665]
book_pages: [628, 629]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 268. Квантование: white-noise модель, chain-rule gain и остаток Taylor

## Какие предпосылки внесены явно

В §16.16.2 шаг сетки задан как $\Delta_\ell=2R_\ell/2^b$. Q1 связывает $R_\ell$ с максимальным абсолютным весом tensor, Q2 моделирует ошибки как независимые равномерные величины с нулевым средним, а Q3 **предполагает** ненулевой перенос ошибки в task operator:

$$
\mathbb E\|\Delta\mathcal B\|_F^2
\ge\frac{(\beta^*)^2\Gamma_0^2}{3\,4^b}.
$$

Это три разных условия. Ограничение одного rounding error половиной шага требует отсутствия clipping и согласованной сетки. Оно не делает ошибки независимыми или случайными при фиксированных весах. Q2 — отдельная стохастическая модель; Q3 — отдельная нижняя граница чувствительности. Ни одна из них не следует только из числа битов. [Q1–Q3](../source/0662-0664-ch16-minimum-budget-dual-and-quantisation-model.md#assumptions-16-16-quantisation), [(16.106)](../source/0662-0664-ch16-minimum-budget-dual-and-quantisation-model.md#equation-16-106).

## Корректное линейное вычисление

Пусть $\delta\phi$ имеет diagonal covariance с variance $\Delta_\ell^2/12$ для координат tensor $\ell$, а $J_\ell$ — производная $\mathcal B$ по этому tensor. Тогда для **линеаризованного** возмущения точно:

$$
\mathbb E\left\|\sum_\ell J_\ell\delta W_\ell\right\|_F^2
=\sum_\ell\frac{\Delta_\ell^2}{12}\|J_\ell\|_{\mathrm{HS}}^2
=\frac1{3\,4^b}\sum_\ell R_\ell^2\|J_\ell\|_{\mathrm{HS}}^2.
$$

Независимость и нулевые средние убирают перекрёстные члены. Это полезное вычисление gain; оно не даёт универсального положительного минимума для нормы Jacobian. [Lemma 16.71](../source/0662-0664-ch16-minimum-budget-dual-and-quantisation-model.md#lemma-16-71), [расчёт (16.108)–(16.109)](../source/0665-0667-ch16-noise-floor-and-joint-deployment-bounds.md#equation-16-108).

## Две оговорки к множителю $1+O(2^{-b})$

Источник предполагает дважды непрерывно дифференцируемый loss с bounded Hessian, затем использует квадратичный Taylor remainder для $\mathcal B(\phi)$. Если $\mathcal B$ построен из градиентов loss, такой remainder требует регулярности именно производной $J_{\mathcal B}$ и условий для перестановки производной с математическим ожиданием. Одной непрерывности bounded Hessian loss обычно достаточно для дифференцируемости соответствующих выражений при дополнительных moment conditions, но не автоматически для Lipschitz $J_{\mathcal B}$.

Даже когда квадратичный remainder есть, переход от аддитивной ошибки к относительному множителю требует **ненулевого ведущего коэффициента**. Нулевой Jacobian не исключает нелинейного quantisation error. Полностью гладкий пример: tensor $(w,z)=(0,1)$, $R=1$, loss

$$
\ell(w,z)=\tfrac32w-\tfrac14\sin(2w),\qquad
\nabla\ell=(1+\sin^2w,0).
$$

Hessian ограничен, а $\mathcal B=\nabla\ell\,\nabla\ell^\top=\operatorname{diag}((1+\sin^2w)^2,0)$. В исходной точке $J_{\mathcal B}=0$. При ненулевом равномерном шуме $\delta w$ из Q2 имеем $\|\Delta\mathcal B\|_F^2=((1+\sin^2\delta w)^2-1)^2>0$ почти наверное. Линейная правая часть (16.107) нулевая, но фактическая ошибка положительна и имеет порядок $\Delta^4$.

Этот пример относится к Lemma 16.71, заявленной под Q1–Q2; он не претендует удовлетворять дополнительной ненулевой first-order gain гипотезе Q3. Корректная общая запись должна сохранять аддитивный remainder, а относительную форму использовать при положительном линейном коэффициенте.

Для проекта нужно оценивать gain на выбранной архитектуре, фиксировать noise model и regularity. Утверждения источника о конкретных quantisation methods и «типичном» $\beta^*$ не проверены здесь по внешним реализациям.
