#!/bin/bash

# Script para generar reporte de cobertura filtrando obj y bin

export PATH="$PATH:/Users/david/.dotnet/tools"

cd "$(dirname "$0")"

echo "Generando reporte de cobertura..."

reportgenerator \
  -reports:"tests/*/TestResults/*/coverage.cobertura.xml" \
  -targetdir:"./coverage-report" \
  -reporttypes:"Html" \
  -assemblyfilters:"-*SourceGenerators*;-*obj*;-*bin*;-FrasesRandomAPI.UnitTests;-FrasesRandomAPI.IntegrationTests;-FrasesRandomAPI.Benchmarks" \
  -classfilters:"-*.SourceGenerators*;-*.AssemblyInfo;-*.GlobalUsings"

echo "Reporte generado en coverage-report/index.html"
