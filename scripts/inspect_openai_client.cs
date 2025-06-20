using System;
using System.Reflection;
using OpenAI;

class Program
{
    static void Main()
    {
        var client = new OpenAIClient("sk-xxx");
        var type = client.GetType();
        Console.WriteLine("Public properties:");
        foreach (var prop in type.GetProperties())
        {
            Console.WriteLine($"- {prop.Name} : {prop.PropertyType}");
        }
        Console.WriteLine("\nPublic methods:");
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Console.WriteLine($"- {method.Name}");
        }
    }
}
