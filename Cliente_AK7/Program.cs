using System;
using System.Diagnostics;
using System.Timers;
using System.IO;
using Cliente_AK7.Models;
using System.Net.Http.Headers;

internal class Program
{
    //Variables Globales 
    static HttpClient client = new HttpClient();

    static System.Timers.Timer timer;
    public static UmbralCompServer registroCPU = new UmbralCompServer();
    public static UmbralCompServer registroMemoria = new UmbralCompServer();
    public static UmbralCompServer registroDisco = new UmbralCompServer();
    public static Servidor Server = new Servidor();

    public Program()
    {
       

        

        registroMemoria.CodServer = "S_W_1";
        registroMemoria.CodComp = "CO3";
        registroMemoria.CodUmbral = "normal";

        registroDisco.CodServer = "S_W_1";
        registroDisco.CodComp = "CO1";
        registroDisco.CodUmbral = "normal";
       

    }

    private static void Main(string[] args)
    {

        RunAsync().GetAwaiter().GetResult();



        Console.WriteLine("oprima una letra para salir \n ");
        Console.ReadKey();
    }//fn main

    static async Task RunAsync()
    {
        client.BaseAddress = new Uri("http://localhost:5021/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        UmbralCompServer c = new UmbralCompServer()
        {
            CodServer = "S_W_1",
            CodComp = "CO2",
            CodUmbral = "normal",
            Porcentaje = usoCPUDevuelve()
        };
        c = await crearRegistro(c);
        Console.WriteLine($"Registrado");
        Console.ReadLine();
    }//fn

    static async Task<UmbralCompServer> crearRegistro(UmbralCompServer c)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("paramSensibilidad", c);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<UmbralCompServer>();
    }

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
    private static int usoCPUDevuelve()
    {
        try
        {
            float processCpuUsage = Process.GetCurrentProcess().TotalProcessorTime.Ticks / (float)Stopwatch.Frequency;

            float usoCPU = 0;

            foreach (var cpu in new PerformanceCounterCategory("Processor").GetCounters("_Total"))
            {
                usoCPU += cpu.NextValue();
            }

            return (int)usoCPU / 10;
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