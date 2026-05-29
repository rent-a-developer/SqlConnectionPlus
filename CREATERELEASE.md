# How to create a release

## Create NuGet package

- Open Visual Studio Developer Command Prompt
- Change directory to the root directory of this project
- Run the following command
~~~shell
dotnet pack -c Release -o release SqlConnectionPlus.slnx
~~~