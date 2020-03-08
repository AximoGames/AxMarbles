# AxMarbles

Get the sources:

```
mkdir aximo
cd aximo
git clone --recursive https://github.com/AximoGames/AxEngine.git
git clone --recursive https://github.com/AximoGames/AxMarbles.git
```

Build from command line:

```
cd AxMarbles/src
dotnet restore
dotnet msbuild /p:Configuration=Debug /property:GenerateFullPaths=true
```
