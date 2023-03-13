using System;
using System.Diagnostics;
using System.Timers;
using System.IO;
using Cliente_AK7.Models;

internal class Program
{
    //Variables Globales 
    static System.Timers.Timer timer;
    public static UmbralCompServer registroCPU = new UmbralCompServer();
    public static UmbralCompServer registroMemoria = new UmbralCompServer();
    public static UmbralCompServer registroDisco = new UmbralCompServer();
    public static Servidor Server = new Servidor();

    public Program()
    {
        //Server.CodServidor = "S_W_1";
        //Server.NombServidor = "PC_Michael";
        //Server.DescServidor = "PC Michael Wimdows";
        //Server.UserAdmiServidor = "admin";
        //Server.PassServidor = "cABhAHMAcwA=";

        registroCPU.CodServer= "S_W_1";
        registroMemoria.CodServer = "S_W_1";
        registroDisco.CodServer = "S_W_1";

        registroDisco.CodComp = "CO1";
        registroCPU.CodComp = "CO2";
        registroMemoria.CodComp = "CO3";

        registroDisco.CodUmbral = "normal";
        registroCPU.CodComp = "normal";
        registroMemoria.CodComp = "normal";

    }

    private static void Main(string[] args)
    {
        monitoreoCPU(5000);
        monitoreoMemoria(5000);
        monitoreoDisco(5000);

       



        Console.WriteLine("oprima una letra para salir \n ");
        Console.ReadKey();
    }//fn main

    
    #region CPU
    private static void usoCPU(object sender, ElapsedEventArgs e)
    {
        try
        {
            float processCpuUsage = Process.GetCurrentProcess().TotalProcessorTime.Ticks / (float)Stopwatch.Frequency;

            float usoCPU = 0;

            foreach (var cpu in new PerformanceCounterCategory("Processor").GetCounters("_Total"))
            {
                usoCPU += cpu.NextValue();
            }

            registroCPU.Porcentaje = (int)usoCPU / 10;

            Console.WriteLine($"Porcentaje CPU: {registroCPU.Porcentaje}%");
        }
        catch (Exception ex)
        {

            throw new Exception("Fallo obtener Param " + ex.Message);
        }

    }//fin
    static void monitoreoCPU(int intervalo_Ms)
    {
        timer = new System.Timers.Timer(intervalo_Ms);
        timer.Elapsed += usoCPU;
        timer.AutoReset = true;
        timer.Start();
     }//fn
    #endregion

    #region Memoria
    private static void usoMemoria(object sender, ElapsedEventArgs e)
    {
        try
        {
            Process procesoActual = Process.GetCurrentProcess();
            long memoriaActual = procesoActual.PrivateMemorySize64;
            memoriaActual = memoriaActual / 1024 / 1024;

            registroMemoria.Porcentaje = (int) (100 -memoriaActual);

            Console.WriteLine($"Memoria usada: {registroMemoria.Porcentaje} %");
        }
        catch (Exception ex)
        {

            throw new Exception("Fallo obtener Param " + ex.Message);
        }

    }//fin
    static void monitoreoMemoria(int intervalo_Ms)
    {
        timer = new System.Timers.Timer(intervalo_Ms);
        timer.Elapsed += usoMemoria;
        timer.AutoReset = true;
        timer.Start();
    }//fn
    #endregion

    #region Disco
    private static void usoDisco(object sender, ElapsedEventArgs e)
    {
        try
        {
            var discoDuro = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));
            long espacioDiscoUsado = discoDuro.TotalSize - discoDuro.TotalFreeSpace;
            //espacioDiscoUsado = espacioDiscoUsado / 1024 / 1024 / 1024;

            registroDisco.Porcentaje= (int)( discoDuro.TotalSize / discoDuro.TotalFreeSpace ) * 10;

            Console.WriteLine($"Espacio Disco Usado: {registroDisco.Porcentaje} %");

        }
        catch (Exception ex)
        {

            throw new Exception("Fallo obtener Param " + ex.Message);
        }

    }//fin
    static void monitoreoDisco(int intervalo_Ms)
    {
        timer = new System.Timers.Timer(intervalo_Ms);
        timer.Elapsed += usoDisco;
        timer.AutoReset = true;
        timer.Start();
    }//fn
    #endregion

}//fin class