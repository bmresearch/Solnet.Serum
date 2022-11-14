#!/bin/bash

PREV="6.0.14"

FILES="README.md
SharedBuildProperties.props
Solnet.Serum/Solnet.Serum.csproj
Solnet.Serum.Examples/Solnet.Serum.Examples.csproj
chver.sh"

for f in $FILES
do
    echo $f
    sed -i "s/$PREV/$1/g" $f
done