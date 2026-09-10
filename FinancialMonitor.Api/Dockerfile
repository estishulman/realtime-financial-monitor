FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY FinancialMonitor.Api.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
VOLUME ["/app/data"]
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*
HEALTHCHECK --interval=10s --timeout=5s --retries=5 CMD curl --fail --silent http://localhost:8080/health || exit 1
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FinancialMonitor.Api.dll"]