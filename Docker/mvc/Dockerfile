FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY SeminarManager.sln SeminarManager.sln
COPY SeminarManager.Model SeminarManager.Model
COPY SeminarManager.MVC SeminarManager.MVC
COPY SeminarManager.SQL SeminarManager.SQL
COPY SeminarManager.EF SeminarManager.EF
RUN dotnet restore SeminarManager.MVC/SeminarManager.MVC.csproj

# Copy everything else and build
RUN dotnet publish -c Release -o out ./SeminarManager.MVC/SeminarManager.MVC.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SeminarManager.MVC.dll"]