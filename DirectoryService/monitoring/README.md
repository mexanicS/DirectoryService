# DirectoryService monitoring

The local monitoring stack consists of Prometheus and Grafana. The application exposes OpenTelemetry metrics at `http://localhost:5128/metrics` only when the Prometheus endpoint is enabled. It is enabled by default in the Development environment and disabled in the base production configuration.

## Docker Compose application stack

The recommended local setup runs the entire application in Docker. From the
repository root, configure `.env` as described in the root README, then run:

```sh
docker compose --profile monitoring up --detach --wait --wait-timeout 180
```

- Grafana: <http://localhost/grafana/>
- Prometheus: <http://localhost/prometheus/>

Only nginx publishes a host port. If `HTTP_PORT` changes, include that port in the
URLs. Grafana credentials come from the root `.env`; the example uses `admin` /
`change-me`. Prometheus scrapes `backend:${BACKEND_PORT}` inside the Compose
network using the inline config in the root `docker-compose.yml`.

The dashboard and datasource are provisioned automatically. Select **Last 15
minutes** and refresh; an absolute interval before this Prometheus instance
started has no data. The scrape interval is 15 seconds. Send API requests and
wait for several scrapes for request-rate and latency charts.

```sh
docker compose --profile monitoring logs --tail 100 grafana prometheus
docker compose --profile monitoring down
```

`down` preserves data; adding `--volumes` removes the monitoring history as well
as the application's database volumes. Set `GF_PLUGINS_PREINSTALL_DISABLED=true`
in the Grafana service environment to keep startup independent of suggested
plugin downloads; the canonical Compose already sets it.

## Legacy infrastructure for a host-run backend

Run these commands from the backend solution directory. This section describes the legacy infrastructure for a host-run backend.

1. Optionally copy `.env.example` to `.env` and change the Grafana credentials.
2. Start the infrastructure:

   ```powershell
   docker compose -f docker-compose.legacy.yml up -d postgres seq prometheus grafana
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

In this legacy setup Prometheus reaches the host-run application through
`host.docker.internal:5128`. The file `prometheus/prometheus.yml` is used only by
the legacy Compose; the application stack uses the inline config described above.

## Production note

Do not expose `/metrics` to the public internet. Keep `Observability:Prometheus:EndpointEnabled` disabled unless the endpoint is protected by a private network or dedicated management port. For a production deployment, prefer the stable OTLP exporter and a Prometheus OTLP receiver or OpenTelemetry Collector.
