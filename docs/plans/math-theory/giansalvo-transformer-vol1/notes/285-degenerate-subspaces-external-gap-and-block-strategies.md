---
title: "285. Вырожденные eigenspaces: внешний gap, устойчивый проектор и выбор block strategy"
source_pdf_pages: [690, 692]
book_pages: [654, 656]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 285. Вырожденные eigenspaces: внешний gap, устойчивый проектор и выбор block strategy

## Вопрос

Когда кратный или тесный spectral cluster следует захватывать как подпространство и какие гарантии у стратегий A/B действительно даны?

**Устойчивым объектом может быть проектор на отделённый кластер, даже когда отдельные eigenvectors внутри него не определены устойчиво.** Для этого необходим gap между кластером и остальным спектром. Внутренняя кратность сама по себе не делает задачу хорошо обусловленной. [Definitions 17.3–17.4](../source/0689-0692-ch17-dwell-time-and-spectral-degeneracy.md#definition-17-3), [стратегии B1–B3](../source/0689-0692-ch17-dwell-time-and-spectral-degeneracy.md#pdf-0692).

## Type A: полезна инвариантность к выбору базиса

Если $\lambda_1=\cdots=\lambda_m>\lambda_{m+1}$, то eigenbasis внутри $V=\operatorname{span}(v_1,\ldots,v_m)$ не уникален. При $U^\top U=I_m$ проектор $UU^\top$ не меняется от замены $U\mapsto UR$, $R\in O(m)$. Блочная дефляция поэтому может быть определена без выбора канонических отдельных направлений. Это верное геометрическое содержание (17.12) и Figure 17.4. [Определение Type A](../source/0689-0692-ch17-dwell-time-and-spectral-degeneracy.md#equation-17-12), [Figure 17.4](../source/0689-0692-ch17-dwell-time-and-spectral-degeneracy.md#figure-17-4).

Но внешний gap $\delta_{\mathrm{out}}=\lambda_m-\lambda_{m+1}$ может быть сколь угодно мал. Например, $\operatorname{diag}(1,1,1-\delta)$ имеет точно кратный верхний кластер при любом $\delta>0$, а чувствительность его projective separation ухудшается при $\delta\downarrow0$. Поэтому авторское “most well-conditioned” не является следствием одного определения Type A. Скорость block solver и projector perturbation bounds зависят от $\delta_{\mathrm{out}}$, масштаба $A$ и начального block overlap.

## Type B: пороги и различение стратегий

Формула (17.13) использует relative gaps $(\lambda_k-\lambda_{k+1})/\lambda_k<\gamma_{\mathrm{near}}$. При положительных знаменателях это разумный масштабно-инвариантный diagnostic. Но нестрогое спектральное упорядочение вместе с таким условием допускает нулевые gaps: текстовая оговорка «strictly positive» должна быть отдельной предпосылкой, иначе Type A попадает и в Type B. Signed или нулевые denominators требуют другого правила. [Definition 17.4](../source/0689-0692-ch17-dwell-time-and-spectral-degeneracy.md#definition-17-4).

В стратегии B1 inverse iteration со shift $\mu$ выбирает eigenvalue, **ближайший к $\mu$**, если он единственный и начальный overlap ненулевой. Улучшение зависит от расстояния до всех конкурентов, а не от одной записи $\lambda_2$. Для более позднего shifted power порядок выбора противоположный; это разобрано в [карточке 288](288-shifted-power-versus-inverse-iteration.md).

В B2 порог $\Delta<\sqrt{\varepsilon_{\mathrm{mach}}}\|A\|$ подаётся как основание считать индивидуальные векторы неразличимыми. Это полезная эвристика, но её надо связать с допустимой ошибкой и реальным backward error. Верхняя perturbation bound не доказывает, что ошибка составляет **не менее** данного масштаба. Контрпример к такой универсальной нижней границе прост: диагональная матрица с очень близкими, но точно представленными diagonal entries имеет точно известные координатные eigenvectors. Никакого обязательного их поворота нет.

В B3 случайное симметричное возмущение может выбрать базис и расщепить кратность. Однако gap порядка $\Theta(\epsilon)$ не следует без распределения, масштаба и вероятностной оговорки; возмущение $\epsilon I$ вообще не расщепляет cluster. Ошибка внешнего projector имеет масштаб вида $\epsilon\|R\|/\delta_{\mathrm{out}}$, а не универсальный $O(\epsilon)$ с независимой от gap константой. Такая regularisation меняет задачу и не гарантирует сохранения выбранных индивидуальных направлений.

## Контракт для проекта

Разумный dispatch должен хранить отдельно multiplicity/cluster size, internal gaps, external gap, estimated residuals и numerical rank. Весь спектр не исчерпывается названиями A/B/C: простая хорошо отделённая full-rank матрица принадлежит обычной ветви, которую сам источник далее явно использует. Block size нельзя вывести только из первого малого gap, а точный projector на кратное пространство нельзя получить единственным scalar-Krylov seed без дополнительных направлений. [Авторский dispatch](../source/0689-0692-ch17-dwell-time-and-spectral-degeneracy.md#prescription-0692-degeneracy-strategy-selection), [ограничение scalar Lanczos](292-lanczos-degeneracy-qr-contract-and-batch-boundary.md).
