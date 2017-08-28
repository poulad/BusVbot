#!/usr/bin/env bash
set -e

scripts_dir="`pwd`"
root_dir="${scripts_dir}/.."
project=BusVbot

echo; echo "@> Build and publish project ${project}"; echo;
cd "${root_dir}/src/${project}" &&
    rm -rf bin/publish/ &&
    dotnet restore &&
    dotnet publish -c Release -o bin/publish/ &&
    cp -v "${scripts_dir}/Dockerfile" Dockerfile


echo; echo "@> Copy nginx Dockerfile into its context"; echo;
cd "${scripts_dir}" &&
    cp -v nginx.Dockerfile nginx/Dockerfile


echo; echo "@> Build docker compose"; echo;
cd "${scripts_dir}" &&
    docker-compose build


echo; echo "@> Remove copied Dockerfiles"; echo;
rm -v "${root_dir}/src/${project}/Dockerfile" "${scripts_dir}/nginx/Dockerfile"


echo; echo "@> Restart and update containers"; echo;
cd "${scripts_dir}" &&
    docker-compose down &&
    docker-compose up -d &&
    docker-compose logs
