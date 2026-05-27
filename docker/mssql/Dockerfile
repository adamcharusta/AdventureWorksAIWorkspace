# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/mssql/server:2025-latest

USER root

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /var/opt/mssql/backup

COPY --chmod=755 init-adventureworks.sh /usr/local/bin/init-adventureworks.sh

ENTRYPOINT ["/usr/local/bin/init-adventureworks.sh"]
