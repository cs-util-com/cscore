Regex to cleanup all lines of successfully killed mutants for the console report:

	\[Killed\]([^\n])+\n


Run stryker from the XUnit test folder:

	dotnet stryker --solution-path "..\CsCore.sln" -ca perTest
	
	For the ca parameter see https://github.com/stryker-mutator/stryker-net/blob/master/docs/Configuration.md#coverage-analysis