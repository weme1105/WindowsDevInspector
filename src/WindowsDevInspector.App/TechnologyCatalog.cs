namespace WindowsDevInspector.App;

public static class TechnologyCatalog
{
    public static IReadOnlyList<TechnologyGroup> CreateDefaultGroups()
    {
        return [
            CreateGroup("frontend", "前端", true, [
                Item("javascript", "JavaScript", true), Item("typescript", "TypeScript", true), Item("html", "HTML", true), Item("css", "CSS", true), Item("sass", "Sass / SCSS", true),
                Item("react", "React"), Item("nextjs", "Next.js"), Item("vue", "Vue"), Item("nuxt", "Nuxt"), Item("angular", "Angular", true),
                Item("svelte", "Svelte"), Item("sveltekit", "SvelteKit"), Item("solidjs", "SolidJS"), Item("qwik", "Qwik"), Item("astro", "Astro"),
                Item("remix", "Remix"), Item("vite", "Vite"), Item("webpack", "webpack"), Item("rollup", "Rollup"), Item("esbuild", "esbuild"),
                Item("parcel", "Parcel"), Item("tailwindcss", "Tailwind CSS"), Item("bootstrap", "Bootstrap", true), Item("material-ui", "Material UI"), Item("storybook", "Storybook"),
                Item("nodejs", "Node.js"), Item("npm", "npm", true), Item("nvm", "nvm", true), Item("pnpm", "pnpm"), Item("yarn", "Yarn"), Item("playwright", "Playwright")
            ]),
            CreateGroup("backend", "後端", true, [
                Item("csharp", "C#", true), Item("cpp", "C++"), Item("dotnet", ".NET", true), Item("netcore", ".NET Core", true), Item("aspnetcore", "ASP.NET Core"), Item("java", "Java"),
                Item("springboot", "Spring Boot"), Item("kotlin", "Kotlin"), Item("go", "Go"), Item("python", "Python"), Item("django", "Django"),
                Item("fastapi", "FastAPI"), Item("flask", "Flask"), Item("nodejs", "Node.js"), Item("express", "Express"), Item("nestjs", "NestJS"),
                Item("ruby", "Ruby"), Item("rails", "Ruby on Rails"), Item("php", "PHP"), Item("laravel", "Laravel"), Item("rust", "Rust"),
                Item("actix-web", "Actix Web"), Item("elixir", "Elixir"), Item("phoenix", "Phoenix"), Item("scala", "Scala"), Item("sbt", "sbt"),
                Item("grpc", "gRPC"), Item("graphql", "GraphQL"), Item("redis", "Redis"), Item("rabbitmq", "RabbitMQ"), Item("kafka", "Apache Kafka")
            ]),
            CreateGroup("database", "資料庫", true, [
                Item("oracle-db", "Oracle Database", true), Item("sqlserver", "SQL Server", true), Item("localdb", "SQL Server LocalDB"), Item("ssms", "SQL Server Management Studio"), Item("sqlcmd", "sqlcmd"),
                Item("postgresql", "PostgreSQL"), Item("pgadmin", "pgAdmin"), Item("mysql", "MySQL"), Item("mariadb", "MariaDB"), Item("mysql-workbench", "MySQL Workbench"),
                Item("sqlite", "SQLite"), Item("oracle-client", "Oracle Client"), Item("mongodb", "MongoDB"), Item("mongosh", "MongoDB Shell"), Item("redis", "Redis"),
                Item("elasticsearch", "Elasticsearch"), Item("opensearch", "OpenSearch"), Item("cassandra", "Apache Cassandra"), Item("dynamodb", "Amazon DynamoDB"), Item("cosmosdb", "Azure Cosmos DB"),
                Item("firestore", "Cloud Firestore"), Item("neo4j", "Neo4j"), Item("influxdb", "InfluxDB"), Item("timescaledb", "TimescaleDB"), Item("clickhouse", "ClickHouse"),
                Item("snowflake", "Snowflake"), Item("bigquery", "BigQuery"), Item("redshift", "Amazon Redshift"), Item("dbt", "dbt"), Item("odbc-driver", "ODBC Driver")
            ]),
            CreateGroup("devops", "雲端 / DevOps", true, [
                Item("docker", "Docker", true), Item("docker-desktop", "Docker Desktop"), Item("wsl", "WSL"), Item("kubernetes", "Kubernetes"), Item("kubectl", "kubectl"),
                Item("helm", "Helm"), Item("terraform", "Terraform"), Item("opentofu", "OpenTofu"), Item("ansible", "Ansible"), Item("powershell7", "PowerShell 7"),
                Item("git", "Git", true), Item("github-cli", "GitHub CLI"), Item("azure-cli", "Azure CLI"), Item("azure-developer-cli", "Azure Developer CLI"), Item("aws-cli", "AWS CLI"),
                Item("google-cloud-cli", "Google Cloud CLI"), Item("github-actions", "GitHub Actions"), Item("azure-devops", "Azure DevOps"), Item("jenkins", "Jenkins", true), Item("gitlab-ci", "GitLab CI"),
                Item("argo-cd", "Argo CD"), Item("prometheus", "Prometheus"), Item("grafana", "Grafana"), Item("opentelemetry", "OpenTelemetry"), Item("nginx", "Nginx"),
                Item("traefik", "Traefik"), Item("podman", "Podman"), Item("minikube", "Minikube"), Item("kind", "Kind"), Item("winget", "winget")
            ]),
            CreateGroup("qa", "QA / 自動化", true, [
                Item("playwright", "Playwright"), Item("selenium", "Selenium"), Item("cypress", "Cypress"), Item("webdriverio", "WebdriverIO"), Item("appium", "Appium"),
                Item("postman", "Postman", true), Item("newman", "Newman"), Item("insomnia", "Insomnia"), Item("rest-assured", "Rest Assured"), Item("k6", "k6"),
                Item("jmeter", "JMeter"), Item("locust", "Locust"), Item("xunit", "xUnit"), Item("nunit", "NUnit"), Item("mstest", "MSTest"),
                Item("junit", "JUnit"), Item("testng", "TestNG"), Item("pytest", "pytest"), Item("jest", "Jest"), Item("vitest", "Vitest"),
                Item("testing-library", "Testing Library"), Item("mock-service-worker", "Mock Service Worker"), Item("cucumber", "Cucumber"), Item("specflow", "SpecFlow"), Item("allure-report", "Allure Report"),
                Item("sonarqube", "SonarQube"), Item("eslint", "ESLint"), Item("prettier", "Prettier"), Item("reportportal", "ReportPortal"), Item("browserstack", "BrowserStack")
            ]),
            CreateGroup("mobile", "Mobile", false, [
                Item("android-sdk", "Android SDK"), Item("android-studio", "Android Studio"), Item("android-emulator", "Android Emulator"), Item("adb", "ADB"), Item("gradle", "Gradle"),
                Item("kotlin", "Kotlin"), Item("java", "Java"), Item("flutter", "Flutter"), Item("dart", "Dart"), Item("react-native", "React Native"),
                Item("expo", "Expo"), Item("dotnet-maui", ".NET MAUI"), Item("xamarin", "Xamarin"), Item("ionic", "Ionic"), Item("capacitor", "Capacitor"),
                Item("cordova", "Cordova"), Item("firebase-cli", "Firebase CLI"), Item("fastlane", "Fastlane"), Item("app-center-cli", "App Center CLI"), Item("maui-check", "MAUI Check"),
                Item("visual-studio-android-workload", "Visual Studio Android workload"), Item("nodejs", "Node.js"), Item("cocoapods", "CocoaPods"), Item("swift", "Swift"), Item("xcode-remote-build", "Xcode Remote Build")
            ]),
            CreateGroup("desktop", "Desktop", true, [
                Item("wpf", "WPF"), Item("winui3", "WinUI 3"), Item("windows-app-sdk", "Windows App SDK"), Item("dotnet-desktop-runtime", ".NET Desktop Runtime"), Item("dotnet", ".NET SDK"),
                Item("visual-studio", "Visual Studio", true), Item("vscode", "Visual Studio Code", true), Item("visual-studio-build-tools", "Visual Studio Build Tools"), Item("msbuild", "MSBuild"), Item("windows-sdk", "Windows SDK"),
                Item("msix-packaging-tool", "MSIX Packaging Tool"), Item("clickonce", "ClickOnce"), Item("wix-toolset", "WiX Toolset"), Item("cmake", "CMake"), Item("ninja", "Ninja"),
                Item("vcpkg", "vcpkg"), Item("msvc", "MSVC"), Item("clang", "Clang"), Item("qt", "Qt"), Item("avalonia", "Avalonia"),
                Item("uno-platform", "Uno Platform"), Item("electron", "Electron"), Item("tauri", "Tauri"), Item("powershell", "PowerShell"), Item("signtool", "signtool"),
                Item("windbg", "WinDbg"), Item("procmon", "ProcMon"), Item("process-explorer", "Process Explorer"), Item("windows-terminal", "Windows Terminal"), Item("developer-mode", "Developer Mode"), Item("long-paths", "Long Paths", true)
            ])
        ];
    }

    private static TechnologyGroup CreateGroup(string id, string name, bool isExpanded, IReadOnlyList<TechnologyItem> technologies)
    {
        TechnologyItem[] indexedTechnologies = technologies
            .OrderBy(technology => technology.Name, StringComparer.OrdinalIgnoreCase)
            .Select((technology, index) => new TechnologyItem(technology.Id, technology.Name, technology.IsSelected)
            {
                OriginalIndex = index
            })
            .ToArray();

        return new TechnologyGroup(id, name, indexedTechnologies)
        {
            IsExpanded = isExpanded
        };
    }

    private static TechnologyItem Item(string id, string name, bool isSelected = false)
    {
        return new TechnologyItem(id, name, isSelected);
    }
}
