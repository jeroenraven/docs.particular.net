<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.VisualBasic.dll</Reference>
  <Namespace>Microsoft.VisualBasic</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
	var toolsDiretory = Path.GetDirectoryName(Util.CurrentQueryPath);
	var docsDirectory = Directory.GetParent(toolsDiretory).FullName;
	var nuget = Path.Combine(toolsDiretory, "nuget.exe");
	var packagesDirectory = Path.Combine(docsDirectory, "packages");
	var tutorialDirectory = Path.Combine(docsDirectory, "tutorials");
	var tutorialsDirectory = Path.Combine(docsDirectory, "tutorials");
	var nugetConfigFile = Path.Combine(docsDirectory, "nuget.config");

	var solutionFiles = Directory.EnumerateFiles(tutorialDirectory, "*.sln", SearchOption.AllDirectories).ToList();

	Directory.SetCurrentDirectory(docsDirectory);

    var version = Interaction.InputBox($"Update tutorials to what NServiceBus version?", "Enter NServiceBus Version", "");
    if (string.IsNullOrWhiteSpace(version))
    {
        Console.WriteLine("No version selected. Aborting.");
        return;
    }

    if (version.Contains("-"))
    {
        if (Interaction.MsgBox("PreRelease Package OK?", MsgBoxStyle.YesNo, "Confirm PreRelease") != MsgBoxResult.Yes)
        {
            Console.WriteLine("Will not update to pre-release version. Aborting.");
            return;
        }
        version += " -PreRelease";
    }

    Parallel.ForEach(solutionFiles,
	new ParallelOptions() { MaxDegreeOfParallelism = 20 },
	(solutionFile) =>
		{
			Debug.WriteLine(solutionFile);
			try
			{
                Execute(nuget, $"restore {solutionFile} -packagesDirectory {packagesDirectory} -configfile {nugetConfigFile}");
                Execute(nuget, $"update {solutionFile} -NonInteractive -Id NServiceBus -Version {version} -repositoryPath {packagesDirectory} -configfile {nugetConfigFile}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine(solutionFile + ": " + ex.ToString());
				if (ex.InnerException != null)
				{
					Debug.WriteLine(ex.InnerException.ToString());
				}
			}
		}
	);
}

void Execute(string file, string arguments)
{
 	var commandline = file + " " + arguments;
	Debug.WriteLine(commandline);

	using (Process process = new Process())
	{
		process.StartInfo.FileName = file;
		process.StartInfo.Arguments = arguments;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.CreateNoWindow = true;

		var output = new StringBuilder();
		var error = new StringBuilder();

		process.OutputDataReceived += (sender, e) =>
		{
			if (e.Data != null)
			{
				output.AppendLine(e.Data);
			}
		};
		process.ErrorDataReceived += (sender, e) =>
		{
			if (e.Data != null)
			{
				error.AppendLine(e.Data);
			}
		};

		process.Start();

		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		if (process.WaitForExit(30000))
		{
			Debug.WriteLine("Finished. ExitCode: " + process.ExitCode);
		}
		else
		{
			Debug.WriteLine("Timed Out: " + error);
		}
		Debug.WriteLine("Error: " + error);
		Debug.WriteLine("output: " + output);
	}
}