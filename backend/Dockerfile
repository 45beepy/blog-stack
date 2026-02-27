# Step 1: Build the application (Changed to 10.0)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /App

# Copy everything and build
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Step 2: Build the runtime image (Changed to 10.0)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /App
COPY --from=build-env /App/out .

# Expose the port Render uses
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BlogApi.dll"]
