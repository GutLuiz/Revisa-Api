FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["UepaMed/UepaMed.csproj", "UepaMed/"]
COPY ["UepaMed.Application/UepaMed.Application.csproj", "UepaMed.Application/"]
COPY ["UepaMed.Domain/UepaMed.Domain.csproj", "UepaMed.Domain/"]
COPY ["UepaMed.Infrastructure/UepaMed.Infrastructure.csproj", "UepaMed.Infrastructure/"]

RUN dotnet restore "UepaMed/UepaMed.csproj"

COPY . .

RUN dotnet publish "UepaMed/UepaMed.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "UepaMed.dll"]