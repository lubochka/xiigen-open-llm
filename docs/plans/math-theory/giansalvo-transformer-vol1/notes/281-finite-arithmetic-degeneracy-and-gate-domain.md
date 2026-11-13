---
title: "281. Finite arithmetic: eigenspace вместо уникального вектора, малый gap и условный unified gate"
source_pdf_pages: [682, 684]
book_pages: [646, 648]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 281. Finite arithmetic: eigenspace вместо уникального вектора, малый gap и условный unified gate

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Какие новые проблемы вводит глава17 и что уже можно проверить до её основных теорем?

## Новая глава уточняет численный механизм

[§17.1](../source/0682-0684-ch17-finite-arithmetic-and-degenerate-gate-overview.md#section-17-1) выделяет три режима: повторное leading eigenvalue, малый положительный gap и eigenvalues ниже numerical threshold. Это разные причины ненадёжности spectral gate; единый scalar stopping test не обязан различать их.

Здесь пока дан overview. Algorithm 7, Definition 17.6 и Theorem 17.11 упомянуты как последующее развитие. Их наличие в навигации не означает, что уже прочитаны proof и все hypotheses. PDF 684 прямо говорит о наследовании условий глав 14–16 и дополнительных basin-entry/online-classification assumptions.

## Exact degeneracy не запрещает любой векторный предел

У $A=\operatorname{diag}(\lambda,\lambda,\mu)$, $\lambda>\mu\ge0$, нет единственного канонического leading eigenvector. Но при $P_Eu_0\ne0$ степенная итерация имеет предел

$$
\frac{A^ku_0}{\|A^ku_0\|}
\longrightarrow\frac{P_Eu_0}{\|P_Eu_0\|},
$$

где $E$ — leading eigenspace. Направление определяется initialisation. Аналогично sphere gradient flow $\dot u=Au-(u^\top Au)u$ сохраняет относительные компоненты внутри degenerate eigenspace.

Поэтому фраза «single-vector limit … is not well-defined» на [PDF 683](../source/0682-0684-ch17-finite-arithmetic-and-degenerate-gate-overview.md#pdf-0683) корректна как отсутствие **единственного spectral target**, но не как универсальное отсутствие сходимости любого запуска. Block method выбирает устойчивое подпространство; его basis всё ещё неоднозначен с точностью до ортогонального вращения. Начало, ортогональное leading eigenspace, также показывает необходимость basin assumptions.

## Малый gap: rate зависит от конкретного update

Для обычной power iteration на подходящем PSD-операторе отношение подавляемых компонент равно $\lambda_2/\lambda_1$. Для нормированной итерации с $I+\eta A$ — $(1+\eta\lambda_2)/(1+\eta\lambda_1)$; для указанного flow — $e^{-(\lambda_1-\lambda_2)t}$. Общая идея ухудшения при малом gap верна, но rate нужно связывать с выбранным шагом и областью spectrum.

Чувствительность также видна без ссылки на общий theorem: у матрицы

$$
\begin{pmatrix}\lambda+\Delta&\tau\\\tau&\lambda\end{pmatrix}
$$

поворот leading eigenvector удовлетворяет $\tan(2\theta)=2\tau/\Delta$. Ошибка оператора мала относительно его нормы, но при сопоставимом gap может сильно изменить направление. Для близкого eigenvalue cluster надо оценивать subspace, отделённое **внешним** gap.

## Signed operator и numerical kernel

На [PDF 682](../source/0682-0684-ch17-finite-arithmetic-and-degenerate-gate-overview.md#display-0682-signed-task-operator) оператор вновь записан как $B=\operatorname{sym}(X^\top XM_a)$. При symmetric $S=X^\top X$ и skew $K=M_a$ это $\tfrac12(SK-KS)$: symmetric и traceless, но обычно indefinite. Например, $S=\operatorname{diag}(1,2)$ и $K=\begin{pmatrix}0&-1\\1&0\end{pmatrix}$ дают eigenvalues $\pm1/2$. Это не прежний PSD gradient second moment.

Для indefinite operator нулевой Rayleigh quotient не означает kernel: при $A=\operatorname{diag}(1,-1)$ и $u=(1,1)/\sqrt2$ имеем $u^\top Au=0$, но $\|Au\|=1$. Поэтому detector numerical kernel требует eigenpair/residual accuracy, а не только малого quotient. Нужна ясность, какой именно residual подаётся в каждый numerical routine и как обрабатываются знаки.

## Regularisation scale не сравнивается с остальными ошибками автоматически

На [PDF 684](../source/0682-0684-ch17-finite-arithmetic-and-degenerate-gate-overview.md#pdf-0684) предлагается $\varepsilon_{\rm reg}=\sqrt{\varepsilon_{\rm mach}}\|A_{\rm res}\|$ и говорится, что bias ниже прочих numerical errors. Но для малого $\varepsilon_{\rm mach}$ эта величина намного больше обычного масштаба $\varepsilon_{\rm mach}\|A\|$, а direction bias дополнительно зависит от gap. Нужен конкретный error budget, чтобы сравнение было доказанным; само значение machine epsilon его не задаёт.

Для Xiigen полезно регистрировать operator norm, gaps, eigenpair residuals, orthogonality defect и uncertainty диагностики отдельно от task threshold. Конкретные dispatcher rules и гарантии должны браться из следующих разделов с сохранением их условий. Этот overview не объявляется доказательством будущей Theorem 17.11.
