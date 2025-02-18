#!/bin/bash

# Ensure required environment variables are set
if [ -z "$GITHUB_TOKEN" ]; then
  echo "GITHUB_TOKEN is not set. Please set it as an environment variable."
  exit 1
fi

if [ -z "$RELEASE_TAG" ]; then
  echo "RELEASE_TAG is not set. Please set it as an environment variable."
  exit 1
fi

# Get the release ID based on the tag name
RELEASE_ID=$(curl -s \
  -H "Authorization: token $GITHUB_TOKEN" \
  "https://api.github.com/repos/ednexusdata/broker-app/releases/tags/$RELEASE_TAG" | \
  jq -r .id)

if [ "$RELEASE_ID" == "null" ]; then
  echo "Release not found for tag $RELEASE_TAG"
  exit 1
fi

# Clone the first repository
git clone https://github.com/ednexusdata/broker-app.git /home/runner/work/build

# Clone the second repository
git clone https://github.com/ednexusdata/broker-common.git /home/runner/work/build

# Navigate to the web application
cd "/home/runner/work/build/broker-app/src/EdNexusData.Broker.Web"

# Restore dependencies
dotnet restore

# Build the ASP.NET Core application
dotnet build --configuration Release

# Navigate to the web application release
cd "/home/runner/work/build/broker-app/src/EdNexusData.Broker.Web/bin/Release/net8.0"

# Compress Release
zip -r "/home/runner/work/build/EdNexusData.Broker.Web.zip"

# Navigate to the web application
cd "/home/runner/work/build/broker-app/src/EdNexusData.Broker.Worker"

# Restore dependencies
dotnet restore

# Build the ASP.NET Core application
dotnet build --configuration Release

# Navigate to the worker release
cd "/home/runner/work/build/broker-app/src/EdNexusData.Broker.Worker/bin/Release/net8.0"

# Compress Release
zip -r "/home/runner/work/build/EdNexusData.Broker.Worker.zip"

# Change directory 
cd "/home/runner/work/build"

# Loop through files and upload each one
FILES_TO_UPLOAD=("EdNexusData.Broker.Web.zip" "EdNexusData.Broker.Worker.zip") # add your files here
for FILE in "${FILES_TO_UPLOAD[@]}"; do
  echo "Uploading $FILE..."

  curl -s \
    -H "Authorization: token $GITHUB_TOKEN" \
    -H "Content-Type: $(file -b --mime-type $FILE)" \
    --data-binary @"$FILE" \
    "https://uploads.github.com/repos/ednexusdata/broker-app/releases/$RELEASE_ID/assets?name=$(basename $FILE)"

  echo "$FILE uploaded successfully!"
done

echo "Build process completed."