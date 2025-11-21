#!/bin/bash

compile_which="both"
output="build"

while [[ $# -gt 0 ]]; do
    case $1 in
        --compile-c|-c)
        compile_which="c"
        shift
        ;;
        --compile-cs|-s)
        compile_which="cs"
        shift
        ;;
        --output|-o)
        output=$2
        shift
        shift
        ;;
        --*|-*)
        echo "Unknown option $1"
        exit 1
        ;;
    esac
done

compile_cs()
{
    dotnet_output="$(dotnet publish .. --output . > /dev/zero)"
    if [[ $dotnet_output -ne 0 ]]; then
        echo "compiling dotnet didn't work"
        echo $dotnet_output
        exit 1
    fi
}

compile_c()
{
    rm -r obj/*

    gcc -c $project_dir/src/Graphics/test.c -o obj/render.o
    gcc -shared -o obj/render.dll obj/render.o
}

project_dir=$(pwd)

echo "Building in $output"

[ ! -d $output ] && mkdir $output
cd $output

[ ! -d obj ] && mkdir obj

if [ "$compile_which" = "both" ]; then
    echo "Building both c and c# source files"
    compile_c
    compile_cs
elif [ "$compile_which" = "c" ]; then
    echo "Building c source files"
    compile_c
elif [ "$compile_which" = "cs" ]; then
    echo "Building c# source files"
    compile_cs
fi