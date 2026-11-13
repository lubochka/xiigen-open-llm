---
title: "107. Signature orbits, observable traces и slack tightening"
source_pdf_pages: [366, 367]
book_pages: [330, 331]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 107. Signature orbits, observable traces и slack tightening

[Индекс библиотеки](../README.md) · [Предыдущая карточка](106-training-claims-and-chapter-eleven-entry.md) · [Следующая карточка](108-two-sars-orbits-and-local-interventions.md)

## От конечной точки к траектории

**Definition 11.1** вводит headwise curve

$$
\mathcal O_{\ell,h}
=\left\{\bigl(\alpha_{\mathrm{SA}}^{(\ell,h)}(t),\Gamma_s^{(\ell,h)}(t),
\Gamma_a^{(\ell,h)}(t),\Sigma(A^{(\ell,h)}(t)),\rho_{\ell,h}(t)\bigr):t\ge0\right\}.
$$

В preceding displayed tuple также включены entropy и attention asymmetry. Source различает full signature orbit и **observable S–A orbit**, когда измерены лишь score/attention coordinates. В latter case $\Sigma(A(t))$ не становится измеренной величиной благодаря названию orbit. [Definition 11.1](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#definition-11-1); [продолжение PDF 367](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#pdf-0367).

Для воспроизводимого trace нужно сохранять parameterized time/checkpoint sequence: запись как множество точек сама не кодирует порядок, скорость и повторные прохождения. Параметризация $t$ названа в prose, поэтому при экспорте важно её не потерять. Layer distribution из headwise paths — распределение **траекторий**; отдельные snapshot measures по времени могут не восстановить их temporal coupling. Это редакторское уточнение того, какие данные нужны для orbit analysis. [§11.1](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#section-11-1).

## Static inequality нельзя просто продифференцировать

Автор прямо предупреждает: из $A(t)\ge B(t)$ не следует $\dot A(t)\ge\dot B(t)$. Например, на $0<t<1$ функции $A(t)=1-t$, $B(t)=0$ удовлетворяют первому неравенству, но $\dot A=-1<0=\dot B$. Это редакторский простой пример к верному предостережению источника. [§11.2](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#section-11-2).

Для гладкого observable $F$ при прямом gradient-descent step:

$$
F(\theta-\eta\nabla\mathcal L)-F(\theta)
=-\eta\langle\nabla F,\nabla\mathcal L\rangle+O(\eta^2).
$$

Знак изменения задаётся correlation двух gradients, а не одним знаком значения $F$. При twice differentiable $F$ и Hessian bound $K$ на отрезке шага абсолютный remainder не больше $\tfrac12K\eta^2\lVert\nabla\mathcal L\rVert^2$. Для другого optimizer вместо $-\eta\nabla\mathcal L$ используется его actual step. [Source Taylor formula](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#display-11-observable-descent).

## Slack tightening и forward invariance — разные условия

Пусть compatibility region задана $s(\theta)=A(\theta)-B(\theta)\ge0$. Тогда нужно проверять именно

$$
\Delta s=-\eta\langle\nabla A-\nabla B,\nabla\mathcal L\rangle+O(\eta^2).
$$

Alignment loss gradient с одним $A$ не контролирует изменение $B$. Уменьшение положительного slack означает движение **к границе** и само не гарантирует, что шаг не пересечёт её. Для continuous flow на гладкой boundary необходим inward/tangent condition $\dot s\ge0$; для discrete step нужно $s+\Delta s\ge0$ с учётом remainder. Если вместо $s$ используется violation $B-A$, смысл «tightening» меняется, поэтому sign convention следует назвать явно. Это редакторская конкретизация, отсутствующая в кратком source описании. [PDF 367, saturation hypothesis](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#section-11-2).

Таким образом, полезная мысль книги — compatibility dynamics требуют gradient-alignment hypotheses — не равна доказательству forward invariance одной лишь фразой «slack decreases». Source сам отделяет local compatibility region от формального stable manifold. Для robust numerical test нужны boundary, step bound и все изменяющиеся variables. [§11.2](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#section-11-2).

## Two-flow formulas сохраняют прежнюю область действия

В этом подразделе снова написано $\partial_tM_s=-\eta G_s+O(\eta^2)$ и аналогично для $M_a$. Это step convention из главы 10; для actual Q/K training нужно сперва вычислить induced $\Delta M$, как прямо требует Remark 10.12. Отсутствие этого шага может дать неправильное направление orbit. [Projected flows](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#display-11-projected-sa-flows); [Remark 10.12](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#remark-10-12); [100](100-parameter-balance-evolution-and-induced-flow.md).

## Начало atlas и граница этой карточки

§11.3 вводит два авторских compact SARS traces. *Directional-tail trace* использует более высокий learning rate и сохраняет несколько heads с заметным skew balance и flip effect. *Symmetric-collapse trace* имеет меньший learning rate и clipped class weights; автор описывает его как более гладкий численно, но почти полностью предсказывающий majority class. Оба названы orbit probes с низким task performance, не готовыми biological classifiers. [PDF 367, §11.3](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#section-11-3).

На PDF 367 закончены оба вводных абзаца §11.3; дальнейшие таблицы и детали находятся в [продолжении PDF 368](../source/0368-0372-ch11-empirical-orbit-atlas.md#pdf-0368) и [карточке 108](108-two-sars-orbits-and-local-interventions.md). Глава 11 здесь остаётся открытой; новые эмпирические показатели в этой карточке не предполагаются.
