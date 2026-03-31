# Stage 1: The "Builder" (Contains the heavy .NET SDK to compile your code)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and download dependencies (NuGet packages)
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of your code and publish it
COPY . ./
RUN dotnet publish -c Release -o out

# Stage 2: The "Runner" (A tiny, secure Linux image just for running the app)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the finished, compiled files from Stage 1
COPY --from=build /app/out .

# Expose the port Nginx will talk to
EXPOSE 5000
ENV ASPNETCORE_URLS=http+:5000

# Start the application
ENTRYPOINT ["dotnet", "minibank.dll"]