namespace Aspirate.Cli.Processors.Components.Postgresql;

/// <summary>
/// Handles producing the Postgres component as Kustomize manifest.
/// </summary>
public class PostgresDatabaseProcessor(IFileSystem fileSystem) : BaseProcessor<PostgresDatabaseTemplateData>(fileSystem)
{

    /// <inheritdoc />
    public override string ResourceType => AspireLiterals.PostgresDatabase;

    /// <inheritdoc />
    public override Resource? Deserialize(ref Utf8JsonReader reader) =>
        JsonSerializer.Deserialize<PostgresDatabase>(ref reader);

    public override Task<bool> CreateManifests(KeyValuePair<string, Resource> resource, string outputPath) =>
        // Do nothing for databases, they are there for configuration.
        Task.FromResult(true);
}