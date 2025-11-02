// See https://aka.ms/new-console-template for more information

using UnitedTestV1;



var simulator = new Simulator();

simulator.Run();

while (true)
{
    Console.WriteLine(ShowMenu());
    var input = Console.ReadLine();
    switch (input)
    {
        case "1":
            simulator.RunAllCupps();
            break;
        case "0":
            Console.WriteLine("Exiting the application...");
            return;
        default:
            Console.WriteLine("Invalid option. Please try again.");
            break;
    }
}



string ShowMenu()
{
    return @"Select an option:
            1. run all the cupps
            0. Exit";
}