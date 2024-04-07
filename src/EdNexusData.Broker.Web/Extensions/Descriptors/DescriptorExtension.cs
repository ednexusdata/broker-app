namespace src.Extensions.Descriptors;
public static class DescriptorExtension
{
    public static string ExtractDescriptor(this string? descriptor)
    {
        if (string.IsNullOrEmpty(descriptor))
            return string.Empty;

        int index = descriptor.LastIndexOf("#");
        if (index >= 0 && index < descriptor.Length - 1)
        {
            return descriptor[(index + 1)..];
        }
        return descriptor;  
    }
}
