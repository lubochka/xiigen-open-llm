---
title: "276. Diffusion task operator: expected loss, gradient second moment и пространство параметров"
source_pdf_pages: [674, 675]
book_pages: [638, 639]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 276. Diffusion task operator: expected loss, gradient second moment и пространство параметров

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Какой оператор задаёт (16.124) и достаточно ли PSD для применения прежних bounds?

## Полезная постановка семейства

Раздел предлагает отдельную диагностику для каждого noise level $t$: noised input $x_t=\alpha(t)x_0+\sigma(t)\epsilon$, свой objective и residual. Такая организация позволяет различать среднюю ошибку по training schedule и плохо обслуживаемые уровни шума. [Начало семейства](../source/0671-0674-ch16-in-context-sample-complexity-and-test-time-adaptation.md#section-16-18), [Definition 16.82](../source/0675-0678-ch16-diffusion-family-bounds-and-stratified-growth.md#definition-16-82).

Но оператор нужно определить однозначно. На PDF 675 сначала записано

$$
\ell_t(\phi)=\mathbb E_{x_0,\epsilon}
\|\epsilon_\phi(x_t,t)-\epsilon\|^2,
$$

а затем $\mathcal B(t)=\mathbb E_{x_0,\epsilon}[\nabla_\phi\ell_t\,\nabla_\phi\ell_t^\top]$. Если $\ell_t$ уже **полный expected loss**, его gradient при фиксированных $t,\phi$ не зависит от конкретного $(x_0,\epsilon)$: внешнее ожидание избыточно, и $B(t)=g_tg_t^\top$ имеет rank не более одного. [Точная формула 16.124](../source/0675-0678-ch16-diffusion-family-bounds-and-stratified-growth.md#equation-16-124).

Если подразумевается sample loss $l_t(\phi;x_0,\epsilon)$, возникает иной объект:

$$
\mathbb E[gg^\top]
=\operatorname{Cov}(g)+\mathbb E[g]\mathbb E[g]^\top,
\qquad g=\nabla_\phi l_t.
$$

Он может иметь большой rank даже при нулевом среднем gradient. Это редакторское различение, а не молчаливая замена исходной формулы.

## Точный пример расхождения

Пусть $z=\pm1$ равновероятно и $l(\phi;z)=\tfrac12(\phi-z)^2$. При $\phi=0$ ожидаемый gradient равен нулю, поэтому literal expected-loss operator $B=0$. Но per-example gradients равны $\mp1$, и $\mathbb E[g^2]=1$. Тот же loss даёт разные spectral quantities в зависимости от порядка ожидания и outer product.

Отсюда меняются $\Gamma_0$, rank и существование положительного $\delta_0$. Нельзя обсуждать многомерную complexity family, не разрешив эту неоднозначность.

## PSD, размерность и нормировка

Любой operator $\mathbb E[gg^\top]$ положительно полуопределён, поскольку $v^\top Bv=\mathbb E[(v^\top g)^2]\ge0$. Это верно независимо от того, является ли objective squared loss, adversarial loss или другой дифференцируемой функцией. Поэтому дальнейшая фраза о возможном indefinite per-bin operator при adversarial training требует **другого определения**, а не следует из (16.124). [Утверждение о совместимости extensions](../source/0675-0678-ch16-diffusion-family-bounds-and-stratified-growth.md#pdf-0678).

Если $\phi$ включает $p$ параметров модели, $B(t)$ имеет размер $p\times p$. Предшествующие операторы внимания действовали в пространстве признаков или головы. Для переноса spectral count нужно задать карту между этими пространствами; PSD само по себе не обеспечивает ни finite rank нужного типа, ни coupling operator error к task loss.

В [16.125](../source/0675-0678-ch16-diffusion-family-bounds-and-stratified-growth.md#equation-16-125) $\delta_0(t)$ — минимальный eigenvalue **выше negligible floor**, в отличие от leading-eigenvalue convention ICL. При отсутствии такого eigenvalue индекс не определён; floor и его единицы должны быть явно заданы. При $\Gamma_0(t)\le\theta_w(t)$ logarithm неположителен, поэтому head-count interpretation требует stopping convention, например нулевого дополнительного бюджета, и целочисленного округления. При $\delta_0\downarrow0$ отдельные estimates могут расходиться.

Для Xiigen эта глава даёт идею conditional diagnostics, но перед использованием нужно выбрать sample или population gradient, фиксировать parameter state, пространство оператора, порог и statistical uncertainty каждого bin. Из PSD не следуют автоматически все условия старого controller.
