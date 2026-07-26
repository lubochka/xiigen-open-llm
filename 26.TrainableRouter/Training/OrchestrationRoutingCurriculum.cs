// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

public static class OrchestrationRoutingCurriculum
{
    public const string OrchestrationV309UnitId = "unit-orchestration-plan-v309";
    public const string TranslationRuEnUnitId = "unit-ru-en-general";
    public const string DocumentationDotNetUnitId = "unit-dotnet-legacy-doc-l1";
    public const string V309TrainingRunId = "train-ready-v309-v307-plus-v308-plan-head16-full-20260512-004000";
    public const string V309DatasetSha256 = "18A52F31385638748D38D559A13EF00A9F89A120F27F7EF4B340BD237E6D2938";

    public static RoutingUnitMetadataRegistry CreateDefaultUnitMetadataRegistry()
        => new(
        [
            new UnitRoutingMetadata(
                OrchestrationV309UnitId,
                "orchestration-planning-unit",
                "orchestration",
                ["prompt", "task_slice", "required_direction", "domain_hint"],
                0.20,
                "manifest-orchestration-v309",
                0.05),
            new UnitRoutingMetadata(
                TranslationRuEnUnitId,
                "translation-unit",
                "translation",
                ["prompt", "task_slice", "required_direction", "domain_hint"],
                0.20,
                "manifest-translation-ru-en-v1",
                0.05),
            new UnitRoutingMetadata(
                DocumentationDotNetUnitId,
                "documentation-unit",
                "documentation",
                ["prompt", "task_slice", "required_direction", "domain_hint"],
                0.20,
                "manifest-dotnet-doc-v1",
                0.05)
        ]);

    public static RoutingTrainingExample[] CreateV309RouterBatch50()
    {
        var candidates = new[] { OrchestrationV309UnitId, TranslationRuEnUnitId, DocumentationDotNetUnitId };
        var examples = new List<RoutingTrainingExample>();

        IEnumerable<(string Prompt, string Slice)> orchestrationInputs = OrchestrationPrompts()
            .Take(12)
            .Concat(PromptFileAcceptanceCases().Select(scenario => (Prompt: scenario.Prompt, Slice: string.Join(" ", scenario.ExpectedSteps))));

        foreach (var (prompt, slice) in orchestrationInputs.Select((value, index) => (value.Prompt, value.Slice)))
            examples.Add(Build(
                $"router-orchestration-v309-{examples.Count + 1:000}",
                prompt,
                slice,
                "orchestration",
                candidates,
                OrchestrationV309UnitId,
                DocumentationDotNetUnitId,
                "The request asks for orchestration, planning order, routing, or sub-phase control.",
                "documentation or translation units cannot choose orchestration phases, dependency order, or Gate B routing."));

        foreach (var (prompt, slice) in TranslationPrompts())
            examples.Add(Build(
                $"router-translation-ru-en-{examples.Count + 1:000}",
                prompt,
                slice,
                "translation",
                candidates,
                TranslationRuEnUnitId,
                OrchestrationV309UnitId,
                "The request asks for Russian to English translation.",
                "the orchestration model would plan work instead of producing direction-preserving translation."));

        foreach (var (prompt, slice) in DocumentationPrompts())
            examples.Add(Build(
                $"router-documentation-dotnet-{examples.Count + 1:000}",
                prompt,
                slice,
                "documentation",
                candidates,
                DocumentationDotNetUnitId,
                OrchestrationV309UnitId,
                "The request asks for bottom-up .NET legacy code documentation.",
                "the orchestration model would route or plan instead of summarizing caller/callee code behavior."));

        return examples.ToArray();
    }

    public static RoutingTrainingExample[] CreateHeldOutEvaluationSet()
    {
        var candidates = new[] { OrchestrationV309UnitId, TranslationRuEnUnitId, DocumentationDotNetUnitId };
        return
        [
            Build("heldout-orch-001", "Choose the right model for a multi-step architecture plan with dependency order and Gate B before implementation.", "architecture planning routing", "orchestration", candidates, OrchestrationV309UnitId, DocumentationDotNetUnitId, "This is orchestration planning.", "documentation loses phase and dependency routing."),
            Build("heldout-orch-002", "The prompt must be split into logical flow stages, scored, and sent to the correct specialist unit.", "logical flow model routing", "orchestration", candidates, OrchestrationV309UnitId, TranslationRuEnUnitId, "This is model orchestration.", "translation cannot select specialist model units."),
            Build("heldout-orch-003", "We need DPO pairs for plan-vs-implementation mistakes and sub-phase routing around 3.1 and 3.4.", "DPO planning sub-phase routing", "orchestration", candidates, OrchestrationV309UnitId, DocumentationDotNetUnitId, "This is orchestration DPO planning.", "documentation cannot train routing boundaries."),
            Build("heldout-orch-004", "User enters Figma, selects client and server technologies plus database, and expects the system to analyze screens, generate flows, create tech design, collect feedback, debug, deploy, and produce code.", "Figma full-system orchestration screens flows tech design feedback deploy code", "orchestration", candidates, OrchestrationV309UnitId, DocumentationDotNetUnitId, "This is end-to-end system orchestration.", "documentation can describe artifacts but cannot coordinate screens, flows, feedback, deployment, and generated code."),
            Build("heldout-trans-001", "Translate this Russian support message to English and keep the original intent.", "RU_EN translation", "translation", candidates, TranslationRuEnUnitId, OrchestrationV309UnitId, "This is translation.", "orchestration would plan instead of translate."),
            Build("heldout-doc-001", "Document this ASP.NET WebForms Page_Load method from leaf function behavior up to caller behavior.", "dotnet legacy code documentation", "documentation", candidates, DocumentationDotNetUnitId, OrchestrationV309UnitId, "This is code documentation.", "orchestration would select phases instead of documenting code.")
        ];
    }

    public static OrchestrationScenarioExpectation[] CreateUserPromptScenarioExpectations()
        =>
        [
            new(
                "figma-whole-system",
                "User enters Figma, selects client-side and server-side technologies plus database, and the system orchestrates the whole system.",
                [
                    "go over screens",
                    "analyze each screen component with AI models",
                    "analyze screens to understand the system",
                    "generate architecture as flows",
                    "create per-screen technical design",
                    "gather feedback and process user input",
                    "debug, deploy, and expose generated code"
                ]),
            new(
                "figma-existing-system-integration",
                "User wants to integrate a Figma design into an existing system.",
                [
                    "orchestrate documentation creation for existing parts",
                    "create skillsets from the documented system",
                    "derive design system theme",
                    "apply style to existing modules when not a new module",
                    "validate integration boundaries"
                ]),
            new(
                "system-from-explanation",
                "User wants to implement a whole system from scratch when design is incomplete.",
                [
                    "generate requirements from user explanation",
                    "derive design theme",
                    "create architecture",
                    "create skills",
                    "create implementation plan",
                    "implement, test, deploy, visualize, and debug"
                ]),
            new(
                "diet-plan-client-app",
                "User wants an app where clients register, submit form data, receive AI-generated diet plans, and send daily WhatsApp food feedback.",
                [
                    "collect registration form data",
                    "run multi-model diet generation",
                    "analyze model outputs across rounds",
                    "generate diet plan",
                    "trigger daily food feedback flow",
                    "calculate calories and supplements",
                    "send push and WhatsApp feedback"
                ]),
            new(
                "content-generation-pipeline",
                "User wants content generation from text or voice through images, short videos, music, avatar audio, captions, descriptions, and scheduled posts.",
                [
                    "audio-to-text when needed",
                    "improve text with AI",
                    "plan images and short videos",
                    "generate image and video prompts",
                    "generate music and avatar audio",
                    "assemble platform-specific video",
                    "schedule social posts"
                ]),
            new(
                "design-pattern-skill-library",
                "User needs one skill for each design pattern with when to use, how to use, per-language variants, and common combinations.",
                [
                    "classify design patterns",
                    "create per-pattern skill",
                    "add per-language alternatives",
                    "add common combinations",
                    "make skill library usable by different AI tools"
                ]),
            new(
                "advanced-uml-validation",
                "Model must support advanced UML scenarios to document and validate full cycles, automations, and entity relations.",
                [
                    "parse UML or drawio source",
                    "extract automations and entity relations",
                    "validate full lifecycle",
                    "produce documentation and validation evidence"
                ]),
            new(
                "dynamic-skill-factory",
                "Use dynamic code skill zips to generate reusable dynamic code skills and generic processing approaches.",
                [
                    "load dynamic skill examples",
                    "extract generic dynamic DNA",
                    "create reusable skill templates",
                    "validate against implementation examples"
                ]),
            new(
                "rag-plan-per-ai-stage",
                "Each AI stage needs a RAG request plan before execution and a storage plan after execution and feedback.",
                [
                    "plan RAG queries before AI call",
                    "execute AI stage with context",
                    "store outputs and feedback",
                    "plan graph or vector memory updates",
                    "make retrieval backend swappable"
                ]),
            new(
                "debuggable-ai-microservices",
                "AI microservices need visible orchestrated flow, node input/output debug traces, code generation, testing, service start, and integration into the flow.",
                [
                    "render orchestrated flow in UI",
                    "capture node inputs, prompts, intermediate results, and outputs",
                    "generate code",
                    "start microservice",
                    "test service",
                    "integrate service into flow"
                ])
        ];

    public static OrchestrationPromptAcceptanceCase[] CreatePromptFileAcceptanceCases()
        => PromptFileAcceptanceCases().ToArray();

    private static RoutingTrainingExample Build(
        string id,
        string prompt,
        string taskSlice,
        string domainHint,
        string[] candidates,
        string chosen,
        string rejected,
        string correctReason,
        string wrongConsequence)
        => new(
            id,
            prompt,
            taskSlice,
            null,
            domainHint,
            candidates,
            chosen,
            rejected,
            correctReason,
            wrongConsequence);

    private static IEnumerable<(string Prompt, string Slice)> OrchestrationPrompts()
    {
        yield return ("Plan a complex implementation across architecture, API contract, tests, and rollout before any code change.", "plan dependency order architecture API tests rollout");
        yield return ("Route this request to the correct planning sub-phase and keep Gate B before implementation.", "routing planning sub-phase Gate B");
        yield return ("Decide whether this prompt belongs to orchestration, skills, or direct code execution.", "orchestration domain routing decision");
        yield return ("Build DPO rejected examples for plan-vs-implementation errors in a multi-agent workflow.", "DPO plan implementation contrast");
        yield return ("Choose the model unit that can manage dependency ordering across multiple logical flows.", "model unit dependency orchestration");
        yield return ("The user asks for a phase plan, arbiter review, and sequence of gates before work starts.", "phase plan arbiter gates");
        yield return ("Split a large task into routable slices for specialist model units and track the chosen unit.", "task slice model routing");
        yield return ("Teach the router to detect architecture planning prompts and send them to the orchestration baseline.", "teach router architecture planning");
        yield return ("Evaluate whether a proposed implementation should wait for design topology and source authority.", "design topology authority planning");
        yield return ("Create an orchestration flow for model selection between translation, documentation, and planning units.", "orchestration flow model selection");
        yield return ("Identify the correct sub-phase for security authorization planning before backend implementation.", "security authorization planning sub-phase");
        yield return ("Prepare a plan that preserves source lineage, current version, and validation gates.", "source lineage version validation gates");
        yield return ("The request mentions workflow DAG, dependency edges, and recovery state for a complex task.", "workflow DAG dependency recovery");
        yield return ("Route a prompt about learning-set quality, negative DPO, and router evidence.", "learning-set DPO router evidence");
        yield return ("Analyze a design review and decide which model unit should continue the orchestration work.", "design review orchestration routing");
        yield return ("Detect that a task is about planning complex work rather than translating text.", "planning not translation");
        yield return ("Detect that a task is about model orchestration rather than documenting legacy code.", "model orchestration not documentation");
        yield return ("Select the V309 orchestration model for task decomposition, phase selection, and Gate B.", "V309 task decomposition phase selection");
        yield return ("The prompt asks how to improve weighted accuracy for orchestration and planning DPO rows.", "orchestration planning DPO metrics");
        yield return ("The executor needs to know what to train next from accepted and rejected run evidence.", "training continuation accepted rejected evidence");
    }

    private static IEnumerable<(string Prompt, string Slice)> UserScenarioPrompts()
    {
        foreach (var scenario in CreateUserPromptScenarioExpectations())
            yield return (
                scenario.Prompt,
                string.Join(" ", scenario.ExpectedSteps));
    }

    private static IEnumerable<OrchestrationPromptAcceptanceCase> PromptFileAcceptanceCases()
    {
        yield return new(
            "initial-n8n-figma-feedback-flow",
            "Create a skill set for a new project with .NET server on Kubernetes, React Native client, n8n-like flow management, trace id job calls, Figma node tree input, several AI model passes, model review, Elasticsearch middle-result storage, and positive neutral negative user feedback reused by the machine.",
            [
                "create reusable project skill set",
                "accept trace id and job body",
                "process raw Figma node tree",
                "run several AI model passes",
                "review model outputs with another model",
                "store prompts and middle results in Elasticsearch",
                "feed user feedback back into later execution"
            ],
            [
                "intake-source",
                "architecture-flow-design",
                "model-round-execution",
                "feedback-memory-loop",
                "skill-library-packaging"
            ]);

        yield return new(
            "figma-whole-system",
            "User enters Figma, selects client-side and server-side technologies plus database, and the system orchestrates the whole system.",
            [
                "go over screens",
                "analyze each screen component with AI models",
                "analyze screens to understand the system",
                "generate architecture as flows",
                "create per-screen technical design",
                "gather feedback and process user input"
            ],
            [
                "intake-source",
                "screen-and-system-analysis",
                "architecture-flow-design",
                "technical-design-and-implementation-plan",
                "feedback-memory-loop"
            ]);

        yield return new(
            "figma-existing-system-integration",
            "User wants to integrate a Figma design into an existing system using orchestrated documentation, microservices, skillsets, design theme extraction, and style application.",
            [
                "orchestrate documentation creation for existing parts",
                "create skillsets from the documented system",
                "derive design system theme",
                "apply style to existing modules when not a new module",
                "validate integration boundaries"
            ],
            [
                "intake-source",
                "screen-and-system-analysis",
                "architecture-flow-design",
                "skill-library-packaging",
                "test-debug-deploy"
            ]);

        yield return new(
            "system-from-explanation",
            "User wants to implement a whole system from scratch when design is incomplete, using user explanation, generated requirements, design theme, architecture, skills, implementation plan, tests, deployment, visualization, debugging, and source code delivery.",
            [
                "generate requirements from user explanation",
                "derive design theme",
                "create architecture",
                "create skills",
                "create implementation plan",
                "implement, test, deploy, visualize, and debug"
            ],
            [
                "intake-source",
                "architecture-flow-design",
                "technical-design-and-implementation-plan",
                "skill-library-packaging",
                "test-debug-deploy"
            ]);

        yield return new(
            "diet-plan-client-app",
            "User wants an app where clients register, fill forms, receive AI generated diet plans from several model rounds, submit daily WhatsApp food feedback, get calorie and supplement calculations, and receive app and WhatsApp adjustments.",
            [
                "collect registration form data",
                "run multi-model diet generation",
                "analyze model outputs across rounds",
                "generate diet plan",
                "trigger daily food feedback flow",
                "calculate calories and supplements",
                "send push and WhatsApp feedback"
            ],
            [
                "intake-source",
                "architecture-flow-design",
                "model-round-execution",
                "feedback-memory-loop",
                "technical-design-and-implementation-plan"
            ]);

        yield return new(
            "content-generation-pipeline",
            "User wants content generation from voice or text through audio-to-text, text improvement, image and short video planning, image generation, video generation, music, avatar audio, platform captions, descriptions, and scheduled posts.",
            [
                "audio-to-text when needed",
                "improve text with AI",
                "plan images and short videos",
                "generate image and video prompts",
                "generate music and avatar audio",
                "assemble platform-specific video",
                "schedule social posts"
            ],
            [
                "intake-source",
                "model-round-execution",
                "architecture-flow-design",
                "technical-design-and-implementation-plan"
            ]);

        yield return new(
            "design-pattern-skill-library",
            "There are different design pattern explanations, and the user needs one skill for each pattern with when to use it, how to use it, per-language variants, and common combinations.",
            [
                "classify design patterns",
                "create per-pattern skill",
                "add per-language alternatives",
                "add common combinations",
                "make skill library usable by different AI tools"
            ],
            [
                "architecture-flow-design",
                "skill-library-packaging",
                "multi-technology-alternatives"
            ]);

        yield return new(
            "advanced-uml-validation",
            "The model must support advanced UML and drawio scenarios to document and validate full cycles, automations, and entity relations.",
            [
                "parse UML or drawio source",
                "extract automations and entity relations",
                "validate full lifecycle",
                "produce documentation and validation evidence"
            ],
            [
                "intake-source",
                "architecture-flow-design",
                "test-debug-deploy"
            ]);

        yield return new(
            "project-base-drawio-flow-examples",
            "Project base drawio has example flow for scenarios one through five, and the model must understand those flows as architecture input.",
            [
                "load project base drawio examples",
                "map scenario flows to architecture",
                "validate flow coverage across scenarios one through five",
                "reuse graph structure for implementation planning"
            ],
            [
                "intake-source",
                "architecture-flow-design",
                "test-debug-deploy"
            ]);

        yield return new(
            "small-business-networking-app",
            "The model needs to generate, deploy, and debug a system like Small Business Networking App functional specifications and architecture.",
            [
                "read functional specification",
                "derive architecture",
                "generate implementation plan",
                "deploy the system",
                "debug the running system"
            ],
            [
                "intake-source",
                "architecture-flow-design",
                "technical-design-and-implementation-plan",
                "test-debug-deploy"
            ]);

        yield return new(
            "dynamic-code-skill-factory",
            "Dynamic code examples and genie code skill zips must be used to generate dynamic code skills and generic processing approaches because the dynamic approach is company DNA.",
            [
                "load dynamic skill examples",
                "extract generic dynamic DNA",
                "create reusable skill templates",
                "validate against implementation examples"
            ],
            [
                "intake-source",
                "architecture-flow-design",
                "skill-library-packaging",
                "test-debug-deploy"
            ]);

        yield return new(
            "debuggable-ai-microservices",
            "AI microservices need prompts from previous jobs, visible orchestrated flow in UI, node input output debugging, generated code, service startup, tests, and integration into the flow.",
            [
                "load prompts from previous jobs",
                "render orchestrated flow in UI",
                "capture node inputs and outputs",
                "generate code",
                "start microservice",
                "test service",
                "integrate service into flow"
            ],
            [
                "rag-context-planning",
                "architecture-flow-design",
                "technical-design-and-implementation-plan",
                "test-debug-deploy",
                "feedback-memory-loop"
            ]);

        yield return new(
            "generic-fabrics-and-microservice-base",
            "The user prefers generic reusable interfaces for databases, queues, third-party AI, dynamic document repository filtering by non-empty values, job queues at each level, and a generic microservice base with architectural capabilities.",
            [
                "define generic database interfaces",
                "define generic queue interfaces",
                "define third-party AI fabric",
                "support dynamic document storage and non-empty filters",
                "manage async job queues at each level",
                "create generic microservice base"
            ],
            [
                "architecture-flow-design",
                "technical-design-and-implementation-plan",
                "multi-technology-alternatives"
            ]);

        yield return new(
            "technology-alternatives",
            "The system must support alternative queues like Kafka and SQS, server implementations in Node.js Rust Java Python PHP, clients in native Angular Vue, and database fabric alternatives such as Mongo Prisma Redis Azure AI Search Graph AI and other common models.",
            [
                "catalog queue alternatives",
                "catalog server language alternatives",
                "catalog client technology alternatives",
                "catalog database fabric alternatives",
                "organize per-technology skill subfolders"
            ],
            [
                "multi-technology-alternatives",
                "skill-library-packaging",
                "technical-design-and-implementation-plan"
            ]);

        yield return new(
            "rag-plan-per-ai-stage",
            "Each AI stage needs a RAG request plan before execution and a storage plan after execution and feedback, with swappable Azure AI, Cosmos DB, graph, Pinecone, Gremlin, and open-source backends.",
            [
                "plan RAG queries before AI call",
                "execute AI stage with context",
                "store outputs and feedback",
                "plan graph or vector memory updates",
                "make retrieval backend swappable"
            ],
            [
                "rag-context-planning",
                "model-round-execution",
                "feedback-memory-loop",
                "multi-technology-alternatives"
            ]);

        yield return new(
            "advanced-implementation-skills",
            "Advanced skills must cover phase implementation prompts, unit tests, e2e tests, UI tests, DevOps and CI CD, documentation, Swagger, MCP, logging, safe code, vulnerability checks, upgrades, resource management, and optimization.",
            [
                "create prompts for implementation phases",
                "require high unit test coverage",
                "add e2e tests",
                "add UI component tests",
                "generate DevOps and CI CD",
                "document everything",
                "add Swagger and MCP",
                "add logging and safe code",
                "optimize resources"
            ],
            [
                "technical-design-and-implementation-plan",
                "test-debug-deploy",
                "skill-library-packaging",
                "multi-technology-alternatives"
            ]);

        yield return new(
            "graphrag-and-devops-alternatives",
            "The architecture must compare GraphRAG alternatives and DevOps hosting alternatives including Neo4j, LightRAG, nano-graphrag, LangChain, LlamaIndex, Graphlit, Memgraph, Neptune, Contextual AI, HybridRAG, Railway, Render, Fly.io, Kubernetes, Nomad, Docker Swarm, OpenShift, and Rancher.",
            [
                "compare GraphRAG alternatives",
                "compare DevOps hosting alternatives",
                "select backend by project constraints",
                "keep alternatives swappable",
                "document quick-pick routing"
            ],
            [
                "rag-context-planning",
                "multi-technology-alternatives"
            ]);

        yield return new(
            "ai-tool-library-chat-and-phase-prompts",
            "The skill library must be organized so Copilot, Claude AI, Claude Code, Cursor, and other AI tools can use it, with ordered prompts to implement the system by phases and a chat where the user can interact with the system.",
            [
                "organize skills for multiple AI tools",
                "create ordered phase prompts",
                "support user chat interaction",
                "make skill loading explicit",
                "persist state for continuation"
            ],
            [
                "skill-library-packaging",
                "technical-design-and-implementation-plan",
                "feedback-memory-loop"
            ]);
    }

    private static IEnumerable<(string Prompt, string Slice)> TranslationPrompts()
    {
        yield return ("Translate this Russian customer message into English.", "RU_EN translation");
        yield return ("Convert the Hebrew sentence to English while preserving tone.", "HE_EN translation");
        yield return ("Translate an English support reply into Russian.", "EN_RU translation");
        yield return ("The user asks for bilingual sentence translation, not planning.", "sentence translation");
        yield return ("Produce an English version of this Russian product description.", "product translation");
        yield return ("Translate a short travel message from Russian to English.", "travel translation");
        yield return ("Turn this English instruction into natural Russian.", "EN_RU natural translation");
        yield return ("Translate a Hebrew legal phrase into English.", "legal translation");
        yield return ("The prompt is only a language conversion request.", "language conversion");
        yield return ("Translate text and measure translation quality.", "translation quality");
    }

    private static IEnumerable<(string Prompt, string Slice)> DocumentationPrompts()
    {
        yield return ("Document this legacy C# method from parameters to side effects.", "dotnet method documentation");
        yield return ("Summarize a WebForms Page_Load handler bottom-up.", "WebForms Page_Load documentation");
        yield return ("Explain what this MVC controller action does and which service calls it makes.", "MVC controller documentation");
        yield return ("Generate documentation for an ADO.NET repository method.", "ADO.NET repository documentation");
        yield return ("Propagate function meaning upward to its callers in legacy .NET code.", "caller propagation documentation");
        yield return ("Describe a WPF event handler and its UI side effects.", "WPF handler documentation");
        yield return ("Document a .NET Framework HttpModule for an organization without AI access.", "HttpModule documentation");
        yield return ("Explain a code-behind method and its database query.", "code-behind documentation");
        yield return ("Create precise docs for a legacy service method.", "legacy service documentation");
        yield return ("Document a C# utility function and edge cases.", "C# utility documentation");
    }
}
