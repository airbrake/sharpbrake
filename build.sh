#!/usr/bin/env bash

##########################################################################
# Custom shell script to bootstrap a Cake build.
#
# Script restores helper packages for build pipeline and starts
# build.cake script.
#
##########################################################################

# define default arguments
TARGET="Default"
CONFIGURATION="Release"
VERBOSITY="verbose"
SCRIPT_ARGUMENTS=()

# parse arguments
for i in "$@"; do
    case $1 in
        -t|--target) TARGET="$2"; shift ;;
        -c|--configuration) CONFIGURATION="$2"; shift ;;
        -v|--verbosity) VERBOSITY="$2"; shift ;;
        --) shift; SCRIPT_ARGUMENTS+=("$@"); break ;;
        *) SCRIPT_ARGUMENTS+=("$1") ;;
    esac
    shift
done

SOLUTION_ROOT=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

###########################################################################
# Prepare Cake and helper tools
###########################################################################

BUILD_PATH=$SOLUTION_ROOT/build
TOOLS_PATH=$SOLUTION_ROOT/tools

TOOLS_PROJECT_JSON=$TOOLS_PATH/project.json
TOOLS_PROJECT_JSON_SRC=$BUILD_PATH/project.json

CAKE_FEED="https://api.nuget.org/v3/index.json"

echo "Preparing Cake and build tools"

if [ ! -d "$TOOLS_PATH" ]; then
    echo "Creating tools directory"
    mkdir "$TOOLS_PATH"
fi

cp "$TOOLS_PROJECT_JSON_SRC" "$TOOLS_PROJECT_JSON"

echo "Restoring build tools (into $TOOLS_PATH)"
dotnet restore "$TOOLS_PATH" --packages "$TOOLS_PATH" --verbosity Warning -f "$CAKE_FEED"
if [ $? -ne 0 ]; then
    echo "Error occurred while restoring build tools"
    exit 1
fi

CAKE_EXE=$( ls $TOOLS_PATH/Cake.CoreCLR/*/Cake.dll | sort | tail -n 1 )

# make sure that Cake has been installed
if [[ ! -f "$CAKE_EXE" ]]; then
    echo "Could not find Cake.exe at '$CAKE_EXE'"
    exit 1
fi

# prepare Sharpbrake.Build dll
SHARPBRAKE_BUILD_PATH=$BUILD_PATH/Sharpbrake.Build

dotnet restore "$SHARPBRAKE_BUILD_PATH"
if [ $? -ne 0 ]; then
    echo "Error occurred while restoring Sharpbrake.Build packages"
    exit 1
fi

dotnet build "$SHARPBRAKE_BUILD_PATH" --configuration $CONFIGURATION
if [ $? -ne 0 ]; then
    echo "Error occurred while building Sharpbrake.Build project"
    exit 1
fi

cp $(find $SHARPBRAKE_BUILD_PATH/bin/$CONFIGURATION/* -name Sharpbrake.Build.dll) $TOOLS_PATH

###########################################################################
# Run build script
###########################################################################

exec dotnet "$CAKE_EXE" build.cake -verbosity=$VERBOSITY -configuration=$CONFIGURATION -target=$TARGET "${SCRIPT_ARGUMENTS[@]}"
