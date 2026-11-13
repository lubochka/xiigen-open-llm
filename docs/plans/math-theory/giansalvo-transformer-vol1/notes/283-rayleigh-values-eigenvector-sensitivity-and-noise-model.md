---
title: "283. Rayleigh quotient и собственный вектор: разные чувствительности и модель шума"
source_pdf_pages: [686, 688]
book_pages: [650, 652]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 283. Rayleigh quotient и собственный вектор: разные чувствительности и модель шума

## Вопрос

Что действительно доказывают Lemmas 17.1–17.2 и когда из них следует шумовой предел для обучаемого gate?

**Значение Rayleigh quotient устойчиво как скаляр, а соответствующее направление может быть чувствительно к малому eigengap.** Lemmas 17.1–17.2 дают полезное различение этих двух ошибок. Однако применение оценки возмущённого eigenvector к случайной динамике gate требует отдельной модели effective perturbation. [Lemma 17.1](../source/0685-0688-ch17-gate-rates-rayleigh-noise-and-orthogonality.md#lemma-17-1), [Lemma 17.2](../source/0685-0688-ch17-gate-rates-rayleigh-noise-and-orthogonality.md#lemma-17-2).

## Точная квадратичная ошибка Rayleigh quotient

Для симметричной $A$, ортонормированных $v_k$ и $u=v_1\cos\theta+w\sin\theta$, где $\|w\|=1$ и $w\perp v_1$,

$$
R_A(u)=\lambda_1-\sin^2\theta\,
\sum_{k=2}^d(\lambda_1-\lambda_k)|v_k^\top w|^2.
$$

Следовательно,

$$
(\lambda_1-\lambda_2)\sin^2\theta
\le \lambda_1-R_A(u)
\le(\lambda_1-\lambda_d)\sin^2\theta.
$$

Это корректное ядро (17.6): ошибка scalar estimate около простого экстремума имеет второй порядок по углу. При $\theta=0$ направление $w$ не определяется делением на $\sin\theta$, но тождество продолжается любым выбором единичного $w\perp v_1$. [Формула (17.6)](../source/0685-0688-ch17-gate-rates-rayleigh-noise-and-orthogonality.md#equation-17-6).

Для фиксированного единичного $u$ также непосредственно $|R_{A+E}(u)-R_A(u)|=|u^\top Eu|\le\|E\|_2$. Здесь нет множителя $1/\Delta$. Он возникает при восстановлении направления, а не в самой оценке скаляра.

## Что контролирует spectral perturbation bound

При симметричном $E$, простом верхнем eigenvalue и $\|E\|_2\le\Delta/2$, где $\Delta=\lambda_1-\lambda_2>0$, источник приводит

$$
\sin\angle(v_1,\widetilde v_1)
\le\frac{\|E\|_2}{\Delta-\|E\|_2}
\le\frac{2\|E\|_2}{\Delta}.
$$

Оценка (17.7) является **верхней границей**. Если $E=cI$ или $E$ диагональна в собственном базисе и не меняет порядок eigenvalues, собственные векторы вообще не поворачиваются. Поэтому даже при очень малой $\Delta$ она не устанавливает обязательную ненулевую ошибку и не даёт нижнюю границу “at least”. Этот же вопрос важен в выборе block strategy на PDF 692. [Формула (17.7)](../source/0685-0688-ch17-gate-rates-rayleigh-noise-and-orthogonality.md#equation-17-7).

## Переход к $\sigma_{\mathrm{eff}}$ остаётся условным

В (17.8) вместо $\|E\|_2$ появляется $\sigma_{\mathrm{eff}}$, а вместо $\widetilde v_1$ — предельное направление gate $u_\infty$. Для такого вывода необходимо показать, что:

- noise parameter контролирует **operator norm симметричного возмущения**, а не только variance отдельной координаты;
- $u_\infty$ действительно является нужным eigenvector одной матрицы $A+E$ либо имеет отдельно ограниченный eigensolver residual;
- для меняющихся mini-batches, target drift и дискретизации установлено нужное событие, ожидание или uniform bound.

В рассматриваемом фрагменте этот мост не построен. Если $\sigma_{\mathrm{eff}}$ — стандартное отклонение, детерминированная подстановка в (17.7) без concentration bound недостаточна. При непрекращающемся stochastic noise существование точечного предела также нельзя считать автоматическим. [Заявленный noise floor (17.8)](../source/0685-0688-ch17-gate-rates-rayleigh-noise-and-orthogonality.md#equation-17-8), [схема Figure 17.2](../source/0685-0688-ch17-gate-rates-rayleigh-noise-and-orthogonality.md#figure-17-2).

Для проектного ресурса полезны три отдельных diagnostics: scalar Rayleigh error, eigenpair residual $\|Au-R(u)u\|$, и angular/subspace error относительно отделённого спектрального блока. Первые два измеримы без знания точного eigenvector; превращение их в третий требует gap. Снижение scalar noise само по себе не доказывает ускорения обучения или точности всей архитектуры.
