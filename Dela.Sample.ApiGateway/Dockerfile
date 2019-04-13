# 父镜像
FROM docker.io/microsoft/dotnet:runtime
# 设置工作目录
WORKDIR /app
# 复制发布文件到/app下
COPY . /app
# 设置端口
EXPOSE 8087
# 使用dotnet Lails.Server.Demo.dll来运行ASP.NET Core项目，注意大小写
ENTRYPOINT ["dotnet", "Dela.Sample.ApiGateway.dll"]