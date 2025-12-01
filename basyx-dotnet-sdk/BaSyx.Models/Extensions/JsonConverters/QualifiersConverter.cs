using BaSyx.Models.AdminShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BaSyx.Models.Extensions.JsonConverters
{
    public class QualifiersConverter : JsonConverter<Qualifier>
    {
        public override Qualifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Qualifier value, JsonSerializerOptions options)
        {
            writer.WriteString("kind", value.Kind.ToString());
            
            if (!string.IsNullOrEmpty(value.Type))
                writer.WriteString("type", value.Type);

            if (value.Value != null)
                writer.WriteString("value", value.Value.ToString());
            
            if (!string.IsNullOrEmpty(value.ValueType))
                writer.WriteString("valueType", $"xs:{value.ValueType}");

            if (value.SupplementalSemanticIds.Any())
                JsonSerializer.Serialize(writer, value.SupplementalSemanticIds, options);

        }
    }
}


