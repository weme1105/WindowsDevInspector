# Environment Profiles

WindowsDevInspector uses explicit technology selection. A user chooses one or more languages, frameworks, runtimes, databases, or platform tools from grouped searchable lists.

## Technology Grouping

There is no independent role-selection state. Labels such as Frontend, Backend, Database, QA, DevOps, Mobile, and Desktop may be used as browsing groups, but selecting a group must not automatically select all technologies in that group.

| ID | Name | Purpose |
|---|---|---|
| frontend | Frontend | Browser, JavaScript, TypeScript, package manager, UI framework, build tool, and local HTTPS technologies. |
| backend | Backend | Runtime, SDK, CLI, API server, service port, package source, certificate, and local service technologies. |
| database | Database | Database client, driver, local engine, migration tool, SQL shell, ODBC/OLE DB, and connectivity technologies. |
| qa | QA / Testing | Browser driver, API test tool, load test tool, device/emulator, report tool, and test runtime technologies. |
| devops | DevOps / SRE | Container, WSL, Kubernetes, cloud CLI, IaC, shell, credential helper, and networking technologies. |
| mobile | Mobile | Android, iOS-adjacent tooling on Windows, emulator, signing, SDK, and device bridge technologies. |
| desktop | Desktop | .NET Desktop, Windows SDK, Visual Studio Build Tools, MSIX, signing, and native build technologies. |

## Technology Selection UX

The technology selector should be a searchable multi-select dropdown grouped by category. Each option should have a stable ID, display name, category, and optional aliases.

Recommended behavior:

- Allow selecting multiple items across categories.
- Support search by name and alias, for example `js` finds JavaScript.
- Show selected items as removable chips.
- Do not auto-select technologies from a group label.
- Use groups only for browsing, filtering, and display.
- Run Common checks even when no role or technology is selected.

## Frontend Technologies

Initial catalog: 30 common frontend languages, frameworks, runtimes, and build tools.

| ID | Name | Type |
|---|---|---|
| javascript | JavaScript | Language |
| typescript | TypeScript | Language |
| html | HTML | Markup |
| css | CSS | Style |
| sass | Sass / SCSS | Style |
| react | React | Framework |
| nextjs | Next.js | Framework |
| vue | Vue | Framework |
| nuxt | Nuxt | Framework |
| angular | Angular | Framework |
| svelte | Svelte | Framework |
| sveltekit | SvelteKit | Framework |
| solidjs | SolidJS | Framework |
| qwik | Qwik | Framework |
| astro | Astro | Framework |
| remix | Remix | Framework |
| vite | Vite | Build Tool |
| webpack | webpack | Build Tool |
| rollup | Rollup | Build Tool |
| esbuild | esbuild | Build Tool |
| parcel | Parcel | Build Tool |
| tailwindcss | Tailwind CSS | CSS Framework |
| bootstrap | Bootstrap | CSS Framework |
| material-ui | Material UI | UI Library |
| storybook | Storybook | UI Tool |
| nodejs | Node.js | Runtime |
| npm | npm | Package Manager |
| pnpm | pnpm | Package Manager |
| yarn | Yarn | Package Manager |
| playwright | Playwright | Testing |

## Backend Technologies

Initial catalog: 30 common backend languages, runtimes, frameworks, and service toolchains.

| ID | Name | Type |
|---|---|---|
| csharp | C# | Language |
| dotnet | .NET | Runtime / SDK |
| aspnetcore | ASP.NET Core | Framework |
| java | Java | Language |
| springboot | Spring Boot | Framework |
| kotlin | Kotlin | Language |
| go | Go | Language / Toolchain |
| python | Python | Language |
| django | Django | Framework |
| fastapi | FastAPI | Framework |
| flask | Flask | Framework |
| nodejs-backend | Node.js | Runtime |
| express | Express | Framework |
| nestjs | NestJS | Framework |
| ruby | Ruby | Language |
| rails | Ruby on Rails | Framework |
| php | PHP | Language |
| laravel | Laravel | Framework |
| rust | Rust | Language / Toolchain |
| actix-web | Actix Web | Framework |
| elixir | Elixir | Language |
| phoenix | Phoenix | Framework |
| scala | Scala | Language |
| sbt | sbt | Build Tool |
| grpc | gRPC | API Tooling |
| graphql | GraphQL | API Tooling |
| redis | Redis | Cache / Service |
| rabbitmq | RabbitMQ | Message Broker |
| kafka | Apache Kafka | Message Broker |
| docker | Docker | Runtime / Packaging |

## Database Technologies

Initial catalog: 30 common database engines, clients, drivers, and data platforms.

| ID | Name | Type |
|---|---|---|
| sqlserver | SQL Server | Relational Database |
| localdb | SQL Server LocalDB | Local Database |
| ssms | SQL Server Management Studio | Client Tool |
| sqlcmd | sqlcmd | CLI Tool |
| postgresql | PostgreSQL | Relational Database |
| pgadmin | pgAdmin | Client Tool |
| mysql | MySQL | Relational Database |
| mariadb | MariaDB | Relational Database |
| mysql-workbench | MySQL Workbench | Client Tool |
| sqlite | SQLite | Embedded Database |
| oracle-db | Oracle Database | Relational Database |
| oracle-client | Oracle Client | Client / Driver |
| mongodb | MongoDB | Document Database |
| mongosh | MongoDB Shell | CLI Tool |
| redis-db | Redis | Key-value Store |
| elasticsearch | Elasticsearch | Search Database |
| opensearch | OpenSearch | Search Database |
| cassandra | Apache Cassandra | Wide-column Database |
| dynamodb | Amazon DynamoDB | Cloud Database |
| cosmosdb | Azure Cosmos DB | Cloud Database |
| firestore | Cloud Firestore | Cloud Database |
| neo4j | Neo4j | Graph Database |
| influxdb | InfluxDB | Time-series Database |
| timescaledb | TimescaleDB | Time-series Database |
| clickhouse | ClickHouse | Analytics Database |
| snowflake | Snowflake | Data Warehouse |
| bigquery | BigQuery | Data Warehouse |
| redshift | Amazon Redshift | Data Warehouse |
| dbt | dbt | Transformation Tool |
| odbc-driver | ODBC Driver | Driver |

## MVP Check Mapping

The first implementation should not attempt to check every selected item. It should use this catalog to drive the UI and then map only supported items to actual checks.

MVP supported checks:

- Common: Windows version/build, PATH health, Long Paths, Developer Mode, `D:\Source`, `D:\Projects`, `D:\Note`, PowerShell 7, Git, winget, Chocolatey.
- Frontend: Node.js, npm, pnpm, Yarn, Angular CLI, Vite, Playwright.
- Backend: .NET SDK/runtime, Go, Python, Java, Docker, Redis CLI where present.
- Database: SQL Server tooling, sqlcmd, ODBC Driver, LocalDB, PostgreSQL CLI, MySQL CLI, SQLite.
- QA: Playwright, browser availability, API test CLI if selected.
- DevOps: Docker, WSL, Kubernetes CLI, Azure CLI, Terraform.

Unsupported selected technologies should be shown as `Info` with a clear message that catalog support exists but automated diagnostics are not implemented yet.
