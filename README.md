# PanOpticon - Interaction Statistics REST API

Public API service for interaction statistics, exposing customer interaction metrics, agent performance, Promise compliance, and fulfillment survey data. Uses **Fediverse** (ActivityPub), **Signal**, **MinIO**, and **Redis** with **Kafka** for job processing.

## Tech Stack

- **.NET 10** / **ASP.NET Core**
- **Redis** – job state and result caching
- **Kafka** – message broker for statistics publisher engine
- **MinIO** – FOSS on-premise object storage for report files

## Features

- **Metadata**: Pages, widgets, filters
- **Statistics Jobs**: Create and poll async jobs
- **Fulfillment**: Response counts, question responses, user responses
- **Reference Data**: Agents, teams, data sources, Promises, routings
- **Authentication**: Bearer token via `auth-token` header

## Quick Start

### Prerequisites

- .NET 10 SDK
- Redis
- Kafka
- MinIO (optional, for report storage)

### Build & Run

```bash
dotnet run --project PanOpticon
```

Or with explicit URL:

```bash
dotnet run --project PanOpticon --urls "http://0.0.0.0:8080"
```

### Docker Compose

```bash
docker compose up -d
```

API: `http://localhost:8080`  
Swagger UI: `http://localhost:8080/swagger`

## Configuration

Environment variables or `appsettings.json` (prefix `PanOpticon__` for env):

| Variable | Default | Description |
|---------|---------|-------------|
| `PanOpticon__RedisUrl` | `redis://localhost:6379/0` | Redis connection |
| `PanOpticon__KafkaBootstrapServers` | `localhost:9092` | Kafka broker(s) |
| `PanOpticon__KafkaStatisticsTopic` | `public_api_panopticon_statistics_publisher_engine` | Statistics topic |
| `PanOpticon__KafkaFulfillmentTopic` | `ods_export_fulfillment_questions_responses_count_v0` | Fulfillment topic |
| `PanOpticon__MinioEndpoint` | `localhost:9000` | MinIO endpoint |
| `PanOpticon__MinioAccessKey` | `minioadmin` | MinIO access key |
| `PanOpticon__MinioSecretKey` | `minioadmin` | MinIO secret key |
| `PanOpticon__MinioBucket` | `panopticon-reports` | MinIO bucket |

## API Endpoints

### Authentication

All endpoints require the `auth-token` header (Bearer token):

```
auth-token: <your-token>
```

### Metadata

- `GET /statistics/pages` – List pages
- `GET /statistics/{pageName}/widgets` – Widgets for page
- `GET /statistics/{pageName}/filters` – Filters for page

### Statistics Jobs

- `POST /statistics/{pageName}/create` – Create job
- `GET /statistics/{pageName}/index?jobId=<id>` – Poll job

### Fulfillment

- `POST /statistics/user-survey/fulfillment_response_counts`
- `POST /statistics/user-survey/fulfillment_question_responses`
- `POST /statistics/user-survey/fulfillment_user_responses`
- `GET /statistics/user-survey/fulfillment_questions`

### Reference Data

- `GET /statistics/agents`
- `GET /statistics/teams`
- `GET /statistics/dataSources`
- `GET /statistics/promises`
- `GET /statistics/routings`

## Data Sources

- **Fediverse** (ActivityPub): `fediverse_public`, `fediverse_private`
- **Signal**: `signal_private`

## Naming

All API fields use **camelCase** (e.g. `widgetsNames`, `startDate`, `companyTimeZone`).
