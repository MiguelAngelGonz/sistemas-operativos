using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class Process
{
    public int Size { get; set; }
    public int ExecutionTime { get; set; }
    public int PartitionIndex { get; set; }

    public Process(int size, int executionTime)
    {
        Size = size;
        ExecutionTime = executionTime;
        PartitionIndex = -1;
    }
}

class MemoryManagement
{
    private int[] memoryPartitions;
    private int[] bitMap;
    private int[] partitionSizes;
    private string[] partitionMessages;
    private object lockObj = new object();

    public MemoryManagement(int[] partitionSizes, string[] partitionMessages)
    {
        Array.Sort(partitionSizes);
        this.partitionSizes = partitionSizes;
        this.partitionMessages = partitionMessages;
        memoryPartitions = new int[partitionSizes.Length];
        bitMap = new int[partitionSizes.Length];
    }

    public int AllocateMemory(Process process)
    {
        lock (lockObj)
        {
            for (int i = 0; i < bitMap.Length; i++)
            {
                if (bitMap[i] == 0 && process.Size <= partitionSizes[i])
                {
                    bitMap[i] = 1;
                    memoryPartitions[i] = process.Size;
                    process.PartitionIndex = i;

                    switch (i)
                    {
                        case 0:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case 1:
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;
                        case 2:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            break;
                        case 3:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                    }

                    Console.WriteLine($"{partitionMessages[i]} Proceso de tamaño {process.Size} asignado a la partición {i} (tamaño de partición: {partitionSizes[i]}) por {process.ExecutionTime} ms.");
                    Console.ResetColor();
                    return i;
                }
            }
            return -1;
        }
    }

    public void FreeMemory(int partitionIndex)
    {
        lock (lockObj)
        {
            if (partitionIndex < 0 || partitionIndex >= bitMap.Length)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Índice de partición no válido.");
                Console.ResetColor();
                return;
            }

            if (bitMap[partitionIndex] == 1)
            {
                bitMap[partitionIndex] = 0;

                switch (partitionIndex)
                {
                    case 0:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case 1:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case 2:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case 3:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    default:
                        Console.ResetColor();
                        break;
                }

                Console.WriteLine($"Partición {partitionIndex} liberada.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Partición {partitionIndex} ya está libre.");
                Console.ResetColor();
            }
        }
    }

    public void DisplayBitMap()
    {
        lock (lockObj)
        {
            Console.WriteLine("Estado del mapa de bits:");
            for (int i = 0; i < bitMap.Length; i++)
            {
                Console.WriteLine($"Partición {i} (tamaño {partitionSizes[i]}): {(bitMap[i] == 1 ? "Ocupada" : "Libre")}");
            }
        }
    }

    public void RunProcess(Process process)
    {
        while (true)
        {
            int partitionIndex = AllocateMemory(process);
            if (partitionIndex != -1)
            {
                Thread.Sleep(process.ExecutionTime);
                FreeMemory(partitionIndex);
                break;
            }
            else
            {
                Thread.Sleep(50);
            }
        }
    }
}

class Program
{
    static void Main()
    {
        int[] partitionSizes = { 150, 200, 350, 400 };
        string[] partitionMessages = 
        {
            "Partición pequeña:", 
            "Partición mediana:", 
            "Partición grande:", 
            "Partición extra grande:" 
        };

        MemoryManagement memoryManager = new MemoryManagement(partitionSizes, partitionMessages);

        // Definir 40 procesos predefinidos
        List<Process> processList = new List<Process>
        {
            new Process(50, 2000),
            new Process(120, 1500),
            new Process(90, 3000),
            new Process(200, 2500),
            new Process(150, 1000),
            new Process(300, 3500),
            new Process(170, 1200),
            new Process(110, 1800),
            new Process(80, 2200),
            new Process(60, 1700),
            new Process(200, 4000),
            new Process(130, 2300),
            new Process(100, 1400),
            new Process(180, 2100),
            new Process(50, 3000),
            new Process(90, 2600),
            new Process(160, 1900),
            new Process(140, 2400),
            new Process(120, 1300),
            new Process(210, 2800),
            new Process(180, 1500),
            new Process(170, 3100),
            new Process(90, 1800),
            new Process(200, 3500),
            new Process(100, 1300),
            new Process(300, 3400),
            new Process(150, 1700),
            new Process(160, 2500),
            new Process(70, 2100),
            new Process(140, 2700),
            new Process(110, 1500),
            new Process(50, 1200),
            new Process(200, 2300),
            new Process(90, 3000),
            new Process(150, 2500),
            new Process(130, 2800),
            new Process(120, 3300),
            new Process(300, 3500),
            new Process(140, 2900),
            new Process(180, 3700)
        };

        // Ordenar la lista de procesos por tiempo de ejecución de forma ascendente
        var orderedProcesses = processList.OrderBy(p => p.ExecutionTime);

        // Encolar los procesos ordenados
        Queue<Process> processQueue = new Queue<Process>(orderedProcesses);

        Stopwatch stopwatch = Stopwatch.StartNew();

        List<Thread> threads = new List<Thread>();

        while (processQueue.Count > 0)
        {
            Process process = processQueue.Dequeue();
            Thread thread = new Thread(() => memoryManager.RunProcess(process));
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        stopwatch.Stop();

        memoryManager.DisplayBitMap();

        Console.WriteLine($"Tiempo total de ejecución: {stopwatch.ElapsedMilliseconds} ms");
    }
}
