using Nancy.Swagger;
using Nancy.Swagger.Services;

namespace OmniSharp.AutoComplete.Metadata
{
    public class AutoCompleteRequestMetadataProvider : ISwaggerModelDataProvider
    {
        public SwaggerModelData GetModelData()
        {
            return SwaggerModelData.ForType<AutoCompleteRequest>(builder =>
            {
                builder.Property(r => r.WordToComplete).Required(true);
                builder.Property(r => r.WantDocumentationForEveryCompletionResult).Description("Specifies whether to return the code documentation for each and every returned autocomplete result.");
                builder.Property(r => r.WantImportableTypes).Description("Specifies whether to return importable types. Defaults to false. Can be turned off to get a small speed boost.");
                builder.Property(r => r.WantMethodHeader).Description("Returns a 'method header' for working with parameter templating.");
                builder.Property(r => r.WantSnippet).Description("Returns a snippet that can be used by common snippet libraries to provide parameter and type parameter placeholders");
                builder.Property(r => r.WantReturnType).Description("Returns the return type ");
            });
        }
    }
}
