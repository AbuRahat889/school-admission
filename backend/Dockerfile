# ---------- build ----------
# SDK 10 builds the net8.0 project fine.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so the layer is cached when only source changes.
COPY SchoolAdmission.Api/SchoolAdmission.Api.csproj SchoolAdmission.Api/
RUN dotnet restore SchoolAdmission.Api/SchoolAdmission.Api.csproj

COPY SchoolAdmission.Api/ SchoolAdmission.Api/
RUN dotnet publish SchoolAdmission.Api/SchoolAdmission.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- runtime ----------
# Project targets net8.0 (CLAUDE.md: stack fixed to .NET 8), so the runtime image must be 8.0.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "SchoolAdmission.Api.dll"]
