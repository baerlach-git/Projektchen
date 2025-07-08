FROM mcr.microsoft.com/dotnet/aspnet:9.0-nanoserver-1809 AS base
WORKDIR /app
EXPOSE 5233

ENV ASPNETCORE_URLS=http://+:5233

FROM mcr.microsoft.com/dotnet/sdk:9.0-nanoserver-1809 AS build
ARG configuration=Release
WORKDIR /src
COPY ["GrpcGreeter.csproj", "./"]
RUN dotnet restore "GrpcGreeter.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "GrpcGreeter.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "GrpcGreeter.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GrpcGreeter.dll"]
