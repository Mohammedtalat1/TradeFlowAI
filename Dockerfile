# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.slnx ./
COPY src/TradeFlowAI.Domain/*.csproj ./src/TradeFlowAI.Domain/
COPY src/TradeFlowAI.Application/*.csproj ./src/TradeFlowAI.Application/
COPY src/TradeFlowAI.Infrastructure/*.csproj ./src/TradeFlowAI.Infrastructure/
COPY src/TradeFlowAI.API/*.csproj ./src/TradeFlowAI.API/
COPY tests/TradeFlowAI.UnitTests/*.csproj ./tests/TradeFlowAI.UnitTests/

# Restore dependencies
RUN dotnet restore src/TradeFlowAI.API/TradeFlowAI.API.csproj

# Copy full source and publish
COPY . .
RUN dotnet publish src/TradeFlowAI.API/TradeFlowAI.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TradeFlowAI.API.dll"]
