using Xunit;

// Several test classes toggle the process-wide ActivePhysicsConstants model flags
// (UseMobilePhysicsModel, UseTimeTravelRocketModel) with save/set/restore, and others assert
// against mode-dependent constants assuming the desktop default. Building a scene also assigns
// both flags from the level's own metadata. Parallel test classes race on those globals, so run
// the suite serially.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
