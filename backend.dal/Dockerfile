# This Dockerfile is meant to deploy a dev database
# environment for the backend.
#
# It is based on the official Postgres Docker image.
# It persists the volume.
# It sets the default right url and the default user.
# 
# Do not user in prod ad the password is in clear in This
# file that is tracked by git.

FROM postgres:18-trixie

WORKDIR /app

ENV POSTGRES_USER=2c032f06 \
    POSTGRES_PASSWORD=39dfe973 \
    POSTGRES_DB=alfred_db \
    PGPORT=4040

RUN groupadd -gid 2222 alfred && useradd -gid 2222 -uid 2222 alfred

VOLUME /var/lib/postgresql

RUN chown alfred:alfred /var/lib/postgresql && \
    chmod 700 /var/lib/postgresql

USER alfred

EXPOSE 4040

