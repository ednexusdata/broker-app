FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

EXPOSE 80
EXPOSE 443

# Set environment variables
ENV GITHUB_RELEASE_TAG="v1.0.0"
ARG ASSET_NAME="EdNexusData.Broker.Web-${GITHUB_RELEASE_TAG}-${BUILDOS}-${BUILDARCH}"

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
ENTRYPOINT ["dotnet", "EdNexusData.Broker.Web.dll"]
