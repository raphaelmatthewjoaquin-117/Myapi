FROM mcr.microsoft.com/devcontainers/dotnet:1-8.0-bookworm

RUN find /etc/apt/sources.list.d -type f -exec grep -l "dl.yarnpkg.com" {} + | xargs -r rm -f