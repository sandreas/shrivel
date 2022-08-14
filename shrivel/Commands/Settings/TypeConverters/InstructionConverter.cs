using System.ComponentModel;
using System.Globalization;

namespace shrivel.Commands.Settings.TypeConverters;

public class InstructionConverter<T> : TypeConverter where T: new()
{

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is not string stringValue)
            {
                throw new NotSupportedException($"Can't convert value to {typeof(T).FullName}.");
            }

            return (T)Activator.CreateInstance(typeof(T), stringValue)!;
        }
    
}