using System;
using Pixel;
using Pixel.Assets;


public class Program
{
    static void Main(string[] args)
    {
        Project proj = Project.Default;
        
        
        Runtime.Initialize(proj);
        
        while(true)
        {
            var input = Console.ReadLine();
            
            if (input != null && input.StartsWith("n") && input.Contains(' '))
            {
                var split = input.Split(' ');
                var name = split[0];
                
                var stage = Runtime.Current.GetStage();
                Console.WriteLine(stage?.FindNode(name));
            }
            if (input != null && input.StartsWith("a"))
            {
                var stage = Runtime.Current.GetStage();
                foreach(var item in stage?.nodes!)
                    Console.WriteLine(item);
            }
        }

            
    }
}

