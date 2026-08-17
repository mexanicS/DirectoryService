# DirectoryService monitoring

The local monitoring stack consists of Prometheus and Grafana. The application exposes OpenTelemetry metrics at `http://localhost:5128/metrics` only when the Prometheus endpoint is enabled. It is enabled by default in the Development environment and disabled in the base production configuration.

## Start locally

1. Optionally copy `.env.example` to `.env` and change the Grafana credentials.
2. Start the infrastructure:

   ```powershell
   docker compose up -d postgres seq prometheus grafana
   ```

3. Start DirectoryService with its HTTP development profile:

   ```powershell
   dotnet run --project src/DirectoryService.Presentation --launch-profile http
   ```

4. Open the services:

   - DirectoryService metrics: <http://localhost:5128/metrics>
   - Prometheus: <http://localhost:9090>
   - Grafana: <http://localhost:3000>
   - Seq: <http://localhost:8081>

Grafana provisions the Prometheus datasource and the `DirectoryService Overview` dashboard automatically. If no `.env` file is created, the local Grafana credentials are `admin` / `admin`.

## Verify the stack

- In Prometheus, open **Status > Target health** and check that `directory-service` is `UP`.
- In Grafana, open **Dashboards > DirectoryService > DirectoryService Overview**.
- Send a few requests to the API and wait for one 15-second scrape interval.

Prometheus reaches the host-run application through `host.docker.internal:5128`. If the application is later containerized, change the scrape target in `prometheus/prometheus.yml` to the Compose service name and container port.

## Production note

Do not expose `/metrics` to the public internet. Keep `Observability:Prometheus:EndpointEnabled` disabled unless the endpoint is protected by a private network or dedicated management port. For a production deployment, prefer the stable OTLP exporter and a Prometheus OTLP receiver or OpenTelemetry Collector.
