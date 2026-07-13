FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY HuanyuFlowerShop/HuanyuFlowerShop.csproj HuanyuFlowerShop/
RUN dotnet restore HuanyuFlowerShop/HuanyuFlowerShop.csproj
COPY HuanyuFlowerShop/ HuanyuFlowerShop/
RUN dotnet publish HuanyuFlowerShop/HuanyuFlowerShop.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5002
EXPOSE 5002
COPY --from=build /app/publish .
RUN mkdir -p /app/uploads
RUN chown -R $APP_UID /app/uploads
USER $APP_UID
ENTRYPOINT ["dotnet", "HuanyuFlowerShop.dll"]
