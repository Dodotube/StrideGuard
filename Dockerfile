FROM mcr.microsoft.com/dotnet/sdk:11.0-preview AS build
WORKDIR /src

COPY ["src/StrideGuard.Domain/StrideGuard.Domain.csproj", "src/StrideGuard.Domain/"]
COPY ["src/StrideGuard.Application/StrideGuard.Application.csproj", "src/StrideGuard.Application/"]
COPY ["src/StrideGuard.Infrastructure/StrideGuard.Infrastructure.csproj", "src/StrideGuard.Infrastructure/"]
COPY ["src/StrideGuard.API/StrideGuard.API.csproj", "src/StrideGuard.API/"]

RUN dotnet restore "src/StrideGuard.API/StrideGuard.API.csproj"
COPY src/ src/
RUN dotnet publish "src/StrideGuard.API/StrideGuard.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:11.0-preview AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StrideGuard.API.dll"]
