.PHONY: build run-web test clean

build:
	dotnet build LeaveManagement.slnx -c Release

run-web:
	dotnet run --project src/LeaveManagement.Web/LeaveManagement.Web.csproj --configuration Release

test:
	dotnet test LeaveManagement.slnx --no-build --configuration Release

clean:
	dotnet clean LeaveManagement.slnx
