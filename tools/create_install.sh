#!/bin/bash

printf "Release name: "
read install_name

root_path="$(pwd)"
install_where="$(realpath releases)/$install_name"
tools_path="$(realpath $(dirname $0))"
project_path="$(dirname $tools_path)"
echo $project_path
echo $tools_path
[ ! -d .temp ] && mkdir .temp
[ -d .temp/$install_name ] && exit 1 || mkdir .temp/$install_name

CreateInstall() {
  cd .temp/$install_name

  cp $tools_path/install.sh install.sh
  cp -a $tools_path/install-assets .

  cp -a $project_path/shaders .
  mkdir ./resources
  cp -a $project_path/resources/textures resources

  tar -czf $install_where.gzip .
}

if ! CreateInstall; then
  echo "Couldn't create installation"
fi

cd $root_path
echo "$(pwd)"
rm -r .temp/$install_name
