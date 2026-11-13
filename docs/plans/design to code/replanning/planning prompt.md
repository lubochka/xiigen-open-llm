Меня очень смущает нынешнее планирование задания. Каждый этап занимает ну очень много времени, даёт очень не большой выхлоп и мызависаем в проверках  windows run on github. Я почти  уверена что мы пытаемся покрыть тестами обучающуюся систему а потом подганяем тесты под нее. Кроме того, эти раны совершенно не нужны, нам нужно как можно быстрее выстроить алгоритмы обучения и перейти к обучению и проверять не тестами а по другому большой охват обучения по разным домейнам. мне необходимо пересмотреть весь план работ - пожалуйста напиши поэтапный  план пересмотра плана разработки. https://github.com/lubochka/xiigen-planning/tree/llm_plan3/llm%20mvp/design%20to%20code%20session/ - разчитывай на 10 запусков сбора информации и гипотез и еще 5 запусков анализа этой информации и еще 5 запускам изменения плана и требований. План надо дополнить в 5 запусков

Используй Скилы архитектора, планирования, проверки планирования и кода, промпт энжиниринга и человеческой комуникации https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills

you need to add it to github https://github.com/lubochka/xiigen-planning/tree/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning

обрати внимание что первый цикл увидел только слайс 4 и не расматривал никакие другие слайсы- он не прошел ни по одному рагу в репозитории что быпонимать что где находится. Так же он не смотрел общие задачи описаные в промптах  в директории, не расматривал workLeft* files. Его проход попытался сузить задачу и решить всё одним проходом, а не систематическую работу над проектом, поиск оптимизаций, и правильное распределение нагрузок на проходы. 

"The plan now has 25 distinct tasks with explicit inputs and outputs, covering duplication, actual algorithms, domain breadth, evaluation, and process cost. WorkLeft’s existing training/DPO research becomes baseline."
Each task has to contain a very detailed prompt, the way to test it's results that the new tasks will cover the functional expectations - no more tests, since we are developing a learning system - which is not deterministic and can't be cover by tests! but we need the way to examin each domain, the ability to map a proper domain and test the improvement of the ability of the system to map the best path to the solution and the best and compect usage of the algorithm wich comain deterministic and non deterministic approach. We need to examin the ability to make proper dpo - positive and negative, and define the best knn dynamic function proper to each domain, and make a proper verctor tokenization depending on the domain. We need to examine that the combination of deterministic and non deterministic algorithmsas described creates a more compact and efficient way of learning that can be run on pc, and continue learning based on user input. So our interest to reach fast to the implemented dynamic algorithms, and to start teaching it as soon as possible. Testing need to be defined like we do in learning algorithms, by defining dpo pairs, and measure the results by time and accuracy from bulk to bulk. We will need to examin the domains real world laws, and define proper tokens and dynamic knn functions, which supposes to improve the accuracy. We will need to measure the ability of the system to map domains properly. We will need to measure the Lora functions to be able to transpose the result to proper vectorized token. Neither of it can be tested by simple test coverage, it will need astra ultra examination, as to decide if we are going the wrong or  the correct way. 

"GitHub is read-only here, so publication needs an external apply step."
github is approved on all!!!!!!!!!!!!!!!!

Please examin previous plan runs results https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/previous%20rounds%20thinking%20results.md

The tasks you are preparing need to be properly designed for a model: prompt, context, skills, rag, references,  good examples, bas examples, edge cases, if the task is too big, proper division for sub tasks where each task has to be properly designed as well.

I still don't see in the plan any reference to the algorithms that have a combined approach - deterministic and learning , how to build dpo properly, how to examin each algorithm efficiency, how to manipulate their connection:
Роли каждого блока:

ILearningUnit - упрощенная матрица принимает домейн, строение векторов, строение выхода, и по входам выпускает Iquearable of results of requested input type many->many
IlearnyngFlow - это взаимоотношение между юнитами , для коррекции входа в выход
IlearningOrchestrator - это уже более глубокая модель, позволяющая асинхронно обрабатывать инпут и превращать его в оутпут
ILearningDynamicGraph - более комплексная модель соотношений различных паттернов разного порядка


Seems that some of the important valuable design decisions are lost - I will be running R1-R20 simultaneously (https://github.com/lubochka/xiigen-planning/tree/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/results), you will need to improve the plan accordingly - if needed add more steps, checks, rounds, references + https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/PARALLEL-R01-R03-JOIN.md

