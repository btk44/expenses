# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /transaction-service
    
# Copy csproj and restore as distinct layers
# Copy .csproj and restore
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out
    
# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /transaction-service
COPY --from=build-env /transaction-service/out .

EXPOSE 80

ENTRYPOINT ["dotnet", "TransactionService.dll"]