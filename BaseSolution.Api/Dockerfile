FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["BaseSolution.Api/BaseSolution.Api.csproj", "BaseSolution.Api/"]
RUN dotnet restore "BaseSolution.Api/BaseSolution.Api.csproj"
COPY . .
WORKDIR "/src/BaseSolution.Api"
RUN dotnet build "BaseSolution.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BaseSolution.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BaseSolution.Api.dll"]