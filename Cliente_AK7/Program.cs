using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        obtenerParametros();

    }//fn main


    public static void obtenerParametros()
    {
        try
        {
            if (/*si es wind*/)
            {
                PerformanceCounter usoCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                PerformanceCounter memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
                PerformanceCounter diskCounter = new PerformanceCounter("LogicalDisk", "% Free Space", "_Total");

                Console.WriteLine("CPU " + usoCPU.NextValue() + "%\n");
                Console.WriteLine("Memoria " + memoryCounter.NextValue() + "%\n");
                Console.WriteLine("Disco " + diskCounter.NextValue() + "%\n");
                //}


            }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
       
    }//fn


}//fin class