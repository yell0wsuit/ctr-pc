using Xunit;

// Several test classes toggle the process-wide ActivePhysicsConstants.UseMobilePhysicsModel
// flag (save/set/restore), and others assert against mode-dependent constants assuming the
// desktop default. Parallel test classes race on that global, so run the suite serially.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
