# How to create a release

## Create NuGet package

- Open Visual Studio Developer Command Prompt
- Change directory to the root directory of this project
- Run the following command
~~~shell
dotnet pack -c Release src\SqlConnectionPlus\SqlConnectionPlus.csproj
~~~

## Sign NuGet package

- Open Visual Studio Developer Command Prompt
- Change directory to the root directory of this project
- Run the following command
~~~shell
dotnet nuget sign src\SqlConnectionPlus\bin\Release\RentADeveloper.SqlConnectionPlus.1.x.x.nupkg ^
--certificate-path Path\To\CodeSigningCertificate.pfx ^
--timestamper http://timestamp.comodoca.com/rfc3161 ^
--certificate-password CertificatePassword
~~~

- Enter the encryption password for the code signing certificate.

## Sign NuGet symbols package

- Open Visual Studio Developer Command Prompt
- Change directory to the root directory of this project
- Run the following command
~~~shell
dotnet nuget sign src\SqlConnectionPlus\bin\Release\RentADeveloper.SqlConnectionPlus.1.x.x.snupkg ^
--certificate-path Path\To\CodeSigningCertificate.pfx ^
--timestamper http://timestamp.comodoca.com/rfc3161 ^
--certificate-password CertificatePassword
~~~
- Enter the encryption password for the code signing certificate.