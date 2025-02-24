FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

ARG TARGETOS
ARG TARGETARCH
ARG GITHUB_RELEASE_TAG

# Set environment variables
ENV GITHUB_RELEASE_TAG=${GITHUB_RELEASE_TAG}
ARG ASSET_NAME="EdNexusData.Broker.Worker-${GITHUB_RELEASE_TAG}-${TARGETOS}-${TARGETARCH}"

# Update required dependencies
RUN apt-get update && \
    apt-get install -y wget tar && \
    apt-get clean

# Download the asset from GitHub releases
RUN wget https://github.com/ednexusdata/broker-app/releases/download/${GITHUB_RELEASE_TAG}/${ASSET_NAME}.tar.gz -O /tmp/${ASSET_NAME}

# Make directory
RUN mkdir /app

# Extract the asset
RUN tar -xzf /tmp/${ASSET_NAME} -C /app/

# Change directory to the extracted asset folder (assuming it has an executable)
WORKDIR /app/${ASSET_NAME%}

# Run the executable (replace 'executable' with the actual executable name)
ENTRYPOINT ["dotnet", "EdNexusData.Broker.Worker.dll"]