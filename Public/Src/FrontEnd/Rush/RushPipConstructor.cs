// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.ContractsLight;
using BuildXL.FrontEnd.MsBuild;
using BuildXL.FrontEnd.Rush.ProjectGraph;
using BuildXL.FrontEnd.Sdk;
using BuildXL.FrontEnd.Utilities;
using BuildXL.FrontEnd.Utilities.GenericProjectGraphResolver;
using BuildXL.FrontEnd.Workspaces.Core;
using BuildXL.Pips;
using BuildXL.Pips.Builders;
using BuildXL.Pips.Operations;
using BuildXL.Utilities;
using BuildXL.Utilities.Collections;
using BuildXL.Utilities.Configuration;
using BuildXL.Utilities.Instrumentation.Common;
using static BuildXL.Utilities.FormattableStringEx;

namespace BuildXL.FrontEnd.Rush
{
    /// <summary>
    /// Creates a pip based on a <see cref="RushProject"/>
    /// </summary>
    internal sealed class RushPipConstructor : IProjectToPipConstructor<RushProject>
    {
        private readonly FrontEndContext m_context;

        private readonly FrontEndHost m_frontEndHost;
        private readonly ModuleDefinition m_moduleDefinition;

        private readonly IRushResolverSettings m_resolverSettings;

        private AbsolutePath Root => m_resolverSettings.Root;

        private readonly IEnumerable<KeyValuePair<string, string>> m_userDefinedEnvironment;
        private readonly IEnumerable<string> m_userDefinedPassthroughVariables;

        private PathTable PathTable => m_context.PathTable;

        private readonly ConcurrentDictionary<RushProject, ProcessOutputs> m_processOutputsPerProject = new ConcurrentDictionary<RushProject, ProcessOutputs>();

        private readonly ConcurrentBigMap<RushProject, IReadOnlySet<RushProject>> m_transitiveDependenciesPerProject = new ConcurrentBigMap<RushProject, IReadOnlySet<RushProject>>();
        
        /// <nodoc/>
        public RushPipConstructor(
            FrontEndContext context,
            FrontEndHost frontEndHost,
            ModuleDefinition moduleDefinition,
            IRushResolverSettings resolverSettings,
            IEnumerable<KeyValuePair<string, string>> userDefinedEnvironment,
            IEnumerable<string> userDefinedPassthroughVariables)
        {
            Contract.Requires(context != null);
            Contract.Requires(frontEndHost != null);
            Contract.Requires(moduleDefinition != null);
            Contract.Requires(resolverSettings != null);
            Contract.Requires(userDefinedEnvironment != null);
            Contract.Requires(userDefinedPassthroughVariables != null);

            m_context = context;
            m_frontEndHost = frontEndHost;
            m_moduleDefinition = moduleDefinition;
            m_resolverSettings = resolverSettings;
            m_userDefinedEnvironment = userDefinedEnvironment;
            m_userDefinedPassthroughVariables = userDefinedPassthroughVariables;
        }

        /// <summary>
        /// Schedules a pip corresponding to the provided project and qualifier
        /// </summary>
        /// <remarks>
        /// The project is assumed to be scheduled in the right order, where all dependencies are scheduled first.
        /// See topographical sort performed in <see cref="ProjectGraphToPipGraphConstructor{TProject}"/>.
        /// </remarks>
        public Possible<Process> TrySchedulePipForProject(RushProject project, QualifierId qualifierId)
        {
            try
            {
                // Create command line and inputs and outputs for pipBuilder.
                if (!TryExecuteArgumentsToPipBuilder(
                    project,
                    qualifierId,
                    out var failureDetail,
                    out var process))
                {
                    Tracing.Logger.Log.SchedulingPipFailure(
                        m_context.LoggingContext,
                        Location.FromFile(project.ProjectFolder.ToString(PathTable)),
                        failureDetail);
                    process = default;

                    return new RushProjectSchedulingFailure(project, failureDetail);
                }

                return process;
            }
            catch (Exception ex)
            {
                Tracing.Logger.Log.UnexpectedPipBuilderException(
                    m_context.LoggingContext,
                    Location.FromFile(project.ProjectFolder.ToString(PathTable)),
                    ex.GetLogEventMessage(),
                    ex.StackTrace);

                return new RushProjectSchedulingFailure(project, ex.ToString());
            }
        }

        private IReadOnlyDictionary<string, string> CreateEnvironment(RushProject project)
        {
            var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            //
            // Initial environment variables that may be overwritten by the outer environment.
            //

            // Observe there is no need to inform the engine this environment is being used since
            // the same environment was used during graph construction, and the engine is already tracking them
            foreach (var input in m_userDefinedEnvironment)
            {
                string envVarName = input.Key;

                // Temp directory entries are added at pip creation time.
                if (string.Equals(envVarName, "TEMP", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(envVarName, "TMP", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                env[envVarName] = input.Value;
            }

            // node_modules/.bin is expected to be part of the project path
            // TODO: is this standard? Revisit
            string nodeModulesBin = project.ProjectFolder.Combine(PathTable, RelativePath.Create(PathTable.StringTable, "node_modules/.bin")).ToString(PathTable);
            env["PATH"] = nodeModulesBin + (env.ContainsKey("PATH")? $";{env["PATH"]}" : string.Empty);
            // redirect the user profile so it points under the temp folder
            env["USERPROFILE"] = project.TempFolder.Combine(PathTable, "USERPROFILE").ToString(PathTable);
            
            return env;
        }

        private bool TryExecuteArgumentsToPipBuilder(
            RushProject project,
            QualifierId qualifierId,
            out string failureDetail,
            out Process process)
        {
            // TODO: don't do it here but in CanSchedule
            if (string.IsNullOrEmpty(project.BuildCommand))
            {
                m_processOutputsPerProject[project] = new ProcessOutputs(new Dictionary<AbsolutePath, FileArtifactWithAttributes>(), new Dictionary<AbsolutePath, StaticDirectory>()) ;
                failureDetail = string.Empty;
                process = default;

                return true;
            }

            // We create a pip construction helper for each project
            var pipConstructionHelper = GetPipConstructionHelperForProject(project, qualifierId);

            using (var processBuilder = ProcessBuilder.Create(PathTable, m_context.GetPipDataBuilder()))
            {
                // Configure the process to add an assortment of settings: arguments, response file, etc.
                ConfigureProcessBuilder(processBuilder, project);

                // Process all predicted outputs and inputs, including the predicted project dependencies
                ProcessInputs(project, processBuilder);
                ProcessOutputs(project, processBuilder);

                // Try to schedule the process pip
                if (!pipConstructionHelper.TryAddProcess(processBuilder, out ProcessOutputs outputs, out process))
                {
                    failureDetail = "Failed to schedule the pip";
                    return false;
                }

                m_processOutputsPerProject[project] = outputs;

                failureDetail = string.Empty;
                return true;
            }
        }

        /// <summary>
        /// Adds all predicted dependencies as inputs, plus all individual inputs predicted for the project
        /// </summary>
        /// <remarks>
        /// Adding all predicted dependencies is key to get the right scheduling. On the other hand, all predicted inputs
        /// are not really needed since we are running in undeclared read mode. However, they contribute to make the weak fingerprint stronger (that
        /// otherwise will likely be just a shared opaque output at the root).
        /// </remarks>
        private void ProcessInputs(
            RushProject project,
            ProcessBuilder processBuilder)
        {
            var argumentsBuilder = processBuilder.ArgumentsBuilder;
            
            IEnumerable<RushProject> references;

            // In this case all the transitive closure is automatically exposed to the project as direct references
            // TODO: make it opt-in
            var transitiveReferences = new HashSet<RushProject>();
            ComputeTransitiveDependenciesFor(project, transitiveReferences);
            references = transitiveReferences;

            foreach (RushProject projectReference in references)
            {
                bool outputsPresent = m_processOutputsPerProject.TryGetValue(projectReference, out var processOutputs);
                if (!outputsPresent)
                {
                    Contract.Assert(false, $"Pips must have been presented in dependency order: {projectReference.ProjectFolder.ToString(PathTable)} missing, dependency of {project.ProjectFolder.ToString(PathTable)}");
                }

                // Add all known output directories
                foreach (StaticDirectory output in processOutputs.GetOutputDirectories())
                {
                    processBuilder.AddInputDirectory(output.Root);
                }
                // Add all known output files
                foreach (FileArtifact output in processOutputs.GetOutputFiles())
                {
                    processBuilder.AddInputFile(output);
                }
            }
        }

        private void ProcessOutputs(RushProject project, ProcessBuilder processBuilder)
        {
            // HACK HACK. We are missing output dirs with exclusions, so we don't include node_modules here, which is a pain for scrubbing.
            // So let's add known common directories one by one
            // Ideally we'd like to say 'the root of the project without node_modules'. No projects are supposed to write there.
            processBuilder.AddOutputDirectory(DirectoryArtifact.CreateWithZeroPartialSealId(project.ProjectFolder.Combine(PathTable, "lib")), SealDirectoryKind.SharedOpaque);
            processBuilder.AddOutputDirectory(DirectoryArtifact.CreateWithZeroPartialSealId(project.ProjectFolder.Combine(PathTable, "temp")), SealDirectoryKind.SharedOpaque);
            processBuilder.AddOutputDirectory(DirectoryArtifact.CreateWithZeroPartialSealId(project.ProjectFolder.Combine(PathTable, "dist")), SealDirectoryKind.SharedOpaque);
            processBuilder.AddOutputDirectory(DirectoryArtifact.CreateWithZeroPartialSealId(project.ProjectFolder.Combine(PathTable, "release")), SealDirectoryKind.SharedOpaque);
            processBuilder.AddOutputDirectory(DirectoryArtifact.CreateWithZeroPartialSealId(project.ProjectFolder.Combine(PathTable, "src")), SealDirectoryKind.SharedOpaque);

            // HACK HACK. Only non-test projects generate these files at the root of the project. So don't add them twice otherwise graph construction complains
            if (!project.Name.EndsWith("_test"))
            {
                processBuilder.AddOutputFile(new FileArtifact(project.ProjectFolder.Combine(PathTable, "test-api.js"), 0), FileExistence.Optional);
                processBuilder.AddOutputFile(new FileArtifact(project.ProjectFolder.Combine(PathTable, "test-api.d.ts"), 0), FileExistence.Optional);
            }

            // Some projects share their temp folder. So don't declare this as a temp location. Anyway, rush makes sure the right files are deleted
            processBuilder.AddUntrackedDirectoryScope(DirectoryArtifact.CreateWithZeroPartialSealId(project.TempFolder));

            // Add all the additional output directories that the rush graph knows about
            foreach(var additionalOutput in project.AdditionalOutputDirectories)
            {
                processBuilder.AddOutputDirectory(DirectoryArtifact.CreateWithZeroPartialSealId(additionalOutput), SealDirectoryKind.SharedOpaque);
            }

            // Add additional output directories configured in the main config file
            AddAdditionalOutputDirectories(processBuilder, project.ProjectFolder);
        }

        private void ComputeTransitiveDependenciesFor(RushProject project, HashSet<RushProject> accumulatedDependencies)
        {
            // We already computed the transitive dependencies for the required project
            if (m_transitiveDependenciesPerProject.TryGetValue(project, out var transitiveDependencies))
            {
                accumulatedDependencies.AddRange(transitiveDependencies);
                return;
            }

            foreach (RushProject dependency in project.Dependencies)
            {
                accumulatedDependencies.Add(dependency);
                ComputeTransitiveDependenciesFor(dependency, accumulatedDependencies);
            }

            m_transitiveDependenciesPerProject.TryAdd(project, accumulatedDependencies.ToReadOnlySet());
        }

        private void ConfigureProcessBuilder(
            ProcessBuilder processBuilder,
            RushProject project)
        {
            SetCmdTool(processBuilder, project);

            // Working directory - the directory where the project file lives.
            processBuilder.WorkingDirectory = DirectoryArtifact.CreateWithZeroPartialSealId(project.ProjectFolder);

            // We allow undeclared inputs to be read
            processBuilder.Options |= Process.Options.AllowUndeclaredSourceReads;

            // We want to enforce the use of weak fingerprint augmentation since input predictions could be not complete/sufficient
            // to avoid a large number of path sets
            processBuilder.Options |= Process.Options.EnforceWeakFingerprintAugmentation;

            // By default the double write policy is to allow same content double writes.
            processBuilder.DoubleWritePolicy |= DoubleWritePolicy.AllowSameContentDoubleWrites;

            PipConstructionUtilities.UntrackUserConfigurableArtifacts(processBuilder, m_resolverSettings);

            var logDirectory = GetLogDirectory(project);
            var logFile = logDirectory.Combine(PathTable, "build.log");

            // Execute the build command and redirect the output to a designated log file
            processBuilder.ArgumentsBuilder.Add(PipDataAtom.FromString("/C"));
            processBuilder.ArgumentsBuilder.Add(PipDataAtom.FromString(project.BuildCommand));
            processBuilder.ArgumentsBuilder.Add(PipDataAtom.FromString(">"));
            processBuilder.ArgumentsBuilder.Add(PipDataAtom.FromAbsolutePath(logFile));
            processBuilder.AddOutputFile(logFile, FileExistence.Required);

            FrontEndUtilities.SetProcessEnvironmentVariables(CreateEnvironment(project), m_userDefinedPassthroughVariables, processBuilder, m_context.PathTable);
        }

        private void AddAdditionalOutputDirectories(ProcessBuilder processBuilder, AbsolutePath projectFolder)
        {
            if (m_resolverSettings.AdditionalOutputDirectories == null)
            {
                return;
            }

            foreach (DiscriminatingUnion<AbsolutePath, RelativePath> directoryUnion in m_resolverSettings.AdditionalOutputDirectories)
            {
                object directory = directoryUnion.GetValue();
                if (directory is AbsolutePath absolutePath)
                {
                    processBuilder.AddOutputDirectory(DirectoryArtifact.CreateWithZeroPartialSealId(absolutePath), SealDirectoryKind.SharedOpaque);
                }
                else
                {
                    // The specified relative path is interpreted relative to the project directory folder
                    AbsolutePath absoluteDirectory = projectFolder.Combine(PathTable, (RelativePath)directory);
                    processBuilder.AddOutputDirectory(DirectoryArtifact.CreateWithZeroPartialSealId(absoluteDirectory), SealDirectoryKind.SharedOpaque);
                }
            }
        }

        private AbsolutePath GetLogDirectory(RushProject projectFile)
        {
            var success = Root.TryGetRelative(PathTable, projectFile.ProjectFolder, out var inFolderPathFromEnlistmentRoot);
            Contract.Assert(success, $"Configuration root '{Root.ToString(PathTable)}' should be a parent of '{projectFile.ProjectFolder.ToString(PathTable)}'");

            // We hardcode the log to go under the output directory Logs/Rush (and follow the project structure underneath)
            // The 'official' log directory (defined by Configuration.Logging) is not stable in CloudBuild across machines, and therefore it would
            // introduce cache misses
            var result = m_frontEndHost.Configuration.Layout.OutputDirectory
                .Combine(PathTable, "Logs")
                .Combine(PathTable, "Rush")
                .Combine(PathTable, inFolderPathFromEnlistmentRoot)
                .Combine(PathTable, PipConstructionUtilities.SanitizeStringForSymbol(projectFile.Name));

            return result;
        }

        private void SetCmdTool(
            ProcessBuilder processBuilder,
            RushProject project)
        {
            var cmdExeArtifact = FileArtifact.CreateSourceFile(AbsolutePath.Create(PathTable, Environment.GetEnvironmentVariable("COMSPEC")));

            processBuilder.Executable = cmdExeArtifact;
            processBuilder.AddInputFile(cmdExeArtifact);
            processBuilder.AddCurrentHostOSDirectories();
            processBuilder.AddUntrackedAppDataDirectories();
            processBuilder.AddUntrackedProgramDataDirectories();

            // Temp directory setup including setting TMP and TEMP env vars. The path to
            // the temp dir is generated in a consistent fashion between BuildXL runs to
            // ensure environment value (and hence pip hash) consistency.
            processBuilder.EnableTempDirectory();

            processBuilder.ToolDescription = StringId.Create(m_context.StringTable, I($"{m_moduleDefinition.Descriptor.Name} - {project.Name}"));
        }

        private PipConstructionHelper GetPipConstructionHelperForProject(RushProject project, QualifierId qualifierId)
        {
            var pathToProject = project.ProjectFolder;

            // We might be adding the same spec file pip more than once when the same project is evaluated
            // under different global properties, but that's fine, the pip graph ignores duplicates
            m_frontEndHost.PipGraph?.AddSpecFile(
                new SpecFilePip(
                    FileArtifact.CreateSourceFile(pathToProject),
                    new LocationData(pathToProject, 0, 0),
                    m_moduleDefinition.Descriptor.Id));

            Root.TryGetRelative(PathTable, pathToProject, out var specRelativePath);
            if (!PathAtom.TryCreate(m_context.StringTable, m_moduleDefinition.Descriptor.Name, out _))
            {
                throw new ArgumentException($"Failed to create PathAtom from {m_moduleDefinition.Descriptor.Name}");
            }

            // Get a symbol that is unique for this particular project instance
            var fullSymbol = GetFullSymbolFromProject(project);

            var pipConstructionHelper = PipConstructionHelper.Create(
                m_context,
                m_frontEndHost.Engine.Layout.ObjectDirectory,
                m_frontEndHost.Engine.Layout.RedirectedDirectory,
                m_frontEndHost.Engine.Layout.TempDirectory,
                m_frontEndHost.PipGraph,
                m_moduleDefinition.Descriptor.Id,
                m_moduleDefinition.Descriptor.Name,
                specRelativePath,
                fullSymbol,
                new LocationData(pathToProject, 0, 0),
                qualifierId);

            return pipConstructionHelper;
        }

        private FullSymbol GetFullSymbolFromProject(RushProject project)
        {
            // We construct the name of the value using the project name
            var valueName = PipConstructionUtilities.SanitizeStringForSymbol(project.Name);

            var fullSymbol = FullSymbol.Create(m_context.SymbolTable, valueName);
            return fullSymbol;
        }

        /// <inheritdoc/>
        public void NotifyProjectNotScheduled(RushProject project)
        {
            // TODO: add logging
        }
    }
}
