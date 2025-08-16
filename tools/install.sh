#!/bin/bash

version="0.3.2"

installer_path="$(dirname $(realpath $0))"
shader_path="$(realpath $root_path/shaders)"

echo "Where to install"
read install_path

if [ -d "$install_path" ]; then
  echo "Directory already exists"
  exit 1
fi

echo "Installing into $install_path"

mkdir $install_path
cd $install_path
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
cp -a $installer_path/install-assets/. $install_path
cp -a $shader_path $install_path

cp -a $installer_path/resources/textures/null.png resources/textures/null.png 
cp -a $installer_path/resources/textures/error_texture.png resources/textures/error_texture.png 