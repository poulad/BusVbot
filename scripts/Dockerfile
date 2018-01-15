FROM microsoft/dotnet:2-runtime

ARG project
ARG env=Production

ENV PROJECT_NAME ${project}
ENV ASPNETCORE_URLS http://+:80
ENV ASPNETCORE_ENVIRONMENT ${env}

WORKDIR /app
EXPOSE 80
COPY bin/publish .

ENTRYPOINT dotnet "${PROJECT_NAME}.dll"