---
title: "278. Frequency law и allocation: недостающий functional, нормировка и сингулярность у t=0"
source_pdf_pages: [676, 677]
book_pages: [640, 641]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 278. Frequency law и allocation: недостающий functional, нормировка и сингулярность у t=0

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Следуют ли σ^(−2/β) и оптимальная head density из power-law spectrum?

## От спектра данных к κ нет автоматического перехода

[Proposition 16.84](../source/0675-0678-ch16-diffusion-family-bounds-and-stratified-growth.md#proposition-16-84) задаёт $\kappa_A(t)^2\propto\sigma(t)^{-2/\beta}$. Proof сначала использует cutoff $\omega_c\sim1/\sigma$, затем без вывода приписывает effective dimensionality степень $-2/\beta$. Эти два шага нуждаются в собственной модели.

Например, в простом signal-to-noise cutoff при $P(\omega)=C\omega^{-\beta}$, white noise power $\sigma^2$ и фиксированном $\alpha$ условие $\alpha^2P(\omega)\gtrsim\sigma^2$ даёт

$$
\omega_c\asymp\left(\frac{\alpha^2C}{\sigma^2}\right)^{1/\beta}.
$$

В пространственной размерности $m$ число Fourier modes до радиуса $\omega_c$ обычно зависит от $\omega_c^m$, то есть возникает степень $-2m/\beta$ в такой модели. Это **редакторский пример зависимости от предпосылок**, не подстановка нового универсального закона вместо источника.

Дополнительно $B(t)$ в (16.124) — operator градиентов параметров, а не covariance pixels. Его spectrum зависит от модели, Jacobian и состояния обучения. Даже если известен effective rank $r$, при convention $\delta_0=\lambda_{\min}>0$

$$
\kappa_A^2=\frac{\sum_{i=1}^r\lambda_i^2}{\lambda_{\min}^2}
$$

не равно автоматически $r$: совпадение требует плоского spectrum, а в общем отношении важны все eigenvalue ratios. Поэтому cutoff → dimensionality → $\kappa_A^2$ — три разных перехода.

## Variational argument не задан

[Theorem 16.85](../source/0675-0678-ch16-diffusion-family-bounds-and-stratified-growth.md#theorem-16-85) объявляет оптимальным $p_K^*\propto\kappa_A^2p_\mu$. Но functional (16.126) зависит от заданных $B,\theta_w,\mu$ и **вообще не содержит переменной $K(t)$**, по которой предлагается оптимизировать. Нормировка $\int p_K=1$ одна не выделяет нужную density.

Даже распределение вклада самого интегранда пропорционально $\kappa_A^2\log(\Gamma_0/\theta_w)p_\mu$, а не только $\kappa_A^2p_\mu$, если log factor меняется с $t$. Для иного resource objective получается иной закон: например, минимизация $\int c(t)/p(t)\,dt$ при $\int p=1$ и $c>0$ даёт $p\propto\sqrt c$, что видно из условия $-c/p^2+\lambda=0$. Это показывает необходимость явного objective, а не исправляет теорему без него.

## Density должна быть нормируемой

Заявленная плотность требует

$$
0<Z=\int_0^T\kappa_A(t)^2p_\mu(t)\,dt<\infty.
$$

В книге $\sigma(0)=0$. Если около нуля $\sigma(t)\asymp t^a$, $p_\mu(t)\asymp t^b$, то при заявленном frequency law нормировка конечна только при

$$
b-\frac{2a}{\beta}>-1.
$$

При uniform sampling и $\sigma(t)=t$ нужна $\beta>2$; для $\beta=1,1.5,2$ из авторского диапазона интеграл расходится. При $\sigma(t)\asymp\sqrt t$ нужна $\beta>1$, а $\beta=1$ даёт logarithmic divergence. Конечный grid, нижний cutoff или другая density могут устранить singularity, но должны быть частью постановки; численные данные на конечном диапазоне не доказывают endpoint law.

## Направление времени

По source $\sigma(0)=0$, $\sigma(T)=\sigma_{\max}$: малое $t$ — низкий noise level. Denoising движется от большого шума к малому, поэтому small $t$ соответствует поздней части такого прохода. Фраза «small t (early time step in the denoising direction)» на [PDF 677](../source/0675-0678-ch16-diffusion-family-bounds-and-stratified-growth.md#pdf-0677) меняет convention без объявления. Figure 16.14 показывает high → low noise и не разрешает смешивать forward-noising time с шагами обратного sampler.

Для реализации сначала задаются schedule, направление обхода, cutoff, sampling density, метрика compute и способ измерить operator complexity. Только после этого можно проверять proposed allocation.
