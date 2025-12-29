FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

ARG TARGETOS
ARG TARGETARCH
ARG GITHUB_RELEASE_TAG

EXPOSE 80
EXPOSE 443

# Set environment variables
ENV GITHUB_RELEASE_TAG=${GITHUB_RELEASE_TAG}
ARG ASSET_NAME="EdNexusData.Broker.Web"

# Update required dependencies
RUN apt-get update && \
    apt-get install -y wget tar && \
    apt-get clean

# Copy files from build context into /app
WORKDIR /app
COPY . .

# Change directory to the extracted asset folder (assuming it has an executable)
WORKDIR /app

# Run the executable (replace 'executable' with the actual executable name)
ENTRYPOINT ["dotnet", "EdNexusData.Broker.Web.dll"]
