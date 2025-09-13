#!/bin/bash

version="0.3.3"

installer_path="$(pwd)"

echo "Where to install"
read install_path

if [ -d "$install_path" ]; then
  echo "Directory already exists"
  exit 1
fi

echo "Installing into $install_path"

mkdir "$install_path"
cd "$install_path"
mkdir resources
mkdir resources/textures

echo "Creating new dotnet project console"
if ! dotnet new console; then
  echo "Couldn't create project with dotnet"
  exit 1
fi

echo "Installing Mukia-Engine"
if ! dotnet add package mukia-engine --version $version; then
  echo "Couldn't add mukia-engine"
  exit 1
fi

rm Program.cs

echo "Copying content"
cp -a $installer_path/tools/install-assets/* .
cp -a $installer_path/shaders .

mkdir assets
cp -a $installer_path/resources/textures resources
