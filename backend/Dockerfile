# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ChatGPTcorpus.sln ./
COPY backend/ChatGPTcorpus.csproj backend/
RUN dotnet restore backend/ChatGPTcorpus.csproj

COPY backend/ backend/
WORKDIR /src/backend
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ChatGPTcorpus.dll"]
