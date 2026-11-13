---
title: "306. Numerical floor: верхняя оценка, неизбежная ошибка и единицы adaptive threshold"
source_pdf_pages: [720, 724]
book_pages: [684, 688]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 306. Numerical floor: верхняя оценка, неизбежная ошибка и единицы adaptive threshold

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Что даёт Proposition 17.16 и можно ли подставлять ошибку подпространства прямо в relative-gap threshold?

## Три вклада полезны как схема измерения

Proposition 17.16 предлагает разделить attainable error на finite arithmetic, конечное число шагов и bias регуляризации:

$$
\operatorname{dist}(\widehat U_T,U_\star)
\lesssim C_1\kappa(U_\star)\varepsilon_{\rm mach}
+C_2e^{-c\eta\delta_{\rm out}T}+C_3\rho.
$$

Сам автор называет результат *operational estimate* и признаёт отсутствие sharp discretisation analysis. Для использования как численной гарантии нужны определение distance, условия на шаг, выбранный кластер, начальное приближение, stochastic noise и смысл вероятности/ожидания. Если $U_\star$ — ортонормированный базис, его обычное матричное condition number равно 1; чувствительность подпространства к оператору зависит также от внешнего gap. Поэтому символ $\kappa(U_\star)$ нельзя использовать как готовый измеряемый показатель без определения. [Постановка и Proposition 17.16](../source/0718-0721-ch17-signed-score-conditioning-and-numerical-floor.md#proposition-17-16), [proof sketch](../source/0718-0721-ch17-signed-score-conditioning-and-numerical-floor.md#pdf-0721).

## Почему из этого не следует ненулевой нижний предел

После исчезновения экспоненциального слагаемого верхняя оценка допускает

$$
\limsup_T\operatorname{dist}(\widehat U_T,U_\star)
\le C\bigl(C_1\kappa\varepsilon_{\rm mach}+C_3\rho\bigr),
$$

если исходная оценка равномерна по $T$. Она совместима и с нулевой фактической ошибкой. Из неё не выводится фраза «increasing T cannot reduce the error below». Для неизбежного floor нужна нижняя оценка, конкретная модель persistent noise либо эмпирический plateau с указанным протоколом. Даже ненулевая perturbation может коммутировать с оператором и не поворачивать eigenspace; не каждый bias обязан насыщать верхнюю оценку. [Напечатанный floor](../source/0718-0721-ch17-signed-score-conditioning-and-numerical-floor.md#display-0721-claimed-iteration-floor).

Число $8\cdot10^{-7}$ в заключении главы — наблюдение конкретной синтетической проверки, зависящее от $\eta,T,d,m$. Оно не объявляется общим пределом floating-point arithmetic и не измерялось нами заново. [Limitation 3](../source/0722-0725-ch17-adaptive-dispatch-and-optimiser-aware-gates.md#pdf-0724).

## Проверка adaptive threshold

Definition 17.17 использует

$$
\gamma_{\rm near}(t)=\max\left\{\gamma_{\min},
\tau_\gamma\frac{\widehat\varepsilon_{\rm floor}(t)}{\widehat\lambda_1(t)+\epsilon}\right\}.
$$

Если numerator обозначает preceding subspace distance, он безразмерен, а denominator имеет единицы eigenvalue. Это не масштабно-инвариантное сравнение относительных spectral gaps. При одинаковом eigenspace и одинаковой ошибке $10^{-3}$, $\tau_\gamma=2$, $\gamma_{\min}=.05$ получаем около .2 при $\widehat\lambda_1=.01$ и .05 при $\widehat\lambda_1=1$ (пренебрегая малым $\epsilon$). Простое масштабирование оператора меняет dispatch, хотя относительные gaps сохранены. [Definition 17.17](../source/0718-0721-ch17-signed-score-conditioning-and-numerical-floor.md#definition-17-17).

Формула получает иной смысл, если numerator — **ошибка eigenvalue/operator в тех же единицах**, либо если весь оператор заранее нормирован. Это необходимо записать явно. Для signed spectrum дополнительно требуется положительный масштаб; добавление $\epsilon>0$ само по себе не исправляет отрицательное $\widehat\lambda_1+\epsilon$. Если adaptive threshold превышает допустимый диапазон relative gaps, нужен определённый fallback, а не незаметное изменение meaning Type-B. Напечатанное `$\widehat\varepsilon_{\rm floor}(t)o0$` сохранено буквально как дефект исходного текста.

## Онлайн оценка и контракт проекта

Предложенная warmup-проверка требует истинного subspace, а сравнение двух запусков оценивает согласие, не точность: два запуска могут совпасть в неверном eigenspace. Исходник сам оставляет этот вопрос открытым. [Open question 1](../source/0722-0725-ch17-adaptive-dispatch-and-optimiser-aware-gates.md#section-17-8-2).

Для проекта полезно отдельно сохранять residual выбранного Ritz pair, gap, solver settings, разброс повторов и bias относительно независимого reference там, где он доступен. Нормировки и единицы должны быть частью записи результата. Предлагаемая operational decomposition — план калибровки, а не уже выполненная сертификация solver.
