namespace Aspirate.Cli.Actions.Manifests;

public sealed class GenerateKustomizeManifestAction(
    IAspireManifestCompositionService manifestCompositionService,
    IServiceProvider serviceProvider) : BaseAction(serviceProvider)
{
    public const string ActionKey = "GenerateKustomizeManifestAction";

    private static bool IsDatabase(Resource resource) =>
        resource is PostgresDatabase;

    public override async Task<bool> ExecuteAsync()
    {
         if (NoSupportedComponentsExitAction())
         {
             return true;
         }

         Logger.MarkupLine("\r\n[bold]Generating kustomize manifests to run against your kubernetes cluster:[/]\r\n");

         foreach (var resource in CurrentState.ComputedParameters.AllSelectedSupportedComponents)
         {
             await ProcessIndividualResourceManifests(resource);
         }

         var finalHandler = Services.GetRequiredKeyedService<IProcessor>(AspireLiterals.Final) as FinalProcessor;
         finalHandler.CreateFinalManifest(CurrentState.ComputedParameters.FinalResources, CurrentState.ComputedParameters.KustomizeManifestPath, CurrentState.InputParameters.LoadedAspirateSettings);

         return true;
    }

    private bool NoSupportedComponentsExitAction()
    {
        if (CurrentState.ComputedParameters.AllSelectedSupportedComponents.Count != 0)
        {
            return false;
        }

        Logger.MarkupLine("\r\n[bold]No supported components selected. Skipping generation of kustomize manifests.[/]");
        return true;

    }

    private async Task ProcessIndividualResourceManifests(KeyValuePair<string, Resource> resource)
    {
        if (resource.Value.Type is null)
        {
            Logger.MarkupLine($"[yellow]Skipping resource '{resource.Key}' as its type is unknown.[/]");
            return;
        }

        var handler = Services.GetKeyedService<IProcessor>(resource.Value.Type);

        if (handler is null)
        {
            Logger.MarkupLine($"[yellow]Skipping resource '{resource.Key}' as its type is unsupported.[/]");
            return;
        }

        var success = await handler.CreateManifests(resource, CurrentState.ComputedParameters.KustomizeManifestPath, CurrentState.InputParameters.LoadedAspirateSettings);

        if (success && !IsDatabase(resource.Value))
        {
            CurrentState.ComputedParameters.AppendToFinalResources(resource.Key, resource.Value);
        }
    }
}