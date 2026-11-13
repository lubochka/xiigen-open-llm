---
title: "101. Совместная диагностика и смысл производной signature geography"
source_pdf_pages: [353, 354]
book_pages: [317, 318]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 101. Совместная диагностика и смысл производной signature geography

[Индекс библиотеки](../README.md) · [Предыдущая карточка](100-parameter-balance-evolution-and-induced-flow.md) · [Следующая карточка](102-sars-v8-aggregate-trace-and-score-conventions.md)

## Три уровня описания одной головы

**Definition 10.14** объединяет

$$
D_\ell=\bigl((\alpha_{\ell,h})_h,(\Gamma_s^{(\ell,h)})_h,
(\Gamma_a^{(\ell,h)})_h,\mu_\ell,(\rho_{\ell,h})_h\bigr).
$$

Параметрическая геометрия, input-dependent scores, attention matrix и canonical signature — связанные, но разные объекты. Remarks 10.16–10.18 подчёркивают, что directional compatibility chain не контролирует весь symmetric channel и что score-side измерений недостаточно для полной signature geography. [Definition 10.14](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#definition-10-14); [Remarks 10.16–10.18](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#remark-10-16).

Для использования кортежа в коде нужно назвать формулу $\alpha$ явно. **Proposition 10.15** ссылается на параметрическую Corollary 10.11, тогда как сразу следующий §10.11 выбирает empirical $\alpha_{\mathrm{score}}$. Их производные нельзя подменять друг другом. [Proposition 10.15](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#proposition-10-15); [эмпирическое определение](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#display-10-empirical-score-balance).

## Какие derivative hypotheses действительно нужны

Утверждение first-order propagation — применение chain rule при дифференцируемости всех участвующих отображений. Для raw score matrix при меняющихся weights и representations:

$$
\dot L=\dot XMX^\top+X\dot MX^\top+XM\dot X^\top.
$$

Fixed-$X$ формула удерживает только средний член. В training trace следует также зафиксировать probe inputs, маску и правила усреднения: смена самого измерительного batch добавляет ещё один источник изменения, не равный движению параметров на общем входе. Это редакторская спецификация измерения, необходимая для интерпретации цепочки источника. [Score-to-signature chain](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#display-10-score-signature-chain); [карточка 98](98-first-order-softmax-adjoint-and-signature-response.md).

Source proof говорит о гладкости score components вдали от zero-energy denominator. Этого недостаточно для **норм** $\Gamma_s=\lVert L_s\rVert_F$, $\Gamma_a=\lVert L_a\rVert_F$: одна компонента может обратиться в ноль, когда полная энергия положительна. В ненулевой компоненте

$$
\dot\Gamma_a=\frac{\langle L_a,\dot L_a\rangle_F}{\Gamma_a},
$$

а при $L_a=0$ обычная линейная производная нормы не существует во всех направлениях. То же относится к symmetric channel. Для squared energy производная гладкая, но это уже другая source convention. [Proof Proposition 10.15](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#pdf-0354); [определение норм](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#display-10-empirical-score-energies).

При fixed stabiliser $\varepsilon$ корректная производная empirical balance, где обе нормы дифференцируемы:

$$
\dot\alpha_{\mathrm{score}}
=\frac{(\Gamma_s+\varepsilon)\dot\Gamma_a-\Gamma_a\dot\Gamma_s}
{(\Gamma_s+\Gamma_a+\varepsilon)^2}.
$$

Это редакторский quotient-rule вывод для §10.11, **не** формула Corollary 10.11. [Источник формулы balance](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#display-10-empirical-score-balance); [карточка 100](100-parameter-balance-evolution-and-induced-flow.md).

## Что означает \(\partial_t\mu_\ell\)

Если $z_h(t)=\Sigma_h(t)$ — гладкие сигнатуры и $\mu_\ell(t)=H^{-1}\sum_h\delta_{z_h(t)}$, то естественный смысл производной меры — **слабый**, через гладкую test function $\varphi$:

$$
\frac{d}{dt}\int\varphi(z)\,d\mu_\ell(t)
=\frac1H\sum_h\nabla\varphi(z_h(t))\cdot\dot z_h(t).
$$

Это позволяет строго читать «география движется со скоростями сигнатур». Источник не задаёт topology для $\partial_t\mu$; нельзя автоматически считать её обычным coordinate vector или derivative в total variation norm: даже движущаяся delta measure ведёт себя там иначе. Wasserstein description также требует своей формулировки скорости. [Proposition 10.15](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#display-10-diagnostic-velocity); [транспортная геометрия](69-hellinger-wasserstein-layer-geometry.md).

Гладкость выбранной матрицы проекции в hull не гарантирует гладкости barycentric selection и residual norm при нуле. Поэтому Proposition 10.15 следует применять на страте гладкости **всего** $\Sigma$, с фиксированным dictionary и правилом выбора coefficients. [Карточки 91](91-signature-differential-chain-and-sa-motors.md) и [98](98-first-order-softmax-adjoint-and-signature-response.md).

## Что измерено в v8

Remark 10.18 прямо ограничивает доступную часть $D_\ell$ score/attention traces и local interventions. Наличие строки $\partial_t\mu$ в теоретическом кортеже не означает, что $\mu(t)$ и $\rho(t)$ вычислены в данном experiment. Это различие повторяется в [§10.12](../source/0355-0360-ch10-sars-trace-and-sa-interventions.md#section-10-12) и [начале главы 11](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#pdf-0367).
