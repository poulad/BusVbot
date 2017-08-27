#!/usr/bin/env bash

# Run database
docker run -d -p 5432:5432 --name busvbot-postgres -e POSTGRES_PASSWORD=password -e POSTGRES_USER=busvbot -e POSTGRES_DB=busvbot postgres

# Connect to running database
docker exec --interactive --tty busvbot-postgres psql --username=busvbot
