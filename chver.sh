#!/bin/bash

PREV="5.0.7"

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