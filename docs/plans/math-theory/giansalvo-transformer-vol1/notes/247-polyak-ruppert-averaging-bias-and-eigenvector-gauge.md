---
title: "247. Polyak–Ruppert averaging: нулевое среднее, autocorrelation и знак eigenvectors"
source_pdf_pages: [632, 633]
book_pages: [596, 597]
batch: "013"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 247. Polyak–Ruppert averaging: нулевое среднее, autocorrelation и знак eigenvectors

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Где изменилось предположение

После утверждений о persistent bias §16.11.2 объявляет этот bias stationary fluctuation, а proof Theorem 16.40 **предполагает** zero asymptotic mean. Это новое свойство, не следствие upper MSE bound Theorem 16.39. [Постановка averaging](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#section-16-11-2), [Proof sketch](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#pdf-0633); различие bias и variance — [246](246-collapsed-scales-bias-bound-and-stationarity.md).

Если $z_t=z^*+b+\xi_t$, где $b\ne0$ постоянно, а независимые centered $\xi_t$ имеют covariance $\Sigma$, то для $N$ наблюдений

$$
\mathbb E\bar z_N-z^*=b,\qquad
\mathbb E\|\bar z_N-z^*\|^2=\|b\|^2+\frac{\operatorname{tr}\Sigma}{N}.
$$

Усреднение не устраняет $b$. Последовательность может удовлетворять постоянному MSE floor исходной теоремы, но не (16.63) с правой частью, стремящейся к нулю. Поэтому основной результат требует доказательства centering именно относительно нужного target.

Даже zero-mean stationarity недостаточно для rate $1/N$: процесс $\xi_t=\xi$ с одной общей случайной centered величиной стационарен, а его average не снижает variance. Для стационарного centered процесса точная формула имеет вид

$$
\mathbb E\|\bar\xi_N\|^2
=\frac1{N^2}\left[N\gamma(0)+2\sum_{h=1}^{N-1}(N-h)\gamma(h)\right],
$$

где $\gamma(h)=\mathbb E\langle\xi_0,\xi_h\rangle$. Summable autocovariance даёт верхнюю оценку порядка $1/N$, но её константа определяется именно этими зависимостями, а не автоматически напечатанным $\sigma_{\rm CLT}^2$. [Theorem 16.40 и 16.63](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#theorem-16-40).

## Eigenvectors требуют согласованного представителя

У unit eigenvector есть неоднозначность знака. При $u_t=(-1)^t u^*$ все projectors $u_tu_t^\top$ точны, но average $u_t$ стремится к нулю. При повторных eigenvalues свободен целый базис подпространства. Поэтому raw averaging пары $(u^+,u^-)$ требует sign alignment и устойчивого выбора eigenvectors; альтернативой может быть averaging projectors с последующим извлечением подпространства. Это уже иной estimator со своими ошибками и computational contract.

Аналогично average нелинейной диагностики не обязан совпасть с диагностикой среднего. В частности, усреднение $\widehat\kappa$ или inverse gap не удаляет bias, созданный нелинейным преобразованием noisy eigenvalues. Все такие quantities нельзя объявить unbiased только из centering исходного вектора.

## Burn-in и единицы скорости

Напечатанный $T_0=\Theta(\log\rho_{\rm sep}/(c_0\eta^+\Delta^+))$ даёт нулевой scale при $\rho_{\rm sep}=1$ и не учитывает initial error или требуемую точность. Для обычного contraction $\|e_t\|\lesssim q^t\|e_0\|$ время подавления transient зависит от $\log(\|e_0\|/\epsilon)/(-\log q)$. Утверждение с одним variance term без initial/transient term требует соответствующей initialisation или обоснованного burn-in.

В (16.63) squared error имеет rate $1/N$; RMS error — $1/\sqrt N$. Фраза «$1/\sqrt T$ in mean square» и следующее сравнение $\Theta(1/T)$ с $\Theta(1/\sqrt T)$ в Corollary 16.41 не указывают одну и ту же метрику. Это нельзя интерпретировать как доказанный slowdown времени роста архитектуры. [Three-level recovery](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#corollary-16-41).

Перенос на три уровня controller требует не только asymptotic estimator accuracy, но и margin для всех decision thresholds, контроля многократных проверок и сохранения условий tracking при меняющихся архитектурах. Ссылка на Adam cancellation наследует ограничения [241](241-adam-contraction-architecture-invariance-and-optimizer-scope.md). Эти условия не снимаются одновременным применением двух недоказанных extensions.
