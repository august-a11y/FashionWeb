FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/FashionShop.API/FashionShop.API.csproj", "src/FashionShop.API/"]
COPY ["src/FashionShop.Application/FashionShop.Application.csproj", "src/FashionShop.Application/"]
COPY ["src/FashionShop.Infrastructure/FashionShop.Infrastructure.csproj", "src/FashionShop.Infrastructure/"]
COPY ["src/FashionShop.Domain/FashionShop.Domain.csproj", "src/FashionShop.Domain/"]

RUN dotnet restore "src/FashionShop.API/FashionShop.API.csproj"

COPY . .
WORKDIR "/src/src/FashionShop.API"
RUN dotnet publish "FashionShop.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FashionShop.API.dll"]