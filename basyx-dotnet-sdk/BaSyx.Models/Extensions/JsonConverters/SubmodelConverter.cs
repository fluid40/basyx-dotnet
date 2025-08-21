using BaSyx.Models.AdminShell;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BaSyx.Models.Extensions.JsonConverters
{
    public class SubmodelConverter : JsonConverter<ISubmodel>
    {
        internal ConverterOptions _converterOptions;

        public SubmodelConverter(ConverterOptions options = null)
        {
            _converterOptions = options ?? new PathConverterOptions();
        }

        public override ISubmodel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ISubmodel value, JsonSerializerOptions options)
        {
            writeMetaData(writer, value, options);
        }

        private void writeMetaData(Utf8JsonWriter writer, ISubmodel value, JsonSerializerOptions options)
        {
            //Submodel trimmedSubmodel = new Submodel(submodel.IdShort, submodel.Id);
            //1 trimmedSubmodel.Administration = submodel.Administration; 1
            //1 trimmedSubmodel.Category = submodel.Category; 1
            //1 trimmedSubmodel.Kind = submodel.Kind;
            //1 trimmedSubmodel.SemanticId = submodel.SemanticId;
            //1 trimmedSubmodel.SupplementalSemanticIds = submodel.SupplementalSemanticIds;
            //1 trimmedSubmodel.Qualifiers = submodel.Qualifiers;
            //1 trimmedSubmodel.Description = submodel.Description;
            //1 trimmedSubmodel.DisplayName = submodel.DisplayName;
            //trimmedSubmodel.EmbeddedDataSpecifications = submodel.EmbeddedDataSpecifications;
            //trimmedSubmodel.ConceptDescription = submodel.ConceptDescription;
            //trimmedSubmodel.SubmodelElements = null;


            writer.WriteString("idShort", value.IdShort);
            writer.WriteString("kind", value.Kind.ToString());
            writer.WriteString("modelType", value.ModelType.ToString());

            if (value.Administration != null)
                JsonSerializer.Serialize(writer, value.Administration, options);

            if (!string.IsNullOrEmpty(value.Category))
                writer.WriteString("category", value.Category);

            if (value.Description?.Count > 0)
            {
                writer.WritePropertyName("description");
                JsonSerializer.Serialize(writer, value.Description, options);
            }

            if (value.DisplayName?.Count > 0)
            {
                writer.WritePropertyName("displayName");
                JsonSerializer.Serialize(writer, value.DisplayName, options);
            }

            if (value.SemanticId != null)
            {
                writer.WritePropertyName("semanticId");
                JsonSerializer.Serialize(writer, value.SemanticId, options);
            }

            if (value.SupplementalSemanticIds?.Count() > 0)
            {
                writer.WritePropertyName("supplementalSemanticIds");
                JsonSerializer.Serialize(writer, value.SupplementalSemanticIds, options);
            }

            if (value.Qualifiers?.Count() > 0)
            {
                writer.WritePropertyName("qualifiers");
                JsonSerializer.Serialize(writer, value.Qualifiers, options);
            }

            if (value.EmbeddedDataSpecifications?.Count() > 0)
            {
                writer.WritePropertyName("qualifiers");
                JsonSerializer.Serialize(writer, value.EmbeddedDataSpecifications, options);
            }
        }
    }
}
